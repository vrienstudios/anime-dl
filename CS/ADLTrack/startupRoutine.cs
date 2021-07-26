using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ADLTrack
{
    public class startupRoutine
    {
        public string workingDir;
        public string[] detectedADLs;
        private List<string> directories;

        public startupRoutine(string adlworkingDir)
        {
            workingDir = adlworkingDir;
            DirSearch();
            detectedADLs = AdlSort();
        }

        private string[] AdlSort()
        {
            List<string> adls = new List<string>();
            for(int idx = 0; idx < directories.Count; idx++)
            {
                string[] files = Directory.GetFiles(directories[idx]);
                for (int idy = 0; idy < files.Length; idx++)
                    if (files[idy].Contains(".adl"))
                        adls.Add(files[idy]);
            }
            return adls.ToArray();
        }

        private string[] GetAllDirs(String startingDir, string[] dirsFound) =>  startingDir != null ? Directory.GetDirectories(startingDir).Length > 0 ? GetAllDirs(null, dirsFound.Concat(Directory.GetDirectories(startingDir)).ToArray()) : null : eDir(dirsFound);

        private string[] eDir(string[] dirs)
        {
            string[] found = { };
            directories.AddRange(dirs);
            for (int idx = 0; idx < dirs.Length; idx++)
                GetAllDirs(dirs[idx], new string[] { });

            return null;
        }


        private void DirSearch()
        {
            directories = new List<string>();
            string[] b = GetAllDirs(workingDir, new string[]{ });
            string[] a = directories.ToArray();
            //string[] a = GetAllDirs(workingDir, null);
            foreach (string ab in a)
                Console.WriteLine(ab);
        }
    }
}
