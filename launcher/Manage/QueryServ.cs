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
    internal class QueryServ
    {
        static readonly StatusType status = StatusType.QueryServ;

        public static bool IsRunning()
        {
            Process[] processes = Process.GetProcessesByName("QueryServ");
            return processes.Length > 0;
        }

        public static void Check()
        {
            Process[] pname = Process.GetProcessesByName("QueryServ");
            if (pname.Length > 0)
            {
                StatusLibrary.SetText(status, $"QueryServ is running");
                StatusLibrary.SetIsFixNeeded(status, false);
                return;
            }
            StatusLibrary.SetText(status, "QueryServ is not running");
            StatusLibrary.SetIsFixNeeded(status, true);
        }

        public static void Start()
        {
            try
            {
                StatusLibrary.SetStatusBar($"Starting QueryServ");
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = $"{Application.StartupPath}\\server\\queryserv.exe",
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
                    StatusLibrary.Log($"QueryServ: {line}");
                });

                proc.ErrorDataReceived += new DataReceivedEventHandler((object src, DataReceivedEventArgs earg) =>
                {
                    string line = earg.Data;
                    if (line == null)
                    {
                        return;
                    }
                    StatusLibrary.Log($"QueryServ error: {line}");
                });
                proc.StartInfo.EnvironmentVariables["PATH"] = UtilityLibrary.EnvironmentPath();
                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

                Check();
            }
            catch (Exception e)
            {
                string result = $"Failed to start QueryServ\n{e.Message}";
                StatusLibrary.SetStatusBar(result);
                MessageBox.Show(result, "QueryServ Start", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void Stop()
        {
            int QueryServCount = 0;
            try
            {

                Process[] workers = Process.GetProcessesByName("QueryServ");
                StatusLibrary.Log($"Found {workers.Length} QueryServ instances");
                foreach (Process worker in workers)
                {
                    StatusLibrary.Log($"Stopping QueryServ pid {worker.Id}");
                    worker.Kill();
                    worker.WaitForExit();
                    worker.Dispose();
                    QueryServCount++;
                }
                if (QueryServCount == 0) StatusLibrary.SetStatusBar("QueryServ not found to stop");
                else StatusLibrary.SetStatusBar($"stopped {QueryServCount} QueryServ instances");
            }
            catch (Exception e)
            {
                string result = $"failed to stop QueryServ: {e.Message}";
                StatusLibrary.SetStatusBar("QueryServ stop failed");
                MessageBox.Show(result, "QueryServ Stop", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
