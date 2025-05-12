// chatbot.js - JavaScript để xử lý tương tác với chatbot
document.addEventListener('DOMContentLoaded', function () {
    // Các phần tử DOM
    const chatButton = document.getElementById('chatButton');
    const chatWidget = document.getElementById('chatWidget');
    const chatToggle = document.getElementById('chatToggle');
    const chatBody = document.getElementById('chatBody');
    const chatInput = document.getElementById('chatInput');
    const chatSend = document.getElementById('chatSend');

    // Trạng thái
    let chatHistory = '';
    let isProcessing = false;

    // Hiển thị/ẩn chat widget
    if (chatButton && chatWidget && chatToggle) {
        chatButton.addEventListener('click', function () {
            chatWidget.style.display = 'flex';
            chatButton.style.display = 'none';
            scrollToBottom();
        });

        chatToggle.addEventListener('click', function () {
            chatWidget.style.display = 'none';
            chatButton.style.display = 'flex';
        });
    }

    // Gửi tin nhắn
    if (chatSend && chatInput) {
        chatSend.addEventListener('click', sendMessage);
        chatInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                sendMessage();
            }
        });
    }

    function sendMessage() {
        // Kiểm tra trạng thái đang xử lý
        if (isProcessing) return;

        const message = chatInput.value.trim();
        if (!message) return;

        // Thêm tin nhắn của người dùng vào khung chat
        appendMessage(message, 'user');
        chatInput.value = '';

        // Hiển thị trạng thái đang xử lý
        isProcessing = true;
        const typingMessageId = showTypingIndicator();

        // Chuẩn bị dữ liệu gửi đi
        const requestData = {
            message: message,
            chatHistory: chatHistory
        };

        // Lấy CSRF token từ form (nếu có)
        let token = "";
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenElement) {
            token = tokenElement.value;
        }

        // Gửi tin nhắn đến server
        fetch('/Chatbot/SendMessage', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(requestData)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Lỗi mạng');
                }
                return response.json();
            })
            .then(data => {
                // Xóa chỉ báo đang nhập
                removeTypingIndicator(typingMessageId);
                isProcessing = false;

                if (data.success) {
                    // Xử lý phản hồi
                    handleBotResponse(data.message);

                    // Cập nhật lịch sử chat
                    chatHistory += `User: ${message}\nBot: ${data.message}\n`;
                } else {
                    appendMessage('Xin lỗi, có lỗi xảy ra. Vui lòng thử lại sau.', 'bot');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                removeTypingIndicator(typingMessageId);
                isProcessing = false;
                appendMessage('Xin lỗi, có lỗi xảy ra khi kết nối với chatbot.', 'bot');
            });
    }

    function handleBotResponse(message) {
        // Kiểm tra xem phản hồi có chứa đường dẫn sản phẩm không
        const lines = message.split('\n');
        let processedMessage = '';

        for (let line of lines) {
            // Chuyển đổi đường dẫn sản phẩm thành liên kết
            if (line.includes('/Product/Details/')) {
                const urlMatch = line.match(/\/Product\/Details\/\d+/);
                if (urlMatch) {
                    const url = urlMatch[0];
                    const linkText = line.trim().replace('Xem chi tiết: ' + url, '');
                    processedMessage += `<a href="${url}" target="_blank" class="chat-link">👉 Xem sản phẩm</a>\n`;
                    continue;
                }
            }
            processedMessage += line + '\n';
        }

        appendMessage(processedMessage, 'bot', true);
    }

    function appendMessage(message, sender, allowHtml = false) {
        const messageDiv = document.createElement('div');
        messageDiv.classList.add('chat-message', sender);
        messageDiv.id = 'msg-' + Date.now();

        const bubbleDiv = document.createElement('div');
        bubbleDiv.classList.add('chat-bubble');

        if (allowHtml) {
            bubbleDiv.innerHTML = message;
        } else {
            bubbleDiv.textContent = message;
        }

        messageDiv.appendChild(bubbleDiv);
        chatBody.appendChild(messageDiv);

        scrollToBottom();
        return messageDiv.id;
    }

    function showTypingIndicator() {
        const typingDiv = document.createElement('div');
        typingDiv.classList.add('chat-message', 'bot', 'typing-indicator');
        typingDiv.id = 'typing-' + Date.now();

        const bubbleDiv = document.createElement('div');
        bubbleDiv.classList.add('chat-bubble', 'typing-bubble');

        const dotContainer = document.createElement('div');
        dotContainer.classList.add('typing-dots');

        for (let i = 0; i < 3; i++) {
            const dot = document.createElement('span');
            dot.classList.add('typing-dot');
            dotContainer.appendChild(dot);
        }

        bubbleDiv.appendChild(dotContainer);
        typingDiv.appendChild(bubbleDiv);
        chatBody.appendChild(typingDiv);

        scrollToBottom();
        return typingDiv.id;
    }

    function removeTypingIndicator(id) {
        const element = document.getElementById(id);
        if (element) {
            chatBody.removeChild(element);
        }
    }

    function scrollToBottom() {
        if (chatBody) {
            chatBody.scrollTop = chatBody.scrollHeight;
        }
    }

    // Lưu trữ lịch sử chat trong localStorage
    function saveChat() {
        if (chatBody && chatBody.innerHTML) {
            localStorage.setItem('fashionShopChatHistory', chatBody.innerHTML);
            localStorage.setItem('fashionShopChatText', chatHistory);
        }
    }

    // Khôi phục lịch sử chat từ localStorage
    function loadChat() {
        const savedHTML = localStorage.getItem('fashionShopChatHistory');
        const savedText = localStorage.getItem('fashionShopChatText');

        if (savedHTML && chatBody) {
            chatBody.innerHTML = savedHTML;
        }

        if (savedText) {
            chatHistory = savedText;
        }
    }

    // Lưu chat khi đóng tab hoặc rời trang
    window.addEventListener('beforeunload', saveChat);

    // Kiểm tra và tải lịch sử chat khi mở trang
    loadChat();

    // Tự động hiển thị chatbot sau 30 giây (có thể tùy chỉnh hoặc tắt)
    setTimeout(function () {
        // Chỉ hiển thị nếu người dùng chưa tương tác với chatbot
        if (chatButton && chatWidget.style.display === 'none' && chatHistory === '') {
            chatButton.click();
        }
    }, 30000);

    // Thêm các hàm hỗ trợ

    // Xóa lịch sử chat
    function clearChat() {
        if (chatBody) {
            // Giữ lại tin nhắn chào mừng đầu tiên
            const welcomeMessage = chatBody.querySelector('.chat-message.bot:first-child');
            chatBody.innerHTML = '';
            if (welcomeMessage) {
                chatBody.appendChild(welcomeMessage);
            }
            chatHistory = '';
            localStorage.removeItem('fashionShopChatHistory');
            localStorage.removeItem('fashionShopChatText');
        }
    }

    // Thêm sự kiện cho nút xóa lịch sử chat (nếu cần)
    const resetChatButton = document.getElementById('resetChat');
    if (resetChatButton) {
        resetChatButton.addEventListener('click', clearChat);
    }

    // Hỗ trợ thêm các câu hỏi gợi ý
    function setupSuggestedQuestions() {
        const suggestionButtons = document.querySelectorAll('.chat-suggestion');
        if (suggestionButtons) {
            suggestionButtons.forEach(button => {
                button.addEventListener('click', function () {
                    const question = this.textContent;
                    if (chatInput) {
                        chatInput.value = question;
                        chatSend.click();
                    }
                });
            });
        }
    }

    // Khởi tạo các câu hỏi gợi ý nếu có
    setupSuggestedQuestions();
});