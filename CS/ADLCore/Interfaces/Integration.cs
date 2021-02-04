using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Interfaces
{
    public abstract class Integration
    {
        Site integratedSite;

        public string userName;

        public abstract void LoadUserData();
    }
}
