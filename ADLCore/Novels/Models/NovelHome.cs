using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ADLCore.Novels.Models
{
    public class NovelHome
    {
        public List<MetaData> booksAtHome;

        public NovelHome()
        {
            booksAtHome = new List<MetaData>();
        }

        public override string ToString()
            => JsonSerializer.Serialize(booksAtHome);
    }
}