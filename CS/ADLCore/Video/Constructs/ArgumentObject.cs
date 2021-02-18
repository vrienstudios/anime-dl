using ADLCore.Ext;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ADLCore.Video.Constructs
{
    public class argumentList
    {
        public string mn = string.Empty;
        public string term = string.Empty;
        public bool d;
        public bool mt;
        public bool cc;
        public bool h;
        public bool s;
        public bool gS;
        public bool hS;
        public bool tS;
        public bool e;
        public bool help;
        public bool aS;
        public bool nS;
        public bool c;
        public bool l;
        public string export = string.Empty;
        public bool vRange;
        public int[] VideoRange;
        public bool addInt;
    }

    //Argument Object for easier management of variables.
    public class ArgumentObject
    {
        public argumentList arguments;
        private FieldInfo[] foo;


        public ArgumentObject(Object[] arr)
        {
            foo = typeof(ArgumentObject).GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
            for (int idx = 0; idx < arr.Length; idx++)
                foo[idx].SetValue(this, arr[idx]);
        }

        public ArgumentObject(argumentList args) 
        {
            foo = typeof(argumentList).GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
            arguments = args;

        }

        public static ArgumentObject Parse(string[] args)
        {
            argumentList argList = new argumentList();

            for (int idx = 0; idx < args.Length; idx++)
            {
                string str = args[idx];
                switch (str)
                {
                    case "ani":
                        if (argList.mn != string.Empty)
                            throw new Exception("ani/nvl selector has already been set in this parameter list.");
                        argList.mn = "ani";
                        break;
                    case "nvl":
                        if (argList.mn != string.Empty)
                            throw new Exception("ani/nvl selector has already been set in this parameter list.");
                        argList.mn = "nvl";
                        break;
                    case "ints":
                        if (argList.mn != string.Empty)
                            throw new Exception("MN Flag already set.");
                        argList.mn = "ints";
                        break;
                    case "-add":

                        break;
                    case "-aS":
                        if (argList.mn != string.Empty)
                            throw new Exception("ani/nvl selector has already been set in this parameter list.");
                        argList.mn = "ani";
                        argList.aS = true;
                        break;
                    case "-nS":
                        if (argList.mn != string.Empty)
                            throw new Exception("ani/nvl selector has already been set in this parameter list.");
                        argList.mn = "nvl";
                        argList.nS = true;
                        break;
                    case "-d":
                        argList.d = true;
                        break;
                    case "-mt":
                        argList.mt = true;
                        break;
                    case "-cc":
                        argList.cc = true;
                        break;
                    case "-c":
                        argList.c = true;
                        break;
                    case "-h":
                        argList.h = true;
                        break;
                    case "-s":
                        argList.s = true;
                        break;
                    case "-gS":
                        argList.gS = true;
                        break;
                    case "-hS":
                        argList.hS = true;
                        break;
                    case "-tS":
                        throw new NotImplementedException("Twist.Moe can not search at the moment");
                        argList.tS = true;
                        break;
                    case "-e":
                        argList.e = true;
                        break;
                    case "-help":
                        argList.help = true;
                        break;
                    case "-l":
                        argList.l = true;
                        idx++;
                        string k = args[idx];
                        if (k[0] == '\"')
                            argList.export = SearchForPath(args, idx);
                        else
                            argList.export = k;

                        if (argList.export[0] == '.' && (argList.export[1] == '/' || argList.export[1] == '\\'))
                            argList.export = new string(argList.export.Skip(2).ToArray()).InsertAtFront(Directory.GetCurrentDirectory());
                            break;
                    case "-range":
                        idx++;
                        string[] range = args[idx].Split('-');
                        argList.vRange = true;
                        argList.VideoRange = new int[2] { int.Parse(range[0]), int.Parse(range[1])};
                        break;
                    default:
                        argList.term += argList.term.Length > 0 ? $" {str}" : str;
                        break;
                }
            }

            if (argList.term[0] == '.' && (argList.term[1] == '/' || argList.term[1] == '\\'))
                argList.term = new string(argList.term.Skip(2).ToArray()).InsertAtFront(Directory.GetCurrentDirectory());

            return new ArgumentObject(argList);
        }

        private static string SearchForPath(string[] args, int beginning)
        {
            StringBuilder sb = new StringBuilder();
            for(int idx = beginning; idx < args.Length; idx++)
            {
                if (args[idx][args[idx].Length - 1] == '\"')
                {
                    sb.Append(args[idx]);
                    return sb.ToString();
                }
                else
                    sb.Append(args[idx] + " ");
            }
            throw new Exception("You didn't add an end quote to your path!");
        }

        //For any legacy code that remains.
        public object this[int i] {
            get
            {
                return foo[i].GetValue(arguments);
            }
            set
            {
                foo[i].SetValue(arguments, value);
            }
        }
    }
}
