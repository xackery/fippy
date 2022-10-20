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
    internal class SharedMemory
    {
        static readonly StatusType status = StatusType.SharedMemory;

        public static void Check()
        {
            Process[] pname = Process.GetProcessesByName("sharedMemory");
            if (pname.Length > 0)
            {
                StatusLibrary.SetText(status, $"sharedMemory is running");
                StatusLibrary.SetIsFixNeeded(status, false);
                return;
            }
            StatusLibrary.SetText(status, "sharedMemory is not running");
            StatusLibrary.SetIsFixNeeded(status, true);
        }

        public static void Start()
        {
            try
            {
                StatusLibrary.SetStatusBar($"starting SharedMemory");
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = $"{Application.StartupPath}\\server\\shared_memory.exe",
                        WorkingDirectory = $"{Application.StartupPath}\\server",
                        Arguments = "",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                proc.StartInfo.EnvironmentVariables["PATH"] = UtilityLibrary.EnvironmentPath();
                proc.Start();

                while (!proc.StandardOutput.EndOfStream)
                {
                    Console.WriteLine($"sharedMemory: {proc.StandardOutput.ReadLine()}");
                }
                Console.WriteLine($"sharedMemory: exited");
                Check();
            } catch (Exception e)
            {
                string result = $"failed sharedMemory start \"server\\sharedMemory.exe\": {e.Message}";
                StatusLibrary.SetStatusBar(result);
                MessageBox.Show(result, "SharedMemory Start", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void Stop()
        {
            int sharedMemoryCount = 0;
            try
            {

                Process[] workers = Process.GetProcessesByName("sharedMemory");
                Console.WriteLine($"found {workers.Length} sharedMemory instances");
                foreach (Process worker in workers)
                {
                    Console.WriteLine($"stopping sharedMemory pid {worker.Id}");
                    worker.Kill();
                    worker.WaitForExit();
                    worker.Dispose();
                    sharedMemoryCount++;
                }
                if (sharedMemoryCount == 0) StatusLibrary.SetStatusBar("sharedMemory not found to stop");
                else StatusLibrary.SetStatusBar($"stopped {sharedMemoryCount} sharedMemory instances");
            }
            catch (Exception e)
            {
                string result = $"failed to stop sharedMemory: {e.Message}";
                StatusLibrary.SetStatusBar("sharedMemory stop failed");
                MessageBox.Show(result, "SharedMemory Stop", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
