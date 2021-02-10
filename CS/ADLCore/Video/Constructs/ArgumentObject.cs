using System;
using System.Reflection;
using System.Text;

namespace ADLCore.Video.Constructs
{
    //Argument Object for easier management of variables.
    public class ArgumentObject
    { 
        public string mn; public string term;
        public bool d; public bool mt; public bool cc; public bool h; public bool s; public bool e; public bool help; public bool aS; public bool nS; public bool c; public bool l;
        public string rootPath;
        public string export;
        private FieldInfo[] foo;


        public ArgumentObject(Object[] arr)
        {
            foo = typeof(ArgumentObject).GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
            for (int idx = 0; idx < arr.Length; idx++)
                foo[idx].SetValue(this, arr[idx]);
        }

        public static ArgumentObject Parse(string[] args)
        {
            string mn = string.Empty;
            string term = string.Empty;
            string export = string.Empty;
            bool d = false, mt = false, cc = false, h = false, s = false, e = false, aS = false, nS = false, help = false, c = false, l = false;
            for (int idx = 0; idx < args.Length; idx++)
            {
                string str = args[idx];
                switch (str)
                {
                    case "ani":
                        if (mn != string.Empty)
                            throw new Exception("ani/nvl selector has already been set in this parameter list.");
                        mn = "ani";
                        break;
                    case "nvl":
                        if (mn != string.Empty)
                            throw new Exception("ani/nvl selector has already been set in this parameter list.");
                        mn = "nvl";
                        break;
                    case "-aS":
                        if (mn != string.Empty)
                            throw new Exception("ani/nvl selector has already been set in this parameter list.");
                        mn = "ani";
                        aS = true;
                        break;
                    case "-nS":
                        if (mn != string.Empty)
                            throw new Exception("ani/nvl selector has already been set in this parameter list.");
                        mn = "nvl";
                        nS = true;
                        break;
                    case "-d":
                        d = true;
                        break;
                    case "-mt":
                        mt = true;
                        break;
                    case "-cc":
                        cc = true;
                        break;
                    case "-c":
                        c = true;
                        break;
                    case "-h":
                        h = true;
                        break;
                    case "-s":
                        s = true;
                        break;
                    case "-e":
                        e = true;
                        break;
                    case "-help":
                        help = true;
                        break;
                    case "-l":
                        l = true;
                        idx++;
                        string k = args[idx];
                        if (k[0] == '\"')
                            k = SearchForPath(args, idx + 1);
                        else
                            export = k;
                        break;
                    default:
                        term += term.Length > 0 ? $" {str}" : str;
                        break;
                }
            }
            return new ArgumentObject(new Object[] { mn, term, d, mt, cc, h, s, e, help, aS, nS, c, l, export });
        }

        private static string SearchForPath(string[] args, int beginning)
        {
            StringBuilder sb = new StringBuilder();
            for(int idx = beginning; idx < args.Length; idx++)
            {
                if (args[idx][0] == '\"')
                    return sb.ToString();
                else
                    sb.Append(args[idx]);
            }
            throw new Exception("You didn't add an end quote to your path!");
        }

        //For any legacy code that remains.
        public object this[int i] {
            get
            {
                return foo[i].GetValue(this);
            }
            set
            {
                foo[i].SetValue(this, value);
            }
        }
    }
}
