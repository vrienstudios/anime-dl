using anime_dl.Ext;
using anime_dl.Video.Constructs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace anime_dl.Video.Extractors
{
    class HAnime : ExtractorBase
    {
        string term, path;
        bool mt, continuos; //Yes, I know this is mis-spelled
        public HAnime(string term, bool mt = false, string path = null, bool continuos = false, int ti = -1, Action<int, string> statusUpdate = null) : base(ti, statusUpdate)
        {
            this.term = term;
            this.mt = mt;
            this.path = path;
            this.continuos = continuos;
        }

        public void Begin()
        {
            downloadTo = path;
            if (term.IsValidUri())
                Download(term, mt, continuos);
            else
                Download(Search(term), mt, continuos);
        }

        private bool ExtractDataFromVideo()
        {
            return false;
        }

        public override bool Download(string path, bool mt, bool continuos)
        {
            GetDownloadUri(videoInfo == null ? new HentaiVideo { slug = path } : videoInfo.hentai_video);
            if (downloadTo == null)
                downloadTo = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}HAnime{Path.DirectorySeparatorChar}{videoInfo.hentai_video.name.TrimIntegrals()}{Path.DirectorySeparatorChar}";

            Directory.CreateDirectory(downloadTo);

            M3U m3 = new M3U(webClient.DownloadString(rootObj.linkToManifest));

            Byte[] b;
            int l = m3.Size;
            double prg;
            updateStatus(taskIndex, $"Beginning download of {videoInfo.hentai_video.name}");
            while ((b = m3.getNext()) != null)
            {
                prg  = (double)m3.location / (double)l;
                updateStatus(taskIndex, $"{videoInfo.hentai_video.name} [{new string('#', (int)(prg * 10))}{new string(' ', 10 - (int)(prg * 10))}] {(int)(prg * 100)}% {m3.location}/{l}");
                mergeToMain(downloadTo + videoInfo.hentai_video.name + ".mp4", b);
            }

            if (continuos && videoInfo.next_hentai_video.name.RemoveSpecialCharacters().TrimIntegrals() == videoInfo.hentai_video.name.TrimIntegrals())
            {
                HAnime h = new HAnime($"https://hanime.tv/videos/hentai/{videoInfo.next_hentai_video.slug}", mt, downloadTo, continuos);
            }

            return true;
        }

        public override void GenerateHeaders()
        {
            throw new NotImplementedException();
        }

        public override string GetDownloadUri(string path)
        {
            string Data = webClient.DownloadString(path);

            Regex reg = new Regex("(?<=<script>window\\.__NUXT__=)(.*)(?=;</script>)");
            Match mc = reg.Match(Data); // Grab json
            // Make it "parsable"
            string a = mc.Value;
            rootObj = JsonSerializer.Deserialize<Root>(a);
            rootObj.state.data.video.hentai_video.name = rootObj.state.data.video.hentai_video.name.RemoveSpecialCharacters();
            rootObj.linkToManifest = $"https://weeb.hanime.tv/weeb-api-cache/api/v8/m3u8s/{rootObj.state.data.video.videos_manifest.servers[0].streams[0].id.ToString()}.m3u8";
            if(videoInfo != null)
                videoInfo.hentai_video = rootObj.state.data.video.hentai_video;
            return $"https://weeb.hanime.tv/weeb-api-cache/api/v8/m3u8s/{rootObj.state.data.video.videos_manifest.servers[0].streams[0].id.ToString()}.m3u8";
        }

        //TODO: Wrap pagination around bufferheight of console.
        public override string Search(string name)
        {
            int np = 0;
            string a;
        a:
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://search.htv-services.com/");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                string json = $"{{\"search_text\":\"{name}\",\"tags\":[],\"tags_mode\":\"AND\",\"brands\":[],\"blacklist\":[],\"order_by\":\"released_at_unix\",\"ordering\":\"asc\",\"page\":{np.ToString()}}}";

                using (StreamWriter sw = new StreamWriter(httpWebRequest.GetRequestStream()))
                    sw.Write(json);

                HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    a = sr.ReadToEnd();

                SearchReq sj = JsonSerializer.Deserialize<SearchReq>(a);

                Program.WriteToConsole($"Hits: {sj.actualHits.Count} {np}/{sj.nbPages} page");

                for (int idx = 0; idx < sj.actualHits.Count; idx++)
                    Program.WriteToConsole($"{idx} -- {sj.actualHits[idx].name} | Ratings: {sj.actualHits[idx].GetRating()}/10\n       tags:{sj.actualHits[idx].tagsAsString()}\n       desc:{new string(sj.actualHits[idx].description.Replace("<p>", string.Empty).Replace("</p>", string.Empty).Replace("\n", string.Empty).Take(60).ToArray())}\n\n", true);

                Program.WriteToConsole($"\nCommands: \n     page {{page}}/{sj.nbPages}\n     select {{episode num}}", true);
            c:
                String[] input = Console.ReadLine().ToLower().Split(' ');

                switch (input[0])
                {
                    case "select":
                        videoInfo = new Constructs.Video() { hentai_video = new HentaiVideo() { slug = $"https://hanime.tv/videos/hentai/{sj.actualHits[int.Parse(input[1])].slug}"} };
                        return $"https://hanime.tv/videos/hentai/{sj.actualHits[int.Parse(input[1])].slug}";
                    case "page":
                        Console.Clear();
                        np = int.Parse(input[1]);
                        goto a;
                    default:
                        goto c;
                }
            }
            catch
            {
                goto a;
            }
        }

        public override string GetDownloadUri(HentaiVideo vid)
        {
            Program.WriteToConsole($"Extracting Download URL for {vid.slug}");
            string Data = webClient.DownloadString(vid.slug);

            Regex reg = new Regex("(?<=<script>window\\.__NUXT__=)(.*)(?=;</script>)");
            Match mc = reg.Match(Data); // Grab json
            // Make it "parsable"
            string a = mc.Value;
            rootObj = JsonSerializer.Deserialize<Root>(a);
            rootObj.state.data.video.hentai_video.name = rootObj.state.data.video.hentai_video.name.RemoveSpecialCharacters();
            rootObj.linkToManifest = $"https://weeb.hanime.tv/weeb-api-cache/api/v8/m3u8s/{rootObj.state.data.video.videos_manifest.servers[0].streams[0].id.ToString()}.m3u8";
            vid.slug = rootObj.linkToManifest;
            if (videoInfo == null)
                videoInfo = rootObj.state.data.video;
            else
                videoInfo.hentai_video = rootObj.state.data.video.hentai_video;
            return vid.slug;
        }
    }
}
