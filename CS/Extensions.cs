using mshtml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VidStreamIORipper
{
    public static class Extensions
    {
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
    }
}
