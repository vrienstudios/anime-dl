using System;
using System.Collections.Generic;
using System.Text;
using ADLCore.Epub;
using ADLCore.Novels.Models;

namespace ADLCore.Ext
{
    public static class EpubE
    {
        public static Dictionary<string, string> RemoveList = new Dictionary<string, string>();

        public static string mediaTypes(MediaType mt)
        {
            switch (mt)
            {
                case MediaType.image:
                    return "image/jpeg";
                case MediaType.xhtml:
                    return "application/xhtml+xml";
                case MediaType.ncx:
                    return "application/x-dtbncx+xml";
                case MediaType.css:
                    return "text/css";
            }

            return null;
        }

        public static List<Item> ToItems(this List<Page> pages)
        {
            List<Item> items = new List<Item>();
            foreach (Page pg in pages)
                items.Add(new Item(pg.id, pg.hrefTo, MediaType.xhtml));
            return items;
        }

        public static List<Item> ToItems(this List<Image> img)
        {
            List<Item> items = new List<Item>();
            foreach (Image imag in img)
                items.Add(new Item(imag.Name, imag.location, MediaType.image));
            return items;
        }

        /// <summary>
        /// Enumerates over all characters in the given string and replaces special chars, <, >, and & with escaped chars.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string
            MakeTextXHTMLReady(string text) // Can be modified to use System.Web.HTTPUtility.Decode instead.
        {
            if (text == null)
                return string.Empty;

            char[] chars = text.ToCharArray();
            StringBuilder sb = new StringBuilder();
            for (int idx = 0; idx < text.Length; idx++)
                switch (text[idx])
                {
                    case '<':
                        sb.Append("&lt;");
                        break;
                    case '>':
                        sb.Append("&gt;");
                        break;
                    case '&':
                        sb.Append("&amp;");
                        break;
                    default:
                        sb.Append(text[idx]);
                        continue;
                }

            return sb.ToString();
        }

        public static Epub.Epub MakeEpub(this Book book, string location)
        {
            throw new NotImplementedException();
        }
    }
}