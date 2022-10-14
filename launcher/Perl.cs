using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EQEmu_Launcher
{
    internal class Perl
    {
        public static void Check()
        {
            var status = StatusType.Perl;
            StatusLibrary.SetStage(status, 1);
            StatusLibrary.SetIsFixNeeded(status, true);
            StatusLibrary.SetText(status, "perl not found");
            StatusLibrary.SetDescription(status, "perl is used for quests. This is required to use PEQ's latest quest files.");

            // Check if perl is installed
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "perl.exe",
                    Arguments = "-v",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            string perlVersion = "";
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if (line.Contains("This is perl"))
                {
                    perlVersion = line;
                }
            }
            if (perlVersion.Length == 0)
            {
                return;
            }

            Regex rg = new Regex(@"\((v[0-9].*)\)");
            MatchCollection matches = rg.Matches(perlVersion);
            for (int i = 0; i < matches.Count; i++)
            {
                StatusLibrary.SetText(status, $"found Perl {matches[i].Groups[1]} installed");
                break;
            }
            StatusLibrary.SetStage(status, 100);
            StatusLibrary.SetIsFixNeeded(status, false);
        }

        public static void Fix()
        {
           
            Check();
        }
    }
}
