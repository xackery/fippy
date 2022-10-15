using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EQEmu_Launcher
{
    internal class Perl
    {
        private readonly static StatusType status = StatusType.Perl;
        public static Task FixTask { get; private set; }

        public static void Check()
        {
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

        public static void FixCheck()
        {
            Console.WriteLine("running fix check");
            CancellationToken ct = new CancellationToken();
            FixTask = Task.Run(() => Fix(ct, false));
            Check();
        }

        public static async void Fix(CancellationToken ct, bool fixAll)
        {
            int startStage = StatusLibrary.Stage(status);
            int stage = await FixDownloadPerl(ct);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }
        }

        public static void FixAll()
        {
            Console.WriteLine("fixing all quest issues");
            CancellationToken ct = new CancellationToken();
            FixTask = Task.Run(() => Fix(ct, true));
            Check();
        }

        public static async Task<int> FixDownloadPerl(CancellationToken ct)
        {
            Console.WriteLine("creating cache...");
            StatusLibrary.SetStage(status, 20);
            string result;
            string path = Application.StartupPath + "\\cache";
            try
            {
                if (!Directory.Exists(path))
                {
                    StatusLibrary.SetStatusBar($"creating directory {path}...");
                    DirectoryInfo di = Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                result = $"failed to create directory {path}: {ex.Message}";
                StatusLibrary.SetStatusBar(result);
                MessageBox.Show(result, "Maps Fix", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            path += "\\strawberry-perl-5.24.4.1-64bit.msi";
            if (!File.Exists(path))
            {
                StatusLibrary.SetStatusBar("downloading maps...");
                result = await UtilityLibrary.DownloadFile(ct, "https://strawberryperl.com/download/5.24.4.1/strawberry-perl-5.24.4.1-64bit.msi", path);
                if (result != "")
                {
                    result = $"failed to download perl from https://strawberryperl.com/download/5.24.4.1/strawberry-perl-5.24.4.1-64bit.msi: {result}";
                    StatusLibrary.SetStatusBar("downloading perl failed");
                    MessageBox.Show(result, "Perl Fix", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }
            }
            StatusLibrary.SetStatusBar("downloaded perl");
            return 0;
        }

    }
}
