/**
 * Modern Chat Application - FIXED VERSION
 * Built with Vanilla JavaScript and jQuery
 * No AngularJS dependency
 */

const ChatApp = (function() {
    'use strict';
    
    // Private variables
    let currentChatId = null;
    let currentChat = null;
    let messages = [];
    let contacts = [];
    let chatRooms = [];
    let selectedFile = null;
    let selectedMessage = null;
    let replyingTo = null;
    let editingMessage = null;
    let isTyping = false;
    let typingTimeout = null;
    let connection = null;
    
    // Emojis array
    const emojis = ['😀', '😃', '😄', '😁', '😆', '😅', '😂', '🤣', '😊', '😇', '🙂', '🙃', '😉', '😌', '😍', '🥰', '😘', '😗', '😙', '😚', '😋', '😛', '😝', '😜', '🤪', '🤨', '🧐', '🤓', '😎', '🤩', '🥳', '😏', '😒', '😞', '😔', '😟', '😕', '🙁', '☹️', '😣', '😖', '😫', '😩', '🥺', '😢', '😭', '😤', '😠', '😡', '🤬', '🤯', '😳', '🥵', '🥶', '😱', '😨', '😰', '😥', '😓', '🤗', '🤔', '🤭', '🤫', '🤥', '😶', '😐', '😑', '😯', '😦', '😧', '😮', '😲', '🥱', '😴', '🤤', '😪', '😵', '🤐', '🥴', '🤢', '🤮', '🤧', '😷', '🤒', '🤕', '🤑', '🤠', '💩', '👻', '💀', '☠️', '👽', '👾', '🤖', '🎃', '👺', '👹', '👿', '😈', '🤡', '💋', '💌', '💘', '💝', '💖', '💗', '💓', '💞', '💕', '💟', '❣️', '💔', '❤️', '🧡', '💛', '💚', '💙', '💜', '🖤', '💯', '💢', '💥', '💫', '💦', '💨', '🕳️', '💬', '🗨️', '🗯️', '💭', '💤'];
    
    // Public methods
    const publicAPI = {
        init: init,
        startChat: startChat,
        sendMessage: sendMessage,
        loadMessages: loadMessages,
        searchUsers: searchUsers,
        showContextMenu: showContextMenu,
        hideContextMenu: hideContextMenu
    };
    
    // Initialize the application
    function init() {
        console.log('Initializing Chat Application...');
        
        // Initialize data
        initializeData();
        
        // Setup event listeners
        setupEventListeners();
        
        // Initialize SignalR
        initializeSignalR();
        
        // Load initial data
        loadInitialData();
        
        // Setup emoji picker
        setupEmojiPicker();
        
        console.log('Chat Application initialized successfully!');
    }
    
    // Initialize data from window object
    function initializeData() {
        chatRooms = window.chatRoomsData || [];
        currentChatId = null;
        currentChat = null;
        messages = [];
        contacts = [];
        
        console.log('Current user ID:', window.currentUserId);
        console.log('Current user name:', window.currentUserName);
        console.log('Chat rooms loaded:', chatRooms.length);
        
        // If no chat rooms data, try to load from server
        if (chatRooms.length === 0) {
            loadChatRoomsFromServer();
        }
    }
    
    // Load chat rooms from server
    function loadChatRoomsFromServer() {
        $.ajax({
            url: window.chatRoomsUrl,
            method: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    chatRooms = response.data;
                    renderChatRooms();
                    console.log('Chat rooms loaded from server:', chatRooms.length);
                }
            },
            error: function(xhr, status, error) {
                console.error('Failed to load chat rooms from server:', error);
            }
        });
    }
    
    // Setup all event listeners
    function setupEventListeners() {
        // Search functionality
        $('#searchInput').on('input', handleSearchInput);
        $('#clearSearch').on('click', clearSearch);
        
        // Chat actions
        $('#startNewChatBtn').on('click', showContactSelection);
        $('#createSelfChatBtn').on('click', createSelfChat);
        $('#backToWelcomeBtn').on('click', backToWelcome);
        
        // Message input
        $('#messageInput').on('input', handleMessageInput);
        $('#messageInput').on('keypress', handleMessageKeyPress);
        $('#sendBtn').on('click', sendMessage);
        
        // File handling
        $('#fileBtn').on('click', selectFile);
        $('#fileInput').on('change', handleFileSelect);
        $('#removeFileBtn').on('click', removeFile);
        
        // Emoji picker
        $('#emojiBtn').on('click', toggleEmojiPicker);
        $('#closeEmojiBtn').on('click', hideEmojiPicker);
        
        // Voice message
        $('#voiceBtn').on('click', selectVoice);
        
        // Context menu actions
        $('#pinMessageBtn').on('click', pinMessage);
        $('#unpinMessageBtn').on('click', unpinMessage);
        $('#editMessageBtn').on('click', editMessage);
        $('#forwardMessageBtn').on('click', forwardMessage);
        $('#replyMessageBtn').on('click', replyToMessage);
        $('#copyMessageBtn').on('click', copyMessage);
        $('#deleteMessageBtn').on('click', deleteMessage);
        
        // Hide context menu when clicking outside
        $(document).on('click', function(e) {
            if (!$(e.target).closest('.message-context-menu, .message').length) {
                hideContextMenu();
            }
        });
    }
    
    // Initialize SignalR connection
    function initializeSignalR() {
        if (typeof signalR === 'undefined') {
            console.warn('SignalR not loaded, real-time features disabled');
            return;
        }
        
        try {
            connection = new signalR.HubConnectionBuilder()
                .withUrl("/chatHub")
                .withAutomaticReconnect()
                .build();
            
            // SignalR event handlers
            connection.on("ReceiveMessage", function(message) {
                if (message.chatRoomId == currentChatId) {
                    messages.push(message);
                    renderMessages();
                    scrollToBottom();
                }
                updateChatRoomLastMessage(message);
            });
            
            connection.on("UserTyping", function(userId, userName, chatRoomId) {
                if (chatRoomId == currentChatId && userId != window.currentUserId) {
                    showTypingIndicator(userName + ' در حال تایپ است...');
                }
            });
            
            connection.on("UserStoppedTyping", function(userId, chatRoomId) {
                if (chatRoomId == currentChatId && userId != window.currentUserId) {
                    hideTypingIndicator();
                }
            });
            
            connection.on("MessageEdited", function(messageId, newContent) {
                const message = messages.find(m => m.id == messageId);
                if (message) {
                    message.content = newContent;
                    message.isEdited = true;
                    renderMessages();
                }
            });
            
            connection.on("MessageDeleted", function(messageId) {
                messages = messages.filter(m => m.id != messageId);
                renderMessages();
            });
            
            // Start connection
            connection.start().catch(function(err) {
                console.error('SignalR Connection Error:', err);
            });
            
            console.log('SignalR connection established');
        } catch (error) {
            console.error('Failed to initialize SignalR:', error);
        }
    }
    
    // Load initial data
    function loadInitialData() {
        loadAllUsers();
        renderChatRooms();
        renderWelcomeScreen();
    }
    
    // Load all users for contacts
    function loadAllUsers() {
        $.ajax({
            url: window.getAllUsersUrl,
            method: 'GET',
            success: function(response) {
                console.log('Users loaded:', response.length);
                contacts = response;
                renderContacts();
                updateContactsCount();
            },
            error: function(xhr, status, error) {
                console.error('Failed to load users:', error);
            }
        });
    }
    
    // Render chat rooms
    function renderChatRooms() {
        const $chatList = $('#chatList');
        $chatList.empty();
        
        if (chatRooms.length === 0) {
            $chatList.append('<li class="empty-chats">چتی یافت نشد</li>');
            return;
        }
        
        chatRooms.forEach(function(room) {
            const $chatItem = $(`
                <li class="chat-item" data-chat-id="${room.id || room.Id}">
                    <div class="avatar">
                        <img src="${getAvatarUrl(room.avatar || room.Avatar)}" alt="" onerror="this.src='/CompanyVariables/avatars/1.png'">
                        <div class="online-indicator" style="display: ${room.participants && room.participants.some(p => p.isOnline) ? 'block' : 'none'}"></div>
                    </div>
                    <div class="chat-info">
                        <div class="chat-name">${room.name || room.Name || 'چت جدید'}</div>
                        <div class="chat-last-message">${room.lastMessage || room.LastMessage || 'بدون پیام'}</div>
                        <div class="chat-meta">
                            <span class="chat-time">${getTimeAgo(room.lastMessageDate || room.LastMessageDate)}</span>
                            <span class="unread-badge" style="display: ${(room.unreadCount || room.UnreadCount || 0) > 0 ? 'inline' : 'none'}">${room.unreadCount || room.UnreadCount || 0}</span>
                        </div>
                    </div>
                    <div class="chat-actions">
                        <button class="btn-delete-chat" title="حذف چت">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </li>
            `);
            
            // Add click event
            $chatItem.find('.chat-info').on('click', function() {
                openChat(room.id || room.Id);
            });
            
            // Add delete event
            $chatItem.find('.btn-delete-chat').on('click', function(e) {
                e.stopPropagation();
                deleteChatRoom(room.id || room.Id);
            });
            
            $chatList.append($chatItem);
        });
        
        updateChatCount();
    }
    
    // Render contacts
    function renderContacts() {
        const $contactsList = $('#contactsList');
        $contactsList.empty();
        
        if (contacts.length === 0) {
            $contactsList.append(`
                <li class="contact-item empty-contacts">
                    <div class="empty-content">
                        <i class="fas fa-user-friends empty-icon"></i>
                        <div class="contact-name">مخاطبی یافت نشد</div>
                        <div class="contact-status">کاربران سایت در اینجا نمایش داده می‌شوند</div>
                    </div>
                </li>
            `);
            return;
        }
        
        contacts.forEach(function(contact) {
            const $contactItem = $(`
                <li class="contact-item" data-user-id="${contact.userId || contact.UserId}">
                    <div class="avatar">
                        <img src="${getAvatarUrl(contact.avatar || contact.Avatar)}" alt="" onerror="this.src='/CompanyVariables/avatars/1.png'">
                        <div class="online-indicator" style="display: ${contact.isOnline || contact.IsOnline ? 'block' : 'none'}"></div>
                        <div class="offline-indicator" style="display: ${!(contact.isOnline || contact.IsOnline) ? 'block' : 'none'}"></div>
                    </div>
                    <div class="contact-info">
                        <div class="contact-name">${contact.fullName || contact.FullName}</div>
                        <div class="contact-status">
                            <span class="status-dot ${contact.isOnline || contact.IsOnline ? 'online' : ''}"></span>
                            ${contact.isOnline || contact.IsOnline ? 'آنلاین' : 'آفلاین'}
                        </div>
                    </div>
                    <button class="btn-start-chat-contact" title="شروع چت">
                        <i class="fas fa-comment"></i>
                    </button>
                </li>
            `);
            
            // Add click events
            $contactItem.find('.contact-info').on('click', function() {
                startChat(contact);
            });
            
            $contactItem.find('.btn-start-chat-contact').on('click', function() {
                startChat(contact);
            });
            
            $contactsList.append($contactItem);
        });
    }
    
    // Render welcome screen
    function renderWelcomeScreen() {
        $('#welcomeScreen').show();
        $('#chatInterface').hide();
    }
    
    // Render chat interface
    function renderChatInterface() {
        $('#welcomeScreen').hide();
        $('#chatInterface').show();
        
        // Update chat header
        if (currentChat) {
            $('#currentChatName').text(currentChat.name || currentChat.Name || 'چت جدید');
            $('#currentChatAvatar').attr('src', getAvatarUrl(currentChat.avatar || currentChat.Avatar));
            
            const isOnline = currentChat.participants && currentChat.participants.some(p => p.isOnline || p.IsOnline);
            $('#currentChatStatus').text(isOnline ? 'آنلاین' : 'آفلاین');
            $('#currentChatStatusDot').toggleClass('online', isOnline);
            $('#currentChatOnlineIndicator').toggle(isOnline);
            
            // Show participants button for group chats
            $('#showParticipantsBtn').toggle(currentChat.isGroup || currentChat.IsGroup);
        }
    }
    
    // Render messages
    function renderMessages() {
        const $chatMessages = $('#chatMessages');
        $chatMessages.empty();
        
        if (messages.length === 0) {
            $chatMessages.append('<div class="no-messages">پیامی یافت نشد</div>');
            return;
        }
        
        messages.forEach(function(message) {
            const isOwnMessage = message.senderId === window.currentUserId;
            const $message = $(`
                <div class="message ${isOwnMessage ? 'own' : ''}" data-message-id="${message.id}">
                    <div class="avatar">
                        <img src="${getAvatarUrl(message.senderAvatar)}" alt="" onerror="this.src='/CompanyVariables/avatars/1.png'">
                    </div>
                    <div class="content">
                        <div class="message-header">
                            <span class="sender-name">${message.senderName}</span>
                            <span class="message-time">${getTimeAgo(message.sentDate)}</span>
                        </div>
                        <div class="text">${message.content}</div>
                        <div class="message-footer">
                            <span class="edited-indicator" style="display: ${message.isEdited ? 'inline' : 'none'}">(ویرایش شده)</span>
                            <div class="message-status" style="display: ${isOwnMessage ? 'block' : 'none'}">
                                <i class="fas fa-check status-icon ${getStatusClass(message.status)}"></i>
                            </div>
                        </div>
                    </div>
                </div>
            `);
            
            // Add context menu events
            $message.on('mouseenter', function(e) {
                showMessageContextMenu(e, message);
            });
            
            $message.on('mouseleave', function() {
                // Don't hide immediately, let the timeout handle it
            });
            
            $chatMessages.append($message);
        });
        
        scrollToBottom();
    }
    
    // Show message context menu
    function showMessageContextMenu(event, message) {
        selectedMessage = message;
        const $contextMenu = $('#messageContextMenu');
        
        // Position the menu
        $contextMenu.css({
            left: event.pageX + 'px',
            top: event.pageY + 'px'
        });
        
        // Show/hide appropriate buttons
        $('#pinMessageBtn').toggle(!message.isPinned);
        $('#unpinMessageBtn').toggle(message.isPinned);
        $('#editMessageBtn').toggle(message.senderId === window.currentUserId);
        $('#deleteMessageBtn').toggle(message.senderId === window.currentUserId);
        
        $contextMenu.show();
        
        // Hide menu after a delay if mouse is not over message or menu
        setTimeout(function() {
            if (!$contextMenu.is(':hover') && !$(event.target).closest('.message').is(':hover')) {
                hideMessageContextMenu();
            }
        }, 3000);
    }
    
    // Hide message context menu
    function hideMessageContextMenu() {
        $('#messageContextMenu').hide();
        selectedMessage = null;
    }
    
    // Show context menu (public method)
    function showContextMenu(event, message) {
        showMessageContextMenu(event, message);
    }
    
    // Hide context menu (public method)
    function hideContextMenu() {
        hideMessageContextMenu();
    }
    
    // Start chat with user
    function startChat(user) {
        if (!user || !(user.userId || user.UserId)) {
            console.error('Invalid user data:', user);
            alert('خطا: شناسه کاربر یافت نشد');
            return;
        }
        
        console.log('Starting chat with user:', user.fullName || user.FullName);
        
        // Check if chat room already exists
        const existingChat = chatRooms.find(room => 
            room.participants && room.participants.some(p => (p.userId || p.UserId) === (user.userId || user.UserId))
        );
        
        if (existingChat) {
            openChat(existingChat.id || existingChat.Id);
            return;
        }
        
        // Create new chat room
        createNewChatRoom(user);
    }
    
    // Create new chat room
    function createNewChatRoom(user) {
        const chatData = {
            name: user.fullName || user.FullName,
            isGroup: false,
            participantUserIds: [user.userId || user.UserId]
        };
        
        $.ajax({
            url: window.createChatRoomUrl,
            method: 'POST',
            data: chatData,
            success: function(response) {
                if (response.success) {
                    console.log('Chat room created successfully:', response);
                    const newChatRoom = {
                        id: response.chatRoomId,
                        name: user.fullName || user.FullName,
                        isGroup: false,
                        participants: [user],
                        avatar: user.avatar || user.Avatar
                    };
                    
                    chatRooms.push(newChatRoom);
                    renderChatRooms();
                    openChat(newChatRoom.id);
                } else {
                    alert('خطا در ایجاد چت: ' + response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('Failed to create chat room:', error);
                alert('خطا در ایجاد چت');
            }
        });
    }
    
    // Open existing chat
    function openChat(chatRoomId) {
        currentChatId = chatRoomId;
        currentChat = chatRooms.find(room => (room.id || room.Id) === chatRoomId);
        
        if (!currentChat) {
            console.error('Chat room not found:', chatRoomId);
            return;
        }
        
        // Render chat interface
        renderChatInterface();
        
        // Load messages
        loadMessages();
        
        // Update active chat in sidebar
        $('.chat-item').removeClass('active');
        $(`.chat-item[data-chat-id="${chatRoomId}"]`).addClass('active');
    }
    
    // Load messages for current chat
    function loadMessages() {
        if (!currentChatId) return;
        
        console.log('Loading messages for chat:', currentChatId);
        
        // Try to load messages from server first
        $.ajax({
            url: `/Chat/Messages/${currentChatId}`,
            method: 'GET',
            success: function(response) {
                if (response.success && response.data && response.data.length > 0) {
                    // Map server response to local format
                    messages = response.data.map(function(msg) {
                        return {
                            id: msg.id || msg.Id,
                            content: msg.content || msg.Content,
                            senderId: msg.senderId || msg.SenderId,
                            senderName: msg.senderName || msg.SenderName,
                            senderAvatar: msg.senderAvatar || msg.SenderAvatar,
                            sentDate: new Date(msg.sentDate || msg.SentDate),
                            status: msg.status || msg.Status || 1,
                            isEdited: msg.isEdited || msg.IsEdited || false
                        };
                    });
                    
                    // Sort messages by date (oldest first)
                    messages.sort((a, b) => new Date(a.sentDate) - new Date(b.sentDate));
                    
                    console.log('Messages loaded from server:', messages.length);
                } else {
                    // Fallback to mock data if no messages found
                    console.log('No messages from server, using mock data');
                    messages = [
                        {
                            id: 1,
                            content: 'سلام! چطوری؟',
                            senderId: currentChat.participants && currentChat.participants[0] ? (currentChat.participants[0].userId || currentChat.participants[0].UserId) : 'unknown',
                            senderName: currentChat.participants && currentChat.participants[0] ? (currentChat.participants[0].fullName || currentChat.participants[0].FullName) : 'کاربر ناشناس',
                            senderAvatar: currentChat.participants && currentChat.participants[0] ? (currentChat.participants[0].avatar || currentChat.participants[0].Avatar) : '1.png',
                            sentDate: new Date(Date.now() - 60000),
                            status: 3,
                            isEdited: false
                        }
                    ];
                }
                
                renderMessages();
            },
            error: function(xhr, status, error) {
                console.error('Failed to load messages from server:', error);
                // Fallback to mock data
                messages = [
                    {
                        id: 1,
                        content: 'سلام! چطوری؟',
                        senderId: currentChat.participants && currentChat.participants[0] ? (currentChat.participants[0].userId || currentChat.participants[0].UserId) : 'unknown',
                        senderName: currentChat.participants && currentChat.participants[0] ? (currentChat.participants[0].fullName || currentChat.participants[0].FullName) : 'کاربر ناشناس',
                        senderAvatar: currentChat.participants && currentChat.participants[0] ? (currentChat.participants[0].avatar || currentChat.participants[0].Avatar) : '1.png',
                        sentDate: new Date(Date.now() - 60000),
                        status: 3,
                        isEdited: false
                    }
                ];
                renderMessages();
            }
        });
    }
    
    // Send message
    function sendMessage() {
        const messageText = $('#messageInput').val().trim();
        
        if (!messageText && !selectedFile) {
            return;
        }
        
        if (!currentChatId) {
            alert('لطفاً ابتدا چتی را انتخاب کنید');
            return;
        }
        
        const messageData = {
            content: messageText,
            chatRoomId: currentChatId
        };
        
        $.ajax({
            url: window.sendMessageUrl,
            method: 'POST',
            data: messageData,
            success: function(response) {
                if (response.success) {
                    // Clear input
                    $('#messageInput').val('');
                    removeFile();
                    
                    // Add message to local array
                    const newMessage = {
                        id: response.messageId || Date.now(),
                        content: messageText,
                        senderId: window.currentUserId,
                        senderName: window.currentUserName,
                        senderAvatar: window.currentUserAvatar,
                        sentDate: new Date(),
                        status: 1,
                        isEdited: false
                    };
                    
                    messages.push(newMessage);
                    renderMessages();
                    
                    // Send typing stopped signal
                    if (connection) {
                        connection.invoke("StopTyping", currentChatId);
                    }
                } else {
                    alert('خطا در ارسال پیام: ' + response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('Failed to send message:', error);
                alert('خطا در ارسال پیام');
            }
        });
    }
    
    // Handle message input
    function handleMessageInput() {
        const messageText = $('#messageInput').val().trim();
        const hasFile = selectedFile !== null;
        
        $('#sendBtn').prop('disabled', !messageText && !hasFile);
        
        // Start typing indicator
        if (messageText && connection) {
            connection.invoke("StartTyping", window.currentUserId, window.currentUserName, currentChatId);
            isTyping = true;
            
            // Clear typing timeout
            if (typingTimeout) {
                clearTimeout(typingTimeout);
            }
            
            // Set timeout to stop typing
            typingTimeout = setTimeout(function() {
                if (isTyping && connection) {
                    connection.invoke("StopTyping", currentChatId);
                    isTyping = false;
                }
            }, 3000);
        }
    }
    
    // Handle message key press
    function handleMessageKeyPress(event) {
        if (event.which === 13 && !event.shiftKey) {
            event.preventDefault();
            sendMessage();
        }
    }
    
    // Select file
    function selectFile() {
        $('#fileInput').click();
    }
    
    // Handle file selection
    function handleFileSelect(event) {
        const file = event.target.files[0];
        if (!file) return;
        
        // Check file size (1GB limit)
        if (file.size > 1073741824) {
            alert('حجم فایل نمی‌تواند بیشتر از 1 گیگابایت باشد');
            return;
        }
        
        selectedFile = file;
        $('#fileName').text(file.name);
        $('#fileSize').text(formatFileSize(file.size));
        $('#fileUploadInfo').show();
        
        // Enable send button
        $('#sendBtn').prop('disabled', false);
    }
    
    // Remove file
    function removeFile() {
        selectedFile = null;
        $('#fileInput').val('');
        $('#fileUploadInfo').hide();
        
        // Check if send button should be disabled
        const messageText = $('#messageInput').val().trim();
        $('#sendBtn').prop('disabled', !messageText);
    }
    
    // Format file size
    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }
    
    // Setup emoji picker
    function setupEmojiPicker() {
        const $emojiGrid = $('#emojiGrid');
        
        emojis.forEach(function(emoji) {
            const $emoji = $(`<span class="emoji">${emoji}</span>`);
            $emoji.on('click', function() {
                insertEmoji(emoji);
            });
            $emojiGrid.append($emoji);
        });
    }
    
    // Toggle emoji picker
    function toggleEmojiPicker() {
        const $emojiPicker = $('#emojiPicker');
        $emojiPicker.toggle();
    }
    
    // Hide emoji picker
    function hideEmojiPicker() {
        $('#emojiPicker').hide();
    }
    
    // Insert emoji
    function insertEmoji(emoji) {
        const $messageInput = $('#messageInput');
        const currentValue = $messageInput.val();
        const cursorPos = $messageInput[0].selectionStart;
        
        const newValue = currentValue.slice(0, cursorPos) + emoji + currentValue.slice(cursorPos);
        $messageInput.val(newValue);
        
        // Set cursor position after emoji
        $messageInput[0].setSelectionRange(cursorPos + emoji.length, cursorPos + emoji.length);
        
        // Trigger input event
        $messageInput.trigger('input');
        
        // Hide emoji picker
        hideEmojiPicker();
    }
    
    // Show contact selection
    function showContactSelection() {
        // Create modal for contact selection
        const $modal = $(`
            <div class="contact-selection-modal" style="display: none;">
                <div class="modal-overlay"></div>
                <div class="modal-content">
                    <div class="modal-header">
                        <h3>انتخاب مخاطب</h3>
                        <button class="close-modal">&times;</button>
                    </div>
                    <div class="modal-body">
                        <div class="search-box">
                            <input type="text" placeholder="جستجو در مخاطبین..." class="contact-search-input">
                        </div>
                        <div class="contacts-list">
                            ${contacts.map(contact => `
                                <div class="contact-item-selectable" data-user-id="${contact.userId || contact.UserId}">
                                    <div class="avatar">
                                        <img src="${getAvatarUrl(contact.avatar || contact.Avatar)}" alt="" onerror="this.src='/CompanyVariables/avatars/1.png'">
                                        <div class="online-indicator" style="display: ${contact.isOnline || contact.IsOnline ? 'block' : 'none'}"></div>
                                    </div>
                                    <div class="contact-info">
                                        <div class="contact-name">${contact.fullName || contact.FullName}</div>
                                        <div class="contact-username">${contact.userName || contact.UserName}</div>
                                    </div>
                                    <button class="btn-select-contact">انتخاب</button>
                                </div>
                            `).join('')}
                        </div>
                    </div>
                </div>
            </div>
        `);
        
        // Add modal to body
        $('body').append($modal);
        
        // Show modal
        $modal.show();
        
        // Close modal events
        $modal.find('.close-modal, .modal-overlay').on('click', function() {
            $modal.remove();
        });
        
        // Contact selection events
        $modal.find('.contact-item-selectable').on('click', function() {
            const userId = $(this).data('user-id');
            const contact = contacts.find(c => (c.userId || c.UserId) === userId);
            if (contact) {
                startChat(contact);
                $modal.remove();
            }
        });
        
        // Search functionality
        $modal.find('.contact-search-input').on('input', function() {
            const searchTerm = $(this).val().toLowerCase();
            $modal.find('.contact-item-selectable').each(function() {
                const contactName = $(this).find('.contact-name').text().toLowerCase();
                const contactUsername = $(this).find('.contact-username').text().toLowerCase();
                if (contactName.includes(searchTerm) || contactUsername.includes(searchTerm)) {
                    $(this).show();
                } else {
                    $(this).hide();
                }
            });
        });
    }
    
    // Create self chat
    function createSelfChat() {
        // Create a personal notes chat room
        const selfChatData = {
            name: 'یادداشت‌های شخصی',
            isGroup: false,
            participantUserIds: [window.currentUserId]
        };
        
        $.ajax({
            url: window.createChatRoomUrl,
            method: 'POST',
            data: selfChatData,
            success: function(response) {
                if (response.success) {
                    console.log('Self chat created successfully:', response);
                    const newSelfChat = {
                        id: response.chatRoomId,
                        name: 'یادداشت‌های شخصی',
                        isGroup: false,
                        participants: [{
                            userId: window.currentUserId,
                            fullName: window.currentUserName,
                            avatar: window.currentUserAvatar
                        }],
                        avatar: window.currentUserAvatar
                    };
                    
                    chatRooms.push(newSelfChat);
                    renderChatRooms();
                    openChat(newSelfChat.id);
                } else {
                    alert('خطا در ایجاد یادداشت شخصی: ' + response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('Failed to create self chat:', error);
                alert('خطا در ایجاد یادداشت شخصی');
            }
        });
    }
    
    // Back to welcome
    function backToWelcome() {
        currentChatId = null;
        currentChat = null;
        messages = [];
        renderWelcomeScreen();
        $('.chat-item').removeClass('active');
    }
    
    // Select voice
    function selectVoice() {
        alert('ضبط صدا در حال توسعه است');
    }
    
    // Search functionality
    function handleSearchInput() {
        const searchTerm = $('#searchInput').val().trim();
        
        if (searchTerm.length === 0) {
            clearSearch();
            return;
        }
        
        $('#clearSearch').show();
        searchUsers(searchTerm);
    }
    
    // Clear search
    function clearSearch() {
        $('#searchInput').val('');
        $('#clearSearch').hide();
        $('#searchResults').hide();
    }
    
    // Search users
    function searchUsers(term) {
        if (term.length < 2) return;
        
        $.ajax({
            url: window.searchUsersUrl,
            method: 'GET',
            data: { term: term },
            success: function(response) {
                renderSearchResults(response);
            },
            error: function(xhr, status, error) {
                console.error('Search failed:', error);
            }
        });
    }
    
    // Render search results
    function renderSearchResults(users) {
        const $searchResults = $('#searchResults');
        $searchResults.empty();
        
        if (users.length === 0) {
            $searchResults.append('<div class="no-results">نتیجه‌ای یافت نشد</div>');
        } else {
            users.forEach(function(user) {
                const $result = $(`
                    <div class="search-result-item" data-user-id="${user.userId || user.UserId}">
                        <div class="avatar">
                            <img src="${getAvatarUrl(user.avatar || user.Avatar)}" alt="" onerror="this.src='/CompanyVariables/avatars/1.png'">
                        </div>
                        <div class="info">
                            <div class="name">${user.fullName || user.FullName}</div>
                            <div class="username">${user.userName || user.UserName}</div>
                        </div>
                        <button class="btn-start-chat">
                            <i class="fas fa-comment"></i>
                            <span>شروع چت</span>
                        </button>
                    </div>
                `);
                
                $result.find('.btn-start-chat').on('click', function() {
                    startChat(user);
                    clearSearch();
                });
                
                $searchResults.append($result);
            });
        }
        
        $searchResults.show();
    }
    
    // Context menu actions
    function pinMessage() {
        if (!selectedMessage) return;
        alert('پین پیام در حال توسعه است');
        hideContextMenu();
    }
    
    function unpinMessage() {
        if (!selectedMessage) return;
        alert('حذف پین در حال توسعه است');
        hideContextMenu();
    }
    
    function editMessage() {
        if (!selectedMessage) return;
        alert('ویرایش پیام در حال توسعه است');
        hideContextMenu();
    }
    
    function forwardMessage() {
        if (!selectedMessage) return;
        alert('فوروارد پیام در حال توسعه است');
        hideContextMenu();
    }
    
    function replyToMessage() {
        if (!selectedMessage) return;
        replyingTo = selectedMessage;
        $('#messageInput').focus();
        hideContextMenu();
    }
    
    function copyMessage() {
        if (!selectedMessage || !selectedMessage.content) return;
        
        if (navigator.clipboard) {
            navigator.clipboard.writeText(selectedMessage.content).then(function() {
                console.log('Message copied to clipboard');
            }).catch(function(err) {
                console.error('Failed to copy message:', err);
                fallbackCopy(selectedMessage.content);
            });
        } else {
            fallbackCopy(selectedMessage.content);
        }
        
        hideContextMenu();
    }
    
    function fallbackCopy(text) {
        const textArea = document.createElement("textarea");
        textArea.value = text;
        document.body.appendChild(textArea);
        textArea.select();
        document.execCommand('copy');
        document.body.removeChild(textArea);
    }
    
    function deleteMessage() {
        if (!selectedMessage) return;
        
        if (confirm('آیا از حذف این پیام اطمینان دارید؟')) {
            // Remove from local array
            messages = messages.filter(m => m.id !== selectedMessage.id);
            renderMessages();
            hideContextMenu();
        }
    }
    
    // Delete chat room
    function deleteChatRoom(chatRoomId) {
        if (confirm('آیا از حذف این چت اطمینان دارید؟')) {
            // Send delete request to server
            $.ajax({
                url: `/Chat/DeleteChatRoom/${chatRoomId}`,
                method: 'DELETE',
                success: function(response) {
                    if (response.success) {
                        // Remove from local array
                        chatRooms = chatRooms.filter(room => (room.id || room.Id) !== chatRoomId);
                        renderChatRooms();
                        
                        // If this was the current chat, go back to welcome
                        if (currentChatId === chatRoomId) {
                            backToWelcome();
                        }
                        
                        console.log('Chat room deleted successfully');
                    } else {
                        alert('خطا در حذف چت: ' + response.message);
                    }
                },
                error: function(xhr, status, error) {
                    console.error('Failed to delete chat room:', error);
                    alert('خطا در حذف چت');
                }
            });
        }
    }
    
    // Show typing indicator
    function showTypingIndicator(text) {
        $('#typingText').text(text);
        $('#typingIndicator').show();
    }
    
    // Hide typing indicator
    function hideTypingIndicator() {
        $('#typingIndicator').hide();
    }
    
    // Update chat room last message
    function updateChatRoomLastMessage(message) {
        const chatRoom = chatRooms.find(room => (room.id || room.Id) === message.chatRoomId);
        if (chatRoom) {
            chatRoom.lastMessage = message.content;
            chatRoom.lastMessageDate = message.sentDate;
            renderChatRooms();
        }
    }
    
    // Update counts
    function updateChatCount() {
        const count = chatRooms.length;
        if (count > 0) {
            $('#chatCount').text(count).show();
        } else {
            $('#chatCount').hide();
        }
    }
    
    function updateContactsCount() {
        const count = contacts.length;
        if (count > 0) {
            $('#contactsCount').text(count).show();
        } else {
            $('#contactsCount').hide();
        }
    }
    
    // Utility functions
    function getAvatarUrl(avatar) {
        if (!avatar || typeof avatar !== 'string') {
            return '/CompanyVariables/avatars/1.png';
        }
        
        if (avatar.startsWith('http')) {
            return avatar;
        }
        
        return `/CompanyVariables/avatars/${avatar}`;
    }
    
    function getTimeAgo(date) {
        if (!date) return '';
        
        const now = new Date();
        const diff = now - new Date(date);
        const minutes = Math.floor(diff / 60000);
        const hours = Math.floor(diff / 3600000);
        const days = Math.floor(diff / 86400000);
        
        if (minutes < 1) return 'همین الان';
        if (minutes < 60) return `${minutes} دقیقه پیش`;
        if (hours < 24) return `${hours} ساعت پیش`;
        if (days < 7) return `${days} روز پیش`;
        
        return new Date(date).toLocaleDateString('fa-IR');
    }
    
    function getStatusClass(status) {
        switch (status) {
            case 1: return 'sent';
            case 2: return 'delivered';
            case 3: return 'read';
            default: return 'sent';
        }
    }
    
    function scrollToBottom() {
        const $chatMessages = $('#chatMessages');
        if ($chatMessages.length) {
            $chatMessages.scrollTop($chatMessages[0].scrollHeight);
        }
    }
    
    // Return public API
    return publicAPI;
})();
