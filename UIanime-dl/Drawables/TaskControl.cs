
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ADLCore.Novels.Models;
using Eto.Forms;
using UIanime_dl.Classes;

namespace UIanime_dl.Drawables
{

    public class TaskControl
    {
        private NovelWrapper nw = new NovelWrapper();

        delegate void rawQuery(string a, Action<dynamic> dyn);
        delegate void chapList(MetaData m, int[] r, Action<Chapter> chp);
        delegate void grabHome(string site, Action<MetaData> mdat);
        delegate void usrCnf_Index(int idx);

        delegate void getCompFromSel_Field(string name);
        delegate void getCompFromComp_Field(string name);
        delegate void str_CompareComp(string comparisonStr);
        
        delegate void ifBld(object aActor, string condition, string bActor, ADLTask a, ADLTask b);

        private List<Delegate> applicableFuncs = new List<Delegate>()
        {
            (chapList)TaskController.GrabChapterList,
            (grabHome)TaskController.GrabHome,
            (usrCnf_Index)TaskController.SelIndex,
            
            (getCompFromSel_Field)TaskController.getCompFromSel_Field,
            (getCompFromComp_Field)TaskController.getCompFromComp_Field,
            (str_CompareComp)TaskController.str_CompareComp,
        };

        public DynamicLayout _mainLayout;
        private TabControl _underly;
        private TabPage lists;
        private TabPage taskProgrammer;

        private DynamicLayout _taskList;
        private DynamicLayout programmer;

        private ADLTask[] runningTasks;
        
        public TaskControl()
        {
            _mainLayout = new DynamicLayout();
            _underly = new TabControl();
            lists = new TabPage();
            taskProgrammer = new TabPage();
            _taskList = new DynamicLayout();
            programmer = new DynamicLayout();

            #region controlBuilding

            lists.Content = _taskList;
            ListBox lb = new ListBox();
            
            #endregion
            
            #region SetControls
            _mainLayout.Content = _underly;
            #endregion
        }
    }
}