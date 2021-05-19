using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Novels.Models
{
    //Provides general information about books and manga.
    public class MetaData
    {
        public string name, author, rating, genre, type;
        public string url;
        public string description;
        public Byte[] cover;
    }
}
