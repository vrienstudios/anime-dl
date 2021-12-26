using System.Collections.Generic;
using ADLCore;
using ADLCore.Novels.Models;
using ADLCore.SiteFolder;
using Gdk;

namespace UIanime_dl.Classes
{
    public class NovelWrapper
    {
        public List<MetaData> GrabHome(Site site)
        {
            List<MetaData> data = null;
            void tracker(dynamic obj)
            {
                if (obj is List<MetaData>)
                    data = obj;
            }
            ADLCore.Interfaces.Main.QuerySTAT("nvl https://novelfull.me/ -grabHome -vRange 0-4", tracker);
            
            return data;
        }
    }
}