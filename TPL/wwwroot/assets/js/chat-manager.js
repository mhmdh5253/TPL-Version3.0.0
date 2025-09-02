"use strict";

// Chat Manager - Additional functionality for the chat system
class ChatManager {
    constructor() {
        this.currentChatRoomId = null;
        this.currentUser = null;
        this.chatConnection = null;
        this.typingTimer = null;
        this.isTyping = false;
        this.messageQueue = [];
        this.isOnline = navigator.onLine;
        
        this.initialize();
    }

    initialize() {
        this.setupNetworkListeners();
        this.setupKeyboardShortcuts();
        this.setupContextMenu();
        this.setupEmojiPicker();
        this.setupFileDragAndDrop();
        this.setupMessageSearch();
        this.setupNotifications();
    }

    // Network status monitoring
    setupNetworkListeners() {
        window.addEventListener('online', () => {
            this.isOnline = true;
            this.showNotification('آنلاین شدید', 'success');
            this.reconnectSignalR();
        });

        window.addEventListener('offline', () => {
            this.isOnline = false;
            this.showNotification('آفلاین شدید', 'warning');
        });
    }

    // Keyboard shortcuts
    setupKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            // Ctrl/Cmd + Enter to send message
            if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
                e.preventDefault();
                this.sendMessage();
            }

            // Escape to close sidebars
            if (e.key === 'Escape') {
                this.closeAllSidebars();
            }

            // Ctrl/Cmd + K to focus search
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                const searchInput = document.querySelector('.chat-search-input');
                if (searchInput) searchInput.focus();
            }
        });
    }

    // Context menu for messages
    setupContextMenu() {
        document.addEventListener('contextmenu', (e) => {
            const messageElement = e.target.closest('.chat-message');
            if (messageElement) {
                e.preventDefault();
                this.showMessageContextMenu(e, messageElement);
            }
        });
    }

    // Emoji picker
    setupEmojiPicker() {
        const emojiButton = document.createElement('button');
        emojiButton.className = 'btn btn-text-secondary btn-icon rounded-pill me-2';
        emojiButton.innerHTML = '<i class="mdi mdi-emoticon mdi-20px"></i>';
        emojiButton.title = 'انتخاب ایموجی';
        
        emojiButton.addEventListener('click', () => {
            this.showEmojiPicker();
        });

        const messageActions = document.querySelector('.message-actions');
        if (messageActions) {
            messageActions.insertBefore(emojiButton, messageActions.firstChild);
        }
    }

    // File drag and drop
    setupFileDragAndDrop() {
        const chatHistory = document.querySelector('.chat-history-body');
        if (chatHistory) {
            chatHistory.addEventListener('dragover', (e) => {
                e.preventDefault();
                chatHistory.classList.add('drag-over');
            });

            chatHistory.addEventListener('dragleave', () => {
                chatHistory.classList.remove('drag-over');
            });

            chatHistory.addEventListener('drop', (e) => {
                e.preventDefault();
                chatHistory.classList.remove('drag-over');
                
                const files = Array.from(e.dataTransfer.files);
                files.forEach(file => this.handleDroppedFile(file));
            });
        }
    }

    // Message search functionality
    setupMessageSearch() {
        const searchButton = document.querySelector('.chat-history-header .mdi-magnify');
        if (searchButton) {
            searchButton.addEventListener('click', () => {
                this.showMessageSearchModal();
            });
        }
    }

    // Browser notifications
    setupNotifications() {
        if ('Notification' in window) {
            Notification.requestPermission();
        }
    }

    // Send message with enhanced functionality
    async sendMessage(content = null) {
        const messageInput = document.querySelector('.message-input');
        const messageContent = content || messageInput?.value?.trim();
        
        if (!messageContent || !this.currentChatRoomId) return;

        try {
            // Add message to UI immediately (optimistic update)
            const tempMessage = this.createTempMessage(messageContent);
            this.addMessageToChat(tempMessage);

            // Clear input
            if (messageInput) messageInput.value = '';

            // Send to server
            const messageData = {
                content: messageContent,
                chatRoomId: this.currentChatRoomId,
                messageType: 0, // Text message
                replyToMessageId: null
            };

            const response = await fetch('/Chat/SendMessage', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(messageData)
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    // Update temp message with real data
                    this.updateTempMessage(tempMessage, result.data);
                    
                    // Send via SignalR if available
                    if (this.chatConnection && this.chatConnection.state === "Connected") {
                        this.chatConnection.invoke("SendMessageToGroup", this.currentChatRoomId.toString(), result.data);
                    }
                }
            } else {
                // Remove temp message if failed
                tempMessage.remove();
                this.showNotification('خطا در ارسال پیام', 'error');
            }
        } catch (error) {
            console.error("Error sending message: ", error);
            this.showNotification('خطا در ارسال پیام', 'error');
        }
    }

    // Create temporary message for optimistic updates
    createTempMessage(content) {
        const messageElement = document.createElement('li');
        messageElement.className = 'chat-message chat-message-right temp-message';
        messageElement.innerHTML = `
            <div class="d-flex overflow-hidden">
                <div class="chat-message-wrapper flex-grow-1">
                    <div class="chat-message-text">
                        <p class="mb-0">${content}</p>
                    </div>
                    <div class="text-end text-muted">
                        <i class="mdi mdi-clock-outline mdi-14px me-1"></i>
                        <small>در حال ارسال...</small>
                    </div>
                </div>
                <div class="user-avatar flex-shrink-0 ms-3">
                    <div class="avatar avatar-sm">
                        <img src="${this.currentUser?.avatar || '/assets/img/avatars/1.png'}" alt="Avatar" class="rounded-circle">
                    </div>
                </div>
            </div>
        `;
        return messageElement;
    }

    // Add message to chat
    addMessageToChat(messageElement) {
        const chatHistory = document.querySelector('.chat-history');
        if (chatHistory) {
            chatHistory.appendChild(messageElement);
            this.scrollToBottom();
        }
    }

    // Update temporary message with real data
    updateTempMessage(tempElement, realMessage) {
        if (tempElement) {
            tempElement.classList.remove('temp-message');
            tempElement.setAttribute('data-message-id', realMessage.id);
            
            const statusElement = tempElement.querySelector('.mdi-clock-outline');
            if (statusElement) {
                statusElement.className = 'mdi mdi-check-all mdi-14px text-success me-1';
            }
            
            const timeElement = tempElement.querySelector('small');
            if (timeElement) {
                timeElement.textContent = this.formatTime(realMessage.sentDate);
            }
        }
    }

    // Handle dropped files
    async handleDroppedFile(file) {
        if (!this.currentChatRoomId) {
            this.showNotification('ابتدا یک چت را انتخاب کنید', 'warning');
            return;
        }

        try {
            const formData = new FormData();
            formData.append('file', file);
            formData.append('chatRoomId', this.currentChatRoomId);

            const response = await fetch('/Chat/UploadFile', {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    await this.sendMessage(`فایل: ${file.name}`, 1);
                    this.showNotification('فایل با موفقیت آپلود شد', 'success');
                }
            }
        } catch (error) {
            console.error("Error uploading dropped file: ", error);
            this.showNotification('خطا در آپلود فایل', 'error');
        }
    }

    // Show emoji picker
    showEmojiPicker() {
        const emojis = ['😀', '😃', '😄', '😁', '😆', '😅', '😂', '🤣', '😊', '😇', '🙂', '🙃', '😉', '😌', '😍', '🥰', '😘', '😗', '😙', '😚', '😋', '😛', '😝', '😜', '🤪', '🤨', '🧐', '🤓', '😎', '🤩', '🥳', '😏', '😒', '😞', '😔', '😟', '😕', '🙁', '☹️', '😣', '😖', '😫', '😩', '🥺', '😢', '😭', '😤', '😠', '😡', '🤬', '🤯', '😳', '🥵', '🥶', '😱', '😨', '😰', '😥', '😓', '🤗', '🤔', '🤭', '🤫', '🤥', '😶', '😐', '😑', '😯', '😦', '😧', '😮', '😲', '🥱', '😴', '🤤', '😪', '😵', '🤐', '🥴', '🤢', '🤮', '🤧', '😷', '🤒', '🤕', '🤑', '🤠', '💩', '👻', '💀', '☠️', '👽', '👾', '🤖', '🎃', '👺', '👹', '👿', '😈', '🤡', '👹', '👺', '💋', '💌', '💘', '💝', '💖', '💗', '💓', '💞', '💕', '💟', '❣️', '💔', '❤️', '🧡', '💛', '💚', '💙', '💜', '🖤', '💯', '💢', '💥', '💫', '💦', '💨', '🕳️', '💬', '🗨️', '🗯️', '💭', '💤'];

        const picker = document.createElement('div');
        picker.className = 'emoji-picker';
        picker.style.cssText = `
            position: absolute;
            top: 100%;
            left: 0;
            background: white;
            border: 1px solid #ddd;
            border-radius: 8px;
            padding: 10px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            z-index: 1000;
            max-width: 300px;
            display: grid;
            grid-template-columns: repeat(8, 1fr);
            gap: 5px;
        `;

        emojis.forEach(emoji => {
            const button = document.createElement('button');
            button.textContent = emoji;
            button.className = 'btn btn-sm btn-text-secondary';
            button.style.cssText = 'font-size: 20px; padding: 5px; min-width: auto;';
            
            button.addEventListener('click', () => {
                const messageInput = document.querySelector('.message-input');
                if (messageInput) {
                    messageInput.value += emoji;
                    messageInput.focus();
                }
                picker.remove();
            });
            
            picker.appendChild(button);
        });

        const emojiButton = document.querySelector('.mdi-emoticon').closest('button');
        emojiButton.style.position = 'relative';
        emojiButton.appendChild(picker);

        // Close picker when clicking outside
        document.addEventListener('click', (e) => {
            if (!picker.contains(e.target) && !emojiButton.contains(e.target)) {
                picker.remove();
            }
        });
    }

    // Show message context menu
    showMessageContextMenu(event, messageElement) {
        const messageId = messageElement.getAttribute('data-message-id');
        if (!messageId) return;

        const menu = document.createElement('div');
        menu.className = 'context-menu';
        menu.style.cssText = `
            position: fixed;
            top: ${event.clientY}px;
            left: ${event.clientX}px;
            background: white;
            border: 1px solid #ddd;
            border-radius: 4px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            z-index: 1000;
            min-width: 150px;
        `;

        const menuItems = [
            { text: 'کپی', icon: 'mdi-content-copy', action: () => this.copyMessageText(messageElement) },
            { text: 'پاسخ', icon: 'mdi-reply', action: () => this.replyToMessage(messageId) },
            { text: 'فوروارد', icon: 'mdi-forward', action: () => this.forwardMessage(messageId) },
            { text: 'ویرایش', icon: 'mdi-pencil', action: () => this.editMessage(messageId) },
            { text: 'حذف', icon: 'mdi-delete', action: () => this.deleteMessage(messageId) }
        ];

        menuItems.forEach(item => {
            const menuItem = document.createElement('div');
            menuItem.className = 'context-menu-item';
            menuItem.style.cssText = 'padding: 8px 12px; cursor: pointer; display: flex; align-items: center; gap: 8px;';
            menuItem.innerHTML = `<i class="mdi ${item.icon}"></i> ${item.text}`;
            
            menuItem.addEventListener('click', () => {
                item.action();
                menu.remove();
            });
            
            menuItem.addEventListener('mouseenter', () => {
                menuItem.style.backgroundColor = '#f5f5f5';
            });
            
            menuItem.addEventListener('mouseleave', () => {
                menuItem.style.backgroundColor = 'transparent';
            });
            
            menu.appendChild(menuItem);
        });

        document.body.appendChild(menu);

        // Close menu when clicking outside
        document.addEventListener('click', () => menu.remove(), { once: true });
    }

    // Message actions
    copyMessageText(messageElement) {
        const text = messageElement.querySelector('.chat-message-text p')?.textContent;
        if (text) {
            navigator.clipboard.writeText(text);
            this.showNotification('متن کپی شد', 'success');
        }
    }

    replyToMessage(messageId) {
        // Implementation for reply functionality
        console.log('Reply to message:', messageId);
    }

    forwardMessage(messageId) {
        // Implementation for forward functionality
        console.log('Forward message:', messageId);
    }

    editMessage(messageId) {
        // Implementation for edit functionality
        console.log('Edit message:', messageId);
    }

    async deleteMessage(messageId) {
        if (!confirm('آیا از حذف این پیام اطمینان دارید؟')) return;

        try {
            const response = await fetch('/Chat/DeleteMessage', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(messageId)
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    const messageElement = document.querySelector(`[data-message-id="${messageId}"]`);
                    if (messageElement) {
                        messageElement.remove();
                        this.showNotification('پیام حذف شد', 'success');
                    }
                }
            }
        } catch (error) {
            console.error("Error deleting message: ", error);
            this.showNotification('خطا در حذف پیام', 'error');
        }
    }

    // Utility functions
    scrollToBottom() {
        const chatHistoryBody = document.querySelector('.chat-history-body');
        if (chatHistoryBody) {
            chatHistoryBody.scrollTo(0, chatHistoryBody.scrollHeight);
        }
    }

    formatTime(dateString) {
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

    showNotification(message, type = 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 12px 20px;
            border-radius: 4px;
            color: white;
            z-index: 9999;
            max-width: 300px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            transform: translateX(100%);
            transition: transform 0.3s ease;
        `;

        // Set background color based on type
        const colors = {
            success: '#28a745',
            error: '#dc3545',
            warning: '#ffc107',
            info: '#17a2b8'
        };
        notification.style.backgroundColor = colors[type] || colors.info;

        notification.textContent = message;
        document.body.appendChild(notification);

        // Animate in
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
        }, 100);

        // Auto remove after 5 seconds
        setTimeout(() => {
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => notification.remove(), 300);
        }, 5000);

        // Browser notification if supported
        if ('Notification' in window && Notification.permission === 'granted') {
            new Notification('چت', { body: message });
        }
    }

    closeAllSidebars() {
        const sidebars = document.querySelectorAll('.app-sidebar');
        sidebars.forEach(sidebar => {
            if (sidebar.classList.contains('show')) {
                sidebar.classList.remove('show');
            }
        });
    }

    reconnectSignalR() {
        if (this.chatConnection && this.chatConnection.state !== "Connected") {
            this.chatConnection.start().catch(err => {
                console.error("SignalR reconnection failed:", err);
            });
        }
    }

    // Public methods for external use
    setCurrentChatRoom(chatRoomId) {
        this.currentChatRoomId = chatRoomId;
        this.enableMessageInput();
    }

    setCurrentUser(user) {
        this.currentUser = user;
    }

    enableMessageInput() {
        const messageInput = document.querySelector('.message-input');
        const sendBtn = document.querySelector('.send-msg-btn');
        
        if (messageInput) messageInput.disabled = false;
        if (sendBtn) sendBtn.disabled = false;
    }

    disableMessageInput() {
        const messageInput = document.querySelector('.message-input');
        const sendBtn = document.querySelector('.send-msg-btn');
        
        if (messageInput) messageInput.disabled = true;
        if (sendBtn) sendBtn.disabled = true;
    }
}

// Initialize chat manager when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    window.chatManager = new ChatManager();
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ChatManager;
}
