using EQEmu_Launcher.Manage;
using LibGit2Sharp;
using MS.WindowsAPICodePack.Internal;
using MySqlConnector;
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
            path = $"{Application.StartupPath}\\server\\updates_staged\\peq-latest.zip";
            if (!File.Exists(path))
            {
                StatusLibrary.SetText(status, "Cannot find peq-latest.zip");
                return 70;
            }

            StatusLibrary.SetIsFixNeeded(status, false);
            StatusLibrary.SetText(status, "Server is up to date");
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
            try
            {
                int startStage = Check();
                int stage;
                string path;
                stage = await UtilityLibrary.Download(0, 5, "https://archive.mariadb.org/mariadb-10.6.10/winx64-packages/mariadb-10.6.10-winx64.zip", "cache", "mariadb-10.6.10-winx64.zip", 136);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Extract(5, 7, "cache", "mariadb-10.6.10-winx64.zip", "db", $"{Application.StartupPath}\\db\\mariadb-10.6.10-winx64\\bin\\mysqld.exe", 136);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                path = $"{Application.StartupPath}\\db\\mariadb-10.6.10-winx64\\data";
                if (!Directory.Exists(path))
                {
                    StatusLibrary.SetStatusBar("Creating mysql data directory");
                    path = $"{Application.StartupPath}\\db\\mariadb-10.6.10-winx64\\bin\\mysql_install_db.exe";
                    var proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = path,
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
                    proc.WaitForExit();
                }

                stage = await UtilityLibrary.Download(7, 15, "https://strawberryperl.com/download/5.24.4.1/strawberry-perl-5.24.4.1-64bit-portable.zip", "cache", "perl-5.24.4.1-64bit-portable.zip", 128);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Extract(15, 20, "cache", "perl-5.24.4.1-64bit-portable.zip", "server\\perl", $"{Application.StartupPath}\\server\\perl\\perl\\bin\\perl.exe", 128);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Download(20, 21, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/scripts/eqemu_server.pl", "server", "eqemu_server.pl", 1);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                bool isNewConfig = (!File.Exists($"{Application.StartupPath}\\server\\eqemu_config.json"));
                stage = await UtilityLibrary.Download(21, 22, "https://raw.githubusercontent.com/Akkadius/eqemu-install-v2/master/eqemu_config.json", "server", "eqemu_config.json", 1);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                string rootPath = $"{Application.StartupPath}\\cache\\reset-root.txt";
                Config.Load(); // Refresh config, since we might just downloaded it
                if (isNewConfig)
                {
                    // Ensure all instances are properly closed
                    SQL.Stop();
                    StatusLibrary.SetProgress(22);
                    StatusLibrary.SetStatusBar("Setting new root password");
                    try
                    {
                        string password = WinLibrary.RandomString(32);
                        Config.Data["server"]["world"]["longname"] = $"Fippy Darklauncher {WinLibrary.RandomAlphaNumericString(8)}";
                        Config.Data["server"]["world"]["shortname"] = $"fip";
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
                        Config.Data["server"]["world"]["telnet"]["ip"] = "127.0.0.1";
                        Config.Data["server"]["directories"]["lua_modules"] = "quests/lua_modules/";
                        Config.Data["server"]["directories"]["quests"] = "quests/";
                        Config.Data["server"]["directories"]["plugins"] = "quests/plugins/";
                        Config.Data["server"]["directories"]["maps"] = "maps/";
                        Config.Save();

                        //File.WriteAllText(rootPath, $"UPDATE mysql.user SET Password=PASSWORD('{password}') WHERE User='root';\nFLUSH PRIVILEGES;");
                        File.WriteAllText(rootPath, $"ALTER USER root@'localhost' IDENTIFIED BY '{password}';\nFLUSH PRIVILEGES;");

                        // Start SQL with no root password
                        path = $"{Application.StartupPath}\\db\\mariadb-10.6.10-winx64\\bin\\mysqld.exe";
                        var proc = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = path,
                                Arguments = $"--init-file=\"{rootPath}\" --console --sql-mode=\"NO_ZERO_DATE\"",
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
                        proc.BeginErrorReadLine();
                        proc.BeginOutputReadLine();
                        proc.Start();
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        string result = $"Failed to set root password: {ex.Message}";
                        StatusLibrary.SetStatusBar(result);
                        MessageBox.Show(result, "Download {fileName}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    StatusLibrary.SetProgress(24);
                }

                stage = await UtilityLibrary.Download(24, 26, "https://ci.appveyor.com/api/projects/KimLS/server-pglwk/artifacts/build_x64.zip", "cache", "build_x64.zip", 12);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Extract(26, 27, "cache", "build_x64.zip", "server", $"{Application.StartupPath}\\server\\zone.exe", 12);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Download(27, 30, "https://www.heidisql.com/downloads/releases/HeidiSQL_12.1_64_Portable.zip", "cache", "HeidiSQL_12.1_64_Portable.zip", 17);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Extract(30, 32, "cache", "HeidiSQL_12.1_64_Portable.zip", "db\\heidi", $"{Application.StartupPath}\\db\\heidi\\heidisql.exe", 17);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Download(32, 33, "https://raw.githubusercontent.com/xackery/emulauncher/main/assets/portable_settings.txt", "db\\heidi", "portable_settings.txt", 1);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                path = $"{Application.StartupPath}\\db\\heidi\\portable.lock";
                if (!File.Exists(path))
                {
                    File.Create(path);
                }

                stage = await UtilityLibrary.Download(34, 45, "https://github.com/Akkadius/EQEmuMaps/archive/refs/heads/master.zip", "cache", "maps.zip", 1046);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Extract(45, 48, "cache", "maps.zip", "server\\maps", $"{Application.StartupPath}\\server\\maps\\base\\nro.map", 1046);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Download(48, 49, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/patches/opcodes.conf", "server\\assets\\opcodes", "opcodes.conf", 1);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Download(49, 50, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/patches/mail_opcodes.conf", "server\\assets\\opcodes", "mail_opcodes.conf", 1);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Download(50, 51, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/patches/patch_Titanium.conf", "server\\assets\\patches", "patch_Titanium.conf", 1);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Download(51, 52, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/patches/patch_RoF2.conf", "server\\assets\\patches", "patch_RoF2.conf", 1);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Download(52, 53, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/patches/patch_SoF.conf", "server\\assets\\patches", "patch_SoF.conf", 1);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Download(53, 54, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/patches/patch_SoD.conf", "server\\assets\\patches", "patch_SoD.conf", 1);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Download(54, 55, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/patches/patch_UF.conf", "server\\assets\\patches", "patch_UF.conf", 1);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Download(55, 56, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/patches/patch_RoF.conf", "server\\assets\\patches", "patch_RoF.conf", 1);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }


                stage = await UtilityLibrary.Download(56, 62, "https://github.com/ProjectEQ/projecteqquests/archive/refs/heads/master.zip", "cache", "peq-quests.zip", 6);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Extract(62, 68, "cache", "peq-quests.zip", "server\\quests", $"{Application.StartupPath}\\server\\quests\\ecommons\\Bubar.pl", 6);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Download(68, 75, "http://db.projecteq.net/api/v1/dump/latest", "cache", "peq-db-latest.zip", 30);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                stage = await UtilityLibrary.Download(75, 76, "https://raw.githubusercontent.com/xackery/emulauncher/main/assets/peq_download.pl", "server", "peq_download.pl", 30);
                if (stage == -1) { return; }
                if (!fixAll && stage > startStage) { return; }

                bool isNewDB = await UtilityLibrary.CreatePEQDB(76, 77);

                if (isNewDB)
                {
                    stage = await UtilityLibrary.Extract(77, 80, "cache", "peq-db-latest.zip", "cache\\peq-db-latest", $"{Application.StartupPath}\\cache\\peq-db-latest\\peq-dump\\create_tables_content.sql", 30);
                    if (stage == -1) { return; }
                    if (!fixAll && stage > startStage) { return; }

                    stage = await UtilityLibrary.SourcePEQDB(80, 90);
                    if (stage == -1) { return; }
                    if (!fixAll && stage > startStage) { return; }
                }

                // wait a second before cleanup
                Thread.Sleep(500);
                StatusLibrary.SetStatusBar("Cleaning up cache");
                path = $"{Application.StartupPath}\\cache\\peq-db-latest";
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }

                // Clean up root password file, if it exists
                if (File.Exists(rootPath))
                {
                    File.Delete(rootPath);
                }

                Config.Load();
                SQL.Check();
                SharedMemory.Check();
                World.Check();
                Zones.Check();
                UCS.Check();
                QueryServ.Check();

                StatusLibrary.SetStatusBar("Server successfully updated");
            } catch (Exception ex)
            {
                string result = $"Failed to download content: {ex.Message}";
                MessageBox.Show(result, "Download Content", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void FixAll()
        {
            StatusLibrary.Log("Fixing all server issues");
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
