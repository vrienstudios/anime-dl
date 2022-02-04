using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ADLCore.Novels.Models;

namespace UIanime_dl.Classes
{
    public class TaskController
    {
        public static object selObj;
        private static object selComp;
        private static bool cmpResult;
        
        public static void SelIndex(int i)
        {
            if(selObj is IList)
            {
                selObj = ((IList) selObj)[i];
                return;
            }
        }
        
        public static void GrabChapterList(MetaData m, int[] r, Action<Chapter> chp)
            => selObj = NovelWrapper.GrabChapterList(m, r, chp);        
        public static void GrabHome(string m, Action<MetaData> chp)
            => selObj = NovelWrapper.GrabHome(m, chp);

        public static void getCompFromSel_Field(string name)
            => selComp = selObj.GetType().GetProperties().First(x => x.Name == name);        
        public static void getCompFromComp_Field(string name)
            => selComp = selComp.GetType().GetProperties().First(x => x.Name == name).GetValue(selComp);        
        public static void str_CompareComp(string name)
            => cmpResult = (string)selComp == name;
        
    }
}