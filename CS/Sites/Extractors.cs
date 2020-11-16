using mshtml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VidStreamIORipper.Sites.HAnime;
using VidStreamIORipper.Sites.VidStreaming;
using System.Web.Script.Serialization;
using VidStreamIORipper.Sites;
using System.IO;

namespace VidStreamIORipper.Sites
{
    public static class Extractors
    {
        static mshtml.HTMLDocument buffer1;
        static mshtml.IHTMLDocument2 buffer2;
        static mshtml.HTMLDocument buffer3;
        static mshtml.IHTMLDocument2 buffer4;

        static mshtml.IHTMLElement node = null;

        public static object[] extractHAnimeLink(string episodeUri)
        {
            Console.WriteLine("Extracting Download URL for {0}", episodeUri);
            WebClient wc = new WebClient();
            string Data = wc.DownloadString(episodeUri);

            Regex reg = new Regex("(?<=<script>window\\.__NUXT__=)(.*)(?=;</script>)");
            Match mc = reg.Match(Data); // Grab json
            // Make it "parsable"
            string a = mc.Value;
            JavaScriptSerializer jss = new JavaScriptSerializer();
            Root r = jss.Deserialize<Root>(a);
            jss = null;

            Console.WriteLine($"https://weeb.hanime.tv/weeb-api-cache/api/v8/m3u8s/{r.state.data.video.videos_manifest.servers[0].streams[0].id.ToString()}.m3u8");
            Storage.videoObj = r.state.data.video;
            return new object[] { $"https://weeb.hanime.tv/weeb-api-cache/api/v8/m3u8s/{r.state.data.video.videos_manifest.servers[0].streams[0].id.ToString()}.m3u8", Storage.videoObj.hentai_video };
        }

