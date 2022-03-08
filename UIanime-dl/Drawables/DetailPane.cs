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
        public List<Chapter> chapters;
        private MetaData mdataObj;
        
        public delegate void chapterSelect(ref Chapter chp);

        public event chapterSelect OnChapterSelect;
        
        public DetailPane(ref MetaData mdata, Chapter[] chapters)
        {
            if (chapters == null)
                this.chapters = new List<Chapter>();
            else
                this.chapters = this.chapters.ToList();
            
            _main = new DynamicLayout();
            img = new ImageView();

            _main.BeginScrollable();
            _main.BeginVertical();

            img.Image = new Bitmap(mdata.cover);
            img.Size = new Size(200, 300);
            _main.Add(img);

            for (int idx = 0; idx < chapters?.Length; idx++)
            {
                Label labelia = new Label() {Text = chapters[idx].name};
                labelia.MouseUp += delegate(object? sender, MouseEventArgs args)
                {
                    OnChapterSelect?.Invoke(ref chapters[idx]);
                };
                _main.Add(labelia);
            }

            Application.Instance.Invoke(() => _main.Create());
        }

        public void DetailsPaneUpdateChapterList(MetaData addr)
        {
            void updater(dynamic c)
            {
                if (!(c is Chapter[]))
                    return;
                DynamicLayout d = null;
                Application.Instance.Invoke(() => d = new DynamicLayout());
                
                d.BeginVertical();

                foreach (Chapter chaps in c)
                {
                    Application.Instance.Invoke(() =>
                    {
                        Label v = new Label() {Text = chaps.name};
                        v.TextAlignment = TextAlignment.Center;
                        d.Add(v);
                    });
                }
                
                d.EndVertical();
                
                Application.Instance.AsyncInvoke(() => _main.Add(d));
                Application.Instance.AsyncInvoke(_main.Create);
            }
            
            new System.Threading.Thread(() =>
            {
                NovelWrapper.GrabChapterList(addr, null, updater);
            }).Start();
        }
    }
}