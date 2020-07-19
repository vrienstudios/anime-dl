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
    }
}
