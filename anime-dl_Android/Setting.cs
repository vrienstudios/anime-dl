using Android.App;
using Android.Widget;
using System.Collections.Generic;

namespace anime_dl_Android
{
    public class Setting
    {
        public bool SaveToSD;
    }

    public class CState
    {
        public bool search;
        public bool multithreaded;
        public bool exportEpub;
        public bool continueDownload;
        public bool skipDownload;
        public string term;

        public List<string> args;

        CState(bool a, bool b, bool c, bool d, bool e)
        {
            args = new List<string>();
            if (a)
            {
                search = true;
                args.Add("-s");
            }
            if (b)
            {
                multithreaded = true;
                args.Add("-mt");
            }
            if (c)
            {
                exportEpub = true;
                args.Add("-e");
            }
            if (d)
            {
                continueDownload = true;
                args.Add("-cc");
            }
            if (e)
            {
                skipDownload = true;
                args.Add("-c");
            }
        }

        // Main Activity MUST be loaded.
        public static CState GetCSTate(Activity mn)
        {
            Android.Widget.CheckBox _searchChk = mn.FindViewById<Android.Widget.CheckBox>(Resource.Id.options_search);
           // Android.Widget.CheckBox _MTChk = mn.FindViewById<Android.Widget.CheckBox>(Resource.Id.options_multithread);
            Android.Widget.CheckBox _exportEpub = mn.FindViewById<Android.Widget.CheckBox>(Resource.Id.options_exportEpub);
            Android.Widget.CheckBox _continueDownload = mn.FindViewById<Android.Widget.CheckBox>(Resource.Id.options_continue);
            Android.Widget.CheckBox _skipDownloaded = mn.FindViewById<Android.Widget.CheckBox>(Resource.Id.options_skip);
            Android.Widget.EditText input = mn.FindViewById<EditText>(Resource.Id.editText1);
            return new CState(_searchChk.Checked, false, _exportEpub.Checked, _continueDownload.Checked, _skipDownloaded.Checked) { term = input.Text };
        }
    }
}