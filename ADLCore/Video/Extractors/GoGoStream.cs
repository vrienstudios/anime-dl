using ADLCore.Alert;
using ADLCore.Ext;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using ADLCore.Constructs;
using UriDec;

namespace ADLCore.Video.Extractors
{
    public class GoGoStream : ExtractorBase
    {
        WebHeaderCollection headersCollection;
        List<VideoData> Series;

        public GoGoStream(argumentList args, int ti = -1, Action<int, string> u = null) : base(args, ti, u,
            Site.Vidstreaming)
        {
            ao = args;
        }

        public override void Begin()
        {
            webClient.wCollection.Add("Referer", $"https://{new Uril(ao.term).Host}");
            webClient.wCollection.Add("Accept", "*/*");
            webClient.wCollection.Add("Origin", $"https://{new Uril(ao.term).Host}");

            Series = new List<VideoData>();
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
                downloadTo =
                    $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}anime{Path.DirectorySeparatorChar}{videoInfo.name}";
            else if (ao.android)
                downloadTo = Path.Combine(ao.export, "Anime", videoInfo.name);
            else
                downloadTo = Path.Combine(ao.export, videoInfo.name);

            Directory.CreateDirectory(downloadTo);
            Download(downloadTo, ao.mt, false, ao.c);
        }

        public GoGoStream(string term, bool multithread = false, string path = null, bool skip = false, int ti = -1,
            Action<int, string> u = null) : base(null, ti, u, Site.Vidstreaming)
        {
            videoInfo = new VideoData();

            Series = new List<VideoData>();
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
                downloadTo =
                    $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}anime{Path.DirectorySeparatorChar}{Series[0].series}";
            else
                downloadTo = Path.Combine("anime", Series[0].series);

