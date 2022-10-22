using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EQEmu_Launcher.Manage
{
    internal class World
    {
        static readonly StatusType status = StatusType.World;

        public static void Check()
        {
            Process[] pname = Process.GetProcessesByName("world");
            if (pname.Length > 0)
            {
                StatusLibrary.SetText(status, $"World is running");
                StatusLibrary.SetIsFixNeeded(status, false);
                return;
            }
            StatusLibrary.SetText(status, "World is not running");
            StatusLibrary.SetIsFixNeeded(status, true);
        }

        public static void Start()
        {
            try
            {
                SharedMemory.Start();

                StatusLibrary.SetStatusBar($"Starting world...");
                StatusLibrary.SetIsFixNeeded(StatusType.World, true);
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = $"{Application.StartupPath}\\server\\world.exe",
                        WorkingDirectory = $"{Application.StartupPath}\\server",
                        Arguments = "",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                proc.StartInfo.EnvironmentVariables["PATH"] = UtilityLibrary.EnvironmentPath();
                proc.Start();

                Task.Run(() => {
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        string line = proc.StandardOutput.ReadLine();
                        StatusLibrary.Log($"World: {line}");
                        if (line.Contains("Server(TCP) listener started on port"))
                        {
                            StatusLibrary.SetStatusBar($"World is started");
                            StatusLibrary.SetIsFixNeeded(StatusType.World, false);
                        }
                    }
                    StatusLibrary.Log($"World: exited");
                });
            } catch (Exception e)
            {
                string result = $"failed world start \"server\\world.exe\": {e.Message}";
                StatusLibrary.SetStatusBar(result);
                MessageBox.Show(result, "World Start", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void Stop()
        {
            int worldCount = 0;
            try
            {

                Process[] workers = Process.GetProcessesByName("world");
                StatusLibrary.Log($"Found {workers.Length} world instances");
                foreach (Process worker in workers)
                {
                    StatusLibrary.Log($"Stopping world pid {worker.Id}");
                    worker.Kill();
                    worker.WaitForExit();
                    worker.Dispose();
                    worldCount++;
                }
                if (worldCount == 0) StatusLibrary.SetStatusBar("world not found to stop");
                else StatusLibrary.SetStatusBar($"stopped {worldCount} world instances");
            }
            catch (Exception e)
            {
                string result = $"failed to stop world: {e.Message}";
                StatusLibrary.SetStatusBar("world stop failed");
                MessageBox.Show(result, "World Stop", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
