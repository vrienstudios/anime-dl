using anime_dl.Ext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace anime_dl.Video.Constructs
{
    class m3Object
    {
        public string header; public string slug;
        public m3Object(string a, string b)
        {
            header = a; slug = b;
        }
    }

    enum encrpytionType
    {
        AES128
    }

    class M3U
    {
        public m3Object Current;

        public int Size;
        private bool encrypted;
        private string[] m3u8Info;
        private string encKey;
        private List<string> headers;
        private ExList<m3Object> parts;

        public int duration = 0;
        public int location = 0;
        WebHeaderCollection collection;
        WebClient webClient;

        private encrpytionType encType;

        public M3U(string dataToParse, WebHeaderCollection wc = null)
        {
            collection = wc;
            webClient = new WebClient();
            m3u8Info = dataToParse.Split('\n');
            headers = new List<string>();
            ParseM3U();
        }

        private void ParseM3U()
        {
            bool flg = false;
            for(int idx = 0; idx < m3u8Info.Length; idx++)
            {
                if (!flg)
                    if (m3u8Info[idx][0] == '#' && m3u8Info[idx + 1][0] == '#')
                    {
                        headers.Add(m3u8Info[idx]);
                        continue;
                    }
                    else
                    {
                        parts = new ExList<m3Object>((m3u8Info.Length / 2) - (headers.Count - 3), false);
                        flg = true;
                        idx--;
                        continue;
                    }

                if (idx == m3u8Info.Length - 1)
                    break;

                parts.push_back(new m3Object(m3u8Info[idx], m3u8Info[idx + 1]));
                idx++;
            }

            for(int idx = 0; idx < headers.Count; idx++)
            {
                string[] a = headers[idx].Replace("\"", string.Empty).Split(':');
                switch (a[0])
                {
                    case "#EXT-X-KEY":
                        {
                            encrypted = true;
                            string[] mkpair = a[1].Split(',');
                            mkpair[0] = mkpair[0].SkipCharSequence("METHOD=".ToCharArray());
                            switch (mkpair[0])
                            {
                                case "AES-128":
                                    encType = encrpytionType.AES128;
                                    break;
                                default:
                                    throw new Exception("There's no decryption support for this encryption method at the moment.");
                            }
                            webClient.Headers = collection;
                            encKey = webClient.DownloadString(mkpair[1].SkipCharSequence("URI=".ToCharArray()) + $":{a[2]}");
                            break;
                        }
                    case "#EXT-X-TARGETDURATION":
                        duration = int.Parse(a[1]);
                        break;
                }
            }
            Size = parts.Size;
        }

        public bool getNextAsObject() => location == parts.Size ? false : (Current = parts[location++]) == Current;

        public Byte[] getNext()
        {
            if (!getNextAsObject())
                return null;
            
            webClient.Headers = collection;
        Retry:
            Byte[] a;
            try
            {
                a = webClient.DownloadData(Current.slug);
            }
            catch
            {
                goto Retry;
            }

            if (encrypted)
                switch (encType)
                {
                    case encrpytionType.AES128:
                        a = DecryptAES128(a);
                        break;
                }

            return a;
        }

        private Byte[] DecryptAES128(Byte[] data)
        {
            byte[] iv = (location - 1).ToBigEndianBytes();
            iv = new byte[8].Concat(iv).ToArray();

            // HLS uses AES-128 w/ CBC & PKCS7
            RijndaelManaged algorithm = new RijndaelManaged()
            {
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC,
                KeySize = 128,
                BlockSize = 128
            };

            algorithm.Key = Encoding.ASCII.GetBytes(encKey);
            algorithm.IV = iv;

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
    }
}
