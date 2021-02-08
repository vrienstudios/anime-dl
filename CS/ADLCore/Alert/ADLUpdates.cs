using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Alert
{
    public class ADLUpdates
    {
        public delegate void SystemUpdate(string text, bool lineBreaks, bool refresh, bool bypassThreadLock);
        public delegate void SystemError(Exception ex);
        public delegate void ThreadCaller(bool tf);

        public static event SystemUpdate onSystemUpdate;
        public static event SystemError onSystemError;
        public static event ThreadCaller onThreadDeclaration;

        public static void CallUpdate(string text, bool lineBreaks = false, bool refresh = false, bool bypassThreadLock = false)
            => onSystemUpdate?.Invoke(text, lineBreaks, refresh, bypassThreadLock);

        public static void CallThreadChange(bool tf)
            => onThreadDeclaration?.Invoke(tf);

        public static void CallError(Exception e)
            => onSystemError?.Invoke(e);
    }
}
