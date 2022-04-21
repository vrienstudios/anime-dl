
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

        delegate void rawQuery(string a, Action<dynamic> dyn, bool expectList);
        delegate void chapList(MetaData m, int[] r, Action<dynamic> chp);
        delegate void grabHome(string site, Action<dynamic> mdat);
        delegate void usrCnf_Index(int idx);
        delegate void getCompFromSel_Field(string name);
        delegate void getCompFromComp_Field(string name);
        delegate void str_CompareComp(string comparisonStr);

        public enum conditionType
        {
            cmp,
            inull,
        }
        
        delegate void ifBld(object aActor, conditionType condition, string bActor, ADLTask a, ADLTask b);

        private List<Delegate> applicableFuncs = new List<Delegate>()
        {   
            //Functions
            (rawQuery)TaskController.RawWrapper,
            (chapList)TaskController.GrabChapterList,
            (grabHome)TaskController.GrabHome,
            (usrCnf_Index)TaskController.SelIndex,
            //Selections
            (getCompFromSel_Field)TaskController.getCompFromSel_Field,
            (getCompFromComp_Field)TaskController.getCompFromComp_Field,
            //Comparisons
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