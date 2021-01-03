using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace anime_dl.Video
{
    class RegexExpressions
    {
        public static Regex reg = new Regex(@"(sub\..*?\..*?\.m3u8)");
        public static Regex dwnldLink = new Regex("(?<=\"file\":\")(.+?)(?=\")");
        public static MatchCollection match;

        public static Regex vidStreamRegex;
        public static String downloadLinkRegex = "(?<=\"file\":\")(.+?)(?=\")";
        public static String searchVideoRegex = "<(A|a) href=\"(.*)\">"; // Don't say anything about parsing html with REGEX. This is a better than importing another library for this case.
        public static String videoIDRegex = @"(?<=streaming\.php\?id\=)(.+?)(?=&)";

        public static Regex cloud9Regex;
        public static String idGetRegex = "(?<=\"file\":\")(.+?)(?=\",)";
    }
}
