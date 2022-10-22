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
    internal class UCS
    {
        static readonly StatusType status = StatusType.UCS;

        public static void Check()
        {
            Process[] pname = Process.GetProcessesByName("UCS");
            if (pname.Length > 0)
            {
                StatusLibrary.SetText(status, $"UCS is running");
                StatusLibrary.SetIsFixNeeded(status, false);
                return;
            }
            StatusLibrary.SetText(status, "UCS is not running");
            StatusLibrary.SetIsFixNeeded(status, true);
        }

        public static void Start()
        {
            try
            {
                StatusLibrary.SetStatusBar($"starting ucs");
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = $"{Application.StartupPath}\\server\\ucs.exe",
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
                        StatusLibrary.Log($"UCS: {proc.StandardOutput.ReadLine()}");
                    }
                    StatusLibrary.Log($"ucs: exited");
                });
                Check();
            }
            catch (Exception e)
            {
                string result = $"failed ucs start \"server\\ucs.exe\": {e.Message}";
                StatusLibrary.SetStatusBar(result);
                MessageBox.Show(result, "UCS Start", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void Stop()
        {
            int UCSCount = 0;
            try
            {

                Process[] workers = Process.GetProcessesByName("UCS");
                StatusLibrary.Log($"Found {workers.Length} UCS instances");
                foreach (Process worker in workers)
                {
                    StatusLibrary.Log($"Stopping UCS pid {worker.Id}");
                    worker.Kill();
                    worker.WaitForExit();
                    worker.Dispose();
                    UCSCount++;
                }
                if (UCSCount == 0) StatusLibrary.SetStatusBar("UCS not found to stop");
                else StatusLibrary.SetStatusBar($"stopped {UCSCount} UCS instances");
            }
            catch (Exception e)
            {
                string result = $"failed to stop UCS: {e.Message}";
                StatusLibrary.SetStatusBar("UCS stop failed");
                MessageBox.Show(result, "UCS Stop", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
