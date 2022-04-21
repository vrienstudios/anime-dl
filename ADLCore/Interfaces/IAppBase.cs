using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml.Schema;
using ADLCore.Constructs;

namespace ADLCore.Interfaces
{
    public interface IAppBase
    {
        public void GenerateHeaders();

        public static WebHeaderCollection GenerateHeaders(string uri)
        {
            WebHeaderCollection Headers = new WebHeaderCollection();
            Headers.Add(
                "accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            Headers.Add(
                "User-Agent: Mozilla/5.0 (compatible; MSIE 10.6; Windows NT 6.1; Trident/5.0; InfoPath.2; SLCC1; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET CLR 2.0.50727) 3gpp-gba UNTRUSTED/1.0");
            Headers.Add("referer", uri);
            Headers.Add("DNT", "1");
            Headers.Add("Upgrade-Insecure-Requests", "1");
            return Headers;
        }

        public void BeginExecution();
        public void MovePage(string uri);
        public void LoadPage(string html);
        public abstract MetaData GetMetaData();
        public abstract void CancelDownload(string mdataLock);
        public abstract void ResumeDownload(string mdataLock);
        public abstract void GrabHome(int amount);
        public abstract void GrabLinks(int[] range);
        public abstract dynamic Search(bool puser, bool query);
    }
}