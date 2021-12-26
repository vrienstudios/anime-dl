using System.Collections.Generic;
using ADLCore;
using ADLCore.Novels.Models;
using ADLCore.SiteFolder;
using Eto.Forms;
using UIanime_dl.Classes;
using UIanime_dl.Drawables;

namespace UIanime_dl
{
    public class Form : Eto.Forms.Form
    {
        #region MainControls

        private TabControl _tabControl;
        private TabPage _home;
        private TabPage _allDownloads;
        private TabPage _help;
        
        #endregion

        private List<MetaData> content;

        //https://www.youtube.com/watch?v=Ig3AUN6LvCo
        public Form()
        {
            this.Width = 1200;
            this.Height = 600;
            this.Title = "ADL";

            #region init

            NovelWrapper novelWrapper = new NovelWrapper();
            content = novelWrapper.GrabHome(Site.NovelFull); //TEST
            
            _tabControl = new TabControl();
            _home = new TabPage() {Text = "Home"};
            _allDownloads = new TabPage() {Text = "Downloads"};
            _help = new TabPage(){Text="Help"};
            
            _tabControl.Pages.Add(_home);
            _tabControl.Pages.Add(_allDownloads);
            _tabControl.Pages.Add(_help);
            Content = _tabControl;

            #endregion
            
            #region HomeSetup
            
            DropDown lb = new DropDown();
            TextBox tb = new SearchBox();
            
            foreach (SiteBase site in Sites.continuity)
                lb.Items.Add(site.host);

            DynamicLayout _dynamicLayout = new DynamicLayout();
            _dynamicLayout.BeginVertical();

            TableLayout userInputGrp = new TableLayout();
            userInputGrp.Rows.Add(new TableRow(new TableCell(lb), new TableCell(tb)));
            
            _dynamicLayout.Add(userInputGrp);
            
            DynamicLayout _cardLayoutA = new DynamicLayout();
            _cardLayoutA.BeginHorizontal();
            
            foreach (MetaData obj in content)
                _cardLayoutA.Add(new Card(obj)._main);
            _dynamicLayout.Add(_cardLayoutA);
            _home.Content = _dynamicLayout;

            #endregion
        }
    }
}