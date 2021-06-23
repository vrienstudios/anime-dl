﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ADLCore.Novels.Models
{
    //Provides general information about books and manga.
    public class MetaData
    {
        public string name, author, rating, genre, type;
        public string url;
        public string description;
        public Byte[] cover;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            FieldInfo[] fields = typeof(MetaData).GetFields();
            foreach(FieldInfo field in fields)
            {
                if (field.Name == "cover")
                    continue;
                string x = field.GetValue(this).ToString();
                sb.AppendLine(String.IsNullOrEmpty(x) ? string.Empty : x);
            }
            return sb.ToString();
        }

        public static MetaData GetMeta(string[] data)
        {
            MetaData md = new MetaData();
            FieldInfo[] fields = typeof(MetaData).GetFields();
            for (int idx = 0; idx < data.Length; idx++)
            {
                if (fields[idx].Name == "cover")
                    continue;
                fields[idx].SetValue(md, data[idx]);
            }
            return md;
        }
    }
}
