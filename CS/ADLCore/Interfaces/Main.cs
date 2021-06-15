using ADLCore.Alert;
using ADLCore.Ext;
using ADLCore.Novels.Models;
using ADLCore.Video;
using ADLCore.Video.Constructs;
using ADLCore.Video.Extractors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace ADLCore.Interfaces
{
    /// <summary>
    /// Used for "automatic" usage of this library. Pass the arguments in upon creation, and it will automatically execute it.
    /// </summary>
    public class Main
    {
        public IAppBase _base;

        public Main(ArgumentObject args, int ti = -1, Action<int, string> u = null)
        {
            Restart:;
            if (args.arguments.mn == "nvl")
                NovelDownload(args.arguments, ti, u);
            else if(args.arguments.mn == "ani")
                AnimeDownload(args.arguments, ti, u);
            else
            {
                if (!searchMN(ref args))
                {
                    u?.Invoke(ti, "Error: could not parse command (Failure to parse website to ani/nvl flag.. you can retry with ani/nvl flag)");
                    ADLUpdates.CallError(new Exception("Error: Could not parse command (mn selector)"));
                    return;
                }
                else
                    goto Restart;
            }
        }

        private bool searchMN(ref ArgumentObject args)
        {
            args.arguments.mn = args.arguments.term.SiteFromString().type;
            return true;
        }

        public Main(string[] arguments, int ti = -1, Action<int, string> u = null)
        {
            ArgumentObject args = new ArgumentObject(arguments);
        Restart:;
            if (args.arguments.mn == "nvl")
                NovelDownload(args.arguments, ti, u);
            else if (args.arguments.mn == "ani")
                AnimeDownload(args.arguments, ti, u);
            else
            {
                if (!searchMN(ref args))
                {
                    u?.Invoke(ti, "Error: could not parse command (Failure to parse website to ani/nvl flag.. you can retry with ani/nvl flag)");
                    ADLUpdates.CallError(new Exception("Error: Could not parse command (mn selector)"));
                    return;
                }
                else
                    goto Restart;

            }
        }

        private void NovelDownload(argumentList args, int ti, Action<int, string> u)
        {
            if (args.s)
                throw new Exception("Novel Downloader does not support searching at this time.");
            if (args.cc)
                throw new Exception("Novel Downloader does not support continuos downloads at this time.");
            Novels.DownloaderBase dbase = args.term.SiteFromString().GenerateExtractor(args, ti, u);
            dbase.BeginExecution();
        }

        private void AnimeDownload(argumentList args, int ti, Action<int, string> u)
        {
            VideoBase e = new VideoBase(args, ti, u);
            _base = e;
            e.BeginExecution();
            return;
        }
    }
}
