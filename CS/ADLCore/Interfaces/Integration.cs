using ADLCore.Ext;
using ADLCore.Integrations;
using ADLCore.Video.Constructs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace ADLCore.Interfaces
{
    public enum listType
    {
        novel,
        anime,
        manga
    }

    public abstract class Integration
    {
        List<Video.Constructs.Video> Items;
        Site integratedSite;

        private Byte[] rawUsername;
        private Byte[] rawPassword;

        public string Username;
        public string Password;

        private ZipArchive integrationArchive;
        ZipArchiveMode mode;
        private FileStream fs;
        private string directory;

        public string integrationID = string.Empty;

        public Integration(Site site, string id = "")
        {
            integrationID = id;
            directory = Path.Join(Directory.GetCurrentDirectory(), "integrations.adl");

            Items = new List<Video.Constructs.Video>();
            integratedSite = site;
            bool k = false;
            if (File.Exists(directory))
                k = true;

            mode = k ? ZipArchiveMode.Update : ZipArchiveMode.Create;

            fs = new FileStream(Path.Join(Directory.GetCurrentDirectory(), "integrations.adl"), FileMode.OpenOrCreate);
            integrationArchive = new ZipArchive(fs, mode);
        }

        public abstract void LoadUserData();
        public abstract listType FindObjectsFromList();

        public void SearchForObjects(int i, int x)
        {
        }

        public void LoadCredentials(string fileName)
        {
        }

        public static void CredentialsTest()
        {
            MyAnimeList integration = new MyAnimeList();
            integration.Login("whentheworldrises", "tothemoontheworldgoes");
            Console.ReadLine();
        }

        public void SaveCredentials(string fileName)
        {
            rawUsername = UTF8Encoding.UTF8.GetBytes(Username);
            rawPassword = UTF8Encoding.UTF8.GetBytes(Password);

            Byte[] key = new byte[16];
            Byte[] iv = new byte[16];
            Byte[][] kp = new byte[2][];
            FillByteArray(ref key);
            kp[0] = key;
            FillByteArray(ref iv);
            Byte[] username = M3U.EncryptAES128(rawUsername, key, iv, 128, 128, PaddingMode.PKCS7);

            Byte[] ivUserPair = new byte[username.Length + iv.Length];
            Array.Copy(username, 0, ivUserPair, 0, username.Length);
            Array.Copy(iv, 0, ivUserPair, username.Length, iv.Length);

            key = new byte[16];
            iv = new byte[16];
            FillByteArray(ref key);
            kp[1] = key;
            FillByteArray(ref iv);
            Byte[] password = M3U.EncryptAES128(rawPassword, key, iv, 128, 128, PaddingMode.PKCS7);


            Byte[] ivPswdPair = new byte[password.Length + iv.Length];

            Array.Copy(password, 0, ivPswdPair, 0, password.Length);
            Array.Copy(iv, 0, ivPswdPair, password.Length, iv.Length);

            ZipArchiveEntry id = mode == ZipArchiveMode.Update
                ? integrationArchive.GetEntry($"{integrationID}.int")
                : integrationArchive.CreateEntry($"{integrationID}.int");
            if (id == null)
                id = integrationArchive.CreateEntry($"{integrationID}.int");
            else if (id != null && mode == ZipArchiveMode.Update)
            {
                id.Delete();
                id = integrationArchive.CreateEntry($"{integrationID}.int");
            }

            using (StreamWriter sw = new StreamWriter(id.Open()))
                sw.Write($"{Convert.ToBase64String(ivUserPair)}:{Convert.ToBase64String(ivPswdPair)}");

            id = mode == ZipArchiveMode.Update
                ? integrationArchive.GetEntry($"ikey.bin")
                : integrationArchive.CreateEntry($"ikey.bin");
            System.IO.Stream keyStream;
            string[] lines = new string[0];

            if (id == null)
                id = integrationArchive.CreateEntry($"ikey.bin");
            else if (id != null && mode == ZipArchiveMode.Update)
            {
                keyStream = id.Open();
                if (keyStream.CanRead)
                {
                    using (StreamReader sr = new StreamReader(keyStream))
                        lines = sr.ReadToEnd().Split("\n");
                }

                id.Delete();
                id = integrationArchive.CreateEntry($"ikey.bin");
            }

            keyStream = id.Open();

            if (lines.Length > 0)
                setKeys(lines, kp);
            else
                lines = new string[1]
                    {$"{integrationID}:{Convert.ToBase64String(kp[0])}:{Convert.ToBase64String(kp[1])}\n"};


            using (StreamWriter sw = new StreamWriter(keyStream))
                foreach (string l in lines)
                    sw.Write(l + "\n");

            integrationArchive.Dispose();
        }

        private void setKeys(string[] lines, Byte[][] kp)
        {
            for (int idx = 0; idx < lines.Length; idx++)
            {
                string[] k = lines[idx].Split(':');
                if (k[0] == integrationID)
                {
                    lines[idx] = $"{integrationID}:{Convert.ToBase64String(kp[0])}:{Convert.ToBase64String(kp[1])}";
                    return;
                }
            }
        }

        private void FillByteArray(ref Byte[] array)
        {
            Random rng = new Random();
            for (int idx = 0; idx < array.Length; idx++)
                array[idx] = (Byte) rng.Next(0, 128); // 0-128 is just random numbers I pulled out of a hat.
        }
    }
}