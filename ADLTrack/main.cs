using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ADLTrack
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length <= 0)
                return -1;

            Console.Title = "ADLUPDTRCK";
            if (args[0] == "trk")
            {
                throw new Exception("Episode tracking is not supported right now.");
                trackingRoutine tr = new trackingRoutine();
            }
            else if (args[0] == "upd")
            {
                startupRoutine sr = new startupRoutine(args[1]);
                ADLCore.Interfaces.Main mainWork = new ADLCore.Interfaces.Main(sr.detectedADLs, true);
            }

            return 0;
        }

        private static void WindowsStartup(Objects.StartupParameters startupParams)
        {
            try
            {
                using (TaskService taskService = new TaskService())
                {
                    TaskDefinition taskdefine = taskService.NewTask();
                    taskdefine.RegistrationInfo.Author = "ADLCORE";
                    taskdefine.RegistrationInfo.Description = "ADL Core updater task; looks for new anime.";
                    taskdefine.Triggers.AddNew(TaskTriggerType.Idle);
                    taskdefine.Actions.Add(Environment.CurrentDirectory + "\\adltrack.exe",
                        $"upd {startupParams.adlLibraryFolder}");

                    /*if (startupParams.trackCurrentEpisodes)
                    {
                        TaskDefinition trackdefine = taskService.NewTask();
                        trackdefine.RegistrationInfo.Author = "ADLCORE";
                        trackdefine.RegistrationInfo.Description = "ADL Core tracker task.";
                        trackdefine.Triggers.Add(new BootTrigger { Enabled = true, });
                        trackdefine.Actions.Add(new ExecAction(Environment.CurrentDirectory + "\\adltrack.exe", $"trk {startupParams.adlLibraryFolder}", null));
                        taskService.RootFolder.RegisterTaskDefinition(@"ADLTrackerTrigger", trackdefine);
                    }*/

                    taskService.RootFolder.RegisterTaskDefinition("ADLUpdaterTrigger", taskdefine);
                }
            }
            catch (Exception x)
            {
                Console.WriteLine("Please run as admin to complete setup.");
                Console.ReadLine();
                Environment.Exit(-1);
            }
        }
    }
}