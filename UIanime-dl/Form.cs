using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using ADLCore;
using ADLCore.Ext;
using ADLCore.Novels.Models;
using ADLCore.SiteFolder;
using Eto.Drawing;
using Eto.Forms;
using Gtk;
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
        private Scrollable _scrollable;
        private TableLayout _dynamicLayout;
        private TableLayout _cardLayoutA;
        private NovelWrapper novelWrapper = new NovelWrapper();
        
        #endregion

        private List<MetaData> content;

        //https://www.youtube.com/watch?v=Ig3AUN6LvCo
        public Form()
        {
            this.Width = 1200;
            this.Height = 600;
            this.Title = "ADL";

            #region init

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
            
            lb = new DropDown();
            tb = new SearchBox();
            
            foreach (SiteBase site in Sites.continuity)
                lb.Items.Add(site.host);

            _dynamicLayout = new TableLayout();
            _dynamicLayout.Padding = new Padding(5, 5, 5, 5);

            userInputGrp = new TableLayout();
            userInputGrp.Rows.Add(new TableRow(new TableCell(lb), new TableCell(tb)));
            
            lb.SelectedKeyChanged += LbOnSelectedKeyChanged;
            
            _dynamicLayout.Rows.Add(userInputGrp);

            _cardLayoutA = new TableLayout();
            TableRow tr = new TableRow();
            cardLayoutB = new DynamicLayout();
            tr.Cells.Add(cardLayoutB);
            tr.Cells.Add(new TableCell(null));
            _cardLayoutA.Rows.Add(tr);

            fin = new TableRow();
            fin.Cells.Add(_cardLayoutA);
            //fin.Cells.Add(null);
            
            _dynamicLayout.Rows.Add(fin);
            _dynamicLayout.Rows.Add(null);
            _home.Content = _dynamicLayout;

            #endregion
        }

        private void LbOnSelectedKeyChanged(object? sender, EventArgs e)
        {
            var selKey = lb.SelectedKey;
            new Thread(x =>
            {
                novelWrapper.GrabHome($"https://{selKey}", CardUpdateHome);
            }).Start();
        }

        private List<Card> cards = new List<Card>();
        TableRow fin;
        private TableLayout userInputGrp;
        private DropDown lb;
        private TextBox tb;
        private DynamicLayout cardLayoutB;
        
        private void CardUpdateHome(MetaData addr)
        {
            var crd = new Card(addr);
            cards.Add(crd);
            crd.onCardClick += sender => { UploadDetailPaneToTabControl(addr); };

            Eto.Forms.Application.Instance.Invoke(cardLayoutB.RemoveAll);
            Eto.Forms.Application.Instance.Invoke(cardLayoutB.Clear);

            bool b = false;
            cardLayoutB.BeginScrollable();
            for (int idx = 0, mindex = 0; mindex < cards.Count && idx != cards.Count; mindex++)
            {
                cardLayoutB.BeginVertical();
                cardLayoutB.BeginHorizontal();
                for (int life = 0; life < 4 && idx < cards.Count; idx++, life++) // is :c
                    cardLayoutB.Add(cards[idx]._main);
                cardLayoutB.EndHorizontal();
                cardLayoutB.EndVertical();
            }
            cardLayoutB.EndScrollable();

            Eto.Forms.Application.Instance.Invoke(cardLayoutB.Create);
        }

        private void UploadDetailPaneToTabControl(MetaData mdata)
        {
            TabPage tp = new TabPage();
            tp.MouseDoubleClick += (sender, args) => { _tabControl.Remove(tp); };
            DetailPane cDat = new DetailPane(ref mdata, null);
            tp.Content = cDat._main;
            _tabControl.Pages.Add(tp);
            tp.Text = mdata.name;
            cDat.DetailsPaneUpdateChapterList(mdata);
        }
    }
}