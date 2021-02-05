using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Interfaces
{
    public enum listType
    {
        novel,
        anime,
        manga
    }

    public abstract class Integration
    {
        List<Video.Constructs.Video> Items;
        Site integratedSite;

        public Integration(Site site)
        {
            Items = new List<Video.Constructs.Video>();
            integratedSite = site;
        }

        public string userName;

        public abstract void LoadUserData();
        public abstract listType FindObjectsFromList();

        public void SearchForObjects(int i, int x)
        {

        }
    }
}
