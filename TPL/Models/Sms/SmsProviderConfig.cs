using System.Collections.Generic;

namespace TPLWeb.Models.Sms
{
    public enum SmsAuthType
    {
        None = 0,
        Bearer = 1,
        ApiKey = 2,
        Basic = 3,
        Custom = 4
    }

    public class HttpHeader
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class SmsProviderConfig
    {
        public string? ProviderName { get; set; } = string.Empty;
        public string? ApiUrl { get; set; } = string.Empty;
        public string? ApiKey { get; set; } = string.Empty;
        public string? Username { get; set; } = string.Empty;
        public string? Password { get; set; } = string.Empty;
        public SmsAuthType AuthType { get; set; } = SmsAuthType.None;
        public string? SenderNumber { get; set; } = string.Empty;
        public string? HttpMethod { get; set; } = "POST"; // POST or GET for binding simplicity
        public string? ContentType { get; set; } = "application/json";
        public int? Priority { get; set; } = 1;
        public List<HttpHeader> CustomHeaders { get; set; } = new List<HttpHeader>();
        
        // Template settings
        public bool UseTemplate { get; set; } = false;
        public string? TemplateCode { get; set; } = string.Empty;
        public Dictionary<string, string> TemplateParameters { get; set; } = new Dictionary<string, string>();
        
        // OTP settings
        public bool EnableOtp { get; set; } = true;
        public int? OtpLength { get; set; } = 6;
        public int? OtpExpiryMinutes { get; set; } = 5;
        public string? OtpMessageTemplate { get; set; } = "کد تایید شما: {code}\nاین کد تا {expiry} دقیقه معتبر است.\nسامانه مدیریت مکاتبات";
        
        // Default provider flag
        public bool IsDefault { get; set; } = false;
        
        // Active/Inactive status
        public bool IsActive { get; set; } = true;
    }
}


