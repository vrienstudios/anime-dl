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
        public int idx = 0;
        public Bitmap bitM;
        
        public delegate void chapterSelect(ChapterLabel cLabel);

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
            bitM = new Bitmap(mdata.cover);
            img.Image = bitM;
            img.Size = new Size(200, 300);
            _main.Add(img);

            for (; idx < chapters?.Length; idx++)
            {
                ChapterLabel labelia = new ChapterLabel(ref bitM, ref this.chapters, idx) {Text = chapters[idx].name};
                labelia.MouseUp += delegate(object? sender, MouseEventArgs args)
                {
                    OnChapterSelect?.Invoke(sender as ChapterLabel);
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
                    chapters.Add(chaps);
                    Application.Instance.Invoke(() =>
                    {
                        ChapterLabel v = new ChapterLabel(ref bitM, ref chapters, idx) {Text = $"{chaps.name}"};
                        v.TextAlignment = TextAlignment.Center;
                        v.MouseDoubleClick += (sender, args) =>
                        {
                            Chapter[] cd = c;
                            OnChapterSelect?.Invoke(sender as ChapterLabel);
                        };
                        idx++;
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