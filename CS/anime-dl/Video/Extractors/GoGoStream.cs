using anime_dl.Ext;
using anime_dl.Video.Constructs;
using MSHTML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace anime_dl.Video.Extractors
{
    class GoGoStream : ExtractorBase
    {
        WebHeaderCollection headersCollection;
        List<HentaiVideo> Series;

        public GoGoStream(string term, string path = null)
        {
            videoInfo = new Constructs.Video();
            videoInfo.hentai_video = new HentaiVideo();

            Series = new List<HentaiVideo>();
            headersCollection = new WebHeaderCollection();

            if (!term.IsValidUri())
                term = Search(term);

            FindAllVideos(term, false);

            downloadTo = $"{Directory.GetCurrentDirectory()}\\anime\\{Series[0].brand}";
            Directory.CreateDirectory(downloadTo);
            Download(downloadTo, false);
        }

        public override bool Download(string path, bool continuos)
        {
            foreach(HentaiVideo vid in Series)
            {
                GetDownloadUri(vid);
                //vid.slug = (string)o[0]; vid.ismp4 = (bool)o[1];
                DownloadVidstream(vid);
            }
            return true;
        }

        private bool DownloadVidstream(HentaiVideo video)
        {
            if (video.slug.IsMp4() == true)
            {
                GenerateHeaders();
                Console.WriteLine("Downloading: {0}", video.slug);
                webClient.DownloadFile(video.slug, $"{downloadTo}\\{video.name}.mp4");
                Console.WriteLine($"Finished Downloading: {video.slug}");
                return true;
            }
            else
            {
                String[] manifestData;
                String basePath = video.slug.TrimToSlash();

                manifestData = webClient.DownloadString(video.slug).Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.None);

                int id = 1;
                for (int idx = 0; idx < manifestData.Length; idx++)
                {
                    if (manifestData[idx][0] != '#')
                    {
                        GenerateHeaders();
                        mergeToMain($"{downloadTo}\\{video.name}.mp4", webClient.DownloadData(basePath + manifestData[idx]));
                        Console.WriteLine($"Downloaded {id++}/{(manifestData.Length / 2) - 5} for {video.name}");
                    }
                }
            }
            return true;
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
            string Data = webClient.DownloadString(video.slug);
            LoadPage(Data);
            RegexExpressions.vidStreamRegex = new Regex(RegexExpressions.videoIDRegex);
            IHTMLElementCollection col = ((HTMLDocument)docu).getElementsByTagName("IFRAME");
            Match match;
            string id = null;
            foreach (IHTMLElement elem in col)
            {
                match = RegexExpressions.vidStreamRegex.Match((string)elem.getAttribute("src"));
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

            IHTMLElementCollection collection;

            collection = ((HTMLDocument)docu).getElementsByTagName("li"); // split by the tag <li>
            string mainVidUri = link.Split('/').Last().TrimIntegrals(); // Trim trailing numbers.
            RegexExpressions.vidStreamRegex = new Regex(String.Format("(?<=<A href=\"/videos/{0}).*?(?=\">)", mainVidUri));

            string val = null;
            Match regMax;
            Console.WriteLine(collection.length);
            List<IHTMLElement> col = new List<IHTMLElement>();

            //reverse order -- first episode to last.

            foreach (IHTMLElement o in collection)
            {
                col.Add(o);
            }
            col.Reverse();

            foreach (IHTMLElement obj in col) // Search for all elements containing "video-block " as a class name and matches them to our trimmed url.
            {
                if (obj.className == "video-block " || obj.className == "video-block click_hover")
                {
                    regMax = RegexExpressions.vidStreamRegex.Match(obj.innerHTML);
                    if (regMax.Success)
                    {
                        if (ck == false)
                        {
                            Match m = Regex.Match(obj.innerText, @"(SUB|DUB)|() (.*?) Episode (.*)");
                            videoInfo.hentai_video.name = m.Groups[3].Value.RemoveSpecialCharacters();
                            ck = true;
                            //continue;
                        }
                        val = "https://vidstreaming.io/videos/" + mainVidUri + regMax.Groups[0].Value;
                        Console.WriteLine("Found a video-block! Adding to list, {0} |", val);
                        if(!Series.Exists(x => x.name == $"{regMax.Value} {videoInfo.hentai_video.name}"))
                            Series.Add(new HentaiVideo() { name = $"{regMax.Value} {videoInfo.hentai_video.name}", brand = videoInfo.hentai_video.name, slug = val });
                    }
                }
            }
            return null;
        }

        public override string Search(string name)
        {
            MSHTML.IHTMLElement node = null;
            Console.WriteLine("Downloading search page for: {0}", name);
            string Data = webClient.DownloadString($"https://vidstreaming.io/search.html?keyword={name}");
            LoadPage(Data); // Write all the data to buffer1 so that we can enumerate it.
            MSHTML.IHTMLElementCollection collection;
            Console.WriteLine("Searching for video-block");
            collection = ((MSHTML.HTMLDocument)docu).getElementsByTagName("li"); //Get all collections with the <li> tag.
            foreach (MSHTML.IHTMLElement obj in collection)
            {
                if (obj.className == "video-block " || obj.className == "video-block click-hover") //if the element has a classname of "video-block " then we are dealing with a show.
                {
                    Console.WriteLine("Found video-block!");
                    node = obj; // set node to object.
                    break; // escape the foreach loop.
                }
            }
            RegexExpressions.vidStreamRegex = new Regex(RegexExpressions.searchVideoRegex); // Don't say anything about parsing html with REGEX. This is a better than importing another library for this case.
            if (node == null)
                throw new Exception("Could not find any videos related to search");
            Match m = RegexExpressions.vidStreamRegex.Match(node.innerHTML);
            return m.Groups.Count >= 1 ? "https://vidstreaming.io" + m.Groups[1].Value : throw new Exception("Could not find any videos related to search term");
        }

        private Object[] GetVidstreamingManifestToStream(string link, bool highestres = true, string id = null)
        {
            String ida = "https://vidstreaming.io/streaming.php?id=" + id;
            headersCollection.Add("Origin", "https://vidstreaming.io");
            headersCollection.Add(ida);

            GenerateHeaders();

            if (BoolE.IsMp4(link))
                return new object[] { link, true };

            if (BoolE.IsMp4(link))
            {
                string k = "null";
                Match mc = Regex.Match(webClient.DownloadString(link), @"episode-(.*?)\.");
                if (mc.Success)
                    k = mc.Groups[1].Value;
                return new object[2] { link, true };

            }
            else
            {
                MatchCollection mc = Regex.Matches(webClient.DownloadString(link), @"(sub\..*?\..*?\.m3u8)");
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
    }
}
