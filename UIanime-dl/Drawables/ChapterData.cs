using System;
using System.Collections.Generic;
using ADLCore.Novels.Models;
using Eto.Drawing;
using Eto.Forms;

namespace UIanime_dl.Drawables
{
    public class ChapterData
    {
        public DynamicLayout _main;
        private MaskedTextBox tb;
        private ImageView img;

        private Scrollable scrolliea;

        private Chapter[] PoX;
        private Chapter[] PosI = new Chapter[3]{null, null, null};
        
        public ChapterData(ref Bitmap bmp, Chapter[] chaps, bool offline = false)
        {
            PoX = chaps;
            tb = new MaskedTextBox();
            scrolliea = new Scrollable();

            for (int i = 0; i < 3 && i < PoX.Length; i++)
                PosI[i] = PoX[i];

            for (int idx = 0; idx < PosI.Length; idx++)
            {
                tb.Text += $"\n\n|{PosI[idx]?.parsedName}|\n\n{PosI[idx]?.GetText()}\n\n";
                if(PosI[idx] != null) PosI[idx].content = null;
            }

            scrolliea.Content = tb;
            scrolliea.UpdateScrollSizes();
            scrolliea.Scroll += delegate(object? sender, ScrollEventArgs args)
            {
                
            };
            _main = new DynamicLayout();
            _main.BeginVertical();
            _main.Add(scrolliea);
            _main.EndVertical();
            _main.Create();
        }
        
    }
}