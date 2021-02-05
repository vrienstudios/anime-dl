using ADLCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Integrations
{
    class MyAnimeList : Interfaces.Integration
    {
        public MyAnimeList() : base(Site.MyAnimeList)
        {

        }

        public override listType FindObjectsFromList()
        {
            throw new NotImplementedException();
        }

        public override void LoadUserData()
        {
            throw new NotImplementedException();
        }
    }
}
