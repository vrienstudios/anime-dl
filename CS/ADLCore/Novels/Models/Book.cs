using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using ADLCore.Alert;
using ADLCore.Epub;
using ADLCore.Ext;
using ADLCore.Novels.Downloaders;
using ADLCore.Novels.Models;
using ADLCore.SiteFolder;

namespace ADLCore.Novels.Models
{
    //TODO: Add support for vRange flag.
    public class Book //Model for Book Objects
    {
        public MetaData metaData { get; set; }
        public Chapter[] chapters { get; set; }
        public string fileLocation { get; set; }
        public DateTime lastUpdated { get; set; }
        public Uri url { get; set; }
        private SiteBase site { get; set; }
        public string chapterDir { get; set; }

        public delegate void threadFinished(int i);
        public event threadFinished onThreadFinish;
        public delegate void downloadFinished();
        public event downloadFinished onDownloadFinish;

        [JsonIgnore]
        private int finishedThreads;
        [JsonIgnore]
        private int limiter;
        [JsonIgnore]
        private bool finished;
        Stopwatch sw = new Stopwatch();
        [JsonIgnore]
        List<Thread> threads = new List<Thread>();
        [JsonIgnore]
        public int ti;
        [JsonIgnore]
        public Action<int, string> statusUpdate;

        [JsonIgnore]
        public bool dwnldFinished = false;
        [JsonIgnore]
        public string root;

        Stream bookStream;

        [JsonIgnore]
        public ZipArchive zapive { get; set; }

        [JsonIgnore]
        public bool pauser = false;
        [JsonIgnore]
        public object locker = new object();
        [JsonIgnore]
        public static Random rng = new Random();
        [JsonIgnore]
        public bool sortedTrustFactor;

        [JsonIgnore]
        public DownloaderBase dBase;

        public Book()
        {
            onThreadFinish += Book_onThreadFinish;
            onDownloadFinish += Book_onDownloadFinish;
        }

        private void Book_onDownloadFinish()
        {
            zapive.Dispose();
            GC.Collect();
            bookStream.Flush();
            zapive.Dispose();

            ThreadManage(false);
        }

        private void Book_onThreadFinish(int i)
        {
            ZipArchiveFinish(i);
            finishedThreads++;
            if (finishedThreads >= limiter)
            {
                sw.Stop();
                statusUpdate(ti, $"Done!, Download of {metaData.name} finished in {sw.Elapsed}");
                dwnldFinished = true;
                onDownloadFinish?.Invoke();
                return;
            }
            UnlockThread(i);
        }

        private void ZipArchiveFinish(int i)
        {
            ZipArchiveEntry[] archive = entries[i];
            while (exo)
                Thread.Sleep(rng.Next(100, 700));
            foreach (ZipArchiveEntry entry in archive)
            {
                exo = true;
                using (StreamWriter sw = new StreamWriter(zapive.CreateEntry(entry.FullName).Open()))
                using (StreamReader sr = new StreamReader(entry.Open()))
                    sw.Write(sr.ReadToEnd());
                exo = false;
            }
            UpdateStream();
        }

        public void InitializeZipper(string loc, bool dc = false)
        {
            bookStream = new FileStream(loc, dc ? FileMode.Open : FileMode.Create);
            zapive = new ZipArchive(bookStream, ZipArchiveMode.Update, true);
        }

        public void InitializeZipper(Stream stream) { 
            zapive = new ZipArchive(stream, ZipArchiveMode.Update, true);
        }

        public void ThreadManage(bool lockresume)
        {
            if (lockresume)
                pauser = true;
            else
            {
                pauser = false;
                lock (locker)
                    Monitor.PulseAll(locker);
            }
        }

        public void awaitThreadUnlock()
        {
            lock (locker)
                Monitor.Wait(locker);
        }
        public void awaitThreadUnlock(int i)
        {
            lock (threadLocks[i])
                Monitor.Wait(threadLocks[i]);
        }
        public void UnlockThread(int i)
        {
            lock (threadLocks[i])
                Monitor.PulseAll(threadLocks[i]);
        }

