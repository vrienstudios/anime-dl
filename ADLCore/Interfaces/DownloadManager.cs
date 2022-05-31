using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using ADLCore.Ext;
using ADLCore.Ext.ExtendedClasses;
using ADLCore.Video.Constructs;
using FFMpegCore;
using FFMpegCore.Pipes;

namespace ADLCore.Interfaces
{
    // ShuJia - 29/5/22 | Credits
    public abstract class DownloadManager
    {
        protected AWebClient wClient;
        private string Path;
        private bool Stream;

        protected ManagerObject managedStreamObject;
        protected int Location;
        
        protected Tuple<string, string, string, string> videoOption;
        protected Tuple<string, string, string, string> audioOption;

        protected ManagerObject videoObject;
        protected ManagerObject audioObject;
        
        public DownloadManager(string export, bool stream)
        {
            Path = export;
            Stream = stream;
        }

        public abstract Task LoadStreamAsync(string uri);
        public abstract Task LoadStreamAsync(string[] datatoParse);
        public abstract void LoadStream(string datatoParse);
        public abstract void LoadStream(string[] dataToParse);
        public abstract void SetPlace(int byteOrPart);
        
        //Async is highly recommended, except in multi-threaded applications (where in, provided its own thread)
        public abstract bool ProcessStream();
        public abstract bool ProcessStream(Action<int, string> updater);

        public abstract Task<bool> ProcessAsync();
        public abstract Task<bool> ProcessAsync(Action<int, string> updater);

        public List<Tuple<string, string, string, string>> GetResolutions() => managedStreamObject.ResolutionOptions;
        public List<Tuple<string, string, string, string>> GetAudios() => managedStreamObject.AudioOptions;
        
        //If not called, we automatically select highest resolution. Get ID from GetResolutions()
        public void SelectResolution(Tuple<string, string, string, string> selected) => videoOption = selected;
        //If not called, we automatically select highest resolution. Get ID from GetAudios()
        public void SelectAudio(Tuple<string, string, string, string> selected) => audioOption = selected;


        public void LoadHeaders(WebHeaderCollection collection) => wClient.wCollection = collection;

        //TODO: Test On Android
        protected async void ExportData(Byte[] video, Byte[] audio)
        {
            using (MemoryStream bV = new MemoryStream(video))
            using (MemoryStream bA = new MemoryStream(audio))
                await FFMpegArguments.FromPipeInput(new StreamPipeSource(bV))
                    .AddPipeInput(new StreamPipeSource(bA)).OutputToFile(Path).ProcessAsynchronously();
        }        
        protected async void ExportData(Byte[] video)
        {
            using (MemoryStream bV = new MemoryStream(video))
            using (MemoryStream bA = new MemoryStream(video))
                await FFMpegArguments.FromPipeInput(new StreamPipeSource(bV))
                    .AddPipeInput(new StreamPipeSource(bA)).OutputToFile(Path).ProcessAsynchronously();
        }
    }
}