            if (!ao.streamOnly)
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
                List<VideoData> buffer = new List<VideoData>();
                for (int idx = ao.VideoRange[0]; idx < ao.VideoRange[1]; idx++)
                    buffer.Add(Series[idx]);
                Series = buffer;
            }

            if (!mt)
                foreach (VideoData vid in Series)
                {
                    if (skip)
                        if (File.Exists($"{downloadTo}\\{vid.name}.mp4"))
                            continue;
                    GetDownloadUri(vid);
                    DownloadVidstream(vid);
                }
            else
            {
                (new Thread(() =>
                {
                    foreach (VideoData vid in Series.Take(Series.Count / 2))
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
                    foreach (VideoData vid in Series.Skip(Series.Count / 2))
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

            while (i != numOfThreads && ao.mt)
                Thread.Sleep(200);
            return true;
        }

        public override bool Download(string path, bool mt, bool continuos)
        {
            int i = 0;
            int numOfThreads = 2;
            if (!mt)
                foreach (VideoData vid in Series)
                {
                    GetDownloadUri(vid);
                    DownloadVidstream(vid);
                }
            else
            {
                (new Thread(() =>
                {
                    foreach (VideoData vid in Series.Take(Series.Count / 2))
                    {
                        GetDownloadUri(vid);
                        DownloadVidstream(vid);
                    }

                    i++;
                })).Start();
                (new Thread(() =>
                {
                    foreach (VideoData vid in Series.Skip(Series.Count / 2))
                    {
                        GetDownloadUri(vid);
                        DownloadVidstream(vid);
                    }

                    i++;
                })).Start();
            }

            while (i != numOfThreads && ao.mt)
                Thread.Sleep(365);
            return true;
        }

        private bool DownloadVidstream(VideoData video)
        {
            if (ao.stream)
                startStreamServer();

            GenerateHeaders();
            if (video.manifestString.IsMp4() == true)
            {
                var whc = UriDec.GoGoStream.GetEncHeaders();
                whc.Add("Referer", video.refer);
                M3UMP4_SETTINGS m3set = new M3UMP4_SETTINGS
                    {Host = "vidstreamingcdn.com", Headers = whc.Clone(), Referer = video.refer};

                if (File.Exists($"{downloadTo}{Path.DirectorySeparatorChar}{video.name}.mp4"))
                    m3set.location = File.ReadAllBytes($"{downloadTo}{Path.DirectorySeparatorChar}{video.name}.mp4")
                        .Length;
                
                M3U m3 = new M3U(video.url, downloadTo, video, null, null, true, m3set);
                int l = m3.Size;
                double prg = (double) m3.location / (double) l;
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
                string cnt = webClient.DownloadString(video.manifestString);
                MatchCollection mc = Regex.Matches(cnt, @"(sub\..*?\..*?\.m3u8)|(ep\..*?\..*?\.m3u8)");
                if (mc.Count() > 0)
                    video.url = $"{video.url.TrimToSlash()}{GetHighestRes(mc.GetEnumerator())}";
                else
                {
                    var bb = GetHighestRes(null, cnt.Split('\n'));
                    if (!bb.IsValidUri())
                        video.url = video.url.TrimToSlash() + bb;
                    else
                        video.url = bb;
                }

                if (ao.c && File.Exists($"{downloadTo}{Path.DirectorySeparatorChar}{video.name}.mp4"))
                    return true;
                WebHeaderCollection whc = new WebHeaderCollection();
                whc.Add("Referer", $"https://{baseUri}/streaming.php");
                whc.Add("x-requested-with", "XMLHttpRequest");
                M3U m3 = new M3U(webClient.DownloadString(video.url), downloadTo, video, whc.Clone(),
                    video.url);
                int l = m3.Size;
                double prg = (double) m3.location / (double) l;
                Byte[] b;
                while ((b = m3.getNext()) != null)
                {
                    if (ao.stream)
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
            LoadPage(webClient.DownloadString(uri));
        }

        private void WebC_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            updateStatus?.Invoke(taskIndex,
                $"{videoInfo.name} | {e.ProgressPercentage} {e.BytesReceived}/{e.TotalBytesToReceive}");
        }

        private bool TryCloud9(string path, bool continuos)
        {
            if (videoInfo.ismp4 == true)
            {
                Console.WriteLine("Downloading: {0}", videoInfo.url);
                webClient.DownloadFile(videoInfo.url, $"{downloadTo}\\{videoInfo.name}.mp4");
                Console.WriteLine($"Finished Downloading: {videoInfo.name}");
                return true;
            }
            else
            {
                String[] manifestData;
                String basePath = string.Empty;

                manifestData = webClient.DownloadString(videoInfo.url)
                    .Split(new string[] {"\n", "\r\n", "\r"}, StringSplitOptions.None);

                int id = 1;
                for (int idx = 0; idx < manifestData.Length; idx++)
                {
                    if (manifestData[idx][0] != '#')
                    {
                        GenerateHeaders();
                        mergeToMain($"{downloadTo}\\{videoInfo.name}.mp4",
                            webClient.DownloadData(basePath + manifestData[idx]));
                        Console.WriteLine(
                            $"Downloaded {id++}/{(manifestData.Length / 2) - 5} for {videoInfo.name}");
                    }
                }
            }

            return true;
        }

        public override void GenerateHeaders()
        {
            webClient.Headers = headersCollection.Clone();
        }

        public override string GetDownloadUri(VideoData video)
        {
            Console.WriteLine("Extracting Download URL for {0}", video.url);
            WebClient webC = new WebClient();
            //webC.Headers = headersCollection;
            string Data = webC.DownloadString(video.url);
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
                string ex = Regex.Match(webClient.DownloadString(source).Replace("\\", string.Empty),
                    RegexExpressions.downloadLinkRegex).Value;
                video.url = ex;
                return $"{video.url}:null";
            }

            source = "https:" + source;
            MovePage(source);
            List<SourceObj> s = null;
            string refer = null;
            
            // The method for decrypting their security will not be made public.
            // If you want this method for a personal project (not public usage), we can talk then.
            //TODO: Generalize DecryptUri, so that it supports slightly different JSON objects for other vidstream sites.
            UriDec.GoGoStream.DecryptUri(docu, baseUri, out s, out refer);

            SourceObj sobj = s.OrderBy(x => x.res).Last();


            videoInfo = new VideoData() {url = sobj.uri, series_id = id, description = refer};
            video.url = sobj.uri;
            video.series_id = id;
            video.refer = refer;
            headersCollection.Add("Referer", refer);
            return $"{sobj.uri}:{id}";
        }

        private void AddNodeToSeries(HtmlNode node)
        {
            VideoData hv = new VideoData();
            hv.name = node.ChildNodes.First(x => x.Name == "a").ChildNodes
                .Where(x => x.Attributes.Count > 0 && x.Attributes[0].Value == "name").First().InnerText
                .RemoveSpecialCharacters().RemoveExtraWhiteSpaces();
            hv.url = "https://" + baseUri + node.ChildNodes.First(x => x.Name == "a").Attributes[0].Value;
            hv.series = videoInfo.name;
            Series.Add(hv);
        }

        public String FindAllVideos(string link, Boolean dwnld, [Optional] String fileDestDirectory)
        {
            videoInfo = new VideoData();
            Console.WriteLine($"Found link: {link}\nDownloading Page...");
            //TRUST CERT WA - ANDROID
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            string Data = webClient.DownloadString(link);
            LoadPage(Data);
            Console.WriteLine("Searching for Videos");

            Dictionary<string, LinkedList<HtmlNode>> animeEPList =
                pageEnumerator.GetElementsByClassNames(new string[] {"listing"});

            IEnumerator<HtmlNode> col = animeEPList["listing"].First.Value.ChildNodes.Where(x => x.Name == "li")
                .AsEnumerable().GetEnumerator();

            col.MoveNext();

            HtmlNode aTag = col.Current.ChildNodes.First(x => x.Name == "a");
            HtmlNode nameVar = aTag.ChildNodes.Where(x => x.Attributes.Count > 0 && x.Attributes[0].Value == "name").First();
           var actualName = nameVar.InnerText.RemoveSpecialCharacters();
           videoInfo.name = actualName;
            if (videoInfo.name.Contains("Episode"))
                videoInfo.name = videoInfo.name.RemoveStringA("Episode", false);
            if (videoInfo.name.Contains("Episodio"))
                videoInfo.name = videoInfo.name.RemoveStringA("Episodio", false);

            videoInfo.name = videoInfo.name.RemoveExtraWhiteSpaces();

            AddNodeToSeries(col.Current);

            while (col.MoveNext())
                AddNodeToSeries(col.Current);
            return null;
        }

        public override dynamic Search(bool puser = false, bool d = false)
        {
            HtmlNode node = null;
            updateStatus(taskIndex, $"Searching for anime: {ao.term}");
            string Data = webClient.DownloadString($"https://streamani.net/search.html?keyword={ao.term}");
            LoadPage(Data); // Write all the data to buffer1 so that we can enumerate it.
            HtmlNodeCollection collection;
            Console.WriteLine("Searching for video-block");
            collection = docu.DocumentNode.SelectNodes("//li"); //Get all collections with the <li> tag.
            foreach (HtmlNode obj in collection)
            {
                if (
                    obj.OuterHtml
                        .Contains(
                            "video-block ")) //if the element has a classname of "video-block " then we are dealing with a show.
                {
                    Console.WriteLine("Found video-block!");
                    node = obj; // set node to object.
                    break; // escape the foreach loop.
                }
            }

            RegexExpressions.vidStreamRegex =
                new Regex(RegexExpressions
                    .searchVideoRegex); // Don't say anything about parsing html with REGEX. This is a better than importing another library for this case.
            if (node == null)
                return null;
            Match m = RegexExpressions.vidStreamRegex.Match(node.InnerHtml);
            return m.Groups.Count >= 1 ? "https://streamani.net" + m.Groups[2].Value : null;
        }

        private Object[] GetVidstreamingManifestToStream(string link, bool highestres = true, string id = null)
        {
            String ida = "https://vidstreaming.io/streaming.php?id=" + id;
            headersCollection.Add("Origin", "https://vidstreaming.io");
            headersCollection.Add(ida);

            WebClient webC = new WebClient();
            webC.Headers = headersCollection.Clone();

            if (BoolE.IsMp4(link))
                return new object[] {link, true};

            if (BoolE.IsMp4(link)) // uneeded
            {
                string k = "null";
                Match mc = Regex.Match(webC.DownloadString(link), @"episode-(.*?)\.");
                if (mc.Success)
                    k = mc.Groups[1].Value;
                return new object[2] {link, true};
            }
            else
            {
                MatchCollection mc = Regex.Matches(webC.DownloadString(link),
                    @"(sub\..*?\..*?\.m3u8)|(ep\..*?\..*?\.m3u8)");
                return new object[2] {$"{link.TrimToSlash()}{GetHighestRes(mc.GetEnumerator())}", false};
            }
        }

        private static String GetHighestRes(System.Collections.IEnumerator enumerator, string[] standardized = null)
        {
            int current = 0;
            string bi = string.Empty;
            string bf = string.Empty;
            if (standardized == null)
            {
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


            HLSListObject hlsStream = new HLSListObject(standardized);
            var b = hlsStream.headerVAL.First(x => x.FirstOrDefault(z => z[0] == "RESOLUTION") != null); // First = highest

            return b.First(x => x.Contains("URI"))[1];


            return null;
        }

        public override string GetDownloadUri(string path)
        {
            throw new NotImplementedException();
        }

        public override MetaData GetMetaData()
        {
            throw new NotImplementedException();
        }

        public override void GrabHome(int amount)
        {
            throw new NotImplementedException();
        }
    }
}