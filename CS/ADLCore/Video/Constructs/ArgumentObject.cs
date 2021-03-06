﻿using ADLCore.Ext;
using System;
using System.Collections;
using System.Collections.Generic;
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
        public bool android;

        public string export = string.Empty;
        public bool vRange;
        public int[] VideoRange;

        public bool addInt;
        public bool resume;
        public bool stream;

        //For API usage in external applications only for now.
        public bool streamOnly;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{mn} {term}");

            FieldInfo[] foo = typeof(argumentList).GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
            for (int idx = 0; idx < foo.Length; idx++)
            {
                if(foo.GetType() == typeof(bool))
                    if ((bool)foo[idx].GetValue(this))
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
            foo = typeof(argumentList).GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
            for (int idx = 0; idx < arr.Length; idx++)
            {
                if (arr[idx] as string == "-vRange" || arr[idx] as string == "-range")
                {
                    idx++;
                    string[] range = (arr[idx] as string).Split('-');
                    arguments.vRange = true;
                    arguments.VideoRange = new int[2] { int.Parse(range[0]) - 1, int.Parse(range[1]) };
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
                        arguments.export = new string(arguments.export.Skip(2).ToArray()).InsertAtFront(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar);
                    continue;
                }
                string arrs = new string(arr[idx].ToString().Skip(1).ToArray());

                IEnumerable<FieldInfo> e = foo.Where(x => x.Name == arrs && (arr[idx] as string)[0] == '-');
                if(e.Count() <= 0)
                {
                    switch((string)arr[idx])
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
                            arguments.term += $"{arr[idx] as string} ";
                            continue;
                    }
                }

                e.First().SetValue(arguments, true);
            }
        }

        public ArgumentObject(argumentList args) 
        {
            foo = typeof(argumentList).GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
            arguments = args;
        }

        [Obsolete]
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
                    case "-skip":
                        argList.c = true;
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
                            argList.export = new string(argList.export.Skip(2).ToArray()).InsertAtFront(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar);
                            break;
                    case "-range":
                        idx++;
                        string[] range = args[idx].Split('-');
                        argList.vRange = true;
                        argList.VideoRange = new int[2] { int.Parse(range[0]) - 1, int.Parse(range[1]) - 1};
                        if (argList.VideoRange[0] < 1 || argList.VideoRange[1] < 1)
                            throw new ArgumentException("x^1 or x^2 can not be less than 1.");
                        break;
                    case "-resume":
                        argList.resume = true;
                        break;                    
                    case "-stream":
                        argList.stream = true;
                        break;
                    default:
                        argList.term += argList.term.Length > 0 ? $" {str}" : str;
                        break;
                }
            }

            if(argList.term.Length > 1)
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