        bool exo = false;
        public void UpdateStream()
        {
            while (exo)
                Thread.Sleep(rng.Next(100, 700));
            exo = true;
            bookStream.Flush();
            zapive.Dispose();
            zapive = new ZipArchive(bookStream, ZipArchiveMode.Update, true);
            exo = false;
        }

        public Book(string uri, bool parseFromWeb, int taski, Action<int, string> act, string loc = null, bool loadChapters = true)
        {
            statusUpdate = act;
            ti = taski;

            if (loc != null)
                root = loc;
            else
                root = Environment.CurrentDirectory;

            if (parseFromWeb)
            {
                if (uri.IsValidUri())
                {
                    onThreadFinish += Book_onThreadFinish;
                    url = new Uri(uri);
                    this.site = uri.SiteFromString();
                    if (parseFromWeb)
                        if (!ParseBookFromWeb(uri))
                        {
                            Console.WriteLine("Can not continue, press enter to exit...");
                            Console.ReadLine();
                            Environment.Exit(-1);
                        }
                    this.chapterDir = "Chapters/";
                }
                else
                {
                    onThreadFinish += Book_onThreadFinish;
                    url = new Uri(uri);
                    this.site = uri.SiteFromString();
                    LoadFromADL(uri, loadChapters);
                    if (parseFromWeb)
                        if (!ParseBookFromWeb(uri))
                        {
                            Console.WriteLine("Can not continue, press enter to exit...");
                            Console.ReadLine();
                            Environment.Exit(-1);
                        }
                    this.chapterDir = "Chapters/";
                }
            }
            else
            {
                onThreadFinish += Book_onThreadFinish;
                metaData = new MetaData();
                LoadFromADL(uri, false, loadChapters);
                if(loadChapters)
                    for (int id = 0; id < chapters.Length; id++)
                        for (int idx = 0; idx < chapters.Length; idx++)
                        {

                            string chr = chapters[idx].name;
                            if (chr.ToArray().FirstLIntegralCount() == 0)
                                chr += 0;
                            string chra = chapters[id].name;
                            if (chra.ToArray().FirstLIntegralCount() == 0)
                                chra += 0;

                            if (chr.ToCharArray().FirstLIntegralCount() > chra.ToCharArray().FirstLIntegralCount())
                            {
                                Chapter a = chapters[id];
                                chapters[id] = chapters[idx];
                                chapters[idx] = a;
                            }
                        }
            }
        }

        public Book(string path)
        {
            if (path.IsValidUri())
            {
                onThreadFinish += Book_onThreadFinish;
                url = new Uri(path);
                this.site = path.SiteFromString();
                if (!ParseBookFromWeb(path))
                    throw new Exception("Unknown Error: e: bp2 | ParseFromWeb returned false");
            }
            else
            {
                onThreadFinish += Book_onThreadFinish;
                metaData = new MetaData();
                LoadFromADL(path);
                for (int id = 0; id < chapters.Length; id++)
                    for (int idx = 0; idx < chapters.Length; idx++)
                    {

                        string chr = chapters[idx].name;
                        if (chr.ToArray().FirstLIntegralCount() == 0)
                            chr += 0;
                        string chra = chapters[id].name;
                        if (chra.ToArray().FirstLIntegralCount() == 0)
                            chra += 0;

                        if (chr.ToCharArray().FirstLIntegralCount() > chra.ToCharArray().FirstLIntegralCount())
                        {
                            Chapter a = chapters[id];
                            chapters[id] = chapters[idx];
                            chapters[idx] = a;
                        }
                    }
            }
        }

        public bool ParseBookFromWeb(string url)
        {
            statusUpdate(ti, $"{metaData?.name} Getting MetaData");
            metaData = dBase.GetMetaData();
            statusUpdate(ti, $"{metaData?.name} Getting Chapter links");
            chapters = dBase.GetChapterLinks();
            fileLocation = $"{chapterDir}/{metaData.name}";
            ADLUpdates.CallUpdate($"Downloading Chapters for {metaData.name}");
            return true;
        }

