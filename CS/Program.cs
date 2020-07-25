using mshtml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VidStreamIORipper
{
    static class Program
    {
        static String Data = null;
        static String FileLinkOutput = null;
        public static HttpClient client;
        public static WebClient wc;
        static Boolean Search = false; //FALSE//0;
        static Boolean Download = false;
        static List<String> directUrls = new List<string>();
        //mshtml::
        static mshtml.HTMLDocument buffer1;
        static mshtml.IHTMLDocument2 buffer2;
        static mshtml.HTMLDocument buffer3;
        static mshtml.IHTMLDocument2 buffer4;

        static mshtml.IHTMLElement node = null;

        static Regex reg;
        static String downloadLinkRegex = "(?<=\"file\":\")(.+?)(?=\")";
        static String searchVideoRegex = "<A href=\"(.*)\">"; // Don't say anything about parsing html with REGEX. This is a better than importing another library for this case.
        static String videoIDRegex = @"(?<=streaming\.php\?id\=)(.+?)(?=&)";
        static String outPutArgs = string.Empty;
        static String fileDestDirectory = string.Empty;
        static void Main(string[] args)
        {
            
            //https://hls12xx.cdnfile.info/videos/hls/mZr_AWCBU2bXDMGgJYQWTQ/1595633921/31723/1d0ec9406221a4716c721caace98412f/sub.8.m3u
            wc = new WebClient();
            Console.ReadLine();
            for (int idx = 0; idx < args.Length; idx++)
            {
                switch (args[idx])
                {
                    case "-help":
                        {
                            break;
                        }
                    case "-S":
                        { 
                        Search = true;//TRUE;
                        }
                        break;
                    case "-O":
                        {
                            FileLinkOutput = args[idx + 1];
                            FileStream fs = new FileStream(FileLinkOutput, FileMode.CreateNew);
                            if (!fs.CanWrite)
                            {
                                Console.WriteLine("Can not create specified file.");
                                return;
                            }
                            fs.Close();
                            break;
                        }
                    case "-pD": // progressive download.
                        {
                            Download = true;
                            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\vidstream"); // || GET_LAST_ERROR == "ALREADY_EXISTS"
                            break;
                        }
                }
            }
            if (Download && Search)
            {
                fileDestDirectory = (Directory.GetCurrentDirectory() + $"\\vidstream\\{args[args.Length - 1]}");
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"\\vidstream\\{args[args.Length - 1]}");
            }
            DownloadVi(args[args.Length - 1]);
            Console.WriteLine("\n\n\nNext step is to copy these links into a text file and run youtube-dl!\nSample command: youtube-dl.exe -o \"%(autonumber)G{0}.%(ext)s\" -k --no-check-certificate -i -a dwnld.txt\n\n", args[args.Length - 1]);
            directUrls.Reverse();
            foreach(string str in directUrls)
                Console.WriteLine(str);
            Console.ReadLine();
        }

        /// <summary>
        /// Get the id from a video and send a request to get the URL from vidstream's ajax api.
        /// </summary>
        /// <param name="episodeUri"></param>
        /// <returns></returns>
        static bool extractDownloadUri(string episodeUri)
        {
            Console.WriteLine("Extracting Download URL for {0}", episodeUri);
            Data = wc.DownloadString(episodeUri);
            buffer3 = new mshtml.HTMLDocument();
            buffer3.designMode = "off";
            buffer4 = (mshtml.IHTMLDocument2)buffer3;
            buffer4.write(Data); // beware the hang.
            reg = new Regex(videoIDRegex);
            IHTMLElementCollection col = buffer3.getElementsByTagName("IFRAME");
            Match match;
            string id = null;
            foreach (IHTMLElement elem in col)
            {
                match = reg.Match(elem.getAttribute("src"));
                if (match.Success)
                {
                    id = match.Groups[0].Value;
                    break;
                }
                else
                    return false;
            }
            col = null;
            buffer3.clear();
            buffer4.clear();
            if (client == null)
                client = new HttpClient();

            Task<String> response = client.GetStringAsync($"https://vidstreaming.io/ajax.php?id={id}");
            reg = new Regex(downloadLinkRegex);
            match = reg.Match(response.Result);
            if (match.Success)
            {
                directUrls.Add(match.Groups[0].Value.Replace("\\", string.Empty));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Download page data and then grab download links to all videos related.
        /// </summary>
        /// <param name="name">Url to the video/search query</param>
        static void DownloadVi(string name)
        {
            Console.WriteLine("Operating for: {0}", name);
            if (wc == null)
                wc = new WebClient();

            String videoUri = null;

            buffer1 = new mshtml.HTMLDocument();
            buffer2 = (mshtml.IHTMLDocument2)buffer1;

            mshtml.IHTMLElementCollection collection;

            //System::String^ Data
            if (Search)
            {
                Console.WriteLine("Downloading search page for: {0}", name);
                Data = wc.DownloadString($"https://vidstreaming.io/search.html?keyword={name}");
                buffer2.write(Data); // Write all the data to buffer1 so that we can enumerate it.

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
                reg = new Regex(searchVideoRegex); // Don't say anything about parsing html with REGEX. This is a better than importing another library for this case.
                videoUri = "https://vidstreaming.io" + reg.Match(node.innerHTML).Groups[1].Value; // Get the video url.
            }
            else
                videoUri = name;

            Console.WriteLine($"Found link: {videoUri}\nDownloading Page...");
            Data = wc.DownloadString(videoUri);
            buffer1 = new mshtml.HTMLDocument();
            buffer2 = (mshtml.IHTMLDocument2)buffer1;
            buffer2.write(Data); //(Again) write data to buffer1 so we can enumerate.

            Console.WriteLine("Searching for Videos");
            collection = buffer1.getElementsByTagName("li"); // split by the tag <li>
            string mainVidUri = videoUri.Split('/').Last().TrimIntegrals(); // Trim trailing numbers.
            reg = new Regex(String.Format("(?<=<A href=\"/videos/{0}).*?(?=\">)", mainVidUri));

            List<String> videoUrls = new List<string>();
            string val = null;
            Match regMax;
            Console.WriteLine("Found potentially: {0}", collection.length);

            int id = 0;
            foreach (mshtml.IHTMLElement obj in collection) // Search for all elements containing "video-block " as a class name and matches them to our trimmed url.
            {
                if (obj.className == "video-block " || obj.className == "video-block click_hover")
                {
                    regMax = reg.Match(obj.innerHTML);
                    if (regMax.Success)
                    {
                        val = "https://vidstreaming.io/videos/" + mainVidUri + regMax.Groups[0].Value;
                        Console.WriteLine("Found a video-block! Adding to list, {0} |", val);
                        videoUrls.Add(val);
                        if (Download)
                        {
                            Dwnl.FileDest = fileDestDirectory + $"\\{id}_{name}";
                            Dwnl.GetM3u8Link(val);
                        }
                    }
                }
            }

            if(FileLinkOutput != null)
            {
                UnicodeEncoding unienc = new UnicodeEncoding();
                FileStream fs = new FileStream(FileLinkOutput, FileMode.CreateNew);
                for (int index = 0; index < videoUrls.Count(); index++) //Run everything through our downloadUri and post to file.
                {
                    extractDownloadUri(videoUrls[index]);
                    fs.Write(unienc.GetBytes(directUrls[index].ToCharArray()), 0, unienc.GetByteCount(directUrls[index]));
                }
                Console.WriteLine($"File saved in: {FileLinkOutput}");
                fs.Close();
            }
            else
                for (int index = 0; index < videoUrls.Count(); index++) //Run everything through our downloadUri
                    extractDownloadUri(videoUrls[index]);
        }
        /*~Program()
        {
            client.Dispose();
            wc.Dispose();
            buffer1.clear();
            buffer2.clear();
            buffer3.clear();
            buffer4.clear();
        }*/
    }
}
