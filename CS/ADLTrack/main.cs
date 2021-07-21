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
            startupRoutine srr = new startupRoutine(@"E:\Programming\vidstreamdownloader\CS\anime-dl");
            //Test code;
            //WindowsStartup(new Objects.StartupParameters() { adlLibraryFolder = Environment.CurrentDirectory, trackCurrentEpisodes = true});
            if (args.Length <= 0)
                return;

            if(args[0] == "trk")
            {
                trackingRoutine tr = new trackingRoutine();
                
            }
            else
            {
                startupRoutine sr = new startupRoutine(args[1]);
            }
        }

        public static void InitNewStartupRoutine(Objects.StartupParameters startupParams)
        {
            if (startupParams.isWindows)
                WindowsStartup(startupParams);
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

                    if (startupParams.trackCurrentEpisodes)
                    {
                        TaskDefinition trackdefine = taskService.NewTask();
                        trackdefine.RegistrationInfo.Author = "ADLCORE";
                        trackdefine.RegistrationInfo.Description = "ADL Core tracker task.";
                        trackdefine.Triggers.Add(new BootTrigger { Enabled = true, });
                        trackdefine.Actions.Add(new ExecAction(Environment.CurrentDirectory + "\\adltrack.exe", $"trk {startupParams.adlLibraryFolder}", null));
                        taskService.RootFolder.RegisterTaskDefinition(@"ADLTrackerTrigger", trackdefine);
                    }

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

        private static void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine("Error setting up tracker on windows:");
            Console.WriteLine(e.Data);
        }
    }
}
