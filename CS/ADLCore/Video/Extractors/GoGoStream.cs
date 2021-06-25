using ADLCore.Ext;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ADLCore.Video.Extractors
{
    public class GoGoStream : ExtractorBase
    {
        WebHeaderCollection headersCollection;
        List<HentaiVideo> Series;

        public GoGoStream(argumentList args, int ti = -1, Action<int, string> u = null) : base(args, ti, u, Site.Vidstreaming)
        {
            ao = args;
        }

        public override void Begin()
        {
            videoInfo = new Constructs.Video();
            videoInfo.hentai_video = new HentaiVideo();

            Series = new List<HentaiVideo>();
            headersCollection = new WebHeaderCollection();

            if (!ao.term.IsValidUri())
                ao.term = Search();

            if (ao.term == null)
            {
                updateStatus(taskIndex, "Failed to get any videos related to your search!");
                return;
            }

            FindAllVideos(ao.term, false);

            if (!ao.l)
                downloadTo = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}anime{Path.DirectorySeparatorChar}{Series[0].brand}";
            else
                if (ao.android)
                    downloadTo = Path.Combine(ao.export, "Anime", Series[0].brand);
                else
                    downloadTo = Path.Combine(ao.export, Series[0].brand);

            Directory.CreateDirectory(downloadTo);
            Download(downloadTo, ao.mt, false, ao.c);
        }

        public GoGoStream(string term, bool multithread = false, string path = null, bool skip = false, int ti = -1, Action<int, string> u = null) : base(null, ti, u, Site.Vidstreaming)
        {
            videoInfo = new Constructs.Video();
            videoInfo.hentai_video = new HentaiVideo();

            Series = new List<HentaiVideo>();
            headersCollection = new WebHeaderCollection();

            if (!term.IsValidUri())
                term = Search();

            if (term == null)
            {
                updateStatus?.Invoke(taskIndex, "Failed to get any videos related to your search!");
                return;
            }

            FindAllVideos(term, false);

            if (path == null)
                downloadTo = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}anime{Path.DirectorySeparatorChar}{Series[0].brand}";
            else
                downloadTo = Path.Combine("anime", Series[0].brand);

            if(!ao.streamOnly)
                Directory.CreateDirectory(downloadTo);

            Download(downloadTo, multithread, false, skip);
        }

        private bool Download(string path, bool mt, bool continuos, bool skip)
        {
            int i = 0;
            int numOfThreads = 2;

            if (ao.vRange)
            { 
                List<HentaiVideo> buffer = new List<HentaiVideo>();
                for (int idx = ao.VideoRange[0]; idx < ao.VideoRange[1]; idx++)
                    buffer.Add(Series[idx]);
                Series = buffer;
            }

            if (!mt)
                foreach (HentaiVideo vid in Series)
                {
                    if (skip)
                        if(File.Exists($"{downloadTo}\\{vid.name}.mp4"))
                        continue;
                    GetDownloadUri(vid);
                    DownloadVidstream(vid);
                }
            else
            {
                (new Thread(() =>
                {
                    foreach (HentaiVideo vid in Series.Take(Series.Count / 2))
                    {
                        if (skip)
                            if (File.Exists($"{downloadTo}\\{vid.name}.mp4"))
                                continue;
                        GetDownloadUri(vid);
                        DownloadVidstream(vid);
                    }
                    i++;
                })).Start();
                (new Thread(() =>
                {
                    foreach (HentaiVideo vid in Series.Skip(Series.Count / 2))
                    {
                        if (skip)
                            if (File.Exists($"{downloadTo}\\{vid.name}.mp4"))
                                continue;
                        GetDownloadUri(vid);
                        DownloadVidstream(vid);
                    }
                    i++;
                })).Start();
            }
            while (i != numOfThreads)
                Thread.Sleep(200);
            return true;
        }

        public override bool Download(string path, bool mt, bool continuos)
        {
            int i = 0;
            int numOfThreads = 2;
            if (!mt)
                foreach (HentaiVideo vid in Series)
                {
                    GetDownloadUri(vid);
                    DownloadVidstream(vid);
                }
            else 
            {
                (new Thread(() =>
                {
                    foreach (HentaiVideo vid in Series.Take(Series.Count / 2))
                    {
                        GetDownloadUri(vid);
                        DownloadVidstream(vid);
                    }
                    i++;
                })).Start();
                (new Thread(() =>
                {
                    foreach (HentaiVideo vid in Series.Skip(Series.Count / 2))
                    {
                        GetDownloadUri(vid);
                        DownloadVidstream(vid);
                    }
                    i++;
                })).Start(); 
            }
            while (i != numOfThreads)
                Thread.Sleep(365);
            return true;
        }

        private bool DownloadVidstream(HentaiVideo video)
        {
            if(ao.stream)
                startStreamServer();

            WebClient webC = new WebClient();
            webC.Headers = headersCollection;
            if (video.slug.IsMp4() == true)
            {
                M3UMP4_SETTINGS m3set = new M3UMP4_SETTINGS { Host = string.Empty, Headers = headersCollection, Referer = string.Empty };
                headersCollection.Add("Accept-Encoding", "gzip, deflate, br");

                if (File.Exists($"{downloadTo}\\{video.name}.mp4"))
                    m3set.location = File.ReadAllBytes($"{downloadTo}\\{video.name}.mp4").Length;

                M3U m3 = new M3U(video.slug, null, null, true, m3set);
                int l = m3.Size;
                double prg = (double)m3.location / (double)l;
                Byte[] b;
                while ((b = m3.getNext()) != null)
                {
                    if (ao.stream)
                        publishToStream(b);
                    updateStatus?.Invoke(taskIndex, $"{video.name} {Strings.calculateProgress('#', m3.location, l)}");
                    mergeToMain($"{downloadTo}{Path.DirectorySeparatorChar}{video.name}.mp4", b);
                }
                return true;
            }
            else
            {
                MatchCollection mc = Regex.Matches(webC.DownloadString(video.slug), @"(sub\..*?\..*?\.m3u8)|(ep\..*?\..*?\.m3u8)");
                video.slug = $"{video.slug.TrimToSlash()}{GetHighestRes(mc.GetEnumerator())}";
                if (ao.c && File.Exists($"{downloadTo}\\{video.name}.mp4"))
                    return true;
                M3U m3 = new M3U(webC.DownloadString(video.slug), headersCollection, video.slug.TrimToSlash());
                int l = m3.Size;
                double prg = (double)m3.location / (double)l;
                Byte[] b;
                while((b = m3.getNext()) != null)
                {
                    if(ao.stream)
                        publishToStream(b);
                    updateStatus?.Invoke(taskIndex, $"{video.name} {Strings.calculateProgress('#', m3.location, l)}");
                    mergeToMain($"{downloadTo}{Path.DirectorySeparatorChar}{video.name}.mp4", b);
                }
            }
            return true;
        }

        private void WebC_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            updateStatus?.Invoke(taskIndex, $"{videoInfo.hentai_video.name} | {e.ProgressPercentage} {e.BytesReceived}/{e.TotalBytesToReceive}");
        }

        private bool TryCloud9(string path, bool continuos)
        {
            if (videoInfo.hentai_video.ismp4 == true)
            {
                GenerateHeaders();
                Console.WriteLine("Downloading: {0}", videoInfo.hentai_video.slug);
                webClient.DownloadFile(videoInfo.hentai_video.slug, $"{downloadTo}\\{videoInfo.hentai_video.name}.mp4");
                Console.WriteLine($"Finished Downloading: {videoInfo.hentai_video.name}");
                return true;
            }
            else
            {
                String[] manifestData;
                String basePath = string.Empty;

                manifestData = webClient.DownloadString(videoInfo.hentai_video.slug).Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.None);

                int id = 1;
                for (int idx = 0; idx < manifestData.Length; idx++)
                {
                    if (manifestData[idx][0] != '#')
                    {
                        GenerateHeaders();
                        mergeToMain($"{downloadTo}\\{videoInfo.hentai_video.name}.mp4", webClient.DownloadData(basePath + manifestData[idx]));
                        Console.WriteLine($"Downloaded {id++}/{(manifestData.Length / 2) - 5} for {videoInfo.hentai_video.name}");
                    }
                }
            }
            return true;
        }

        public override void GenerateHeaders()
        {
            webClient.Headers = headersCollection;
        }

        public override string GetDownloadUri(HentaiVideo video)
        {
            Console.WriteLine("Extracting Download URL for {0}", video.slug);
            WebClient webC = new WebClient();
            //webC.Headers = headersCollection;
            string Data = webC.DownloadString(video.slug);
            LoadPage(Data);
            RegexExpressions.vidStreamRegex = new Regex(RegexExpressions.videoIDRegex);
            HtmlNodeCollection col = docu.DocumentNode.SelectNodes("//iframe");
            Match match;
            string id = null;
            foreach (HtmlNode elem in col)
            {
                match = RegexExpressions.vidStreamRegex.Match(elem.GetAttributeValue("src", "null"));
                if (match.Success)
                {
                    id = match.Groups[0].Value;
                    break;
                }
                else
                    return null;
            }

            using(HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://vidstreaming.io");
                Task<String> response = client.GetStringAsync($"https://vidstreaming.io/ajax.php?id={id}&refer=none");
                RegexExpressions.vidStreamRegex = new Regex(RegexExpressions.downloadLinkRegex);
                match = RegexExpressions.vidStreamRegex.Match(response.Result);
            }

            if (match.Success)
            {
                string ursTruly = match.Groups[0].Value.Replace("\\", string.Empty);
                int ids = Ext.Integer.indexOfEquals(ursTruly) + 1;
                if (ursTruly.Contains("goto.php")) // If the url is a redirect, get the underlying link.
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ursTruly);
                    request.AutomaticDecompression = DecompressionMethods.GZip;
                    WebResponse res = request.GetResponse();
                    string s = res.ResponseUri.ToString();
                    //delete
                    request = null;
                    res.Dispose();
                    video.slug = s; video.brand_id = id;
                    videoInfo.hentai_video = new Constructs.HentaiVideo() { slug = s, brand_id = id };
                    return $"{s}:{id}";
                }
                else
                {
                    video.slug = ursTruly; video.brand_id = id;
                    videoInfo.hentai_video = new Constructs.HentaiVideo() { slug = ursTruly, brand_id = id };
                    return ($"{ursTruly}:{id}");
                }

            }
            return null;
        }

        public String FindAllVideos(string link, Boolean dwnld, [Optional] String fileDestDirectory)
        {
            bool ck = false;
            Console.WriteLine($"Found link: {link}\nDownloading Page...");
            string Data = webClient.DownloadString(link);
            LoadPage(Data);
            Console.WriteLine("Searching for Videos");

            HtmlNodeCollection collection;

            collection = docu.DocumentNode.SelectNodes("//li"); // split by the tag <li>
            string mainVidUri = link.Split('/').Last().TrimIntegrals(); // Trim trailing numbers.
            RegexExpressions.vidStreamRegex = new Regex(String.Format("(?<=<(A|a) href=\"/videos/{0}).*?(?=\">)", mainVidUri));

            string val = null;
            Match regMax;
            Console.WriteLine(collection.Count);
            List<HtmlNode> col = new List<HtmlNode>();

            //reverse order -- first episode to last.

            foreach (HtmlNode o in collection)
                col.Add(o);

            col.Reverse();

            foreach (HtmlNode obj in col) // Search for all elements containing "video-block " as a class name and matches them to our trimmed url.
            {
                if (obj.OuterHtml.Contains("video-block "))
                {
                    regMax = RegexExpressions.vidStreamRegex.Match(obj.InnerHtml);
                    if (regMax.Success)
                    {
                        if (ck == false)
                        {
                            Match m = Regex.Match(obj.InnerText.Sanitize(), @"(.*?) Episode (.*)");
                            videoInfo.hentai_video.name = m.Groups[1].Value.Sanitize().RemoveSpecialCharacters();
                            ck = true;
                            //continue;
                        }
                        val = "https://vidstreaming.io/videos/" + mainVidUri + regMax.Groups[0].Value;
                        updateStatus(taskIndex, $"Found a video-block! Adding to list, {val} |");
                        if(!Series.Exists(x => x.name == $"{regMax.Value} {videoInfo.hentai_video.name}"))
                            Series.Add(new HentaiVideo() { name = $"{regMax.Value} {videoInfo.hentai_video.name}", brand = videoInfo.hentai_video.name, slug = val });
                    }
                }
            }
            return null;
        }

        public override string Search(bool puser = false)
        {
            HtmlNode node = null;
            updateStatus(taskIndex, $"Searching for anime: {ao.term}");
            string Data = webClient.DownloadString($"https://vidstreaming.io/search.html?keyword={ao.term}");
            LoadPage(Data); // Write all the data to buffer1 so that we can enumerate it.
            HtmlNodeCollection collection;
            Console.WriteLine("Searching for video-block");
            collection = docu.DocumentNode.SelectNodes("//li"); //Get all collections with the <li> tag.
            foreach (HtmlNode obj in collection)
            {
                if (obj.OuterHtml.Contains("video-block ")) //if the element has a classname of "video-block " then we are dealing with a show.
                {
                    Console.WriteLine("Found video-block!");
                    node = obj; // set node to object.
                    break; // escape the foreach loop.
                }
            }
            RegexExpressions.vidStreamRegex = new Regex(RegexExpressions.searchVideoRegex); // Don't say anything about parsing html with REGEX. This is a better than importing another library for this case.
            if (node == null)
                return null;
            Match m = RegexExpressions.vidStreamRegex.Match(node.InnerHtml);
            return m.Groups.Count >= 1 ? "https://vidstreaming.io" + m.Groups[2].Value : null;
        }

        private Object[] GetVidstreamingManifestToStream(string link, bool highestres = true, string id = null)
        {
            String ida = "https://vidstreaming.io/streaming.php?id=" + id;
            headersCollection.Add("Origin", "https://vidstreaming.io");
            headersCollection.Add(ida);

            WebClient webC = new WebClient();
            webC.Headers = headersCollection;

            if (BoolE.IsMp4(link))
                return new object[] { link, true };

            if (BoolE.IsMp4(link)) // uneeded
            {
                string k = "null";
                Match mc = Regex.Match(webC.DownloadString(link), @"episode-(.*?)\.");
                if (mc.Success)
                    k = mc.Groups[1].Value;
                return new object[2] { link, true };

            }
            else
            {
                MatchCollection mc = Regex.Matches(webC.DownloadString(link), @"(sub\..*?\..*?\.m3u8)|(ep\..*?\..*?\.m3u8)");
                return new object[2] { $"{link.TrimToSlash()}{GetHighestRes(mc.GetEnumerator())}", false };
            }

        }

        private static String GetHighestRes(System.Collections.IEnumerator enumerator)
        {
            int current = 0;
            string bi = string.Empty;
            string bf = string.Empty;
            //enumerator.MoveNext(); // First step should be nil, at least it is in CLI
            while (enumerator.MoveNext())
            {
                bf = enumerator.Current.ToString();
                int ia = (int.Parse(bf.Split('.')[2]) > current) ? current = int.Parse(bf.Split('.')[2]) : -1;
                switch (ia)
                {
                    case -1: // not higher break;
                        continue;
                    default:
                        {
                            current = ia;
                            ia = 0;
                            bi = bf;
                            continue;
                        }
                }
            }
            return bf;
        }

        public override string GetDownloadUri(string path)
        {
            throw new NotImplementedException();
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
