using ADLCore.Ext;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using ADLCore.Ext.ExtendedClasses;

namespace ADLCore.Video.Constructs
{
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
        private VideoData vidData;

        public M3U(string dataToParse, string operatingDir, VideoData video, WebHeaderCollection wc = null,
            string bpath = null, bool mp4 = false, M3UMP4_SETTINGS settings = null)
        {
            collection = wc;
            webClient = new WebClient();
            m3u8Info = dataToParse.Split('\n');

            headers = new List<string>();
            bPath = bpath == null ? null : bpath.TrimToSlash();
            vidData = video;

            if (mp4)
            {
                this.mp4 = true;
                ParseMp4(settings);
            }
            else
                throw new Exception("You can not run HLS through M3U.cs anymore, as it is being deprecated. Use HLSManager.cs instead.");
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
                sw.Write($"{progPath}:{vidData.url}:{idx}");
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
            else
                throw new Exception("Not allowed to use HLS on M3U class anymore.");
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