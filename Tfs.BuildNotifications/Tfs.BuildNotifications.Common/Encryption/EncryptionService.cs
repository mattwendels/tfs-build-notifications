using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Tfs.BuildNotifications.Common.Encryption
{
    public static class EncryptionService
    {
        /* AES encryption using a key and random initialisation vector for the most secure method. This is a modified version
        based on the examples here: http://stackoverflow.com/questions/8041451/good-aes-initialization-vector-practice */

        /// <summary>
        /// Encrypts a string using AES encryption via the key provided.
        /// </summary>
        /// <param name="key">A key to encrpyt/decrypt the data with.</param>
        /// <param name="unencrypted">The string data to encrpyt.</param>
        /// <returns>A Base64 AES encrypted string.</returns>
        public static string AesEncryptString(string key, string toEncrypt)
        {
            var toEncryptBytes = Encoding.UTF8.GetBytes(toEncrypt);
            var keyBytes = Convert.FromBase64String(key);

            using (var aesProvider = new AesCryptoServiceProvider())
            {
                aesProvider.Key = keyBytes;
                aesProvider.Mode = CipherMode.CBC;
                aesProvider.Padding = PaddingMode.PKCS7;

                aesProvider.GenerateIV();

                using (var encryptor = aesProvider.CreateEncryptor(aesProvider.Key, aesProvider.IV))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        //Prepend random, IV to the encrypted ciphertext.
                        memoryStream.Write(aesProvider.IV, 0, 16);

                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(toEncryptBytes, 0, toEncryptBytes.Length);
                            cryptoStream.FlushFinalBlock();
                        }

                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// Decrypts a string using AES decryption via the key provided.
        /// </summary>
        /// <param name="key">A key to decrypt the data with.</param>
        /// <param name="unencrypted">The Base64 AES encrypted string data to decrypt.</param>
        /// <returns>The decrypted UTF8 string.</returns>
        public static string AesDecryptString(string key, string encrypted)
        {
            var encryptedBytes = Convert.FromBase64String(encrypted);
            var keyBytes = Convert.FromBase64String(key);

            using (var aesProvider = new AesCryptoServiceProvider())
            {
                aesProvider.Key = keyBytes;
                aesProvider.Mode = CipherMode.CBC;
                aesProvider.Padding = PaddingMode.PKCS7;

                using (var memoryStream = new MemoryStream(encryptedBytes))
                {
                    var orginalIv = new byte[16];

                    /* The IV should have been prepeneded in the original encryption, extract it here before decrypting
                    the actual data. */
                    memoryStream.Read(orginalIv, 0, 16);

                    aesProvider.IV = orginalIv;

                    using (var decryptor = aesProvider.CreateDecryptor(aesProvider.Key, aesProvider.IV))
                    {
                        using (var cs = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            var decrypted = new byte[encryptedBytes.Length];
                            var byteCount = cs.Read(decrypted, 0, encryptedBytes.Length);

                            return Encoding.UTF8.GetString(decrypted, 0, byteCount);
                        }
                    }
                }
            }
        }
    }
}
