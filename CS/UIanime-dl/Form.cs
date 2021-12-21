using Eto.Forms;

namespace UIanime_dl
{
    public class Form : Eto.Forms.Form
    {
        #region Controls
        
        private TabControl _tabControl;
        private TabPage _home;
        private TabPage _allDownloads;
        private TabPage _help;
        
        #endregion
        
        public Form()
        {
            this.Width = 1200;
            this.Height = 600;
            this.Title = "ADL";
            
            #region init

            _tabControl = new TabControl();
            _home = new TabPage();
            _allDownloads = new TabPage();
            _help = new TabPage();

            #endregion
        }
    }
}