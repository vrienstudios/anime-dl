using mshtml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VidStreamIORipper.Sites.VidStreaming
{
    public static class VidStreamingMain
    {
        static mshtml.HTMLDocument buffer1;
        static mshtml.IHTMLDocument2 buffer2;
        static mshtml.HTMLDocument buffer3;
        static mshtml.IHTMLDocument2 buffer4;

        static mshtml.IHTMLElement node = null;

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

            Task<String> response = Storage.client.GetStringAsync($"https://vidstreaming.io/ajax.php?id={id}");
            Expressions.vidStreamRegex = new Regex(Expressions.downloadLinkRegex);
            match = Expressions.vidStreamRegex.Match(response.Result);
            if (match.Success)
                return (match.Groups[0].Value.Replace("\\", string.Empty));
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

        public static String FindAllVideos(string link, Boolean dwnld, [Optional] String fileDestDirectory)
        {
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
            foreach (mshtml.IHTMLElement obj in collection) // Search for all elements containing "video-block " as a class name and matches them to our trimmed url.
            {
                if (obj.className == "video-block " || obj.className == "video-block click_hover")
                {
                    regMax = Expressions.vidStreamRegex.Match(obj.innerHTML);
                    if (regMax.Success)
                    {
                        val = "https://vidstreaming.io/videos/" + mainVidUri + regMax.Groups[0].Value;
                        Console.WriteLine("Found a video-block! Adding to list, {0} |", val);
                        videoUrls.Add(val);
                        switch (dwnld)
                        {
                            case true://case 0:
                                {
                                    Download.FileDest = fileDestDirectory + $"\\{id + 1}_{Storage.Aniname}.mp4";
                                    if (Program.multTthread)
                                    {
                                        if (!Download.dwS && Download.downloadLinks.Length >= 2)
                                        {
                                            Download.QueueDownload(val);
                                            Download.StartDownload();
                                        }
                                        else
                                            Download.QueueDownload(val);
                                    }
                                    else
                                        Download.GetM3u8Link(val);
                                    id++;
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
            return dwnld ? null : $"{fileDestDirectory}_VDLI_temp.txt";
        }
    }

}
