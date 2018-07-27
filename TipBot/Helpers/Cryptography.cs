using System;
using System.Security.Cryptography;
using System.Text;

namespace TipBot.Helpers
{
    public static class Cryptography
    {
        private static readonly SHA256 sha256 = new SHA256CryptoServiceProvider();

        /// <summary>Hashes the specified data using SHA256.</summary>
        /// <param name="data">HEX encoded hash.</param>
        public static string Hash(string data)
        {
            byte[] input = StringToBytes(data);

            byte[] output = sha256.ComputeHash(input);

            return ByteArrayToHexString(output);
        }

        /// <summary>Determines whether <paramref name="data"/>'s hash is equal to <paramref name="hash"/>.</summary>
        public static bool IsHashOfData(string data, string hash)
        {
            string actualHash = Hash(data);

            return actualHash == hash.ToLower();
        }

        private static byte[] StringToBytes(string data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(data);

            return bytes;
        }

        private static string BytesToString(byte[] bytes)
        {
            string data = Encoding.ASCII.GetString(bytes);

            return data;
        }

        private static string ByteArrayToHexString(byte[] bytes)
        {
            var hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private static byte[] HexStringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            var bytes = new byte[NumberChars / 2];

            for (var i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return bytes;
        }
    }
}
