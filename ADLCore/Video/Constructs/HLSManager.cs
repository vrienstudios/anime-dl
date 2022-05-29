using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ADLCore.Ext.ExtendedClasses;

namespace ADLCore.Video.Constructs
{
    public class HLSManager : Interfaces.DownloadManager
    {
        public HLSManager(string export, bool stream) : base(export, stream)
        {
        }

        public override async void LoadStreamAsync(string uri)
        {
            var b = await wClient.DownloadStringAsync(uri);
            if (b[0] != '#')
                throw new Exception("This doesn't seem to be a valid HLS stream. Expected #EXT3M3U but got\n" + b);
            LoadStream(b.Split('\n'));
        }

        public override void LoadStream(string[] dataToParse)
        {
            managedStreamObject = new ManagerObject(dataToParse);
            // Set Default
            videoOption = managedStreamObject.ResolutionOptions.Last();
            audioOption = managedStreamObject.AudioOptions.Last();
        }

        public override void SetPlace(int parts)
        {
            throw new NotImplementedException();
        }

        public override bool ProcessStream()
        {
            throw new NotImplementedException();
        }
        
        public override bool ProcessStream(Action<int, string> updater)
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> ProcessAsync()
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> ProcessAsync(Action<int, string> updater)
        {
            throw new NotImplementedException();
        }
    }
}