        private void sU(int a, string b)
        {
            b = $"{metaData.name} {b}";
            statusUpdate(a, b);
        }

        public void DownloadChapters()
            => chapters = Chapter.BatchChapterGet(chapters, chapterDir, this, zapive, ti, sU);

        [JsonIgnore]
        ZipArchiveEntry[][] entries { get; set; }

        [JsonIgnore]
        object[] threadLocks;
        public void DownloadChapters(bool multithreaded)
        {
            if (!multithreaded)
            {
                DownloadChapters();
                dwnldFinished = true;
                onDownloadFinish?.Invoke();
                return;
            }
            ThreadManage(true);

            int[] a = chapters.Length.GCFS();
            int dlm = 0;
            if(a[0] == -1)
            {
                a = new int[] { a[1], a[2] };
                dlm = (chapters.Length) - (a[0] * a[1]);
            }
            int thrdCount = a[0] + (dlm > 0 ? 1 : 0);
            entries = new ZipArchiveEntry[a[1]][];
            this.limiter = thrdCount;
            int limiter = 0;
            Chapter[][] chaps = new Chapter[a[0] + (dlm == 0 ? 0 : 1)][];
            for (int i = a[0] - 1; i > -1; i--)
            {
                chaps[i] = chapters.Skip(limiter).Take(a[1]).ToArray();
                limiter += a[1];
            }
            if (dlm > 0)
                chaps[chaps.Length - 1] = chapters.Skip(limiter).Take(dlm).ToArray();
            threadLocks = new object[chaps.Length];
            for (int idx = 0; idx < chaps.Length; idx++)
                threadLocks[idx] = new object();
            for (int idx = 0; idx < thrdCount; idx++)
            {
                Chapter[] chpa = chaps[idx];
                int i = idx;
                if (chpa == null)
                    Thread.Sleep(199);
                Thread c = new Thread(() => { awaitThreadUnlock(i - 1); });
                if (i != 0)
                    c.Start();
                Thread ab = new Thread(() => { entries[i] = (Chapter.BatchChapterGetMT(chpa, this, chapterDir, ti, sU)); if(i != 0) c.Join(); onThreadFinish?.Invoke(i); }) { Name = i.ToString() };
                ab.Start();
                threads.Add(ab);
            }
        }

        public void ExportToADL()
        {
            if (File.Exists(root))
            {
                LoadFromADL(root, false); // Changed from True to False.
                zapive.GetEntry("main.adl").Delete();
                zapive.GetEntry("cover.jpeg").Delete();
                zapive.GetEntry("auxi.cmd").Delete();
            }
            else
                InitializeZipper(root);

            TextWriter tw = new StreamWriter(zapive.CreateEntry("main.adl").Open());
            foreach (FieldInfo pie in typeof(MetaData).GetFields())
            {
                if (pie.Name != "cover")
                    tw.WriteLine($"{pie.Name}|{pie.GetValue(metaData)}");
                else
                    using (BinaryWriter bw = new BinaryWriter(zapive.CreateEntry("cover.jpeg").Open()))
                        bw.Write(metaData.cover, 0, metaData.cover.Length);
            }
            tw.Close();
            using (tw = new StreamWriter(zapive.CreateEntry("auxi.cmd").Open()))
                tw.Write($"nvl -d -e {this.url}\n{this.url}\n{DateTime.Now}");

            UpdateStream();
        }
        
