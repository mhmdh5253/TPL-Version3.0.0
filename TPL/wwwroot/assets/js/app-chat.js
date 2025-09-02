"use strict";

document.addEventListener("DOMContentLoaded", function () {
    // Chat state management
    let currentChatRoomId = null;
    let currentUser = null;
    let chatConnection = null;
    let typingTimer = null;
    let isTyping = false;

    // DOM elements
    const chatContactsSidebar = document.querySelector(".app-chat-contacts .sidebar-body");
    const chatContactListItems = [].slice.call(document.querySelectorAll(".chat-contact-list-item:not(.chat-contact-list-item-title)"));
    const chatHistoryBody = document.querySelector("#chat-history-body");
    const chatHistory = document.querySelector("#chat-history");
    const leftSidebar = document.querySelector(".app-chat-sidebar-left .sidebar-body");
    const rightSidebar = document.querySelector(".app-chat-sidebar-right .sidebar-body");
    const statusRadios = [].slice.call(document.querySelectorAll(".form-check-input[name='chat-user-status']"));
    const userAboutTextarea = $(".chat-sidebar-left-user-about");
    const messageForm = document.querySelector(".form-send-message");
    const messageInput = document.querySelector("#message-input");
    const chatSearchInput = document.querySelector(".chat-search-input");
    const speechToTextBtn = $("#speech-to-text-btn");
    const fileInput = document.querySelector("#attach-doc");
    const sendBtn = document.querySelector("#send-msg-btn");
    
    // Chat header elements
    const chatHeaderName = document.querySelector("#chat-header-name");
    const chatHeaderStatus = document.querySelector("#chat-header-status");
    const chatHeaderAvatar = document.querySelector("#chat-header-avatar img");
    
    // Chat status elements
    const welcomeMessage = document.querySelector("#welcome-message");
    const loadingMessages = document.querySelector("#loading-messages");
    const noMessages = document.querySelector("#no-messages");
    const errorMessages = document.querySelector("#error-messages");
    
    // Typing and upload indicators
    const typingIndicator = document.querySelector("#typing-indicator");
    const uploadProgress = document.querySelector("#upload-progress");
    const progressBar = document.querySelector("#upload-progress .progress-bar");

    // Status classes mapping
    const statusClasses = {
        active: "avatar-online",
        offline: "avatar-offline",
        away: "avatar-away",
        busy: "avatar-busy"
    };

    // Initialize chat functionality
    function initializeChat() {
        // Set current user information from ViewBag
        const currentUserElement = document.querySelector('.chat-sidebar-left-user h5');
        if (currentUserElement) {
            currentUser = {
                userName: currentUserElement.textContent,
                avatar: document.querySelector('.chat-sidebar-left-user .avatar img')?.src || '/assets/img/avatars/1.png'
            };
        }
        
        console.log("Initializing chat with current user:", currentUser);
        
        // Initially disable message input until a chat is selected
        disableMessageInput();
        
        // Setup scrollbars with a small delay to ensure DOM is ready
        setTimeout(() => {
            setupScrollbars();
            ensureScrollbars();
        }, 100);
        
        setupEventListeners();
        setupSignalR();
        loadChatRooms();
        loadContacts(); // Load real contacts from server
        setupSearchFunctionality();
        setupMessageForm();
        setupFileUpload();
        setupSpeechToText();
        setupStatusChanges();
        setupContactSelection();
        scrollToBottom();
        
        // Set up periodic refresh of contacts and chat rooms
        setupPeriodicRefresh();
        
        // Ensure scrollbars are working after everything is loaded
        setTimeout(() => {
            ensureScrollbars();
            refreshScrollbars();
        }, 500);
    }

    // Setup event listeners
    function setupEventListeners() {
        // Add any additional event listeners here
        console.log("Event listeners setup completed");
    }

    // Set up periodic refresh of data
    function setupPeriodicRefresh() {
        // Refresh contacts every 30 seconds
        setInterval(() => {
            try {
                const searchInput = document.querySelector('.chat-search-input');
                if (!searchInput || !searchInput.value?.trim()) {
                    loadContacts();
                }
            } catch (error) {
                console.warn("Error in contacts refresh interval:", error);
            }
        }, 30000);
        
        // Refresh chat rooms every 60 seconds
        setInterval(() => {
            try {
                loadChatRooms();
            } catch (error) {
                console.warn("Error in chat rooms refresh interval:", error);
            }
        }, 60000);
    }

    // Setup PerfectScrollbar for all sidebars
    function setupScrollbars() {
        // Store PerfectScrollbar instances for later use
        window.chatScrollbars = window.chatScrollbars || {};
        
        if (chatContactsSidebar && typeof PerfectScrollbar !== 'undefined') {
            try {
                window.chatScrollbars.contactsSidebar = new PerfectScrollbar(chatContactsSidebar, { 
                    wheelPropagation: false, 
                    suppressScrollX: true 
                });
            } catch (error) {
                console.warn("Failed to setup scrollbar for chat contacts sidebar:", error);
            }
        }
        
        // Setup scrollbar for chat list (chat rooms)
        const chatList = document.getElementById('chat-list');
        if (chatList && typeof PerfectScrollbar !== 'undefined') {
            try {
                window.chatScrollbars.chatList = new PerfectScrollbar(chatList, { 
                    wheelPropagation: false, 
                    suppressScrollX: true 
                });
                console.log("Chat list scrollbar initialized successfully");
            } catch (error) {
                console.warn("Failed to setup scrollbar for chat list:", error);
            }
        }
        
        if (chatHistoryBody && typeof PerfectScrollbar !== 'undefined') {
            try {
                window.chatScrollbars.chatHistory = new PerfectScrollbar(chatHistoryBody, { 
                    wheelPropagation: false, 
                    suppressScrollX: true 
                });
                console.log("Chat history scrollbar initialized successfully");
            } catch (error) {
                console.warn("Failed to setup scrollbar for chat history body:", error);
            }
        }
        
        if (leftSidebar && typeof PerfectScrollbar !== 'undefined') {
            try {
                window.chatScrollbars.leftSidebar = new PerfectScrollbar(leftSidebar, { 
                    wheelPropagation: false, 
                    suppressScrollX: true 
                });
            } catch (error) {
                console.warn("Failed to setup scrollbar for left sidebar:", error);
            }
        }
        
        if (rightSidebar && typeof PerfectScrollbar !== 'undefined') {
            try {
                window.chatScrollbars.rightSidebar = new PerfectScrollbar(rightSidebar, { 
                    wheelPropagation: false, 
                    suppressScrollX: true 
                });
            } catch (error) {
                console.warn("Failed to setup scrollbar for right sidebar:", error);
            }
        }
    }

    // Refresh scrollbars when content changes
    function refreshScrollbars() {
        if (typeof PerfectScrollbar !== 'undefined' && window.chatScrollbars) {
            // Update chat list scrollbar
            if (window.chatScrollbars.chatList) {
                try {
                    window.chatScrollbars.chatList.update();
                    console.log("Chat list scrollbar updated successfully");
                } catch (error) {
                    console.warn("Failed to update chat list scrollbar:", error);
                }
            }
            
            // Update chat history scrollbar
            if (window.chatScrollbars.chatHistory) {
                try {
                    window.chatScrollbars.chatHistory.update();
                    console.log("Chat history scrollbar updated successfully");
                } catch (error) {
                    console.warn("Failed to update chat history scrollbar:", error);
                }
            }
            
            // Update contacts sidebar scrollbar
            if (window.chatScrollbars.contactsSidebar) {
                try {
                    window.chatScrollbars.contactsSidebar.update();
                } catch (error) {
                    console.warn("Failed to update contacts sidebar scrollbar:", error);
                }
            }
            
            // Update left sidebar scrollbar
            if (window.chatScrollbars.leftSidebar) {
                try {
                    window.chatScrollbars.leftSidebar.update();
                } catch (error) {
                    console.warn("Failed to update left sidebar scrollbar:", error);
                }
            }
            
            // Update right sidebar scrollbar
            if (window.chatScrollbars.rightSidebar) {
                try {
                    window.chatScrollbars.rightSidebar.update();
                } catch (error) {
                    console.warn("Failed to update right sidebar scrollbar:", error);
                }
            }
        }
    }

    // Setup SignalR connection
    function setupSignalR() {
        try {
            chatConnection = new signalR.HubConnectionBuilder()
                .withUrl("/chatHub")
                .withAutomaticReconnect([0, 2000, 10000, 30000])
                .build();

            chatConnection.on("ReceiveMessage", function (message) {
                // Check if this message is for the current chat room
                if (message.chatRoomId === currentChatRoomId) {
                    displayMessage(message);
                    scrollToBottom();
                } else {
                    // Show notification for messages in other chat rooms
                    showNewMessageNotification(message);
                }
                
                // Refresh chat rooms to update unread counts
                loadChatRooms();
            });

            chatConnection.on("UserTyping", function (userId, userName, chatRoomId) {
                if (chatRoomId === currentChatRoomId) {
                    showTypingIndicator(userName);
                }
            });

            chatConnection.on("UserStoppedTyping", function (userId, chatRoomId) {
                if (chatRoomId === currentChatRoomId) {
                    hideTypingIndicator();
                }
            });

            chatConnection.on("MessageEdited", function (messageId, newContent) {
                updateMessageContent(messageId, newContent);
            });

            chatConnection.on("MessageDeleted", function (messageId) {
                removeMessage(messageId);
            });

            chatConnection.on("UserOnline", function (userId) {
                updateUserStatus(userId, true);
            });

            chatConnection.on("UserOffline", function (userId) {
                updateUserStatus(userId, false);
            });

            // Connection event handlers
            chatConnection.onreconnecting((error) => {
                console.log("SignalR reconnecting...", error);
                showConnectionStatus('در حال اتصال مجدد...', 'warning');
            });

            chatConnection.onreconnected((connectionId) => {
                console.log("SignalR reconnected. ConnectionId: ", connectionId);
                showConnectionStatus('متصل شد', 'success');
                
                // Rejoin current chat room if any
                if (currentChatRoomId) {
                    joinChatRoom(currentChatRoomId);
                }
            });

            chatConnection.onclose((error) => {
                console.log("SignalR connection closed.", error);
                showConnectionStatus('اتصال قطع شد', 'error');
            });

            chatConnection.start().catch(function (err) {
                console.error("SignalR Connection Error: ", err);
                showConnectionStatus('خطا در اتصال', 'error');
            });
        } catch (error) {
            console.error("SignalR not available: ", error);
            showConnectionStatus('SignalR در دسترس نیست', 'error');
        }
    }

    // Show notification for new messages in other chat rooms
    function showNewMessageNotification(message) {
        // Only show notification if user is not focused on the page
        if (!document.hasFocus()) {
            // Browser notification
            if ('Notification' in window && Notification.permission === 'granted') {
                new Notification('پیام جدید', {
                    body: `${message.senderName || 'کاربر'}: ${message.content ? message.content.substring(0, 50) : 'پیام جدید'}...`,
                    icon: '/assets/img/avatars/1.png',
                    tag: 'chat-notification'
                });
            }
            
            // Update page title
            const currentTitle = document.title;
            if (!currentTitle.includes('پیام جدید')) {
                document.title = 'پیام جدید - ' + currentTitle;
            }
        }
    }

    // Update user online status in contacts list
    function updateUserStatus(userId, isOnline) {
        if (!userId) {
            console.warn("No userId provided to updateUserStatus");
            return;
        }
        
        // Update in contacts list
        const contactItem = document.querySelector(`[data-user-id="${userId}"]`);
        if (contactItem) {
            const avatar = contactItem.querySelector('.avatar');
            const statusBadge = contactItem.querySelector('.badge');
            
            if (avatar) {
                avatar.className = `flex-shrink-0 avatar ${isOnline ? 'avatar-online' : 'avatar-offline'}`;
            }
            
            if (statusBadge) {
                statusBadge.className = `badge ${isOnline ? 'bg-success' : 'bg-secondary'} rounded-pill`;
                statusBadge.style.cssText = 'width: 8px; height: 8px;';
            }
        }
        
        // Update in chat rooms list if user is a participant
        const chatItems = document.querySelectorAll('[data-chat-id]');
        chatItems.forEach(chatItem => {
            const avatar = chatItem.querySelector('.avatar');
            if (avatar && (avatar.classList.contains('avatar-online') || avatar.classList.contains('avatar-offline'))) {
                // This is a single user chat, update status
                avatar.className = `flex-shrink-0 avatar ${isOnline ? 'avatar-online' : 'avatar-offline'}`;
            }
        });
        
        // Show status change notification
        if (window.chatManager) {
            const userName = contactItem?.querySelector('.chat-contact-name')?.textContent || 'کاربر';
            const statusText = isOnline ? 'آنلاین شد' : 'آفلاین شد';
            window.chatManager.showNotification(`${userName} ${statusText}`, isOnline ? 'success' : 'info');
        }
    }

    // Load chat rooms from controller
    async function loadChatRooms() {
        const chatList = document.getElementById('chat-list');
        if (!chatList) return;

        // First, check if there are already chat rooms loaded in the DOM
        const existingChatItems = chatList.querySelectorAll('.chat-contact-list-item[data-chat-id]');
        if (existingChatItems.length > 0) {
            console.log(`Found ${existingChatItems.length} existing chat rooms in DOM, using them`);
            
            // Convert DOM elements to chat room objects
            const chatRooms = Array.from(existingChatItems).map(item => {
                const chatId = item.getAttribute('data-chat-id');
                const nameElement = item.querySelector('.chat-contact-name');
                const statusElement = item.querySelector('.chat-contact-status');
                const timeElement = item.querySelector('small');
                const badgeElement = item.querySelector('.badge');
                const avatarElement = item.querySelector('.avatar img');
                
                return {
                    id: chatId,
                    name: nameElement ? nameElement.textContent : `چت ${chatId}`,
                    lastMessage: statusElement ? statusElement.textContent : 'هیچ پیامی وجود ندارد',
                    lastMessageDate: timeElement ? new Date() : new Date(), // We'll need to parse this properly
                    unreadCount: badgeElement ? parseInt(badgeElement.textContent) : 0,
                    participants: avatarElement ? [{
                        id: chatId,
                        fullName: nameElement ? nameElement.textContent : 'کاربر',
                        avatar: avatarElement.src,
                        isOnline: item.querySelector('.avatar').classList.contains('avatar-online')
                    }] : []
                };
            });
            
            console.log("Chat rooms extracted from DOM:", chatRooms);
            updateChatList(chatRooms);
            return;
        }

        // Show loading state
        showChatRoomsLoading(chatList);

        try {
            // Try to get chat rooms from the main Chat endpoint first
            const response = await fetch('/Chat', {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {
                const contentType = response.headers.get('content-type');
                if (contentType && contentType.includes('application/json')) {
                    const chatRooms = await response.json();
                    console.log("Chat rooms loaded from server:", chatRooms);
                    updateChatList(chatRooms);
                    return;
                } else {
                    console.warn("Chat endpoint returned non-JSON response, trying alternative methods");
                }
            }

            // If main endpoint fails, try alternative endpoints
            const alternativeEndpoints = [
                '/Chat/GetRooms',
                '/Chat/GetAllRooms',
                '/Chat/GetUserChats',
                '/Chat/GetUserRooms'
            ];

            for (const endpoint of alternativeEndpoints) {
                try {
                    console.log(`Trying alternative endpoint: ${endpoint}`);
                    const altResponse = await fetch(endpoint, {
                        method: 'GET',
                        headers: {
                            'Accept': 'application/json',
                            'X-Requested-With': 'XMLHttpRequest'
                        }
                    });

                    if (altResponse.ok) {
                        const contentType = altResponse.headers.get('content-type');
                        if (contentType && contentType.includes('application/json')) {
                            const chatRooms = await altResponse.json();
                            console.log(`Chat rooms loaded from ${endpoint}:`, chatRooms);
                            updateChatList(chatRooms);
                            return;
                        }
                    }
                } catch (error) {
                    console.warn(`Failed to load from ${endpoint}:`, error);
                }
            }

            // If all endpoints fail, show sample data for testing
            console.warn("All endpoints failed, showing sample data for testing");
            const sampleChatRooms = createSampleChatRooms();
            updateChatList(sampleChatRooms);

        } catch (error) {
            console.error("Error loading chat rooms: ", error);
            
            // Show sample data for testing
            const sampleChatRooms = createSampleChatRooms();
            updateChatList(sampleChatRooms);
        }
    }

    // Create sample chat rooms for testing
    function createSampleChatRooms() {
        return [
            {
                id: 1,
                name: "چت عمومی",
                lastMessage: "سلام، چطوری؟",
                lastMessageDate: new Date(Date.now() - 300000), // 5 minutes ago
                lastMessageSenderName: "احمد محمدی",
                unreadCount: 2,
                participants: [
                    {
                        id: 1,
                        fullName: "احمد محمدی",
                        avatar: "/assets/img/avatars/1.png",
                        isOnline: true
                    }
                ]
            },
            {
                id: 2,
                name: "گروه کاری",
                lastMessage: "جلسه فردا ساعت 10",
                lastMessageDate: new Date(Date.now() - 1800000), // 30 minutes ago
                lastMessageSenderName: "فاطمه احمدی",
                unreadCount: 0,
                participants: [
                    {
                        id: 2,
                        fullName: "فاطمه احمدی",
                        avatar: "/assets/img/avatars/2.png",
                        isOnline: false
                    }
                ]
            },
            {
                id: 3,
                name: "چت خصوصی",
                lastMessage: "ممنون از کمکت",
                lastMessageDate: new Date(Date.now() - 3600000), // 1 hour ago
                lastMessageSenderName: "علی رضایی",
                unreadCount: 1,
                participants: [
                    {
                        id: 3,
                        fullName: "علی رضایی",
                        avatar: "/assets/img/avatars/3.png",
                        isOnline: true
                    }
                ]
            }
        ];
    }

    // Show loading state for chat rooms
    function showChatRoomsLoading(chatList) {
        // Clear existing items
        const existingItems = chatList.querySelectorAll('.chat-contact-list-item:not(.chat-contact-list-item-title):not(.chat-list-item-0)');
        existingItems.forEach(item => item.remove());

        // Add loading indicator
        const loadingItem = document.createElement('li');
        loadingItem.className = 'chat-contact-list-item';
        loadingItem.innerHTML = `
            <div class="d-flex align-items-center justify-content-center py-3">
                <div class="spinner-border spinner-border-sm text-primary me-2" role="status">
                    <span class="visually-hidden">در حال بارگذاری...</span>
                </div>
                <span class="text-muted" lang="fa">در حال بارگذاری چت‌ها...</span>
            </div>
        `;
        chatList.appendChild(loadingItem);
    }

    // Show error state for chat rooms
    function showChatRoomsError(chatList, errorMessage) {
        // Clear existing items
        const existingItems = chatList.querySelectorAll('.chat-contact-list-item:not(.chat-contact-list-item-title):not(.chat-list-item-0)');
        existingItems.forEach(item => item.remove());

        // Add error message
        const errorItem = document.createElement('li');
        errorItem.className = 'chat-contact-list-item';
        errorItem.innerHTML = `
            <div class="d-flex align-items-center justify-content-center py-3">
                <i class="mdi mdi-alert-circle text-danger me-2"></i>
                <span class="text-danger" lang="fa">${errorMessage}</span>
            </div>
        `;
        chatList.appendChild(errorItem);
    }

    // Load contacts (real users) from controller
    async function loadContacts() {
        const contactList = document.getElementById('contact-list');
        if (!contactList) {
            console.error("Contact list element not found");
            return;
        }

        // Show loading state
        showContactsLoading(contactList);

        try {
            // Try multiple endpoints to get users from the server
            const endpoints = [
                '/Chat/GetAllUsers',
                '/Chat/GetUsers',
                '/Chat/GetContacts',
                '/Chat/GetUserList',
                '/User/GetAll',
                '/User/GetList'
            ];

            for (const endpoint of endpoints) {
                try {
                    console.log(`Trying to load contacts from: ${endpoint}`);
                    const response = await fetch(endpoint, {
                        method: 'GET',
                        headers: {
                            'Accept': 'application/json',
                            'X-Requested-With': 'XMLHttpRequest'
                        }
                    });
                    
                    if (response.ok) {
                        const contentType = response.headers.get('content-type');
                        if (contentType && contentType.includes('application/json')) {
                            const users = await response.json();
                            console.log(`Users loaded from ${endpoint}:`, users);
                            
                            // If users returned, show them
                            if (users && Array.isArray(users) && users.length > 0) {
                                updateContactsList(users);
                                return;
                            } else {
                                console.log(`No users returned from ${endpoint}, trying next endpoint`);
                            }
                        } else {
                            console.warn(`${endpoint} returned non-JSON response, trying next endpoint`);
                        }
                    } else {
                        console.warn(`${endpoint} failed with status ${response.status}, trying next endpoint`);
                    }
                } catch (error) {
                    console.warn(`Error loading from ${endpoint}:`, error);
                }
            }

            // If all endpoints fail, show sample data for testing
            console.warn("All endpoints failed, showing sample data for testing");
            updateContactsList(sampleUsers);

        } catch (error) {
            console.error("Error loading contacts: ", error);
            
            // Always show sample data for testing
            updateContactsList(sampleUsers);
        }
    }

   

    // Show loading state for contacts
    function showContactsLoading(contactList) {
        // Clear existing items
        const existingItems = contactList.querySelectorAll('.chat-contact-list-item:not(.chat-contact-list-item-title):not(.contact-list-item-0)');
        existingItems.forEach(item => item.remove());

        // Add loading indicator
        const loadingItem = document.createElement('li');
        loadingItem.className = 'chat-contact-list-item';
        loadingItem.innerHTML = `
            <div class="d-flex align-items-center justify-content-center py-3">
                <div class="spinner-border spinner-border-sm text-primary me-2" role="status">
                    <span class="visually-hidden">در حال بارگذاری...</span>
                </div>
                <span class="text-muted" lang="fa">در حال بارگذاری مخاطبین...</span>
            </div>
        `;
        contactList.appendChild(loadingItem);
    }

    // Show error state for contacts
    function showContactsError(contactList, errorMessage) {
        // Clear existing items
        const existingItems = contactList.querySelectorAll('.chat-contact-list-item:not(.chat-contact-list-item-title):not(.contact-list-item-0)');
        existingItems.forEach(item => item.remove());

        // Add error message
        const errorItem = document.createElement('li');
        errorItem.className = 'chat-contact-list-item';
        errorItem.innerHTML = `
            <div class="d-flex align-items-center justify-content-center py-3">
                <i class="mdi mdi-alert-circle text-danger me-2"></i>
                <span class="text-danger" lang="fa">${errorMessage}</span>
            </div>
        `;
        contactList.appendChild(errorItem);
    }

    // Update contacts list with real user data
    function updateContactsList(users) {
        const contactList = document.getElementById('contact-list');
        if (!contactList) {
            console.error("Contact list element not found in updateContactsList");
            return;
        }

        // Clear existing contact items (keep title and no-results)
        const existingItems = contactList.querySelectorAll('.chat-contact-list-item:not(.chat-contact-list-item-title):not(.contact-list-item-0)');
        existingItems.forEach(item => item.remove());

        if (users && Array.isArray(users) && users.length > 0) {
            users.forEach((user, index) => {
                const contactItem = createContactListItem(user);
                if (contactItem) {
                    contactList.appendChild(contactItem);
                }
            });
            
            // Hide no results message
            const noResultsItem = contactList.querySelector('.contact-list-item-0');
            if (noResultsItem) {
                noResultsItem.classList.add('d-none');
            }
        } else {
            // Show no results message
            const noResultsItem = contactList.querySelector('.contact-list-item-0');
            if (noResultsItem) {
                noResultsItem.classList.remove('d-none');
            }
        }
        
        // Refresh scrollbars after updating content
        setTimeout(() => {
            refreshScrollbars();
        }, 100);
    }

    // Create contact list item for real users
    function createContactListItem(user) {
        if (!user) {
            console.error("User object is null or undefined");
            return null;
        }

        const li = document.createElement('li');
        li.className = 'chat-contact-list-item';
        
        // Handle different user ID field names
        const userId = user.userId || user.id || user.UserId || user.Id;
        if (!userId) {
            console.error("User ID not found in user object:", user);
            return null;
        }
        
        li.setAttribute('data-user-id', userId);

        // Handle different avatar field names and use fallback if not found
        let avatarSrc = user.avatar || user.avatarUrl || user.Avatar || user.AvatarUrl;
        
        // If avatar path is relative and doesn't start with /, make it absolute
        if (avatarSrc && !avatarSrc.startsWith('http') && !avatarSrc.startsWith('/')) {
            avatarSrc = '/' + avatarSrc;
        }
        
        // Use fallback avatar if no avatar or if it's a problematic path
        if (!avatarSrc || avatarSrc.includes('avatar_') && avatarSrc.includes('_14040531141429.jpg')) {
            // Generate a fallback avatar based on user name
            const fallbackIndex = (userId.charCodeAt(0) % 8) + 1; // Use 8 different fallback avatars
            avatarSrc = `/assets/img/avatars/${fallbackIndex}.png`;
        }
        
        // Handle different name field names
        const fullName = user.fullName || user.name || user.FullName || user.Name || user.userName || user.UserName || 'کاربر ناشناس';
        const userName = user.userName || user.UserName || user.user_name || user.User_Name || 'کاربر';
        
        // Handle online status
        const isOnline = user.isOnline !== undefined ? user.isOnline : (user.isOnline === true);
        const statusClass = isOnline ? 'avatar-online' : 'avatar-offline';

        li.innerHTML = `
            <a class="d-flex align-items-center" href="javascript:void(0);">
                <div class="flex-shrink-0 avatar ${statusClass}">
                    <img src="${avatarSrc}" alt="Avatar" class="rounded-circle" onerror="this.src='/assets/img/avatars/1.png'">
                </div>
                <div class="chat-contact-info flex-grow-1 ms-3">
                    <h6 class="chat-contact-name text-truncate m-0" lang="fa">${fullName}</h6>
                    <p class="chat-contact-status text-truncate mb-0" lang="fa">${userName}</p>
                </div>
                <div class="d-flex align-items-center">
                    <span class="badge ${isOnline ? 'bg-success' : 'bg-secondary'} rounded-pill" style="width: 8px; height: 8px;"></span>
                </div>
            </a>
        `;

        // Add click event to start chat with user
        li.addEventListener('click', () => startChatWithUser(user));
        
        return li;
    }

    // Find existing chat room with a specific user
    async function findExistingChatRoomWithUser(userId) {
        try {
            // First, check in the current chat rooms list (DOM) - this is the fastest method
            const chatList = document.getElementById('chat-list');
            if (chatList) {
                const existingChatItems = chatList.querySelectorAll('.chat-contact-list-item[data-chat-id]');
                
                for (const chatItem of existingChatItems) {
                    const chatId = chatItem.getAttribute('data-chat-id');
                    const nameElement = chatItem.querySelector('.chat-contact-name');
                    const chatName = nameElement ? nameElement.textContent : '';
                    
                    // Check if this is a single user chat (not a group)
                    // Single user chats usually have the user's name as the chat name
                    if (chatName && !chatName.includes('گروه') && !chatName.includes('Group')) {
                        // Try to get chat room details to check participants
                        try {
                            const response = await fetch(`/Chat/GetChatRoom/${chatId}`, {
                                method: 'GET',
                                headers: {
                                    'Accept': 'application/json',
                                    'X-Requested-With': 'XMLHttpRequest'
                                }
                            });
                            
                            if (response.ok) {
                                const result = await response.json();
                                if (result.success && result.data && result.data.participants) {
                                    const participants = result.data.participants;
                                    // Check if this user is a participant
                                    if (participants.some(p => 
                                        (p.id && p.id.toString() === userId.toString()) ||
                                        (p.userId && p.userId.toString() === userId.toString()) ||
                                        (p.UserId && p.UserId.toString() === userId.toString())
                                    )) {
                                        console.log(`Found existing chat room ${chatId} with user ${userId}`);
                                        return {
                                            id: chatId,
                                            name: chatName,
                                            participants: participants
                                        };
                                    }
                                }
                            }
                        } catch (error) {
                            console.warn(`Error checking chat room ${chatId}:`, error);
                        }
                    }
                }
            }
            
            // If not found in DOM, try to search on server with multiple endpoints
            const searchEndpoints = [
                `/Chat/FindChatRoomWithUser/${userId}`,
                `/Chat/GetUserChatRoom/${userId}`,
                `/Chat/SearchChatRoom?userId=${userId}`,
                `/Chat/GetChatRoomsByUser/${userId}`,
                `/Chat/GetUserChats/${userId}`
            ];
            
            for (const endpoint of searchEndpoints) {
                try {
                    console.log(`Searching for existing chat room at: ${endpoint}`);
                    const response = await fetch(endpoint, {
                        method: 'GET',
                        headers: {
                            'Accept': 'application/json',
                            'X-Requested-With': 'XMLHttpRequest'
                        }
                    });
                    
                    if (response.ok) {
                        const result = await response.json();
                        if (result.success && result.data) {
                            console.log(`Found existing chat room via ${endpoint}:`, result.data);
                            return result.data;
                        } else if (Array.isArray(result) && result.length > 0) {
                            // Return the first chat room found
                            console.log(`Found existing chat room via ${endpoint}:`, result[0]);
                            return result[0];
                        }
                    }
                } catch (error) {
                    console.warn(`Error searching at ${endpoint}:`, error);
                }
            }
            
            // If still not found, try to get all user's chat rooms
            try {
                const response = await fetch('/Chat/GetUserChatRooms', {
                    method: 'GET',
                    headers: {
                        'Accept': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });
                
                if (response.ok) {
                    const result = await response.json();
                    if (result.success && result.data && Array.isArray(result.data)) {
                        // Find chat room with this user
                        for (const chatRoom of result.data) {
                            if (chatRoom.participants && Array.isArray(chatRoom.participants)) {
                                if (chatRoom.participants.some(p => 
                                    (p.id && p.id.toString() === userId.toString()) ||
                                    (p.userId && p.userId.toString() === userId.toString()) ||
                                    (p.UserId && p.UserId.toString() === userId.toString())
                                )) {
                                    console.log(`Found existing chat room via GetUserChatRooms:`, chatRoom);
                                    return chatRoom;
                                }
                            }
                        }
                    }
                }
            } catch (error) {
                console.warn("Error getting user chat rooms:", error);
            }
            
            console.log(`No existing chat room found for user ${userId}`);
            return null; // No existing chat room found
        } catch (error) {
            console.error("Error finding existing chat room:", error);
            return null;
        }
    }

    // Start chat with a specific user
    async function startChatWithUser(user) {
        if (!user) {
            console.error("User object is null or undefined");
            return;
        }

        // Handle different user ID field names
        const userId = user.userId || user.id || user.UserId || user.Id;
        if (!userId) {
            console.error("User ID not found in user object:", user);
            if (window.chatManager) {
                window.chatManager.showNotification('خطا: شناسه کاربر یافت نشد', 'error');
            }
            return;
        }

        try {
            // First, check if a chat room already exists with this user
            const existingChatRoom = await findExistingChatRoomWithUser(userId);
            
            if (existingChatRoom) {
                console.log(`Found existing chat room with user ${userId}:`, existingChatRoom);
                
                // Highlight the existing chat room in the list
                const existingChatItem = document.querySelector(`[data-chat-id="${existingChatRoom.id}"]`);
                if (existingChatItem) {
                    // Remove active class from all chat items
                    document.querySelectorAll('.chat-contact-list-item').forEach(item => {
                        item.classList.remove('active');
                    });
                    // Add active class to the found chat item
                    existingChatItem.classList.add('active');
                }
                
                // Load the existing chat room
                await loadChatRoom(existingChatRoom.id);
                
                // Show notification
                const userName = user.fullName || user.name || user.userName || 'کاربر';
                if (window.chatManager) {
                    window.chatManager.showNotification(`چت با ${userName} باز شد`, 'info');
                }
                return;
            }

            // If no existing chat room, create a new one
            console.log(`No existing chat room found, creating new one for user ${userId}`);
            const participantIds = [userId];
            const response = await fetch('/Chat/CreateChatRoom', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(participantIds)
            });

            if (response.ok) {
                const result = await response.json();
                
                if (result.success) {
                    // Load the newly created chat room
                    await loadChatRoom(result.chatRoomId);
                    
                    // Update chat list to show the new chat
                    await loadChatRooms();
                    
                    // Show success notification
                    const userName = user.fullName || user.name || user.userName || 'کاربر';
                    if (window.chatManager) {
                        window.chatManager.showNotification(`چت جدید با ${userName} شروع شد`, 'success');
                    }
                } else {
                    throw new Error(result.message || 'خطا در ایجاد چت‌روم');
                }
            } else {
                const errorText = await response.text();
                console.error("Failed to create chat room:", response.status, errorText);
                throw new Error(`خطا در ایجاد چت‌روم: ${response.status}`);
            }
        } catch (error) {
            console.error("Error starting chat with user: ", error);
            if (window.chatManager) {
                window.chatManager.showNotification(`خطا در شروع چت: ${error.message}`, 'error');
            }
        }
    }

    // Update chat list with real data
    function updateChatList(chatRooms) {
        const chatList = document.getElementById('chat-list');
        if (!chatList) return;

        // If we already have chat rooms in the DOM and no new data, don't clear them
        const existingChatItems = chatList.querySelectorAll('.chat-contact-list-item[data-chat-id]');
        if (existingChatItems.length > 0 && (!chatRooms || chatRooms.length === 0)) {
            console.log("Keeping existing chat rooms in DOM");
            return;
        }

        // Clear existing chat items (keep title and no-results)
        const existingItems = chatList.querySelectorAll('.chat-contact-list-item:not(.chat-contact-list-item-title):not(.chat-list-item-0)');
        existingItems.forEach(item => item.remove());

        if (chatRooms && chatRooms.length > 0) {
            console.log(`Updating chat list with ${chatRooms.length} chat rooms`);
            chatRooms.forEach(room => {
                const chatItem = createChatListItem(room);
                chatList.appendChild(chatItem);
            });
            
            // Update total unread count in header
            updateTotalUnreadCount(chatRooms);
        } else {
            // Show no results message
            const noResultsItem = chatList.querySelector('.chat-list-item-0');
            if (noResultsItem) {
                noResultsItem.classList.remove('d-none');
            }
            
            // Clear unread count
            updateTotalUnreadCount([]);
        }
        
        // Refresh scrollbars after updating content
        setTimeout(() => {
            refreshScrollbars();
        }, 100);
    }

    // Update total unread message count in header
    function updateTotalUnreadCount(chatRooms) {
        if (!chatRooms || !Array.isArray(chatRooms)) {
            chatRooms = [];
        }
        
        const totalUnread = chatRooms.reduce((total, room) => total + (room.unreadCount || 0), 0);
        
        // Update header badge if exists
        const headerBadge = document.querySelector('.chat-header .badge');
        if (headerBadge) {
            if (totalUnread > 0) {
                headerBadge.textContent = totalUnread > 99 ? '99+' : totalUnread.toString();
                headerBadge.classList.remove('d-none');
            } else {
                headerBadge.classList.add('d-none');
            }
        }
        
        // Update page title with unread count
        if (totalUnread > 0) {
            document.title = `(${totalUnread}) چت - TPL`;
        } else {
            document.title = 'چت - TPL';
        }
        
        // Update favicon or show browser notification for new messages
        if (totalUnread > 0 && 'Notification' in window && Notification.permission === 'granted') {
            // Only show notification if user is not focused on the page
            if (!document.hasFocus()) {
                new Notification('پیام جدید', {
                    body: `${totalUnread} پیام نخوانده دارید`,
                    icon: '/assets/img/avatars/1.png',
                    tag: 'chat-notification'
                });
            }
        }
    }

    // Create chat list item
    function createChatListItem(room) {
        const li = document.createElement('li');
        li.className = 'chat-contact-list-item';
        li.setAttribute('data-chat-id', room.id);

        // Handle different date formats and field names
        let lastMessageTime = '';
        if (room.lastMessageDate) {
            if (typeof room.lastMessageDate === 'string') {
                lastMessageTime = formatTime(room.lastMessageDate);
            } else if (room.lastMessageDate instanceof Date) {
                lastMessageTime = formatTime(room.lastMessageDate);
            } else {
                lastMessageTime = formatTime(new Date(room.lastMessageDate));
            }
        }

        const unreadBadge = room.unreadCount > 0 ? `<span class="badge bg-primary rounded-pill ms-auto">${room.unreadCount}</span>` : '';
        
        // Handle different field names for last message sender
        const lastMessageSender = room.lastMessageSenderName || room.lastMessageSender || room.senderName || '';
        const lastMessageContent = room.lastMessage || room.lastMessageText || 'هیچ پیامی وجود ندارد';
        const fullLastMessage = lastMessageSender ? `${lastMessageSender}: ${lastMessageContent}` : lastMessageContent;

        // Handle different field names for room name
        const roomName = room.name || room.chatRoomName || room.title || `چت ${room.id}`;

        // Handle participants data
        const participants = room.participants || room.Participants || [];
        const hasOnlineParticipants = Array.isArray(participants) && participants.some(p => p.isOnline || p.IsOnline);
        const isSingleUserChat = Array.isArray(participants) && participants.length === 1;

        li.innerHTML = `
            <a class="d-flex align-items-center" href="javascript:void(0);">
                <div class="flex-shrink-0 avatar ${hasOnlineParticipants ? 'avatar-online' : 'avatar-offline'}">
                    ${isSingleUserChat ? 
                        `<img src="${participants[0].avatar || participants[0].Avatar || '/assets/img/avatars/1.png'}" alt="Avatar" class="rounded-circle" onerror="this.src='/assets/img/avatars/1.png'">` :
                        `<span class="avatar-initial rounded-circle bg-label-primary">${roomName ? roomName.charAt(0) : 'گ'}</span>`
                    }
                </div>
                <div class="chat-contact-info flex-grow-1 ms-3">
                    <h6 class="chat-contact-name text-truncate m-0">${roomName}</h6>
                    <p class="chat-contact-status text-truncate mb-0">
                        ${fullLastMessage}
                    </p>
                </div>
                <div class="d-flex flex-column align-items-end position-relative">
                    <small class="text-muted mb-1">${lastMessageTime}</small>
                    ${unreadBadge}
                    <button class="btn btn-sm btn-outline-danger delete-chat-btn" 
                            title="حذف چت" 
                            data-chat-id="${room.id}"
                            style="opacity: 0; transition: all 0.3s ease; position: absolute; top: -8px; right: -8px; width: 28px; height: 28px; padding: 0; border-radius: 50%; border: 2px solid #dc3545; background: white; color: #dc3545; font-size: 12px; z-index: 10; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <i class="mdi mdi-close mdi-16px"></i>
                    </button>
                </div>
            </a>
        `;

        // Add click event to load chat room
        li.addEventListener('click', (e) => {
            // Don't load chat room if delete button was clicked
            if (e.target.closest('.delete-chat-btn')) {
                return;
            }
            loadChatRoom(room.id);
        });

        // Add hover effects for delete button
        li.addEventListener('mouseenter', () => {
            const deleteBtn = li.querySelector('.delete-chat-btn');
            if (deleteBtn) {
                deleteBtn.style.opacity = '1';
                deleteBtn.style.transform = 'scale(1.1)';
                deleteBtn.style.background = '#dc3545';
                deleteBtn.style.color = 'white';
                deleteBtn.style.boxShadow = '0 4px 8px rgba(220,53,69,0.3)';
            }
        });

        li.addEventListener('mouseleave', () => {
            const deleteBtn = li.querySelector('.delete-chat-btn');
            if (deleteBtn) {
                deleteBtn.style.opacity = '0';
                deleteBtn.style.transform = 'scale(1)';
                deleteBtn.style.background = 'white';
                deleteBtn.style.color = '#dc3545';
                deleteBtn.style.boxShadow = '0 2px 4px rgba(0,0,0,0.1)';
            }
        });

        // Add delete button click event
        const deleteBtn = li.querySelector('.delete-chat-btn');
        if (deleteBtn) {
            deleteBtn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                deleteChatRoom(room.id, roomName);
            });
        }

        return li;
    }

    // Load chat room and messages
    async function loadChatRoom(chatRoomId) {
        try {
            currentChatRoomId = chatRoomId;
            
            // Show loading state
            showStatusMessage('loading');
            
            // Update active state
            document.querySelectorAll('.chat-contact-list-item').forEach(item => {
                item.classList.remove('active');
            });
            
            // Find the chat item and add active class if it exists
            const chatItem = document.querySelector(`[data-chat-id="${chatRoomId}"]`);
            if (chatItem) {
                chatItem.classList.add('active');
            } else {
                console.warn(`Chat item with ID ${chatRoomId} not found in DOM`);
            }

            // Try to mark messages as read (don't fail if this doesn't work)
            try {
                await markMessagesAsRead(chatRoomId);
            } catch (error) {
                console.warn("Failed to mark messages as read:", error);
            }
            
            // Load chat room data from server using the correct endpoint
            try {
                console.log(`Loading chat room ${chatRoomId} from server...`);
                const response = await fetch(`/Chat/Room/${chatRoomId}`, {
                    method: 'GET',
                    headers: {
                        'Accept': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });
                
                if (response.ok) {
                    const result = await response.json();
                    console.log(`Chat room data loaded successfully:`, result);
                    
                    if (result.success && result.data) {
                        // Transform the data to match our display format
                        const chatData = {
                            chatRoomName: result.data.chatRoomName || `چت ${chatRoomId}`,
                            isGroup: result.data.isGroup || false,
                            messages: result.data.messages || [],
                            participants: result.data.participants || []
                        };
                        
                        // Update header with participant info
                        if (chatData.participants && chatData.participants.length > 0) {
                            const otherParticipant = chatData.participants.find(p => p.userId !== currentUser?.id);
                            if (otherParticipant) {
                                chatData.chatRoomName = otherParticipant.fullName || otherParticipant.userName || chatData.chatRoomName;
                            }
                        }
                        
                        displayChatRoom(chatData);
                        joinChatRoom(chatRoomId);
                        
                        // Refresh scrollbars after loading chat room
                        setTimeout(() => {
                            refreshScrollbars();
                        }, 100);
                        
                        return;
                    } else {
                        throw new Error(result.message || 'خطا در بارگذاری چت‌روم');
                    }
                } else {
                    const errorText = await response.text();
                    console.error(`Failed to load chat room: ${response.status} - ${errorText}`);
                    throw new Error(`خطا در بارگذاری چت‌روم: ${response.status}`);
                }
            } catch (error) {
                console.error("Error loading chat room from server: ", error);
                
                // Show error message
                showStatusMessage('error');
                
                // Try to show any existing messages from DOM if available
                const existingMessages = document.querySelectorAll('.chat-message');
                if (existingMessages.length > 0) {
                    // Messages already exist, just update header
                    const chatItem = document.querySelector(`[data-chat-id="${chatRoomId}"]`);
                    if (chatItem) {
                        const chatName = chatItem.querySelector('.chat-contact-name')?.textContent || `چت ${chatRoomId}`;
                        updateChatHeader(chatName, false);
                    }
                    joinChatRoom(chatRoomId);
                } else {
                    // No messages, show default welcome
                    const defaultChatData = {
                        chatRoomName: `چت ${chatRoomId}`,
                        isGroup: false,
                        messages: [
                            {
                                id: 'welcome',
                                content: 'خطا در بارگذاری پیام‌ها. لطفاً دوباره تلاش کنید.',
                                sentDate: new Date(),
                                isOwnMessage: false,
                                senderName: 'سیستم',
                                senderAvatar: '/assets/img/avatars/1.png'
                            }
                        ]
                    };
                    
                    displayChatRoom(defaultChatData);
                    joinChatRoom(chatRoomId);
                }
            }
            
        } catch (error) {
            console.error("Error loading chat room: ", error);
            showStatusMessage('error');
        }
    }

    // Mark messages as read for a specific chat room
    async function markMessagesAsRead(chatRoomId) {
        try {
            // Try multiple endpoints to mark messages as read
            const endpoints = [
                '/Chat/MarkMessagesAsRead',
                '/Chat/MarkAsRead',
                '/Chat/UpdateMessageStatus',
                '/Chat/SetMessagesRead'
            ];

            for (const endpoint of endpoints) {
                try {
                    console.log(`Trying to mark messages as read from: ${endpoint}`);
                    const response = await fetch(endpoint, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                        },
                        body: JSON.stringify(chatRoomId)
                    });

                    if (response.ok) {
                        console.log(`Messages marked as read successfully from ${endpoint}`);
                        
                        // Update unread count in chat list
                        const chatItem = document.querySelector(`[data-chat-id="${chatRoomId}"]`);
                        if (chatItem) {
                            const unreadBadge = chatItem.querySelector('.badge');
                            if (unreadBadge) {
                                unreadBadge.remove();
                            }
                        }
                        
                        // Refresh chat rooms to update unread counts
                        await loadChatRooms();
                        return; // Success, exit function
                    }
                } catch (error) {
                    console.warn(`Failed to mark messages as read from ${endpoint}:`, error);
                }
            }
            
            console.warn("All endpoints failed for marking messages as read");
        } catch (error) {
            console.error("Error marking messages as read: ", error);
        }
    }

    // Display chat room
    function displayChatRoom(chatData) {
        if (!chatData) {
            console.warn("No chat data provided to displayChatRoom");
            return;
        }

        // Update header
        updateChatHeader(chatData.chatRoomName || 'چت جدید', chatData.isGroup || false);
        
        // Update header avatar if available
        if (chatHeaderAvatar && chatData.participants && chatData.participants.length > 0) {
            const firstParticipant = chatData.participants[0];
            if (firstParticipant.avatar) {
                chatHeaderAvatar.src = firstParticipant.avatar;
            }
        }

        // Enable message input area and send button
        enableMessageInput();

        // Hide all status messages first
        hideAllStatusMessages();

        // Clear and load messages
        if (chatHistory) {
            // Clear existing messages
            const existingMessages = chatHistory.querySelectorAll('.chat-message');
            existingMessages.forEach(msg => msg.remove());
            
            if (chatData.messages && Array.isArray(chatData.messages) && chatData.messages.length > 0) {
                console.log(`Displaying ${chatData.messages.length} messages:`, chatData.messages);
                chatData.messages.forEach((message, index) => {
                    // Add a small delay between messages for better visual effect
                    setTimeout(() => {
                        displayMessage(message);
                    }, index * 100);
                });
            } else {
                // Show no messages indicator
                showStatusMessage('no-messages');
            }
        }

        scrollToBottom();
        
        // Add a small delay to ensure all messages are rendered before scrolling
        setTimeout(() => {
            scrollToBottom();
            refreshScrollbars();
        }, 500);
    }

    // Update chat header
    function updateChatHeader(chatName, isGroup) {
        if (chatHeaderName) chatHeaderName.textContent = chatName;
        if (chatHeaderStatus) chatHeaderStatus.textContent = isGroup ? 'گروه' : 'آنلاین';
        
        // Update right sidebar
        const rightSidebarName = document.querySelector('.app-chat-sidebar-right h5');
        const rightSidebarRole = document.querySelector('.app-chat-sidebar-right span');
        if (rightSidebarName) rightSidebarName.textContent = chatName;
        if (rightSidebarRole) rightSidebarRole.textContent = isGroup ? 'گروه' : 'کاربر';
    }

    // Display message
    function displayMessage(message) {
        if (!chatHistory) return;

        const messageElement = createMessageElement(message);
        chatHistory.appendChild(messageElement);
        
        // Refresh scrollbars after adding message
        setTimeout(() => {
            refreshScrollbars();
        }, 50);
    }

    // Create message element
    function createMessageElement(message) {
        const li = document.createElement('li');
        
        // Determine if this is the current user's message
        const isCurrentUser = message.isOwnMessage || 
                            (currentUser && message.senderName === currentUser.userName) ||
                            (currentUser && message.senderId === currentUser.id) ||
                            (currentUser && message.senderId === currentUser.id);
        
        li.className = `chat-message ${isCurrentUser ? 'chat-message-right' : 'chat-message-left'}`;
        li.setAttribute('data-message-id', message.id);

        const messageTime = formatTime(message.sentDate);
        const messageContent = message.content || '';
        
        // Handle avatar path - convert relative paths to absolute
        let avatarSrc = message.senderAvatar || '/assets/img/avatars/1.png';
        if (avatarSrc && !avatarSrc.startsWith('http') && !avatarSrc.startsWith('/')) {
            avatarSrc = '/' + avatarSrc;
        }
        if (!avatarSrc || avatarSrc.includes('avatar_') && avatarSrc.includes('_14040531141429.jpg')) {
            // Use fallback avatar
            avatarSrc = '/assets/img/avatars/1.png';
        }
        
        const senderName = message.senderName || 'کاربر';

        if (isCurrentUser) {
            // Current user's message (right side)
            li.innerHTML = `
                <div class="d-flex overflow-hidden justify-content-end">
                    <div class="chat-message-wrapper flex-grow-1 me-3" style="max-width: 70%;">
                        <div class="chat-message-text bg-primary text-white p-3 rounded">
                            <p class="mb-0">${messageContent}</p>
                        </div>
                        <div class="text-end text-muted mt-1">
                            <i class="mdi mdi-check-all mdi-14px text-success me-1"></i>
                            <small>${messageTime}</small>
                        </div>
                    </div>
                    <div class="user-avatar flex-shrink-0">
                        <div class="avatar avatar-sm">
                            <img src="${avatarSrc}" alt="Avatar" class="rounded-circle" onerror="this.src='/assets/img/avatars/1.png'">
                        </div>
                    </div>
                </div>
            `;
        } else {
            // Other user's message (left side)
            li.innerHTML = `
                <div class="d-flex overflow-hidden">
                    <div class="user-avatar flex-shrink-0 me-3">
                        <div class="avatar avatar-sm">
                            <img src="${avatarSrc}" alt="Avatar" class="rounded-circle" onerror="this.src='/assets/img/avatars/1.png'">
                        </div>
                    </div>
                    <div class="chat-message-wrapper flex-grow-1" style="max-width: 70%;">
                        <div class="chat-message-sender mb-1">
                            <small class="text-primary fw-bold">${senderName}</small>
                        </div>
                        <div class="chat-message-text bg-light p-3 rounded">
                            <p class="mb-0">${messageContent}</p>
                        </div>
                        <div class="text-muted mt-1">
                            <small>${messageTime}</small>
                        </div>
                    </div>
                </div>
            `;
        }

        return li;
    }

    // Status message management functions
    function hideAllStatusMessages() {
        if (welcomeMessage) welcomeMessage.classList.add('d-none');
        if (loadingMessages) loadingMessages.classList.add('d-none');
        if (noMessages) noMessages.classList.add('d-none');
        if (errorMessages) errorMessages.classList.add('d-none');
    }

    function showStatusMessage(messageType) {
        hideAllStatusMessages();
        
        switch (messageType) {
            case 'welcome':
                if (welcomeMessage) welcomeMessage.classList.remove('d-none');
                break;
            case 'loading':
                if (loadingMessages) loadingMessages.classList.remove('d-none');
                break;
            case 'no-messages':
                if (noMessages) noMessages.classList.remove('d-none');
                break;
            case 'error':
                if (errorMessages) errorMessages.classList.remove('d-none');
                break;
        }
    }

    function showTypingIndicator(userName) {
        if (typingIndicator) {
            typingIndicator.classList.remove('d-none');
            const typingText = typingIndicator.querySelector('small');
            if (typingText) {
                typingText.textContent = `${userName || 'کاربر'} در حال تایپ...`;
            }
        }
    }

    function hideTypingIndicator() {
        if (typingIndicator) {
            typingIndicator.classList.add('d-none');
        }
    }

    function showUploadProgress(fileName) {
        if (uploadProgress) {
            uploadProgress.classList.remove('d-none');
            const progressText = uploadProgress.querySelector('small');
            if (progressText) {
                progressText.textContent = `در حال آپلود ${fileName}...`;
            }
            if (progressBar) {
                progressBar.style.width = '0%';
            }
        }
    }

    function hideUploadProgress() {
        if (uploadProgress) {
            uploadProgress.classList.add('d-none');
        }
    }

    function updateUploadProgress(percent) {
        if (progressBar) {
            progressBar.style.width = `${percent}%`;
        }
    }

    // Delete chat room
    async function deleteChatRoom(chatRoomId, chatName) {
        // Use SweetAlert for confirmation if available, otherwise fallback to confirm
        let confirmed = false;
        
        if (typeof Swal !== 'undefined') {
            const result = await Swal.fire({
                title: 'حذف چت',
                text: `آیا مطمئن هستید که می‌خواهید چت "${chatName}" را حذف کنید؟`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'بله، حذف کن',
                cancelButtonText: 'انصراف'
            });
            confirmed = result.isConfirmed;
        } else {
            confirmed = confirm(`آیا مطمئن هستید که می‌خواهید چت "${chatName}" را حذف کنید؟`);
        }
        
        if (!confirmed) {
            return;
        }

        try {
            // Show loading state
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'در حال حذف...',
                    text: 'لطفاً صبر کنید',
                    allowOutsideClick: false,
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });
            }

            // Try multiple endpoints to delete chat room from server
            const endpoints = [
                '/Chat/DeleteChatRoom',
                '/Chat/DeleteRoom',
                '/Chat/RemoveChatRoom',
                '/Chat/RemoveRoom'
            ];

            let deleted = false;
            for (const endpoint of endpoints) {
                try {
                    console.log(`Trying to delete chat room from: ${endpoint}`);
                    const response = await fetch(endpoint, {
                        method: 'POST', // Changed from DELETE to POST for better compatibility
                        headers: {
                            'Content-Type': 'application/json',
                        },
                        body: JSON.stringify(chatRoomId) // Send just the ID, not wrapped in object
                    });

                    if (response.ok) {
                        const result = await response.json();
                        if (result.success || result.message) {
                            console.log(`Chat room deleted successfully from ${endpoint}`);
                            deleted = true;
                            break;
                        }
                    }
                } catch (error) {
                    console.warn(`Failed to delete from ${endpoint}:`, error);
                }
            }

            // Remove chat item from DOM regardless of server response
            const chatItem = document.querySelector(`[data-chat-id="${chatRoomId}"]`);
            if (chatItem) {
                chatItem.remove();
            }

            // If this was the current chat room, clear it
            if (currentChatRoomId === chatRoomId) {
                currentChatRoomId = null;
                disableMessageInput();
                
                // Clear chat history
                const chatHistory = document.querySelector('.chat-history');
                if (chatHistory) {
                    chatHistory.innerHTML = '';
                }
                
                // Update header
                const headerName = document.querySelector('.chat-history-header h6');
                const headerStatus = document.querySelector('.chat-history-header .user-status');
                if (headerName) headerName.textContent = 'چت جدید';
                if (headerStatus) headerStatus.textContent = 'آنلاین';
                
                // Update right sidebar
                const rightSidebarName = document.querySelector('.app-chat-sidebar-right h5');
                const rightSidebarRole = document.querySelector('.app-chat-sidebar-right span');
                if (rightSidebarName) rightSidebarName.textContent = 'چت جدید';
                if (rightSidebarRole) rightSidebarRole.textContent = 'کاربر';
            }

            // Close loading and show success notification
            if (typeof Swal !== 'undefined') {
                Swal.close();
                
                if (deleted) {
                    Swal.fire({
                        title: 'موفقیت!',
                        text: `چت "${chatName}" با موفقیت حذف شد`,
                        icon: 'success',
                        timer: 2000,
                        showConfirmButton: false
                    });
                } else {
                    Swal.fire({
                        title: 'هشدار',
                        text: `چت "${chatName}" حذف شد (فقط از رابط کاربری)`,
                        icon: 'warning',
                        timer: 3000,
                        showConfirmButton: false
                    });
                }
            } else {
                // Fallback notifications
                if (window.chatManager) {
                    if (deleted) {
                        window.chatManager.showNotification(`چت "${chatName}" با موفقیت حذف شد`, 'success');
                    } else {
                        window.chatManager.showNotification(`چت "${chatName}" حذف شد (فقط از رابط کاربری)`, 'warning');
                    }
                }
            }

            // Refresh scrollbars
            setTimeout(() => {
                refreshScrollbars();
            }, 100);

        } catch (error) {
            console.error("Error deleting chat room: ", error);
            
            // Close loading and show error
            if (typeof Swal !== 'undefined') {
                Swal.close();
                Swal.fire({
                    title: 'خطا!',
                    text: `خطا در حذف چت: ${error.message}`,
                    icon: 'error'
                });
            } else {
                if (window.chatManager) {
                    window.chatManager.showNotification(`خطا در حذف چت: ${error.message}`, 'error');
                }
            }

            // If all attempts fail, just remove from DOM
            const chatItem = document.querySelector(`[data-chat-id="${chatRoomId}"]`);
            if (chatItem) {
                chatItem.remove();
            }

            // Refresh scrollbars
            setTimeout(() => {
                refreshScrollbars();
            }, 100);
        }
    }

    // Join chat room via SignalR
    function joinChatRoom(chatRoomId) {
        if (chatConnection && chatConnection.state === "Connected") {
            chatConnection.invoke("JoinChatRoom", chatRoomId.toString());
        }
    }

    // Send message
    async function sendMessage(content, messageType = 0) {
        if (!currentChatRoomId || !content.trim()) {
            if (window.chatManager) {
                window.chatManager.showNotification('ابتدا یک چت را انتخاب کنید', 'warning');
            }
            return;
        }

        try {
            // Create optimistic message for immediate display
            const optimisticMessage = {
                id: 'temp_' + Date.now(),
                content: content.trim(),
                sentDate: new Date(),
                isOwnMessage: true,
                senderName: currentUser?.userName || 'شما',
                senderAvatar: currentUser?.avatar || '/assets/img/avatars/1.png',
                isOptimistic: true
            };

            // Display optimistic message immediately
            displayMessage(optimisticMessage);
            scrollToBottom();

            // Clear input immediately for better UX
            messageInput.value = '';

            const messageData = {
                content: content.trim(),
                chatRoomId: currentChatRoomId,
                messageType: messageType,
                replyToMessageId: null
            };

            // Try multiple endpoints for sending message
            const endpoints = [
                '/Chat/SendMessage',
                '/Chat/Send',
                '/Chat/PostMessage',
                '/Chat/CreateMessage'
            ];

            let messageSent = false;
            let serverMessage = null;

            for (const endpoint of endpoints) {
                try {
                    console.log(`Trying to send message to: ${endpoint}`);
                    const response = await fetch(endpoint, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                        },
                        body: JSON.stringify(messageData)
                    });

                    if (response.ok) {
                        const result = await response.json();
                        if (result.success) {
                            console.log(`Message sent successfully to ${endpoint}:`, result);
                            messageSent = true;
                            serverMessage = result.data || result;
                            
                            // Update optimistic message with server data
                            updateOptimisticMessage(optimisticMessage.id, serverMessage);
                            
                            // If SignalR is available, send via hub
                            if (chatConnection && chatConnection.state === "Connected") {
                                chatConnection.invoke("SendMessageToGroup", currentChatRoomId.toString(), serverMessage);
                            }
                            
                            break;
                        } else {
                            console.warn(`Message sending failed at ${endpoint}:`, result.message);
                        }
                    } else {
                        console.warn(`Message sending failed at ${endpoint} with status ${response.status}`);
                    }
                } catch (error) {
                    console.warn(`Error sending message to ${endpoint}:`, error);
                }
            }

            if (messageSent) {
                // Show success notification
                if (window.chatManager) {
                    window.chatManager.showNotification('پیام ارسال شد', 'success');
                }
            } else {
                // If all endpoints fail, show error and keep optimistic message
                if (window.chatManager) {
                    window.chatManager.showNotification('پیام ارسال شد (فقط محلی)', 'warning');
                }
                
                // Mark optimistic message as failed
                markOptimisticMessageAsFailed(optimisticMessage.id);
            }

        } catch (error) {
            console.error("Error sending message: ", error);
            if (window.chatManager) {
                window.chatManager.showNotification(`خطا در ارسال پیام: ${error.message}`, 'error');
            }
        }
    }

    // Setup message form
    function setupMessageForm() {
        if (messageForm) {
            messageForm.addEventListener("submit", function (e) {
                e.preventDefault();
                const content = messageInput.value.trim();
                if (content) {
                    sendMessage(content);
                }
            });
        }

        // Setup typing indicators
        if (messageInput) {
            messageInput.addEventListener('input', function() {
                if (typingTimer) clearTimeout(typingTimer);
                
                if (!isTyping && currentChatRoomId) {
                    isTyping = true;
                    if (chatConnection && chatConnection.state === "Connected") {
                        chatConnection.invoke("UserTyping", currentChatRoomId.toString(), currentUser?.userName || "کاربر");
                    }
                }

                typingTimer = setTimeout(() => {
                    isTyping = false;
                    if (chatConnection && chatConnection.state === "Connected") {
                        chatConnection.invoke("UserStoppedTyping", currentChatRoomId.toString());
                    }
                }, 1000);
            });
        }
        
        // Setup send button
        if (sendBtn) {
            sendBtn.addEventListener('click', function(e) {
                e.preventDefault();
                const content = messageInput.value.trim();
                if (content) {
                    sendMessage(content);
                }
            });
        }
    }

    // Setup file upload
    function setupFileUpload() {
        if (fileInput) {
            fileInput.addEventListener('change', async function(e) {
                const file = e.target.files[0];
                if (file && currentChatRoomId) {
                    await uploadFile(file);
                }
            });
        }
    }

    // Upload file
    async function uploadFile(file) {
        if (!currentChatRoomId) {
            if (window.chatManager) {
                window.chatManager.showNotification('ابتدا یک چت را انتخاب کنید', 'warning');
            }
            return;
        }

        try {
            // Show upload progress
            showUploadProgress(file.name);
            
            // Try multiple endpoints for file upload
            const endpoints = [
                '/Chat/UploadFile',
                '/Chat/Upload',
                '/Chat/FileUpload',
                '/Chat/SendFile'
            ];

            let uploadSuccess = false;
            let uploadResult = null;

            for (const endpoint of endpoints) {
                try {
                    console.log(`Trying to upload file to: ${endpoint}`);
                    const formData = new FormData();
                    formData.append('file', file);
                    formData.append('chatRoomId', currentChatRoomId);

                    const response = await fetch(endpoint, {
                        method: 'POST',
                        body: formData
                    });

                    if (response.ok) {
                        const result = await response.json();
                        if (result.success) {
                            console.log(`File uploaded successfully to ${endpoint}:`, result);
                            uploadSuccess = true;
                            uploadResult = result.data || result;
                            break;
                        } else {
                            console.warn(`Upload failed at ${endpoint}:`, result.message);
                        }
                    } else {
                        console.warn(`Upload failed at ${endpoint} with status ${response.status}`);
                    }
                } catch (error) {
                    console.warn(`Error uploading to ${endpoint}:`, error);
                }
            }

            if (uploadSuccess && uploadResult) {
                // Send file message with enhanced content
                const fileMessage = createFileMessage(file, uploadResult);
                await sendMessage(fileMessage, 1); // File message type
                
                if (window.chatManager) {
                    window.chatManager.showNotification('فایل با موفقیت آپلود شد', 'success');
                }
            } else {
                // If upload fails, send a simple file message
                const simpleFileMessage = createSimpleFileMessage(file);
                await sendMessage(simpleFileMessage, 1);
                
                if (window.chatManager) {
                    window.chatManager.showNotification('فایل ارسال شد (آپلود ناموفق)', 'warning');
                }
            }
        } catch (error) {
            console.error("Error uploading file: ", error);
            if (window.chatManager) {
                window.chatManager.showNotification(`خطا در ارسال فایل: ${error.message}`, 'error');
            }
        } finally {
            hideUploadProgress();
        }
    }

    // Create enhanced file message content
    function createFileMessage(file, uploadResult) {
        const fileSize = formatFileSize(file.size);
        const fileType = getFileType(file.type);
        
        // Safely get download URL from upload result
        const downloadUrl = uploadResult?.downloadUrl || uploadResult?.url || uploadResult?.fileUrl || '#';
        
        return `
            <div class="file-message">
                <div class="file-icon">
                    <i class="mdi ${getFileIcon(file.type)} mdi-24px"></i>
                </div>
                <div class="file-info">
                    <div class="file-name">${file.name}</div>
                    <div class="file-details">
                        <span class="file-size">${fileSize}</span>
                        <span class="file-type">${fileType}</span>
                    </div>
                </div>
                <div class="file-actions">
                    <a href="${downloadUrl}" class="btn btn-sm btn-primary" download>
                        <i class="mdi mdi-download"></i> دانلود
                    </a>
                </div>
            </div>
        `;
    }

    // Create simple file message when upload fails
    function createSimpleFileMessage(file) {
        const fileSize = formatFileSize(file.size);
        const fileType = getFileType(file.type);
        
        return `
            <div class="file-message">
                <div class="file-icon">
                    <i class="mdi ${getFileIcon(file.type)} mdi-24px"></i>
                </div>
                <div class="file-info">
                    <div class="file-name">${file.name}</div>
                    <div class="file-details">
                        <span class="file-size">${fileSize}</span>
                        <span class="file-type">${fileType}</span>
                    </div>
                </div>
                <div class="file-actions">
                    <span class="text-muted">فایل ارسال شد</span>
                </div>
            </div>
        `;
    }

    // Format file size
    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    // Get file type description
    function getFileType(mimeType) {
        const types = {
            'image/': 'تصویر',
            'video/': 'ویدیو',
            'audio/': 'صوت',
            'text/': 'متن',
            'application/pdf': 'PDF',
            'application/msword': 'Word',
            'application/vnd.openxmlformats-officedocument.wordprocessingml.document': 'Word',
            'application/vnd.ms-excel': 'Excel',
            'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': 'Excel'
        };
        
        for (const [type, description] of Object.entries(types)) {
            if (mimeType.startsWith(type)) {
                return description;
            }
        }
        return 'فایل';
    }

    // Get file icon based on type
    function getFileIcon(mimeType) {
        if (mimeType.startsWith('image/')) return 'mdi-image';
        if (mimeType.startsWith('video/')) return 'mdi-video';
        if (mimeType.startsWith('audio/')) return 'mdi-music';
        if (mimeType.startsWith('text/')) return 'mdi-file-document';
        if (mimeType === 'application/pdf') return 'mdi-file-pdf-box';
        if (mimeType.includes('word')) return 'mdi-file-word-box';
        if (mimeType.includes('excel')) return 'mdi-file-excel-box';
        return 'mdi-file';
    }

    // Show upload progress
    function showUploadProgress(fileName) {
        const progressElement = document.createElement('div');
        progressElement.className = 'upload-progress';
        progressElement.innerHTML = `
            <div class="d-flex align-items-center p-2 bg-light rounded">
                <div class="spinner-border spinner-border-sm text-primary me-2" role="status">
                    <span class="visually-hidden">در حال آپلود...</span>
                </div>
                <span class="text-muted">در حال آپلود ${fileName}...</span>
            </div>
        `;
        
        const chatHistory = document.querySelector('.chat-history');
        if (chatHistory) {
            chatHistory.appendChild(progressElement);
            scrollToBottom();
        }
    }

    // Hide upload progress
    function hideUploadProgress() {
        const progressElement = document.querySelector('.upload-progress');
        if (progressElement) {
            progressElement.remove();
        }
    }

    // Setup search functionality
    function setupSearchFunctionality() {
        if (chatSearchInput) {
            let searchTimeout;
            
            chatSearchInput.addEventListener("keyup", function (e) {
                const searchTerm = e.currentTarget.value.toLowerCase();
                
                // Clear previous timeout
                if (searchTimeout) {
                    clearTimeout(searchTimeout);
                }
                
                // If search term is empty, load all contacts
                if (!searchTerm.trim()) {
                    loadContacts();
                    return;
                }
                
                // If search term is long enough, search on server
                if (searchTerm.length >= 2) {
                    searchTimeout = setTimeout(() => {
                        searchUsers(searchTerm);
                    }, 300); // Debounce search
                }
                
                // Also filter locally for immediate feedback
                filterContacts(searchTerm);
            });
        }
    }

    // Filter contacts
    function filterContacts(searchTerm) {
        const chatItems = [].slice.call(document.querySelectorAll("#chat-list li:not(.chat-contact-list-item-title)"));
        const contactItems = [].slice.call(document.querySelectorAll("#contact-list li:not(.chat-contact-list-item-title)"));
        const noChatResults = document.querySelector(".chat-list-item-0");
        const noContactResults = document.querySelector(".contact-list-item-0");

        filterItems(chatItems, searchTerm, noChatResults);
        filterItems(contactItems, searchTerm, noContactResults);
    }

    // Enhanced search with server-side search
    async function searchUsers(searchTerm) {
        if (searchTerm.length < 2) return; // Don't search for very short terms
        
        try {
            const response = await fetch(`/Chat/SearchUsers?term=${encodeURIComponent(searchTerm)}`);
            if (response.ok) {
                const users = await response.json();
                updateContactsList(users);
            }
        } catch (error) {
            console.error("Error searching users: ", error);
        }
    }

    // Filter items helper
    function filterItems(items, searchTerm, noResultsElement) {
        let visibleCount = 0;
        items.forEach(item => {
            const text = item.textContent.toLowerCase();
            if (!searchTerm || text.indexOf(searchTerm) !== -1) {
                item.classList.add("d-flex");
                item.classList.remove("d-none");
                visibleCount++;
            } else {
                item.classList.add("d-none");
            }
        });

        if (noResultsElement) {
            if (visibleCount === 0) {
                noResultsElement.classList.remove("d-none");
            } else {
                noResultsElement.classList.add("d-none");
            }
        }
    }

    // Setup status changes
    function setupStatusChanges() {
        if (statusRadios && statusRadios.length > 0) {
            statusRadios.forEach(radio => {
                radio.addEventListener("click", function (e) {
                    const status = e.currentTarget.value;
                    updateUserStatus(status);
                });
            });
        } else {
            // If no status radios found, try to find them dynamically
            setTimeout(() => {
                const dynamicStatusRadios = document.querySelectorAll(".form-check-input[name='chat-user-status']");
                if (dynamicStatusRadios.length > 0) {
                    dynamicStatusRadios.forEach(radio => {
                        radio.addEventListener("click", function (e) {
                            const status = e.currentTarget.value;
                            updateUserStatus(status);
                        });
                    });
                }
            }, 1000);
        }
    }

    // Update user status
    function updateUserStatus(status) {
        const leftSidebarAvatar = document.querySelector(".chat-sidebar-left-user .avatar");
        const contactsAvatar = document.querySelector(".app-chat-contacts .avatar");

        if (leftSidebarAvatar) {
            leftSidebarAvatar.removeAttribute("class");
            leftSidebarAvatar.className = `avatar avatar-xl ${statusClasses[status]}`;
        }

        if (contactsAvatar) {
            contactsAvatar.removeAttribute("class");
            contactsAvatar.className = `flex-shrink-0 avatar ${statusClasses[status]} me-3`;
        }
    }

    // Setup contact selection
    function setupContactSelection() {
        if (chatContactListItems && chatContactListItems.length > 0) {
            chatContactListItems.forEach(item => {
                item.addEventListener("click", function (e) {
                    chatContactListItems.forEach(item => item.classList.remove("active"));
                    e.currentTarget.classList.add("active");
                });
            });
        } else {
            // If no contact items found, try to find them dynamically
            setTimeout(() => {
                const dynamicContactItems = document.querySelectorAll(".chat-contact-list-item:not(.chat-contact-list-item-title)");
                if (dynamicContactItems.length > 0) {
                    dynamicContactItems.forEach(item => {
                        item.addEventListener("click", function (e) {
                            dynamicContactItems.forEach(item => item.classList.remove("active"));
                            e.currentTarget.classList.add("active");
                        });
                    });
                }
            }, 1000);
        }
    }

    // Setup speech to text
    function setupSpeechToText() {
        if (speechToTextBtn.length && typeof webkitSpeechRecognition !== 'undefined') {
            const recognition = new webkitSpeechRecognition();
            let isRecording = false;

            // Configure recognition
            recognition.continuous = false;
            recognition.interimResults = true;
            recognition.lang = 'fa-IR'; // Persian language

            speechToTextBtn.on("click", function () {
                const button = $(this);
                
                if (!isRecording) {
                    startVoiceRecording(recognition, button);
                } else {
                    stopVoiceRecording(recognition, button);
                }
            });

            recognition.onresult = function (event) {
                let finalTranscript = '';
                let interimTranscript = '';
                
                for (let i = event.resultIndex; i < event.results.length; i++) {
                    const transcript = event.results[i][0].transcript;
                    if (event.results[i].isFinal) {
                        finalTranscript += transcript;
                    } else {
                        interimTranscript += transcript;
                    }
                }
                
                // Update input with final transcript
                if (finalTranscript) {
                    messageInput.value = finalTranscript;
                }
                
                // Show interim results
                showInterimResults(interimTranscript);
            };

            recognition.onerror = function (event) {
                console.error("Speech recognition error:", event.error);
                stopVoiceRecording(recognition, speechToTextBtn);
                
                let errorMessage = 'خطا در تشخیص صدا';
                switch (event.error) {
                    case 'no-speech':
                        errorMessage = 'صدایی تشخیص داده نشد';
                        break;
                    case 'audio-capture':
                        errorMessage = 'خطا در ضبط صدا';
                        break;
                    case 'not-allowed':
                        errorMessage = 'دسترسی به میکروفون مجاز نیست';
                        break;
                }
                
                if (window.chatManager) {
                    window.chatManager.showNotification(errorMessage, 'error');
                }
            };

            recognition.onend = function () {
                stopVoiceRecording(recognition, speechToTextBtn);
            };
        } else {
            // Hide speech-to-text button if not supported
            speechToTextBtn.hide();
        }
    }

    // Start voice recording
    function startVoiceRecording(recognition, button) {
        try {
            recognition.start();
            isRecording = true;
            button.addClass('btn-danger').removeClass('btn-text-secondary');
            button.html('<i class="mdi mdi-stop mdi-20px"></i>');
            button.attr('title', 'توقف ضبط صدا');
            
            // Show recording indicator
            showRecordingIndicator();
        } catch (error) {
            console.error("Error starting speech recognition:", error);
            if (window.chatManager) {
                window.chatManager.showNotification('خطا در شروع ضبط صدا', 'error');
            }
        }
    }

    // Stop voice recording
    function stopVoiceRecording(recognition, button) {
        try {
            recognition.stop();
            isRecording = false;
            button.removeClass('btn-danger').addClass('btn-text-secondary');
            button.html('<i class="mdi mdi-microphone mdi-20px"></i>');
            button.attr('title', 'ضبط صدا');
            
            // Hide recording indicator
            hideRecordingIndicator();
        } catch (error) {
            console.error("Error stopping speech recognition:", error);
        }
    }

    // Show recording indicator
    function showRecordingIndicator() {
        const indicator = document.createElement('div');
        indicator.className = 'recording-indicator';
        indicator.innerHTML = `
            <div class="d-flex align-items-center p-2 bg-danger text-white rounded position-fixed" 
                 style="top: 20px; left: 50%; transform: translateX(-50%); z-index: 9999;">
                <div class="spinner-border spinner-border-sm me-2" role="status">
                    <span class="visually-hidden">در حال ضبط...</span>
                </div>
                <span>در حال ضبط صدا...</span>
            </div>
        `;
        document.body.appendChild(indicator);
    }

    // Hide recording indicator
    function hideRecordingIndicator() {
        const indicator = document.querySelector('.recording-indicator');
        if (indicator) {
            indicator.remove();
        }
    }

    // Show interim speech recognition results
    function showInterimResults(interimTranscript) {
        if (!interimTranscript) return;
        
        // Create or update interim results display
        let interimDisplay = document.querySelector('.interim-results');
        if (!interimDisplay) {
            interimDisplay = document.createElement('div');
            interimDisplay.className = 'interim-results alert alert-info alert-dismissible fade show position-fixed';
            interimDisplay.style.cssText = 'top: 80px; left: 50%; transform: translateX(-50%); z-index: 9999; min-width: 300px;';
            interimDisplay.innerHTML = `
                <div class="d-flex align-items-center">
                    <i class="mdi mdi-microphone me-2"></i>
                    <span class="interim-text"></span>
                    <button type="button" class="btn-close ms-auto" data-bs-dismiss="alert"></button>
                </div>
            `;
            document.body.appendChild(interimDisplay);
        }
        
        // Update interim text
        const interimText = interimDisplay.querySelector('.interim-text');
        if (interimText) {
            interimText.textContent = interimTranscript;
        }
        
        // Auto hide after 3 seconds
        setTimeout(() => {
            if (interimDisplay.parentNode) {
                interimDisplay.remove();
            }
        }, 3000);
    }

    // Utility functions
    function scrollToBottom() {
        if (chatHistoryBody) {
            chatHistoryBody.scrollTo(0, chatHistoryBody.scrollHeight);
        }
    }

    function formatTime(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return 'همین الان';
        if (diffMins < 60) return `${diffMins} دقیقه`;
        if (diffHours < 24) return `${diffHours} ساعت`;
        if (diffDays < 7) return `${diffDays} روز`;
        
        return date.toLocaleDateString('fa-IR');
    }

    function showTypingIndicator(userName) {
        // Remove existing typing indicator
        hideTypingIndicator();
        
        const chatHistory = document.querySelector('.chat-history');
        if (!chatHistory) return;

        const typingElement = document.createElement('li');
        typingElement.className = 'chat-message typing-indicator';
        typingElement.innerHTML = `
            <div class="d-flex overflow-hidden">
                <div class="user-avatar flex-shrink-0 me-3">
                    <div class="avatar avatar-sm">
                        <img src="/assets/img/avatars/1.png" alt="Avatar" class="rounded-circle">
                    </div>
                </div>
                <div class="chat-message-wrapper flex-grow-1">
                    <div class="chat-message-text">
                        <p class="mb-0 text-muted">
                            <em>${userName || 'کاربر'} در حال تایپ کردن است</em>
                            <span class="typing-dots">
                                <span class="dot"></span>
                                <span class="dot"></span>
                                <span class="dot"></span>
                            </span>
                        </p>
                    </div>
                </div>
            </div>
        `;

        chatHistory.appendChild(typingElement);
        scrollToBottom();
    }

    function hideTypingIndicator() {
        const existingTypingIndicator = document.querySelector('.typing-indicator');
        if (existingTypingIndicator) {
            existingTypingIndicator.remove();
        }
    }

    function updateMessageContent(messageId, newContent) {
        const messageElement = document.querySelector(`[data-message-id="${messageId}"]`);
        if (messageElement) {
            const contentElement = messageElement.querySelector('.chat-message-text p');
            if (contentElement) {
                contentElement.textContent = newContent;
            }
        }
    }

    // Update optimistic message with server data
    function updateOptimisticMessage(tempId, serverMessage) {
        const messageElement = document.querySelector(`[data-message-id="${tempId}"]`);
        if (messageElement && serverMessage) {
            // Update the ID to match server
            messageElement.setAttribute('data-message-id', serverMessage.id || serverMessage.messageId);
            
            // Remove optimistic styling
            messageElement.classList.remove('optimistic-message');
            
            // Update content if different
            const contentElement = messageElement.querySelector('.chat-message-text p');
            if (contentElement && serverMessage.content && contentElement.textContent !== serverMessage.content) {
                contentElement.textContent = serverMessage.content;
            }
            
            // Update time if available
            const timeElement = messageElement.querySelector('small');
            if (timeElement && serverMessage.sentDate) {
                timeElement.textContent = formatTime(serverMessage.sentDate);
            }
        }
    }

    // Mark optimistic message as failed
    function markOptimisticMessageAsFailed(tempId) {
        const messageElement = document.querySelector(`[data-message-id="${tempId}"]`);
        if (messageElement) {
            // Add failed styling
            messageElement.classList.add('message-failed');
            
            // Add retry button
            const messageText = messageElement.querySelector('.chat-message-text');
            if (messageText) {
                const retryButton = document.createElement('button');
                retryButton.className = 'btn btn-sm btn-outline-danger mt-2';
                retryButton.innerHTML = '<i class="mdi mdi-refresh"></i> تلاش مجدد';
                retryButton.onclick = () => retryMessage(tempId);
                messageText.appendChild(retryButton);
            }
        }
    }

    // Retry sending failed message
    async function retryMessage(tempId) {
        const messageElement = document.querySelector(`[data-message-id="${tempId}"]`);
        if (messageElement) {
            const contentElement = messageElement.querySelector('.chat-message-text p');
            if (contentElement) {
                const content = contentElement.textContent;
                // Remove retry button
                const retryButton = messageElement.querySelector('button');
                if (retryButton) retryButton.remove();
                
                // Remove failed styling
                messageElement.classList.remove('message-failed');
                
                // Remove the failed message
                messageElement.remove();
                
                // Try to send again
                await sendMessage(content);
            }
        }
    }

    function removeMessage(messageId) {
        const messageElement = document.querySelector(`[data-message-id="${messageId}"]`);
        if (messageElement) {
            messageElement.remove();
        }
    }

    // Show connection status notification
    function showConnectionStatus(message, type) {
        console.log("Connection status:", message, type);
        
        if (window.chatManager) {
            window.chatManager.showNotification(message, type);
        } else {
            // Fallback notification
            const notification = document.createElement('div');
            notification.className = `alert alert-${type === 'error' ? 'danger' : type} alert-dismissible fade show position-fixed`;
            notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
            notification.innerHTML = `
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            `;
            document.body.appendChild(notification);
            
            // Auto remove after 5 seconds
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.remove();
                }
            }, 5000);
        }
    }

    // Enable message input area and send button
    function enableMessageInput() {
        if (messageInput && sendBtn) {
            messageInput.removeAttribute('disabled');
            sendBtn.removeAttribute('disabled');
            messageInput.focus(); // Focus the input after enabling
        }
    }

    // Disable message input area and send button
    function disableMessageInput() {
        if (messageInput && sendBtn) {
            messageInput.setAttribute('disabled', 'disabled');
            sendBtn.setAttribute('disabled', 'disabled');
        }
    }

    // Ensure scrollbars are working, add fallback if needed
    function ensureScrollbars() {
        // Check if PerfectScrollbar is available
        if (typeof PerfectScrollbar !== 'undefined') {
            // If scrollbars are not initialized, try to initialize them
            if (!window.chatScrollbars || !window.chatScrollbars.chatList) {
                console.log("Reinitializing scrollbars...");
                setupScrollbars();
            }
        } else {
            // Fallback: enable native scrolling for chat list
            const chatList = document.getElementById('chat-list');
            if (chatList) {
                chatList.style.overflowY = 'auto';
                chatList.style.maxHeight = '400px';
                console.log("Native scrolling enabled for chat list");
            }
            
            // Fallback: enable native scrolling for chat history
            if (chatHistoryBody) {
                chatHistoryBody.style.overflowY = 'auto';
                console.log("Native scrolling enabled for chat history");
            }
        }
    }

    // Initialize chat when DOM is ready
    initializeChat();
});