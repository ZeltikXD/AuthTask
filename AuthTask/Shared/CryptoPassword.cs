using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace AuthTask.Shared
{
    public static class CryptoPassword
    {
        public static (string password, string salt) HashPassword(string password)
        {
            var rndNumber = RandomNumberGenerator.GetInt32(int.MaxValue);
            byte[] saltBytes = UniqueSalt(password + rndNumber);

            string hashPass = Convert.ToBase64String(UsePbkdf2(password, saltBytes));
            return (hashPass, Convert.ToBase64String(saltBytes));
        }

        private static byte[] UniqueSalt(string password)
        {
            byte[] passBytes = Encoding.Unicode.GetBytes(password);
            byte[] newSalt = UsePbkdf2(password, passBytes);
            return newSalt;
        }

        public static bool CheckHash(string attemptedPassword, string hash, string salt)
        {
            string hashed = Convert.ToBase64String(UsePbkdf2(attemptedPassword, Convert.FromBase64String(salt)));
            return hashed == hash;
        }

        private static byte[] UsePbkdf2(string password, byte[] saltBytes)
            => KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 150000,
                numBytesRequested: 512 / 8);
    }
}
