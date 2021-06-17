using System;
using System.Collections.Generic;

namespace ADLCore.Ext
{
    public static class Integer
    {
        public static int getNum(this string uri, string e = "", int i = -1) => i < 0 ? getNum(uri, e, uri.Length - 1) : Char.IsDigit(uri[i]) == true ? getNum(uri, Strings.InsertAtFront(e, uri[i]), i - 1) : int.Parse(e);

        public static int countFirstChars(this string[] arr, char c, int position = 0, int count = 0) => position < arr.Length ? arr[position][0] != c ? countFirstChars(arr, c, position + 1, count + 1) : countFirstChars(arr, c, position + 1, count) : count;
        
        /// <summary>
        /// Counts amount of numbers.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static int LeadingIntegralCount(this char[] str, int h = 0)
                => h != str.Length ? ((str[h] >= '0' && str[h] <= '9') ? LeadingIntegralCount(str, h + 1) : h) : h;

        /// <summary>
        /// Get first integrels in a sequence
        /// </summary>
        /// <param name="str"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static int FirstLIntegralCount(this char[] str, int h = 0, int num = 0, bool f = false)
            => h != str.Length ? (str[h] >= '0' && str[h] <= '9' ? FirstLIntegralCount(str, h + 1, (num * 10) + int.Parse(str[h].ToString()), true) : f == true ? num : FirstLIntegralCount(str, h + 1, num, f)) : (num);

        public static int CountFollowingWhiteSpace(this string str, int h, int i = 0)
                => (h > 0) ? (str[h] == '\x20' ? CountFollowingWhiteSpace(str, h - 1, i + 1) : i) : i;

        /// <summary>
        /// Gets Greatest Common factors from one number.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int[] GCFS(this int n)
        {
            if(Math.Sqrt(n) % 1 == 0)
            {
                int[] ad = GetPrimeFactors(n); //Not perfect, but it helps?
                if (ad[0] == -1)
                    return new int[] { ad[0], ad[1], ad[2] }; //Make up for shortfall in multithreading code, ad[0,1] will always be < n
                else return new int[] { ad[0], ad[1] };
            }
            int m = (int)Math.Ceiling(Math.Sqrt(n));    
            int b = m * m - n;
            int loop = 0;
            while (Math.Sqrt(b) % 1 != 0)
            {
                m++;
                b = m * m - n;
                loop++;
                if (loop >= 600)
                    return new int[] { -1, 2, n / 2 };
            }
            int a = (int)(m - Math.Sqrt(b));
            int d = (int)(m + Math.Sqrt(b));
            return new int[] { a, d };
        }

        public static int[] Force(int n)
        {
            int a, d;
            return null;
        }

        //http://ramanujan.math.trinity.edu/rdaileda/teach/s18/m3341/lectures/fermat_factor.pdf
        public static int[] GetPrimeFactors(long n) // Search for Pairs
        {
            long a, d;
            switch (n)
            {
                case 1: a = -1; d = -1; break;
                case 2: a = 2; d = 1; break;
                default:
                    {
                        long ab = (long)Math.Ceiling(Math.Sqrt(n));
                        long tn = 0;
                        long s;
                        long x;

                        while(true)
                        {
                            x = (((ab + tn) * (ab + tn)) - n);
                            float sx = (float)Math.Sqrt(x);
                            if (sx % 1 == 0) {
                                s = (long)sx;
                                break;
                            }
                            tn++;
                        }

                        a = (int)((ab + tn) - s);
                        d = (int)((ab + tn) + s);

                        break;
                    }
            }
            if (a * d == n)
                return new int[] { (int)a, (int)d };
            else
                return new int[] { -1, (int)a, (int)d };
        }

        public static int indexOfEquals(string id)
        {
            for (int idx = 0; idx < id.Length; idx++)
                if (id[idx] == '=')
                    return idx;
            return -1;
        }
    }
}
