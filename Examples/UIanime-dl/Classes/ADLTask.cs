using System;
using System.Collections.Generic;

namespace UIanime_dl.Classes
{
    public class ADLFunc
    {
        public string name;
        public Delegate _delegate;
        public bool repeat;
        public int repeatN = 0;
    }
    public class ADLTask
    {
        public int id;
        public string status;

        public Delegate[] actions;
        public Dictionary<ADLFunc, object[]> ActionParamTuple;

        public ADLTask()
        {
        }

        public void AddFunc(Delegate method, params object[] paramaters)
        {
            ActionParamTuple.Add(new ADLFunc() { _delegate = method, name = method.Method.Name }, paramaters);
        }

        public void BeginRoutine()
        {
            foreach (ADLFunc func in ActionParamTuple.Keys)
                func._delegate.DynamicInvoke(ActionParamTuple[func]);
        }
        
        private bool loop;
        public Action<dynamic, dynamic> callBack;
    }
}