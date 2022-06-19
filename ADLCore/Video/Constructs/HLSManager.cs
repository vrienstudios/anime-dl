using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ADLCore.Ext.ExtendedClasses;

namespace ADLCore.Video.Constructs
{
    public class HLSManager : Interfaces.DownloadManager
    {
        private IEnumerator<string> videoEnumeration;
        private IEnumerator<string> audioEnumeration;
        
        public HLSManager(string export, bool stream, byte[] encryptionKey = null) : base(export, stream)
        {
        }

        public override async Task LoadStreamAsync(string uri)
        {
            var b = await wClient.DownloadStringAsync(uri);
            await LoadStreamAsync(b.Split('\n'));
        }

        public override async Task LoadStreamAsync(string[] dataToParse)
            => await Task.Run(() => LoadStream(dataToParse));

        public override void LoadStream(string uri)
        {
            var b = wClient.DownloadString(uri);
            LoadStream(b.Split('\n'));
        }

        public override void LoadStream(string[] dataToParse)
        {
            if (dataToParse[0][0] != '#')
                throw new Exception("This doesn't seem to be a valid HLS stream. Expected #EXT3M3U but got\n" + dataToParse[0]);
            managedStreamObject = new ManagerObject(dataToParse);
            // Set Default
            videoOption = managedStreamObject.ResolutionOptions?.LastOrDefault();
            audioOption = managedStreamObject.AudioOptions?.LastOrDefault();

            if (videoOption == null)
            {
                videoObject = managedStreamObject;
                videoEnumeration = videoObject.Segments.GetEnumerator();
                audioEnumeration = Enumerable.Empty<string>().GetEnumerator();
            }
            else
            {
                var videoManifest = wClient.DownloadString(videoOption.Item4);
            
                videoObject = new ManagerObject(videoManifest.Split('\n'));

                if (audioOption != null)
                {
                    var audioManifest = wClient.DownloadString(audioOption.Item4);
                    audioObject = new ManagerObject(audioManifest.Split('\n'));
                }
            }

            
            Size = videoObject.Segments.Count();
        }

        public override void SetPlace(int parts)
        {
            throw new NotImplementedException();
        }

        public override bool ProcessStream()
        {
            //Call MoveNext in export, so we can check if the enumerable has elements.
            Location++;
            ExportData(videoEnumeration.MoveNext() ? wClient.DownloadData(videoEnumeration.Current) : null,  audioEnumeration.MoveNext() ? wClient.DownloadData(audioEnumeration.Current) : null);
            return true;
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