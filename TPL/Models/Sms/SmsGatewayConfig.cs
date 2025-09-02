using System.Collections.Generic;
using TPLWeb.Models.Sms;

namespace TPLWeb.Services.Sms
{
    /// <summary>
    /// تنظیمات Gateway برای ارسال SMS
    /// </summary>
    public class SmsGatewayConfig
    {
        /// <summary>
        /// نام Gateway
        /// </summary>
        public string GatewayName { get; set; } = string.Empty;
        
        /// <summary>
        /// نام نمایشی
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
        
        /// <summary>
        /// توضیحات
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// URL اصلی API
        /// </summary>
        public string ApiUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// نوع احراز هویت
        /// </summary>
        public SmsAuthType AuthType { get; set; }
        
        /// <summary>
        /// متد HTTP
        /// </summary>
        public string HttpMethod { get; set; } = "POST";
        
        /// <summary>
        /// نوع محتوا
        /// </summary>
        public string ContentType { get; set; } = "application/json";
        
        /// <summary>
        /// آیا از OTP پشتیبانی می‌کند
        /// </summary>
        public bool SupportOtp { get; set; }
        
        /// <summary>
        /// Endpoint برای OTP
        /// </summary>
        public string? OtpEndpoint { get; set; }
        
        /// <summary>
        /// قالب پیام OTP
        /// </summary>
        public string? OtpMessageTemplate { get; set; }
        
        /// <summary>
        /// آیا از SMS عادی پشتیبانی می‌کند
        /// </summary>
        public bool SupportRegularSms { get; set; }
        
        /// <summary>
        /// Endpoint برای SMS عادی
        /// </summary>
        public string? SmsEndpoint { get; set; }
        
        /// <summary>
        /// آیا از Bulk SMS پشتیبانی می‌کند
        /// </summary>
        public bool SupportBulkSms { get; set; }
        
        /// <summary>
        /// آیا از Template پشتیبانی می‌کند
        /// </summary>
        public bool SupportTemplate { get; set; }
        
        /// <summary>
        /// کلید API
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;
        
        /// <summary>
        /// نام کاربری
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// رمز عبور
        /// </summary>
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// شماره فرستنده
        /// </summary>
        public string SenderNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// پارامترهای درخواست
        /// </summary>
        public Dictionary<string, string> RequestParameters { get; set; } = new();
        
        /// <summary>
        /// الگوی پاسخ موفق
        /// </summary>
        public string? SuccessResponsePattern { get; set; }
        
        /// <summary>
        /// الگوی پاسخ خطا
        /// </summary>
        public string? ErrorResponsePattern { get; set; }
        
        /// <summary>
        /// آیا مانیتورینگ فعال است
        /// </summary>
        public bool EnableMonitoring { get; set; }
        
        /// <summary>
        /// زمان انتظار (ثانیه)
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;
        
        /// <summary>
        /// تعداد تلاش مجدد
        /// </summary>
        public int RetryCount { get; set; } = 3;
    }
}
