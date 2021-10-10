using System;
using System.Net;

namespace ADLCore.Ext.ExtendedClasses
{
    public class AWebClient : WebClient
    {
        public WebHeaderCollection wCollection = new WebHeaderCollection();
        // DEFAULT UA
        public string userAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";

        private void preprocessing()
        {
            Headers = wCollection.Clone();
            Headers.Add("user-agent", userAgent);
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
