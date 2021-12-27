using System;
using System.Collections.Generic;
using System.Text;

namespace ADLTrack.Objects
{
    public class StartupParameters
    {
        //The invterval (in days) for checking if an anime has new episodes.
        public int updateInterval;

        public bool isWindows;

        //Track what episode the user is on if true (this requires starting the tracker at login)
        public bool trackCurrentEpisodes;

        public bool accessibleAnywhere;

        public string adlLibraryFolder;
    }
}