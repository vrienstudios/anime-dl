using ADLCore.Alert;
using ADLCore.Ext;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ADLCore.Video.Extractors
{
    public class HAnime : ExtractorBase
    {

        /// <summary>
        /// HAnime Download Class
        /// </summary>
        /// <param name="term">Search term or download link</param>
        /// <param name="mt">Multithreading parameter (ineffective on HAnime)</param>
        /// <param name="path">Download Path, not functional</param>
        /// <param name="continuos">Download multiple videos in a row</param>
        /// <param name="ti">"taskindex" to be used with status update</param>
        /// <param name="statusUpdate">The function will call this when ever a notable update occurs</param>
        public HAnime(argumentList args,  int ti = -1, Action<int, string> statusUpdate = null) : base(args, ti, statusUpdate, Site.HAnime)
        {
            ao = args;
            if (ao.vRange)
                throw new ArgumentException("HAnime does not support vRange.");
        }

        public override void Begin()
        {
            downloadTo = ao.export;

            if (ao.term.IsValidUri())
                Download(ao.term, ao.mt, ao.cc);
            else
            {
                string a = Search();
                if (a == null)
                    return;
                Download(a, ao.mt, ao.cc);
            }
        }

        private bool ExtractDataFromVideo()
        {
            return false;
        }

        public override bool Download(string path, bool mt, bool continuos)
        {
            GetDownloadUri(videoInfo == null ? new HentaiVideo { slug = path } : videoInfo.hentai_video);

            if (!ao.l)
                downloadTo = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}HAnime{Path.DirectorySeparatorChar}{videoInfo.hentai_video.name.TrimIntegrals()}{Path.DirectorySeparatorChar}";
            else
                if (ao.android)
                downloadTo = Path.Combine(ao.export, "ADL", "HAnime");
            else
                downloadTo = Path.Combine(ao.export, videoInfo.hentai_video.brand);

            Directory.CreateDirectory(downloadTo);

            M3U m3 = new M3U(webClient.DownloadString(rootObj.linkToManifest));

            Byte[] b;
            int l = m3.Size;
            double prg;
            updateStatus(taskIndex, $"Beginning download of {videoInfo.hentai_video.name}");

            if (ao.stream)
            {
                startStreamServer();
                while ((b = m3.getNext()) != null)
                {
                    if (allStop)
                    {
                        invoker();
                        return false;
                    }
                    updateStatus(taskIndex, $"{videoInfo.hentai_video.name} {Strings.calculateProgress('#', m3.location, l)}");
                    publishToStream(b);
                    mergeToMain(downloadTo + videoInfo.hentai_video.name + ".mp4", b);
                }
            }
            else
            {
                while ((b = m3.getNext()) != null)
                {
                    if (allStop)
                    {
                        invoker();
                        return false;
                    }
                    prg = (double)m3.location / (double)l;

                    updateStatus(taskIndex, $"{videoInfo.hentai_video.name} {Strings.calculateProgress('#', m3.location, l)}");
                    mergeToMain(downloadTo + videoInfo.hentai_video.name + ".mp4", b);
                }
            }

            if (continuos && videoInfo.next_hentai_video.name.RemoveSpecialCharacters().TrimIntegrals() == videoInfo.hentai_video.name.TrimIntegrals())
            {
                HAnime h = new HAnime(new argumentList { term = $"https://hanime.tv/videos/hentai/{videoInfo.next_hentai_video.slug}", mt = mt, export = downloadTo, cc = continuos }, taskIndex, updateStatus);
                h.Begin();
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

        private string SearchPrompt(SearchReq sj, ref int np)
        {
            for (int idx = 0; idx < sj.actualHits.Count; idx++)
                ADLUpdates.CallUpdate($"{idx} -- {sj.actualHits[idx].name} | Ratings: {sj.actualHits[idx].GetRating()}/10\n       tags:{sj.actualHits[idx].tagsAsString()}\n       desc:{new string(sj.actualHits[idx].description.Replace("<p>", string.Empty).Replace("</p>", string.Empty).Replace("\n", string.Empty).Take(60).ToArray())}\n\n", true, false, true);

            ADLUpdates.CallUpdate($"\nCommands: \n     page {{page}}/{sj.nbPages}\n     select {{episode num}}", true, false, true);
        c:
            String[] input = Console.ReadLine().ToLower().Split(' ');

            switch (input[0])
            {
                case "select":
                    videoInfo = new Constructs.Video() { hentai_video = new HentaiVideo() { slug = $"https://hanime.tv/videos/hentai/{sj.actualHits[int.Parse(input[1])].slug}" } };
                    ADLUpdates.CallThreadChange(false);
                    return $"https://hanime.tv/videos/hentai/{sj.actualHits[int.Parse(input[1])].slug}";
                case "page":
                    Console.Clear();
                    np = int.Parse(input[1]);
                    return "CNT";
                default:
                    goto c;
            }
        }

        public override string Search(bool promptUser = true)
        {
            int np = 0;
            string a;
        a:
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://search.htv-services.com/");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                string json = $"{{\"search_text\":\"{ao.term}\",\"tags\":[],\"tags_mode\":\"AND\",\"brands\":[],\"blacklist\":[],\"order_by\":\"released_at_unix\",\"ordering\":\"asc\",\"page\":{np.ToString()}}}";

                using (StreamWriter sw = new StreamWriter(httpWebRequest.GetRequestStream()))
                    sw.Write(json);

                HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    a = sr.ReadToEnd();

                SearchReq sj = JsonSerializer.Deserialize<SearchReq>(a);

                if(sj.actualHits.Count <= 0)
                {
                    ADLUpdates.CallUpdate($"No videos matching search query.");
                    return null;
                }

                ADLUpdates.CallUpdate($"Hits: {sj.actualHits.Count} {np}/{sj.nbPages} page", false, false, true);

                if (promptUser)
                {
                    ADLUpdates.CallThreadChange(true);
                    while (true)
                    {
                        string searchResponse = SearchPrompt(sj, ref np);
                        if (searchResponse == "CNT")
                            goto a;
                        return searchResponse;
                    }
                }
                else
                    return $"https://hanime.tv/videos/hentai/{sj.actualHits[0].slug}"; // Else return first video returned.
            }
            catch
            {
                goto a;
            }
        }

        public override string GetDownloadUri(HentaiVideo vid)
        {
            ADLUpdates.CallUpdate($"Extracting Download URL for {vid.slug}");
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

        public override dynamic Get(HentaiVideo obj, bool dwnld)
        {
            throw new NotImplementedException();
        }

        public override MetaData GetMetaData()
        {
            throw new NotImplementedException();
        }
    }
}
