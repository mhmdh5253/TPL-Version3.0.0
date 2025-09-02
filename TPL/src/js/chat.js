// TPL Chat System - JavaScript Module
'use strict';

// Import dependencies
import { showNotification } from './app.js';

// Chat module
class ChatManager {
    constructor() {
        this.currentChatRoom = null;
        this.messageQueue = [];
        this.isConnected = false;
        this.autoScroll = true;
        
        this.init();
    }
    
    init() {
        this.bindEvents();
        this.initializePerfectScrollbar();
        this.setupFileUpload();
        this.setupVoiceRecording();
    }
    
    bindEvents() {
        // Chat room selection
        document.addEventListener('click', (e) => {
            if (e.target.closest('.chat-contact-list-item a')) {
                this.handleChatRoomSelection(e);
            }
        });
        
        // Message form submission
        const messageForm = document.querySelector('.form-send-message');
        if (messageForm) {
            messageForm.addEventListener('submit', (e) => this.handleMessageSubmit(e));
        }
        
        // File attachment
        const fileInput = document.getElementById('attach-doc');
        if (fileInput) {
            fileInput.addEventListener('change', (e) => this.handleFileAttachment(e));
        }
        
        // Voice recording
        const voiceButton = document.querySelector('.voice-record-btn');
        if (voiceButton) {
            voiceButton.addEventListener('click', (e) => this.handleVoiceRecording(e));
        }
        
        // Search functionality
        const searchInput = document.querySelector('.chat-search-input');
        if (searchInput) {
            searchInput.addEventListener('input', (e) => this.handleSearch(e));
        }
    }
    
    initializePerfectScrollbar() {
        // Initialize perfect scrollbar for chat areas
        const chatAreas = document.querySelectorAll('.ps');
        chatAreas.forEach(area => {
            if (typeof PerfectScrollbar !== 'undefined') {
                new PerfectScrollbar(area);
            }
        });
    }
    
    setupFileUpload() {
        // Drag and drop file upload
        const dropZone = document.querySelector('.chat-history-body');
        if (dropZone) {
            dropZone.addEventListener('dragover', (e) => {
                e.preventDefault();
                dropZone.classList.add('drag-over');
            });
            
            dropZone.addEventListener('dragleave', () => {
                dropZone.classList.remove('drag-over');
            });
            
            dropZone.addEventListener('drop', (e) => {
                e.preventDefault();
                dropZone.classList.remove('drag-over');
                const files = e.dataTransfer.files;
                this.handleFileDrop(files);
            });
        }
    }
    
