using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ADLCore.Ext
{
    public static class Strings
    {
        //Deletes all duplicate characters in a string
        public static string DeleteConDuplicate(this string str)
        {
            List<char> chars = new List<char>();
            foreach (char chr in str.ToList())
                if (chars.Count <= 0 || chars.Last() != chr)
                    chars.Add(chr);
            return new string(chars.ToArray());
        }

        /// <summary>
        /// Deletes all duplicate characters of the delimiter
        /// </summary>
        /// <param name="str"></param>
        /// <param name="del">Chars to remove duplicates of</param>
        /// <returns></returns>
        public static string DeleteConDuplicate(this string str, char del)
        {
            List<char> chars = new List<char>();
            foreach (char chr in str.ToList())
                if (chars.Count == 0 || (chars.Last() == del ? !(chr == del) : true))
                    chars.Add(chr);
            return new string(chars.ToArray());
        }

        /// <summary>
        /// Sanitize text so that it 'looks normal'
        /// </summary>
        /// <param name="_base"></param>
        /// <param name="f"></param>
        /// <returns></returns>
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
                if (_base[idx] != ' ' && _base[idx] != '\r' && _base[idx] != '\n' && _base[idx] != '\t')
                {
                    sb.Append(new string(((!f) ? _base.Skip(idx) : _base.Take(idx + 1)).ToArray()));
                    break;
                }

                bool d = f == true ? (idx--) > 0 : (idx++) > 0;
            }

            return f == false ? Sanitize(sb.ToString(), true) : sb.ToString();
        }

        private static string Reverse(this string str)
        {
            char[] a = str.ToCharArray();
            Array.Reverse(a);
            return new string(a);
        }

        /// <summary>
        /// Skips a sequence of chars, e.x abcdhelloworld -> helloworld.
        /// </summary>
        /// <param name="_base"></param>
        /// <param name="charSeq"></param>
        /// <param name="h">Index</param>
        /// <returns></returns>
        public static string SkipCharSequence(this string _base, char[] charSeq, int h = 0)
        {
            while (true)
            {
                if ((h < charSeq.Length))
                {
                    if ((_base[h] == charSeq[h]))
                    {
                        h = h + 1;
                        continue;
                    }

                    return _base.Substring(h, _base.Length - h);
                }

                return _base.Substring(h, _base.Length - h);
                break;
            }
        }

        public static string SkipPreceedingAndChar(this string _base, char singular, int h = 0)
        {
            while (true)
            {
                if ((h < _base.Length))
                {
                    if (_base[h] == singular) return _base.Substring(h + 1, _base.Length - (h + 1));
                    h = h + 1;
                    continue;
                }

                return _base;
                break;
            }
        }

        /// <summary>
        /// Deletes everything after the first whitespace detected.
        /// </summary>
        /// <param name="_base"></param>
        /// <param name="h"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string DeleteFollowingWhiteSpaceA(this string _base, int h = 0, int a = 0)
        {
            while (true)
            {
                if ((h < _base.Length))
                {
                    if (_base[h] != '\x20')
                    {
                        h = h + 1;
                        a = 0;
                        continue;
                    }

                    h = h + 1;
                    a = a + 1;
                    continue;
                }

                return _base.Substring(0, _base.Length - a);
                break;
            }
        }

        /// <summary>
        /// If the string has multiple spaces/whitespaces, use this one.
        /// </summary>
        /// <param name="_base"></param>
        /// <param name="h"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string DeleteFollowingWhiteSpaceB(this string _base, int h = 0, int a = 0)
            => (h < _base.Length)
                ? ((_base[h] != '\x20')
                    ? DeleteFollowingWhiteSpaceA(_base, h + 1)
                    : ((h < _base.Length - 1)
                        ? (_base[h + 1] != '\x20'
                            ? DeleteFollowingWhiteSpaceA(_base, h + 1)
                            : DeleteFollowingWhiteSpaceA(_base, h + 1, a + 1))
                        : DeleteFollowingWhiteSpaceA(_base, h + 1, a + 1)))
                : _base.Substring(0, _base.Length - a);

        public static string RemoveSpecialCharacters(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            StringBuilder sb = new StringBuilder
            {
                Capacity = str.Length
            };
            foreach (var c in str.Where(c =>
                (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' ||
                c == '\'' || c == ' ' || c == '(' || c == ')' || c == '-'))
                sb.Append(c);
            return sb.ToString();
        }

        public static string NonSafeRemoveSpecialCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder();
            sb.Capacity = str.Length;
            foreach (var c in str.Where(c =>
                (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' ||
                c == '\'' || c == ' ' || c == '(' || c == ')' || c == '-' || c >= '!' || c <= ')'))
                sb.Append(c);
            return sb.ToString();
        }

        private static int countChars(string _base, char t, int i)
        {
            for (int idx = i, k = 0; idx < _base.Length; idx++, k++)
                if (_base[idx] == t)
                    continue;
                else
                    return k;
            return _base.Length - i;
        }

        //Haven't done one in a long time; give me a break.
        public static string RemoveExtraWhiteSpaces(this string _base, int h = 0, char[] yoreck = null)
        {
            while (true)
            {
                if ((_base.Length - 1 <= h))
                    return _base.Last() == ' '
                        ? yoreck.Last() == ' '
                            ? new string(yoreck.Take(yoreck.Length - 1).ToArray())
                            : new string(yoreck)
                        : new string(yoreck.push_back(_base[h]));
                if ((h == _base.Length - 1))
                {
                    if ((_base[h] == ' ')) return new string(yoreck);
                    var @base = _base;
                    h = h + 1;
                    yoreck = yoreck.push_back(@base[@base.Length]);
                    continue;
                }

                if (yoreck == null)
                {
                    yoreck = new char[0];
                    continue;
                }

                if (_base[0] == ' ' && h == 0)
                {
                    var @base = _base;
                    h = h + countChars(@base, ' ', h);
                    continue;
                }

                if (_base[h] == ' ' && _base[h + 1] == ' ')
                {
                    var @base = _base;
                    var h1 = h;
                    h = h + countChars(@base, ' ', h);
                    yoreck = yoreck.push_back(@base[h1]);
                    continue;
                }

                var base1 = _base;
                var h2 = h;
                h = h + 1;
                yoreck = yoreck.push_back(base1[h2]);
            }
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
            StringBuilder sb = new StringBuilder() {Capacity = r - n};
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

        public static string InsertAtFront(this string buffer, object ins)
        {
            string a = "";
            a += ins;
            return a += buffer;
        }

        public static string getNumStr(this string uri, string e = "", int i = -1)
        {
            while (true)
            {
                if (i < 0)
                {
                    var uri1 = uri;
                    i = uri1.Length - 1;
                    continue;
                }

                if (Char.IsDigit(uri[i]) == true)
                {
                    var uri1 = uri;
                    e = InsertAtFront(e, uri1[i]);
                    i = i - 1;
                    continue;
                }

                return e;
                break;
            }
        }

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

            return str.Substring(0, str.Length - charsRemoved).RemoveExtraWhiteSpaces();
        }

        //Extract Episode number from GoGoStream manifests.
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

        public static string TrimToSlash(this String str, char slashType = '/', bool keepSlash = true)
        {
            int charsRemoved = 0;
            for (int idx = str.Length - 1; idx > 0; idx--)
            {
                if (str[idx] != slashType)
                    charsRemoved++;
                else
                {
                    if (!keepSlash)
                        charsRemoved++;
                    break;
                }
            }

            str = str.Substring(0, str.Length - charsRemoved);
            return str;
        }

        //http://www.merriampark.com/ldjava.htm (Modified)
        public static int getSimilarityScore(this string a, string b)
        {
            if (b.Length == 0)
                return a.Length;

            int[] pchor = new int[b.Length + 1];
            int[] cchor = new int[b.Length + 1];
            int[] placeholder = new int[b.Length + 1];
            int i, j;

            for (i = 0; i <= b.Length; i++)
                pchor[i] = i;

            for (j = 1; j <= a.Length; j++)
            {
                if (j >= cchor.Length)
                    continue;
                cchor[j] = j; //b[j - 1];
                for (i = 1; i <= b.Length; i++)
                {
                    if (b[i - 1] == a[j - 1])
                        cchor[i] = Math.Min(Math.Min(cchor[i - 1] + 1, pchor[i] + 1), pchor[i - 1] + 0);
                    else
                        cchor[i] = Math.Min(Math.Min(cchor[i - 1] + 1, pchor[i] + 1), pchor[i - 1] + 1);
                }

                Array.Copy(pchor, placeholder, pchor.Length);
                Array.Copy(cchor, pchor, cchor.Length);
                Array.Copy(placeholder, cchor, placeholder.Length);
            }

            return b.Length >= pchor.Length ? pchor[pchor.Length - 1] + (b.Length - a.Length) : pchor[b.Length];
        }

        public static string getBase64Uri(string xe) => FixUri(Encoding.UTF8.GetString(Convert.FromBase64String(xe)));

        public static string calculateProgress(char type, int progress, int total)
        {
            double prg = (double) progress / (double) total;
            if (double.IsNaN(prg))
                return "0/0";
            else
                return new string(
                    $"[{new string(type, (int) (prg * 10))}{new string(' ', 10 - (int) (prg * 10))}] {(int) (prg * 100)}% {progress}/{total}");
        }

        public static string RemoveStringA(this string original, string purge, bool allafter, int h = 0)
        {
            char[] pruneBuffer = purge.ToCharArray();
            char[] charBuffer = new char[pruneBuffer.Length];
            while (h < original.Length)
            {
                original.CopyTo(h, charBuffer, 0, charBuffer.Length);
                if (pruneBuffer.SequenceEqual(charBuffer))
                    break;
                h++;
            }

            return original.Substring(0, h);
        }

        public static WebHeaderCollection Clone(this WebHeaderCollection coll)
        {
            if (coll == null) //nullcheck
                return null;
            var ncol = new WebHeaderCollection();
            foreach (string k in coll.AllKeys)
                ncol.Add((string) k.Clone(), (string) coll[k].Clone());
            return ncol;
        }
    }
}