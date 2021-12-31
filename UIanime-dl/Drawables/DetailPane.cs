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
            
            _main.BeginScrollable();
            _main.BeginVertical();

            img.Image = new Bitmap(bytes);
            img.Size = new Size(200, 300);
            _main.Add(img);

            for (int idx = 0; idx < chapters?.Length; idx++)
                _main.Add(new Label(){Text=chapters[idx].name});
            
            Application.Instance.Invoke(() => _main.Create());
        }

        public void DetailsPaneUpdateChapterList(MetaData addr)
        {
            int sectionalIndex = 0;
            int amntRead = 0;
            void updater(Chapter c)
            {
                if (c == null)
                {
                    DynamicLayout de = null;
                    Application.Instance.Invoke(() => de = new DynamicLayout());
                    de.BeginVertical();

                    if (chapters.Count <= 10 * sectionalIndex + 1)
                        return;

                    for(; amntRead < chapters.Count; amntRead++)
                        Application.Instance.Invoke(() =>
                        {
                            de.Add(new Label() {Text = chapters[amntRead].name});
                        });
                
                    de.EndVertical();
                    Application.Instance.AsyncInvoke(() => _main.Add(de));
                    Application.Instance.AsyncInvoke(_main.Create);
                    return;
                }
                chapters.Add(c);

                DynamicLayout d = null;
                Application.Instance.Invoke(() => d = new DynamicLayout());
                
                d.BeginVertical();

                if (chapters.Count <= 20 * sectionalIndex + 1)
                    return;
                for (int idx = 0; idx < 20 && idx < chapters.Count; idx++, amntRead++)
                    Application.Instance.Invoke(() =>
                    {
                        d.Add(new Label() {Text = chapters[amntRead].name});
                    });
                
                d.EndVertical();

                sectionalIndex++;
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