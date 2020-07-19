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
    class Program
    {
        static String Data = null;
        static String FileLinkOutput = null;
        static HttpClient client;
        static WebClient wc;
        static Boolean Search = false; //FALSE//0;
        static List<String> directUrls = new List<string>();
        //mshtml::
        static mshtml.HTMLDocument buffer;
        static mshtml.IHTMLDocument2 buffer2;
        static mshtml.IHTMLElement node = null;
        static Regex reg;
        static void Main(string[] args)
        {
            for (int idx = 0; idx < args.Length; idx++)
            {
                switch (args[idx])
                {
                    case "-help":
                        break;
                    case "-S":
                        Search = true;//TRUE;
                        break;
                    case "-O":
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
            }
            Download(args[args.Length - 1]);
            Console.WriteLine("\n\n\nNext step is to copy these links into a text file and run youtube-dl!\nSample command: youtube-dl.exe -o \"%(autonumber)G{0}.%(ext)s\" -k --no-check-certificate -i -a dwnld.txt\n\n", args[args.Length - 1]);
            directUrls.Reverse();
            foreach(string str in directUrls)
            {
                Console.WriteLine(str);
            }
            Console.ReadLine();
        }
        static mshtml.HTMLDocument buffer3;
        static mshtml.IHTMLDocument2 buffer4;

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
            reg = new Regex(@"(?<=streaming\.php\?id\=)(.+?)(?=&)");
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

            Task<String> response = client.GetStringAsync(string.Format("https://vidstreaming.io/ajax.php?id={0}", id));
            reg = new Regex("(?<=\"file\":\")(.+?)(?=\")");
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
        static void Download(string name)
        {
            Console.WriteLine("Operating for: {0}", name);
            if (wc == null)
                wc = new WebClient();

            String videoUri = null;

            buffer = new mshtml.HTMLDocument();
            buffer2 = (mshtml.IHTMLDocument2)buffer;

            mshtml.IHTMLElementCollection collection;

            //System::String^ Data
            if (Search)
            {
                Console.WriteLine("Downloading search page for: {0}", name);
                Data = wc.DownloadString(String.Format("https://vidstreaming.io/search.html?keyword={0}", name));
                buffer2.write(Data); // Write all the data to buffer1 so that we can enumerate it.

                Console.WriteLine("Searching for video-block");
                collection = buffer.getElementsByTagName("li"); //Get all collections with the <li> tag.
                foreach (mshtml.IHTMLElement obj in collection)
                {
                    if (obj.className == "video-block " || obj.className == "video-block click-hover") //if the element has a classname of "video-block " then we are dealing with a show.
                    {
                        Console.WriteLine("Found video-block!");
                        node = obj; // set node to object.
                        break; // escape the foreach loop.
                    }
                }
                reg = new Regex("<A href=\"(.*)\">"); // Don't say anything about parsing html with REGEX. This is a better than importing another library for this case.
                videoUri = "https://vidstreaming.io" + reg.Match(node.innerHTML).Groups[1].Value; // Get the video url.
            }
            else
                videoUri = name;

            Console.WriteLine("Found link: {0}\nDownloading Page...", videoUri);
            Data = wc.DownloadString(videoUri);
            buffer = new mshtml.HTMLDocument();
            buffer2 = (mshtml.IHTMLDocument2)buffer;
            buffer2.write(Data); //(Again) write data to buffer1 so we can enumerate.

            Console.WriteLine("Searching for Videos");
            collection = buffer.getElementsByTagName("li"); // split by the tag <li>
            string mainVidUri = videoUri.Split('/').Last().TrimIntegrals(); // Trim trailing numbers.
            reg = new Regex(String.Format("(?<=<A href=\"/videos/{0}).*?(?=\">)", mainVidUri));

            List<String> videoUrls = new List<string>();
            string val = null;
            Match regMax;
            Console.WriteLine("Found potentially: {0}", collection.length); 

            foreach (mshtml.IHTMLElement obj in collection) // Search for all elements containing "video-block " as a class name and matches them to our trimmed url.
            {
                if (obj.className == "video-block " || obj.className == "video-block click_hover")
                {
                    regMax = reg.Match(obj.innerHTML);
                    if (regMax.Success)
                    {
                        Console.WriteLine("Reg Success!");
                        val = "https://vidstreaming.io/videos/" + mainVidUri + regMax.Groups[0].Value;
                        Console.WriteLine("Found a video-block! Adding to list, {0} |", val);
                        videoUrls.Add(val);
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
                Console.WriteLine("File saved in: {0}", FileLinkOutput);
                fs.Close();
            }
            else
            {
                for (int index = 0; index < videoUrls.Count(); index++) //Run everything through our downloadUri
                {
                    extractDownloadUri(videoUrls[index]);
                }
            }
        }
    }
}
