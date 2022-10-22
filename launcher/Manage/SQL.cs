using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EQEmu_Launcher
{
    internal class SQL
    {
        static CancellationTokenSource CancelToken;
        static readonly StatusType status = StatusType.SQL;

        public static void Check()
        {
            Process[] pname = Process.GetProcessesByName("mysqld");
            if (pname.Length > 0)
            {
                StatusLibrary.SetText(status, "SQL is running");
                StatusLibrary.SetIsFixNeeded(status, false);
                return;
            }
            StatusLibrary.SetText(status, "SQL is not running");
            StatusLibrary.SetIsFixNeeded(status, true);
        }

        public static void Start() {            
            if (CancelToken != null)
            {
                CancelToken.Cancel();
            }
            CancelToken = new CancellationTokenSource();

            Stop();
            StatusLibrary.SetStatusBar($"starting sql");
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = $"{Application.StartupPath}\\db\\mariadb-5.5.29-winx64\\bin\\mysqld.exe",
                    Arguments = "--console",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            Task.Run(() =>
            {
                while (!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput.ReadLine();
                    StatusLibrary.Log($"SQL: {line}");
                }
            });
           
            Check();
        }

        public static bool Stop()
        {
            bool isStopped = true;
            int sqlCount = 0;
            try
            {
                if (CancelToken != null)
                {
                    CancelToken.Cancel();
                }

                Process[] processes = Process.GetProcessesByName("mysqld");
                StatusLibrary.Log($"Found {processes.Length} mysqld instances");
                bool isMySQLForeign = false;
                foreach (Process process in processes)
                {
                    if (!process.MainModule.FileName.Equals($"{Application.StartupPath}\\db\\mariadb-5.5.29-winx64\\bin\\mysqld.exe"))
                    {
                        isMySQLForeign = true;
                    }
                }

                if (isMySQLForeign)
                {
                    var response = MessageBox.Show("MySQL is currently running and it isn't the one Fippy manages.\nDo you want Fippy to try to stop it?", "MySQL Already Running", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (response == DialogResult.No)
                    {
                        StatusLibrary.SetStatusBar("Cancelled MySQL action");
                        return false;
                    }
                }

                foreach (Process process in processes)
                {
                    StatusLibrary.Log($"Stopping sql pid {process.Id}");
                    process.Kill();
                    process.WaitForExit();
                    process.Dispose();
                    sqlCount++;
                }

                if (sqlCount == 0) StatusLibrary.SetStatusBar("SQL not found to stop");
                else StatusLibrary.SetStatusBar($"Stopped {sqlCount} sql instances");
            } catch(Exception e)
            {
                string result = $"Failed to stop SQL: {e.Message}";
                StatusLibrary.SetStatusBar("sql stop failed");
                MessageBox.Show(result, "SQL Stop", MessageBoxButtons.OK, MessageBoxIcon.Error);
                isStopped = false;
            }
            return isStopped;
        }
    }
}
