using System;
using System.IO;
using System.Linq;
using System.Threading;
using ADLCore;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using ADLCore.Video.Extractors;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Text.Method;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using ADLCore.Ext;
using Java.IO;
using Xamarin.Essentials;
using Xamarin;
using Xamarin.Forms;
using ADLCore.Interfaces;

namespace anime_dl_Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        static TextView cons;
        static EditText input;
        static TextView[] tviews;
        static string rot;

        public async System.Threading.Tasks.Task PermissionReaderAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (status == PermissionStatus.Denied)
            {
                status = await Permissions.RequestAsync<Permissions.StorageWrite>();
                if(status == PermissionStatus.Denied)
                {
                    Process.KillProcess(Process.MyPid());
                }
            }

            status = await Permissions.CheckStatusAsync<Permissions.NetworkState>();
            if (status == PermissionStatus.Denied)
            {
                status = await Permissions.RequestAsync<Permissions.NetworkState>();
                if (status == PermissionStatus.Denied)
                {
                    Process.KillProcess(Process.MyPid());
                }
            }
        }


        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            input = FindViewById<EditText>(Resource.Id.editText1);
            input.EditorAction += Et_EditorAction;
            rot = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

            await PermissionReaderAsync();

            if (!Directory.Exists(rot + "/ADL"))
                Directory.CreateDirectory(rot + "/ADL");
            if (!Directory.Exists(rot + "/ADL/Epubs"))
                Directory.CreateDirectory(rot + "/ADL/Epubs");
            if (!Directory.Exists(rot + "/ADL/Anime"))
                Directory.CreateDirectory(rot + "/ADL/Anime");            
            if (!Directory.Exists(rot + "/ADL/HAnime"))
                Directory.CreateDirectory(rot + "/ADL/HAnime");

            rot = Path.Combine(rot, "ADL");

            tviews = new TextView[3];
            tasksRunning = new bool[3];
            concurrentTasks = new arrWrapper<string>(3, new Action<int, string>(UpdateTask));

            tviews[0] = FindViewById<TextView>(Resource.Id.textView2);
            tviews[1] = FindViewById<TextView>(Resource.Id.textView3);
            tviews[2] = FindViewById<TextView>(Resource.Id.textView4);

            cons = FindViewById<TextView>(Resource.Id.textView1);
            cons.MovementMethod = new ScrollingMovementMethod();
            ADLCore.Alert.ADLUpdates.onSystemUpdate += WriteToConsole;
        }

        static int ctasks = 0;
        private static bool[] tasksRunning;
        public static arrWrapper<string> concurrentTasks;

        private void CreateNewCommandInstance(string[] arguments)
        {
            if (ctasks >= 3)
            {
                WriteToConsole("E: Too many tasks running, try again later.");
                return;
            }
            Thread a = new Thread(() => {
                int tid = tasksRunning[ctasks] == true ? tasksRunning.ToList().FindLastIndex(x => x == false) : ctasks;
                concurrentTasks[tid] = "New Task Created!";
                tasksRunning[tid] = true;
                ctasks++;
                //try
                //{
                    ParseArgs(arguments, tid);
                    concurrentTasks[tid] += " Task Finished";
                //}
                //catch(Exception ex)
                //{
                //    concurrentTasks[tid] = ex.Message + " Task Finished";
                //}
                WriteToConsole(null, false);
                tasksRunning[tid] = false;
                ctasks--;
                GC.Collect();
            });
            a.Start();
        }

        private void Et_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId == ImeAction.Done)
            {
                CreateNewCommandInstance(input.Text.Split(' '));
            }
        }

        private static void UpdateTask(int ti, string m)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                tviews[ti].Text = m;
            });
        }

        private void ParseArgs(string[] x, int id)
        {
            ArgumentObject parsedArgs = ArgumentObject.Parse(x);

            if (parsedArgs.arguments.help)
            {
                PrintHelp();
                return;
            }

        }

        private static void WriteToConsole(string a, bool lineBreaks = false, bool refresh = false, bool bypassThreadLock = false)
        {
            if(a != null)
                cons.Append(a);
        }

        private void PrintHelp()
        {
            WriteToConsole(("ani (use at the start of any search to specify anime-dl)\n" +
                " -d (Specifies download)\n" +
                " -mt (Enables multithreading; unavailable on hanime)\n" +
                " -cc (Enables continuos downloading for HAnime series, experimental)\n" +
                " -c  (Enables skipping already downloaded anime; excludes HAnime)\n" +
                " -h (Specifies HAnime search/download explicitly\n" +
                " -s (Specifies search explicitly\n" +
                " -hS (Specifically searches HAnime\n" +
                " -gS (Specifically searches Gogostream)\n" +
                " -tS (Unavailable)\n" +
                "nvl (use at the start of any search to specify novel-dl)\n" +
                " -d (Enables download)\n" +
                " -mt (Enables multithreading; does not work on odd-prime numbers\n" +
                " -e (Specifies to export the novel to epub)\n" +
                "misc:\n" +
                " -help (cancels everything else and prompts help text)\n" +
                "Example usages:\n" +
                " {alias} {parameters}\n" +
                " ani Godly -d -s             | downloads and searches for anime Godly\n" +
                " Godly -d -s -aS             | Does the same as above\n" +
                " nvl www.wuxiaworld.com/Godly -d | Downloads novel Godly"), true);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}