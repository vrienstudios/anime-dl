using ADLCore.Novels.Models;

namespace UIanime_dl.Drawables
{
    public class ChapterData
    {
        private string text;
        public ChapterData(Chapter parent, bool offline = false)
        {
            if(!offline)
                text = parent.GetText();
        }
    }
}