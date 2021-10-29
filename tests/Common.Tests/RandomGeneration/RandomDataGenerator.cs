using System;
using System.Linq;
using System.Text;

namespace Storage.Tests.RandomGeneration
{
    public static class RandomDataGenerator
    {
        private static readonly Random random = new ();

        public static byte[] RandomBinary(int length)
        {
            return Encoding.UTF8.GetBytes(RandomString(length));
        }
        
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());
        }
    }
}