using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Ext
{
    public static class ActionExt
    {
        public static void CommitMessage(this Action<int, string> pointer, int i, string message) =>
            pointer?.Invoke(i, message);
    }
}