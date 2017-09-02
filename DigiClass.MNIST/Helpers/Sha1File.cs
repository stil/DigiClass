using System;
using System.IO;
using System.Security.Cryptography;

namespace DigiClass.MNIST.Helpers
{
    internal static class Sha1File
    {
        public static string CalculateChecksum(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                using (var sha = new SHA1Managed())
                {
                    var checksum = sha.ComputeHash(stream);
                    var sendCheckSum = BitConverter.ToString(checksum)
                        .Replace("-", string.Empty);

                    return sendCheckSum;
                }
            }
        }

        public static bool ValidateChecksum(string expected, string path)
        {
            return string.Equals(
                expected,
                CalculateChecksum(path),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}