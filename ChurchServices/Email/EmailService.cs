using ChurchContracts.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace ChurchServices.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var senderEmail = _configuration["Email:SenderEmail"];
                var senderPassword = _configuration["Email:SenderPassword"];
                var senderName = _configuration["Email:SenderName"] ?? "FinChurch";

                if (string.IsNullOrWhiteSpace(smtpServer) || string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPassword))
                {
                    _logger.LogError("Email config missing. Check Email:SmtpServer, Email:SenderEmail, Email:SenderPassword.");
                    return false;
                }

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(senderEmail, senderPassword)
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", to);
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error sending email to {Email}. StatusCode: {StatusCode}", to, ex.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                return false;
            }
        }

        public Task<bool> SendEmailVerificationAsync(string to, string verificationLink)
        {
            var subject = "Email Verification - FinChurch";
            var body = $@"
                <h2>Email Verification</h2>
                <p>Thank you for registering with FinChurch. Please verify your email address by clicking the link below:</p>
                <p><a href='{verificationLink}'>Verify Email</a></p>
                <p>This link will expire in 24 hours.</p>
                <p>If you did not register for this account, please ignore this email.</p>
            ";

            return SendEmailAsync(to, subject, body, true);
        }

        public Task<bool> SendPasswordResetAsync(string to, string resetLink)
        {
            var subject = "Password Reset - FinChurch";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>We received a request to reset your password. Click the link below to reset it:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>This link will expire in 1 hour.</p>
                <p>If you did not request a password reset, please ignore this email.</p>
            ";

            return SendEmailAsync(to, subject, body, true);
        }

        public Task<bool> SendPhoneVerificationAsync(string to, string otp)
        {
            var subject = "Phone Verification OTP - FinChurch";
            var body = $@"
                <h2>Phone Verification</h2>
                <p>Your OTP (One-Time Password) for phone verification is:</p>
                <h1 style='color: #007bff; font-size: 32px; letter-spacing: 5px;'>{otp}</h1>
                <p>This OTP will expire in 10 minutes.</p>
                <p>Do not share this OTP with anyone.</p>
            ";

            return SendEmailAsync(to, subject, body, true);
        }
    }
}
