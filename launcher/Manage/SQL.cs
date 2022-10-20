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
                    Console.WriteLine($"sql: {line}");
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

                Process[] workers = Process.GetProcessesByName("mysqld");
                Console.WriteLine($"found {workers.Length} mysqld instances");
                foreach (Process worker in workers)
                {
                    Console.WriteLine($"stopping sql pid {worker.Id}");
                    worker.Kill();
                    worker.WaitForExit();
                    worker.Dispose();
                    sqlCount++;
                }
                if (sqlCount == 0) StatusLibrary.SetStatusBar("sql not found to stop");
                else StatusLibrary.SetStatusBar($"stopped {sqlCount} sql instances");
            } catch(Exception e)
            {
                string result = $"failed to stop SQL: {e.Message}";
                StatusLibrary.SetStatusBar("sql stop failed");
                MessageBox.Show(result, "SQL Stop", MessageBoxButtons.OK, MessageBoxIcon.Error);
                isStopped = false;
            }
            return isStopped;
        }
    }
}
