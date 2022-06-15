using System;
using System.Diagnostics;
using System.Timers;
using ADLCore;
using Eto.Forms;
using SkiaSharp;
using UIanime_dl.Classes;

namespace UIanime_dl
{
    class Program
    {
        [MTAThread]
        static void Main(string[] args)
        {
            /*Stopwatch sw = new Stopwatch(); //TODO: implement unit tests to tesst all downloaders and time it takes. //avg is 4 seconds w/o cover on slow internet; avg is 6 seconds w/ cover on slow internet.
            sw.Start();
            var q = NovelWrapper.SearchNovel("Romanian Eagle", "novelhall", null);
            sw.Stop();
            Console.ReadLine();*/
            new Application().Run(new UIanime_dl.Form());
        }

        public static byte[] SK_IMG(byte[] orig)
        =>  SKImage.FromBitmap(SKBitmap.Decode(orig)).Encode(SKEncodedImageFormat.Png, 100).ToArray();
    }
}