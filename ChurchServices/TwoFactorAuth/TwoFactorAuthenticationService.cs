using ChurchContracts;
using ChurchContracts.Interfaces.Repositories;
using ChurchContracts.Interfaces.Services;
using ChurchData;
using ChurchData.Entities;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace ChurchServices.TwoFactorAuth
{
    public class TwoFactorAuthenticationService : ITwoFactorAuthenticationService
    {
        private readonly IUserAuthenticatorRepository _authenticatorRepository;
        private readonly IUser2FARecoveryCodeRepository _recoveryCodeRepository;
        private readonly IUser2FASessionRepository _sessionRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;

        private const int RecoveryCodeCount = 10;
        private const int RecoveryCodeLength = 8;
        private const int TwoFactorSessionExpiryMinutes = 5;
        private const int MaxSessionAttempts = 5;

        public TwoFactorAuthenticationService(
            IUserAuthenticatorRepository authenticatorRepository,
            IUser2FARecoveryCodeRepository recoveryCodeRepository,
            IUser2FASessionRepository sessionRepository,
            IUserRepository userRepository,
            UserManager<User> userManager)
        {
            _authenticatorRepository = authenticatorRepository;
            _recoveryCodeRepository = recoveryCodeRepository;
            _sessionRepository = sessionRepository;
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public async Task<EnableAuthenticatorResponseDto> EnableAuthenticatorAsync(Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
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
                CreatedAt = DateTime.UtcNow
            };

            await _authenticatorRepository.AddAsync(authenticator);

            await _recoveryCodeRepository.DeleteAllByUserIdAsync(userId);
            var recoveryCodes = await GenerateRecoveryCodesAsync(userId);

            var qrCodeUri = GenerateQrCodeUri(user.Email ?? user.UserName, secretKey);

            return new EnableAuthenticatorResponseDto
            {
                SecretKey = FormatSecretKey(secretKey),
                QrCodeUri = qrCodeUri,
                RecoveryCodes = recoveryCodes
            };
        }

        public async Task<bool> VerifyAuthenticatorAsync(Guid userId, string code)
        {
            var authenticator = await _authenticatorRepository.GetActiveByUserIdAsync(userId);
            if (authenticator == null)
            {
                return false;
            }

            var isValid = VerifyTotpCode(authenticator.SecretKey, code);
            if (isValid && authenticator.VerifiedAt == null)
            {
                authenticator.VerifiedAt = DateTime.UtcNow;
                await _authenticatorRepository.UpdateAsync(authenticator);

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user != null)
                {
                    user.TwoFactorEnabled = true;
                    user.TwoFactorType = "AUTHENTICATOR";
                    user.TwoFactorEnabledAt = DateTime.UtcNow;
                    await _userRepository.UpdateUserAsync(user);
                }
            }

            return isValid;
        }

        public async Task DisableAuthenticatorAsync(Guid userId)
        {
            await _authenticatorRepository.RevokeAllByUserIdAsync(userId);
            await _recoveryCodeRepository.DeleteAllByUserIdAsync(userId);
            await _sessionRepository.DeleteAllByUserIdAsync(userId);

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user != null)
            {
                user.TwoFactorEnabled = false;
                user.TwoFactorType = null;
                user.TwoFactorEnabledAt = null;
                await _userRepository.UpdateUserAsync(user);
            }
        }

        public async Task<TwoFactorRequiredResponseDto> CreateTwoFactorSessionAsync(Guid userId, string ipAddress, string userAgent)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var tempToken = GenerateTempToken();
            var session = new User2FASession
            {
                UserId = userId,
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

        public async Task<User?> VerifyTwoFactorLoginAsync(string tempToken, string code)
        {
            var session = await _sessionRepository.GetByTempTokenAsync(tempToken);
            if (session == null || session.ExpiresAt < DateTime.UtcNow)
            {
                return null;
            }

            if (session.Attempts >= MaxSessionAttempts)
            {
                await _sessionRepository.DeleteAsync(session.SessionId);
                return null;
            }

            var authenticator = await _authenticatorRepository.GetActiveByUserIdAsync(session.UserId);
            if (authenticator == null)
            {
                return null;
            }

            var isValid = VerifyTotpCode(authenticator.SecretKey, code);
            if (!isValid)
            {
                session.Attempts++;
                await _sessionRepository.UpdateAsync(session);
                return null;
            }

            await _sessionRepository.DeleteAsync(session.SessionId);

            return await _userRepository.GetUserByIdAsync(session.UserId);
        }

        public async Task<bool> VerifyRecoveryCodeAsync(Guid userId, string code)
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

        public async Task<List<string>> RegenerateRecoveryCodesAsync(Guid userId)
        {
            await _recoveryCodeRepository.DeleteAllByUserIdAsync(userId);
            return await GenerateRecoveryCodesAsync(userId);
        }

        public async Task<IEnumerable<RecoveryCodeDto>> GetRecoveryCodesAsync(Guid userId)
        {
            var recoveryCodes = await _recoveryCodeRepository.GetAllByUserIdAsync(userId);
            return recoveryCodes.Select(rc => new RecoveryCodeDto
            {
                Code = "********",
                IsUsed = rc.IsUsed,
                UsedAt = rc.UsedAt
            });
        }

        public async Task IncrementSessionAttemptsAsync(string tempToken)
        {
            var session = await _sessionRepository.GetByTempTokenAsync(tempToken);
            if (session != null)
            {
                session.Attempts++;
                await _sessionRepository.UpdateAsync(session);

                if (session.Attempts >= MaxSessionAttempts)
                {
                    await _sessionRepository.DeleteAsync(session.SessionId);
                }
            }
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

        private async Task<List<string>> GenerateRecoveryCodesAsync(Guid userId)
        {
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

        private string GenerateTempToken()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
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

        private bool VerifyHash(string code, string hash)
        {
            var computedHash = HashCode(code);
            return computedHash == hash;
        }

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

        private string GenerateQrCodeUri(string accountName, string secretKey)
        {
            var issuer = "FinChurch";
            return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountName)}?secret={secretKey}&issuer={Uri.EscapeDataString(issuer)}";
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
    }
}
