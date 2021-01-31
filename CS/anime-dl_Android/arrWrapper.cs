using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace anime_dl_Android
{
    public class arrWrapper<T>
    {
        Action<int, string> onIndexSet;

        T[] array;

        public arrWrapper(int size, Action<int, string> u)
        {
            array = new T[size];
            onIndexSet = u;
        }

        public T this[int id]
        {
            set
            {
                array[id] = value;
                onIndexSet(id, value.ToString());
            }
            get => array[id];
        }
    }
}