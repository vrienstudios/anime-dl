using System;
using System.IO;
using ADLCore.Novels.Models;
using Eto.Drawing;
using Eto.Forms;
using SkiaSharp;

namespace UIanime_dl.Drawables
{
    public class Card
    {
        public DynamicLayout _main;
        private DynamicLayout right;
        private ImageView img;
        
        public Card(MetaData mdata)
        {
            _main = new DynamicLayout();
            right = new DynamicLayout();
            img = new ImageView();

            _main.BeginHorizontal();

            if (mdata.cover == null)
                mdata.cover = mdata.getCover(mdata);


            img.Height = 100;
            img.Width = 100;

            byte[] bytes = null;
            bytes = SKImage.FromBitmap(SKBitmap.Decode(mdata.cover)).Encode(SKEncodedImageFormat.Png, 100).ToArray();

            img.Image = new Bitmap(bytes);
            _main.Add(img);
            
            right.BeginVertical();
            Label name = new Label() { Text = mdata.name };
            Label author = new Label() { Text = mdata.author };
            right.Add(name);
            right.Add(author);
            _main.Add(right);
        }
    }
}