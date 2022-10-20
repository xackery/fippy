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
    internal class Zone
    {
        static readonly StatusType status = StatusType.Zone;

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
                    StatusLibrary.SetStatusBar($"starting {i} zone");
                    var proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = $"{Application.StartupPath}\\server\\zone.exe",
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
                            Console.WriteLine($"zone {i}: {proc.StandardOutput.ReadLine()}");
                        }
                        Console.WriteLine($"zone: exited");
                    });
                    Check();
                }
            } catch (Exception e)
            {
                string result = $"failed zone start \"server\\zone.exe\": {e.Message}";
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
                Console.WriteLine($"found {workers.Length} zone instances");
                foreach (Process worker in workers)
                {
                    Console.WriteLine($"stopping zone pid {worker.Id}");
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
                string result = $"failed to stop zone: {e.Message}";
                StatusLibrary.SetStatusBar("zone stop failed");
                MessageBox.Show(result, "Zone Stop", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
