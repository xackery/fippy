using LibGit2Sharp;
using MS.WindowsAPICodePack.Internal;
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

            stage = await UtilityLibrary.Download(0, 5, "https://archive.mariadb.org/mariadb-5.5.29/winx64-packages/mariadb-5.5.29-winx64.zip", "cache", "mariadb-5.5.29-winx64.zip", 136);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Extract(5, 10, "cache", "mariadb-5.5.29-winx64.zip", "db", $"{Application.StartupPath}\\db\\mariadb-5.5.29-winx64\\bin\\mysqld.exe", 136);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Download(10, 15, "https://strawberryperl.com/download/5.24.4.1/strawberry-perl-5.24.4.1-64bit-portable.zip", "cache", "perl-5.24.4.1-64bit-portable.zip", 128);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Extract(15, 30, "cache", "perl-5.24.4.1-64bit-portable.zip", "server\\perl", $"{Application.StartupPath}\\server\\perl\\perl\\bin\\perl.exe", 128);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Download(30, 31, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/scripts/eqemu_server.pl", "server", "eqemu_server.pl", 1);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            bool isNewConfig = (!File.Exists($"{Application.StartupPath}\\server\\eqemu_config.json"));
            stage = await UtilityLibrary.Download(31, 32, "https://raw.githubusercontent.com/Akkadius/eqemu-install-v2/master/eqemu_config.json", "server", "eqemu_config.json", 1);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            string rootPath = $"{Application.StartupPath}\\cache\\reset-root.txt";
            Config.Load(); // Refresh config, since we might just downloaded it
            if (isNewConfig)
            {
                StatusLibrary.SetProgress(32);
                StatusLibrary.SetStatusBar("Setting new root password");
                try
                {
                    string password = WinLibrary.RandomString(32);
                    Config.Data["server"]["database"]["host"] = "127.0.0.1";
                    Config.Data["server"]["database"]["port"] = "3306";
                    Config.Data["server"]["database"]["username"] = "root";
                    Config.Data["server"]["database"]["password"] = password;
                    Config.Data["server"]["database"]["db"] = "peq";
                    Config.Data["server"]["qsdatabase"]["host"] = "127.0.0.1";
                    Config.Data["server"]["qsdatabase"]["port"] = "3306";
                    Config.Data["server"]["qsdatabase"]["username"] = "root";
                    Config.Data["server"]["qsdatabase"]["password"] = password;
                    Config.Data["server"]["qsdatabase"]["db"] = "peq";
                    Config.Save();
                    
                    File.WriteAllText(rootPath, $"UPDATE mysql.user SET Password=PASSWORD('{password}') WHERE User='root';\nFLUSH PRIVILEGES;");

                    // Start SQL with no root password
                    string path = $"{Application.StartupPath}\\db\\mariadb-5.5.29-winx64\\bin\\mysqld.exe";
                    var proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = path,
                            Arguments = $"--init-file=\"{rootPath}\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    proc.Start();
                    _ = Task.Run(() =>
                    {
                        while (!proc.StandardOutput.EndOfStream)
                        {
                            string line = proc.StandardOutput.ReadLine();
                            Console.WriteLine($"sql: {line}");
                        }
                    });
                    Thread.Sleep(1000);
                } catch (Exception ex)
                {
                    string result = $"Failed to set root password: {ex.Message}";
                    StatusLibrary.SetStatusBar(result);
                    MessageBox.Show(result, "Download {fileName}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                StatusLibrary.SetProgress(40);
            }

            stage = await UtilityLibrary.Download(40, 42, "https://ci.appveyor.com/api/projects/KimLS/server-pglwk/artifacts/build_x64.zip", "cache", "build_x64.zip", 12);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Extract(42, 45, "cache", "build_x64.zip", "server", $"{Application.StartupPath}\\server\\zone.exe", 12);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Download(45, 47, "https://www.heidisql.com/downloads/releases/HeidiSQL_12.1_64_Portable.zip", "cache", "HeidiSQL_12.1_64_Portable.zip", 17);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Extract(47, 50, "cache", "HeidiSQL_12.1_64_Portable.zip", "db\\heidi", $"{Application.StartupPath}\\db\\heidi\\heidisql.exe", 17);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Download(50, 51, "https://raw.githubusercontent.com/xackery/emulauncher/main/launcher/Assets/portable_settings.txt", "db\\heidi", "portable_settings.txt", 1);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            File.Create($"{Application.StartupPath}\\db\\heidi\\portable.lock");

            stage = await UtilityLibrary.Download(51, 70, "https://github.com/Akkadius/EQEmuMaps/archive/refs/heads/master.zip", "cache", "maps.zip", 1046);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Extract(70, 80, "cache", "maps.zip", "server\\maps", $"{Application.StartupPath}\\server\\maps\\base\\nro.map", 1046);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Download(80, 81, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/patches/opcodes.conf", "server\\assets\\opcodes", "opcodes.conf", 1);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Download(81, 82, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/patches/mail_opcodes.conf", "server\\assets\\opcodes", "mail_opcodes.conf", 1);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Download(82, 83, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/patches/patch_Titanium.conf", "server\\assets\\patches", "patch_Titanium.conf", 1);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Download(83, 84, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/patches/patch_RoF2.conf", "server\\assets\\patches", "patch_RoF2.conf", 1);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Download(84, 85, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/patches/patch_SoF.conf", "server\\assets\\patches", "patch_SoF.conf", 1);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Download(85, 86, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/patches/patch_SoD.conf", "server\\assets\\patches", "patch_SoD.conf", 1);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Download(86, 87, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/patches/patch_UF.conf", "server\\assets\\patches", "patch_UF.conf", 1);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Download(87, 88, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/patches/patch_RoF.conf", "server\\assets\\patches", "patch_RoF.conf", 1);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }


            stage = await UtilityLibrary.Download(88, 90, "https://github.com/ProjectEQ/projecteqquests/archive/refs/heads/master.zip", "cache", "peq-quests.zip", 6);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Extract(90, 92, "cache", "peq-quests.zip", "server\\quests", $"{Application.StartupPath}\\server\\quests\\ecommons\\Bubar.pl", 6);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Download(92, 95, "http://db.projecteq.net/api/v1/dump/latest", "cache", "db-latest.zip", 29);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.SourceFromDatabase2(95, 98, "cache", "db-latest.zip");
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            // Clean up root password file, if it exists
            if (File.Exists(rootPath))
            {
                File.Delete(rootPath);
            }
            
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