        //Legacy for novels downloaded to directories.
        public void LoadFromDIR(string pathToDir, bool merge = false, bool parseChapters = true)
        {
            StreamReader sr = new StreamReader(new FileStream($"{pathToDir}{Path.DirectorySeparatorChar}main.adl", FileMode.Open));
            string[] adl = sr.ReadToEnd().Split(Environment.NewLine);

            FieldInfo[] fi = typeof(MetaData).GetFields();
            foreach (string str in adl)
                if (str != "")
                    fi.First(x => x.Name == str.Split('|')[0]).SetValue(metaData, str.Split('|')[1]);

            sr.Close();
            sr = new StreamReader(new FileStream($"{pathToDir}{Path.DirectorySeparatorChar}cover.jpg", FileMode.Open));
            MemoryStream ss = new MemoryStream();
            sr.BaseStream.CopyTo(ss);
            metaData.cover = ss.ToArray();
            sr.Close();
            ss.Dispose();

            adl = Directory.GetFiles($"{pathToDir}{Path.DirectorySeparatorChar}Chapters{Path.DirectorySeparatorChar}");
            List<Chapter> chaps = new List<Chapter>();
            if (parseChapters)
            {
                foreach (string str in adl)
                {
                    Chapter chp = new Chapter();
                    if (str == null || str == string.Empty)
                        continue;
                    chp.name = str.Replace('_', ' ').Replace(".txt", string.Empty);

                    if (str.GetImageExtension() != ImageExtensions.Error)
                        chp.image = File.ReadAllBytes($"{pathToDir}{Path.DirectorySeparatorChar}Chapters{Path.DirectorySeparatorChar}{str}");
                    else
                        chp.text = File.ReadAllText($"{pathToDir}{Path.DirectorySeparatorChar}Chapters{Path.DirectorySeparatorChar}{str}");

                    chaps.Add(chp);
                }
                if (!merge)
                    chapters = chaps.ToArray();
                else
                    for (int idx = 0; idx < chaps.Count; idx++)
                        chapters[idx] = chaps[idx];
            }
            else
                chapters = new Chapter[adl.Length];

            chaps.Clear();

            return;
        }


        public string[] ADLChapterList;

        public void LoadFromADL(string pathToDir, bool merge = false, bool parseChapters = true)
        {
            InitializeZipper(pathToDir, true);
            StreamReader sr = new StreamReader(zapive.GetEntry("main.adl").Open());
            string[] adl = sr.ReadToEnd().Split(Environment.NewLine);

            FieldInfo[] fi = typeof(MetaData).GetFields();
            foreach (string str in adl)
                if (str != "")
                    fi.First(x => x.Name == str.Split('|')[0]).SetValue(metaData, str.Split('|')[1]);

            sr.Close();
            sr = new StreamReader(zapive.GetEntry("cover.jpeg").Open());
            MemoryStream ss = new MemoryStream();
            sr.BaseStream.CopyTo(ss);
            metaData.cover = ss.ToArray();
            sr.Close();
            ss.Dispose();

            adl = zapive.GetEntriesUnderDirectoryToStandardString("Chapters/");
            List<Chapter> chaps = new List<Chapter>();
            if (parseChapters)
            {
                foreach (string str in adl)
                {
                    if (str == null || str == string.Empty)
                        continue;
                    Chapter chp = new Chapter();
                    chp.name = str.Replace('_', ' ').Replace(".txt", string.Empty);

                    if (str.GetImageExtension() != ImageExtensions.Error)
                        chp.image = zapive.GetEntry("Chapters/" + str).GetAllBytes();
                    else
                        chp.text = zapive.GetEntry("Chapters/" + str).GetString();

                    chaps.Add(chp);
                }
                if (!merge)
                    chapters = chaps.ToArray();
                else
                {
                    chapters = new Chapter[chaps.Count];
                    for (int idx = 0; idx < chaps.Count; idx++)
                        chapters[idx] = chaps[idx];
                }
            }
            else
                chapters = new Chapter[adl.Length];

            chaps.Clear();

            return;
        }

