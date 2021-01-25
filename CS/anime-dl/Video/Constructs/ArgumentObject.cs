using System;
using System.Reflection;

namespace anime_dl.Video.Constructs
{
    //Argument Object for easier management of variables.
    class ArgumentObject
    { 
        public string mn; public string term;
        public bool d; public bool mt; public bool cc; public bool h; public bool s; public bool e; public bool help; public bool aS; public bool nS; public bool c;
        private FieldInfo[] foo;


        public ArgumentObject(Object[] arr)
        {
            foo = typeof(ArgumentObject).GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
            for (int idx = 0; idx < foo.Length; idx++)
                foo[idx].SetValue(this, arr[idx]);
        }

        //For any legacy code that remains.
        public object this[int i] {
            get
            {
                return foo[i].GetValue(this);
            }
            set
            {
                foo[i].SetValue(this, value);
            }
        }
    }
}
