﻿using LaunchMate.Enums;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LaunchMate.Utilities;

namespace LaunchMate.Models
{
    public class AppAction : ActionBase
    {
        //private string _lnkName;

        //public string LnkName { get => _lnkName; set => SetValue(ref _lnkName, value); }

        public string WorkingDirectory { get; set; }
        public override bool Execute(string groupName, Screen screen = null, Playnite.SDK.Events.OnGameStartingEventArgs args = null)
        {
            ILogger logger = LogManager.GetLogger();
            if (screen == null)
            {
                screen = Screen.PrimaryScreen;
            }
            // If no target, return
            if ((Target ?? "") == string.Empty)
            {
                return false;
            }

            string processedArgs = TargetArgs;
            if (args != null)
            {
                processedArgs = processedArgs.Replace("%GameDir%", args.Game.InstallDirectory);
                processedArgs = processedArgs.Replace("%GameExe%", Path.GetFileName(args.Game.GameActions.First().Path));
            }

            logger.Info($"{groupName} - Launching application \"{Target}\" with arguments \"{processedArgs}\"");
            try
            {
                API.Instance.Notifications.Remove($"{groupName} - Error: {Target}");
                ProcessStartInfo startInfo = new ProcessStartInfo(Target)
                {
                    Arguments = processedArgs,
                    WorkingDirectory = WorkingDirectory
                };
                Process p = Process.Start(startInfo);
                //MoveWindow(p, screen);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"{groupName} - Something went wrong trying to start application at {Target}");
                API.Instance.Notifications.Add($"{groupName} - Error: {Target}", $"An error occurred when LaunchMate tried to launch application {Target} from group {groupName}, see logs for more info", NotificationType.Error);
                return false;
            }
        }

        //public async void MoveWindow(Process p,  Screen screen)
        //{
        //    await Task.Run(() =>
        //    {
        //        System.Threading.Thread.Sleep(2000);
        //        WindowHelper.MoveWindow(p.MainWindowHandle, screen);
        //    });
        //}

        public override void AutoClose(string groupName)
        {
            ILogger logger = LogManager.GetLogger();
            // Use WMI query to find processes with the same executable path as the app launched by this action
            var wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            using (var results = searcher.Get())
            {
                var query = from p in Process.GetProcesses()
                            join mo in results.Cast<ManagementObject>()
                            on p.Id equals (int)(uint)mo["ProcessId"]
                            select new
                            {
                                Process = p,
                                Path = (string)mo["ExecutablePath"],
                                CommandLine = (string)mo["CommandLine"],
                            };
                foreach (var item in query)
                {
                    // Check if the executable path of the process is the launched executable and stop the process
                    if (item.Path != null && item.Path.Contains(Path.GetDirectoryName(Target)))
                    {
                        logger.Info($"{groupName} - Stopping process {item.Process.ProcessName}|{item.Process.Id} associated with {Target}");
                        item.Process.Kill();
                    }
                }
            }
        }

    }
}
