namespace ChurchContracts.Interfaces.Services
{
    public interface ITotpSecretEncryptionService
    {
        /// <summary>
        /// Encrypts a TOTP secret key for storage.
        /// </summary>
        /// <param name="plainSecret">The Base32-encoded TOTP secret</param>
        /// <returns>Encrypted secret (nonce + ciphertext + tag) as Base64</returns>
        string Encrypt(string plainSecret);

        /// <summary>
        /// Decrypts an encrypted TOTP secret for verification.
        /// </summary>
        /// <param name="encryptedSecret">The encrypted secret from database</param>
        /// <returns>The decrypted Base32-encoded TOTP secret, or null if decryption fails</returns>
        string? Decrypt(string encryptedSecret);
    }
}
