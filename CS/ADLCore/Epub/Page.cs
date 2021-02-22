using ADLCore.Ext;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Epub
{
    public class Page
    {
        public string id;
        public string Text;
        public string FileName;
        public string hrefTo;
        public Image[] images;

        /// <summary>
        /// For those who just want to auto generate a page.
        /// </summary>
        /// <param name="pageText">Self-Explanatory</param>
        /// <param name="title">Self-Explanatory</param>
        /// <param name="images">Self-Explanatory</param>
        /// <returns></returns>
        public static Page AutoGenerate(string pageText, string title, Image[] images = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<!DOCTYPE html PUBLIC \" -//W3C//DTD XHTML 1.1//EN\"\n\"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n");
            sb.AppendLine("<html xmlns=\"http://www.w3.org/1999/xhtml\">\n<head><title></title><link href=\"../Styles/stylesheet.css\" type=\"text/css\" rel=\"stylesheet\"/></head>");
            if (pageText != string.Empty)
            {
                pageText = EpubE.MakeTextXHTMLReady(pageText);
                foreach (KeyValuePair<string, string> str in EpubE.RemoveList)
                    pageText = pageText.Replace(str.Key, str.Value);
                sb.AppendLine($"<body>\n<h1 class=\"entry-title\">{title}</h1><p></p>");
                string[] st = pageText.Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.None);
                foreach (string str in st)
                    sb.AppendLine($"<p>{str}</p>");
            }
            if (images != null)
                foreach (Image img in images)
                    sb.AppendLine(img.ToString());
            sb.AppendLine("</body></html>");
            return new Page() { id = title, Text = sb.ToString(), FileName = title, images = images };
        }
    }
}
