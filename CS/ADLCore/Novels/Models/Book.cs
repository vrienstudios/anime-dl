using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ADLCore.Alert;
using ADLCore.Ext;
using ADLCore.Novels.Downloaders;
using ADLCore.Novels.Models;
using KobeiD.Downloaders;

namespace ADLCore.Novels.Models
{
    public class Book
    {
        public MetaData metaData;
        public Chapter[] chapters;
        public string fileLocation;
        public DateTime lastUpdated;
        public Uri url;
        private Site site;
        public string chapterDir;

        public delegate void threadFinished();
        public event threadFinished onThreadFinish;
        public delegate void downloadFinished();
        public event downloadFinished onDownloadFinish;

        private int finishedThreads;
        private int limiter;
        private bool finished;
        Stopwatch sw = new Stopwatch();
        List<Thread> threads = new List<Thread>();
        private int ti;
        Action<int, string> statusUpdate;

        public bool dwnldFinished = false;
        public string root;
        public Book()
        {
            onThreadFinish += Book_onThreadFinish;
        }

        private void Book_onThreadFinish()
        {
            finishedThreads++;
            if (finishedThreads >= limiter)
            {
                sw.Stop();
                statusUpdate(ti, $"Done!, Download of {metaData.name} finished in {sw.Elapsed}");
                dwnldFinished = true;
                onDownloadFinish?.Invoke();
            }
        }

