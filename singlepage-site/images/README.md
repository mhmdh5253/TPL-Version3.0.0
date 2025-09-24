# تصاویر مورد نیاز برای وب‌سایت TPL

## 📸 فهرست تصاویر

### لوگو و آیکون‌ها
1. **logo.png** (200x200px, شفاف)
   - لوگوی اصلی TPL
   - فرمت: PNG با پس‌زمینه شفاف
   - کیفیت: بالا، مناسب برای نمایش در اندازه‌های مختلف

2. **favicon.png** (32x32px)
   - آیکون کوچک برای تب مرورگر
   - فرمت: PNG یا ICO
   - رنگ‌ها: سازگار با برند TPL

### تصاویر اصلی
3. **hero-dashboard.webp** (1200x800px)
   - تصویر اصلی dashboard در بخش hero
   - نمای کلی از رابط کاربری
   - فرمت: WebP برای بهینه‌سازی

### اسکرین‌شات‌ها
4. **screenshot-dashboard.webp** (800x600px)
   - نمای dashboard اصلی
   - شامل آمار و نمودارها

5. **screenshot-letters.webp** (800x600px)
   - صفحه مدیریت مکاتبات
   - لیست نامه‌ها و فیلترها

6. **screenshot-calendar.webp** (800x600px)
   - نمای تقویم تعاملی
   - رویدادها و یادآوری‌ها

7. **screenshot-chat.webp** (800x600px)
   - رابط چت و پیام‌رسانی
   - چت گروهی و خصوصی

8. **screenshot-users.webp** (800x600px)
   - صفحه مدیریت کاربران
   - لیست کاربران و دسترسی‌ها

9. **screenshot-mobile.webp** (400x800px)
   - نمای موبایل PWA
   - رابط کاربری موبایل

## 🎨 راهنمای طراحی

### رنگ‌های برند TPL:
- **Primary**: #1e3a8a (آبی تیره)
- **Secondary**: #f59e0b (نارنجی/طلایی)
- **Accent**: #10b981 (سبز)
- **Neutral**: #6b7280 (خاکستری)
- **Background**: #f8fafc (خاکستری روشن)

### سبک طراحی:
- **مینیمال و تمیز**
- **فلت دیزاین**
- **سایه‌های ملایم**
- **گرادیان‌های ظریف**
- **تایپوگرافی واضح**

### نکات فنی:
- **فرمت**: WebP برای تصاویر، PNG برای لوگو
- **کیفیت**: حداقل 72 DPI
- **فشرده‌سازی**: بهینه شده برای وب
- **Alt text**: توضیح کامل برای SEO

## 📐 اندازه‌ها و نسبت‌ها

### تصاویر افقی (16:9)
- Hero images
- Desktop screenshots
- Landscape orientations

### تصاویر عمودی (9:16)
- Mobile screenshots
- Portrait orientations

### تصاویر مربع (1:1)
- Logo
- Profile images
- Square thumbnails

## 🖼️ نحوه تهیه تصاویر

### از سیستم TPL:
1. اسکرین‌شات با کیفیت بالا
2. حذف اطلاعات حساس
3. افزودن داده‌های نمونه
4. ویرایش و بهینه‌سازی

### از طراح گرافیک:
1. ارسال راهنمای برند
2. نمونه‌های موجود
3. مشخصات فنی
4. Timeline پروژه

### ابزارهای پیشنهادی:
- **Figma**: طراحی UI/UX
- **Adobe XD**: پروتوتایپ
- **Sketch**: طراحی رابط
- **Canva**: طراحی سریع

## 🔧 بهینه‌سازی

### فشرده‌سازی:
```bash
# تبدیل به WebP
cwebp input.png -q 80 -o output.webp

# کاهش حجم PNG
pngquant --quality=65-80 input.png

# بهینه‌سازی JPEG
jpegtran -optimize -progressive input.jpg > output.jpg
```

### Lazy Loading:
تصاویر به صورت خودکار lazy load می‌شوند:
```html
<img data-src="images/screenshot.webp" alt="توضیح" class="lazy">
```

## 📱 Responsive Images

### استفاده از srcset:
```html
<img srcset="
  images/mobile-320.webp 320w,
  images/tablet-768.webp 768w,
  images/desktop-1200.webp 1200w
" sizes="(max-width: 320px) 280px,
         (max-width: 768px) 720px,
         1200px"
     src="images/desktop-1200.webp"
     alt="توضیح تصویر">
```

## ✅ چک‌لیست تصاویر

- [ ] logo.png (200x200, شفاف)
- [ ] favicon.png (32x32)
- [ ] hero-dashboard.webp (1200x800)
- [ ] screenshot-dashboard.webp (800x600)
- [ ] screenshot-letters.webp (800x600)
- [ ] screenshot-calendar.webp (800x600)
- [ ] screenshot-chat.webp (800x600)
- [ ] screenshot-users.webp (800x600)
- [ ] screenshot-mobile.webp (400x800)

## 🎯 نکات مهم

1. **کیفیت تصاویر**: حتماً با رزولوشن بالا
2. **سازگاری برند**: رعایت راهنمای بصری TPL
3. **داده‌های نمونه**: استفاده از محتوای واقعی اما غیرحساس
4. **بهینه‌سازی**: حجم کم با کیفیت مناسب
5. **Alt text**: توضیح کامل برای دسترسی‌پذیری

---

*برای دریافت فایل‌های تصویری یا سفارش طراحی، با تیم TPL تماس بگیرید.*