using System.Collections.Generic;
using ADLCore.Novels.Models;
using Eto.Drawing;
using Eto.Forms;

namespace UIanime_dl.Drawables
{
    public class ChapterData
    {
        public DynamicLayout _main;
        private ImageView img;
        private Chapter Parent;
        
        public ChapterData(ref Bitmap bmp, Chapter parent, bool offline = false)
        {
            Parent = parent;

            _main = new DynamicLayout();
            _main.BeginVertical();
            img.Image = bmp;
            MaskedTextBox tb = new MaskedTextBox();
            tb.Enabled = false;
            tb.Text = Parent.GetText();
            _main.Add(tb);
            _main.EndVertical();
            _main.Create();
        }
    }
}