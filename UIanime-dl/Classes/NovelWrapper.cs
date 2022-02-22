using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ADLCore;
using ADLCore.Novels.Models;
using ADLCore.SiteFolder;
using Gdk;

namespace UIanime_dl.Classes
{
    public class NovelWrapper
    {
        public static void SQuery(string args, Action<dynamic> ret = null)
            => ADLCore.Interfaces.Main.QuerySTAT(args, ret);

        public static MetaData GrabNovel(string uri)
        {
            MetaData mdata = null;

            void tracker(dynamic obj)
            {
                if (obj is MetaData)
                    mdata = obj as MetaData;
            }
            
            ADLCore.Interfaces.Main.QuerySTAT($"nvl {uri}", tracker);
            return mdata;
        }
        
        public static List<MetaData> GrabHome(string site, Action<MetaData> returned = null)
        {
            List<MetaData> data = null;
            void tracker(dynamic obj)
            {
                if (obj is List<MetaData>)
                    data = obj;
                if (obj is MetaData)
                    returned?.Invoke(obj as MetaData);
            }
            
            ADLCore.Interfaces.Main.QuerySTAT($"nvl {site} -grabHome -vRange 0-4 -imgDefault", tracker);
            
            return data;
        }
        
        public static List<Chapter> GrabChapterList(MetaData mdata, int[] range, Action<dynamic> linUpdater = null)
        {
            void tracker(dynamic obj)
                => linUpdater?.Invoke(obj);

            ADLCore.Interfaces.Main.QuerySTAT($"nvl {mdata.url} -linksOnly {(range != null ? $"-vRange {range[0]}-{range[1]}" : string.Empty)}", tracker);
            linUpdater?.Invoke(null); //FIN SIG
            return null;
        }
    }
}