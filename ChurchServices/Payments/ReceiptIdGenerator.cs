using System.Security.Cryptography;
using ChurchContracts.ChurchContracts;

namespace ChurchServices.Payments
{
    public class ReceiptIdGenerator : IReceiptIdGenerator
    {
        private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int ReceiptIdLength = 6;

        public string Generate()
        {
            Span<char> result = stackalloc char[ReceiptIdLength];
            Span<byte> randomBytes = stackalloc byte[ReceiptIdLength];
            RandomNumberGenerator.Fill(randomBytes);

            for (int i = 0; i < ReceiptIdLength; i++)
            {
                result[i] = AllowedChars[randomBytes[i] % AllowedChars.Length];
            }

            return new string(result);
        }
    }
}
