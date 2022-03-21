using System.Collections.Generic;
using ADLCore.Novels.Models;
using Eto.Drawing;
using Eto.Forms;

namespace UIanime_dl.Drawables
{
    public class ChapterLabel : Label
    {
        public List<Chapter> chapters;
        public readonly int cPos;
        public Bitmap bmp;

        //No idea how tempting it is to use pointers...
        public ChapterLabel(ref Bitmap _bmp, ref List<Chapter> c, int pos)
        {
            bmp = _bmp;
            chapters = c;
            cPos = pos;
        }
    }
}