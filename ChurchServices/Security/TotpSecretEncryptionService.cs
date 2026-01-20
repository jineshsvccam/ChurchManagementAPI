using System.Security.Cryptography;
using ChurchCommon.Settings;
using ChurchContracts.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChurchServices.Security
{
    /// <summary>
    /// Provides AES-GCM authenticated encryption for TOTP secrets at rest.
    /// Format: [12-byte nonce][ciphertext][16-byte auth tag] encoded as Base64
    /// 
    /// MIGRATION NOTE: This service handles both legacy plaintext (Base32) secrets
    /// and new encrypted secrets transparently during the migration period.
    /// </summary>
    public class TotpSecretEncryptionService : ITotpSecretEncryptionService
    {
        private readonly byte[] _key;
        private readonly ILogger<TotpSecretEncryptionService> _logger;

        private const int NonceSize = 12;  // AES-GCM standard nonce size
        private const int TagSize = 16;    // AES-GCM standard tag size
        private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public TotpSecretEncryptionService(
            IOptions<TotpEncryptionSettings> settings,
            ILogger<TotpSecretEncryptionService> logger)
        {
            _logger = logger;

            if (string.IsNullOrEmpty(settings.Value.EncryptionKey))
            {
                throw new InvalidOperationException(
                    "TOTP encryption key is not configured. Set TotpEncryptionSettings:EncryptionKey in configuration.");
            }

            try
            {
                _key = Convert.FromBase64String(settings.Value.EncryptionKey);
                
                if (_key.Length != 32)
                {
                    throw new InvalidOperationException(
                        "TOTP encryption key must be exactly 256 bits (32 bytes).");
                }
            }
            catch (FormatException)
            {
                throw new InvalidOperationException(
                    "TOTP encryption key must be a valid Base64 string.");
            }
        }

        public string Encrypt(string plainSecret)
        {
            if (string.IsNullOrEmpty(plainSecret))
                throw new ArgumentNullException(nameof(plainSecret));

            var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainSecret);
            var nonce = new byte[NonceSize];
            var ciphertext = new byte[plainBytes.Length];
            var tag = new byte[TagSize];

            // Generate cryptographically secure random nonce
            RandomNumberGenerator.Fill(nonce);

            using var aesGcm = new AesGcm(_key, TagSize);
            aesGcm.Encrypt(nonce, plainBytes, ciphertext, tag);

            // Combine: nonce + ciphertext + tag
            var result = new byte[NonceSize + ciphertext.Length + TagSize];
            Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
            Buffer.BlockCopy(ciphertext, 0, result, NonceSize, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, NonceSize + ciphertext.Length, TagSize);

            // Clear sensitive data from memory
            CryptographicOperations.ZeroMemory(plainBytes);

            return Convert.ToBase64String(result);
        }

        public string? Decrypt(string encryptedSecret)
        {
            if (string.IsNullOrEmpty(encryptedSecret))
                return null;

            // Check if this is a legacy plaintext Base32 secret (not encrypted)
            if (IsPlaintextBase32Secret(encryptedSecret))
            {
                _logger.LogDebug("Detected legacy plaintext TOTP secret, returning as-is");
                return encryptedSecret;
            }

            try
            {
                var combined = Convert.FromBase64String(encryptedSecret);

                if (combined.Length < NonceSize + TagSize + 1)
                {
                    _logger.LogWarning("Invalid encrypted secret format: data too short");
                    return null;
                }

                var nonce = new byte[NonceSize];
                var ciphertextLength = combined.Length - NonceSize - TagSize;
                var ciphertext = new byte[ciphertextLength];
                var tag = new byte[TagSize];

                Buffer.BlockCopy(combined, 0, nonce, 0, NonceSize);
                Buffer.BlockCopy(combined, NonceSize, ciphertext, 0, ciphertextLength);
                Buffer.BlockCopy(combined, NonceSize + ciphertextLength, tag, 0, TagSize);

                var plainBytes = new byte[ciphertextLength];

                using var aesGcm = new AesGcm(_key, TagSize);
                aesGcm.Decrypt(nonce, ciphertext, tag, plainBytes);

                var result = System.Text.Encoding.UTF8.GetString(plainBytes);

                // Clear sensitive data from memory
                CryptographicOperations.ZeroMemory(plainBytes);

                return result;
            }
            catch (FormatException)
            {
                // Not valid Base64 - might be legacy plaintext
                _logger.LogWarning("Secret is not valid Base64, checking if plaintext");
                if (IsPlaintextBase32Secret(encryptedSecret))
                {
                    return encryptedSecret;
                }
                return null;
            }
            catch (CryptographicException ex)
            {
                // Authentication failed - tampered data or wrong key
                // Could also be a plaintext secret that happens to be valid Base64
                _logger.LogWarning(ex, "TOTP secret decryption failed, checking if legacy plaintext");
                
                // Final fallback: if it looks like a valid Base32 TOTP secret, return it
                if (IsPlaintextBase32Secret(encryptedSecret))
                {
                    _logger.LogInformation("Treating as legacy plaintext TOTP secret");
                    return encryptedSecret;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during TOTP secret decryption");
                return null;
            }
        }

        /// <summary>
        /// Checks if a string appears to be a plaintext Base32-encoded TOTP secret.
        /// Valid TOTP secrets are typically 16-32 characters of Base32 (A-Z, 2-7).
        /// </summary>
        private bool IsPlaintextBase32Secret(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            // Remove any spaces (formatted secrets may have spaces)
            var cleaned = value.Replace(" ", "").ToUpperInvariant();

            // TOTP secrets are typically 16-32 Base32 characters (80-160 bits)
            // Common lengths: 16 (80-bit), 20 (100-bit), 32 (160-bit)
            if (cleaned.Length < 16 || cleaned.Length > 64)
                return false;

            // Check if all characters are valid Base32
            foreach (var c in cleaned)
            {
                if (!Base32Chars.Contains(c))
                    return false;
            }

            return true;
        }
    }
}
