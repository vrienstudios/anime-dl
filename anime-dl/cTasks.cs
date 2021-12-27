using System;
using System.Collections.Generic;
using System.Text;

namespace anime_dl
{
    public class cTasks
    {
        private string[] concurrentTasks;
        private Action<string, bool, bool, bool> CallBack;

        public int Length
        {
            get { return concurrentTasks.Length; }
        }
        //string text, bool lineBreaks = false, bool refresh = false, bool bypassThreadLock = false

        public cTasks(int length, Action<string, bool, bool, bool> callBack)
        {
            concurrentTasks = new string[length];
            CallBack = callBack;
        }

        public string this[int index, bool lbreaks = false]
        {
            get { return concurrentTasks[index]; }
            set
            {
                concurrentTasks[index] = value;
                CallBack(null, lbreaks, false, false);
            }
        }

        public string this[int index, string message, bool lbreaks]
        {
            get { return concurrentTasks[index]; }
            set
            {
                concurrentTasks[index] = value;
                CallBack(message, lbreaks, false, false);
            }
        }
    }
}