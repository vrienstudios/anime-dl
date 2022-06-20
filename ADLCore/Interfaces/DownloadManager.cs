using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using ADLCore.Alert;
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

        private int tiStatus;
        private Action<int, string> updateStatus;
        
        public DownloadManager(string export, bool stream)
        {
            wClient = new AWebClient();
            Path = export;
            Stream = stream;

            mpLock = File.Open(Path + ".ts", FileMode.Create);
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

            try
            {
                File.Open(Path.TrimToSlash() + "AAC_RAW/" + Location + ".aac", FileMode.OpenOrCreate).Close();
                using(FileStream fs = File.Open(Path.TrimToSlash() + "AAC_RAW/" + Location + ".aac", FileMode.Append))
                using (MemoryStream mss = new MemoryStream(audio))
                    mss.CopyTo(fs);
                File.Open(Path.TrimToSlash() + "TS_RAW/" + Location + ".ts", FileMode.OpenOrCreate).Close();
                using(FileStream fs = File.Open(Path.TrimToSlash() + "TS_RAW/" + Location + ".ts", FileMode.Append))
                using (MemoryStream mss = new MemoryStream(video))
                    mss.CopyTo(fs);
                File.Open(Path.TrimToSlash() + "AAC_RAW/" + "MAIN.aac", FileMode.OpenOrCreate).Close();
                using(FileStream fs = File.Open(Path.TrimToSlash() + "AAC_RAW/" + "MAIN.aac", FileMode.Append))
                using (MemoryStream mss = new MemoryStream(audio))
                    mss.CopyTo(fs);
                File.Open(Path.TrimToSlash() + "TS_RAW/" + "MAIN.ts", FileMode.OpenOrCreate).Close();
                using(FileStream fs = File.Open(Path.TrimToSlash() + "TS_RAW/" + "MAIN.ts", FileMode.Append))
                using (MemoryStream mss = new MemoryStream(video))
                    mss.CopyTo(fs);
                
                using (MemoryStream bV = new MemoryStream(video))
                using (MemoryStream bA = new MemoryStream(audio))
                    await FFMpegArguments.FromPipeInput(new StreamPipeSource(bV), x => x.WithCustomArgument(""))
                        .AddPipeInput(new StreamPipeSource(bA), x => x.WithCustomArgument(""))
                        .OutputToPipe(mpSink,
                            options => options.ForceFormat("mpegts")
                                .WithCustomArgument("-map 0:v -map 1:a -c copy -shortest")).NotifyOnError(b)
                        .ProcessAsynchronously();
            }
            catch
            {
                return;
            }
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
            mpLock.Write(dec);
        }

        protected async Task Finalizer()
        {
            //transmux to mp4.
            if (File.Exists(Path + ".aac"))
            {
                await FFMpegArguments.FromFileInput(Path + ".ts", false).AddFileInput(Path + ".aac")
                    .OutputToFile(Path, true,
                        options => options.ForceFormat("mp4")
                            .WithCustomArgument("-map 0:v -vcodec copy -acodec copy -map 0:a")).ProcessAsynchronously();
                File.Delete(Path + ".ts");
            }
            else
            {
                await FFMpegArguments.FromFileInput(Path + ".ts", false)
                    .OutputToFile(Path, true,
                        options => options.ForceFormat("mp4")
                            .WithCustomArgument("-map 0:v -vcodec copy -acodec copy -map 0:a")).ProcessAsynchronously();
                File.Delete(Path + ".ts");
            }

            msSource.Close();
            await msSource.DisposeAsync();
            source = null;
            
            mpLock.Close();
            await mpLock.DisposeAsync();
            mpSink = null;
        }

        private string s = string.Empty;
        void b(string a)
        {
            s += a + "\n";
            return;
        }

        public void SetNotifier(int i, Action<int, string> update)
        {
            tiStatus = i;
            updateStatus = update;
        }

        protected void StatusUpdate(string message, ADLUpdates.LogLevel level = ADLUpdates.LogLevel.Low)
        {
            updateStatus?.Invoke(tiStatus, message);
            ADLUpdates.CallLogUpdate(message, level);
        }
    }
}