        public Book(string uri, bool parseFromWeb, int taski, Action<int, string> act, string loc = null)
        {
            statusUpdate = act;
            ti = taski;

            if (loc != null)
                root = loc;
            else
                root = Environment.CurrentDirectory;

            if (parseFromWeb)
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
                this.chapterDir = Path.GetFullPath(Path.Combine(root != null ? root : Environment.CurrentDirectory, "Downloaded", metaData.name, "Chapters"));
            }
            else
            {
                onThreadFinish += Book_onThreadFinish;
                metaData = new MetaData();
                LoadFromADL(uri);
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
                Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "Downloaded", metaData.name, "Chapters"));
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
            statusUpdate(ti, $"{metaData?.name} Creating Novel Object");
            DownloaderBase dbase = null;
            switch (site)
            {
                case Site.AsianHobbyist:
                    dbase = new AsianHobbyist(url, ti, statusUpdate);
                    break;
                case Site.wuxiaWorldA:
                    dbase = new dWuxiaWorld(url, ti, statusUpdate);
                    break;
                case Site.wuxiaWorldB:
                    dbase = new cWuxiaWorld(url, ti, statusUpdate);
                    break;
                case Site.ScribbleHub:
                    dbase = new cScribbleHub(url, ti, statusUpdate);
                    break;
                case Site.NovelFull:
                    dbase = new cNovelFull(url, ti, statusUpdate);
                    break;
                case Site.NovelHall:
                    dbase = new NovelHall(url, ti, statusUpdate);
                    break;
                case Site.Error:
                    ADLUpdates.CallUpdate("Error: This site doesn't seem to be supported.");
                    return false;
                default:
                    ADLUpdates.CallUpdate("Unknown error");
                    return false;
            }
            statusUpdate(ti, $"{metaData?.name} Getting MetaData");
            metaData = dbase.GetMetaData();
            statusUpdate(ti, $"{metaData?.name} Getting Chapter links");
            chapters = dbase.GetChapterLinks();
            fileLocation = $"{root}{Path.DirectorySeparatorChar}{metaData.name}";
            ADLUpdates.CallUpdate($"Downloading Chapters for {metaData.name}");
            return true;
        }

        private void sU(int a, string b)
        {
            b = $"{metaData.name} {b}";
            statusUpdate(a, b);
        }

        public void DownloadChapters()
            => chapters = Chapter.BatchChapterGet(chapters, chapterDir, site, ti, sU);

        public void DownloadChapters(bool multithreaded)
        {
            if (!multithreaded)
            {
                DownloadChapters();
                dwnldFinished = true;
                onDownloadFinish?.Invoke();
                return;
            }

            int[] a = chapters.Length.GCFS();
            this.limiter = a[0];
            int limiter = 0;
            Chapter[][] chaps = new Chapter[a[0]][];
            for (int i = a[0] - 1; i > -1; i--)
            {
                chaps[i] = chapters.Skip(limiter).Take(a[1]).ToArray();
                limiter += a[1];
            }

            for (int idx = 0; idx < a[0]; idx++)
            {
                Chapter[] chpa = chaps[idx];
                int i = idx;
                Thread ab = new Thread(() => { chpa = Chapter.BatchChapterGet(chpa, chapterDir, site, ti, sU); onThreadFinish?.Invoke(); }) { Name = i.ToString() };
                ab.Start();
                threads.Add(ab);
            }
        }

        public void ExportToADL()
        {
            Directory.CreateDirectory(chapterDir);
            TextWriter tw = new StreamWriter(new FileStream($"{root}{Path.DirectorySeparatorChar}Downloaded{Path.DirectorySeparatorChar}{metaData.name}{Path.DirectorySeparatorChar}main.adl", FileMode.OpenOrCreate));
            foreach (FieldInfo pie in typeof(MetaData).GetFields())
            {
                if (pie.Name != "cover")
                    tw.WriteLine($"{pie.Name}|{pie.GetValue(metaData)}");
                else
                    using (BinaryWriter bw = new BinaryWriter(new FileStream($"{root}{Path.DirectorySeparatorChar}Downloaded{Path.DirectorySeparatorChar}{metaData.name}{Path.DirectorySeparatorChar}cover.jpeg", FileMode.OpenOrCreate)))
                        bw.Write(metaData.cover, 0, metaData.cover.Length);
            }
            tw.Close();
        }

        public void LoadFromADL(string pathToDir)
        {
            string[] adl = File.ReadAllLines(pathToDir + Path.DirectorySeparatorChar + "main.adl");
            FieldInfo[] fi = typeof(MetaData).GetFields();
            foreach (string str in adl)
                if (str != "")
                    fi.First(x => x.Name == str.Split('|')[0]).SetValue(metaData, str.Split('|')[1]);
            metaData.cover = File.ReadAllBytes(pathToDir + Path.DirectorySeparatorChar + "cover.jpeg");

            adl = Directory.GetFiles(pathToDir + $"{Path.DirectorySeparatorChar}Chapters");

            List<Chapter> chaps = new List<Chapter>();

            foreach (string str in adl) {
                Chapter chp = new Chapter();
                chp.name = str.GetFileName().Replace('_', ' ');
                if (str.GetImageExtension() != ImageExtensions.Error)
                    chp.image = File.ReadAllBytes(str);
                else
                    chp.text = File.ReadAllText(str);

                chaps.Add(chp);
            }

            chapters = chaps.ToArray();
            chaps.Clear();

            return;
        }

        public void ExportToEPUB(string location)
        {
            statusUpdate(ti, $"{metaData?.name} Exporting to EPUB");
            Epub e = new Epub(metaData.name, metaData.author, new Image() { bytes = metaData.cover }, new Uri(metaData.url));
            foreach (Chapter chp in chapters)
            {
                statusUpdate(ti, $"{metaData?.name} Generating page for {chp.name.Replace('_', ' ')}");
                e.AddPage(Page.AutoGenerate(chp.image == null ? chp.text : null, chp.name.Replace('_', ' '), chp.image != null ? new Image[] { Image.GenerateImageFromByte(chp.image, "IMG_" + chp.name)  } : null));
            }
            e.CreateEpub();
            statusUpdate(ti, $"{metaData?.name} EPUB Created!");
            e.ExportToEpub(location);
        }

        public void ExportToZipLocation(string location, bool deleteSource = false)
        {
            ZipFile.CreateFromDirectory(Path.Join(location == null ? root : location, "Epubs", metaData.name), Path.Join(root, "Epubs", this.metaData.name + ".epub"));
            if(deleteSource)
                Directory.Delete(Path.Join(root, "Epubs", this.metaData.name), true);
            statusUpdate(ti, $"{metaData?.name} Finished Exporting to .EPUB File");
        }

        public void ParseBookFromFile()
        {

        }

        public void UpdateBook()
        {

        }

        public void MergeChapters()
        {

        }
    }
}
