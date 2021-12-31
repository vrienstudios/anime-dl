using System.Collections.Generic;
using System.Linq;
using ADLCore.Novels.Models;
using Eto.Drawing;
using Eto.Forms;
using Eto.Threading;
using SkiaSharp;
using UIanime_dl.Classes;

namespace UIanime_dl.Drawables
{
    public class DetailPane
    {
        public DynamicLayout _main;
        private ImageView img;
        private List<Chapter> chapters;
        public DetailPane(ref MetaData mdata, Chapter[] chapters)
        {
            if (chapters == null)
                this.chapters = new List<Chapter>();
            else
                this.chapters = this.chapters.ToList();
            
            _main = new DynamicLayout();
            img = new ImageView();

            if (mdata.cover == null)
                mdata.cover = mdata.getCover(mdata);

            byte[] bytes = null;
            bytes = SKImage.FromBitmap(SKBitmap.Decode(mdata.cover)).Encode(SKEncodedImageFormat.Png, 100).ToArray();
            
            _main.BeginVertical();
            
            img.Image = new Bitmap(bytes);
            img.Size = new Size(200, 300);
            _main.Add(img);

            for (int idx = 0; idx < chapters?.Length; idx++)
                _main.Add(new Label(){Text=chapters[idx].name});
            
            _main.EndVertical();
            Application.Instance.Invoke(() => _main.Create());
        }
        
        public void DetailsPaneUpdateChapterList(MetaData addr)
        {
            void updater(Chapter c)
            {
                chapters.Add(c);
                Eto.Forms.Application.Instance.Invoke(_main.RemoveAll);
                Eto.Forms.Application.Instance.Invoke(_main.Clear);
                _main.BeginVertical();
                
                _main.Add(img);

                for (int idx = 0; idx < chapters.Count; idx++)
                    Application.Instance.Invoke(() =>
                    {
                        return _main.Add(new Label() {Text = chapters[idx].name});
                    });
            
                _main.EndVertical();

                Application.Instance.Invoke(_main.Create);
            }

            new System.Threading.Thread(() =>
            {
                NovelWrapper.GrabChapterList(addr, null, updater);
            }).Start();
        }
    }
}