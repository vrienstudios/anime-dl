using ADLCore.Ext;
using ADLCore.Video.Constructs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ADLCore.Video.Extractors
{
    public class TwistMoe : ExtractorBase
    {
        private WebHeaderCollection whc;
        private HttpWebRequest wRequest;
        private WebResponse response;
        private TwistMoeAnimeInfo info;
        private Byte[] KEY = Convert.FromBase64String("MjY3MDQxZGY1NWNhMmIzNmYyZTMyMmQwNWVlMmM5Y2Y=");
        //episodes to download: 0-12, 1-12, 5-6 etc.
        //TODO: Implement download ranges for GoGoStream and TwistMoe (and novel downloaders)

        //key  MjY3MDQxZGY1NWNhMmIzNmYyZTMyMmQwNWVlMmM5Y2Y= -> search for atob(e) and floating-player
        public TwistMoe(ArgumentObject args, int ti = -1, Action<int, string> u = null) : base(ti, u)
        {
            GenerateHeaders();
        }

        public override void Begin()
        {
            videoInfo = new Constructs.Video();
            Download(ao.term, ao.mt, ao.cc);
        }

        //TODO: Implement dual threaded downloading for multithreading.
        public override bool Download(string path, bool mt, bool continuos)
        {
            for(int idx = 0; idx < info.episodes.Count; idx++)
            {
                string source = Encoding.UTF8.GetString(M3U.DecryptAES128(Convert.FromBase64String(info.episodes[idx].source), KEY, null, new byte[8], 256));
                downloadVideo(source, idx);
            }
            return true;
        }

        private void downloadVideo(string url, int number)
        {
            int downloadPartAmount = 500000; //500k bytes/0.5mb (at a time)
            int[] downloadRange = new int[2];
            wRequest = (HttpWebRequest)WebRequest.Create(url);
            wRequest.Headers = whc;
            wRequest.Host = "cdn.twist.moe";
            wRequest.Referer = $"https://twist.moe/{info.slug}";
            wRequest.AddRange(0, 999999999999);
            WebResponse a = wRequest.GetResponse();
            
            downloadRange[1] = int.Parse(a.Headers["Content-Length"]);
            downloadRange[0] = 0;

            FileStream fs = new FileStream($"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}Anime{Path.DirectorySeparatorChar}Twist{Path.DirectorySeparatorChar}{info.title.RemoveSpecialCharacters()}_{number}.mp4", FileMode.OpenOrCreate);
            while (downloadRange[0] < downloadRange[1])
            {
                wRequest = (HttpWebRequest)WebRequest.Create(url);
                wRequest.Headers = whc;
                wRequest.Host = "cdn.twist.moe";
                wRequest.Referer = "https://twist.moe/a/18if/2";
                wRequest.AddRange(downloadRange[0], downloadRange[0] + downloadPartAmount);
                a = wRequest.GetResponse();
                a.GetResponseStream().CopyTo(fs);
                downloadRange[0] += downloadPartAmount;
            }
        }

        public override void GenerateHeaders()
        {
            whc = new WebHeaderCollection();
            whc.Add("DNT", "1");
            whc.Add("Sec-Fetch-Dest", "video");
            whc.Add("Sec-Fetch-Site", "same-site");

            //Get anime slug to use for api
            string k = ao.term.TrimToSlash().SkipCharSequence("https://twist.moe/a/".ToCharArray());
            wRequest = (HttpWebRequest)WebRequest.Create($"https://api.twist.moe/api/anime/{k}");
            wRequestSet();
            WebResponse wb = wRequest.GetResponse();
            using (StreamReader str = new StreamReader(wb.GetResponseStream()))
                info = JsonSerializer.Deserialize<TwistMoeAnimeInfo>(str.ReadToEnd());

            wRequest = (HttpWebRequest)WebRequest.Create($"https://api.twist.moe/api/anime/{k}/sources");
            wb = wRequest.GetResponse();
            using (StreamReader str = new StreamReader(wb.GetResponseStream()))
                info.episodes = JsonSerializer.Deserialize<List<Episode>>(str.ReadToEnd());
        }

        private void wRequestSet()
        {
            wRequest.Headers = whc;
            wRequest.Host = "cdn.twist.moe";
        }

        public override dynamic Get(HentaiVideo obj, bool dwnld)
        {
            throw new NotImplementedException();
        }

        public override string GetDownloadUri(string path)
        {
            throw new NotImplementedException();
        }

        public override string GetDownloadUri(HentaiVideo path)
        {
            throw new NotImplementedException();
        }

        public override string Search(string name, bool d = false)
        {
            throw new NotImplementedException();
        }
    }
}
