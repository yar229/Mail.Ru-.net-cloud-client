using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MailRuCloudApi.Api.Crypt
{
    class KeyGenerator
    {

        public KeyGenerator(string password, byte[] iv = null)
        {
            if (iv != null)
            {
                InitVector = iv;
            }
            else
            {
                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(InitVector);
                }
            }

            //System.Security.Cryptography.Oid.RegisterSha2OidInformationForRsa();

            //byte[] salt = CreateRandomSalt(7);
            //PasswordDeriveBytes cdk = new PasswordDeriveBytes(password, null);

            //Rfc2898DeriveBytes cdk = new Rfc2898DeriveBytes(password, salt);
            //SecretKey = cdk.CryptDeriveKey("RC2", "SHA1", 256, InitVector);
            //SecretKey = cdk.CryptDeriveKey("RC4", "MD5", 256, InitVector);
            SecretKey = CreateKey(password, 32);


        }

        public static readonly byte[] Salt = { 10, 20, 30, 40, 50, 60, 70, 80 };

        private static byte[] CreateKey(string password, int keyBytes = 32)
        {
            const int iterations = 300;
            var keyGenerator = new Rfc2898DeriveBytes(password, Salt, iterations);
            return keyGenerator.GetBytes(keyBytes);
        }

        private static byte[] CreateRandomSalt(int length)
        {
            var randBytes = length >= 1 ? new byte[length] : new byte[1];

            RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();
            rand.GetBytes(randBytes);
            return randBytes;
        }

        public byte[] InitVector { get; } = new byte[32];

        public byte[] SecretKey { get; }
    }
}
