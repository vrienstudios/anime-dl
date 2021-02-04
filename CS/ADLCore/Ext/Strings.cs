using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ADLCore.Ext
{
    public static class Strings
    {
        public static string DeleteConDuplicate(this string str)
        {
            List<char> chars = new List<char>();
            foreach (char chr in str.ToList())
                if (chars.Count <= 0 || chars.Last() != chr)
                    chars.Add(chr);
            return new string(chars.ToArray());
        }

        public static string DeleteConDuplicate(this string str, char del)
        {
            List<char> chars = new List<char>();
            foreach (char chr in str.ToList())
                if (chars.Count == 0 || (chars.Last() == del ? !(chr == del) : true))
                    chars.Add(chr);
            return new string(chars.ToArray());
        }

        public static string Sanitize(this string _base, bool f = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Capacity = _base.Length;
            int idx = 0, _b = 0;

            if (f)
                idx = _base.Length - 1;
            else
                _b = _base.Length;

            for (; idx < _b || idx > _b;)
            {
                if (_base[idx] != ' ' && _base[idx] != '\r' && _base[idx] != '\n')
                {
                    sb.Append(new string(((!f) ? _base.Skip(idx) : _base.Take(idx + 1)).ToArray()));
                    break;
                }
                bool d = f == true ? (idx--) > 0 : (idx++) > 0;
            }
            return f == false ? Sanitize(sb.ToString(), true) : sb.ToString();
        }

        private static string Reverse(this string str) {
            char[] a = str.ToCharArray();
            Array.Reverse(a);
            return new string(a);
        }

        public static string SkipCharSequence(this string _base, char[] charSeq, int h = 0)
            => (h < charSeq.Length) ? ((_base[h] == charSeq[h]) ? SkipCharSequence(_base, charSeq, h + 1) : _base.Substring(h, _base.Length - h)) : _base.Substring(h, _base.Length - h);

        public static string SkipPreceedingAndChar(this string _base, char singular, int h = 0)
            => (h < _base.Length) ? (_base[h] == singular ? _base.Substring(h + 1, _base.Length - (h + 1)) : SkipPreceedingAndChar(_base, singular, h + 1)) : _base;

        /// <summary>
        /// Deletes everything after the first whitespace detected.
        /// </summary>
        /// <param name="_base"></param>
        /// <param name="h"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string DeleteFollowingWhiteSpaceA(this string _base, int h = 0, int a = 0)
            => (h < _base.Length) ? (_base[h] != '\x20' ? DeleteFollowingWhiteSpaceA(_base, h + 1) : DeleteFollowingWhiteSpaceA(_base, h + 1, a + 1)) : _base.Substring(0, _base.Length - a);

        /// <summary>
        /// If the string has multiple spaces/whitespaces, use this one.
        /// </summary>
        /// <param name="_base"></param>
        /// <param name="h"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string DeleteFollowingWhiteSpaceB(this string _base, int h = 0, int a = 0)
            => (h < _base.Length) ? ((_base[h] != '\x20') ? DeleteFollowingWhiteSpaceA(_base, h + 1) : ((h < _base.Length - 1) ? (_base[h + 1] != '\x20' ? DeleteFollowingWhiteSpaceA(_base, h + 1) : DeleteFollowingWhiteSpaceA(_base, h + 1, a + 1)) : DeleteFollowingWhiteSpaceA(_base, h + 1, a + 1))) : _base.Substring(0, _base.Length - a);

        public static string RemoveSpecialCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder();
            sb.Capacity = str.Length;
            foreach (char c in str)
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == '\'' || c == ' ' || c == '(' || c == ')')
                    sb.Append(c);
            return sb.ToString();
        }

        public static string GetFileName(this string str, int dif = 4)
        {
            int length = str.Length - dif;
            for (int idx = length; idx > 0; idx--)
                if (str[idx] == '\\')
                    return str.Substring(idx + 1, length - (idx + 1));
            return null;
        }

        public static string ParseFromRange(string[] arr, int n, int r)
        {
            StringBuilder sb = new StringBuilder() { Capacity = r - n };
            for (int idx = n; idx < r; idx++)
                sb.Append(arr[idx]);
            return sb.ToString();
        }

        public static string InsertAtFront(this string buffer, char ins)
        {
            string a = "";
            a += ins;
            return a += buffer;
        }

        public static string getNumStr(this string uri, string e = "", int i = -1) => i < 0 ? getNumStr(uri, e, uri.Length - 1) : Char.IsDigit(uri[i]) == true ? getNumStr(uri, InsertAtFront(e, uri[i]), i - 1) : e;

        public static string FixUri(string uri)
        {
            string tes = "storage";
            for (int idx = 0; idx < uri.Length; idx++)
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
            while (d < x)
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
            for (int idx = str.Length - 1; idx > 0; idx--)
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
            for (int idx = ep.Length - 1; idx > 0; idx--)
            {
                if (char.IsDigit(ep[idx]))
                {
                    if (!enNum)
                        enNum = true;
                    numbers = numbers.push_back(ep[idx]);
                }
                else if (enNum && !char.IsDigit(ep[idx]))
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

        public static string getBase64Uri(string xe) => FixUri(Encoding.UTF8.GetString(Convert.FromBase64String(xe)));
    }
}
