using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TPLWeb.Tools
{
    public interface ISmsSender
    {
        Task<string> SendSmsAsync(string message, string phoneNumber);
        Task<string> SendNormalSmsAsync(string message, string phoneNumber);
        Task<string> SendBulkSmsAsync(string message, List<string> phoneNumbers);
    }

    public class SmsSender : ISmsSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsSender> _logger;

        public SmsSender(IConfiguration configuration, ILogger<SmsSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> SendSmsAsync(string message, string phoneNumber)
        {
            try
            {
                var apiUrl = _configuration["SmsSettings:ApiUrl"] ?? "https://s.api.ir/api/sw1/SmsOTP";
                var bearerToken = _configuration["SmsSettings:BearerToken"] ?? "woKUqb4hQBBnQZHsv35mORIpHN4JbqKLIRaNrFpNeXz2WBTwx5gk/EZJN1bnEGe8H+b1WLBubjeta5EqwwJgUWBNM5aaBlI8+um6j+4jrMs=";

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(apiUrl),
                    Content = new StringContent($"{{\"code\": \"{message}\", \"mobile\": \"{phoneNumber}\"}}", Encoding.UTF8, "application/json")
                };
                
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                var response = await new HttpClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("SMS sent successfully to {PhoneNumber}. Response: {Response}", phoneNumber, responseBody);
                return $"Success: {responseBody}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> SendNormalSmsAsync(string message, string phoneNumber)
        {
            try
            {
                var apiUrl = _configuration["SmsSettings:NormalSmsApiUrl"] ?? "https://s.api.ir/api/sw1/SendSms";
                var bearerToken = _configuration["SmsSettings:BearerToken"] ?? "woKUqb4hQBBnQZHsv35mORIpHN4JbqKLIRaNrFpNeXz2WBTwx5gk/EZJN1bnEGe8H+b1WLBubjeta5EqwwJgUWBNM5aaBlI8+um6j+4jrMs=";

                // ساخت JSON payload بر اساس مستندات API جدید
                var payload = new
                {
                    message = message,
                    mobiles = new[] { phoneNumber }
                };

                var jsonContent = JsonSerializer.Serialize(payload);
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(apiUrl),
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };
                
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                var response = await new HttpClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Normal SMS sent successfully to {PhoneNumber}. Response: {Response}", phoneNumber, responseBody);
                return $"Success: {responseBody}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending normal SMS to {PhoneNumber}", phoneNumber);
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> SendBulkSmsAsync(string message, List<string> phoneNumbers)
        {
            try
            {
                var apiUrl = _configuration["SmsSettings:NormalSmsApiUrl"] ?? "https://s.api.ir/api/sw1/SendSms";
                var bearerToken = _configuration["SmsSettings:BearerToken"] ?? "woKUqb4hQBBnQZHsv35mORIpHN4JbqKLIRaNrFpNeXz2WBTwx5gk/EZJN1bnEGe8H+b1WLBubjeta5EqwwJgUWBNM5aaBlI8+um6j+4jrMs=";

                // پاکسازی شماره‌های موبایل
                var cleanPhoneNumbers = phoneNumbers
                    .Select(p => p.Replace(" ", "").Replace("-", "").Replace("_", ""))
                    .Where(p => p.StartsWith("09") && p.Length == 11)
                    .ToArray();

                if (!cleanPhoneNumbers.Any())
                {
                    return "Error: هیچ شماره موبایل معتبری یافت نشد";
                }

                // ساخت JSON payload بر اساس مستندات API جدید
                var payload = new
                {
                    message = message,
                    mobiles = cleanPhoneNumbers
                };

                var jsonContent = JsonSerializer.Serialize(payload);
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(apiUrl),
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };
                
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                var response = await new HttpClient().SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Bulk SMS sent successfully to {Count} numbers. Response: {Response}", cleanPhoneNumbers.Length, responseBody);
                return $"Success: {responseBody}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk SMS to {Count} numbers", phoneNumbers.Count);
                return $"Error: {ex.Message}";
            }
        }
    }
}