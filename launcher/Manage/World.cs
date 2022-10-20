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
                StatusLibrary.SetText(status, $"world is running");
                StatusLibrary.SetIsFixNeeded(status, false);
                return;
            }
            StatusLibrary.SetText(status, "world is not running");
            StatusLibrary.SetIsFixNeeded(status, true);
        }

        public static void Start()
        {
            try
            {
                SharedMemory.Start();

                StatusLibrary.SetStatusBar($"starting world");
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
                        Console.WriteLine($"world: {proc.StandardOutput.ReadLine()}");
                    }
                    Console.WriteLine($"world: exited");
                });
                Check();
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
                Console.WriteLine($"found {workers.Length} world instances");
                foreach (Process worker in workers)
                {
                    Console.WriteLine($"stopping world pid {worker.Id}");
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
