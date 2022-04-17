using System;
using System.Collections.Generic;
using System.Linq;
using ADLCore.Ext;
using ADLCore.Novels.Models;
using Eto.Drawing;
using Eto.Forms;
using UIanime_dl.Classes;
using Application = Eto.Forms.Application;
using Button = Eto.Forms.Button;
using ListBox = Eto.Forms.ListBox;

namespace UIanime_dl.Drawables
{
    public class DetailPane
    {
        public TabPage Parent;
        
        public DynamicLayout _main;
        private TableLayout _controller;
        public int idx = 0;
        public Bitmap bitM;

        public bool isCollapsed = false;
        
        private List<Chapter> chapters;
        private MetaData mdataObj;
        private ListBox chapterList;
        private ImageView img;
        private TextArea chapterText;
        
        private Button close;
        private Button collapse;
        
        public delegate void chapterSelect(ChapterLabel cLabel);
        public event chapterSelect OnChapterSelect;        
        
        public delegate void reqClose(DetailPane debt);
        public event reqClose OnRequestedClose;
        
        public DetailPane(ref MetaData mdata, Chapter[] chapters, ref TabPage tb)
        {
            if (chapters == null)
                this.chapters = new List<Chapter>();
            else
                this.chapters = this.chapters.ToList();
            Parent = tb;
            
            _main = new DynamicLayout();
            img = new ImageView();
            
            _main.KeyUp += delegate(object? sender, KeyEventArgs args)
            {
                switch (args.Key)
                {
                    case Keys.D:
                    {
                        chapterList.SelectedIndex++;
                        break;
                    }
                    case Keys.A:
                    {
                        if (chapterList.SelectedIndex <= 0)
                            break;
                        chapterList.SelectedIndex--;
                        break;
                    }
                }
            };

            //            this.Width = 1200;
            //              this.Height = 600;
            bitM = new Bitmap(mdata.cover);
            img.Image = bitM;
            img.Width = 100;
            img.Height = 100;
            chapterText = new TextArea();
            chapterText.Width = 800;
            chapterText.Font = new Font("sans-serif", 15.0f, FontStyle.None);
            chapterText.Wrap = true;
            chapterText.ReadOnly = true;
            chapterText.TextAlignment = TextAlignment.Left;

            chapterList = new ListBox();
            chapterList.Width = 400;
            chapterList.SelectedIndexChanged += delegate(object? sender, EventArgs args)
            {   //Default is to keep text in memory.
                if (chapterList.SelectedIndex < 0)
                    return;
                chapterText.Text = this.chapters[chapterList.SelectedIndex].GetText();
            };

            collapse = new Button();
            collapse.Width = 10;
            collapse.Text = ">";
            collapse.Click += delegate(object? sender, EventArgs args)
            {
                if (isCollapsed)
                    chapterText.Width = 800;
                else
                    chapterText.Width = 1180;

                isCollapsed = !isCollapsed;
                Application.Instance.Invoke(() => _main.Create());
            };

            close = new Button();
            close.Width = 10;
            close.Text = "X";
            close.Click += (sender, args) => { OnRequestedClose?.Invoke(this); };

            _controller = new TableLayout();
            _controller.Rows.Add(new TableRow(new TableCell[] {new TableCell(collapse), new TableCell(close), new TableCell(img, true)}));

            _main.BeginHorizontal();
            _main.BeginVertical();
            _main.Add(chapterText);
            _main.EndVertical();
            //_main.AddSpace();
            //_main.AddSpace();
            _main.BeginVertical();
            _main.Add(_controller);
            _main.Add(chapterList);
            _main.EndVertical();
            _main.EndHorizontal();
            Application.Instance.Invoke(() => _main.Create());
        }

        private bool loadedFirst = false;
        public void DetailsPaneUpdateChapterList(MetaData addr)
        {
            void updater(dynamic c)
            {
                if (!(c is Chapter[]))
                    return;
                
                foreach (Chapter chaps in c)
                {
                    chapters.Add(chaps);
                    Application.Instance.Invoke(() =>
                    {
                        if (!loadedFirst)
                        {
                            loadedFirst = true;
                            chapterText.Text = chaps.GetText();
                        }

                        chapterList.Items.Add(chaps?.name.Sanitize());
                    });
                }
                
                Application.Instance.AsyncInvoke(_main.Create);
            }
            
            new System.Threading.Thread(() =>
            {
                NovelWrapper.GrabChapterList(addr, null, updater);
            }).Start();
        }
        ~DetailPane()
        {
            bitM.Dispose();
            chapters.Clear();
            mdataObj = null;
        }
    }
}