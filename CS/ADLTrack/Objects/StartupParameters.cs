using System;
using System.Collections.Generic;
using System.Text;

namespace ADLTrack.Objects
{
    public class StartupParameters
    {
        //The invterval (in days) for checking if an anime has new episodes.
        public int updateInterval;

        //The interval (in hours) when the program starts and performs "startupRoutine".
        public int runInterval;

        public bool isWindows;

        //Track what episode the user is on if true (this requires starting the tracker at boot)
        public bool trackCurrentEpisodes;
    }
}