        //CALL THIS FIRST (VHLD)
        public void LoadMetaDataFromADL(string pathToDir)
        {
            InitializeZipper(pathToDir, true);
            StreamReader sr = new StreamReader(zapive.GetEntry("main.adl").Open());
            string[] adl = sr.ReadToEnd().Split(Environment.NewLine);

            FieldInfo[] fi = typeof(MetaData).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            metaData = new MetaData();
            foreach (string str in adl)
                if (str != "")
                    fi.First(x => x.Name == str.Split('|')[0]).SetValue(metaData, str.Split('|')[1]);

            sr.Close();
            sr = new StreamReader(zapive.GetEntry("cover.jpeg").Open());
            MemoryStream ss = new MemoryStream();
            sr.BaseStream.CopyTo(ss);
            metaData.cover = ss.ToArray();
            sr.Close();
            ss.Dispose();
        }

        public void LoadChapterListFromADL(int[] range)
        {
            ADLChapterList = zapive.GetEntriesUnderDirectoryToStandardString("Chapters/");
            List<Chapter> chaps = new List<Chapter>();
            for (int idx = range[0]; idx < range[1]; idx++)
            {
                if (ADLChapterList[idx] == null || ADLChapterList[idx] == string.Empty)
                    continue;
                Chapter chp = new Chapter();
                chp.name = ADLChapterList[idx].Replace('_', ' ').Replace(".txt", string.Empty);
                chaps.Add(chp);
            }
            chapters = chaps.ToArray();
        }

        public void LoadChapterListFromADL() // DEBUG
        {
            ADLChapterList = zapive.GetEntriesUnderDirectoryToStandardString("Chapters/");
            List<Chapter> chaps = new List<Chapter>();
            foreach (string str in ADLChapterList)
            {
                if (str == null || str == string.Empty)
                    continue;
                Chapter chp = new Chapter();
                chp.name = str.Replace('_', ' ').Replace(".txt", string.Empty);
                chaps.Add(chp);
            }
            chapters = chaps.ToArray();
        }

        public static Book getBook_server(string path)
        {
            Book bk = new Book();
            bk.LoadMetaDataFromADL(path);
            bk.LoadChapterListFromADL(); // DEBUG Line
            return bk;
        }

        public void ExportToEPUB(string location)
        {
            //SORT
            if (sortedTrustFactor)
            {
                statusUpdate(ti, "Trust Lost, Sorting Chapters.");
                for (int id = 0; id < chapters.Length; id++)
                    for (int idx = 0; idx < chapters.Length; idx++)
                    {
                        if (chapters[idx].chapterNum > chapters[id].chapterNum)
                        {
                            Chapter a = chapters[id];
                            chapters[id] = chapters[idx];
                            chapters[idx] = a;
                        }
                    }
            }

            statusUpdate(ti, $"{metaData?.name} Exporting to EPUB");
            Epub.Epub e = new Epub.Epub(metaData.name, metaData.author, new Image() { bytes = metaData.cover }, new Uri(metaData.url));
            foreach (Chapter chp in chapters)
            {
                statusUpdate(ti, $"{metaData?.name} Generating page for {chp.name.Replace('_', ' ')}");
                e.AddPage(Page.AutoGenerate(chp.image == null ? chp.text : null, chp.name.Replace('_', ' '), chp.image != null ? new Image[] { Image.GenerateImageFromByte(chp.image, "IMG_" + chp.name)  } : null));
            }
            e.CreateEpub(new OPFMetaData(this.metaData.name, this.metaData.author, "Chay#3670", "null", DateTime.Now.ToString()));
            statusUpdate(ti, $"{metaData?.name} EPUB Created!");
            e.ExportToEpub(location);
        }

        [Obsolete] // DO NOT USE
        public void ExportToZipLocation(string location, bool deleteSource = false)
        {
            ZipFile.CreateFromDirectory(Path.Join(location == null ? root : location, "Epubs", metaData.name), Path.Join(root, "Epubs", this.metaData.name + ".epub"));
            if(deleteSource)
                Directory.Delete(Path.Join(root, "Epubs", this.metaData.name), true);
            statusUpdate(ti, $"{metaData?.name} Finished Exporting to .EPUB File");
        }
    }
}
