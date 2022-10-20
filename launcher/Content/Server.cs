using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EQEmu_Launcher
{
    internal class Server
    {
        private readonly static StatusType status = StatusType.Server;
        public static Task FixTask { get; private set; }

        public static int Check()
        {
            StatusLibrary.SetIsFixNeeded(status, true);
            string path;

            path = Application.StartupPath + "\\server\\perl\\perl\\bin\\perl.exe";
            if (!File.Exists(path))
            {
                StatusLibrary.SetText(status, "perl not found");
                return 0;
            }

            path = Application.StartupPath + "\\server\\eqemu_server.pl";
            if (!File.Exists(path))
            {
                StatusLibrary.SetText(status, "eqemu_server.pl not found");
                return 70;
            }

            path = Application.StartupPath + "\\server\\eqemu_config.json";
            if (!File.Exists(path))
            {
                StatusLibrary.SetText(status, "eqemu_config.json not found");
                return 71;
            }

            path = Application.StartupPath + "\\server\\zone.exe";
            if (!File.Exists(path))
            {
                StatusLibrary.SetText(status, "zone.exe not found");
                return 72;
            }

            StatusLibrary.SetIsFixNeeded(status, false);
            StatusLibrary.SetText(status, "server is up to date");
            return 100;
        }

        public static void FixCheck()
        {
            FixTask = Task.Run(async() => { 
                StatusLibrary.LockUI(); 
                await Fix(false);  
                Check(); 
                StatusLibrary.UnlockUI(); 
            });
        }

        public static async Task Fix(bool fixAll)
        {
            int startStage = Check();
            int stage;
            
            stage = await UtilityLibrary.Download(0, 50, "https://strawberryperl.com/download/5.24.4.1/strawberry-perl-5.24.4.1-64bit-portable.zip", "cache", "perl-5.24.4.1-64bit-portable.zip", 128 );
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Extract(50, 70, "cache", "perl-5.24.4.1-64bit-portable.zip", "server\\perl", $"{Application.StartupPath}\\server\\perl\\perl\\bin\\perl.exe", 128);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Download(70, 71, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/scripts/eqemu_server.pl", "server", "eqemu_server.pl", 1);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Download(71, 72, "https://raw.githubusercontent.com/Akkadius/eqemu-install-v2/master/eqemu_config.json", "server", "eqemu_config.json", 1);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Download(72, 95, "https://ci.appveyor.com/api/projects/KimLS/server-pglwk/artifacts/build_x64.zip", "cache", "build_x64.zip", 12);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Extract(95, 100, "cache", "build_x64.zip", "server", $"{Application.StartupPath}\\server\\zone.exe", 12);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            StatusLibrary.SetStatusBar("Server successfully updated");
        }

        public static void FixAll()
        {
            Console.WriteLine("fixing all server issues");
            Task.Run(async () => 
            { 
                StatusLibrary.LockUI();
                await Fix(true);
                Check();
                StatusLibrary.UnlockUI();
            });
        }
    }
}
