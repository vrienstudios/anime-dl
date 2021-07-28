using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ADLTrack
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length <= 0)
                return;

            if(args[0] == "trk")
            {
                trackingRoutine tr = new trackingRoutine();
                throw new Exception("Episode tracking is not supported right now.");
            }
            else if(args[0] == "upd")
            {
                startupRoutine sr = new startupRoutine(args[1]);
                
            }
            else
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    WindowsStartup(new Objects.StartupParameters() { accessibleAnywhere = args[0] == "true", adlLibraryFolder = args[1], isWindows = true, trackCurrentEpisodes = args[2] == "true", updateInterval = int.Parse(args[3])});
            }
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
                    taskdefine.Actions.Add(Environment.CurrentDirectory + "\\adltrack.exe", $"upd {startupParams.adlLibraryFolder}");

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
            catch(Exception x)
            {
                Console.WriteLine("Please run as admin to complete setup.");
                Console.ReadLine();
                Environment.Exit(-1);
            }
        }
    }
}
