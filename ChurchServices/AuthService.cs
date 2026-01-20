using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ChurchContracts;
using ChurchContracts.Interfaces.Repositories;
using ChurchData;
using ChurchData.Entities;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<AuthService> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IUser2FASessionRepository _sessionRepository;
    private readonly IUserAuthenticatorRepository _authenticatorRepository;
    private readonly IUser2FARecoveryCodeRepository _recoveryCodeRepository;

    private const int TwoFactorSessionExpiryMinutes = 5;
    private const int MaxSessionAttempts = 5;
    private const int RecoveryCodeCount = 3;
    private const int RecoveryCodeLength = 8;

    // Constructor: Sets up dependencies
    public AuthService(UserManager<User> userManager, SignInManager<User> signInManager,
        RoleManager<Role> roleManager, IConfiguration configuration,
        ILogger<AuthService> logger, ApplicationDbContext context,
        IUser2FASessionRepository sessionRepository,
        IUserAuthenticatorRepository authenticatorRepository,
        IUser2FARecoveryCodeRepository recoveryCodeRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
        _context = context;
        _sessionRepository = sessionRepository;
        _authenticatorRepository = authenticatorRepository;
        _recoveryCodeRepository = recoveryCodeRepository;
    }

    // AuthenticateUserAsync: Checks login and returns token
    public async Task<object> AuthenticateUserAsync(string username, string password, string ipAddress, string userAgent)
    {
        var user = await _context.Users
             .Include(u => u.Family)
             .Include(u => u.Parish)
             .FirstOrDefaultAsync(u => u.UserName == username);

        if (user == null)
            return new AuthResultDto
            {
                IsSuccess = false,
                Message = "User not found."
            };

        if (!await _userManager.CheckPasswordAsync(user, password))
            return new AuthResultDto
            {
                IsSuccess = false,
                Message = "Invalid password."
            };

        if (user.Status != UserStatus.Active)
            return new AuthResultDto
            {
                IsSuccess = false,
                Message = "Your account is not approved."
            };

        // If user has TwoFactorEnabled, create a session and return temp token
        if (user.TwoFactorEnabled)
        {
            var tempToken = GenerateTempToken();
            var session = new User2FASession
            {
                UserId = user.Id,
                TempToken = tempToken,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Attempts = 0,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(TwoFactorSessionExpiryMinutes)
            };

            await _sessionRepository.AddAsync(session);

            return new TwoFactorRequiredResponseDto
            {
                TempToken = tempToken,
                TwoFactorType = user.TwoFactorType ?? "AUTHENTICATOR",
                Message = "Two-factor authentication is required."
            };
        }

        // Get roles for the user
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResultDto
        {
            IsSuccess = true,
            Token = GenerateJwtToken(user),
            Message = "Login successful.",
            FullName = user.FullName,
            ParishId = user.ParishId,
            ParishName = user.Parish?.ParishName,
            FamilyId = user.Family?.FamilyNumber,
            FamilyName = string.Concat(user.Family?.HeadName, " ", user.Family?.FamilyName),
            Roles = roles.ToList()
        };
    }

    // VerifyTwoFactorAsync: Validates TOTP code or recovery code and manages session
    public async Task<AuthResultDto> VerifyTwoFactorAsync(string tempToken, string code)
    {
        var session = await _sessionRepository.GetByTempTokenAsync(tempToken);

        if (session == null)
        {
            return new AuthResultDto
            {
                IsSuccess = false,
                Message = "Invalid or expired 2FA session."
            };
        }

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            await _sessionRepository.DeleteAsync(session.SessionId);
            return new AuthResultDto
            {
                IsSuccess = false,
                Message = "2FA session has expired. Please login again."
            };
        }

        if (session.Attempts >= MaxSessionAttempts)
        {
            await _sessionRepository.DeleteAsync(session.SessionId);
            return new AuthResultDto
            {
                IsSuccess = false,
                Message = "Maximum verification attempts exceeded. Please login again."
            };
        }

        bool isValidCode = false;
        bool isRecoveryCode = !IsTotpCode(code);

        if (isRecoveryCode)
        {
            isValidCode = await VerifyRecoveryCodeAsync(session.UserId, code);
        }
        else
        {
            var authenticator = await _authenticatorRepository.GetActiveByUserIdAsync(session.UserId);

            if (authenticator == null)
            {
                await _sessionRepository.DeleteAsync(session.SessionId);
                return new AuthResultDto
                {
                    IsSuccess = false,
                    Message = "2FA is not properly configured for this account."
                };
            }

            isValidCode = VerifyTotpCode(authenticator.SecretKey, code);
        }

        if (!isValidCode)
        {
            session.Attempts++;
            await _sessionRepository.UpdateAsync(session);

            var attemptsRemaining = MaxSessionAttempts - session.Attempts;
            return new AuthResultDto
            {
                IsSuccess = false,
                Message = attemptsRemaining > 0
                    ? $"Invalid verification code. {attemptsRemaining} attempt(s) remaining."
                    : "Invalid verification code. Maximum attempts exceeded."
            };
        }

        await _sessionRepository.DeleteAsync(session.SessionId);

        var user = await _context.Users
            .Include(u => u.Family)
            .Include(u => u.Parish)
            .FirstOrDefaultAsync(u => u.Id == session.UserId);

        if (user == null)
        {
            return new AuthResultDto
            {
                IsSuccess = false,
                Message = "User not found."
            };
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResultDto
        {
            IsSuccess = true,
            Token = GenerateJwtToken(user),
            Message = "Login successful.",
            FullName = user.FullName,
            ParishId = user.ParishId,
            ParishName = user.Parish?.ParishName,
            FamilyId = user.Family?.FamilyNumber,
            FamilyName = string.Concat(user.Family?.HeadName, " ", user.Family?.FamilyName),
            Roles = roles.ToList()
        };
    }

    private bool IsTotpCode(string code)
    {
        return code.Length == 6 && code.All(char.IsDigit);
    }

    private async Task<bool> VerifyRecoveryCodeAsync(Guid userId, string code)
    {
        var recoveryCodes = await _recoveryCodeRepository.GetUnusedByUserIdAsync(userId);

        foreach (var recoveryCode in recoveryCodes)
        {
            if (VerifyHash(code, recoveryCode.RecoveryCodeHash))
            {
                recoveryCode.IsUsed = true;
                recoveryCode.UsedAt = DateTime.UtcNow;
                await _recoveryCodeRepository.UpdateAsync(recoveryCode);
                return true;
            }
        }

        return false;
    }

    private bool VerifyHash(string code, string hash)
    {
        var computedHash = HashCode(code);
        return computedHash == hash;
    }

    // EnableAuthenticatorAsync: Sets up and enables authenticator app for 2FA
    public async Task<EnableAuthenticatorResponseDto> SetupTwoFactorAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        await _authenticatorRepository.RevokeAllByUserIdAsync(userId);

        var secretKey = GenerateSecretKey();
        var authenticator = new UserAuthenticator
        {
            UserId = userId,
            SecretKey = secretKey,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            VerifiedAt = null
        };

        await _authenticatorRepository.AddAsync(authenticator);

        var qrCodeUri = GenerateQrCodeUri(user.Email ?? user.UserName, secretKey);

        return new EnableAuthenticatorResponseDto
        {
            SecretKey = FormatSecretKey(secretKey),
            QrCodeUri = qrCodeUri,
            RecoveryCodes = new List<string>()
        };
    }

    // VerifySetupTwoFactorAsync: Verifies the setup of the authenticator app
    public async Task<EnableAuthenticatorResponseDto> VerifySetupTwoFactorAsync(Guid userId, string code)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var authenticator = await _authenticatorRepository.GetActiveByUserIdAsync(userId);
        if (authenticator == null)
        {
            throw new InvalidOperationException("No active authenticator found. Please setup 2FA first.");
        }

        if (authenticator.VerifiedAt != null)
        {
            throw new InvalidOperationException("Authenticator is already verified.");
        }

        var isValidCode = VerifyTotpCode(authenticator.SecretKey, code);
        if (!isValidCode)
        {
            throw new InvalidOperationException("Invalid verification code.");
        }

        authenticator.VerifiedAt = DateTime.UtcNow;
        await _authenticatorRepository.UpdateAsync(authenticator);

        user.TwoFactorEnabled = true;
        user.TwoFactorType = "AUTHENTICATOR";
        user.TwoFactorEnabledAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var recoveryCodes = await GenerateRecoveryCodesAsync(userId);

        return new EnableAuthenticatorResponseDto
        {
            SecretKey = string.Empty,
            QrCodeUri = string.Empty,
            RecoveryCodes = recoveryCodes
        };
    }

    // RegisterUserAsync: Creates user and assigns roles
    public async Task<User?> RegisterUserAsync(RegisterDto model)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                EmailConfirmed = true,
                ParishId = model.ParishId,
                FamilyId = model.FamilyId,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Status = UserStatus.Pending
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"User creation failed: {errors}");
            }

            foreach (var roleId in model.RoleIds)
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role == null)
                    throw new Exception($"Role with ID {roleId} not found.");

                var isInRole = await _userManager.IsInRoleAsync(user, role.Name);
                if (!isInRole)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roleId,
                        Status = RoleStatus.Pending,
                        ApprovedBy = null,
                        ApprovedAt = null
                    };
                    _context.UserRoles.Add(userRole);
                }
            }

            await _context.SaveChangesAsync(); // Save all UserRoles at once
            await transaction.CommitAsync();
            return user;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during user registration for {Username}", model.Username);
            throw;
        }
    }
    // GenerateJwtToken: Creates JWT for user
    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var userRoles = _userManager.GetRolesAsync(user).Result; // Get user roles
        foreach (var role in userRoles)
            claims.Add(new Claim(ClaimTypes.Role, role)); // Add roles to claims

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])); // JWT key
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1), // Expires in 1 hour
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token); // Return token string
    }

    // GenerateTempToken: Creates a temporary token for 2FA
    private string GenerateTempToken()
    {
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes);
    }

    // VerifyTotpCode: Validates a TOTP code against the secret key
    private bool VerifyTotpCode(string secretKey, string code)
    {
        var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timeStep = unixTime / 30;

        for (int i = -1; i <= 1; i++)
        {
            var testCode = GenerateTotpCode(secretKey, timeStep + i);
            if (testCode == code)
            {
                return true;
            }
        }

        return false;
    }

    // GenerateTotpCode: Creates a TOTP code based on the secret key and time step
    private string GenerateTotpCode(string secretKey, long timeStep)
    {
        var keyBytes = Base32Decode(secretKey);
        var timeBytes = BitConverter.GetBytes(timeStep);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timeBytes);
        }

        using (var hmac = new HMACSHA1(keyBytes))
        {
            var hash = hmac.ComputeHash(timeBytes);
            var offset = hash[hash.Length - 1] & 0x0F;
            var binary = ((hash[offset] & 0x7F) << 24)
                | ((hash[offset + 1] & 0xFF) << 16)
                | ((hash[offset + 2] & 0xFF) << 8)
                | (hash[offset + 3] & 0xFF);

            var otp = binary % 1000000;
            return otp.ToString("D6");
        }
    }

    private byte[] Base32Decode(string encoded)
    {
        const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        encoded = encoded.Replace(" ", "").ToUpper();

        var result = new List<byte>();
        int bits = 0;
        int value = 0;

        foreach (var c in encoded)
        {
            var index = base32Chars.IndexOf(c);
            if (index < 0)
            {
                continue;
            }

            value = (value << 5) | index;
            bits += 5;

            if (bits >= 8)
            {
                bits -= 8;
                result.Add((byte)((value >> bits) & 0xFF));
            }
        }

        return result.ToArray();
    }

    private string GenerateSecretKey()
    {
        var bytes = new byte[20];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Base32Encode(bytes);
    }

    private string Base32Encode(byte[] data)
    {
        const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new StringBuilder();
        int bits = 0;
        int value = 0;

        foreach (var b in data)
        {
            value = (value << 8) | b;
            bits += 8;

            while (bits >= 5)
            {
                bits -= 5;
                result.Append(base32Chars[(value >> bits) & 0x1F]);
            }
        }

        if (bits > 0)
        {
            result.Append(base32Chars[(value << (5 - bits)) & 0x1F]);
        }

        return result.ToString();
    }

    private string FormatSecretKey(string key)
    {
        var formatted = new StringBuilder();
        for (int i = 0; i < key.Length; i++)
        {
            if (i > 0 && i % 4 == 0)
            {
                formatted.Append(' ');
            }
            formatted.Append(key[i]);
        }
        return formatted.ToString();
    }

    private string GenerateQrCodeUri(string accountName, string secretKey)
    {
        var issuer = "FinChurch";
        return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountName)}?secret={secretKey}&issuer={Uri.EscapeDataString(issuer)}";
    }

    private async Task<List<string>> GenerateRecoveryCodesAsync(Guid userId)
    {
        await _recoveryCodeRepository.DeleteAllByUserIdAsync(userId);

        var codes = new List<string>();
        var entities = new List<User2FARecoveryCode>();

        for (int i = 0; i < RecoveryCodeCount; i++)
        {
            var code = GenerateRecoveryCode();
            codes.Add(code);

            entities.Add(new User2FARecoveryCode
            {
                UserId = userId,
                RecoveryCodeHash = HashCode(code),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _recoveryCodeRepository.AddRangeAsync(entities);
        return codes;
    }

    private string GenerateRecoveryCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var bytes = new byte[RecoveryCodeLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }

    private string HashCode(string code)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(code);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}