using ADLCore.Ext;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ADLCore.SiteFolder;

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
        public bool android;
        public bool grabHome;
        public bool linksOnly;

        public string export = string.Empty;
        public bool vRange;
        public int[] VideoRange;

        public bool addInt;
        public bool resume;
        public bool stream;

        //For API usage in external applications only for now.
        public bool streamOnly;
        
        //Use if you're concerned about data usage; it starts a stream whilst downloading, thus doubling bandwidth usage.
        public bool conserve;
        public bool metaO;
        public bool imgDefault;
        
        public SiteBase SiteSelected;
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{mn} {term}");

            FieldInfo[] foo = typeof(argumentList).GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly |
                                                             BindingFlags.Instance | BindingFlags.NonPublic);
            for (int idx = 0; idx < foo.Length; idx++)
            {
                if (foo.GetType() == typeof(bool))
                    if ((bool) foo[idx].GetValue(this))
                    {
                        sb.Append($" -{foo[idx].Name}");
                        if (foo[idx].Name == "l")
                            sb.Append(" " + export);
                        if (foo[idx].Name == "vRange")
                            sb.Append($" {VideoRange[0]}-{VideoRange[1]}");
                    }
            }

            return sb.ToString();
        }
    }

    //Argument Object for easier management of variables.
    public class ArgumentObject
    {
        public argumentList arguments;
        private FieldInfo[] foo;


        public ArgumentObject(Object[] arr)
        {
            arguments = new argumentList();
            foo = typeof(argumentList).GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly |
                                                 BindingFlags.Instance | BindingFlags.NonPublic);
            for (int idx = 0; idx < arr.Length; idx++)
            {
                if (arr[idx] as string == "-vRange" || arr[idx] as string == "-range")
                {
                    idx++;
                    string[] range = (arr[idx] as string).Split('-');
                    arguments.vRange = true;
                    arguments.VideoRange = new int[2] {int.Parse(range[0]) - 1, int.Parse(range[1])};
                    continue;
                }
                else if (arr[idx] as string == "-l")
                {
                    idx++;
                    string k = arr[idx] as string;
                    if (k[0] == '\"')
                    {
                        string[] arre = new string[arr.Length - idx];
                        for (int d = idx; d < arr.Length; d++)
                        {
                            arre[d] = arr[d] as string;
                        }

                        arguments.export = SearchForPath(arre, 0);
                    }
                    else
                        arguments.export = k;

                    if (arguments.export[0] == '.' && (arguments.export[1] == '/' || arguments.export[1] == '\\'))
                        arguments.export =
                            new string(arguments.export.Skip(2).ToArray()).InsertAtFront(
                                Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar);
                    continue;
                }
                else if (arr[idx] as string == "-site")
                {
                    idx++;
                    arguments.SiteSelected = (arr[idx] as string).SiteFromString(true);
                    idx++;
                    continue;
                }

                string arrs = new string(arr[idx].ToString().Skip(1).ToArray());

                IEnumerable<FieldInfo> e = foo.Where(x => x.Name == arrs && (arr[idx] as string)[0] == '-');
                if (e.Count() <= 0)
                {
                    switch ((string) arr[idx])
                    {
                        case "nvl":
                            arguments.mn = "nvl";
                            continue;
                        case "ani":
                            arguments.mn = "ani";
                            continue;
                        case "man":
                            arguments.mn = "man";
                            continue;
                        default:
                        {
                            if (((arr[idx] as string).Length > 0) && ((arr[idx] as string)[0] == '\"' || (arr[idx] as string)[0] == '\''))
                            {
                                arguments.term = SearchForPath(arr as string[], ref idx);
                                continue;
                            }
                            arguments.term += $"{arr[idx] as string} ";
                            continue;
                        }
                    }
                }

                e.First().SetValue(arguments, true);
            }

            arguments.term = arguments.term.RemoveExtraWhiteSpaces();
        }

        public ArgumentObject(argumentList args)
        {
            foo = typeof(argumentList).GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly |
                                                 BindingFlags.Instance | BindingFlags.NonPublic);
            arguments = args;
        }

        //TODO: cleanup the two functions and combine. Just a quick fix to ignore ref.
        private static string SearchForPath(string[] args, int beginning)
        {
            StringBuilder sb = new StringBuilder();
            for (int idx = beginning; idx < args.Length; idx++)
            {
                if (args[idx][args[idx].Length - 1] == '\"' || args[idx][args[idx].Length - 1] == '\'')
                {
                    sb.Append(args[idx]);
                    beginning = idx;
                    return sb.ToString().Replace("\"", string.Empty).Replace("\'", string.Empty);
                }
                else
                    sb.Append(args[idx] + " ");
            }

            throw new Exception("You didn't add an end quote to your path!");
        }
        private static string SearchForPath(string[] args, ref int beginning)
        {
            StringBuilder sb = new StringBuilder();
            for (int idx = beginning; idx < args.Length; idx++)
            {
                if (args[idx][args[idx].Length - 1] == '\"' || args[idx][args[idx].Length - 1] == '\'')
                {
                    sb.Append(args[idx]);
                    beginning = idx;
                    return sb.ToString().Replace("\"", string.Empty).Replace("\'", string.Empty);
                }
                else
                    sb.Append(args[idx] + " ");
            }

            throw new Exception("You didn't add an end quote to your path!");
        }

        //For any legacy code that remains.
        public object this[int i]
        {
            get { return foo[i].GetValue(arguments); }
            set { foo[i].SetValue(arguments, value); }
        }
    }
}