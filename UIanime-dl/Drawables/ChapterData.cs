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
        private TextArea tb;
        private ImageView img;

        private Scrollable scrolliea;

        private List<Chapter> PoX;
        private Chapter[] PosI = new Chapter[3]{null, null, null};
        
        public ChapterData(ref Bitmap bmp, ref List<Chapter> chaps, bool offline = false)
        {
            PoX = chaps;
            tb = new TextArea();
            tb.TextAlignment = TextAlignment.Left;
            tb.Wrap = true;
            scrolliea = new Scrollable();
            tb.Font = new Font("sans-serif", 20.0f, FontStyle.None);
            for (int i = 0; i < 3 && i < PoX.Count; i++)
                PosI[i] = PoX[i];

            for (int idx = 0; idx < PosI.Length; idx++)
            {
                tb.Text += $"\r\n\r\n|{PosI[idx]?.parsedName}|\r\n\r\n{PosI[idx]?.GetText()}\r\n\r\n";
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