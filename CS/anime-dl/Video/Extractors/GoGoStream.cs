using anime_dl.Ext;
using MSHTML;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace anime_dl.Video.Extractors
{
    class GoGoStream : ExtractorBase
    {
        public GoGoStream(string term, bool search)
        {
            videoInfo = new Constructs.Video();
        }

        public override bool Download(string path, bool continuos)
        {
            throw new NotImplementedException();
        }

        public override void GenerateHeaders()
        {
            throw new NotImplementedException();
        }

        public override string GetDownloadUri(string path)
        {
            Console.WriteLine("Extracting Download URL for {0}", path);
            string Data = webClient.DownloadString(path);
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
                    return $"{s}:{id}";
                }
                else // Else continue.
                    return ($"{ursTruly}:{id}");

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
    }
}
