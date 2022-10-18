using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

        public static void Check()
        {
            StatusLibrary.SetIsFixNeeded(status, true);
            string path = Application.StartupPath + "\\server";
            if (!Directory.Exists(path))
            {
                StatusLibrary.SetText(status, "server subfolder not found");
                return;
            }

            path = Application.StartupPath + "\\server\\eqemu_server.pl";
            if (!File.Exists(path))
            {
                StatusLibrary.SetText(status, "eqemu_server.pl not found");
                return;
            }

            path = Application.StartupPath + "\\server\\eqemu_config.json";
            if (!File.Exists(path))
            {
                StatusLibrary.SetText(status, "eqemu_config.json not found");
                return;
            }

            path = Application.StartupPath + "\\server\\zone.exe";
            if (!File.Exists(path))
            {
                StatusLibrary.SetText(status, "zone.exe not found");
                return;
            }

            StatusLibrary.SetIsFixNeeded(status, false);
            StatusLibrary.SetStage(status, 100);
            StatusLibrary.SetText(status, "server is installed");
        }

        public static void FixCheck()
        {
            Console.WriteLine("running fix check");
            FixTask = Task.Run(() => { Fix(false);  Check();});
        }

        public static async void Fix(bool fixAll)
        {
            int startStage = StatusLibrary.Stage(status);

            int stage = FixPath();
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await DownloadServerPerl();
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await DownloadConfig();
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await DownloadBinaries();
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = ExtractBinaries();
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }
        }

        public static void FixAll()
        {
            Console.WriteLine("fixing all server issues");
            FixTask = Task.Run(() => { Fix(true);  Check();});
        }

        public static int FixPath()
        {
            Console.WriteLine("fixing server path...");
            StatusLibrary.SetStage(status, 10);
            string path = Application.StartupPath + "\\server";

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
                string result = $"failed to create directory {path}: {ex.Message}";
                StatusLibrary.SetStatusBar(result);
                MessageBox.Show(result, "Server Folder Create", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }

            return 0;
        }

        public static async Task<int> DownloadServerPerl()
        {
            StatusLibrary.SetStage(status, 20);
            string result;
            
            string path = Application.StartupPath + "\\server\\eqemu_server.pl";
            if (!File.Exists(path))
            {
                Console.WriteLine("downloading eqemu_server.pl...");
                var ct = new CancellationToken();
                result = await UtilityLibrary.DownloadFile(ct, "https://raw.githubusercontent.com/EQEmu/Server/master/utils/scripts/eqemu_server.pl", path);
                if (result != "")
                {
                    result = $"failed to download eqemu_server.pl from https://raw.githubusercontent.com/EQEmu/Server/master/utils/scripts/eqemu_server.pl: {result}";
                    StatusLibrary.SetStatusBar("downloading eqemu_server.pl failed");
                    MessageBox.Show(result, "Server Download", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }
            }
            StatusLibrary.SetStatusBar("downloaded eqemu_server.pl");
            return 0;
        }

        public static async Task<int> DownloadConfig()
        {
            StatusLibrary.SetStage(status, 30);
            string result;

            string path = Application.StartupPath + "\\server\\eqemu_config.json";
            if (!File.Exists(path))
            {
                Console.WriteLine("downloading eqemu_config.json...");
                var ct = new CancellationToken();
                result = await UtilityLibrary.DownloadFile(ct, "https://raw.githubusercontent.com/Akkadius/eqemu-install-v2/master/eqemu_config.json", path);
                if (result != "")
                {
                    result = $"failed to download eqemu_config.json from https://raw.githubusercontent.com/Akkadius/eqemu-install-v2/master/eqemu_config.json: {result}";
                    StatusLibrary.SetStatusBar("downloading eqemu_config.json failed");
                    MessageBox.Show(result, "Server Download Config", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }
            }
            StatusLibrary.SetStatusBar("downloaded eqemu_config.json");
            Config.Load();
            
            return 0;
        }

        public static async Task<int> DownloadBinaries()
        {
            StatusLibrary.SetStage(status, 40);
            string result;
            string path = Application.StartupPath + "\\cache";

            Console.WriteLine("creating cache...");
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
                MessageBox.Show(result, "MariaDB Download", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }

            path = Application.StartupPath + "\\cache\\build_x64.zip";
            if (!File.Exists(path))
            {
                Console.WriteLine("downloading build_x64.zip...");
                var ct = new CancellationToken();
                result = await UtilityLibrary.DownloadFile(ct, "https://ci.appveyor.com/api/projects/KimLS/server-pglwk/artifacts/build_x64.zip", path);
                if (result != "")
                {
                    result = $"failed to download build_x64.zip from https://ci.appveyor.com/api/projects/KimLS/server-pglwk/artifacts/build_x64.zip: {result}";
                    StatusLibrary.SetStatusBar("downloading build_x64.zip failed");
                    MessageBox.Show(result, "Server Download Binaries", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }
            }
            StatusLibrary.SetStatusBar("downloaded build_x64.zip");
            return 0;
        }

        public static int ExtractBinaries()
        {
            Console.WriteLine("extracting build_x64.zip...");
            StatusLibrary.SetStage(status, 30);

            if (File.Exists(Application.StartupPath + "\\server\\zone.exe"))
            {
                StatusLibrary.SetStatusBar("zone.exe exists");
                return 0;
            }
            string result;
            string srcPath = Application.StartupPath + "\\cache";
            string dstPath = Application.StartupPath + "\\server";

            srcPath += "\\build_x64.zip";

            if (!File.Exists(srcPath))
            {
                result = $"failed to extract binaries from cache/build_x64.zip: file not found";
                StatusLibrary.SetStatusBar("downloading mariadb failed");
                MessageBox.Show(result, "MariaDB Download", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }

            try
            {
                ZipFile.ExtractToDirectory(srcPath, dstPath);
            }
            catch (Exception ex)
            {
                result = $"failed to extract {srcPath}: {ex.Message}";
                StatusLibrary.SetStatusBar(result);
                MessageBox.Show(result, "Binaries Extract", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            StatusLibrary.SetStatusBar("extracted binaries to server");
            return 0;
        }
    }
}
