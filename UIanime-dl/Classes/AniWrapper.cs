using System;
using System.Collections.Generic;
using ADLCore.Constructs;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;

namespace UIanime_dl.Classes
{
    public class AniWrapper
    {
        public static List<UriDec.SourceObj> GrabLinks(string site, string uri, Action<MetaData> returned = null)
        {
            List<UriDec.SourceObj> data = null;
            void tracker(dynamic obj)
            {
                if (obj is List<UriDec.SourceObj>)
                    data = obj;
                if (obj is string)
                    returned?.Invoke(obj as MetaData);
            }
            
            ADLCore.Interfaces.Main.QuerySTAT($"ani {site} -links", tracker);
            
            return data;
        }
    }
}