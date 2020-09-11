using System;

namespace VidStreamIORipper.Classes
{
    public class Anime
    {
        public String title;
        public String url;
        public sites site;
        public Episode[] episodes;

        public Anime(string title, string url, sites site)
        {
            this.title = title;
            this.url = url;
            this.site = site;
        }

        public Anime()
        {

        }

        ~Anime()
        {
            title = string.Empty;
            url = string.Empty;
        }
    }
}
