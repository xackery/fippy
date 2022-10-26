using EQEmu_Launcher.Manage;
using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EQEmu_Launcher
{
    internal class SQL
    {
        static readonly StatusType status = StatusType.SQL;

        public static bool IsRunning()
        {
            Process[] processes = Process.GetProcessesByName("mysqld");
            if (processes.Length == 0)
            {
                return false;
            }
            foreach (Process process in processes)
            {
                if (!process.MainModule.FileName.Equals($"{Application.StartupPath}\\db\\mariadb-10.6.10-winx64\\bin\\mysqld.exe"))
                {
                    return false;
                }
            }
            return true;
        }

        public static void Check()
        {
            Process[] processes = Process.GetProcessesByName("mysqld");
            if (processes.Length == 0)
            {
                StatusLibrary.SetText(status, "SQL is not running");
                StatusLibrary.SetIsFixNeeded(status, true);
                StatusLibrary.SetIsEnabled(StatusType.SharedMemory, false);
                return;
            }

            bool isExited = true;
            foreach (Process process in processes)
            {
                if (process.HasExited)
                {                    
                    continue;
                }
                try
                {
                    if (!process.MainModule.FileName.Equals($"{Application.StartupPath}\\db\\mariadb-10.6.10-winx64\\bin\\mysqld.exe"))
                    {
                        StatusLibrary.SetText(status, "Another SQL instance running");
                        StatusLibrary.SetIsFixNeeded(status, true);
                        StatusLibrary.SetIsEnabled(StatusType.SharedMemory, false);
                        return;
                    }
                }  catch (Exception ex)
                {
                    StatusLibrary.Log($"Failed to get SQL process list: {ex.Message}");
                }
                isExited = false;
            }
            if (isExited)
            {
                StatusLibrary.SetText(status, "SQL is not running");
                StatusLibrary.SetIsFixNeeded(status, true);
                StatusLibrary.SetIsEnabled(StatusType.SharedMemory, false);
                return;
            }

            StatusLibrary.SetText(status, "SQL is running");
            StatusLibrary.SetIsFixNeeded(status, false);
            StatusLibrary.SetIsEnabled(StatusType.SharedMemory, true);
            return;
           
        }

        public static void Start() {            
            Stop();
            StatusLibrary.SetStatusBar($"starting sql");
            string path = $"{Application.StartupPath}\\db\\mariadb-10.6.10-winx64\\data";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = $"{Application.StartupPath}\\db\\mariadb-10.6.10-winx64\\bin\\mysqld.exe",
                    Arguments = "--console --sql-mode=\"NO_ZERO_DATE\"",
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
                StatusLibrary.Log($"sql: {line}");
            });

            proc.ErrorDataReceived += new DataReceivedEventHandler((object src, DataReceivedEventArgs earg) =>
            {
                string line = earg.Data;
                if (line == null)
                {
                    return;
                }
                StatusLibrary.Log($"sql error: {line}");
            });
            proc.Start();
            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();
           
            Check();
            SharedMemory.Check();
            World.Check();
            Zones.Check();
            UCS.Check();
            QueryServ.Check();
        }

        public static bool Stop()
        {
            bool isStopped = true;
            int sqlCount = 0;
            try
            {

                Process[] processes = Process.GetProcessesByName("mysqld");
                StatusLibrary.Log($"Found {processes.Length} mysqld instances");
                bool isMySQLForeign = false;
                foreach (Process process in processes)
                {
                    if (!process.MainModule.FileName.Equals($"{Application.StartupPath}\\db\\mariadb-10.6.10-winx64\\bin\\mysqld.exe"))
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
