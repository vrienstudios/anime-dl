using mshtml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VidStreamIORipper
{
    public static class Extensions
    {
        public static bool IsMp4(string lnk) => lnk.Contains(".mp4");

        public static string getBase64Uri(string xe) => FixUri(Encoding.UTF8.GetString(Convert.FromBase64String(xe)));

        public static int indexOfEquals(string id)
        {
            for (int idx = 0; idx < id.Length; idx++)
                if (id[idx] == '=')
                    return idx;
            return -1;
        }

        public static string FixUri(string uri)
        {
            string tes = "storage";
            for(int idx = 0; idx < uri.Length; idx++)
            {
                if (sString(uri, idx, (tes.Length)) == tes)
                {
                    return $"https://{uri.Substring(idx, uri.Length - idx)}";
                }
            }
            return null;
        }

        public static string sString(string b, int i, int x)
        {
            Char[] ca = new char[0];
            int d = 0;
            while(d < x)
            {
                ca = ca.push_back(b[i + d]);
                d++;
                if (d > x)
                    break;
            }
            return new string(ca);
        }

        public static String TrimIntegrals(this String str)
        {
            int charsRemoved = 0;
            for(int idx = str.Length - 1; idx > 0; idx--)
            {
                if (char.IsNumber(str[idx]))
                    charsRemoved++;
                else
                    break;
            }
            return str.Substring(0, str.Length - charsRemoved);
        }

        public static string ExtractEpisodeNumber(string ep)
        {
            bool enNum = false;
            Char[] numbers = new char[0];
            ep = ep.Split('.')[1];
            for(int idx = ep.Length - 1; idx > 0; idx--)
            {
                if (char.IsDigit(ep[idx]))
                {
                    if (!enNum)
                        enNum = true;
                    numbers = numbers.push_back(ep[idx]);
                }
                else if(enNum && !char.IsDigit(ep[idx]))
                    return new string(numbers);
            }
            Array.Reverse(numbers);
            return new string(numbers);
        }

        public static string TrimToSlash(this String str)
        {
            int charsRemoved = 0;
            for (int idx = str.Length - 1; idx > 0; idx--)
            {
                if (str[idx] != '/')
                    charsRemoved++;
                else
                    break;
            }
            str = str.Substring(0, str.Length - charsRemoved);
            return str;
        }

        public static Char[][] Clear(this Char[][] chr) => new char[0][];

        public static Char[][] push_back(this Char[][] charArray, char[] value)
        {
            Char[][] cAT = new char[charArray.Length + 1][];
            for(uint i = 0; i < charArray.Length; i++)
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

        public static string getNumStr(this string uri, string e = "", int i = -1) => i < 0 ? getNumStr(uri, e, uri.Length - 1) : Char.IsDigit(uri[i]) == true ? getNumStr(uri, InsertAtFront(e, uri[i]), i - 1) : e;

        public static int getNum(this string uri, string e = "", int i = -1) => i < 0 ? getNum(uri, e, uri.Length - 1) : Char.IsDigit(uri[i]) == true ? getNum(uri, InsertAtFront(e, uri[i]), i - 1) : int.Parse(e);

        public static string InsertAtFront(this string buffer, char ins)
        {
            string a = "";
            a += ins;
            return a += buffer;
        }
    }
}
