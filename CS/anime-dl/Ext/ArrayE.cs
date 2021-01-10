using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace anime_dl.Ext
{
    public class ExList<T, A>
    {
        private bool reverse;
        private bool ccl;
        private T[] arr;
        private A[] arr1;
        public int Size;
        private int b;

        public T this[int i] => arr[i];

        public void ModifySize(int newSize)
        {
            if (newSize == Size)
                return;
            if (newSize < arr.Length)
                if (!reverse)
                {
                    arr = arr.Skip(arr.Length - newSize).ToArray();
                    arr1 = arr1.Skip(arr1.Length - newSize).ToArray();
                }
                else
                {
                    arr = arr.Take(newSize).ToArray();
                    arr1 = arr1.Take(newSize).ToArray();
                }
            else
            {
                Array.Resize(ref arr, newSize);
                Array.Resize(ref arr1, newSize);
            }
            Size = newSize;
        }

        public ExList(int size, bool r = false, bool ccl = false)
        {
            Size = size;
            arr = new T[size];
            arr1 = new A[size];
            reverse = r;
            this.ccl = ccl;
        }

        public void push_back(T item, A i)
        {
            for (int idx = (reverse) ? Size - 2 : 1; idx < Size && idx > 0 || idx > 1 && reverse; b = (reverse) ? idx-- : idx++)
                arr[(reverse) ? idx : idx - 1] = arr[(reverse) ? idx - 1 : idx];
            arr[(reverse) ? 0 : Size - 1] = item;

            for (int idx = (reverse) ? Size - 2 : 1; idx < Size && idx > 0 || idx > 1 && reverse; b = (reverse) ? idx-- : idx++)
                arr1[(reverse) ? idx : idx - 1] = arr1[(reverse) ? idx - 1 : idx];
            arr1[(reverse) ? 0 : Size - 1] = i;
        }

        public void Clear()
        { 
            arr = new T[Size];
            arr1 = new A[Size];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (T i in arr)
                sb.Append(i?.ToString() + (ccl ? new string(' ', Console.WindowWidth - (i == null ? 0 : i.ToString().Length)) + "\n" : "\n"));
            return sb.ToString();
        }
    }

    public class ExList<T>
    {
        private bool reverse;
        private bool ccl;
        private T[] arr;
        public int Size;
        private int b;

        public T this[int i] => arr[i];

        public void ModifySize(int newSize)
        {
            if (newSize == Size)
                return;
            if (newSize < arr.Length)
                if (!reverse)
                    arr = arr.Skip(arr.Length - newSize).ToArray();
                else
                    arr = arr.Take(newSize).ToArray();
            else
                Array.Resize(ref arr, newSize);
            Size = newSize;
        }

        public ExList(int size, bool r = false, bool ccl = false)
        {
            Size = size;
            arr = new T[size];
            reverse = r;
            this.ccl = ccl;
        }

        public void push_back(T item)
        {
            for (int idx = (reverse) ? Size - 2 : 1; idx < Size && idx > 0 || idx > 1 && reverse; b = (reverse) ? idx-- : idx++)
                arr[(reverse) ? idx : idx - 1] = arr[(reverse) ? idx - 1 : idx];
            arr[(reverse) ? 0 : Size - 1] = item;
        }

        public void Clear()
            => arr = new T[Size];

        public override string ToString() //TODO: add try/catch
        {
            StringBuilder sb = new StringBuilder();
            foreach (T i in arr)
                sb.Append(i?.ToString() + (ccl ? new string(' ', (Console.WindowWidth >= i?.ToString().Length) ? Console.WindowWidth - (i == null ? 0 : i.ToString().Length) : 0) + "\n" : "\n"));
            return sb.ToString();
        }
    }

    public static class ArrayE
    {
        public static Char[][] Clear(this Char[][] chr) => new char[0][];

        public static Char[][] push_back(this Char[][] charArray, char[] value)
        {
            Char[][] cAT = new char[charArray.Length + 1][];
            for (uint i = 0; i < charArray.Length; i++)
            {
                cAT[i] = charArray[i];
            }
            cAT[cAT.Length - 1] = value;
            return cAT;
        }

        public static Char[] push_back(this Char[] charArray, char value)
        {
            Char[] cAT = new char[charArray.Length + 1];
            for (uint i = 0; i < charArray.Length; i++)
            {
                cAT[i] = charArray[i];
            }
            cAT[cAT.Length - 1] = value;
            return cAT;
        }

        public static object[] push_back(this object[] charArray, object value)
        {
            object[] cAT = new object[charArray.Length + 1];
            for (uint i = 0; i < charArray.Length; i++)
            {
                cAT[i] = charArray[i];
            }
            cAT[cAT.Length - 1] = value;
            return cAT;
        }

        public static Thread[] push_back(this Thread[] charArray, Thread value)
        {
            Thread[] cAT = new Thread[charArray.Length + 1];
            for (uint i = 0; i < charArray.Length; i++)
            {
                cAT[i] = charArray[i];
            }
            cAT[cAT.Length - 1] = value;
            return cAT;
        }

        public static byte[] ToBigEndianBytes(this int i)
        {
            byte[] bytes = BitConverter.GetBytes(Convert.ToUInt64(i));
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }
    }
}
