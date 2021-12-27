using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ADLCore;
using ADLCore.Ext;
using ADLCore.Ext.ExtendedClasses;
using ADLCore.Interfaces;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using HtmlAgilityPack;

namespace ADLCore.Video.Extractors
{
    public abstract class ExtractorBase : IAppBase
    {
        public string baseUri;
        public string downloadTo;
        public HtmlDocument docu;
        public IEnumerator<HtmlNode> pageEnumerator;
        public AWebClient webClient;
        public Root rootObj;
        public Constructs.Video videoInfo;
        public int taskIndex;
        public Action<int, string> updateStatus;
        public Site quester;
        public argumentList ao;
        public VideoStream videoStream;
        protected int m3uLocation;

        public ManualResetEvent Aborted;
        public bool allStop = false;


        public ExtractorBase(argumentList a, int ti, Action<int, string> u, Site host)
        {
            Aborted = new ManualResetEvent(false);
            ao = a;
            webClient = new AWebClient();
            if (ti > -1 && u != null)
            {
                taskIndex = ti;
                updateStatus = u;
            }
            else
                throw new Exception("Action not provided when setting taskIndex");

            videoStream = new VideoStream();
            if (ao.stream)
                videoStream.onNewByte += VideoStream_onNewByte;

            quester = host;
        }

        private void VideoStream_onNewByte(byte[] b)
        {
            try
            {
                vlc.GetStream().Write(b, 0, b.Length);
            }
            catch
            {
                Alert.ADLUpdates.CallError(new Exception("Error pushing bytes to vlc stream"));
            }
        }

        protected void invoker()
        {
            Aborted.Set();
        }

        public abstract void Begin();
        public abstract bool Download(string path, bool mt, bool continuos);

        protected bool mergeToMain(string path, byte[] data)
        {
            if (data.Length <= 0)
                return false;

            if (!File.Exists(path))
                File.Create(path).Close();
            using (FileStream fs = new FileStream(path, FileMode.Append))
                fs.Write(data, 0, data.Length);
            return true;
        }

        public abstract String Search(bool d = false);
        public abstract String GetDownloadUri(string path);
        public abstract String GetDownloadUri(HentaiVideo path);
        public abstract void GenerateHeaders();
        public abstract dynamic Get(HentaiVideo obj, bool dwnld);
        public abstract MetaData GetMetaData();

        public void LoadPage(string html)
        {
            docu = new HtmlDocument();
            docu.LoadHtml(html);
            pageEnumerator = docu.DocumentNode.FindAllNodes();
            GC.Collect();
        }

        public virtual void MovePage(string uri)
        {
            webClient.Headers.Clear();
            GenerateHeaders();

            LoadPage(webClient.DownloadString(uri));
        }

        public void CancelDownload(string mdataLock)
        {
            string _base = downloadTo.TrimToSlash();
            string exp = downloadTo += "_tmp";

            using (FileStream fs = new FileStream(exp, FileMode.Create))
            using (ZipArchive zarch = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(zarch.CreateEntry("part").Open()))
                {
                    sw.WriteLine($"{m3uLocation}");
                    sw.WriteLine($"{videoInfo.hentai_video.name}");
                }

                using (StreamWriter sw = new StreamWriter(zarch.CreateEntry("mDat").Open()))
                {
                    sw.WriteLine($"{ao.ToString()}");
                    sw.WriteLine($"{videoInfo.hentai_video.slug}"); //Integrate into all anime downloaders.
                }
            }
        }

        public static ExtractorBase ResumeDownload(string mdataLock, int ti, Action<int, string> u)
        {
            int guragura = 0;
            argumentList rawr;

            using (FileStream fs = new FileStream(mdataLock, FileMode.Open))
            using (ZipArchive zarch = new ZipArchive(fs, ZipArchiveMode.Read))
            {
                using (StreamReader sw = new StreamReader(zarch.GetEntry("part").Open()))
                {
                    guragura = int.Parse(sw.ReadLine());
                }

                using (StreamReader sw = new StreamReader(zarch.GetEntry("mDat").Open()))
                {
                    rawr = new ArgumentObject(sw.ReadLine().Split(' ')).arguments;
                    rawr.resume = true;
                }
            }

            throw new Exception("Not implemented yet");
        }

        TcpClient vlc;

        public void startStreamServer()
        {
            new Thread(() => streamServer()).Start();
            Thread.Sleep(50);
            new Thread(() => startVLC()).Start();
        }

        private void streamServer()
        {
            TcpListener tcp = new TcpListener(IPAddress.Loopback, 3472);
            tcp.Start();
            vlc = tcp.AcceptTcpClient();
        }

        //Throw Bytes to VLC as they download
        public void publishToStream(Byte[] b)
        {
            try
            {
                vlc.GetStream().Write(b, 0, b.Length);
            }
            catch
            {
                Alert.ADLUpdates.CallError(new Exception("Error pushing bytes to vlc stream"));
            }
        }

        //Start VLC on the local IP:3472
        public void startVLC()
        {
            System.Diagnostics.Process VlcProc = new System.Diagnostics.Process();

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform
                .Linux))
                VlcProc.StartInfo.FileName = "vlc";
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices
                .OSPlatform.Windows))
                VlcProc.StartInfo.FileName = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            else
                throw new PlatformNotSupportedException("Platform is not supported for streaming.");

            VlcProc.StartInfo.Arguments = $"-vv tcp/ts://{IPAddress.Loopback}:{3472}";
            VlcProc.Start();
        }

        protected bool CompatibilityCheck()
        {
            switch (quester)
            {
                case Site.HAnime: return true;
                default:
                    throw new Exception("This site does not support Cancellation or Resumation");
            }
        }

        public void ResumeDownload(string mdataLock)
        {
            throw new NotImplementedException();
        }

        public abstract void GrabHome(int amount);

        public void BeginExecution()
        {
            throw new NotImplementedException();
        }
    }
}