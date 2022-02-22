using System;
using ADLCore;
using Eto.Forms;
using SkiaSharp;
using UIanime_dl.Classes;

namespace UIanime_dl
{
    class Program
    {
        static void Main(string[] args)
        {
            new Application().Run(new UIanime_dl.Form());
        }

        public static byte[] SK_IMG(byte[] orig)
        =>  SKImage.FromBitmap(SKBitmap.Decode(orig)).Encode(SKEncodedImageFormat.Png, 100).ToArray();
    }
}