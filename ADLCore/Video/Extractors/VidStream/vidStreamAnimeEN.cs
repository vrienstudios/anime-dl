using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using ADLCore.Ext;
using ADLCore.Video.Constructs;
using HtmlAgilityPack;
using UriDec;

namespace ADLCore.Video.Extractors.VidStream
{
    public class vidStreamAnimeEN : ExtractorBase
    {
        public vidStreamAnimeEN(argumentList a, int ti, Action<int, string> u, Site host) : base(a, ti, u, host)
        {
            downloadTo = a.export;
        }

        public override void Begin()
        {
            webClient.wCollection.Add("Referer", $"https://{new Uril(ao.term).Host}");
            webClient.wCollection.Add("Accept", "*/*");
            webClient.wCollection.Add("Origin", $"https://{new Uril(ao.term).Host}");

            videoInfo = new VideoData();

            if (!ao.term.IsValidUri())
                ao.term = Search();
            if (ao.term == null)
                throw new Exception("Failed to get any relevant searches.");

            Directory.CreateDirectory(downloadTo);
            Download(downloadTo, false, ao.cc);
        }

        public override bool Download(string path, bool mt, bool continuous)
        {
            List<VideoData> videos = new List<VideoData>();
            videos.Add(videoInfo);
            
            if (continuous)
                videos.AddRange(videoInfo.playListItems.ToArray());
            if(ao.stream)
                startStreamServer();
            
            GenerateHeaders();

            List<VideoData>.Enumerator enuma = videos.GetEnumerator();
            while (enuma.MoveNext())
            {
                bool isM4 = enuma.Current.manifestString.IsMp4();
                var encodedHeaders = UriDec.GoGoStream.GetEncHeaders();
                encodedHeaders.Add("Referer", enuma.Current.refer);
                
                M3UMP4_SETTINGS m3Set = new M3UMP4_SETTINGS()
                {
                    Host = "vidstreamingcdn.com", Headers = encodedHeaders.Clone(), 
                    Referer = enuma.Current.refer
                };

                if (isM4 && File.Exists($"{downloadTo}{Path.DirectorySeparatorChar}{enuma.Current.name}.mp4"))
                    m3Set.location =
                        File.ReadAllBytes($"{downloadTo}{Path.DirectorySeparatorChar}{enuma.Current.name}.mp4")
                            .Length;

                M3U m3 = new M3U(enuma.Current.url, downloadTo, videoInfo, 
                    null, null, isM4, m3Set);
                
                while (m3.getNext() != null)
                    base.ProgressChangeUpd(m3.location, m3.Size);
            }
            
            return true;
        }

        public override void GrabHome(int amount)
        {
            throw new NotImplementedException();
        }

        public override dynamic Search(bool a = false, bool d = false)
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

        private void GrabAllRelated(string link)
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
            videoInfo.name = videoInfo.name.RemoveStringA("Episode", false);

            videoInfo.name = videoInfo.name.RemoveExtraWhiteSpaces();

            AddNodeToSeries(col.Current);

            while (col.MoveNext())
                AddNodeToSeries(col.Current);
        }
        
        private void AddNodeToSeries(HtmlNode node)
        {
            VideoData hv = new VideoData();
            hv.name = node.ChildNodes.First(x => x.Name == "a").ChildNodes
                .Where(x => x.Attributes.Count > 0 && x.Attributes[0].Value == "name").First().InnerText
                .RemoveSpecialCharacters().RemoveExtraWhiteSpaces();
            hv.url = "https://" + baseUri + node.ChildNodes.First(x => x.Name == "a").Attributes[0].Value;
            hv.series = videoInfo.name;
            videoInfo.playListItems.Add(hv);
        }
        
        public override string GetDownloadUri(string path)
        {
            throw new NotImplementedException();
        }

        public override string GetDownloadUri(VideoData video)
        {
            Console.WriteLine("Extracting Download URL for {0}", video.name);
            WebClient webC = new WebClient();
            //webC.Headers = headersCollection;
            string Data = webC.DownloadString(video.url);
            LoadPage(Data);
            RegexExpressions.vidStreamRegex = new Regex(RegexExpressions.videoIDRegex);
            HtmlNodeCollection col = docu.DocumentNode.SelectNodes("//iframe");
            
            Match match;
            string source = col[0].GetAttributeValue("src", "null");

            string id = null;

            source = "https:" + source;
            MovePage(source);
            List<SourceObj> s = null;
            string refer = null;
            
            // The method for decrypting their security will not be made public.
            // If you want this method for a personal project (not public usage), we can talk then.
            //TODO: Generalize DecryptUri, so that it supports slightly different JSON objects for other vidstream sites.
            UriDec.GoGoStream.DecryptUri(docu, baseUri, out s, out refer);

            SourceObj sobj = s.OrderBy(x => x.res).Last();
            
            video.manifestString = sobj.uri;
            video.series_id = id;
            video.refer = refer;
            return $"{sobj.uri}:{id}";
        }

        public override void GenerateHeaders()
        {
            throw new NotImplementedException();
        }

        public override dynamic GetMetaData()
        {
            throw new NotImplementedException();
        }
    }
}