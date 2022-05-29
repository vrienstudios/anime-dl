using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ADLCore.Ext;

namespace ADLCore
{
    public class Encrpytion
    {
        public static Byte[] DecryptAES128(Byte[] data, string encKey, int location, byte[] enciv, int kSize = 128,
            int blockSize = 128)
        {
            byte[] iv;
            if (enciv == null)
            {
                iv = (location - 1).ToBigEndianBytes();
                iv = new byte[8].Concat(iv).ToArray();
            }
            else
                iv = enciv;

            RijndaelManaged algorithm = GetRijndael(Encoding.ASCII.GetBytes(encKey), iv, kSize, blockSize);

            Byte[] bytes;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, algorithm.CreateDecryptor(), CryptoStreamMode.Write))
                    cs.Write(data, 0, data.Length);
                bytes = ms.ToArray();
            }

            GC.Collect();

            return bytes;
        }

        public static Byte[] DecryptAES128(Byte[] data, Byte[] Key, Byte[] IV, Byte[] saltBuffer = null,
            int kSize = 128, int blockSize = 128, PaddingMode pm = PaddingMode.PKCS7)
        {
            if (IV == null)
                DeriveKeyAndIV(Key, data, saltBuffer, out Key, out IV, out data);

            RijndaelManaged algorithm = GetRijndael(Key, IV, kSize, blockSize, pm);
            Byte[] bytes;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, algorithm.CreateDecryptor(), CryptoStreamMode.Write))
                    cs.Write(data, 0, data.Length);
                bytes = ms.ToArray();
            }

            GC.Collect();

            return bytes;
        }

        public static Byte[] EncryptAES128(Byte[] data, Byte[] Key, Byte[] IV = null, int kSize = 128,
            int blockSize = 128, PaddingMode pm = PaddingMode.None)
        {
            RijndaelManaged algorithm = GetRijndael(Key, IV, kSize, blockSize, pm);

            Byte[] bytes;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, algorithm.CreateEncryptor(Key, IV),
                    CryptoStreamMode.Write))
                    cs.Write(data, 0, data.Length);
                bytes = ms.ToArray();
            }

            GC.Collect();

            return bytes;
        }

        private static RijndaelManaged GetRijndael(Byte[] Key, Byte[] IV, int kSize = 128, int blockSize = 128,
            PaddingMode pm = PaddingMode.None)
        {
            RijndaelManaged Alg = new RijndaelManaged()
            {
                Padding = pm,
                Mode = CipherMode.CBC,
                KeySize = kSize,
                BlockSize = blockSize,
                Key = Key,
            };
            if (IV != null)
                Alg.IV = IV;
            return Alg;
        }

        public static void DeriveKeyAndIV(byte[] p, byte[] source, byte[] salt, out byte[] key, out byte[] iv,
            out byte[] encryptedBytes)
        {
            salt = new byte[8];
            encryptedBytes = new byte[source.Length - salt.Length - 8];

            Buffer.BlockCopy(source, 8, salt, 0, salt.Length);
            Buffer.BlockCopy(source, salt.Length + 8, encryptedBytes, 0, encryptedBytes.Length);

            //Credits To: http://www.openssl.org/docs/crypto/EVP_BytesToKey.html#KEY_DERIVATION_ALGORITHM
            //Credits To: https://stackoverflow.com/questions/8008253/c-sharp-version-of-openssl-evp-bytestokey-method
            List<byte> concatenatedHashes = new List<byte>(48);
            byte[] currentHash = new byte[0];
            MD5 md5 = MD5.Create();
            bool enoughBytesForKey = false;

            while (!enoughBytesForKey)
            {
                int preHashLength = currentHash.Length + p.Length + salt.Length;

                byte[] preHash = new byte[preHashLength];


                Buffer.BlockCopy(currentHash, 0, preHash, 0, currentHash.Length);
                Buffer.BlockCopy(p, 0, preHash, currentHash.Length, p.Length);
                Buffer.BlockCopy(salt, 0, preHash, currentHash.Length + p.Length, salt.Length);

                currentHash = md5.ComputeHash(preHash);
                concatenatedHashes.AddRange(currentHash);

                if (concatenatedHashes.Count >= 48)
                    enoughBytesForKey = true;
            }

            key = new byte[32];
            iv = new byte[16];
            concatenatedHashes.CopyTo(0, key, 0, 32);
            concatenatedHashes.CopyTo(32, iv, 0, 16);
            md5.Clear();
            md5 = null;
        }

        public static String DecryptBrotliStream(System.IO.Stream source)
        {
            using (System.IO.Stream str = source)
            using (BrotliStream bs = new BrotliStream(str, System.IO.Compression.CompressionMode.Decompress))
            using (System.IO.MemoryStream ms = new MemoryStream())
            {
                bs.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                using (StreamReader sr = new StreamReader(ms))
                    return sr.ReadToEnd();
            }
        }
    }
}