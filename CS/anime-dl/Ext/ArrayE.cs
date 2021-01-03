using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace anime_dl.Ext
{
    public class ExList<T>
    {
        private T type;
        private T[] arr;
        private int Size;

        public ExList(int size)
        {
            Size = size;
            arr = new T[size];
        }

        public void push_back(T item)
        {
            for (int idx = 1; idx < Size; idx++)
                arr[idx - 1] = arr[idx];
            arr[Size] = item;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (T i in arr)
                sb.Append(i.ToString());
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
