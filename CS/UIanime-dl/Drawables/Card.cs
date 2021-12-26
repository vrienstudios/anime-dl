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
        public TableLayout _main;
        private TableLayout right;
        private ImageView img;
        
        public Card(MetaData mdata)
        {
            _main = new TableLayout();
            right = new TableLayout();
            img = new ImageView();

            if (mdata.cover == null)
                mdata.cover = mdata.getCover(mdata);

            byte[] bytes = null;
            bytes = SKImage.FromBitmap(SKBitmap.Decode(mdata.cover)).Encode(SKEncodedImageFormat.Png, 100).ToArray();
            
            img.Image = new Bitmap(bytes);
            img.Height = 200;
            img.Width = 150;
            Label name = new Label() { Text = mdata.name };
            Label author = new Label() { Text = mdata.author };
            TableRow r = new TableRow();
            r.Cells.Add(name);            
            r.Cells.Add(null);            
            TableRow r2 = new TableRow();
            r2.Cells.Add(author);
            r2.Cells.Add(null);

            right.Rows.Add(r);
            right.Rows.Add(r2);
            TableRow row = new TableRow();
            row.Cells.Add(img);
            row.Cells.Add(right);
            row.Cells.Add(null);

            _main.Rows.Add(row);
        }
    }
}