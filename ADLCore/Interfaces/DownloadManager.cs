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
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;

namespace ADLCore.Interfaces
{
    // ShuJia - 29/5/22 | Credits
    public abstract class DownloadManager
    {
        protected AWebClient wClient;
        private string Path;
        private bool Stream;
        public bool UseAltExport;
        
        protected ManagerObject managedStreamObject;
        public int Location;
        
        protected Tuple<string, string, string, string> videoOption;
        protected Tuple<string, string, string, string> audioOption;

        protected ManagerObject videoObject;
        protected ManagerObject audioObject;

        public byte[] encKey;

        public int Size;

        private MemoryStream msSource;
        private StreamPipeSource source;
        
        private FileStream mpLock;
        private StreamPipeSink mpSink;
        public DownloadManager(string export, bool stream)
        {
            wClient = new AWebClient();
            Path = export;
            Stream = stream;

            mpLock = File.Open(Path + "ts", FileMode.Create);
            mpSink = new StreamPipeSink(mpLock);

            msSource = new MemoryStream();
            source = new StreamPipeSource(msSource);
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
        //TODO: Pipe downloads directly to FFMPEG.
        protected async void ExportData(Byte[] video, Byte[] audio)
        {
            if (video == null)
            {
                return;
            }
            if (audio == null)
            {
                ExportData(video);
                return;
            }
            

            if(managedStreamObject.EncryptionType > 0)
                switch (managedStreamObject.EncryptionType)
                {
                    case 0:
                    {
                        video = Encrpytion.DecryptAES128(video, encKey, Location, null);
                        audio = Encrpytion.DecryptAES128(audio, encKey, Location, null);
                        break;
                    }
                }
            using (MemoryStream bV = new MemoryStream(video))
            using (MemoryStream bA = new MemoryStream(audio))
                await FFMpegArguments.FromPipeInput(new StreamPipeSource(bV))
                    .AddPipeInput(new StreamPipeSource(bA))
                    .OutputToPipe(mpSink, 
                        options => options.ForceFormat("mpegts").WithAudioCodec("aac")
                            .WithVideoCodec("h264")).NotifyOnError(b).ProcessAsynchronously();
        }

        protected async void ExportData(Byte[] video)
        {
            byte[] dec = null;
            if (managedStreamObject.EncryptionType > 0)
                switch (managedStreamObject.EncryptionType)
                {
                    case 1:
                    {
                        dec = Encrpytion.DecryptAES128(video, encKey, Location, null);
                        break;
                    }
                }
            
            if (UseAltExport)
            {
                File.Open(Path + ".tmp", FileMode.OpenOrCreate).Close();
                using (FileStream fs = File.Open(Path + ".tmp", FileMode.Append)) 
                    fs.Write(dec);
            }
            else
            {
                IMediaAnalysis bn;
                try
                {
                    msSource = new MemoryStream(dec);
                    msSource.Seek(0, SeekOrigin.Begin);
                    await FFMpegArguments.FromPipeInput(new StreamPipeSource(msSource),
                            options => options.ForceFormat("mpegts"))
                        .OutputToPipe(mpSink,
                            options => options.ForceFormat("mpegts").WithAudioCodec("aac")
                                .WithVideoCodec("h264"))
                        .NotifyOnError(b)
                        .ProcessAsynchronously().ConfigureAwait(false);
                }
                catch
                {
                    return;
                }
            }
        }

        protected async Task Finalize()
        {
            await FFMpegArguments.FromFileInput(Path + "ts", false, x => 
                    x.WithCustomArgument("-map 0:v -vcodec copy -acodec copy -map 0:a"))
                .OutputToFile(Path, true, options => options.ForceFormat("mp4")).ProcessAsynchronously();
            File.Delete(Path + "ts");
        }

        private string s = string.Empty;
        void b(string a)
        {
            s += a + "\n";
            return;
        }
    }
}