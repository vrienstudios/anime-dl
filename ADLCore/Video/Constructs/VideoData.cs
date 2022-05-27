using ADLCore.Constructs;

namespace ADLCore.Video.Constructs
{
    public class VideoData : MetaData
    {
        public string manifestString;
        public VideoData nextVideo;
        public string series;
        public bool ismp4 = false;
        public string refer;

        public string series_id;
    }
}