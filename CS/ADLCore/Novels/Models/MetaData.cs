using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Novels.Models
{
    //Provides general information about books and manga.
    public class MetaData
    {
        public string name { get; set; } //author, rating, genre, type;
        public string author { get; set; }
        public string rating { get; set; }
        public string genre { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public string description { get; set; }
        public Byte[] cover { get; set; }
    }
}
