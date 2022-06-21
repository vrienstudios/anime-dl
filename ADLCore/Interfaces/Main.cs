using ADLCore.Alert;
using ADLCore.Ext;
using ADLCore.Novels.Models;
using ADLCore.SiteFolder;
using ADLCore.Video;
using ADLCore.Video.Constructs;
using ADLCore.Video.Extractors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ADLCore.Constructs;
using ADLCore.Novels;
using ADLCore.Novels.Downloaders;
using NovelHall = ADLCore.SiteFolder.NovelHall;

namespace ADLCore.Interfaces
{
    /// <summary>
    /// Used for "automatic" usage of this library. Pass the arguments in upon creation, and it will automatically execute it.
    /// </summary>
    [ComVisible(true)]
    public class Main
    {
        public IAppBase _base;
        public static Func<byte[], byte[]> imageConverter;
        
        public Main(string[] adls, bool sequential = true)
        {
            if (!sequential)
                throw new NotImplementedException("Multithreaded updating not yet supported.");
            for (int idx = 0; idx < adls.Length; idx++)
            {
                Tuple<SiteBase, MetaData, Book> tuple = ADLArchiveManager.GetADLInformation(adls[idx]);
                Novels.DownloaderBase _ =
                    tuple.Item1.GenerateExtractor(new ArgumentObject(tuple.Item2.givenCommand.Split(' ')).arguments, 0,
                        null);
                _.thisBook = tuple.Item3;
                _.BeginExecution();
            }
        }

        public Main(ArgumentObject args, int ti = -1, Action<int, dynamic> u = null)
        {
            Restart: ;
            if (args.arguments.mn == "nvl")
                NovelDownload(args.arguments, ti, u);
            else if (args.arguments.mn == "ani")
                AnimeDownload(args.arguments, ti, u);
            else if (args.arguments.mn == "mng")
                throw new NotImplementedException("Manga not supported yet");
            else
            {
                if (!searchMN(ref args))
                {
                    u?.Invoke(ti,
                        "Error: could not parse command (Failure to parse website to ani/nvl flag.. you can retry with ani/nvl flag)");
                    ADLUpdates.CallError(new Exception("Error: Could not parse command (mn selector)"));
                    return;
                }
                else
                    goto Restart;
            }
        }

        public static Main Execute(ArgumentObject args, int ti = -1, Action<int, dynamic> u = null)
            => new Main(args, ti, u);

        static Object obj = new Object();

        //Returns C#/JSON objects depending on args.
        //This was a very large mistake on my part.
        public static dynamic QuerySTAT(string args, [AllowNull] Action<dynamic> linearUpdater)
        {
            ArgumentObject argumentObject = new ArgumentObject(args?.Split(' '));

            if (String.IsNullOrEmpty(args))
                throw new Exception("Invalid Argument Exception");

            dynamic shorthandSearch(string str, SiteBase sb)
            {
                argumentObject.arguments.term = str;
                IAppBase downBase = sb.GenerateExtractor(argumentObject.arguments, -1, null);
                var lak = downBase.Search(false, false);
                if (lak is List<IAppBase> || lak is List<ExtractorBase> || lak is List<DownloaderBase>)
                {
                    List<MetaData> mdata = new List<MetaData>();
                    foreach(IAppBase _base in (lak as List<IAppBase>))
                        mdata.Add(_base.GetMetaData());
                    return mdata;
                }
                if (lak is IAppBase) 
                    return (lak as IAppBase).GetMetaData();
                return null;
            }
            
            if (argumentObject.arguments.s == true)
            {
                dynamic mdatL = null;
                var b = argumentObject.arguments.term.Split(',');
                if (argumentObject.arguments.SiteSelected != null)
                {
                    if (b.Length > 1)
                    {
                        mdatL = new List<MetaData>();
                        foreach(string str in b)
                            (mdatL as List<MetaData>).Add(shorthandSearch(str, argumentObject.arguments.SiteSelected));
                    }
                    else
                        mdatL = shorthandSearch(b[0], argumentObject.arguments.SiteSelected);
                }
                else
                {
                    mdatL = new List<MetaData>();
                    foreach (string str in b)
                    foreach (SiteBase sb in Sites.continuity.Where(x => x.isSupported("search") && x.type == argumentObject.arguments.mn))
                            (mdatL as List<MetaData>).Add(shorthandSearch(str, sb));
                }

                return mdatL;
            }
            
            Main m = new Main();
            m.OnCallbackReturn += MOnOnCallbackReturn;
            

            void MOnOnCallbackReturn(int ti, dynamic retObj)
            {
                linearUpdater?.Invoke(retObj);
            }

            void Fire(int i, dynamic s)
                => m.OnCallbackReturn?.Invoke(i, s);

            DownloaderBase appbase = null;
            
            if (argumentObject.arguments.term.IsValidUri())
                appbase = argumentObject.arguments.term.SiteFromString()
                    .GenerateExtractor(argumentObject.arguments, -1, Fire);
            else
                throw new NotImplementedException("Can not currently interface with already downloaded ADLS");

            //TODO FINISH IMPLEMENTATION
            return appbase?.StartQuery();

            // Continuous updates processed by the callBack with Videos and linear downloads of Novel/Manga chapters, so it can be processed by caller.
            //lock (obj)
            //    Monitor.Wait(obj);
            //return ret;
        }

        delegate void CallbackReturn(int ti, dynamic returnedObj);

        private event CallbackReturn OnCallbackReturn;

        private bool searchMN(ref ArgumentObject args)
        {
            args.arguments.mn = args.arguments.term.SiteFromString().type;
            return true;
        }

        private Main()
        {
        }

        public Main(string[] arguments, int ti = -1, Action<int, dynamic> u = null)
        {
            ArgumentObject args = new ArgumentObject(arguments);
            Restart: ;
            if (args.arguments.mn == "nvl")
                NovelDownload(args.arguments, ti, u);
            else if (args.arguments.mn == "ani")
                AnimeDownload(args.arguments, ti, u);
            else
            {
                if (!searchMN(ref args))
                {
                    u?.Invoke(ti,
                        "Error: could not parse command (Failure to parse website to ani/nvl flag.. you can retry with ani/nvl flag)");
                    ADLUpdates.CallError(new Exception("Error: Could not parse command (mn selector)"));
                    return;
                }
                else
                    goto Restart;
            }
        }

        private void NovelDownload(argumentList args, int ti, Action<int, dynamic> u)
        {
            if (args.s)
                throw new Exception("Novel Downloader does not support searching at this time.");
            if (args.cc)
                throw new Exception("Novel Downloader does not support continuos downloads at this time.");

            IAppBase appbase;
            SiteBase bas;
            if (args.term.IsValidUri())
                appbase = args.term.SiteFromString().GenerateExtractor(args, ti, u);
            else
            {
                ArchiveManager am = new ArchiveManager();
                am.InitReadOnlyStream(args.term);
                string[] b;
                using (StreamReader sr = new StreamReader(am.zapive.GetEntry("main.adl").Open()))
                    b = sr.ReadToEnd().Split("\n");
                bas = MetaData.GetMeta(b).url.SiteFromString();
                am.CloseStream();
                GC.Collect();
                appbase = bas.GenerateExtractor(args, ti, u);
            }

            //Novels.DownloaderBase dbase = args.term.SiteFromString().GenerateExtractor(args, ti, u
            appbase.BeginExecution();
        }

        private void AnimeDownload(argumentList args, int ti, Action<int, dynamic> u)
        {
            VideoBase e = new VideoBase(args, ti, u);
            _base = e;
            e.BeginExecution();
            return;
        }
    }
}