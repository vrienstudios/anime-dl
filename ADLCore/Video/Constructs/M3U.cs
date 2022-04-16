using ADLCore.Ext;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ADLCore.Video.Constructs
{
    public class HLSListObject
    {
        public List<string> keys;
        public List<List<string[]>> headerVAL;

        public HLSListObject(string[] m3uList, int idx = 0)
        {
            headerVAL = new List<List<string[]>>();
            keys = new List<string>();
            
            for (; idx < m3uList.Length - 1; idx++)
            {
                List<string[]> vals = new List<string[]>();
                if (m3uList[idx][0] == '#')
                {
                    string[] v = m3uList[idx].Split(':');
                    string title = v[0].Substring(1);
                    string[] f = new string[] { string.Empty, string.Empty };
                    if(v.Length > 1)
                        f = v[1].Split(',');
                    foreach (string foo in f)
                    {
                        string[] nameVP = foo.Split('=');
                        string b = nameVP[0];
                        string c = string.Empty;
                        if (nameVP.Length > 1)
                            c = nameVP[1];
                        vals.Add(new string[] {b, c});
                    }
                    if(m3uList.Length - 1 > idx)
                        if(m3uList[idx++ + 1][0] != '#')
                            vals.Add(new string[]{"URI", m3uList[idx]});
                    headerVAL.Add(vals);
                    keys.Add(title);
                }
            }
        }
    }
    public class m3Object
    {
        public string header;
        public string slug;
        public Byte[] data;

        public m3Object(string a, string b)
        {
            header = a;
            slug = b;
        }
    }

    enum encrpytionType
    {
        AES128
    }

    public class M3UMP4_SETTINGS
    {
        public string Host;
        public string Referer;
        public WebHeaderCollection Headers;

        public bool WAITSTOP = false;
        public int location = -1;

        public HttpWebRequest GenerateWebRequest(string url)
        {
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(url);
            if (Host != string.Empty)
                req.Host = Host;
            if (Referer != string.Empty)
                req.Referer = Referer;
            req.Headers = Headers.Clone();
            req.KeepAlive = true;
            //req.Headers = Headers;
            // req.UseDefaultCredentials(true);
            //req.UserAgent = "Mozilla/5.0";
            return req;
        }
    }

    public class M3U
    {
        public m3Object Current;

        public int Size;
        private bool encrypted;
        private string[] m3u8Info;
        private string progPath;
        private string encKey;
        private string bPath = null;
        private List<string> headers;
        private ExList<m3Object> parts;
        private List<string> streams;
        public int duration = 0;
        public int location = 0;
        WebHeaderCollection collection;
        WebClient webClient;

        private encrpytionType encType;
        private bool mp4 = false;

        private MemoryStream mp4ByteStream;
        public bool downloadComplete = false;
        private FileStream trackingStream;
        private HentaiVideo _hentaiVideo;

        public M3U(string dataToParse, string operatingDir, HentaiVideo video, WebHeaderCollection wc = null,
            string bpath = null, bool mp4 = false, M3UMP4_SETTINGS settings = null)
        {
            collection = wc;
            webClient = new WebClient();
            m3u8Info = dataToParse.Split('\n');
            headers = new List<string>();
            bPath = bpath == null ? null : bpath.TrimToSlash();
            _hentaiVideo = video;

            if (mp4)
            {
                this.mp4 = true;
                ParseMp4(settings);
            }
            else
            {
                progPath = $"{operatingDir}{Path.PathSeparator}{video.name}.mp4.prog";
                if (File.Exists(progPath))
                {
                    string[] dataLine;
                    SetUpTrackingFileStream(progPath, FileMode.Open);
                    using (StreamReader sr = new StreamReader(trackingStream, Encoding.Default, true, 512,
                        leaveOpen: true)) //leaveopen
                        dataLine = sr.ReadLine().Split(':');
                    int.TryParse(dataLine[2], out location);
                }
                else
                    SetUpTrackingFileStream(progPath, FileMode.Create);

                using (var sw = new StreamWriter(trackingStream, Encoding.Default, leaveOpen: true, bufferSize: 512))
                    sw.Write($"{progPath}:{video.slug}:0");

                ParseM3U();
            }
        }

        public M3U(FileStream fs, string dataToParse, WebHeaderCollection wc = null, string bpath = null,
            bool mp4 = false, M3UMP4_SETTINGS settings = null)
        {
            collection = wc.Clone();
            webClient = new WebClient();
            m3u8Info = dataToParse.Split('\n');
            headers = new List<string>();
            bPath = bpath;

            if (mp4)
            {
                this.mp4 = true;
                ParseMp4FS(settings);
            }
            else
                ParseM3U();
        }

        FileStream fileStream;

        HttpWebRequest wRequest;
        public int[] downloadRange;
        const int downloadAmnt = 100000;
        private int aDownloaded = 0;

        public delegate void newBytes(Byte[] bytes);

        public event newBytes onNewBytes;

        private void ParseMp4(M3UMP4_SETTINGS settings)
        {
            WebResponse a = mp4Setup(settings);

            if (settings.location != -1)
            {
                downloadRange[0] = settings.location;
                location = settings.location;
            }

            // Start thread to download file.
            new Thread(() =>
            {
                Thread.CurrentThread.Name = "downloader";
                System.IO.Stream ab;
                while (downloadRange[0] < downloadRange[1])
                {
                    SDG: ;
                    wRequest = settings.GenerateWebRequest(m3u8Info[0]);
                    wRequest.AddRange(downloadRange[0], downloadRange[0] + downloadAmnt);
                    Thread.Sleep(100);
                    try
                    {
                        a = wRequest.GetResponse();
                    }
                    catch
                    {
                        goto SDG;
                    }
                    ab = a.GetResponseStream();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ab.CopyTo(ms);
                        Byte[] arr = ms.ToArray();
                        downloadRange[0] += arr.Length;
                        location += arr.Length;
                        ms.Seek(0, SeekOrigin.Begin);
                        reset.WaitOne();
                        ms.CopyTo(mp4ByteStream);
                        onNewBytes?.Invoke(arr);
                    }
                }

                location = -99;
            }).Start();
        }

        private void SetUpTrackingFileStream(string path, FileMode mode)
        {
            trackingStream = new FileStream(path,
                mode, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        private void IncreaseTrackingInterval(int idx)
        {
            File.WriteAllText(progPath, string.Empty); //OVERWRITE
            using (var sw = new StreamWriter(trackingStream, Encoding.Default, leaveOpen: true, bufferSize: 512))
                sw.Write($"{progPath}:{_hentaiVideo.slug}:{idx}");
        }

        private WebResponse mp4Setup(M3UMP4_SETTINGS settings)
        {
            downloadRange = new int[2];
            //string parsedTitle = info.title.RemoveSpecialCharacters();
            wRequest = settings.GenerateWebRequest(m3u8Info[0]);
            wRequest.AddRange(0, 999999999999);
            //wRequest.Headers.Add("range", "bytes=0-");
            WebResponse a = null;
            try
            {
                a = wRequest.GetResponse();
            }
            catch(WebException ex)
            {
                var response = ex.Response;
                var dataStream = response.GetResponseStream();
                var reader = new StreamReader(dataStream);
                var details = reader.ReadToEnd();
            }
            downloadRange[1] = int.Parse(a.Headers["Content-Length"]);
            downloadRange[0] = 0;
            Size = downloadRange[1];
            mp4ByteStream = new MemoryStream();
            return a;
        }

        private void ParseMp4FS(M3UMP4_SETTINGS settings)
        {
            WebResponse a = mp4Setup(settings);

            if (settings.location != -1)
            {
                downloadRange[0] = settings.location;
                location = settings.location;
            }

            // Start thread to download file.
            new Thread(() =>
            {
                Thread.CurrentThread.Name = "downloader";
                System.IO.Stream ab;
                while (downloadRange[0] < downloadRange[1])
                {
                    wRequest = settings.GenerateWebRequest(m3u8Info[0]);
                    wRequest.AddRange(downloadRange[0], downloadRange[0] + downloadAmnt);
                    a = wRequest.GetResponse();
                    ab = a.GetResponseStream();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ab.CopyTo(ms);
                        Byte[] arr = ms.ToArray();
                        downloadRange[0] += arr.Length;
                        location += arr.Length;
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.CopyTo(mp4ByteStream);
                        onNewBytes?.Invoke(arr);
                    }

                    fileStream.Write(mp4ByteStream.ToArray(), 0, mp4ByteStream.ToArray().Length);
                    mp4ByteStream.SetLength(0);
                }

                location = -99;
            }).Start();
        }

        ManualResetEvent reset = new ManualResetEvent(true);

        private void ParseM3U()
        {
            bool flg = false;
            for (int idx = 0; idx < m3u8Info.Length; idx++)
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

                parts.push_back(new m3Object(m3u8Info[idx],
                    m3u8Info[idx + 1].IsValidUri() ? m3u8Info[idx + 1] : $"{bPath}{m3u8Info[idx + 1]}"));
                idx++;
            }

            for (int idx = 0; idx < headers.Count; idx++)
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
                                throw new Exception(
                                    "There's no decryption support for this encryption method at the moment.");
                        }

                        webClient.Headers = collection.Clone();
                        encKey = webClient.DownloadString(mkpair[1].SkipCharSequence("URI=".ToCharArray()) +
                                                          $":{a[2]}");
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

        public Byte[] getNextStreamBytes()
        {
            void delProg()
            {
                trackingStream?.Dispose();
                if(progPath != null)
                    File.Delete(progPath);
            }
            while (mp4ByteStream.Length < 2048)
            {
                if (location == -99)
                    if (mp4ByteStream.Length > 0)
                    {
                        //continue until stream empty.
                        delProg();
                        break;
                    }
                    else
                        delProg();

                Thread.Sleep(128);
            }

            reset.Reset();
            Byte[] b = mp4ByteStream.ToArray();
            Byte[] buffer = mp4ByteStream.GetBuffer();
            Array.Clear(buffer, 0, buffer.Length);
            mp4ByteStream.Position = 0;
            mp4ByteStream.SetLength(0);
            reset.Set();
            return b;
            ;
        }

        public Byte[] getNext()
        {
            if (mp4)
                return getNextStreamBytes();

            if (parts[0] == null && location == 0)
                location = 1; //GoGoStream manifests sometimes lack a first part.

            if (!getNextAsObject())
                return null;
            webClient.Headers = collection.Clone();
            Retry:
            Byte[] a;
            try
            {
                a = webClient.DownloadData(Current.slug);
            }
            catch (WebException EX)
            {
                if (EX.Status == WebExceptionStatus.ProtocolError)
                    return new byte[] { };
                goto Retry;
            }

            if (encrypted)
                switch (encType)
                {
                    case encrpytionType.AES128:
                        a = DecryptAES128(a, encKey, location, null);
                        break;
                }

            IncreaseTrackingInterval(location);
            return a;
        }

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

            //http://www.openssl.org/docs/crypto/EVP_BytesToKey.html#KEY_DERIVATION_ALGORITHM @#%@#$^#$&@^#$%!!#$^!
            //https://stackoverflow.com/questions/8008253/c-sharp-version-of-openssl-evp-bytestokey-method
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