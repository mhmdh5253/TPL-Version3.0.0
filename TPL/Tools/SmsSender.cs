using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using PARSGREEN.CORE.RESTful.SMS;
using System.Linq;

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
                var apiKey = _configuration["SmsSettings:ApiKey"] ?? _configuration["SmsSettings:BearerToken"] ?? string.Empty;
                if (string.IsNullOrWhiteSpace(apiKey))
                    return "Error: API key not configured (SmsSettings:ApiKey/BearerToken)";

                var messageClient = new Message(apiKey);
                var result = await Task.Run(() => messageClient.SendOtp(phoneNumber, message));
                var responseText = SafeSerialize(result);

                _logger.LogInformation("OTP SMS sent to {PhoneNumber}. Response: {Response}", phoneNumber, responseText);
                return $"Success: {responseText}";
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
                var apiKey = _configuration["SmsSettings:ApiKey"] ?? _configuration["SmsSettings:BearerToken"] ?? string.Empty;
                if (string.IsNullOrWhiteSpace(apiKey))
                    return "Error: API key not configured (SmsSettings:ApiKey/BearerToken)";

                var messageClient = new Message(apiKey);
                var recipients = new[] { phoneNumber };

                var result = await Task.Run(() => messageClient.SendSms(message, recipients));
                var responseText = SafeSerialize(result);

                _logger.LogInformation("Normal SMS sent to {PhoneNumber}. Response: {Response}", phoneNumber, responseText);
                return $"Success: {responseText}";
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
                var apiKey = _configuration["SmsSettings:ApiKey"] ?? _configuration["SmsSettings:BearerToken"] ?? string.Empty;
                if (string.IsNullOrWhiteSpace(apiKey))
                    return "Error: API key not configured (SmsSettings:ApiKey/BearerToken)";

                var cleanPhoneNumbers = phoneNumbers
                    .Select(p => p.Replace(" ", "").Replace("-", "").Replace("_", ""))
                    .Where(p => p.StartsWith("09") && p.Length == 11)
                    .ToArray();

                if (!cleanPhoneNumbers.Any())
                {
                    return "Error: هیچ شماره موبایل معتبری یافت نشد";
                }

                var messageClient = new Message(apiKey);
                var result = await Task.Run(() => messageClient.SendSms(message, cleanPhoneNumbers));
                var responseText = SafeSerialize(result);

                _logger.LogInformation("Bulk SMS sent to {Count} numbers. Response: {Response}", cleanPhoneNumbers.Length, responseText);
                return $"Success: {responseText}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk SMS to {Count} numbers", phoneNumbers.Count);
                return $"Error: {ex.Message}";
            }
        }

        private static string SafeSerialize(object? obj)
        {
            try
            {
                return JsonSerializer.Serialize(obj);
            }
            catch
            {
                return obj?.ToString() ?? string.Empty;
            }
        }
    }
}