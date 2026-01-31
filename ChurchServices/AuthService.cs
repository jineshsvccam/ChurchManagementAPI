using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ChurchCommon.Settings;
using ChurchContracts;
using ChurchContracts.Interfaces.Repositories;
using ChurchContracts.Interfaces.Services;
using ChurchData;
using ChurchData.Entities;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    private readonly TwoFactorSettings _twoFactorSettings;
    private readonly ITotpSecretEncryptionService _totpEncryption;
    private readonly ISecurityAuditService _auditService;
    private readonly IVerificationService _verificationService;

    private const int TwoFactorSessionExpiryMinutes = 5;
    private const int MaxSessionAttempts = 5;
    private const int RecoveryCodeCount = 3;
    private const int RecoveryCodeLength = 8;
    private const string FamilyMemberRole = "FamilyMember";

    // Constructor: Sets up dependencies
    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<Role> roleManager,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        ApplicationDbContext context,
        IUser2FASessionRepository sessionRepository,
        IUserAuthenticatorRepository authenticatorRepository,
        IUser2FARecoveryCodeRepository recoveryCodeRepository,
        IOptions<TwoFactorSettings> twoFactorSettings,
        ITotpSecretEncryptionService totpEncryption,
        ISecurityAuditService auditService,
        IVerificationService verificationService)
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
        _twoFactorSettings = twoFactorSettings.Value;
        _totpEncryption = totpEncryption;
        _auditService = auditService;
        _verificationService = verificationService;
    }

    // AuthenticateUserAsync: Checks login and returns token
    public async Task<object> AuthenticateUserAsync(string username, string password, string ipAddress, string userAgent)
    {
        var user = await _context.Users
             .Include(u => u.Family)
             .Include(u => u.Parish)
             .FirstOrDefaultAsync(u => u.UserName == username);

        if (user == null)
        {
            // Audit log: Login failed - user not found
            _auditService.LogFireAndForget(
                SecurityEventType.LoginFailed,
                null,
                $"Login failed: user '{username}' not found",
                ipAddress,
                userAgent,
                AuditSeverity.Warning);

            return new AuthResultDto
            {
                IsSuccess = false,
                AuthStage = "FAILED",
                Message = "User not found."
            };
        }

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            // Audit log: Login failed - invalid password
            _auditService.LogFireAndForget(
                SecurityEventType.LoginFailed,
                user.Id,
                "Login failed: invalid password",
                ipAddress,
                userAgent,
                AuditSeverity.Warning);

            return new AuthResultDto
            {
                IsSuccess = false,
                AuthStage = "FAILED",
                Message = "Invalid password."
            };
        }

        // Mandatory verification gates before any login
        if (!user.EmailConfirmed)
        {
            _auditService.LogFireAndForget(
                SecurityEventType.LoginFailed,
                user.Id,
                "Login blocked: email not verified",
                ipAddress,
                userAgent,
                AuditSeverity.Warning);

            return new
            {
                isSuccess = false,
                authStage = "VERIFICATION_REQUIRED",
                emailVerified = false,
                phoneVerified = user.PhoneNumberConfirmed,
                message = "Email verification is required."
            };
        }

        // Phone verification should be required for privileged roles only.
        var rolesForVerification = await _userManager.GetRolesAsync(user);
        var requiresPhoneVerification = rolesForVerification.Any(r =>
            r.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("Trustee", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("Secretary", StringComparison.OrdinalIgnoreCase));

        if (requiresPhoneVerification && !user.PhoneNumberConfirmed)
        {
            _auditService.LogFireAndForget(
                SecurityEventType.LoginFailed,
                user.Id,
                "Login blocked: phone not verified",
                ipAddress,
                userAgent,
                AuditSeverity.Warning);

            return new
            {
                isSuccess = false,
                authStage = "VERIFICATION_REQUIRED",
                emailVerified = true,
                phoneVerified = false,
                message = "Phone verification is required."
            };
        }

        // Account must be active
        if (user.Status != UserStatus.Active)
        {
            // Audit log: Login failed - account not active
            _auditService.LogFireAndForget(
                SecurityEventType.LoginFailed,
                user.Id,
                $"Login failed: account status is {user.Status}",
                ipAddress,
                userAgent,
                AuditSeverity.Warning);

            return new AuthResultDto
            {
                IsSuccess = false,
                AuthStage = "FAILED",
                Message = "Your account is not approved."
            };
        }

        // Get roles early to use for both 2FA check and response
        var roles = rolesForVerification;

        // Check if 2FA should be enforced for this user
        bool requires2FA = ShouldRequire2FA(user, roles);

        if (requires2FA)
        {
            // Rate limit check: Enforce BEFORE creating new session
            var windowStart = DateTime.UtcNow.AddMinutes(-_twoFactorSettings.RateLimitWindowMinutes);
            var activeSessionCount = await _sessionRepository.CountActiveSessionsAsync(user.Id, windowStart);
            
            if (activeSessionCount >= _twoFactorSettings.MaxSessionsPerUser)
            {
                _logger.LogWarning(
                    "2FA session rate limit exceeded for user {UserId}. Active sessions: {Count}, Max allowed: {Max}",
                    user.Id, activeSessionCount, _twoFactorSettings.MaxSessionsPerUser);
                
                // Audit log: Rate limit exceeded
                _auditService.LogFireAndForget(
                    SecurityEventType.TwoFactorRateLimitExceeded,
                    user.Id,
                    $"Rate limit exceeded. Active sessions: {activeSessionCount}",
                    ipAddress,
                    userAgent,
                    AuditSeverity.Warning);
                
                return new AuthResultDto
                {
                    IsSuccess = false,
                    AuthStage = "FAILED",
                    Message = "Too many login attempts. Please wait a few minutes before trying again."
                };
            }

            var tempToken = GenerateTempToken();
            var clientFingerprint = ComputeClientFingerprint(ipAddress, userAgent);
            
            var session = new User2FASession
            {
                UserId = user.Id,
                TempToken = tempToken,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                ClientFingerprint = clientFingerprint,
                Attempts = 0,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(TwoFactorSessionExpiryMinutes)
            };

            await _sessionRepository.AddAsync(session);

            // Audit log: 2FA challenge created
            _auditService.LogFireAndForget(
                SecurityEventType.TwoFactorChallengeCreated,
                user.Id,
                "2FA challenge issued, awaiting verification",
                ipAddress,
                userAgent,
                AuditSeverity.Info);

            return new TwoFactorRequiredResponseDto
            {
                IsSuccess = true,
                AuthStage = "REQUIRES_2FA",
                TempToken = tempToken,
                IsTwoFactorRequired = true,
                TwoFactorType = user.TwoFactorType ?? "AUTHENTICATOR",
                Message = "Two-factor authentication is required."
            };
        }

        // No 2FA required - issue JWT token
        // Audit log: Login success (no 2FA required)
        _auditService.LogFireAndForget(
            SecurityEventType.LoginSuccess,
            user.Id,
            "Login successful (2FA not required)",
            ipAddress,
            userAgent,
            AuditSeverity.Info);

        // Check if this is the first login
        bool isFirstLogin = user.FirstLoginAt == null;

        // Update FirstLoginAt on first successful login
        if (isFirstLogin)
        {
            user.FirstLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // Determine if user should be prompted to set up 2FA
        bool shouldSetupTwoFactor = ShouldPromptFor2FASetup(user, roles);

        return new AuthResultDto
        {
            IsSuccess = true,
            AuthStage = "COMPLETED",
            Token = GenerateJwtToken(user),
            Message = "Login successful.",
            IsFirstLogin = isFirstLogin,
            IsTwoFactorEnabled = user.TwoFactorEnabled,
            IsTwoFactorRequired = false,
            TwoFactorType = user.TwoFactorType,
            ShouldSetupTwoFactor = shouldSetupTwoFactor,
            FullName = user.FullName,
            ParishId = user.ParishId,
            ParishName = user.Parish?.ParishName,
            FamilyId = user.Family?.FamilyId,
            FamilyNumber=user.Family?.FamilyNumber,
            FamilyName = string.Concat(user.Family?.HeadName, " ", user.Family?.FamilyName),
            Roles = roles.ToList()
        };
    }

    /// <summary>
    /// Determines if user should be prompted to set up 2FA.
    /// Prompts when:
    /// - User has any role other than FamilyMember-only
    /// - AND 2FA is not yet enabled
    /// </summary>
    private bool ShouldPromptFor2FASetup(User user, IList<string> roles)
    {
        // If 2FA is already enabled, no need to prompt
        if (user.TwoFactorEnabled)
            return false;

        // If user has ONLY FamilyMember role, don't prompt
        if (IsOnlyFamilyMember(roles))
            return false;

        // All other roles should be prompted to set up 2FA
        return true;
    }

    /// <summary>
    /// Determines if 2FA should be required for a user based on:
    /// - Global TwoFactorSettings.Enabled
    /// - User's TwoFactorEnabled flag
    /// - User's roles (FamilyMember-only users are exempt, all others require 2FA)
    /// </summary>
    private bool ShouldRequire2FA(User user, IList<string> roles)
    {
        // If global 2FA is disabled, never require 2FA
        if (!_twoFactorSettings.Enabled)
            return false;

        // If user has enabled 2FA, require it regardless of role
        if (user.TwoFactorEnabled)
            return true;

        // If user has ONLY FamilyMember role and has NOT enabled 2FA, don't require
        if (IsOnlyFamilyMember(roles))
            return false;

        // All other roles require 2FA when it's enabled
        return true;
    }

    /// <summary>
    /// Checks if user has only the FamilyMember role.
    /// Returns false if:
    /// - Roles list is null or empty (require 2FA)
    /// - User has any role other than FamilyMember
    /// </summary>
    private bool IsOnlyFamilyMember(IList<string> roles)
    {
        // No roles or empty roles list → require 2FA (not exempt)
        if (roles == null || roles.Count == 0)
            return false;

        // If user has exactly one role and it's FamilyMember → exempt
        if (roles.Count == 1 && roles[0].Equals(FamilyMemberRole, StringComparison.OrdinalIgnoreCase))
            return true;

        // If user has multiple roles, check if ANY role is NOT FamilyMember
        // If so, they need 2FA
        return roles.All(r => r.Equals(FamilyMemberRole, StringComparison.OrdinalIgnoreCase));
    }

    // VerifyTwoFactorAsync: Validates TOTP code or recovery code and manages session
    public async Task<AuthResultDto> VerifyTwoFactorAsync(string tempToken, string code, string ipAddress, string userAgent)
    {
        var session = await _sessionRepository.GetByTempTokenAsync(tempToken);

        if (session == null)
        {
            return new AuthResultDto
            {
                IsSuccess = false,
                AuthStage = "FAILED",
                Message = "Invalid or expired 2FA session."
            };
        }

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            // Audit log: Session expired
            _auditService.LogFireAndForget(
                SecurityEventType.TwoFactorSessionExpired,
                session.UserId,
                "2FA session expired during verification",
                ipAddress,
                userAgent,
                AuditSeverity.Info);

            await _sessionRepository.DeleteAsync(session.SessionId);
            return new AuthResultDto
            {
                IsSuccess = false,
                AuthStage = "FAILED",
                Message = "2FA session has expired. Please login again."
            };
        }

        // Validate client fingerprint (MITM protection)
        if (!ValidateClientFingerprint(session.ClientFingerprint, ipAddress, userAgent))
        {
            _logger.LogWarning(
                "2FA session fingerprint mismatch for user {UserId}. Possible MITM attack.",
                session.UserId);

            // Audit log: Fingerprint mismatch (potential MITM)
            _auditService.LogFireAndForget(
                SecurityEventType.TwoFactorFingerprintMismatch,
                session.UserId,
                "Client fingerprint mismatch - possible MITM attack",
                ipAddress,
                userAgent,
                AuditSeverity.Critical);

            await _sessionRepository.DeleteAsync(session.SessionId);
            return new AuthResultDto
            {
                IsSuccess = false,
                AuthStage = "FAILED",
                Message = "Invalid or expired 2FA session."
            };
        }

        if (session.Attempts >= MaxSessionAttempts)
        {
            // Audit log: Max attempts exceeded
            _auditService.LogFireAndForget(
                SecurityEventType.TwoFactorMaxAttemptsExceeded,
                session.UserId,
                $"Maximum verification attempts ({MaxSessionAttempts}) exceeded",
                ipAddress,
                userAgent,
                AuditSeverity.Warning);

            await _sessionRepository.DeleteAsync(session.SessionId);
            return new AuthResultDto
            {
                IsSuccess = false,
                AuthStage = "FAILED",
                Message = "Maximum verification attempts exceeded. Please login again."
            };
        }

        bool isValidCode = false;
        bool isRecoveryCode = !IsTotpCode(code);
        bool usedRecoveryCode = false;

        if (isRecoveryCode)
        {
            isValidCode = await VerifyRecoveryCodeAsync(session.UserId, code, ipAddress, userAgent);
            usedRecoveryCode = isValidCode;
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
                    AuthStage = "FAILED",
                    Message = "2FA is not properly configured for this account."
                };
            }

            // Decrypt the secret key for verification
            var decryptedSecret = _totpEncryption.Decrypt(authenticator.SecretKey);
            if (decryptedSecret == null)
            {
                _logger.LogError("Failed to decrypt TOTP secret for user {UserId}", session.UserId);
                await _sessionRepository.DeleteAsync(session.SessionId);
                return new AuthResultDto
                {
                    IsSuccess = false,
                    AuthStage = "FAILED",
                    Message = "2FA verification failed. Please contact support."
                };
            }

            // Verify TOTP with replay attack protection
            var (isValid, usedTimeStep, isReplayAttempt) = VerifyTotpCodeWithReplayProtection(
                decryptedSecret, 
                code, 
                authenticator.LastUsedTimeStep,
                session.UserId,
                ipAddress,
                userAgent);

            if (isValid && usedTimeStep.HasValue)
            {
                // Update last used time step to prevent replay
                authenticator.LastUsedTimeStep = usedTimeStep.Value;
                await _authenticatorRepository.UpdateAsync(authenticator);
            }

            isValidCode = isValid;
        }

        if (!isValidCode)
        {
            session.Attempts++;
            await _sessionRepository.UpdateAsync(session);

            // Audit log: Failed verification attempt
            _auditService.LogFireAndForget(
                SecurityEventType.TwoFactorVerificationFailed,
                session.UserId,
                $"Verification failed. Attempt {session.Attempts}/{MaxSessionAttempts}",
                ipAddress,
                userAgent,
                AuditSeverity.Warning);

            var attemptsRemaining = MaxSessionAttempts - session.Attempts;
            return new AuthResultDto
            {
                IsSuccess = false,
                AuthStage = "FAILED",
                Message = attemptsRemaining > 0
                    ? $"Invalid verification code. {attemptsRemaining} attempt(s) remaining."
                    : "Invalid verification code. Maximum attempts exceeded."
            };
        }

        await _sessionRepository.DeleteAsync(session.SessionId);

        var user = await _context.Users
            // .Include(u => u.Family)
            // .Include(u => u.Parish)
            .FirstOrDefaultAsync(u => u.Id == session.UserId);

        if (user == null)
        {
            return new AuthResultDto
            {
                IsSuccess = false,
                AuthStage = "FAILED",
                Message = "User not found."
            };
        }

        // Audit log: Successful 2FA verification
        _auditService.LogFireAndForget(
            SecurityEventType.TwoFactorVerificationSuccess,
            session.UserId,
            usedRecoveryCode ? "Verified using recovery code" : "Verified using TOTP",
            ipAddress,
            userAgent,
            AuditSeverity.Info);

        // Audit log: Login success (after 2FA)
        _auditService.LogFireAndForget(
            SecurityEventType.LoginSuccess,
            session.UserId,
            "Login successful (2FA verified)",
            ipAddress,
            userAgent,
            AuditSeverity.Info);

        var roles = await _userManager.GetRolesAsync(user);

        // Check if this is the first login
        bool isFirstLogin = user.FirstLoginAt == null;

        // Update FirstLoginAt on first successful login (after 2FA)
        if (isFirstLogin)
        {
            user.FirstLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // Determine if user should be prompted to set up 2FA (should be false since they just used 2FA)
        bool shouldSetupTwoFactor = ShouldPromptFor2FASetup(user, roles);

        return new AuthResultDto
        {
            IsSuccess = true,
            AuthStage = "COMPLETED",
            Token = GenerateJwtToken(user),
            Message = "Login successful.",
            IsFirstLogin = isFirstLogin,
            IsTwoFactorEnabled = user.TwoFactorEnabled,
            IsTwoFactorRequired = false,
            TwoFactorType = user.TwoFactorType,
            ShouldSetupTwoFactor = shouldSetupTwoFactor,
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

    private async Task<bool> VerifyRecoveryCodeAsync(Guid userId, string code, string ipAddress, string userAgent)
    {
        var recoveryCodes = await _recoveryCodeRepository.GetUnusedByUserIdAsync(userId);

        foreach (var recoveryCode in recoveryCodes)
        {
            if (VerifyHash(code, recoveryCode.RecoveryCodeHash))
            {
                recoveryCode.IsUsed = true;
                recoveryCode.UsedAt = DateTime.UtcNow;
                await _recoveryCodeRepository.UpdateAsync(recoveryCode);

                // Audit log: Recovery code used
                _auditService.LogFireAndForget(
                    SecurityEventType.RecoveryCodeUsed,
                    userId,
                    "Recovery code successfully used for 2FA verification",
                    ipAddress,
                    userAgent,
                    AuditSeverity.Warning);

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

        // Generate plaintext secret for QR code display
        var plaintextSecret = GenerateSecretKey();
        
        // Encrypt secret before storing
        var encryptedSecret = _totpEncryption.Encrypt(plaintextSecret);
        
        var authenticator = new UserAuthenticator
        {
            UserId = userId,
            SecretKey = encryptedSecret, // Store encrypted
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            VerifiedAt = null
        };

        await _authenticatorRepository.AddAsync(authenticator);

        // Audit log: 2FA setup initiated
        _auditService.LogFireAndForget(
            SecurityEventType.TwoFactorSetupInitiated,
            userId,
            "2FA authenticator setup initiated",
            severity: AuditSeverity.Info);

        // Return plaintext for user to scan (only time it's exposed)
        var qrCodeUri = GenerateQrCodeUri(user.Email ?? user.UserName, plaintextSecret);

        return new EnableAuthenticatorResponseDto
        {
            SecretKey = FormatSecretKey(plaintextSecret),
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

        // Decrypt secret for verification
        var decryptedSecret = _totpEncryption.Decrypt(authenticator.SecretKey);
        if (decryptedSecret == null)
        {
            _logger.LogError("Failed to decrypt TOTP secret during setup verification for user {UserId}", userId);
            throw new InvalidOperationException("2FA setup verification failed. Please try again.");
        }

        // Verify with replay protection (lastUsedTimeStep is null for new setup)
        var (isValid, usedTimeStep, _) = VerifyTotpCodeWithReplayProtection(
            decryptedSecret, 
            code, 
            authenticator.LastUsedTimeStep,
            userId,
            null,
            null);

        if (!isValid)
        {
            throw new InvalidOperationException("Invalid verification code.");
        }

        // Update authenticator with verification time and initial time step
        authenticator.VerifiedAt = DateTime.UtcNow;
        authenticator.LastUsedTimeStep = usedTimeStep;
        await _authenticatorRepository.UpdateAsync(authenticator);

        user.TwoFactorEnabled = true;
        user.TwoFactorType = "AUTHENTICATOR";
        user.TwoFactorEnabledAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var recoveryCodes = await GenerateRecoveryCodesAsync(userId);

        // Audit log: 2FA setup verified and enabled
        _auditService.LogFireAndForget(
            SecurityEventType.TwoFactorSetupVerified,
            userId,
            "2FA authenticator setup verified and enabled",
            severity: AuditSeverity.Info);

        // Audit log: Recovery codes generated
        _auditService.LogFireAndForget(
            SecurityEventType.RecoveryCodesGenerated,
            userId,
            $"{recoveryCodes.Count} recovery codes generated",
            severity: AuditSeverity.Info);

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
        throw new InvalidOperationException(
            "Direct user creation is disabled. Use RegistrationRequestService (register-request -> verify-registration-email -> complete-registration)."
        );
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

    /// <summary>
    /// Verifies a TOTP code with replay attack protection.
    /// Rejects codes that have already been used (same or older time step).
    /// </summary>
    private (bool isValid, long? usedTimeStep, bool isReplayAttempt) VerifyTotpCodeWithReplayProtection(
        string secretKey, 
        string code, 
        long? lastUsedTimeStep,
        Guid userId,
        string ipAddress,
        string userAgent)
    {
        var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var currentTimeStep = unixTime / 30;

        // Check time steps with ±1 tolerance for clock skew
        // But enforce that we only accept NEW time steps (greater than last used)
        for (int i = -1; i <= 1; i++)
        {
            var testTimeStep = currentTimeStep + i;
            var testCode = GenerateTotpCode(secretKey, testTimeStep);
            
            if (testCode == code)
            {
                // Code matches - now check for replay
                if (lastUsedTimeStep.HasValue && testTimeStep <= lastUsedTimeStep.Value)
                {
                    // This code/time step was already used - replay attack!
                    _logger.LogWarning(
                        "TOTP replay attack detected. TimeStep: {TimeStep}, LastUsed: {LastUsed}",
                        testTimeStep, lastUsedTimeStep.Value);

                    // Audit log: Replay attempt
                    _auditService.LogFireAndForget(
                        SecurityEventType.TwoFactorReplayAttempt,
                        userId,
                        $"TOTP replay attack detected. TimeStep: {testTimeStep}, LastUsed: {lastUsedTimeStep.Value}",
                        ipAddress,
                        userAgent,
                        AuditSeverity.Critical);

                    return (false, null, true);
                }
                
                // Valid and not a replay
                return (true, testTimeStep, false);
            }
        }

        // Code doesn't match any valid time step
        return (false, null, false);
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

    /// <summary>
    /// Computes a SHA256 fingerprint of the client's IP address and User-Agent.
    /// Used to bind 2FA sessions to a specific client for MITM protection.
    /// </summary>
    private string ComputeClientFingerprint(string ipAddress, string userAgent)
    {
        // Normalize inputs to handle null/empty values consistently
        var normalizedIp = ipAddress ?? "unknown";
        var normalizedUserAgent = userAgent ?? "unknown";
        
        // Combine IP and User-Agent with a separator that won't appear in either
        var combined = $"{normalizedIp}|{normalizedUserAgent}";
        
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Validates the client fingerprint using constant-time comparison.
    /// Returns true if fingerprint matches or if session has no fingerprint (backward compatibility).
    /// </summary>
    private bool ValidateClientFingerprint(string? storedFingerprint, string ipAddress, string userAgent)
    {
        // Backward compatibility: if no fingerprint stored, skip validation
        if (string.IsNullOrEmpty(storedFingerprint))
            return true;

        var currentFingerprint = ComputeClientFingerprint(ipAddress, userAgent);
        
        // Convert to bytes for constant-time comparison
        var storedBytes = Convert.FromHexString(storedFingerprint);
        var currentBytes = Convert.FromHexString(currentFingerprint);
        
        // Constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(storedBytes, currentBytes);
    }

    // Disables 2FA for a user after verifying the TOTP code
    public async Task<bool> DisableTwoFactorAsync(Guid userId, string code)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        var authenticator = await _authenticatorRepository.GetActiveByUserIdAsync(userId);
        if (authenticator == null || authenticator.VerifiedAt == null)
            throw new InvalidOperationException("2FA is not enabled for this user.");

        // Decrypt secret for verification
        var decryptedSecret = _totpEncryption.Decrypt(authenticator.SecretKey);
        if (decryptedSecret == null)
            throw new InvalidOperationException("2FA secret could not be decrypted.");

        // Verify TOTP code
        var (isValid, _, _) = VerifyTotpCodeWithReplayProtection(
            decryptedSecret,
            code,
            authenticator.LastUsedTimeStep,
            userId,
            null,
            null);

        if (!isValid)
            throw new InvalidOperationException("Invalid verification code.");

        // Disable 2FA
        user.TwoFactorEnabled = false;
        user.TwoFactorType = null;
        user.TwoFactorEnabledAt = null;
        await _context.SaveChangesAsync();
        await _authenticatorRepository.RevokeAllByUserIdAsync(userId);
        await _recoveryCodeRepository.DeleteAllByUserIdAsync(userId);

        // Audit log: 2FA disabled
        _auditService.LogFireAndForget(
            SecurityEventType.TwoFactorDisabled,
            userId,
            "2FA disabled by user",
            severity: AuditSeverity.Info);

        return true;
    }

    // Returns the current 2FA status for a user
    public async Task<TwoFactorStatusDto> GetTwoFactorStatusAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new InvalidOperationException("User not found.");
        return new TwoFactorStatusDto { Enabled = user.TwoFactorEnabled, Type = user.TwoFactorType };
    }
}