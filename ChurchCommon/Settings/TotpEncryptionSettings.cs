namespace ChurchCommon.Settings
{
    public class TotpEncryptionSettings
    {
        /// <summary>
        /// Base64-encoded 256-bit (32 byte) encryption key.
        /// Should be stored in Key Vault or environment variable in production.
        /// </summary>
        public string EncryptionKey { get; set; } = string.Empty;
    }
}
