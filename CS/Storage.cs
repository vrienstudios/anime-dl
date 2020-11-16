using System;
using System.Net;
using System.Net.Http;
using VidStreamIORipper.Sites.HAnime;

namespace VidStreamIORipper
{
    public enum Encryption
    {
        None,
        AES128
    }
    public enum cSites
    {
        Vidstreaming = 1,
        HAnime = 2,
    }
    public static class Storage
    {
        // Web variables

        // Global HTTP client for vidstreaming.io
        public static HttpClient client;

        // Global WC for single download usage.
        public static WebClient wc;

        // Download variables

        // Site we're downloading from.
        public static cSites selectedSite;

        // Search flag
        public static bool Search;
        // Download flag
        public static bool dwnld;
        // Multithreading flag.
        public static bool multTthread;
        // Skip downloaded flag.
        public static bool skip;

        //Where we're going to export files.
        public static String fileDestDirectory = null;
        // Link to the starting web page.
        public static String lnk = null;
        // Base link to the website, e.x HAnime.tv
        public static String hostSiteStr = null;
        // Name of the anime
        public static String Aniname;

        public static Video videoObj;
    }
}
