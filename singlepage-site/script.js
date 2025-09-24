/*
=========================
TPL Website Scripts v4.0
=========================
*/

(function() {
    'use strict';
    
    // DOM Content Loaded Event
    document.addEventListener('DOMContentLoaded', function() {
        initNavigation();
        initSmoothScroll();
        initAnimations();
        initContactForm();
        initModal();
        initBackToTop();
        initTypingEffect();
        initParallax();
        
        console.log('TPL Website v4.0 - Loaded Successfully!');
    });
    
    // Navigation Enhancement
    function initNavigation() {
        const navbar = document.querySelector('.navbar');
        const navLinks = document.querySelectorAll('.nav-link');
        
        // Navbar scroll effect
        window.addEventListener('scroll', function() {
            if (window.scrollY > 50) {
                navbar.classList.add('scrolled');
            } else {
                navbar.classList.remove('scrolled');
            }
        });
        
        // Active nav link highlighting
        window.addEventListener('scroll', function() {
            const sections = document.querySelectorAll('section[id]');
            const scrollPos = window.scrollY + 100;
            
            sections.forEach(function(section) {
                const sectionTop = section.offsetTop;
                const sectionHeight = section.offsetHeight;
                const sectionId = section.getAttribute('id');
                
                if (scrollPos >= sectionTop && scrollPos < sectionTop + sectionHeight) {
                    navLinks.forEach(function(link) {
                        link.classList.remove('active');
                        if (link.getAttribute('href') === '#' + sectionId) {
                            link.classList.add('active');
                        }
                    });
                }
            });
        });
        
        // Mobile menu close on link click
        navLinks.forEach(function(link) {
            link.addEventListener('click', function() {
                const navbarCollapse = document.querySelector('.navbar-collapse');
                if (navbarCollapse.classList.contains('show')) {
                    const bsCollapse = new bootstrap.Collapse(navbarCollapse);
                    bsCollapse.hide();
                }
            });
        });
    }
    
    // Smooth Scrolling
    function initSmoothScroll() {
        const links = document.querySelectorAll('a[href^="#"]');
        
        links.forEach(function(link) {
            link.addEventListener('click', function(e) {
                const targetId = this.getAttribute('href');
                if (targetId === '#') return;
                
                const targetSection = document.querySelector(targetId);
                if (targetSection) {
                    e.preventDefault();
                    
                    const offsetTop = targetSection.offsetTop - 80;
                    window.scrollTo({
                        top: offsetTop,
                        behavior: 'smooth'
                    });
                }
            });
        });
    }
    
    // Scroll Animations
    function initAnimations() {
        const observer = new IntersectionObserver(function(entries) {
            entries.forEach(function(entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate');
                    
                    // Counter animation for statistics
                    if (entry.target.classList.contains('hero-stats')) {
                        animateCounters();
                    }
                }
            });
        }, {
            threshold: 0.2,
            rootMargin: '0px 0px -50px 0px'
        });
        
        // Observe elements for animation
        const animateElements = document.querySelectorAll('.feature-card, .screenshot-item, .pricing-card, .hero-stats');
        animateElements.forEach(function(el) {
            el.classList.add('animate-on-scroll');
            observer.observe(el);
        });
    }
    
    // Counter Animation
    function animateCounters() {
        const counters = document.querySelectorAll('.hero-stats h3');
        
        counters.forEach(function(counter) {
            const target = parseInt(counter.textContent);
            let current = 0;
            const increment = target / 50;
            const duration = 2000;
            const stepTime = duration / 50;
            
            const timer = setInterval(function() {
                current += increment;
                if (current >= target) {
                    current = target;
                    clearInterval(timer);
                }
                counter.textContent = Math.floor(current) + '%';
            }, stepTime);
        });
    }
    
    // Contact Form Handler
    function initContactForm() {
        const form = document.querySelector('.contact-form form');
        
        if (form) {
            form.addEventListener('submit', function(e) {
                e.preventDefault();
                
                // Form validation
                const formData = new FormData(form);
                const firstName = form.querySelector('#firstName').value.trim();
                const lastName = form.querySelector('#lastName').value.trim();
                const email = form.querySelector('#email').value.trim();
                const phone = form.querySelector('#phone').value.trim();
                const message = form.querySelector('#message').value.trim();
                
                // Basic validation
                if (!firstName || !lastName || !email || !phone || !message) {
                    showNotification('لطفاً تمام فیلدهای ضروری را پر کنید.', 'error');
                    return;
                }
                
                if (!isValidEmail(email)) {
                    showNotification('لطفاً آدرس ایمیل معتبر وارد کنید.', 'error');
                    return;
                }
                
                if (!isValidPhone(phone)) {
                    showNotification('لطفاً شماره تماس معتبر وارد کنید.', 'error');
                    return;
                }
                
                // Submit form (simulate API call)
                const submitBtn = form.querySelector('button[type="submit"]');
                const originalText = submitBtn.innerHTML;
                
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> در حال ارسال...';
                submitBtn.disabled = true;
                
                // Simulate API call
                setTimeout(function() {
                    // Success simulation
                    showNotification('پیام شما با موفقیت ارسال شد. بزودی با شما تماس خواهیم گرفت.', 'success');
                    form.reset();
                    
                    submitBtn.innerHTML = originalText;
                    submitBtn.disabled = false;
                }, 2000);
            });
        }
    }
    
    // Notification System
    function showNotification(message, type = 'info') {
        // Create notification container if it doesn't exist
        let container = document.querySelector('.notification-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'notification-container';
            container.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 9999;
                max-width: 400px;
            `;
            document.body.appendChild(container);
        }
        
        // Create notification element
        const notification = document.createElement('div');
        const bgColor = type === 'success' ? '#10b981' : type === 'error' ? '#ef4444' : '#3b82f6';
        const icon = type === 'success' ? 'fa-check-circle' : type === 'error' ? 'fa-exclamation-circle' : 'fa-info-circle';
        
        notification.innerHTML = `
            <div style="
                background: ${bgColor};
                color: white;
                padding: 1rem 1.5rem;
                border-radius: 0.5rem;
                box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
                margin-bottom: 1rem;
                display: flex;
                align-items: center;
                animation: slideInRight 0.3s ease-out;
            ">
                <i class="fas ${icon}" style="margin-left: 0.75rem; font-size: 1.25rem;"></i>
                <span>${message}</span>
                <button onclick="this.parentElement.parentElement.remove()" style="
                    background: none;
                    border: none;
                    color: white;
                    margin-right: auto;
                    margin-left: 0.75rem;
                    cursor: pointer;
                    font-size: 1.25rem;
                ">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;
        
        container.appendChild(notification);
        
        // Auto remove after 5 seconds
        setTimeout(() => {
            if (notification.parentElement) {
                notification.remove();
            }
        }, 5000);
    }
    
    // Add slide in animation CSS
    const style = document.createElement('style');
    style.textContent = `
        @keyframes slideInRight {
            from {
                transform: translateX(100%);
                opacity: 0;
            }
            to {
                transform: translateX(0);
                opacity: 1;
            }
        }
    `;
    document.head.appendChild(style);
    
    // Email validation
    function isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }
    
    // Phone validation (Iranian phone numbers)
    function isValidPhone(phone) {
        const phoneRegex = /^(\+98|0)?9\d{9}$/;
        return phoneRegex.test(phone.replace(/\s/g, ''));
    }
    
    // Modal Handler
    function initModal() {
        const demoButtons = document.querySelectorAll('a[href="#demo"]');
        
        demoButtons.forEach(function(btn) {
            btn.addEventListener('click', function(e) {
                e.preventDefault();
                
                // Create or show demo modal
                let modal = document.querySelector('#demoModal');
                if (modal) {
                    const bsModal = new bootstrap.Modal(modal);
                    bsModal.show();
                } else {
                    showNotification('نسخه آزمایشی به زودی در دسترس قرار خواهد گرفت.', 'info');
                }
            });
        });
    }
    
    // Back to Top Button
    function initBackToTop() {
        // Create back to top button
        const backToTop = document.createElement('button');
        backToTop.innerHTML = '<i class="fas fa-arrow-up"></i>';
        backToTop.className = 'back-to-top';
        backToTop.style.cssText = `
            position: fixed;
            bottom: 30px;
            left: 30px;
            background: var(--primary-color);
            color: white;
            border: none;
            width: 50px;
            height: 50px;
            border-radius: 50%;
            font-size: 1.25rem;
            cursor: pointer;
            opacity: 0;
            visibility: hidden;
            transition: all 0.3s ease;
            z-index: 1000;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        `;
        
        document.body.appendChild(backToTop);
        
        // Show/hide button based on scroll position
        window.addEventListener('scroll', function() {
            if (window.scrollY > 300) {
                backToTop.style.opacity = '1';
                backToTop.style.visibility = 'visible';
            } else {
                backToTop.style.opacity = '0';
                backToTop.style.visibility = 'hidden';
            }
        });
        
        // Scroll to top on click
        backToTop.addEventListener('click', function() {
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        });
        
        // Hover effect
        backToTop.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-3px)';
            this.style.boxShadow = '0 6px 20px rgba(0, 0, 0, 0.2)';
        });
        
        backToTop.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
            this.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.15)';
        });
    }
    
    // Typing Effect for Hero Section
    function initTypingEffect() {
        const titles = [
            'سامانه مدیریت مکاتبات',
            'سیستم جامع اداری',
            'مدیریت پروژه هوشمند',
            'ارتباطات سازمانی'
        ];
        
        let currentIndex = 0;
        let currentText = '';
        let isDeleting = false;
        
        function typeText() {
            const titleElement = document.querySelector('.hero-content h1');
            if (!titleElement) return;
            
            const fullText = titles[currentIndex];
            
            if (isDeleting) {
                currentText = fullText.substring(0, currentText.length - 1);
            } else {
                currentText = fullText.substring(0, currentText.length + 1);
            }
            
            titleElement.innerHTML = currentText + '<span class="typing-cursor">|</span>';
            
            let typeSpeed = isDeleting ? 100 : 150;
            
            if (!isDeleting && currentText === fullText) {
                typeSpeed = 2000; // Pause at end
                isDeleting = true;
            } else if (isDeleting && currentText === '') {
                isDeleting = false;
                currentIndex = (currentIndex + 1) % titles.length;
                typeSpeed = 500; // Pause before next word
            }
            
            setTimeout(typeText, typeSpeed);
        }
        
        // Add typing cursor CSS
        const cursorStyle = document.createElement('style');
        cursorStyle.textContent = `
            .typing-cursor {
                animation: blink 1s infinite;
                color: #f59e0b;
            }
            @keyframes blink {
                0%, 50% { opacity: 1; }
                51%, 100% { opacity: 0; }
            }
        `;
        document.head.appendChild(cursorStyle);
        
        // Start typing effect with delay
        setTimeout(typeText, 1000);
    }
    
    // Parallax Effect
    function initParallax() {
        const parallaxElements = document.querySelectorAll('.hero-image img');
        
        window.addEventListener('scroll', function() {
            const scrolled = window.pageYOffset;
            const rate = scrolled * -0.5;
            
            parallaxElements.forEach(function(el) {
                el.style.transform = `translateY(${rate}px)`;
            });
        });
    }
    
    // Easter Egg - Konami Code
    let konamiCode = [];
    const konamiSequence = [38, 38, 40, 40, 37, 39, 37, 39, 66, 65]; // ↑↑↓↓←→←→BA
    
    document.addEventListener('keydown', function(e) {
        konamiCode.push(e.keyCode);
        
        if (konamiCode.length > konamiSequence.length) {
            konamiCode = konamiCode.slice(-konamiSequence.length);
        }
        
        if (konamiCode.join(',') === konamiSequence.join(',')) {
            // Easter egg activated!
            showNotification('🎉 شما کد مخفی را پیدا کردید! تیم TPL سلام میگه!', 'success');
            
            // Add some visual effects
            document.body.style.animation = 'rainbow 2s ease-in-out';
            
            const rainbowStyle = document.createElement('style');
            rainbowStyle.textContent = `
                @keyframes rainbow {
                    0% { filter: hue-rotate(0deg); }
                    50% { filter: hue-rotate(180deg); }
                    100% { filter: hue-rotate(360deg); }
                }
            `;
            document.head.appendChild(rainbowStyle);
            
            setTimeout(() => {
                document.body.style.animation = '';
                rainbowStyle.remove();
            }, 2000);
            
            konamiCode = [];
        }
    });
    
    // Lazy Loading for Images
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver(function(entries, observer) {
            entries.forEach(function(entry) {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove('lazy');
                    imageObserver.unobserve(img);
                }
            });
        });
        
        document.querySelectorAll('img[data-src]').forEach(function(img) {
            imageObserver.observe(img);
        });
    }
    
    // Performance Monitoring
    if ('performance' in window) {
        window.addEventListener('load', function() {
            const perfData = performance.getEntriesByType('navigation')[0];
            console.log('TPL Website Performance:');
            console.log(`Page Load Time: ${Math.round(perfData.loadEventEnd - perfData.fetchStart)}ms`);
            console.log(`DOM Ready Time: ${Math.round(perfData.domContentLoadedEventEnd - perfData.fetchStart)}ms`);
        });
    }
    
})();