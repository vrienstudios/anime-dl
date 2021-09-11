using ADLCore.Alert;
using ADLCore.Ext;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
                downloadTo = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}anime{Path.DirectorySeparatorChar}{videoInfo.hentai_video.name}";
            else
                if (ao.android)
                    downloadTo = Path.Combine(ao.export, "Anime", videoInfo.hentai_video.name);
                else
                    downloadTo = Path.Combine(ao.export, videoInfo.hentai_video.name);

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
            Series.Reverse();
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
            GenerateHeaders();
            if (video.slug.IsMp4() == true)
            {
                M3UMP4_SETTINGS m3set = new M3UMP4_SETTINGS { Host = string.Empty, Headers = headersCollection, Referer = string.Empty };
                headersCollection.Add("Accept-Encoding", "gzip, deflate, br");

                if (File.Exists($"{downloadTo}{Path.DirectorySeparatorChar}{video.name}.mp4"))
                    m3set.location = File.ReadAllBytes($"{downloadTo}{Path.DirectorySeparatorChar}{video.name}.mp4").Length;

                M3U m3 = new M3U(video.slug, null, null, true, m3set);
                int l = m3.Size;
                double prg = (double)m3.location / (double)l;
                Byte[] b;
                while ((b = m3.getNext()) != null)
                {
                    if (ao.stream)
                        publishToStream(b);
                    updateStatus?.Invoke(taskIndex, $"{video.name} {Strings.calculateProgress('#', m3.location, l)}");
                    ADLUpdates.CallLogUpdate($"{video.name} {Strings.calculateProgress('#', m3.location, l)}");
                    mergeToMain($"{downloadTo}{Path.DirectorySeparatorChar}{video.name}.mp4", b);
                }
                return true;
            }
            else
            {
                //LEGACY
                MatchCollection mc = Regex.Matches(webC.DownloadString(video.slug), @"(sub\..*?\..*?\.m3u8)|(ep\..*?\..*?\.m3u8)");
                video.slug = $"{video.slug.TrimToSlash()}{GetHighestRes(mc.GetEnumerator())}";
                if (ao.c && File.Exists($"{downloadTo}{Path.DirectorySeparatorChar}{video.name}.mp4"))
                    return true;
                GenerateHeaders();
                M3U m3 = new M3U(webC.DownloadString(video.slug), headersCollection, video.slug.TrimToSlash());
                int l = m3.Size;
                double prg = (double)m3.location / (double)l;
                Byte[] b;
                while((b = m3.getNext()) != null)
                {
                    if(ao.stream)
                        publishToStream(b);
                    updateStatus?.Invoke(taskIndex, $"{video.name} {Strings.calculateProgress('#', m3.location, l)}");
                    ADLUpdates.CallLogUpdate($"{video.name} {Strings.calculateProgress('#', m3.location, l)}");

                    mergeToMain($"{downloadTo}{Path.DirectorySeparatorChar}{video.name}.mp4", b);
                }
            }
            return true;
        }

        public override void MovePage(string uri)
        {
            GenerateHeaders();

            LoadPage(webClient.DownloadString(uri));
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
            webClient.Headers = headersCollection.Clone();
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
            video.ismp4 = true;



            Match match;
            string source = col[0].GetAttributeValue("src", "null");

            string id = null;
            if (baseUri == "animeid.to")
            {
                source = $"https://{baseUri}/ajax.php?" + source.Split("?")[1];
                string ex = Regex.Match(webClient.DownloadString(source).Replace("\\", string.Empty), RegexExpressions.downloadLinkRegex).Value;
                video.slug = ex;
                return $"{video.slug}:null";
            }
            else
                source = $"https://{baseUri}/loadserver.php?" + source.Split("?")[1];

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

            var requestHeaders = new WebHeaderCollection();
        B: ;
            MovePage(source);
            Dictionary<string, LinkedList<HtmlNode>> animeEPLink = pageEnumerator.GetElementsByClassNames(new string[] { "linkserver", "videocontent" });
            HtmlNode dwnldUriContainer;
            if (animeEPLink["linkserver"].Count != 0)
            {
                dwnldUriContainer = animeEPLink["linkserver"].ToArray()[1];
                MovePage(dwnldUriContainer.GetAttributeValue("data-video", "null"));
                animeEPLink = pageEnumerator.GetElementsByClassNames(new string[] { "videocontent" });
            }
            else
                dwnldUriContainer = animeEPLink["videocontent"].First.Value.ChildNodes.First(x => x.Name == "script");

            HttpWebRequest request;
            RegexExpressions.vidStreamRegex = new Regex("(?<={file: \')(.+?)(?=\')");
            match = RegexExpressions.vidStreamRegex.Match(dwnldUriContainer.InnerHtml);
            if (!match.Success)
            {
                // (asiaload.cc) I don't have the time to focus on decrypting their storage.google links; I can't figure out how the id param in /encrypt-ajax.php is generated for the time being, and I don't have time for reverse engineering their player. I suspect the key 
                source = dwnldUriContainer.Attributes.First(x => x.Name == "data-video").Value;
                requestHeaders.Add("referer", "https://asianload1.com/");
                requestHeaders.Add("origin", "https://asianload1.com/");
                goto B;
            }


            request = (HttpWebRequest)WebRequest.Create(match.Value);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Headers = requestHeaders; 
            if (requestHeaders.Count > 0)
            {
                request.Referer = requestHeaders["referer"];
                request.Headers.Add(requestHeaders["origin"]);
                headersCollection.Clear();
                headersCollection.Add("Referer", "https://asianload1.com/");
                headersCollection.Add("Origin", "https://asianload1.com/");
                headersCollection.Add("Accept", "*/*");
            }
            WebResponse res = request.GetResponse();
            string s = res.ResponseUri.ToString();
            //delete
            request = null;
            res.Dispose();
            video.slug = s; video.brand_id = id;
            videoInfo.hentai_video = new Constructs.HentaiVideo() { slug = s, brand_id = id };
            return $"{s}:{id}";

        }

        private void AddNodeToSeries(HtmlNode node)
        {
            HentaiVideo hv = new HentaiVideo();
            hv.name = node.ChildNodes.First(x => x.Name == "a").ChildNodes.Where(x => x.Attributes.Count > 0 && x.Attributes[0].Value == "name").First().InnerText.RemoveSpecialCharacters().RemoveExtraWhiteSpaces();
            hv.slug = "https://" + baseUri + node.ChildNodes.First(x => x.Name == "a").Attributes[0].Value;
            hv.brand = videoInfo.hentai_video.name;
            Series.Add(hv);
        }

        public String FindAllVideos(string link, Boolean dwnld, [Optional] String fileDestDirectory)
        {
            Console.WriteLine($"Found link: {link}\nDownloading Page...");
            string Data = webClient.DownloadString(link);
            LoadPage(Data);
            Console.WriteLine("Searching for Videos");

            Dictionary<string, LinkedList<HtmlNode>> animeEPList = pageEnumerator.GetElementsByClassNames(new string[] { "listing" });

            IEnumerator<HtmlNode> col = animeEPList["listing"].First.Value.ChildNodes.Where(x => x.Name == "li").AsEnumerable().GetEnumerator();

            col.MoveNext();
            videoInfo.hentai_video.name = col.Current.ChildNodes.First(x => x.Name == "a").ChildNodes.Where(x => x.Attributes.Count > 0 && x.Attributes[0].Value == "name").First().InnerText.RemoveSpecialCharacters();
            
            if (videoInfo.hentai_video.name.Contains("Episode"))
                videoInfo.hentai_video.name = videoInfo.hentai_video.name.RemoveStringA("Episode", false);
            if (videoInfo.hentai_video.name.Contains("Episodio"))
                videoInfo.hentai_video.name = videoInfo.hentai_video.name.RemoveStringA("Episodio", false);

            videoInfo.hentai_video.name = videoInfo.hentai_video.name.RemoveExtraWhiteSpaces();

            AddNodeToSeries(col.Current);

            while(col.MoveNext())
                AddNodeToSeries(col.Current);
            return null;
            /*HtmlNodeCollection collection;

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
            }*/
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
                int ia;
                try
                {
                    ia = (int.Parse(bf.Split('.')[2]) > current) ? current = int.Parse(bf.Split('.')[2]) : -1;
                }
                catch
                {
                    //work backwards
                    ia = int.Parse(bf.Substring(bf.Length - 8, 3));
                }
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
