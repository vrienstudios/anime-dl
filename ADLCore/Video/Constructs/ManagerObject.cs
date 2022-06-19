using System;
using System.Collections.Generic;
using System.Linq;
using ADLCore.Alert;

namespace ADLCore.Video.Constructs
{
    // ShuJia - 29/5/22 | Credits
    public class ManagerObject
    {
        public Dictionary<string, Dictionary<string, string>> dict;
        public IEnumerable<string> Segments;
        
        // 1) ID 2) Audio_ID 3) Resolution 4) URI
        public List<Tuple<string, string, string, string>> ResolutionOptions;
        // 1) ID 2) Audio_Name 3) Language 4) URI
        public List<Tuple<string, string, string, string>> AudioOptions;

        public Int16 EncryptionType = 0;

        public ManagerObject(string[] m3uList, int idx = 0, bool pExtra = true)
        {
            Segments = new List<string>();
            ResolutionOptions = new List<Tuple<string, string, string, string>>();
            AudioOptions = new List<Tuple<string, string, string, string>>();

            int dictKeysNum = 0;
            dict = new Dictionary<string, Dictionary<string, string>>();

            var ba = m3uList.ToList();
            ba.RemoveAll(x => x == string.Empty || x == "");
            m3uList = ba.ToArray();
            for (; idx < m3uList.Length - 1; idx++)
            {
                Dictionary<string, string> valsN = new Dictionary<string, string>();
                if (m3uList[idx][0] == '#')
                {
                    string[] v = m3uList[idx].Split(':');
                    string title = v[0].Substring(1);
                    string[] f = new string[] { string.Empty, string.Empty };
                    if(v.Length > 1)
                        f = v[1].Split(',');
                    foreach (string foo in f)
                    {
                        string[] nameVP = foo.Split('=');
                        if (nameVP[0] == "URI" && v.Length > 2)
                            nameVP[1] += v[2];//v.SelectMany(from s in nameVP where nameVP. ));
                        string b = nameVP[0];
                        string c = string.Empty;
                        if (nameVP.Length > 1)
                            c = nameVP[1];
                        if(!valsN.ContainsKey(b))
                            valsN.Add(b, c.Replace("\"", string.Empty));
                    }
                    if(m3uList.Length - 1 > idx)
                        if(m3uList[idx + 1][0] != '#')
                            valsN.Add("URI", m3uList[++idx]);
                    var r_title = title + " " + dictKeysNum++;
                    valsN.Add("PARENT", r_title);
                    dict.Add(r_title, valsN);
                }
            }
            
            try
            {
                foreach (var a in (from key in dict.Keys.Where(x => x.Contains("EXT-X-STREAM-INF"))
                             group dict[key] by int.Parse(dict[key]["RESOLUTION"].Split('x')[0])
                             into grp1
                             select new
                             {
                                 KEY = grp1.Max()["PARENT"], AUDIO = grp1.Max()["AUDIO"],
                                 RESOLUTION = grp1.Max()["RESOLUTION"], URI = grp1.Max()["URI"]
                             }))
                    ResolutionOptions.Add(
                        new Tuple<string, string, string, string>(a.KEY, a.AUDIO, a.RESOLUTION, a.URI));
            }
            catch {ADLCore.Alert.ADLUpdates.CallLogUpdate("No Video Elements Found", ADLUpdates.LogLevel.Low);}

            try
            {
                foreach (var b in (from key in dict.Keys.Where(x => x.Contains("EXT-X-MEDIA"))
                             where dict[key]["TYPE"] == "AUDIO"
                             select new
                             {
                                 KEY = dict[key]["PARENT"], ID = dict[key]["GROUP-ID"], LANGUAGE = dict[key]["LANGUAGE"], URI = dict[key]["URI"].Insert(5, ":")
                             }))
                    AudioOptions.Add(new Tuple<string, string, string, string>(b.KEY, b.ID, b.LANGUAGE, b.URI));
            }
            catch(Exception x) {ADLCore.Alert.ADLUpdates.CallLogUpdate("No Audio Elements Found", ADLUpdates.LogLevel.Low);}

            Segments = from k in dict.Keys.Where(x => x.Contains("EXTINF")) select dict[k]["URI"];
            
            //Temporary function, and it will change, if there are other encryption methods used beside AES.
            if (dict[dict.Keys.First(x => x.Contains("EXT-X-KEY"))]["METHOD"] == "AES-128")
            {
                EncryptionType = 1;
            }
        }
    }
}