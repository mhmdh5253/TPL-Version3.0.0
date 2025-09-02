/**
 * Offline Libraries Configuration
 * This file ensures all JavaScript libraries are loaded from local assets
 */

(function() {
    'use strict';
    
    // Offline Libraries Configuration
    window.OfflineLibraries = {
        // Bootstrap Icons
        bootstrapIcons: {
            loaded: false,
            path: '/assets/bootstrap-icons-1.11.3/bootstrap-icons.css',
            check: function() {
                return document.querySelector('link[href*="bootstrap-icons"]') !== null;
            }
        },
        
        // Material Design Icons
        materialIcons: {
            loaded: false,
            path: '/assets/vendor/fonts/materialdesignicons.css',
            check: function() {
                return document.querySelector('link[href*="materialdesignicons"]') !== null;
            }
        },
        
        // IRANSans Font
        iranSans: {
            loaded: false,
            path: '/assets/vendor/fonts/IRANSans.css',
            check: function() {
                return document.querySelector('link[href*="IRANSans"]') !== null;
            }
        },
        
        // Osam Font
        osamFont: {
            loaded: false,
            path: '/assets/vendor/fonts/Osam.css',
            check: function() {
                return document.querySelector('link[href*="Osam"]') !== null;
            }
        },
        
        // Check all libraries
        checkAll: function() {
            this.bootstrapIcons.loaded = this.bootstrapIcons.check();
            this.materialIcons.loaded = this.materialIcons.check();
            this.iranSans.loaded = this.iranSans.check();
            this.osamFont.loaded = this.osamFont.check();
            
            console.log('Offline Libraries Status:', {
                bootstrapIcons: this.bootstrapIcons.loaded,
                materialIcons: this.materialIcons.loaded,
                iranSans: this.iranSans.loaded,
                osamFont: this.osamFont.loaded
            });
            
            return this.bootstrapIcons.loaded && 
                   this.materialIcons.loaded && 
                   this.iranSans.loaded && 
                   this.osamFont.loaded;
        },
        
        // Initialize
        init: function() {
            // Check libraries after DOM is loaded
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', this.checkAll.bind(this));
            } else {
                this.checkAll();
            }
            
            // Check again after a short delay to ensure all resources are loaded
            setTimeout(this.checkAll.bind(this), 1000);
        }
    };
    
    // Icon replacement system for removed FontAwesome
    window.IconReplacement = {
        // Replace FontAwesome classes with Bootstrap Icons
        replaceIcons: function() {
            const iconMappings = {
                'fa': 'bi',
                'fas': 'bi',
                'far': 'bi',
                'fab': 'bi',
                'fa-home': 'bi-house',
                'fa-user': 'bi-person',
                'fa-cog': 'bi-gear',
                'fa-search': 'bi-search',
                'fa-edit': 'bi-pencil',
                'fa-pencil': 'bi-pencil',
                'fa-trash': 'bi-trash',
                'fa-plus': 'bi-plus',
                'fa-minus': 'bi-dash',
                'fa-check': 'bi-check',
                'fa-times': 'bi-x',
                'fa-close': 'bi-x',
                'fa-arrow-left': 'bi-arrow-left',
                'fa-arrow-right': 'bi-arrow-right',
                'fa-arrow-up': 'bi-arrow-up',
                'fa-arrow-down': 'bi-arrow-down',
                'fa-calendar': 'bi-calendar',
                'fa-clock': 'bi-clock',
                'fa-envelope': 'bi-envelope',
                'fa-phone': 'bi-telephone',
                'fa-map-marker': 'bi-geo-alt',
                'fa-download': 'bi-download',
                'fa-upload': 'bi-upload',
                'fa-print': 'bi-printer',
                'fa-save': 'bi-save',
                'fa-refresh': 'bi-arrow-clockwise',
                'fa-settings': 'bi-gear',
                'fa-info': 'bi-info-circle',
                'fa-warning': 'bi-exclamation-triangle',
                'fa-exclamation-triangle': 'bi-exclamation-triangle',
                'fa-error': 'bi-x-circle',
                'fa-success': 'bi-check-circle'
            };
            
            // Replace classes in existing elements
            Object.keys(iconMappings).forEach(oldClass => {
                const newClass = iconMappings[oldClass];
                const elements = document.querySelectorAll('.' + oldClass);
                elements.forEach(element => {
                    element.classList.remove(oldClass);
                    element.classList.add(newClass);
                });
            });
        },
        
        // Initialize
        init: function() {
            // Replace icons after DOM is loaded
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', this.replaceIcons);
            } else {
                this.replaceIcons();
            }
            
            // Replace icons after dynamic content is loaded
            const observer = new MutationObserver(() => {
                this.replaceIcons();
            });
            
            observer.observe(document.body, {
                childList: true,
                subtree: true
            });
        }
    };
    
    // Initialize offline libraries
    window.OfflineLibraries.init();
    window.IconReplacement.init();
    
    // Export for use in other scripts
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = {
            OfflineLibraries: window.OfflineLibraries,
            IconReplacement: window.IconReplacement
        };
    }
    
})();
