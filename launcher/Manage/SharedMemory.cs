using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EQEmu_Launcher.Manage
{
    internal class SharedMemory
    {
        public static bool IsRunning()
        {
            Process[] processes = Process.GetProcessesByName("shared_memory");
            return processes.Length > 0;
        }

        static readonly StatusType status = StatusType.SharedMemory;

        public static void Check()
        {
            try
            {
                string path = $"{Application.StartupPath}\\server\\shared\\base_data";
                if (!File.Exists(path))
                {
                    StatusLibrary.SetText(status, "SharedMemory needs to be ran");
                    StatusLibrary.SetIsFixNeeded(status, true);
                    StatusLibrary.SetIsEnabled(StatusType.World, false);
                    return;
                }
                StatusLibrary.SetText(status, "SharedMemory is valid");
                StatusLibrary.SetIsFixNeeded(status, false);
                StatusLibrary.SetIsEnabled(StatusType.World, true);
            }
            catch (Exception ex)
            {
                StatusLibrary.Log($"Failed to check SharedMemory: {ex.Message}");
            }
        }

        public static async Task Start()
        {
            try
            {
                StatusLibrary.SetStatusBar($"Starting SharedMemory");
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = $"{Application.StartupPath}\\server\\shared_memory.exe",
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
                    StatusLibrary.Log($"SharedMemory: {line}");
                });

                proc.ErrorDataReceived += new DataReceivedEventHandler((object src, DataReceivedEventArgs earg) =>
                {
                    string line = earg.Data;
                    if (line == null)
                    {
                        return;
                    }
                    StatusLibrary.Log($"SharedMemory error: {line}");
                });
                proc.StartInfo.EnvironmentVariables["PATH"] = UtilityLibrary.EnvironmentPath();
                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
                proc.WaitForExit();
            } catch (Exception e)
            {
                string result = $"Failed SharedMemory start \"server\\sharedMemory.exe\": {e.Message}";
                StatusLibrary.SetStatusBar(result);
                MessageBox.Show(result, "SharedMemory Start", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Check();
        }

        public static void Stop()
        {
            int sharedMemoryCount = 0;
            try
            {

                Process[] workers = Process.GetProcessesByName("sharedmemory");
                StatusLibrary.Log($"Found {workers.Length} sharedMemory instances");
                foreach (Process worker in workers)
                {
                    StatusLibrary.Log($"Stopping sharedMemory pid {worker.Id}");
                    worker.Kill();
                    worker.WaitForExit();
                    worker.Dispose();
                    sharedMemoryCount++;
                }
                if (sharedMemoryCount == 0) StatusLibrary.SetStatusBar("SharedMemory not found to stop");
                else StatusLibrary.SetStatusBar($"Stopped {sharedMemoryCount} sharedMemory instances");
            }
            catch (Exception e)
            {
                string result = $"failed to stop sharedMemory: {e.Message}";
                StatusLibrary.SetStatusBar("SharedMemory stop failed");
                MessageBox.Show(result, "SharedMemory Stop", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