        #region VidStream
        // Quick patch to circumvent the new serving based on cookies for vidstream.
        // May not work on anime lacking the Cloud9 option.
        public static String extractCloudDUri(string episodeUri)
        {
            Console.WriteLine("Extracting Download URL for {0}", episodeUri);
            WebClient wc = new WebClient();
            string Data = wc.DownloadString(episodeUri);
            buffer3 = new mshtml.HTMLDocument();
            wc.Dispose();
            buffer3.designMode = "off";
            buffer4 = (mshtml.IHTMLDocument2)buffer3;
            buffer4.write(Data); // beware the hang.
            Expressions.vidStreamRegex = new Regex(Expressions.videoIDRegex);
            IHTMLElementCollection col = buffer3.getElementsByTagName("IFRAME");
            foreach (IHTMLElement el in col)
            {
                Console.WriteLine(el.innerHTML);
            }
            Match match;
            string id = null;
            string url = null;
            foreach (IHTMLElement elem in col)
            {
                url = elem.getAttribute("src");
            }
            col = null;
            buffer3 = new mshtml.HTMLDocument();
            buffer4.clear();
            Storage.client = new HttpClient();
            Task<String> response = Storage.client.GetStringAsync($"https:{url}");
            buffer4 = (mshtml.IHTMLDocument2)buffer3;
            buffer4.write(response.Result);
            foreach (IHTMLElement ele in buffer3.all)
            {
                if (ele.className == "list-server-items")
                {
                    foreach (IHTMLElement service in ele.all)
                    {
                        if (service.innerText == "Cloud9 ")
                        {
                            string[] a = service.getAttribute("data-video").ToString().Split('/');
                            string La = wc.DownloadString($"https://api.cloud9.to/stream/{a.Last()}");
                            Expressions.cloud9Regex = new Regex(Expressions.idGetRegex);
                            Match m = Expressions.cloud9Regex.Match(La); // keep forgetting Search is not available in C# (this way)
                            return m.Groups[0].Value;
                        }
                        else
                            continue;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the URL to the downloadable content. || Needs to be revamped to accept cookies to work.
        /// </summary>
        /// <param name="episodeUri"></param>
        /// <returns></returns>
        public static String extractDownloadUri(string episodeUri)
        {
            Console.WriteLine("Extracting Download URL for {0}", episodeUri);
            WebClient wc = new WebClient();
            string Data = wc.DownloadString(episodeUri);
            buffer3 = new mshtml.HTMLDocument();
            wc.Dispose();
            buffer3.designMode = "off";
            buffer4 = (mshtml.IHTMLDocument2)buffer3;
            buffer4.write(Data); // beware the hang.
            Expressions.vidStreamRegex = new Regex(Expressions.videoIDRegex);
            IHTMLElementCollection col = buffer3.getElementsByTagName("IFRAME");
            Match match;
            string id = null;
            foreach (IHTMLElement elem in col)
            {
                match = Expressions.vidStreamRegex.Match(elem.getAttribute("src"));
                if (match.Success)
                {
                    id = match.Groups[0].Value;
                    break;
                }
                else
                    return null;
            }
            col = null;
            buffer3.clear();
            buffer4.clear();

            Task<String> response = Storage.client.GetStringAsync($"https://vidstreaming.io/ajax.php?id={id}&refer=none");
            Expressions.vidStreamRegex = new Regex(Expressions.downloadLinkRegex);
            match = Expressions.vidStreamRegex.Match(response.Result);
            if (match.Success)
            {
                string ursTruly = match.Groups[0].Value.Replace("\\", string.Empty);
                int ids = Extensions.indexOfEquals(ursTruly) + 1;
                if (ursTruly.Contains("goto.php")) // If the url is a redirect, get the underlying link.
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ursTruly);
                    request.AutomaticDecompression = DecompressionMethods.GZip;
                    WebResponse res = request.GetResponse();
                    string s = res.ResponseUri.ToString();
                    //delete
                    request = null;
                    res.Dispose();
                    return $"{s}:{id}";
                }
                else // Else continue.
                    return ($"{ursTruly}:{id}");

            }
            return null;
        }

        public static String Search(string name)
        {
            buffer1 = new mshtml.HTMLDocument();
            buffer2 = (mshtml.IHTMLDocument2)buffer1;
            Console.WriteLine("Downloading search page for: {0}", name);
            string Data = Storage.wc.DownloadString($"https://vidstreaming.io/search.html?keyword={name}");
            buffer2.write(Data); // Write all the data to buffer1 so that we can enumerate it.
            mshtml.IHTMLElementCollection collection;
            Console.WriteLine("Searching for video-block");
            collection = buffer1.getElementsByTagName("li"); //Get all collections with the <li> tag.
            foreach (mshtml.IHTMLElement obj in collection)
            {
                if (obj.className == "video-block " || obj.className == "video-block click-hover") //if the element has a classname of "video-block " then we are dealing with a show.
                {
                    Console.WriteLine("Found video-block!");
                    node = obj; // set node to object.
                    break; // escape the foreach loop.
                }
            }
            Expressions.vidStreamRegex = new Regex(Expressions.searchVideoRegex); // Don't say anything about parsing html with REGEX. This is a better than importing another library for this case.
            if (node == null)
                return "E";
            Match m = Expressions.vidStreamRegex.Match(node.innerHTML);
            return m.Groups.Count >= 1 ? "https://vidstreaming.io" + m.Groups[1].Value : "E";
        }

        public static HentaiVideo[] LSearch(string name)
        {
            buffer1 = new mshtml.HTMLDocument();
            buffer2 = (mshtml.IHTMLDocument2)buffer1;
            Console.WriteLine("Downloading search page for: {0}", name);
            string Data = Storage.wc.DownloadString($"https://vidstreaming.io/search.html?keyword={name}");
            buffer2.write(Data); // Write all the data to buffer1 so that we can enumerate it.
            mshtml.IHTMLElementCollection collection;
            Console.WriteLine("Searching for video-block");
            collection = buffer1.getElementsByTagName("li"); //Get all collections with the <li> tag.
            List<mshtml.IHTMLElement> nodes = new List<IHTMLElement>();
            List<HentaiVideo> titles = new List<HentaiVideo>();
            foreach (mshtml.IHTMLElement obj in collection)
            {
                if (obj.className == "video-block " || obj.className == "video-block click-hover") //if the element has a classname of "video-block " then we are dealing with a show.
                {
                    Console.WriteLine("Found video-block!");
                    nodes.Add(obj); // set node to object.
                    break; // escape the foreach loop.
                }
            }
            Expressions.vidStreamRegex = new Regex(Expressions.searchVideoRegex); // Don't say anything about parsing html with REGEX. This is a better than importing another library for this case.
            Match m;
            foreach (mshtml.IHTMLElement ele in nodes)
            {
                HentaiVideo ep = new HentaiVideo();
                ep.name = ele.innerText;

                m = Expressions.vidStreamRegex.Match(node.innerHTML);
                ep.slug = $"https://vidstreaming.io{m.Groups[1].Value}";
                titles.Add(ep);
            }


            return titles.ToArray();
            //return m.Groups.Count >= 1 ? "https://vidstreaming.io" + m.Groups[1].Value : "E";
        }

        public static String FindAllVideos(string link, Boolean dwnld, [Optional] String fileDestDirectory)
        {
            bool ck = false;
            Console.WriteLine($"Found link: {link}\nDownloading Page...");
            string Data = Storage.wc.DownloadString(link);
            buffer1 = new mshtml.HTMLDocument();
            buffer2 = (mshtml.IHTMLDocument2)buffer1;
            buffer2.write(Data); //(Again) write data to buffer1 so we can enumerate.
            mshtml.IHTMLElementCollection collection;
            Console.WriteLine("Searching for Videos");
            collection = buffer1.getElementsByTagName("li"); // split by the tag <li>
            string mainVidUri = link.Split('/').Last().TrimIntegrals(); // Trim trailing numbers.
            Expressions.vidStreamRegex = new Regex(String.Format("(?<=<A href=\"/videos/{0}).*?(?=\">)", mainVidUri));

            List<String> videoUrls = new List<string>();
            string val = null;
            Match regMax;
            int id = 0;
            Console.WriteLine(collection.length);
            List<IHTMLElement> col = new List<IHTMLElement>();

            //reverse order -- first episode to last.

            foreach (mshtml.IHTMLElement o in collection)
            {
                col.Add(o);
            }
            col.Reverse();

            foreach (mshtml.IHTMLElement obj in col) // Search for all elements containing "video-block " as a class name and matches them to our trimmed url.
            {
                if (obj.className == "video-block " || obj.className == "video-block click_hover")
                {
                    regMax = Expressions.vidStreamRegex.Match(obj.innerHTML);
                    if (regMax.Success)
                    {
                        if (ck == false)
                        {
                            Match m = Regex.Match(obj.innerText, @"(SUB|DUB)(.*?) Episode (.*)");
                            Storage.Aniname = m.Groups[2].Value;
                            fileDestDirectory = m.Groups[2].Value;
                            ck = true;
                        }
                        val = "https://vidstreaming.io/videos/" + mainVidUri + regMax.Groups[0].Value;
                        Console.WriteLine("Found a video-block! Adding to list, {0} |", val);
                        videoUrls.Add(val);
                        switch (Storage.dwnld)
                        {
                            case true://case 0:
                                {
                                    if (Storage.multTthread)
                                    {
                                        Download.QueueDownload(val, new HentaiVideo() { name = $"{regMax.Value}_{Storage.Aniname}", brand = Storage.Aniname});
                                    }
                                    else
                                        Download.StartDownload(val, Directory.GetCurrentDirectory() + "\\" + Storage.hostSiteStr + "\\" + Storage.Aniname, cSites.Vidstreaming, Encryption.None, new HentaiVideo() { name = $"{regMax.Value}_{Storage.Aniname}" });
                                    continue;
                                    //break;
                                }
                            case false:
                                {
                                    System.IO.File.AppendAllText($"{fileDestDirectory}_VDLI_temp.txt", $"\n{val}");
                                    continue;
                                }
                            default:
                                continue;
                                //break;
                        }
                    }
                }
            }
            if (Storage.multTthread)
                Download.StartMTDownload();
            return dwnld ? null : $"{fileDestDirectory}_VDLI_temp.txt";
        }
        #endregion


    }
}
