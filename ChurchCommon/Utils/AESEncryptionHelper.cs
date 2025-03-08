using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ChurchCommon.Utils
{

    public class AESEncryptionHelper
    {

        private readonly string _secretKey;
        private readonly string _iv;

        public AESEncryptionHelper(IConfiguration configuration)
        {
             _secretKey = configuration.GetSection("Encryption")["SecretKey"];
           // _iv = configuration.GetSection("Encryption")["Iv"];


        }
        public  string EncryptString(string plainText, string passphrase)
        {
            // Generate a random 8-byte salt
            byte[] salt = new byte[8];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            // Derive key and IV from the passphrase and salt
            byte[] key, iv;
            EvpKDF(passphrase, salt, out key, out iv);

            byte[] encrypted;
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                }
            }

            // Prepend "Salted__" and the salt to the encrypted data
            byte[] result = new byte[8 + salt.Length + encrypted.Length];
            Array.Copy(Encoding.ASCII.GetBytes("Salted__"), 0, result, 0, 8);
            Array.Copy(salt, 0, result, 8, salt.Length);
            Array.Copy(encrypted, 0, result, 8 + salt.Length, encrypted.Length);

            // Return the result as a Base64 encoded string
            return Convert.ToBase64String(result);
        }

        // Decrypts ciphertext (in OpenSSL salted format) using the provided passphrase.
        public  string DecryptAES(string cipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            // Extract the salt (after the "Salted__" header)
            byte[] salt = cipherBytes.Skip(8).Take(8).ToArray();
            byte[] encryptedData = cipherBytes.Skip(16).ToArray();

            // Derive key and IV using the same passphrase and extracted salt
            byte[] key, iv;
            EvpKDF(_secretKey, salt, out key, out iv);

            byte[] decrypted;
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    decrypted = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                }
            }
            return Encoding.UTF8.GetString(decrypted);
        }

        // EVP KDF method that derives 48 bytes (32 for key + 16 for IV) from the passphrase and salt.
        private static void EvpKDF(string passphrase, byte[] salt, out byte[] key, out byte[] iv)
        {
            byte[] passBytes = Encoding.UTF8.GetBytes(passphrase);
            byte[] hashedBytes = new byte[48]; // 32 bytes for key + 16 bytes for IV
            byte[] currentHash = new byte[0];
            int bytesFilled = 0;
            using (var md5 = MD5.Create())
            {
                while (bytesFilled < 48)
                {
                    byte[] data;
                    if (currentHash.Length > 0)
                    {
                        data = new byte[currentHash.Length + passBytes.Length + salt.Length];
                        Buffer.BlockCopy(currentHash, 0, data, 0, currentHash.Length);
                        Buffer.BlockCopy(passBytes, 0, data, currentHash.Length, passBytes.Length);
                        Buffer.BlockCopy(salt, 0, data, currentHash.Length + passBytes.Length, salt.Length);
                    }
                    else
                    {
                        data = new byte[passBytes.Length + salt.Length];
                        Buffer.BlockCopy(passBytes, 0, data, 0, passBytes.Length);
                        Buffer.BlockCopy(salt, 0, data, passBytes.Length, salt.Length);
                    }
                    currentHash = md5.ComputeHash(data);
                    int bytesToCopy = Math.Min(currentHash.Length, 48 - bytesFilled);
                    Buffer.BlockCopy(currentHash, 0, hashedBytes, bytesFilled, bytesToCopy);
                    bytesFilled += bytesToCopy;
                }
            }
            key = new byte[32];
            iv = new byte[16];
            Buffer.BlockCopy(hashedBytes, 0, key, 0, 32);
            Buffer.BlockCopy(hashedBytes, 32, iv, 0, 16);
        }
        public string Generate256BitKey()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] key = new byte[32]; // 256 bits = 32 bytes
                rng.GetBytes(key);
                // Return as Base64 string for easy storage and transport
                return Convert.ToBase64String(key);
            }
        }

        private static byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string must have an even length");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}

