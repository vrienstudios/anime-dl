using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HtmlAgilityPack;

namespace UriDec
{
    public class SourceObj
    {
        public bool bk;
        public string uri;
        public int res;
    }

    public static class GoGoStream
    {
        public static void DecryptUri(HtmlDocument doc, string baseUri, out List<SourceObj> decrypted, out string refer)
        {
            //STUB -- Contact Chay#3670 for full code.
            //To respect the developers of vidstream/gogoplay and to protect the current method, this will not be made public.
            decrypted = null;
            refer = null;
        }

        public static WebHeaderCollection GetEncHeaders()
        {
            var collection = new WebHeaderCollection();
            collection.Add("Accept", "*/*");
            collection.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 6.0) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.");
            collection.Add("Accept-Encoding", "identity");
            collection.Add("sec-fetch-site", "same-origin");
            collection.Add("sec-fetch-mode", "no-cors");
            collection.Add("sec-fetch-dest", "video");
            return collection;
        }
    }
}