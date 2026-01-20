using System.Security.Cryptography;

namespace ChurchCommon.Utils
{
    /// <summary>
    /// Utility for generating cryptographic keys.
    /// Use this to generate a new TOTP encryption key for production.
    /// </summary>
    public static class KeyGenerator
    {
        /// <summary>
        /// Generates a cryptographically secure 256-bit key suitable for AES-GCM encryption.
        /// </summary>
        /// <returns>Base64-encoded 32-byte key</returns>
        public static string GenerateAes256Key()
        {
            var key = new byte[32]; // 256 bits
            RandomNumberGenerator.Fill(key);
            return Convert.ToBase64String(key);
        }

        /// <summary>
        /// Validates that a key is properly formatted and correct length for AES-256.
        /// </summary>
        /// <param name="base64Key">Base64-encoded key to validate</param>
        /// <returns>True if key is valid for AES-256</returns>
        public static bool ValidateAes256Key(string base64Key)
        {
            if (string.IsNullOrEmpty(base64Key))
                return false;

            try
            {
                var keyBytes = Convert.FromBase64String(base64Key);
                return keyBytes.Length == 32;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
