using System;
using System.Collections.Generic;
using ADLCore;
using ADLCore.Novels.Models;
using ADLCore.SiteFolder;
using Gdk;


namespace UIanime_dl.Classes
{
    public class NovelWrapper
    {

        public List<MetaData> GrabHome(string site, Action<MetaData> returned = null)
        {
            List<MetaData> data = null;
            void tracker(dynamic obj)
            {
                if (obj is List<MetaData>)
                    data = obj;
                if (obj is MetaData)
                    returned?.Invoke(obj as MetaData);
            }
            
            ADLCore.Interfaces.Main.QuerySTAT($"nvl {site} -grabHome -vRange 0-4", tracker);
            
            return data;
    }
}