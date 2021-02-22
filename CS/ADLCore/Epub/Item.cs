using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Epub
{
    public class Item
    {
        public string id, href;
        MediaType mediaType;

        public Item(string id, string href, MediaType mediaType)
        {
            this.id = id; this.href = href; this.mediaType = mediaType;
        }

        public override string ToString()
            => $"<item id=\"{id}\" href=\"{href}\" media-type=\"{shorts.mediaTypes(mediaType)}\"/>";
    }
}
