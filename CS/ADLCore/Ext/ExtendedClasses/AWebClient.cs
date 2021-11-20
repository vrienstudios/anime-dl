using System;
using System.Net;

namespace ADLCore.Ext.ExtendedClasses
{
    public class AWebClient : WebClient
    {
        public WebHeaderCollection wCollection = new WebHeaderCollection();
        // DEFAULT UA
        public string userAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:91.0) Gecko/20100101 Firefox/91.0";

        private void preprocessing()
        {
            Headers = wCollection.Clone();
            Headers.Add("User-Agent", userAgent);
        }

        public new byte[] DownloadData(string address)
        {
            preprocessing();
            return base.DownloadData(address);
        }

        public new string DownloadString(string address)
            => DownloadString(new Uri(address));

        public new string DownloadString(Uri address)
        {
            preprocessing();
            return base.DownloadString(address);
        }
    }
}
