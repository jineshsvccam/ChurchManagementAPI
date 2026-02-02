using ChurchContracts.Interfaces.Repositories;
using ChurchContracts.Interfaces.Services;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace ChurchServices.Verification
{
    public class VerificationService : IVerificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailVerificationTokenRepository _emailTokenRepository;
        private readonly IPhoneVerificationTokenRepository _phoneTokenRepository;
        private readonly IPasswordResetTokenRepository _passwordTokenRepository;
        private readonly IEmailService _emailService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<VerificationService> _logger;
        private readonly IConfiguration _configuration;

        private const int EmailTokenExpiryHours = 24;
        private const int PhoneTokenExpiryMinutes = 10;
        private const int PasswordTokenExpiryHours = 1;
        private const int MaxPhoneVerificationAttempts = 5;

        public VerificationService(
            ApplicationDbContext context,
            IEmailVerificationTokenRepository emailTokenRepository,
            IPhoneVerificationTokenRepository phoneTokenRepository,
            IPasswordResetTokenRepository passwordTokenRepository,
            IEmailService emailService,
            UserManager<User> userManager,
            ILogger<VerificationService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _emailTokenRepository = emailTokenRepository;
            _phoneTokenRepository = phoneTokenRepository;
            _passwordTokenRepository = passwordTokenRepository;
            _emailService = emailService;
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
        }

        // ==================== EMAIL VERIFICATION ====================

        public async Task<EmailVerificationResponseDto> SendEmailVerificationAsync(Guid userId, string email)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new EmailVerificationResponseDto
                    {
                        IsSuccess = false,
                        Message = "User not found."
                    };
                }

                // Delete existing tokens
                await _emailTokenRepository.DeleteAllByUserIdAsync(userId);

                // Generate token
                var token = GenerateToken();
                var emailToken = new EmailVerificationToken
                {
                    UserId = userId,
                    Email = email,
                    Token = token,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(EmailTokenExpiryHours),
                    IsUsed = false
                };

                await _emailTokenRepository.AddAsync(emailToken);

                // Create verification link
                var verificationLink = $"{_configuration["Frontend:Url"]}/verify-email?token={Uri.EscapeDataString(token)}";

                // Send email
                var emailSent = await _emailService.SendEmailVerificationAsync(email, verificationLink);

                if (!emailSent)
                {
                    _logger.LogWarning("Failed to send email verification to {Email}", email);
                    return new EmailVerificationResponseDto
                    {
                        IsSuccess = false,
                        Message = "Failed to send verification email. Please try again later."
                    };
                }

                return new EmailVerificationResponseDto
                {
                    IsSuccess = true,
                    Message = "Verification email sent successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email verification for user {UserId}", userId);
                return new EmailVerificationResponseDto
                {
                    IsSuccess = false,
                    Message = "An error occurred while sending verification email."
                };
            }
        }

        public async Task<EmailVerificationResponseDto> VerifyEmailAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return new EmailVerificationResponseDto
                    {
                        IsSuccess = false,
                        Message = "Invalid token."
                    };
                }

                var emailToken = await _emailTokenRepository.GetByTokenAsync(token);
                if (emailToken == null)
                {
                    return new EmailVerificationResponseDto
                    {
                        IsSuccess = false,
                        Message = "Invalid or expired verification token."
                    };
                }

                var user = await _context.Users.FindAsync(emailToken.UserId);
                if (user == null)
                {
                    return new EmailVerificationResponseDto
                    {
                        IsSuccess = false,
                        Message = "User not found."
                    };
                }

                // Mark token as used
                emailToken.IsUsed = true;
                emailToken.VerifiedAt = DateTime.UtcNow;
                await _emailTokenRepository.UpdateAsync(emailToken);

                // Update user
                user.EmailConfirmed = true;
                await _context.SaveChangesAsync();

                return new EmailVerificationResponseDto
                {
                    IsSuccess = true,
                    Message = "Email verified successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email");
                return new EmailVerificationResponseDto
                {
                    IsSuccess = false,
                    Message = "An error occurred while verifying email."
                };
            }
        }

        // ==================== PHONE VERIFICATION ====================

        public async Task<PhoneVerificationResponseDto> SendPhoneVerificationAsync(Guid userId, string phoneNumber)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new PhoneVerificationResponseDto
                    {
                        IsSuccess = false,
                        Message = "User not found."
                    };
                }

                // Delete existing tokens for this phone number
                var existingToken = await _phoneTokenRepository.GetByPhoneAndUserIdAsync(phoneNumber, userId);
                if (existingToken != null)
                {
                    await _phoneTokenRepository.DeleteAsync(existingToken.TokenId);
                }

                // Generate OTP
                var otp = GenerateOtp();
                var phoneToken = new PhoneVerificationToken
                {
                    UserId = userId,
                    PhoneNumber = phoneNumber,
                    Otp = otp,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(PhoneTokenExpiryMinutes),
                    IsUsed = false,
                    Attempts = 0
                };

                await _phoneTokenRepository.AddAsync(phoneToken);

                // Send OTP (simulating SMS via email for now)
                var otpSent = await _emailService.SendPhoneVerificationAsync(user.Email, otp);

                if (!otpSent)
                {
                    _logger.LogWarning("Failed to send phone verification OTP to {PhoneNumber}", phoneNumber);
                    return new PhoneVerificationResponseDto
                    {
                        IsSuccess = false,
                        Message = "Failed to send OTP. Please try again later."
                    };
                }

                return new PhoneVerificationResponseDto
                {
                    IsSuccess = true,
                    Message = "OTP sent successfully to your email. (In production, this would be sent via SMS)"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending phone verification OTP for user {UserId}", userId);
                return new PhoneVerificationResponseDto
                {
                    IsSuccess = false,
                    Message = "An error occurred while sending OTP."
                };
            }
        }

        public async Task<PhoneVerificationResponseDto> VerifyPhoneAsync(Guid userId, string otp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(otp))
                {
                    return new PhoneVerificationResponseDto
                    {
                        IsSuccess = false,
                        Message = "Invalid OTP."
                    };
                }

                var phoneToken = await _phoneTokenRepository.GetActiveByUserIdAsync(userId);
                if (phoneToken == null)
                {
                    return new PhoneVerificationResponseDto
                    {
                        IsSuccess = false,
                        Message = "No active OTP found. Please request a new one."
                    };
                }

                // Check attempts
                if (phoneToken.Attempts >= MaxPhoneVerificationAttempts)
                {
                    await _phoneTokenRepository.DeleteAsync(phoneToken.TokenId);
                    return new PhoneVerificationResponseDto
                    {
                        IsSuccess = false,
                        Message = "Maximum verification attempts exceeded. Please request a new OTP."
                    };
                }

                // Verify OTP
                if (phoneToken.Otp != otp)
                {
                    phoneToken.Attempts++;
                    await _phoneTokenRepository.UpdateAsync(phoneToken);

                    var attemptsRemaining = MaxPhoneVerificationAttempts - phoneToken.Attempts;
                    return new PhoneVerificationResponseDto
                    {
                        IsSuccess = false,
                        Message = $"Invalid OTP. {attemptsRemaining} attempt(s) remaining."
                    };
                }

                // Mark token as used
                phoneToken.IsUsed = true;
                phoneToken.VerifiedAt = DateTime.UtcNow;
                await _phoneTokenRepository.UpdateAsync(phoneToken);

                // Update user using UserManager to ensure Identity store consistency
                var identityUser = await _userManager.FindByIdAsync(userId.ToString());
                if (identityUser != null)
                {
                    if (!string.IsNullOrWhiteSpace(phoneToken.PhoneNumber))
                    {
                        identityUser.PhoneNumber = phoneToken.PhoneNumber;
                        identityUser.PhoneNumberConfirmed = true; // mark confirmed
                    }
                    else
                    {
                        // Even if phone number wasn't stored on the token, still mark confirmed
                        identityUser.PhoneNumberConfirmed = true;
                    }

                    var updateResult = await _userManager.UpdateAsync(identityUser);
                    if (!updateResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to update user phone confirmation for {UserId}: {Errors}", userId, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                    }
                }

                return new PhoneVerificationResponseDto
                {
                    IsSuccess = true,
                    Message = "Phone number verified successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying phone for user {UserId}", userId);
                return new PhoneVerificationResponseDto
                {
                    IsSuccess = false,
                    Message = "An error occurred while verifying phone."
                };
            }
        }

        // ==================== PASSWORD RESET ====================

        public async Task<PasswordResetRequestResponseDto> ForgotPasswordAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return new PasswordResetRequestResponseDto
                    {
                        IsSuccess = false,
                        Message = "Email is required."
                    };
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    // Don't reveal if email exists (security best practice)
                    return new PasswordResetRequestResponseDto
                    {
                        IsSuccess = true,
                        Message = "If an account exists with this email, a password reset link will be sent shortly."
                    };
                }

                // Delete existing reset tokens
                await _passwordTokenRepository.DeleteAllByUserIdAsync(user.Id);

                // Generate reset token
                var token = GenerateToken();
                var resetToken = new PasswordResetToken
                {
                    UserId = user.Id,
                    Token = token,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(PasswordTokenExpiryHours),
                    IsUsed = false
                };

                await _passwordTokenRepository.AddAsync(resetToken);

                // Create reset link
                var resetLink = $"{_configuration["Frontend:Url"]}/reset-password?token={Uri.EscapeDataString(token)}";

                // Send email
                var emailSent = await _emailService.SendPasswordResetAsync(email, resetLink);

                if (!emailSent)
                {
                    _logger.LogWarning("Failed to send password reset email to {Email}", email);
                }

                return new PasswordResetRequestResponseDto
                {
                    IsSuccess = true,
                    Message = "If an account exists with this email, a password reset link will be sent shortly."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in forgot password for {Email}", email);
                return new PasswordResetRequestResponseDto
                {
                    IsSuccess = false,
                    Message = "An error occurred. Please try again later."
                };
            }
        }

        public async Task<ResetPasswordResponseDto> ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
                {
                    return new ResetPasswordResponseDto
                    {
                        IsSuccess = false,
                        Message = "Invalid request."
                    };
                }

                var resetToken = await _passwordTokenRepository.GetByTokenAsync(token);
                if (resetToken == null)
                {
                    return new ResetPasswordResponseDto
                    {
                        IsSuccess = false,
                        Message = "Invalid or expired password reset token."
                    };
                }

                var user = await _context.Users.FindAsync(resetToken.UserId);
                if (user == null)
                {
                    return new ResetPasswordResponseDto
                    {
                        IsSuccess = false,
                        Message = "User not found."
                    };
                }

                // Set password hash directly to avoid writing null to the database
                var hashed = _userManager.PasswordHasher.HashPassword(user, newPassword);
                user.PasswordHash = hashed;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    return new ResetPasswordResponseDto
                    {
                        IsSuccess = false,
                        Message = $"Failed to set new password: {errors}"
                    };
                }

                // Mark token as used
                resetToken.IsUsed = true;
                resetToken.UsedAt = DateTime.UtcNow;
                await _passwordTokenRepository.UpdateAsync(resetToken);

                return new ResetPasswordResponseDto
                {
                    IsSuccess = true,
                    Message = "Password reset successfully. You can now login with your new password."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return new ResetPasswordResponseDto
                {
                    IsSuccess = false,
                    Message = "An error occurred while resetting password."
                };
            }
        }

        // ==================== HELPER METHODS ====================

        private string GenerateToken()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        private string GenerateOtp()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var otp = (Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000).ToString("D6");
                return otp;
            }
        }
    }
}
