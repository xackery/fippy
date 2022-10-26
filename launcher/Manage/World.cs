using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EQEmu_Launcher.Manage
{
    internal class World
    {
        static readonly StatusType status = StatusType.World;

        public static bool IsRunning()
        {
            Process[] processes = Process.GetProcessesByName("world");
            return processes.Length > 0;
        }

        public static void Check()
        {
            bool isRunning = IsRunning();
            StatusLibrary.SetText(status, $"World is {(isRunning ? "" : "not ")}running");
            StatusLibrary.SetIsFixNeeded(status, !isRunning);
            StatusLibrary.SetIsEnabled(status, true);
        }

        public static void Start()
        {
            try
            {
                StatusLibrary.SetStatusBar($"Starting World...");
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
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                proc.OutputDataReceived += new DataReceivedEventHandler((object src, DataReceivedEventArgs earg) =>
                {
                    string line = earg.Data;
                    if (line == null)
                    {
                        return;
                    }
                    StatusLibrary.Log($"World: {line}");
                    if (line.Contains("Server(TCP) listener started on port"))
                    {
                        StatusLibrary.SetStatusBar($"World is started");
                        StatusLibrary.SetIsFixNeeded(StatusType.World, false);
                    }
                });

                proc.ErrorDataReceived += new DataReceivedEventHandler((object src, DataReceivedEventArgs earg) =>
                {
                    string line = earg.Data;
                    if (line == null)
                    {
                        return;
                    }
                    StatusLibrary.Log($"World error: {line}");
                });

                proc.StartInfo.EnvironmentVariables["PATH"] = UtilityLibrary.EnvironmentPath();
                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
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
