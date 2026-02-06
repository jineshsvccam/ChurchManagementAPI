using System;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ChurchServices.Registration
{
    public class SnsPhoneOtpSender : IPhoneOtpSender
    {
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly ILogger<SnsPhoneOtpSender> _logger;
        private readonly IConfiguration _configuration;

        public SnsPhoneOtpSender(IAmazonSimpleNotificationService snsClient, ILogger<SnsPhoneOtpSender> logger, IConfiguration configuration)
        {
            _snsClient = snsClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendOtpAsync(string phoneNumber, string otp)
        {
            try
            {
                // Basic phone number normalization could be configured; keep it minimal to avoid changing behavior.
                var message = $"Your verification code is: {otp}";

                var publishRequest = new PublishRequest
                {
                    PhoneNumber = phoneNumber,
                    Message = message
                };

                // Optionally set SMS attributes like SenderID or SMS Type from configuration if present
                // but do not change behavior if not configured.
                var senderId = _configuration["Aws:Sns:SenderId"];
                var smsType = _configuration["Aws:Sns:SmsType"]; // e.g., "Transactional"
               
                publishRequest.MessageAttributes ??= new Dictionary<string, MessageAttributeValue>();

                if (!string.IsNullOrWhiteSpace(senderId))
                {
                    publishRequest.MessageAttributes.Add("AWS.SNS.SMS.SenderID", new MessageAttributeValue
                    {
                        StringValue = senderId,
                        DataType = "String"
                    });
                }

                if (!string.IsNullOrWhiteSpace(smsType))
                {
                    publishRequest.MessageAttributes.Add("AWS.SNS.SMS.SMSType", new MessageAttributeValue
                    {
                        StringValue = smsType,
                        DataType = "String"
                    });
                }

                var response = await _snsClient.PublishAsync(publishRequest);

                // Treat non-exceptional failure as a failure to send
                if (response == null || string.IsNullOrWhiteSpace(response.MessageId))
                {
                    _logger.LogWarning("SNS Publish returned no message id when sending OTP to {PhoneNumber}", phoneNumber);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log but do not throw; callers expect a boolean and generic behavior.
                _logger.LogWarning(ex, "Failed to send OTP via SNS to {PhoneNumber}", phoneNumber);
                return false;
            }
        }
    }
}
