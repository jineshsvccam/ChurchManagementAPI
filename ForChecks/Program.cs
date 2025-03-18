using System;
using Npgsql;

class Program
{

    static void Main()
    {
        string connString = "Server=finchurch.postgres.database.azure.com;Database=churchdb;Port=5432;User Id=finchurchadmin;Password=January@231986;Ssl Mode=Require;";

        using (var conn = new NpgsqlConnection(connString))
        {
            try
            {
                conn.Open();
                Console.WriteLine("Connection successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Connection failed: {ex.Message}");
            }
        }
    }
    //static void encryption()
    //{
    //    AESEncryptionHelper aESEncryptionHelper = new AESEncryptionHelper(null);
    //    string secretKey = "my32byteSecretKey1234567890abcd";
    //    string originalText = "jiness";
    //    Console.WriteLine("Original Text: " + originalText);

    //    // Encrypt the original text
    //    string cipherText = aESEncryptionHelper.EncryptString(originalText, secretKey);
    //    cipherText = "U2FsdGVkX1/VxZPobs8ueWwzayouYias2VPLLYdr6gU=";
    //    Console.WriteLine("Encrypted Text: " + cipherText);

    //    // Decrypt the generated ciphertext
    //    //string decryptedText = aESEncryptionHelper.DecryptString(cipherText, secretKey);
    //    //Console.WriteLine("Decrypted Text: " + decryptedText);
    //}
}
