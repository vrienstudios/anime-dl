using ADLCore.Alert;
using ADLCore.Ext;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace ADLCore.Video.Extractors
{
    public class TwistMoe : ExtractorBase
    {
        private WebHeaderCollection whc;
        private HttpWebRequest wRequest;
        private TwistMoeAnimeInfo info;
        private List<TwistMoeAnimeInfo> twistCache;
        //TODO: implement auto lookup
        private Byte[] KEY = Convert.FromBase64String("MjY3MDQxZGY1NWNhMmIzNmYyZTMyMmQwNWVlMmM5Y2Y=");
        //episodes to download: 0-12, 1-12, 5-6 etc.
        //TODO: Implement download ranges for GoGoStream and TwistMoe (and novel downloaders)

        //key  MjY3MDQxZGY1NWNhMmIzNmYyZTMyMmQwNWVlMmM5Y2Y= -> search for atob(e) and floating-player
        public TwistMoe(argumentList args, int ti = -1, Action<int, string> u = null) : base(args, ti, u, Site.TwistMoe)
        {
            ADLUpdates.CallUpdate("Beginning instantiation of TwistMoe Object");
            updateStatus?.Invoke(taskIndex, "Proceeding with setup");
        }

        public override void Begin()
        {
            if (ao.tS)
                ao.term = Search();

            GenerateHeaders();
            videoInfo = new Constructs.Video();
            Download(ao.term, ao.mt, ao.cc);
        }

        //TODO: Implement dual threaded downloading for multithreading.
        //TODO: Implement searching method and caching of json anime list.
        public override bool Download(string path, bool mt, bool continuos)
        {
            List<Episode> episodes = info.episodes;
            if (ao.vRange)
            {
                episodes = new List<Episode>();
                for (int idx = ao.VideoRange[0]; idx < ao.VideoRange[1]; idx++)
                    episodes.Add(info.episodes[idx]);
            }

            for(int idx = 0; idx < episodes.Count; idx++)
            {
                string source = Encoding.UTF8.GetString(M3U.DecryptAES128(Convert.FromBase64String(info.episodes[idx].source), KEY, null, new byte[8], 256));
                source = Uri.EscapeUriString(source);
                downloadVideo("https://cdn.twist.moe" + source, idx);
            }
            return true;
        }

        private void downloadVideo(string url, int number)
        {
            number++;
            string parsedTitle = info.title.RemoveSpecialCharacters();
            
            if (ao.l)
                downloadTo = ao.export;
            else
                if (ao.android)
                    downloadTo = Path.Combine(ao.export, "ADL", "Anime");
                else
                    downloadTo = Path.Combine(ao.export, parsedTitle);

            M3U m3 = new M3U(url, whc, null, true, new M3UMP4_SETTINGS() { Host = "cdn.twist.moe", Referer = $"https://twist.moe/", Headers = whc});
            Byte[] b;
            FileStream fs = null;
            if (ao.stream || ao.streamOnly)
                startStreamServer();
            
            while((b = m3.getNextStreamBytes()) != null) //TODO: Rewrite download continuation code.
            {
                if(ao.streamOnly)
                    videoStream.addNewBytes(b);
                else
                {
                    if (ao.stream)
                        videoStream.addNewBytes(b);

                    // Init
                    if (fs == null)
                    {
                        Directory.CreateDirectory(downloadTo);
                        fs = new FileStream($"{downloadTo}{parsedTitle}_{number}.mp4", FileMode.OpenOrCreate);
                    }

                    fs.Write(b);
                }
            }
        }

        public override void GenerateHeaders()
        {
            whc = new WebHeaderCollection();
            whc.Add("DNT", "1");
            whc.Add("Sec-Fetch-Dest", "document");
            whc.Add("Sec-Fetch-Site", "none");
            whc.Add("Accept", "video/webm,video/ogg,video/*;q=0.9,application/ogg;q=0.7,audio/*;q=0.6,*/*;q=0.5");

            //Get anime slug to use for api
            ADLUpdates.CallUpdate("Getting anime title and episode list from api.twist.moe");
            string k = ao.term.TrimToSlash(keepSlash: false).SkipCharSequence("https://twist.moe/a/".ToCharArray());
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
            //c2335bb06c03720b7f86.js
            wRequest.Headers.Add("x-access-token", "0df14814b9e590a1f26d3071a4ed7974");
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
            string _twistCache = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}twistIndex.json";
            
            if (!File.Exists(_twistCache))
                GenerateTwistCache();
            else
                LoadTwistCache(_twistCache);

            List<TwistMoeAnimeInfo> ordered;
            //if (d)
            ordered = GetSimilarityALT(twistCache);
            //else
             //   ordered = GetSimilarity(twistCache);

            return $"https://twist.moe/a/{ordered.First().slug.slug}/";
        }

        private List<TwistMoeAnimeInfo> GetSimilarity(List<TwistMoeAnimeInfo> twistCache)
        {
            List<TwistMoeAnimeInfo> OrderedSimilarity = new List<TwistMoeAnimeInfo>();
            foreach (TwistMoeAnimeInfo anime in twistCache)
            {
                anime.hb_id = anime.title.getSimilarityScore(ao.term);
                OrderedSimilarity.Add(anime);
            }

            return OrderedSimilarity.OrderBy(x => x.hb_id).ToList();
        }

        private List<TwistMoeAnimeInfo> GetSimilarityALT(List<TwistMoeAnimeInfo> twistCache)
        {
            List<TwistMoeAnimeInfo> OrderedSimilarity = new List<TwistMoeAnimeInfo>();
            foreach (TwistMoeAnimeInfo anime in twistCache)
            {
                anime.hb_id = anime.title.getSimilarityScore(ao.term);
                OrderedSimilarity.Add(anime);
            }

            return OrderedSimilarity.OrderBy(x => x.hb_id).ToList();
        }

        private void GenerateTwistCache(bool exportToDisk = false)
        {
            string data = string.Empty;
            wRequest = (HttpWebRequest)WebRequest.Create("https://api.twist.moe/api/anime");
            wRequestSet();
            WebResponse wb = wRequest.GetResponse();
            string decodedContent = M3U.DecryptBrotliStream(wb.GetResponseStream());
            if(exportToDisk)
            {
                Byte[] bytes = Encoding.UTF8.GetBytes(decodedContent);
                using (FileStream fs = new FileStream($"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}twistIndex.json", FileMode.Create))
                    fs.Write(bytes, 0, bytes.Length);
            }
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
