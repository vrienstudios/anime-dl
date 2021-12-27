using System;

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