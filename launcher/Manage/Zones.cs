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
    internal class Zones
    {
        static readonly StatusType status = StatusType.Zone;

        public static bool IsRunning()
        {
            Process[] processes = Process.GetProcessesByName("zone");
            return processes.Length > 0;
        }

        public static void Check()
        {
            Process[] pname = Process.GetProcessesByName("zone");
            if (pname.Length > 0)
            {
                StatusLibrary.SetText(status, $"{pname.Length} of          zone instances running");
                StatusLibrary.SetIsFixNeeded(status, false);
                return;
            }
            StatusLibrary.SetText(status, "0 of          zone instances running");
            StatusLibrary.SetIsFixNeeded(status, true);
        }

        public static void Start()
        {
            try
            {
                
                for (int i = 0; i < 3; i++)
                {
                    StatusLibrary.SetStatusBar($"Starting Zone #{i}");
                    var proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = $"{Application.StartupPath}\\server\\zone.exe",
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
                        StatusLibrary.Log($"Zone: {line}");
                    });

                    proc.ErrorDataReceived += new DataReceivedEventHandler((object src, DataReceivedEventArgs earg) =>
                    {
                        string line = earg.Data;
                        if (line == null)
                        {
                            return;
                        }
                        StatusLibrary.Log($"Zone error: {line}");
                    });

                    proc.StartInfo.EnvironmentVariables["PATH"] = UtilityLibrary.EnvironmentPath();
                    proc.Start();
                    proc.BeginErrorReadLine();
                    proc.BeginOutputReadLine();
                    Check();
                    StatusLibrary.SetStatusBar("Zones started");
                }
            } catch (Exception e)
            {
                string result = $"Failed zone start \"server\\zone.exe\": {e.Message}";
                StatusLibrary.SetStatusBar(result);
                MessageBox.Show(result, "Zone Start", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void Stop()
        {
            int zoneCount = 0;
            try
            {

                Process[] workers = Process.GetProcessesByName("zone");
                StatusLibrary.Log($"Found {workers.Length} zone instances");
                foreach (Process worker in workers)
                {
                    StatusLibrary.Log($"Stopping zone pid {worker.Id}");
                    worker.Kill();
                    worker.WaitForExit();
                    worker.Dispose();
                    zoneCount++;
                }
                if (zoneCount == 0) StatusLibrary.SetStatusBar("zone not found to stop");
                else StatusLibrary.SetStatusBar($"stopped {zoneCount} zone instances");
            }
            catch (Exception e)
            {
                string result = $"Failed to stop zone: {e.Message}";
                StatusLibrary.SetStatusBar("zone stop failed");
                MessageBox.Show(result, "Zone Stop", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
