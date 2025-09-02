/**
 * App Calendar
 */

'use strict';

document.addEventListener('DOMContentLoaded', function () {
  (function () {
    // Detect FullCalendar environment (theme bundle vs CDN global)
    function detectCalendarEnv() {
      const hasThemeBundle = typeof window !== 'undefined' && typeof window.Calendar !== 'undefined' && typeof window.calendarPlugins !== 'undefined';
      const hasFullCalendarNamespace = typeof window !== 'undefined' && typeof window.FullCalendar !== 'undefined' && typeof window.FullCalendar.Calendar !== 'undefined';
      return { hasThemeBundle, hasFullCalendarNamespace };
    }

    const calendarEl = document.getElementById('calendar'),
      appCalendarSidebar = document.querySelector('.app-calendar-sidebar'),
      addEventSidebar = document.getElementById('addEventSidebar'),
      appOverlay = document.querySelector('.app-overlay'),
      calendarsColor = {
        Business: 'primary',
        Holiday: 'success',
        Personal: 'danger',
        Family: 'warning',
        ETC: 'info',
        'اعلان مدیریتی': 'danger'
      },
      offcanvasTitle = document.querySelector('.offcanvas-title'),
      btnToggleSidebar = document.querySelector('.btn-toggle-sidebar'),
      btnAddEvent = document.querySelector('.btn-add-event'),
      btnUpdateEvent = document.querySelector('.btn-update-event'),
      btnDeleteEvent = document.querySelector('.btn-delete-event'),
      btnCancel = document.querySelector('.btn-cancel'),
      eventTitle = document.querySelector('#eventTitle'),
      eventStartDate = document.querySelector('#eventStartDate'),
      eventEndDate = document.querySelector('#eventEndDate'),
      eventUrl = document.querySelector('#eventURL'),
      eventLabel = $('#eventLabel'),
      eventGuests = $('#eventGuests'),
      eventLocation = document.querySelector('#eventLocation'),
      eventDescription = document.querySelector('#eventDescription'),
      eventStatusInputs = document.querySelectorAll('input[name="eventStatus"]'),
      allDaySwitch = document.querySelector('.allDay-switch'),
      selectAll = document.querySelector('.select-all'),
      filterInput = [].slice.call(document.querySelectorAll('.input-filter')),
      inlineCalendar = document.querySelector('.inline-calendar');

    let eventToUpdate,
      currentEvents = [],
      inlineCalInstance,
      calendar,
      filtersLoaded = false,
      categoryNameToColorHex = {};
    // Always start with empty events; will be filled from database
    // Format a Date as local ISO without timezone (avoid UTC shift)
    function toLocalIsoString(date) {
      if (!date || !(date instanceof Date)) return null;
      const pad = n => (n < 10 ? '0' + n : '' + n);
      const y = date.getFullYear();
      const m = pad(date.getMonth() + 1);
      const d = pad(date.getDate());
      const hh = pad(date.getHours());
      const mm = pad(date.getMinutes());
      const ss = pad(date.getSeconds());
      return `${y}-${m}-${d}T${hh}:${mm}:${ss}`;
    }

    // Parse API date that may be Gregorian ISO or Jalali ISO-like (e.g. 1404-05-23T08:30:00Z)
    function parseApiDate(value) {
      try {
        if (!value) return null;
        if (value instanceof Date) return value;
        if (typeof value === 'string') {
          // Extract parts
          const m = value.match(/^(\d{4})-(\d{2})-(\d{2})[T\s](\d{2}):(\d{2})(?::(\d{2}))?/);
          if (m) {
            const y = parseInt(m[1], 10);
            const mo = parseInt(m[2], 10);
            const d = parseInt(m[3], 10);
            const hh = parseInt(m[4], 10);
            const mm = parseInt(m[5], 10);
            const ss = m[6] ? parseInt(m[6], 10) : 0;
            // Heuristic: treat 1300-1600 as Jalali years
            if (y >= 1300 && y <= 1600 && typeof JDate !== 'undefined') {
              try {
                // First attempt: numeric ctor (year, monthIndex, day, hh, mm, ss)
                const j = new JDate(y, mo - 1, d, hh, mm, ss);
                if (j && j['_date']) return j['_date'];
              } catch (_) {}
              try {
                // Second attempt: string in expected format Y/m/d - H:i
                const pad = (n) => (n < 10 ? '0' + n : '' + n);
                const jStr = `${y}/${pad(mo)}/${pad(d)} - ${pad(hh)}:${pad(mm)}`;
                const j2 = new JDate(jStr);
                if (j2 && j2['_date']) return j2['_date'];
              } catch (_) {}
              // Fallback: keep as-is but avoid throw
              return new Date();
            }
          }
          // Fallback: let browser parse
          return new Date(value);
        }
        return new Date(value);
      } catch (e) {
        console.error('parseApiDate error:', e && e.message ? e.message : e, value);
        return new Date();
      }
    }

    // Check if required elements exist
    if (!calendarEl) {
      console.error('Calendar element not found');
      return;
    }

    // Function to load events from database
    async function loadEventsFromDatabase() {
      try {
        console.log('Loading events from database...');
        // Request events for visible range if available (FullCalendar calls fetchEvents with range too)
        let rangeParams = '';
        try {
          if (calendar && calendar.view && calendar.view.activeStart && calendar.view.activeEnd) {
            const s = calendar.view.activeStart.toISOString();
            const e = calendar.view.activeEnd.toISOString();
            rangeParams = `?start=${encodeURIComponent(s)}&end=${encodeURIComponent(e)}`;
          }
        } catch (_) {}
        const response = await fetch('/Calendar/GetEvents' + rangeParams);
        if (response.ok) {
          const data = await response.json();
          console.log('Events loaded:', data);
          console.log('Raw events array:', Array.isArray(data) ? data : data.events);
          const raw = Array.isArray(data) ? data : (data.events || []);
          currentEvents = raw.map(e => {
            try {
            const startVal = e.start || e.Start || e.startDate || e.StartDate;
            const endVal = e.end || e.End || e.endDate || e.EndDate || null;
            const startDate = parseApiDate(startVal) || new Date();
            const endDate = endVal ? parseApiDate(endVal) : null;
            const ext = e.extendedProps || {};
            const calendarName = ext.calendar || e.CategoryName || e.categoryName || e.category || 'Business';
            return {
              id: e.id || e.Id,
              title: e.title || e.Title || '',
              start: startDate,
              end: endDate,
              allDay: e.allDay === true || e.AllDay === true,
              url: e.url || e.Url || '',
              extendedProps: {
                calendar: calendarName,
                location: ext.location || e.Location || e.location || '',
                description: ext.description || e.Description || e.description || '',
                guests: ext.guests || [],
                isCompleted: (ext.isCompleted === true) || (e.IsCompleted === true),
                isManagerAnnouncement: (ext.isManagerAnnouncement === true) || (e.IsManagerAnnouncement === true)
              }
            };
            } catch (mapError) {
              console.error('Map event error:', mapError, e);
              return null;
            }
          });
          currentEvents = currentEvents.filter(Boolean);

          if (calendar && typeof calendar.refetchEvents === 'function') {
            calendar.refetchEvents();
          }
        }
      } catch (error) {
        console.error('Error loading events from database:', error);
        // On error, show empty calendar (no demo data)
        currentEvents = [];
      }
    }

    // Function to load calendar categories from database
    async function loadCalendarCategories() {
      try {
        console.log('Loading calendar categories...');
        const response = await fetch('/Calendar/GetCategories');
        if (response.ok) {
          const data = await response.json();
          console.log('Categories loaded:', data);
          updateCategoryFilters(data.categories || []);
          // Fill eventLabel select from DB categories
          const eventLabelSelect = document.getElementById('eventLabel');
          if (eventLabelSelect && Array.isArray(data.categories)) {
            const previous = eventLabelSelect.value;
            eventLabelSelect.innerHTML = '';
            data.categories.forEach(cat => {
              const opt = document.createElement('option');
              opt.value = cat.id;
              opt.textContent = cat.name;
              opt.setAttribute('data-label', 'primary');
              if (cat.color) opt.setAttribute('data-color', cat.color);
              opt.setAttribute('data-name', cat.name);
              eventLabelSelect.appendChild(opt);
            });
            // Re-init select2 if available
            if (typeof $(eventLabelSelect).select2 === 'function') {
              $(eventLabelSelect).select2('destroy');
              $(eventLabelSelect).select2({
                placeholder: 'انتخاب',
                dropdownParent: $(eventLabelSelect).parent(),
                minimumResultsForSearch: -1,
                templateResult: function(option){
                  if (!option.id) return option.text;
                  var colorHex = $(option.element).data('color');
                  var labelClass = $(option.element).data('label');
                  return colorHex
                    ? ("<span class='badge badge-dot me-2' style='background-color:" + colorHex + "'></span>" + option.text)
                    : ("<span class='badge badge-dot bg-" + labelClass + " me-2'> </span>" + option.text);
                },
                templateSelection: function(option){
                  if (!option.id) return option.text;
                  var colorHex = $(option.element).data('color');
                  var labelClass = $(option.element).data('label');
                  return colorHex
                    ? ("<span class='badge badge-dot me-2' style='background-color:" + colorHex + "'></span>" + option.text)
                    : ("<span class='badge badge-dot bg-" + labelClass + " me-2'> </span>" + option.text);
                },
                escapeMarkup: function (es) { return es; }
              });
            }
            if (previous) {
              eventLabelSelect.value = previous;
            }
          }
        }
      } catch (error) {
        console.error('Error loading calendar categories:', error);
      }
    }

    // Function to load users from database
    async function loadUsersFromDatabase() {
      try {
        console.log('Loading users from database...');
        const response = await fetch('/Calendar/GetUsers');
        console.log('Response status:', response.status);
        if (response.ok) {
          const data = await response.json();
          console.log('Users data:', data);
          if (data.success && data.users) {
            updateUserSelect(data.users);
          }
        } else {
          console.error('Failed to load users:', response.statusText);
        }
      } catch (error) {
        console.error('Error loading users from database:', error);
      }
    }

    // Function to update user select options
    function updateUserSelect(users) {
      console.log('Updating user select with:', users);
      if (eventGuests && eventGuests.length) {
        eventGuests.empty();
        
        users.forEach(user => {
          const avatarUrl = (user.avatar && typeof user.avatar === 'string' && user.avatar.trim() !== '') ? user.avatar : '/CompanyVariables/avatars/1.png';
          eventGuests.append(`<option value="${user.id}" data-avatar="${avatarUrl}">${user.text}</option>`);
        });
        
        // Reinitialize select2
        if (typeof eventGuests.select2 === 'function') {
          eventGuests.select2('destroy');
          eventGuests.select2({
            placeholder: 'انتخاب کاربران',
            dropdownParent: eventGuests.parent(),
            closeOnSelect: false,
            templateResult: renderGuestAvatar,
            templateSelection: renderGuestAvatar,
            escapeMarkup: function (m) { return m; }
          });
        }
      }
    }

    // Function to render guest avatar in select2
    function renderGuestAvatar(option) {
      // For optgroup or placeholder
      if (!option.id) {
        return option.text;
      }

      // Resolve avatar URL from option element or fallback by value
      var defaultAvatar = '/CompanyVariables/avatars/1.png';
      var avatarSrc = defaultAvatar;
      try {
        if (option.element) {
          avatarSrc = $(option.element).data('avatar') || defaultAvatar;
        } else if (eventGuests && eventGuests.length) {
          var $opt = eventGuests.find('option[value="' + option.id + '"]');
          if ($opt && $opt.length) {
            avatarSrc = $opt.data('avatar') || defaultAvatar;
          }
        }
      } catch (_) { avatarSrc = defaultAvatar; }

      // Build DOM node to avoid escaping issues
      var container = document.createElement('span');
      container.className = 'd-flex align-items-center';
      var img = document.createElement('img');
      img.src = avatarSrc;
      img.alt = 'avatar';
      img.className = 'rounded-circle me-2';
      img.style.width = '20px';
      img.style.height = '20px';
      img.style.objectFit = 'cover';
      var textNode = document.createTextNode(option.text);
      container.appendChild(img);
      container.appendChild(textNode);
      return container;
    }

    // Function to check event edit permissions
    async function checkEventEditPermissions(eventId) {
      try {
        const response = await fetch(`/Calendar/CanEditEvent?eventId=${eventId}`);
        const data = await response.json();
        
        if (data.success) {
          const canEdit = data.canEdit;
          const isManagerAnnouncement = eventToUpdate && eventToUpdate.extendedProps && eventToUpdate.extendedProps.isManagerAnnouncement;
          
          // Get current user role
          const roleResponse = await fetch('/Calendar/GetCurrentUserRole');
          const roleData = await roleResponse.json();
          const isAdmin = roleData.success && roleData.isAdmin;
          
          // Set form permissions
          setFormPermissions(canEdit, isManagerAnnouncement, isAdmin);
        }
      } catch (error) {
        console.error('Error checking event permissions:', error);
        // Default to read-only if there's an error
        setFormPermissions(false, true, false);
      }
    }

    // Function to set form permissions
    function setFormPermissions(canEdit, isManagerAnnouncement, isAdmin) {
      const formElements = [
        'eventTitle', 'eventLabel', 'eventStartDate', 'eventEndDate', 
        'eventURL', 'eventGuests', 'eventLocation', 'eventDescription',
        'isManagerAnnouncement', 'allDay-switch'
      ];
      
      const statusRadios = document.querySelectorAll('input[name="eventStatus"]');
      
      formElements.forEach(elementId => {
        const element = document.getElementById(elementId);
        if (element) {
          if (elementId === 'eventGuests') {
            // Handle Select2
            if (element.select2) {
              element.select2('enable', canEdit);
            }
          } else if (elementId === 'eventLabel') {
            // Handle Select2
            if (element.select2) {
              element.select2('enable', canEdit);
            }
          } else if (elementId === 'allDay-switch') {
            element.disabled = !canEdit;
          } else if (element.type === 'checkbox') {
            element.disabled = !canEdit;
          } else {
            element.readOnly = !canEdit;
            element.disabled = !canEdit;
          }
        }
      });
      
      // Handle status radios
      statusRadios.forEach(radio => {
        radio.disabled = !canEdit;
      });
      
      // Show/hide action buttons
      const btnUpdateEvent = document.querySelector('.btn-update-event');
      const btnDeleteEvent = document.querySelector('.btn-delete-event');
      const btnAddEvent = document.querySelector('.btn-add-event');
      
      if (btnUpdateEvent) btnUpdateEvent.style.display = canEdit ? 'inline-block' : 'none';
      if (btnDeleteEvent) btnDeleteEvent.style.display = canEdit ? 'inline-block' : 'none';
      
      // Add mark as read button for management announcements if user can't edit
      if (isManagerAnnouncement && !canEdit) {
        addMarkAsReadButton();
      } else {
        removeMarkAsReadButton();
      }
    }

    // Function to add mark as read button
    function addMarkAsReadButton() {
      removeMarkAsReadButton(); // Remove existing button first
      
      const buttonContainer = document.querySelector('.btn-update-event').parentElement;
      const markAsReadBtn = document.createElement('button');
      markAsReadBtn.type = 'button';
      markAsReadBtn.className = 'btn btn-success me-2';
      markAsReadBtn.innerHTML = '<i class="bx bx-check"></i> متوجه شدم';
      markAsReadBtn.onclick = markNotificationAsRead;
      
      buttonContainer.appendChild(markAsReadBtn);
    }

    // Function to remove mark as read button
    function removeMarkAsReadButton() {
      const existingBtn = document.querySelector('.btn-success[onclick="markNotificationAsRead"]');
      if (existingBtn) {
        existingBtn.remove();
      }
    }

    // Function to mark notification as read
    async function markNotificationAsRead() {
      try {
        // Get current user role to check if they can mark as read
        const roleResponse = await fetch('/Calendar/GetCurrentUserRole');
        const roleData = await roleResponse.json();
        
        if (!roleData.success) {
          throw new Error('خطا در دریافت اطلاعات کاربر');
        }
        
        // Show success message
        if (typeof Swal !== 'undefined') {
          Swal.fire({
            icon: 'success',
            title: 'موفقیت!',
            text: 'اعلان به عنوان خوانده شده علامت‌گذاری شد',
            showConfirmButton: false,
            timer: 2000,
            timerProgressBar: true
          });
        } else {
          alert('اعلان به عنوان خوانده شده علامت‌گذاری شد');
        }
        
        // Close the sidebar
        const sidebar = document.getElementById('addEventSidebar');
        if (sidebar) {
          const bsOffcanvas = bootstrap.Offcanvas.getInstance(sidebar);
          if (bsOffcanvas) {
            bsOffcanvas.hide();
          }
        }
        
        // Optionally, you could also mark the event as viewed in the calendar
        // This would require additional backend implementation
        console.log('Event marked as read by user:', roleData.userId);
        
      } catch (error) {
        console.error('Error marking notification as read:', error);
        if (typeof Swal !== 'undefined') {
          Swal.fire({
            icon: 'error',
            title: 'خطا!',
            text: 'خطا در علامت‌گذاری اعلان',
            showConfirmButton: true
          });
        } else {
          alert('خطا در علامت‌گذاری اعلان');
        }
      }
    }

    // Function to update category filters
    function updateCategoryFilters(categories) {
      const filterContainer = document.querySelector('.app-calendar-events-filter');
      if (filterContainer && categories.length > 0) {
        filterContainer.innerHTML = '';
        
        categories.forEach(category => {
          const filterItem = document.createElement('div');
          filterItem.className = 'form-check mb-2 pb-1';
          const color = category.color || '#3788d8';
          categoryNameToColorHex[category.name] = color;
          filterItem.innerHTML = `
            <input class="form-check-input input-filter" type="checkbox" id="select-${category.name.replace(/\s+/g,'-').toLowerCase()}" data-value="${category.name}" checked>
            <label class="form-check-label d-flex align-items-center" for="select-${category.name.replace(/\s+/g,'-').toLowerCase()}">
              <span class="badge badge-dot me-2" style="background-color:${color}"></span>
              <span>${category.name}</span>
            </label>
          `;
          filterContainer.appendChild(filterItem);
        });
        
        // Reinitialize filter functionality
        initializeFilters();
        filtersLoaded = true;
        if (calendar && typeof calendar.refetchEvents === 'function') {
          calendar.refetchEvents();
        }
      }
    }

    // Function to initialize filters
    function initializeFilters() {
      const newFilterInput = [].slice.call(document.querySelectorAll('.input-filter'));
      
      if (newFilterInput.length > 0) {
        newFilterInput.forEach(item => {
          item.addEventListener('click', () => {
            try {
              document.querySelectorAll('.input-filter:checked').length < document.querySelectorAll('.input-filter').length
                ? (selectAll.checked = false)
                : (selectAll.checked = true);
              if (calendar && typeof calendar.refetchEvents === 'function') {
                calendar.refetchEvents();
              }
            } catch (error) {
              console.error('Error handling filter input:', error);
            }
          });
        });
      }
    }

    // Init event Offcanvas
    const bsAddEventSidebar = new bootstrap.Offcanvas(addEventSidebar);

    // Event Label (select2)
    if (eventLabel && eventLabel.length && typeof eventLabel.select2 === 'function') {
      try {
        function renderBadges(option) {
          if (!option.id) {
            return option.text;
          }
          var $badge =
            "<span class='badge badge-dot bg-" + $(option.element).data('label') + " me-2'> " + '</span>' + option.text;

          return $badge;
        }
        eventLabel.wrap('<div class="position-relative"></div>').select2({
          placeholder: 'انتخاب',
          dropdownParent: eventLabel.parent(),
          templateResult: renderBadges,
          templateSelection: renderBadges,
          minimumResultsForSearch: -1,
          escapeMarkup: function (es) {
            return es;
          }
        });
      } catch (error) {
        console.error('Error initializing event label select2:', error);
      }
    }

    // Event Guests (select2)
    if (eventGuests && eventGuests.length && typeof eventGuests.select2 === 'function') {
      try {
        eventGuests.wrap('<div class="position-relative"></div>').select2({
          placeholder: 'انتخاب کاربران',
          dropdownParent: eventGuests.parent(),
          closeOnSelect: false,
          templateResult: renderGuestAvatar,
          templateSelection: renderGuestAvatar,
          escapeMarkup: function (es) {
            return es;
          }
        });
      } catch (error) {
        console.error('Error initializing event guests select2:', error);
      }
    }

    // Event start (flatpicker)
    let start;
    if (eventStartDate && typeof eventStartDate.flatpickr === 'function') {
      try {
        start = eventStartDate.flatpickr({
          enableTime: true,
          altInput: true,
          altFormat: 'Y/m/d - H:i',
          onReady: function (selectedDates, dateStr, instance) {
            if (instance.isMobile) {
              instance.mobileInput.setAttribute('step', null);
            }
          },
          locale: 'fa',
          disableMobile: true
        });
      } catch (error) {
        console.error('Error initializing start date picker:', error);
      }
    }

    // Event end (flatpicker)
    let end;
    if (eventEndDate && typeof eventEndDate.flatpickr === 'function') {
      try {
        end = eventEndDate.flatpickr({
          enableTime: true,
          altInput: true,
          altFormat: 'Y/m/d - H:i',
          onReady: function (selectedDates, dateStr, instance) {
            if (instance.isMobile) {
              instance.mobileInput.setAttribute('step', null);
            }
          },
          locale: 'fa',
          disableMobile: true
        });
      } catch (error) {
        console.error('Error initializing end date picker:', error);
      }
    }

    // Inline sidebar calendar (flatpicker)
    if (inlineCalendar && typeof inlineCalendar.flatpickr === 'function') {
      try {
        inlineCalInstance = inlineCalendar.flatpickr({
          monthSelectorType: 'static',
          inline: true,
          locale: 'fa',
          disableMobile: true
        });
      } catch (error) {
        console.error('Error initializing inline calendar:', error);
      }
    }

    // Event click function
    function eventClick(info) {
      eventToUpdate = info.event;
      if (eventToUpdate.url) {
        info.jsEvent.preventDefault();
        window.open(eventToUpdate.url, '_blank');
      }
      bsAddEventSidebar.show();

      // Check if user can edit this event
      checkEventEditPermissions(eventToUpdate.id);

      btnAddEvent.classList.add('d-none');
      btnUpdateEvent.classList.remove('d-none');

      // For update event set offcanvas title text: Update Event
      if (offcanvasTitle) {
        offcanvasTitle.innerHTML = 'به‌روزرسانی رویداد';
      }
      btnDeleteEvent.classList.remove('d-none');

      if (eventTitle) eventTitle.value = eventToUpdate.title || '';
      if (start && start.setDate) {
        start.setDate(new JDate(eventToUpdate.start), true, 'Y-m-d');
      }
      if (allDaySwitch) {
        allDaySwitch.checked = eventToUpdate.allDay === true;
      }
      if (end && end.setDate) {
        eventToUpdate.end !== null
          ? end.setDate(new JDate(eventToUpdate.end), true, 'Y-m-d')
          : end.setDate(new JDate(eventToUpdate.start), true, 'Y-m-d');
      }
      if (eventLabel && eventLabel.val) {
        try {
          const labelDom = document.getElementById('eventLabel');
          let targetValue = '';
          if (labelDom && labelDom.options) {
            for (let i = 0; i < labelDom.options.length; i++) {
              const o = labelDom.options[i];
              if ((o.getAttribute('data-name') || o.textContent) === eventToUpdate.extendedProps.calendar) {
                targetValue = o.value;
                break;
              }
            }
          }
          if (targetValue) {
            eventLabel.val(targetValue).trigger('change');
          }
        } catch (e) {}
      }
      try {
        // Set status radios from event props if present
        const isCompleted = !!(eventToUpdate.extendedProps && eventToUpdate.extendedProps.isCompleted);
        const isOverdue = eventToUpdate.end ? (new Date(eventToUpdate.end) < new Date() && !isCompleted) : (new Date(eventToUpdate.start) < new Date() && !isCompleted);
        if (eventStatusInputs && eventStatusInputs.length) {
          eventStatusInputs.forEach(function(input){ input.checked = false; });
          const target = isCompleted ? document.getElementById('statusDone') : (isOverdue ? document.getElementById('statusOverdue') : document.getElementById('statusPending'));
          if (target) target.checked = true;
        }
      } catch (_) {}
      if (eventLocation) {
        eventLocation.value = eventToUpdate.extendedProps.location || '';
      }
      if (eventGuests && eventGuests.val) {
        eventGuests.val(eventToUpdate.extendedProps.guests || []).trigger('change');
      }
      if (eventDescription) {
        eventDescription.value = eventToUpdate.extendedProps.description || '';
      }
      // Set management notification checkbox if available
      const managerAnnouncementCheckbox = document.getElementById('isManagerAnnouncement');
      if (managerAnnouncementCheckbox) {
        managerAnnouncementCheckbox.checked = !!(eventToUpdate.extendedProps && eventToUpdate.extendedProps.isManagerAnnouncement);
      }

      // Ensure users are loaded for editing
      if (typeof loadUsersFromDatabase === 'function') {
        loadUsersFromDatabase();
      }
      
      // Ensure Select2 is properly initialized for eventGuests
      if (eventGuests && eventGuests.length && typeof eventGuests.select2 === 'function') {
        try {
          // Refresh the Select2 to ensure it's properly initialized
          eventGuests.select2('destroy');
          eventGuests.select2({
            placeholder: 'انتخاب کاربران',
            dropdownParent: eventGuests.parent(),
            closeOnSelect: false,
            templateResult: renderGuestAvatar,
            templateSelection: renderGuestAvatar,
            escapeMarkup: function (es) {
              return es;
            }
          });
          
          // Ensure users are loaded and Select2 is populated
          setTimeout(() => {
            if (typeof loadUsersFromDatabase === 'function') {
              loadUsersFromDatabase();
            }
          }, 100);
        } catch (error) {
          console.error('Error reinitializing eventGuests Select2:', error);
        }
      }
    }

    // Modify sidebar toggler
    function modifyToggler() {
      const fcSidebarToggleButton = document.querySelector('.fc-sidebarToggle-button');
      if (fcSidebarToggleButton) {
        fcSidebarToggleButton.classList.remove('fc-button-primary');
        fcSidebarToggleButton.classList.add('d-lg-none', 'd-inline-block', 'ps-0');
        while (fcSidebarToggleButton.firstChild) {
          fcSidebarToggleButton.firstChild.remove();
        }
        fcSidebarToggleButton.setAttribute('data-bs-toggle', 'sidebar');
        fcSidebarToggleButton.setAttribute('data-overlay', '');
        fcSidebarToggleButton.setAttribute('data-target', '#app-calendar-sidebar');
        fcSidebarToggleButton.insertAdjacentHTML('beforeend', '<i class="mdi mdi-menu"></i>');
        
        // Add click event for sidebar toggle
        fcSidebarToggleButton.addEventListener('click', function() {
          if (appCalendarSidebar) {
            appCalendarSidebar.classList.toggle('show');
          }
          if (appOverlay) {
            appOverlay.classList.toggle('show');
          }
        });
      }
    }

    // Filter events by calender
    function selectedCalendars() {
      let selected = [],
        filterInputChecked = [].slice.call(document.querySelectorAll('.input-filter:checked'));

      filterInputChecked.forEach(item => {
        selected.push(item.getAttribute('data-value'));
      });

      return selected;
    }

    // --------------------------------------------------------------------------------------------------
    // fetchEvents function for FullCalendar
    // --------------------------------------------------------------------------------------------------
    function fetchEvents(info, successCallback) {
      try {
        if (!filtersLoaded) {
          successCallback(currentEvents);
          return;
        }
        let calendars = selectedCalendars();
        console.log('Selected calendars:', calendars);
        console.log('Current events:', currentEvents);
        
        const norm = (s) => (s || '').toString().trim().toLowerCase();
        let selectedEvents = currentEvents.filter(function (event) {
          const eventCalendarRaw = event.extendedProps ? (event.extendedProps.calendar || event.CategoryName || event.categoryName || event.category || '') : '';
          const eventCalendar = eventCalendarRaw.toString();
          const isSelected = calendars.map(norm).includes(norm(eventCalendar));
          // include items that have no real category (or only fallback like 'Business') so they are not hidden unintentionally
          const noRealCategory = !eventCalendar || norm(eventCalendar) === 'business';
          const keep = isSelected || noRealCategory;
          console.log(`Event ${event.title} calendar: ${eventCalendar || '(none)'}, selected: ${isSelected}, keep: ${keep}`);
          return keep;
        });
        
        console.log('Filtered events:', selectedEvents);
        
        if (successCallback && typeof successCallback === 'function') {
          successCallback(selectedEvents);
        }
      } catch (error) {
        console.error('Error fetching events:', error);
        if (successCallback && typeof successCallback === 'function') {
          successCallback([]);
        }
      }
    }

    // Wait for FullCalendar to be loaded (either theme bundle or CDN global)
    function waitForFullCalendar() {
      return new Promise((resolve) => {
        const ready = () => {
          const env = detectCalendarEnv();
          return env.hasThemeBundle || env.hasFullCalendarNamespace;
        };
        if (ready()) {
          console.log('FullCalendar environment ready');
          resolve();
          return;
        }
        console.log('Waiting for FullCalendar assets...');
        const checkInterval = setInterval(() => {
          if (ready()) {
            clearInterval(checkInterval);
            console.log('FullCalendar assets detected');
            resolve();
          }
        }, 100);
        setTimeout(() => {
          clearInterval(checkInterval);
          console.warn('FullCalendar loading timeout, continuing anyway');
          resolve();
        }, 5000);
      });
    }

    // Initialize calendar after FullCalendar is loaded
    async function initializeCalendar() {
      try {
        console.log('Waiting for FullCalendar to load...');
        await waitForFullCalendar();
        console.log('FullCalendar loaded, initializing calendar...');
        
        // Resolve ctor and plugins for both environments
        const env = detectCalendarEnv();
        let CalendarCtor = null;
        let resolvedPlugins = undefined; // let FullCalendar detect plugins when using CDN bundle
        if (env.hasThemeBundle) {
          CalendarCtor = window.Calendar;
          if (typeof window.calendarPlugins === 'object') {
            const { dayGrid, timeGrid, interaction, list } = window.calendarPlugins;
            resolvedPlugins = [interaction, dayGrid, timeGrid, list];
          }
        } else if (env.hasFullCalendarNamespace) {
          CalendarCtor = window.FullCalendar.Calendar;
          // For CDN index.global build, plugins are bundled globally → omit plugins option
          resolvedPlugins = undefined;
        } else {
          console.error('FullCalendar constructor not found');
          return;
        }
        
        calendar = new CalendarCtor(calendarEl, {
          initialView: 'dayGridMonth',
          events: fetchEvents,
          // plugins only for theme bundle; omit for CDN global
          ...(resolvedPlugins ? { plugins: resolvedPlugins } : {}),
          editable: true,
          dragScroll: true,
          dayMaxEvents: 2,
          eventResizableFromStart: true,
          customButtons: {
            sidebarToggle: {
              text: 'نوار کناری'
            }
          },
          headerToolbar: {
            start: 'sidebarToggle, prev,next, title',
            end: 'dayGridMonth,timeGridWeek,timeGridDay,listMonth'
          },
          eventDidMount: function(info){
            try {
              // colorize event blocks and list items based on category name
              const cat = (info.event.extendedProps && (info.event.extendedProps.calendar || info.event.extendedProps.CategoryName)) || '';
              // try to find matching color from filters (badge inline style)
              const id = 'select-' + cat.toString().replace(/\s+/g,'-').toLowerCase();
              const label = document.querySelector('label[for="'+id+'"] .badge');
              const hex = label ? label.style.backgroundColor : '';
              if (hex) {
                // dayGrid/timeGrid boxes
                info.el.style.setProperty('background-color', hex, 'important');
                info.el.style.setProperty('border-color', hex, 'important');
                info.el.style.setProperty('color', '#fff', 'important');
                // list view item dot/border
                const dot = info.el.querySelector('.fc-list-event-dot');
                if (dot) dot.style.setProperty('border-color', hex, 'important');
                // give list row a subtle bg
                const row = info.el.closest('.fc-list-event');
                if (row) row.style.setProperty('background-color', 'rgba(0,0,0,0.04)', 'important');
              } else {
                // default subtle for list view even if no color
                const row = info.el.closest('.fc-list-event');
                if (row) row.style.setProperty('background-color', 'rgba(0,0,0,0.04)', 'important');
              }
            } catch (e) {}
          },
          direction: 'rtl',
          initialDate: new Date(),
          navLinks: true,
          eventClassNames: function ({ event: calendarEvent }) {
            const colorName = calendarsColor[calendarEvent._def.extendedProps.calendar] || 'primary';
            return ['fc-event-' + colorName];
          },
          dateClick: function (info) {
            try {
              let date = new Date(info.date);
              resetValues();
              bsAddEventSidebar.show();

              // For new event set offcanvas title text: Add Event
              if (offcanvasTitle) {
                offcanvasTitle.innerHTML = 'افزودن رویداد';
              }

              if (btnAddEvent) btnAddEvent.classList.remove('d-none');
              if (btnUpdateEvent) btnUpdateEvent.classList.add('d-none');
              if (btnDeleteEvent) btnDeleteEvent.classList.add('d-none');
              
              if (start && start.setDate) start.setDate(new JDate(date), true, 'Y-m-d');
              if (end && end.setDate) end.setDate(new JDate(date), true, 'Y-m-d');
            } catch (error) {
              console.error('Error handling date click:', error);
            }
          },
          eventDrop: function(info){
            try {
              const ev = info.event;
              const payload = {
                Id: ev.id,
                Title: ev.title,
                Description: ev.extendedProps && ev.extendedProps.description ? ev.extendedProps.description : '',
                StartDate: ev.start ? toLocalIsoString(ev.start) : null,
                EndDate: ev.end ? toLocalIsoString(ev.end) : (ev.start ? toLocalIsoString(ev.start) : null),
                IsAllDay: ev.allDay === true,
                Color: '#3788d8',
                EventType: 1,
                Visibility: 1,
                Location: ev.extendedProps && ev.extendedProps.location ? ev.extendedProps.location : '',
                CategoryId: null,
                ParticipantUserIds: [],
                IsManagerAnnouncement: !!(ev.extendedProps && ev.extendedProps.isManagerAnnouncement),
                IsCompleted: !!(ev.extendedProps && ev.extendedProps.isCompleted)
              };
              const idx = currentEvents.findIndex(e => e.id == ev.id);
              if (idx > -1) {
                currentEvents[idx].start = ev.start;
                currentEvents[idx].end = ev.end;
                currentEvents[idx].allDay = ev.allDay;
              }
              updateEvent({ id: ev.id, ...currentEvents[idx] }, payload);
            } catch (e) { console.error('Error on eventDrop:', e); }
          },
          eventResize: function(info){
            try {
              const ev = info.event;
              const payload = {
                Id: ev.id,
                Title: ev.title,
                Description: ev.extendedProps && ev.extendedProps.description ? ev.extendedProps.description : '',
                StartDate: ev.start ? toLocalIsoString(ev.start) : null,
                EndDate: ev.end ? toLocalIsoString(ev.end) : (ev.start ? toLocalIsoString(ev.start) : null),
                IsAllDay: ev.allDay === true,
                Color: '#3788d8',
                EventType: 1,
                Visibility: 1,
                Location: ev.extendedProps && ev.extendedProps.location ? ev.extendedProps.location : '',
                CategoryId: null,
                ParticipantUserIds: [],
                IsManagerAnnouncement: !!(ev.extendedProps && ev.extendedProps.isManagerAnnouncement),
                IsCompleted: !!(ev.extendedProps && ev.extendedProps.isCompleted)
              };
              const idx = currentEvents.findIndex(e => e.id == ev.id);
              if (idx > -1) {
                currentEvents[idx].start = ev.start;
                currentEvents[idx].end = ev.end;
                currentEvents[idx].allDay = ev.allDay;
              }
              updateEvent({ id: ev.id, ...currentEvents[idx] }, payload);
            } catch (e) { console.error('Error on eventResize:', e); }
          },
          eventClick: function (info) {
            eventClick(info);
          },
          datesSet: function () {
            modifyToggler();
          },
          viewDidMount: function () {
            modifyToggler();
          },

          // Locale
          locale: 'fa',
          firstDay: 6,
          buttonText: {
            today: 'امروز',
            month: 'ماه',
            week: 'هفته',
            day: 'روز',
            list: 'لیست'
          },
          weekText: 'هفته',
          allDayText: 'تمام روز',
          moreLinkText: function(n) {
            return '+' + n + ' مورد دیگر';
          },
          noEventsText: 'رویدادی برای نمایش وجود ندارد'
        });

        // Render calendar
        calendar.render();
        // Modify sidebar toggler
        modifyToggler();
        
        console.log('Calendar initialized successfully');
      } catch (error) {
        console.error('Error creating FullCalendar instance:', error);
        return;
      }
    }

    const eventForm = document.getElementById('eventForm');
    if (!eventForm) {
      console.error('Event form not found');
      return;
    }
    
    // Sidebar Toggle Btn
    if (btnToggleSidebar) {
      btnToggleSidebar.addEventListener('click', e => {
        try {
          if (btnCancel) btnCancel.classList.remove('d-none');
        } catch (error) {
          console.error('Error handling sidebar toggle button:', error);
        }
      });
    }

    // Add Event
    async function addEvent(eventData, apiPayload) {
      try {
        console.log('Adding event:', eventData);
        const response = await fetch('/Calendar/AddEvent', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(apiPayload || eventData)
        });

        if (response.ok) {
          const result = await response.json();
          if (result.success) {
            // Add to local events array with server-generated ID
            if (result.eventId) {
              eventData.id = result.eventId;
            }
            currentEvents.push(eventData);
            if (calendar && typeof calendar.refetchEvents === 'function') {
              calendar.refetchEvents();
            }
            
            // Show success message
            if (typeof Swal !== 'undefined') {
              Swal.fire({
                icon: 'success',
                title: 'موفقیت!',
                text: 'رویداد با موفقیت ثبت شد',
                showConfirmButton: false,
                timer: 2000,
                timerProgressBar: true
              });
            } else {
              alert('رویداد با موفقیت ثبت شد');
            }
          } else {
            throw new Error(result.message || 'خطا در ثبت رویداد');
          }
        } else {
          throw new Error('خطا در ارتباط با سرور');
        }
      } catch (error) {
        console.error('Error adding event:', error);
        
        // Show error message
        if (typeof Swal !== 'undefined') {
          Swal.fire({
            icon: 'error',
            title: 'خطا!',
            text: error.message || 'خطا در ثبت رویداد',
            showConfirmButton: false,
            timer: 2000,
            timerProgressBar: true
          });
        } else {
          alert('خطا در ثبت رویداد: ' + error.message);
        }
      }
    }

    // Update Event
    async function updateEvent(eventData, apiPayload) {
      try {
        const response = await fetch('/Calendar/UpdateEvent', {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(apiPayload || eventData)
        });

        if (response.ok) {
          const result = await response.json();
          if (result.success) {
            // Update local events array
            eventData.id = parseInt(eventData.id);
            const eventIndex = currentEvents.findIndex(el => el.id === eventData.id);
            if (eventIndex !== -1) {
              currentEvents[eventIndex] = eventData;
            }
            
            if (calendar && typeof calendar.refetchEvents === 'function') {
              calendar.refetchEvents();
            }
            
            // Show success message
            if (typeof Swal !== 'undefined') {
              Swal.fire({
                icon: 'success',
                title: 'موفقیت!',
                text: 'رویداد با موفقیت به‌روزرسانی شد',
                showConfirmButton: false,
                timer: 2000,
                timerProgressBar: true
              });
            } else {
              alert('رویداد با موفقیت به‌روزرسانی شد');
            }
          } else {
            throw new Error(result.message || 'خطا در به‌روزرسانی رویداد');
          }
        } else {
          throw new Error('خطا در ارتباط با سرور');
        }
      } catch (error) {
        console.error('Error updating event:', error);
        
        // Show error message
        if (typeof Swal !== 'undefined') {
          Swal.fire({
            icon: 'error',
            title: 'خطا!',
            text: error.message || 'خطا در به‌روزرسانی رویداد',
            showConfirmButton: false,
            timer: 2000,
            timerProgressBar: true
          });
        } else {
          alert('خطا در به‌روزرسانی رویداد: ' + error.message);
        }
      }
    }

    // Remove Event
    async function removeEvent(eventId) {
      try {
        // Show confirmation dialog
        let confirmed = false;
        if (typeof Swal !== 'undefined') {
          const result = await Swal.fire({
            icon: 'warning',
            title: 'آیا مطمئن هستید؟',
            text: 'این عملیات قابل بازگشت نیست',
            showCancelButton: true,
            confirmButtonText: 'بله، حذف کن',
            cancelButtonText: 'انصراف'
          });
          confirmed = result.isConfirmed;
        } else {
          confirmed = confirm('آیا مطمئن هستید که می‌خواهید این رویداد را حذف کنید؟');
        }

        if (!confirmed) return;

        // Delete event from database
        const response = await fetch(`/Calendar/DeleteEvent/${eventId}`, {
          method: 'DELETE'
        });

        if (response.ok) {
          const result = await response.json();
          if (result.success) {
            // Remove from local events array
            currentEvents = currentEvents.filter(function (event) {
              return event.id != eventId;
            });
            
            if (calendar && typeof calendar.refetchEvents === 'function') {
              calendar.refetchEvents();
            }
            
            // Show success message
            if (typeof Swal !== 'undefined') {
              Swal.fire({
                icon: 'success',
                title: 'موفقیت!',
                text: 'رویداد با موفقیت حذف شد',
                showConfirmButton: false,
                timer: 2000,
                timerProgressBar: true
              });
            } else {
              alert('رویداد با موفقیت حذف شد');
            }
          } else {
            throw new Error(result.message || 'خطا در حذف رویداد');
          }
        } else {
          throw new Error('خطا در ارتباط با سرور');
        }
      } catch (error) {
        console.error('Error removing event:', error);
        
        // Show error message
        if (typeof Swal !== 'undefined') {
          Swal.fire({
            icon: 'error',
            title: 'خطا!',
            text: error.message || 'خطا در حذف رویداد',
            showConfirmButton: false,
            timer: 2000,
            timerProgressBar: true
          });
        } else {
          alert('خطا در حذف رویداد: ' + error.message);
        }
      }
    }

    // Add new event
    btnAddEvent.addEventListener('click', e => {
      try {
        e.preventDefault();
        
        // Validate form manually
        let isValid = true;
        if (!eventTitle || !eventTitle.value.trim()) {
          isValid = false;
          if (eventTitle) eventTitle.classList.add('is-invalid');
        } else if (eventTitle) {
          eventTitle.classList.remove('is-invalid');
        }
        
        if (!eventStartDate || !eventStartDate.value) {
          isValid = false;
          if (eventStartDate) eventStartDate.classList.add('is-invalid');
        } else if (eventStartDate) {
          eventStartDate.classList.remove('is-invalid');
        }
        
        if (!eventEndDate || !eventEndDate.value) {
          isValid = false;
          if (eventEndDate) eventEndDate.classList.add('is-invalid');
        } else if (eventEndDate) {
          eventEndDate.classList.remove('is-invalid');
        }
        
        if (!isValid) {
          // Show error message
          if (typeof Swal !== 'undefined') {
            Swal.fire({
              icon: 'error',
              title: 'خطا!',
              text: 'لطفا تمام فیلدهای اجباری را پر کنید',
              showConfirmButton: false,
              timer: 4000,
              timerProgressBar: true
            });
          } else {
            alert('لطفا تمام فیلدهای اجباری را پر کنید');
          }
          return;
        }
        
        // resolve category id and name
        let selectedCategoryId = null;
        let selectedCategoryName = 'Business';
        try {
          const labelDom = document.getElementById('eventLabel');
          if (labelDom) {
            selectedCategoryId = labelDom.value ? parseInt(labelDom.value) : null;
            const opt = labelDom.options[labelDom.selectedIndex];
            selectedCategoryName = (opt && (opt.getAttribute('data-name') || opt.textContent)) || 'Business';
          }
        } catch (e) {}

        let newEvent = {
          title: eventTitle ? eventTitle.value.trim() : '',
          start: eventStartDate && eventStartDate.value ? new JDate(eventStartDate.value)['_date'] : new Date(),
          end: eventEndDate && eventEndDate.value ? new JDate(eventEndDate.value)['_date'] : new Date(),
          startStr: eventStartDate ? eventStartDate.value : '',
          endStr: eventEndDate ? eventEndDate.value : '',
          display: 'block',
          extendedProps: {
            location: eventLocation ? eventLocation.value : '',
            guests: eventGuests && eventGuests.val ? eventGuests.val() : [],
            calendar: selectedCategoryName,
            description: eventDescription ? eventDescription.value : '',
            isCompleted: false
          },
          categoryId: selectedCategoryId
        };
        
        if (eventUrl && eventUrl.value) {
          newEvent.url = eventUrl.value;
        }
        
        if (allDaySwitch && allDaySwitch.checked) {
          newEvent.allDay = true;
        }
        
        // Build API payload to match server model
        const payload = {
          Title: newEvent.title,
          Description: newEvent.extendedProps.description,
          StartDate: newEvent.start instanceof Date ? toLocalIsoString(newEvent.start) : newEvent.start,
          EndDate: newEvent.end instanceof Date ? toLocalIsoString(newEvent.end) : newEvent.end,
          IsAllDay: allDaySwitch && allDaySwitch.checked ? true : false,
          Color: '#3788d8',
          EventType: 1, // Personal
          Visibility: 1, // Private
          Location: newEvent.extendedProps.location,
          CategoryId: selectedCategoryId,
          ParticipantUserIds: (eventGuests && eventGuests.val) ? eventGuests.val() : [],
          IsManagerAnnouncement: (function(){
            try { const checked = document.getElementById('isManagerAnnouncement'); return checked && checked.checked; } catch (_) { return false; }
          })(),
          IsCompleted: (function(){
            try { const checked = document.querySelector('input[name="eventStatus"]:checked'); return checked && checked.value === 'done'; } catch (_) { return false; }
          })()
        };

        // Add event to database and calendar
        addEvent(newEvent, payload);
        bsAddEventSidebar.hide();
        
      } catch (error) {
        console.error('Error adding new event:', error);
        if (typeof Swal !== 'undefined') {
          Swal.fire({
            icon: 'error',
            title: 'خطا!',
            text: 'خطا در افزودن رویداد: ' + error.message,
            showConfirmButton: false,
            timer: 2000,
            timerProgressBar: true
          });
        } else {
          alert('خطا در افزودن رویداد: ' + error.message);
        }
      }
    });

    // Update event
    btnUpdateEvent.addEventListener('click', e => {
      try {
        e.preventDefault();
        
        // Validate form manually
        let isValid = true;
        if (!eventTitle || !eventTitle.value.trim()) {
          isValid = false;
          if (eventTitle) eventTitle.classList.add('is-invalid');
        } else if (eventTitle) {
          eventTitle.classList.remove('is-invalid');
        }
        
        if (!eventStartDate || !eventStartDate.value) {
          isValid = false;
          if (eventStartDate) eventStartDate.classList.add('is-invalid');
        } else if (eventStartDate) {
          eventStartDate.classList.remove('is-invalid');
        }
        
        if (!eventEndDate || !eventEndDate.value) {
          isValid = false;
          if (eventEndDate) eventEndDate.classList.add('is-invalid');
        } else if (eventEndDate) {
          eventEndDate.classList.remove('is-invalid');
        }
        
        if (!isValid) {
          // Show error message
          if (typeof Swal !== 'undefined') {
            Swal.fire({
              icon: 'error',
              title: 'خطا!',
              text: 'لطفا تمام فیلدهای اجباری را پر کنید',
              showConfirmButton: false,
              timer: 4000,
              timerProgressBar: true
            });
          } else {
            alert('لطفا تمام فیلدهای اجباری را پر کنید');
          }
          return;
        }
        
        // resolve category id and name
        let selectedCategoryId = null;
        let selectedCategoryName = 'Business';
        try {
          const labelDom = document.getElementById('eventLabel');
          if (labelDom) {
            selectedCategoryId = labelDom.value ? parseInt(labelDom.value) : null;
            const opt = labelDom.options[labelDom.selectedIndex];
            selectedCategoryName = (opt && (opt.getAttribute('data-name') || opt.textContent)) || 'Business';
          }
        } catch (e) {}

        let eventData = {
          id: eventToUpdate ? eventToUpdate.id : null,
          title: eventTitle ? eventTitle.value.trim() : '',
          start: eventStartDate && eventStartDate.value ? new JDate(eventStartDate.value)['_date'] : new Date(),
          end: eventEndDate && eventEndDate.value ? new JDate(eventEndDate.value)['_date'] : new Date(),
          url: eventUrl ? eventUrl.value : '',
          extendedProps: {
            location: eventLocation ? eventLocation.value : '',
            guests: eventGuests && eventGuests.val ? eventGuests.val() : [],
            calendar: selectedCategoryName,
            description: eventDescription ? eventDescription.value : '',
            isCompleted: (function(){
              try { const checked = document.querySelector('input[name="eventStatus"]:checked'); return checked && checked.value === 'done'; } catch (_) { return false; }
            })()
          },
          display: 'block',
          allDay: allDaySwitch && allDaySwitch.checked ? true : false
        };

        const payload = {
          Id: eventData.id,
          Title: eventData.title,
          Description: eventData.extendedProps.description,
          StartDate: eventData.start instanceof Date ? toLocalIsoString(eventData.start) : eventData.start,
          EndDate: eventData.end instanceof Date ? toLocalIsoString(eventData.end) : eventData.end,
          IsAllDay: eventData.allDay === true,
          Color: '#3788d8',
          EventType: 1,
          Visibility: 1,
          Location: eventData.extendedProps.location,
          CategoryId: selectedCategoryId,
          ParticipantUserIds: (eventGuests && eventGuests.val) ? eventGuests.val() : [],
          IsManagerAnnouncement: (function(){
            try { const checked = document.getElementById('isManagerAnnouncement'); return checked && checked.checked; } catch (_) { return false; }
          })(),
          IsCompleted: eventData.extendedProps.isCompleted === true
        };

        updateEvent(eventData, payload);
        bsAddEventSidebar.hide();
        
      } catch (error) {
        console.error('Error updating event:', error);
        if (typeof Swal !== 'undefined') {
          Swal.fire({
            icon: 'error',
            title: 'خطا!',
            text: 'خطا در به‌روزرسانی رویداد: ' + error.message,
            showConfirmButton: false,
            timer: 2000,
            timerProgressBar: true
          });
        } else {
          alert('خطا در به‌روزرسانی رویداد: ' + error.message);
        }
      }
    });

    // Call removeEvent function
    btnDeleteEvent.addEventListener('click', e => {
      try {
        if (eventToUpdate && eventToUpdate.id) {
          removeEvent(parseInt(eventToUpdate.id));
        }
        bsAddEventSidebar.hide();
      } catch (error) {
        console.error('Error deleting event:', error);
      }
    });

    // Reset event form inputs values
    function resetValues() {
      try {
        if (eventEndDate) eventEndDate.value = '';
        if (eventUrl) eventUrl.value = '';
        if (eventStartDate) eventStartDate.value = '';
        if (eventTitle) eventTitle.value = '';
        if (eventLocation) eventLocation.value = '';
        if (allDaySwitch) allDaySwitch.checked = false;
        if (eventGuests && eventGuests.val) eventGuests.val('').trigger('change');
        if (eventDescription) eventDescription.value = '';
        // Reset management notification checkbox
        const managerAnnouncementCheckbox = document.getElementById('isManagerAnnouncement');
        if (managerAnnouncementCheckbox) managerAnnouncementCheckbox.checked = false;
        
        // Reset form permissions to editable
        setFormPermissions(true, false, false);
        
        // Remove mark as read button
        removeMarkAsReadButton();
      } catch (error) {
        console.error('Error resetting values:', error);
      }
    }

    // When modal hides reset input values
    if (addEventSidebar) {
      addEventSidebar.addEventListener('hidden.bs.offcanvas', function () {
        try {
          resetValues();
        } catch (error) {
          console.error('Error handling modal hide:', error);
        }
      });
    }

    // Hide left sidebar if the right sidebar is open
    btnToggleSidebar.addEventListener('click', e => {
      try {
        if (btnDeleteEvent) btnDeleteEvent.classList.add('d-none');
        if (btnUpdateEvent) btnUpdateEvent.classList.add('d-none');
        if (btnAddEvent) btnAddEvent.classList.remove('d-none');
        if (appCalendarSidebar) appCalendarSidebar.classList.remove('show');
        if (appOverlay) appOverlay.classList.remove('show');
        
        // Set form permissions for adding new event (always editable)
        setFormPermissions(true, false, false);
        
        // Remove mark as read button
        removeMarkAsReadButton();
      } catch (error) {
        console.error('Error handling sidebar toggle button:', error);
      }
    });

    // Calender filter functionality
    if (selectAll) {
      selectAll.addEventListener('click', e => {
        try {
          if (e.currentTarget.checked) {
            document.querySelectorAll('.input-filter').forEach(c => (c.checked = 1));
          } else {
            document.querySelectorAll('.input-filter').forEach(c => (c.checked = 0));
          }
          if (calendar && typeof calendar.refetchEvents === 'function') {
            calendar.refetchEvents();
          }
        } catch (error) {
          console.error('Error handling select all filter:', error);
        }
      });
    }

    if (filterInput) {
      filterInput.forEach(item => {
        item.addEventListener('click', () => {
          try {
            document.querySelectorAll('.input-filter:checked').length < document.querySelectorAll('.input-filter').length
              ? (selectAll.checked = false)
              : (selectAll.checked = true);
            if (calendar && typeof calendar.refetchEvents === 'function') {
              calendar.refetchEvents();
            }
          } catch (error) {
            console.error('Error handling filter input:', error);
          }
        });
      });
    }

    // Jump to date on sidebar(inline) calendar change
    if (inlineCalInstance) {
      inlineCalInstance.config.onChange.push(function (date) {
        try {
          if (calendar && typeof calendar.changeView === 'function') {
            calendar.changeView(calendar.view.type, moment(date[0]['_date']).format('YYYY-MM-DD'));
          }
          modifyToggler();
          if (appCalendarSidebar) appCalendarSidebar.classList.remove('show');
          if (appOverlay) appOverlay.classList.remove('show');
        } catch (error) {
          console.error('Error changing calendar view:', error);
        }
      });
    }

    // Initialize everything
    async function initializeEverything() {
      try {
        console.log('Starting initialization...');
        
        // First initialize the calendar
        await initializeCalendar();
        
        // Then load data
        await loadEventsFromDatabase();
        await loadCalendarCategories();
        await loadUsersFromDatabase();
        
        console.log('Initialization completed successfully');
      } catch (error) {
        console.error('Error during initialization:', error);
      }
    }

    // Start initialization
    initializeEverything();
  })();
});
