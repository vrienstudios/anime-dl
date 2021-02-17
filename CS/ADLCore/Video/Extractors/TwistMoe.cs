using ADLCore.Alert;
using ADLCore.Ext;
using ADLCore.Novels.Models;
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
        private List<TwistMoeAnimeInfo> twistCache;
        private Byte[] KEY = Convert.FromBase64String("MjY3MDQxZGY1NWNhMmIzNmYyZTMyMmQwNWVlMmM5Y2Y=");
        //episodes to download: 0-12, 1-12, 5-6 etc.
        //TODO: Implement download ranges for GoGoStream and TwistMoe (and novel downloaders)

        //key  MjY3MDQxZGY1NWNhMmIzNmYyZTMyMmQwNWVlMmM5Y2Y= -> search for atob(e) and floating-player
        public TwistMoe(argumentList args, int ti = -1, Action<int, string> u = null) : base(args, ti, u)
        {
            ADLUpdates.CallUpdate("Beginning instantiation of TwistMoe Object");
            updateStatus?.Invoke(taskIndex, "Proceeding with setup");
        }

        public override void Begin()
        {
            GenerateHeaders();
            videoInfo = new Constructs.Video();
            Download(ao.term, ao.mt, ao.cc);
        }

        //TODO: Implement dual threaded downloading for multithreading.
        //TODO: Implement searching method and caching of json anime list.
        public override bool Download(string path, bool mt, bool continuos)
        {
            for(int idx = 0; idx < info.episodes.Count; idx++)
            {
                string source = Encoding.UTF8.GetString(M3U.DecryptAES128(Convert.FromBase64String(info.episodes[idx].source), KEY, null, new byte[8], 256));
                downloadVideo("https://cdn.twist.moe" + source, idx);
            }
            return true;
        }

        private void downloadVideo(string url, int number)
        {
            number++;
            int downloadPartAmount = 100000; //500k bytes/0.5mb (at a time)
            int[] downloadRange = new int[2];
            string parsedTitle = info.title.RemoveSpecialCharacters();
            string novelPath;
            
            if (ao.l)
                novelPath = ao.export;
            else
                novelPath = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}Anime{Path.DirectorySeparatorChar}{parsedTitle}{Path.DirectorySeparatorChar}";

            wRequest = (HttpWebRequest)WebRequest.Create(url);
            wRequest.Headers = whc;
            wRequest.Host = "cdn.twist.moe";
            wRequest.Referer = $"https://twist.moe/{info.slug}";
            wRequest.AddRange(0, 999999999999);
            WebResponse a = wRequest.GetResponse();
            
            downloadRange[1] = int.Parse(a.Headers["Content-Length"]);
            downloadRange[0] = 0;
            Directory.CreateDirectory(novelPath);
            if (File.Exists($"{novelPath}{parsedTitle}_{number}.mp4"))
                downloadRange[0] = File.ReadAllBytes($"{novelPath}{parsedTitle}_{number}.mp4").Length;

            FileStream fs = new FileStream($"{novelPath}{parsedTitle}_{number}.mp4", FileMode.OpenOrCreate);

            fs.Position = downloadRange[0];
            while (downloadRange[0] < downloadRange[1])
            {
                updateStatus?.Invoke(taskIndex, $"{downloadRange[0]}/{downloadRange[1]} Bytes downloaded");
                System.IO.Stream ab;
            Retry:;
                try
                {
                    wRequest = (HttpWebRequest)WebRequest.Create(url);
                    wRequest.Headers = whc;
                    wRequest.Host = "cdn.twist.moe";
                    wRequest.Referer = $"https://twist.moe/{info.slug}";
                    wRequest.AddRange(downloadRange[0], downloadRange[0] + downloadPartAmount);
                    a = wRequest.GetResponse();
                    ab = a.GetResponseStream();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ab.CopyTo(ms);
                        downloadRange[0] += ms.ToArray().Length;
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.CopyTo(fs);
                    }
                }
                catch(Exception x)
                {
                    goto Retry;
                }
            }
        }

        public override void GenerateHeaders()
        {
            whc = new WebHeaderCollection();
            whc.Add("DNT", "1");
            whc.Add("Sec-Fetch-Dest", "document");
            whc.Add("Sec-Fetch-Site", "none");

            //Get anime slug to use for api
            ADLUpdates.CallUpdate("Getting anime title and episode list from api.twist.moe");
            string k = ao.term.TrimToSlash(false).SkipCharSequence("https://twist.moe/a/".ToCharArray());
            string uri = $"https://api.twist.moe/api/anime/{k}";
            wRequest = (HttpWebRequest)WebRequest.Create(uri);
            wRequestSet();
            WebResponse wb = wRequest.GetResponse();
            string decodedContent = M3U.DecryptBrotliStream(wb.GetResponseStream());
            info = JsonSerializer.Deserialize<TwistMoeAnimeInfo>(decodedContent);

            wRequest = (HttpWebRequest)WebRequest.Create($"https://api.twist.moe/api/anime/{k}/sources");
            wRequestSet();
            wb = wRequest.GetResponse();
            decodedContent = M3U.DecryptBrotliStream(wb.GetResponseStream());
            info.episodes = JsonSerializer.Deserialize<List<Episode>>(decodedContent);
        }

        private void wRequestSet(bool api = true)
        {
            //wRequest.Headers = whc;
            wRequest.Headers.Add("cache-control", "max-age=0");
            wRequest.Headers.Add("upgrade-insecure-requests", "1");
            wRequest.UseDefaultCredentials = true;
            wRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            wRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.141 Safari/537.36 OPR/73.0.3856.344";
            
            wRequest.Host = $"{(api == true ? "api" : "cdn")}.twist.moe";
            wRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            //            wRequest.Referer = "https://twist.moe";

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

        public override string Search(bool d = false)
        {
            string twistCache = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}twistIndex.json";
            
            if (!File.Exists(twistCache))
                GenerateTwistCache();
            else
                LoadTwistCache(twistCache);

            //TODO: Similarity comparison on title and alt_title;
            //        Return full link generated from anime slug.
            return null;
        }

        private void GenerateTwistCache(bool exportToDisk = false)
        {
            string data = string.Empty;
            wRequest = (HttpWebRequest)WebRequest.Create("https://api.twist.moe/api/anime");
            wRequestSet();
            WebResponse wb = wRequest.GetResponse();
            string decodedContent = M3U.DecryptBrotliStream(wb.GetResponseStream());
            LoadTwistCache(decodedContent);
        }

        private void LoadTwistCache(string json)
        {
            twistCache = JsonSerializer.Deserialize<List<TwistMoeAnimeInfo>>(json);
        }

        public override MetaData GetMetaData()
        {
            throw new NotImplementedException();
        }
    }
}
