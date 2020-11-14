using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VidStreamIORipper.Sites.VidStreaming
{
    public static class Expressions
    {
        public static Regex reg = new Regex(@"(sub\..*?\..*?\.m3u8)");
        public static Regex dwnldLink = new Regex("(?<=\"file\":\")(.+?)(?=\")");
        public static MatchCollection match;

        public static Regex vidStreamRegex;
        public static String downloadLinkRegex = "(?<=\"file\":\")(.+?)(?=\")";
        public static String searchVideoRegex = "<A href=\"(.*)\">"; // Don't say anything about parsing html with REGEX. This is a better than importing another library for this case.
        public static String videoIDRegex = @"(?<=streaming\.php\?id\=)(.+?)(?=&)";

        public static Regex cloud9Regex;
        public static String idGetRegex = "(?<=\"file\":\")(.+?)(?=\",)";
    }
}
