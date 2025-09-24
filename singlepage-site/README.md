# TPL System Single Page Website v4.0

## 📁 فایل‌های پروژه

### ساختار پروژه:
```
singlepage-site/
├── index.html          # فایل اصلی HTML
├── style.css           # استایل‌های CSS
├── script.js           # جاوااسکریپت
├── README.md           # مستندات پروژه
└── images/             # تصاویر و گرافیک‌ها
    ├── logo.png
    ├── favicon.png
    ├── hero-dashboard.webp
    ├── screenshot-dashboard.webp
    ├── screenshot-letters.webp
    ├── screenshot-calendar.webp
    ├── screenshot-chat.webp
    ├── screenshot-users.webp
    └── screenshot-mobile.webp
```

## 🚀 راه‌اندازی

### نیازمندی‌ها:
- هر وب سرور (Apache, Nginx, IIS)
- یا حتی فایل HTML را مستقیماً در مرورگر باز کنید

### مراحل نصب:
1. تمام فایل‌ها را در یک پوشه قرار دهید
2. تصاویر مورد نیاز را در پوشه `images` اضافه کنید
3. فایل `index.html` را در مرورگر باز کنید

## 🎨 ویژگی‌های طراحی

### رنگ‌های اصلی:
- **Primary**: `#1e3a8a` (سرمه‌ای تیره)
- **Secondary**: `#f59e0b` (طلایی)
- **Accent**: `#10b981` (سبز)
- **Background**: `#f8fafc` (خاکستری روشن)

### فونت‌ها:
- **اصلی**: Vazirmatn (فارسی)
- **پشتیبان**: -apple-system, BlinkMacSystemFont, 'Segoe UI'

### انیمیشن‌ها:
- Fade in animations
- Smooth scrolling
- Hover effects
- Counter animations
- Typing effect

## 📱 واکنش‌گرایی

### نقاط شکست (Breakpoints):
- **Mobile**: < 576px
- **Tablet**: 576px - 768px
- **Desktop**: 768px - 992px
- **Large Desktop**: > 992px

## 🔧 سفارشی‌سازی

### تغییر رنگ‌ها:
متغیرهای CSS در فایل `style.css` را ویرایش کنید:
```css
:root {
    --primary-color: #1e3a8a;
    --secondary-color: #f59e0b;
    /* سایر رنگ‌ها */
}
```

### اضافه کردن بخش جدید:
1. HTML مربوطه را در `index.html` اضافه کنید
2. استایل‌های مرتبط را در `style.css` بنویسید
3. عملکرد جاوااسکریپت را در `script.js` اضافه کنید

## 📧 فرم تماس

فرم تماس دارای اعتبارسنجی کامل است:
- اعتبارسنجی ایمیل
- اعتبارسنجی شماره تلفن ایرانی
- نمایش پیام‌های خطا و موفقیت
- جلوگیری از ارسال مکرر

### تنظیم backend:
برای عملکرد واقعی فرم، نیاز به backend دارید:
```javascript
// در script.js خط 200
// جایگزین URL زیر با endpoint واقعی
fetch('/api/contact', {
    method: 'POST',
    body: formData
})
```

## 🖼️ تصاویر

### فرمت‌های پیشنهادی:
- **Logo**: PNG شفاف
- **Screenshots**: WebP (بهینه شده)
- **Hero Image**: WebP یا JPG
- **Icons**: SVG یا Font Awesome

### اندازه‌های پیشنهادی:
- **Hero Image**: 1200x800px
- **Screenshots**: 800x600px
- **Logo**: 200x200px (شفاف)
- **Favicon**: 32x32px

## ⚡ بهینه‌سازی عملکرد

### ویژگی‌های پیاده‌شده:
- Lazy loading برای تصاویر
- Minified CSS و JS
- استفاده از WebP برای تصاویر
- CDN برای کتابخانه‌ها
- Service Worker برای کش

### توصیه‌های اضافی:
1. فشرده‌سازی Gzip روی سرور
2. تنظیم Cache Headers
3. استفاده از CDN
4. بهینه‌سازی تصاویر

## 🧪 تست

### مرورگرهای پشتیبانی شده:
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

### تست موبایل:
از Chrome DevTools برای تست responsiveness استفاده کنید.

## 📊 آنالیتیکس

### Google Analytics:
کد زیر را قبل از `</head>` اضافه کنید:
```html
<!-- Google tag (gtag.js) -->
<script async src="https://www.googletagmanager.com/gtag/js?id=GA_MEASUREMENT_ID"></script>
<script>
  window.dataLayer = window.dataLayer || [];
  function gtag(){dataLayer.push(arguments);}
  gtag('js', new Date());
  gtag('config', 'GA_MEASUREMENT_ID');
</script>
```

## 🔍 SEO

### بهینه‌سازی‌های پیاده‌شده:
- Meta tags کامل
- Structured data
- Alt text برای تصاویر
- Semantic HTML
- Fast loading

### بهبودهای پیشنهادی:
1. اضافه کردن Open Graph tags
2. Schema markup
3. XML Sitemap
4. Robots.txt

## 🛡️ امنیت

### تنظیمات پیشنهادی:
```apache
# .htaccess
Header always set X-Frame-Options DENY
Header always set X-Content-Type-Options nosniff
Header always set Referrer-Policy strict-origin-when-cross-origin
```

## 📞 پشتیبانی

### اطلاعات تماس:
- **ایمیل**: support@tpl.com
- **تلفن**: ۰۲۱-۱۲۳۴۵۶۷۸
- **تلگرام**: @TPLSupport

### مستندات بیشتر:
- [راهنمای کاربری](features.md)
- [مستندات API](api-docs.md)
- [نمونه‌های کد](code-examples.md)

---

**نسخه**: 4.0.0  
**تاریخ به‌روزرسانی**: مهر ۱۴۰۳  
**سازندگان**: تیم TPL Development  

*این وب‌سایت با ❤️ و دقت طراحی شده است.*