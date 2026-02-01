using System.Security.Cryptography;
using System.Text;
using ChurchContracts.Interfaces.Services;
using ChurchData;
using ChurchData.Entities;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Registration
{
    public class RegistrationRequestService : IRegistrationRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegistrationRequestService> _logger;

        // Keep token TTL short to reduce theft/replay window (15–30 minutes as required).
        private static readonly TimeSpan TokenTtl = TimeSpan.FromMinutes(30);

        // Per-email throttling to limit spam: max 3 requests per email per hour.
        private static readonly TimeSpan EmailRateWindow = TimeSpan.FromHours(1);
        private const int MaxRequestsPerEmailPerWindow = 3;

        // Phone OTP rules
        private static readonly TimeSpan PhoneOtpTtl = TimeSpan.FromMinutes(10);
        private const int MaxPhoneOtpAttempts = 5;

        public RegistrationRequestService(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<RegistrationRequestService> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<RegisterRequestResponseDto> CreateRegisterRequestAsync(RegisterRequestDto request, string ipAddress)
        {
            try
            {
                var now = DateTime.UtcNow;

                // Security: always respond with a generic success message to avoid account enumeration
                // (do not reveal whether email exists, role is valid, etc.).
                const string genericMessage = "If an account can be created for this email, a verification link will be sent shortly.";

                // Validate role exists (but do not disclose result)
                var roleExists = await _context.Roles.AsNoTracking().AnyAsync(r => r.Name == request.Role);
                if (!roleExists)
                {
                    return new RegisterRequestResponseDto { IsSuccess = true, Message = genericMessage };
                }

                // If user already exists, do nothing (silent)
                var existingUser = await _context.Users.AsNoTracking().AnyAsync(u => u.Email == request.Email);
                if (existingUser)
                {
                    return new RegisterRequestResponseDto { IsSuccess = true, Message = genericMessage };
                }

                // Cleanup expired requests for this email (best-effort)
                await _context.RegistrationRequests
                    .Where(r => r.Email == request.Email && r.ExpiresAt < now)
                    .ExecuteDeleteAsync();

                // Email rate limit: max 3 create attempts per email per hour.
                // We count by CreatedAt to enforce a rolling window.
                var emailWindowStart = now - EmailRateWindow;
                var emailAttempts = await _context.RegistrationRequests
                    .AsNoTracking()
                    .CountAsync(r => r.Email == request.Email && r.CreatedAt >= emailWindowStart);

                if (emailAttempts >= MaxRequestsPerEmailPerWindow)
                {
                    return new RegisterRequestResponseDto { IsSuccess = true, Message = genericMessage };
                }

                // If there is an active request already, do not create another one (reduces email spam).
                var activeRequest = await _context.RegistrationRequests
                    .AsNoTracking()
                    .AnyAsync(r => r.Email == request.Email && r.ExpiresAt >= now && !r.EmailVerified);

                if (activeRequest)
                {
                    return new RegisterRequestResponseDto { IsSuccess = true, Message = genericMessage };
                }

                var token = GenerateToken();

                var rr = new RegistrationRequest
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    Role = request.Role,
                    ParishId = request.ParishId,
                    FamilyId = request.FamilyId,
                    EmailVerificationToken = token,
                    EmailVerified = false,
                    EmailVerifiedAt = null,
                    PhoneNumber = null,
                    PhoneVerified = false,
                    PhoneVerifiedAt = null,
                    CreatedAt = now,
                    ExpiresAt = now.Add(TokenTtl)
                };

                _context.RegistrationRequests.Add(rr);
                await _context.SaveChangesAsync();

                var verificationLink = $"{_configuration["Frontend:Url"]}/auth/verify-registration-email?token={Uri.EscapeDataString(token)}";
                var sent = await _emailService.SendEmailVerificationAsync(request.Email, verificationLink);

                if (!sent)
                {
                    // Security: still return generic success to client.
                    _logger.LogWarning("Failed to send registration verification email to {Email}.", request.Email);
                }

                return new RegisterRequestResponseDto { IsSuccess = true, Message = genericMessage };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating registration request for {Email}", request.Email);

                // Security: still return generic success in production to avoid turning this into an oracle.
                return new RegisterRequestResponseDto
                {
                    IsSuccess = true,
                    Message = "If an account can be created for this email, a verification link will be sent shortly."
                };
            }
        }

        public async Task<VerifyRegistrationEmailResponseDto> VerifyRegistrationEmailAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return new VerifyRegistrationEmailResponseDto { IsSuccess = false, Message = "Invalid token." };
                }

                var now = DateTime.UtcNow;

                // Token is single-use: do not accept if already verified.
                var rr = await _context.RegistrationRequests
                    .FirstOrDefaultAsync(r => r.EmailVerificationToken == token);

                if (rr == null || rr.ExpiresAt < now)
                {
                    return new VerifyRegistrationEmailResponseDto { IsSuccess = false, Message = "Invalid or expired token." };
                }

                if (rr.EmailVerified)
                {
                    return new VerifyRegistrationEmailResponseDto { IsSuccess = false, Message = "Invalid or expired token." };
                }

                rr.EmailVerified = true;
                rr.EmailVerifiedAt = now;

                await _context.SaveChangesAsync();

                return new VerifyRegistrationEmailResponseDto { IsSuccess = true, Message = "Email verified successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying registration email");
                return new VerifyRegistrationEmailResponseDto { IsSuccess = false, Message = "An error occurred while verifying email." };
            }
        }

        public async Task<CompleteRegistrationResponseDto> CompleteRegistrationAsync(CompleteRegistrationDto request)
        {
            try
            {
                var now = DateTime.UtcNow;

                if (string.IsNullOrWhiteSpace(request.Token))
                {
                    return new CompleteRegistrationResponseDto { IsSuccess = false, Message = "Invalid or expired token." };
                }

                // Token must match AND must still be valid.
                // UI should reuse the same token from the verification link for complete-registration
                
                var rr = await _context.RegistrationRequests
                    .FirstOrDefaultAsync(r => r.EmailVerificationToken == request.Token);

                if (rr == null || rr.ExpiresAt < now)
                {
                    return new CompleteRegistrationResponseDto { IsSuccess = false, Message = "Invalid or expired token." };
                }

                if (!rr.EmailVerified)
                {
                    return new CompleteRegistrationResponseDto { IsSuccess = false, Message = "Email verification is required." };
                }

                var alreadyExists = await _context.Users.AsNoTracking().AnyAsync(u => u.Email == rr.Email);
                if (alreadyExists)
                {
                    _context.RegistrationRequests.Remove(rr);
                    await _context.SaveChangesAsync();
                    return new CompleteRegistrationResponseDto { IsSuccess = false, Message = "Unable to complete registration." };
                }

                var user = new User
                {
                    UserName = request.Username,
                    Email = rr.Email,
                    EmailConfirmed = true,
                    PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? rr.PhoneNumber : request.PhoneNumber,
                    PhoneNumberConfirmed = false,
                    ParishId = rr.ParishId,
                    FamilyId = rr.FamilyId,
                    FullName = rr.FullName,
                    Status = UserStatus.Pending
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var msg = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new CompleteRegistrationResponseDto { IsSuccess = false, Message = msg };
                }

                // Reload user to ensure Id is generated and available for custom UserRole join entity.
                var createdUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == rr.Email);
                if (createdUser != null)
                {
                    user.Id = createdUser.Id;
                }

                // Assign role using custom UserRole entity (composite key + extra columns).
                // Using UserManager.AddToRoleAsync can fail when the underlying join entity is customized.
                var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.Name == rr.Role);
                if (roleEntity == null)
                {
                    _logger.LogWarning("Role {Role} not found while completing registration for {Email}", rr.Role, rr.Email);
                }
                else
                {
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roleEntity.Id,
                        Status = RoleStatus.Pending,
                        RequestedAt = DateTime.UtcNow
                    };

                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                _context.RegistrationRequests.Remove(rr);
                await _context.SaveChangesAsync();

                return new CompleteRegistrationResponseDto { IsSuccess = true, Message = "Registration completed successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing registration");
                return new CompleteRegistrationResponseDto { IsSuccess = false, Message = "An error occurred while completing registration." };
            }
        }

        public async Task<RegistrationPhoneVerificationResponseDto> SendRegistrationPhoneOtpAsync(SendRegistrationPhoneOtpDto request, string ipAddress)
        {
            try
            {
                var now = DateTime.UtcNow;

                if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    return new RegistrationPhoneVerificationResponseDto
                    {
                        IsSuccess = false,
                        PhoneVerified = false,
                        Message = "Invalid request."
                    };
                }

                var rr = await _context.RegistrationRequests
                    .FirstOrDefaultAsync(r => r.EmailVerificationToken == request.Token);

                if (rr == null || rr.ExpiresAt < now)
                {
                    // Do not reveal token validity details.
                    return new RegistrationPhoneVerificationResponseDto
                    {
                        IsSuccess = false,
                        PhoneVerified = false,
                        Message = "Invalid or expired token."
                    };
                }

                if (!rr.EmailVerified)
                {
                    return new RegistrationPhoneVerificationResponseDto
                    {
                        IsSuccess = false,
                        PhoneVerified = false,
                        Message = "Email verification is required."
                    };
                }

                if (rr.PhoneVerified)
                {
                    return new RegistrationPhoneVerificationResponseDto
                    {
                        IsSuccess = true,
                        PhoneVerified = true,
                        Message = "Phone already verified."
                    };
                }

                // Respect lockout when too many failed attempts with an active OTP
                if (rr.PhoneOtpExpiresAt.HasValue && rr.PhoneOtpExpiresAt.Value >= now && rr.PhoneOtpAttempts >= MaxPhoneOtpAttempts)
                {
                    return new RegistrationPhoneVerificationResponseDto
                    {
                        IsSuccess = false,
                        PhoneVerified = false,
                        Message = "Too many attempts. Please try again later."
                    };
                }

                var otp = GenerateOtp();

                rr.PhoneNumber = request.PhoneNumber;
                rr.PhoneVerificationOtpHash = HashOtp(otp);
                rr.PhoneOtpExpiresAt = now.Add(PhoneOtpTtl);
                rr.PhoneOtpAttempts = 0;

                await _context.SaveChangesAsync();

                // Existing SMS provider isn't wired; use existing email sender as the OTP delivery mechanism.
                // Security: do not return OTP in API response.
                var sent = await _emailService.SendPhoneVerificationAsync(rr.Email, otp);
                if (!sent)
                {
                    _logger.LogWarning("Failed to send staged phone OTP for registration email {Email}", rr.Email);
                }

                return new RegistrationPhoneVerificationResponseDto
                {
                    IsSuccess = true,
                    PhoneVerified = false,
                    Message = "If the request is valid, an OTP has been sent."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending staged phone OTP");
                return new RegistrationPhoneVerificationResponseDto
                {
                    IsSuccess = false,
                    PhoneVerified = false,
                    Message = "An error occurred while sending OTP."
                };
            }
        }

        public async Task<RegistrationPhoneVerificationResponseDto> VerifyRegistrationPhoneOtpAsync(VerifyRegistrationPhoneOtpDto request, string ipAddress)
        {
            try
            {
                var now = DateTime.UtcNow;

                if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Otp))
                {
                    return new RegistrationPhoneVerificationResponseDto
                    {
                        IsSuccess = false,
                        PhoneVerified = false,
                        Message = "Invalid request."
                    };
                }

                var rr = await _context.RegistrationRequests
                    // FirstOrDefaultAsync --> SingleOrDefaultAsync
                    .FirstOrDefaultAsync(r => r.EmailVerificationToken == request.Token);

                if (rr == null || rr.ExpiresAt < now)
                {
                    return new RegistrationPhoneVerificationResponseDto
                    {
                        IsSuccess = false,
                        PhoneVerified = false,
                        Message = "Invalid or expired token."
                    };
                }

                if (!rr.EmailVerified)
                {
                    return new RegistrationPhoneVerificationResponseDto
                    {
                        IsSuccess = false,
                        PhoneVerified = false,
                        Message = "Email verification is required."
                    };
                }

                if (rr.PhoneVerified)
                {
                    return new RegistrationPhoneVerificationResponseDto
                    {
                        IsSuccess = true,
                        PhoneVerified = true,
                        Message = "Phone already verified."
                    };
                }

                if (!rr.PhoneOtpExpiresAt.HasValue || rr.PhoneOtpExpiresAt.Value < now || string.IsNullOrEmpty(rr.PhoneVerificationOtpHash))
                {
                    return new RegistrationPhoneVerificationResponseDto
                    {
                        IsSuccess = false,
                        PhoneVerified = false,
                        Message = "OTP expired. Please request a new one."
                    };
                }

                if (rr.PhoneOtpAttempts >= MaxPhoneOtpAttempts)
                {
                    return new RegistrationPhoneVerificationResponseDto
                    {
                        IsSuccess = false,
                        PhoneVerified = false,
                        Message = "Too many attempts. Please request a new OTP."
                    };
                }

                var isValid = VerifyOtp(request.Otp, rr.PhoneVerificationOtpHash);
                if (!isValid)
                {
                    rr.PhoneOtpAttempts++;
                    await _context.SaveChangesAsync();

                    return new RegistrationPhoneVerificationResponseDto
                    {
                        IsSuccess = false,
                        PhoneVerified = false,
                        Message = "Invalid OTP."
                    };
                }

                rr.PhoneVerified = true;
                rr.PhoneVerifiedAt = now;

                // Invalidate OTP immediately after success (single-use)
                rr.PhoneVerificationOtpHash = null;
                rr.PhoneOtpExpiresAt = null;
                rr.PhoneOtpAttempts = 0;

                await _context.SaveChangesAsync();

                return new RegistrationPhoneVerificationResponseDto
                {
                    IsSuccess = true,
                    PhoneVerified = true,
                    Message = "Phone verified successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying staged phone OTP");
                return new RegistrationPhoneVerificationResponseDto
                {
                    IsSuccess = false,
                    PhoneVerified = false,
                    Message = "An error occurred while verifying OTP."
                };
            }
        }

        private static string GenerateToken()
        {
            // Use random 256-bit token; Base64Url makes it safe for URLs without encoding issues.
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Base64UrlEncode(bytes);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string GenerateOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            return (Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000).ToString("D6");
        }

        private static string HashOtp(string otp)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return Convert.ToBase64String(hash);
        }

        private static bool VerifyOtp(string otp, string expectedHash)
        {
            using var sha256 = SHA256.Create();
            var computedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(otp));
            var expectedBytes = Convert.FromBase64String(expectedHash);
            return CryptographicOperations.FixedTimeEquals(computedBytes, expectedBytes);
        }
    }
}