    setupVoiceRecording() {
        // Voice recording setup
        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
            const voiceButton = document.querySelector('.voice-record-btn');
            if (voiceButton) {
                voiceButton.addEventListener('mousedown', () => this.startVoiceRecording());
                voiceButton.addEventListener('mouseup', () => this.stopVoiceRecording());
                voiceButton.addEventListener('mouseleave', () => this.stopVoiceRecording());
            }
        }
    }
    
    handleChatRoomSelection(event) {
        const link = event.target.closest('a');
        const chatRoomId = link.getAttribute('href').split('/').pop();
        
        // Update active state
        document.querySelectorAll('.chat-contact-list-item').forEach(item => {
            item.classList.remove('active');
        });
        link.closest('.chat-contact-list-item').classList.add('active');
        
        // Load chat room
        this.loadChatRoom(chatRoomId);
    }
    
    async loadChatRoom(chatRoomId) {
        try {
            const response = await fetch(`/Chat/Room/${chatRoomId}`);
            if (response.ok) {
                // Update URL without page reload
                window.history.pushState({}, '', `/Chat/Room/${chatRoomId}`);
                
                // Load messages
                this.loadMessages(chatRoomId);
            } else {
                showNotification('خطا در بارگذاری چت', 'error');
            }
        } catch (error) {
            console.error('Error loading chat room:', error);
            showNotification('خطا در بارگذاری چت', 'error');
        }
    }
    
    async loadMessages(chatRoomId) {
        try {
            const response = await fetch(`/Chat/GetMessages/${chatRoomId}`);
            if (response.ok) {
                const messages = await response.json();
                this.displayMessages(messages);
                this.scrollToBottom();
            }
        } catch (error) {
            console.error('Error loading messages:', error);
        }
    }
    
    displayMessages(messages) {
        const chatHistory = document.querySelector('.chat-history');
        if (!chatHistory) return;
        
        chatHistory.innerHTML = '';
        
        messages.forEach(message => {
            const messageElement = this.createMessageElement(message);
            chatHistory.appendChild(messageElement);
        });
    }
    
    createMessageElement(message) {
        const li = document.createElement('li');
        li.className = `chat-message ${message.isOwnMessage ? 'chat-message-right' : 'chat-message-left'}`;
        
        li.innerHTML = `
            <div class="d-flex overflow-hidden">
                ${!message.isOwnMessage ? `
                    <div class="chat-message-wrapper flex-shrink-0 me-3">
                        <div class="chat-message-avatar">
                            <div class="avatar ${message.senderIsOnline ? 'avatar-online' : 'avatar-offline'}">
                                <img src="/CompanyVariables/avatars/${message.senderAvatar || '1.png'}" alt="Avatar" class="rounded-circle">
                            </div>
                        </div>
                    </div>
                ` : ''}
                <div class="chat-message-wrapper flex-grow-1">
                    <div class="chat-message-content">
                        ${!message.isOwnMessage ? `
                            <div class="chat-message-header mb-1">
                                <span class="chat-message-author">${message.senderName}</span>
                                <small class="chat-message-time text-muted">${this.formatTime(message.sentDate)}</small>
                            </div>
                        ` : ''}
                        <div class="chat-message-text">
                            ${this.formatMessageContent(message)}
                        </div>
                        ${message.isOwnMessage ? `
                            <div class="chat-message-footer mt-1 text-end">
                                <small class="text-muted">${this.formatTime(message.sentDate)}</small>
                                ${this.getStatusIcon(message.status)}
                            </div>
                        ` : ''}
                    </div>
                </div>
            </div>
        `;
        
        return li;
    }
    
    formatMessageContent(message) {
        switch (message.messageType) {
            case 'Text':
                return `<p class="mb-0">${message.content}</p>`;
            case 'Image':
                return `<img src="${message.filePath}" alt="تصویر" class="img-fluid rounded" style="max-width: 200px;">`;
            case 'File':
                return `
                    <div class="file-message">
                        <i class="mdi mdi-file me-2"></i>
                        <a href="${message.filePath}" target="_blank">${message.fileName}</a>
                        <small class="text-muted d-block">${message.fileSize ? `${Math.round(message.fileSize / 1024)} KB` : ''}</small>
                    </div>
                `;
            case 'Voice':
                return `
                    <div class="voice-message">
                        <i class="mdi mdi-microphone me-2"></i>
                        <audio controls>
                            <source src="${message.voicePath}" type="audio/mpeg">
                            مرورگر شما از پخش صدا پشتیبانی نمی‌کند.
                        </audio>
                        <small class="text-muted d-block">${message.voiceDuration ? `${message.voiceDuration} ثانیه` : ''}</small>
                    </div>
                `;
            default:
                return `<p class="mb-0">${message.content}</p>`;
        }
    }
    
    getStatusIcon(status) {
        switch (status) {
            case 'Read':
                return '<i class="mdi mdi-check-all text-primary ms-1" title="خوانده شده"></i>';
            case 'Delivered':
                return '<i class="mdi mdi-check text-muted ms-1" title="تحویل داده شده"></i>';
            default:
                return '<i class="mdi mdi-clock text-muted ms-1" title="در حال ارسال"></i>';
        }
    }
    
    formatTime(dateString) {
        const date = new Date(dateString);
        return date.toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' });
    }
    
    async handleMessageSubmit(event) {
        event.preventDefault();
        
        const form = event.target;
        const formData = new FormData(form);
        
        try {
            const response = await fetch('/Chat/SendMessage', {
                method: 'POST',
                body: formData
            });
            
            if (response.ok) {
                // Clear form
                form.reset();
                
                // Reload messages
                const chatRoomId = formData.get('ChatRoomId');
                if (chatRoomId) {
                    this.loadMessages(chatRoomId);
                }
            } else {
                showNotification('خطا در ارسال پیام', 'error');
            }
        } catch (error) {
            console.error('Error sending message:', error);
            showNotification('خطا در ارسال پیام', 'error');
        }
    }
    
    handleFileAttachment(event) {
        const file = event.target.files[0];
        if (file) {
            this.uploadFile(file);
        }
    }
    
    handleFileDrop(files) {
        Array.from(files).forEach(file => {
            this.uploadFile(file);
        });
    }
    
    async uploadFile(file) {
        const formData = new FormData();
        formData.append('file', file);
        
        const chatRoomId = document.querySelector('input[name="ChatRoomId"]')?.value;
        if (chatRoomId) {
            formData.append('chatRoomId', chatRoomId);
        }
        
        try {
            const response = await fetch('/Chat/SendFileMessage', {
                method: 'POST',
                body: formData
            });
            
            if (response.ok) {
                showNotification('فایل با موفقیت ارسال شد', 'success');
                // Reload messages
                if (chatRoomId) {
                    this.loadMessages(chatRoomId);
                }
            } else {
                showNotification('خطا در ارسال فایل', 'error');
            }
        } catch (error) {
            console.error('Error uploading file:', error);
            showNotification('خطا در ارسال فایل', 'error');
        }
    }
    
    startVoiceRecording() {
        // Voice recording implementation
        console.log('Voice recording started');
    }
    
    stopVoiceRecording() {
        // Voice recording stop implementation
        console.log('Voice recording stopped');
    }
    
    handleSearch(event) {
        const searchTerm = event.target.value.toLowerCase();
        const chatItems = document.querySelectorAll('.chat-contact-list-item:not(.chat-contact-list-item-title)');
        
        chatItems.forEach(item => {
            const name = item.querySelector('.chat-contact-name')?.textContent.toLowerCase() || '';
            const status = item.querySelector('.chat-contact-status')?.textContent.toLowerCase() || '';
            
            if (name.includes(searchTerm) || status.includes(searchTerm)) {
                item.style.display = '';
            } else {
                item.style.display = 'none';
            }
        });
    }
    
    scrollToBottom() {
        if (this.autoScroll) {
            const chatBody = document.querySelector('.chat-history-body');
            if (chatBody) {
                chatBody.scrollTop = chatBody.scrollHeight;
            }
        }
    }
    
    // Public methods
    setAutoScroll(enabled) {
        this.autoScroll = enabled;
    }
    
    refreshChat() {
        const chatRoomId = window.location.pathname.split('/').pop();
        if (chatRoomId && !isNaN(chatRoomId)) {
            this.loadMessages(chatRoomId);
        }
    }
}

// Initialize chat manager when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    if (document.querySelector('.app-chat')) {
        window.chatManager = new ChatManager();
    }
});

// Export for use in other modules
export default ChatManager;

