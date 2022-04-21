using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ADLCore;
using ADLCore.Novels;
using ADLCore.Novels.Models;
using ADLCore.SiteFolder;
using Eto.Forms;
using Gdk;
using Monitor = System.Threading.Monitor;

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
        
        
        public static async Task<List<MetaData>> GrabHome(string site, Action<dynamic> returned = null)
        {
            List<MetaData> data = null;
            void tracker(dynamic obj)
            {
                Application.Instance.Invoke(() =>
                {
                    if (obj is List<MetaData>)
                        returned?.Invoke(obj as List<MetaData>);
                });
            }
            
            await ADLCore.Interfaces.Main.QuerySTAT($"nvl {site} -grabHome -vRange 0-4 -imgDefault", tracker);
            
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

        public static dynamic SearchNovel(string queryTerm, string site, Action<dynamic> linUpdater = null)
        {
            return ADLCore.Interfaces.Main.QuerySTAT($"nvl -s {queryTerm} -site {site}", linUpdater);
        }
    }
}