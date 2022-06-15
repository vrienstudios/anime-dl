using System;
using System.IO;
using ADLCore.Constructs;
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

        public delegate void onClick(Card sender);
        public event onClick onCardClick;
        public MetaData obj;
        
        public Card(MetaData mdata)
        {
            obj = mdata;
            
            _main = new TableLayout();
            right = new TableLayout();
            img = new ImageView();

            if (mdata.cover == null)
                mdata.cover = mdata.getCover(mdata);

            //byte[] bytes = null;
            //bytes = SKImage.FromBitmap(SKBitmap.Decode(mdata.cover)).Encode(SKEncodedImageFormat.Png, 100).ToArray();

            Application.Instance.Invoke(run);
            void run()
            {
                img.Image = new Bitmap(mdata.cover);
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
                _main.Height = 200;
                _main.Width = 300;
                _main.Rows.Add(row);
                _main.MouseDown += (sender, args) => { onCardClick?.Invoke(this); };
            }
        }
    }
}