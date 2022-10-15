using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EQEmu_Launcher
{
    internal class Database
    {
        private readonly static StatusType status = StatusType.Database;
        public static Task FixTask { get; private set; }

        public static void Check()
        {
            StatusLibrary.SetIsFixNeeded(status, true);

            string path = Application.StartupPath + "\\db";
            if (!Directory.Exists(path))
            {
                StatusLibrary.SetText(status, "db subfolder not found");
                return;
            }

            path = Application.StartupPath + "\\cache\\mariadb-5.5.29-winx64.zip";
            if (!File.Exists(path))
            {
                StatusLibrary.SetText(status, "mariadb not found");
                return;
            }

            path = Application.StartupPath + "\\db\\mariadb-5.5.29-winx64";
            if (!Directory.Exists(path))
            {
                StatusLibrary.SetText(status, "mariadb not found");
                return;
            }

            if (!File.Exists(Application.StartupPath + "\\db\\heidi\\heidisql.exe"))
            {
                StatusLibrary.SetText(status, "heidi not found");
                return;
            }

            if (!File.Exists(Application.StartupPath + "\\db\\heidi\\portable_settings.txt"))
            {
                StatusLibrary.SetText(status, "heidi settings not found");
                return;
            }

            StatusLibrary.SetIsFixNeeded(status, false);
            StatusLibrary.SetText(status, "database found");
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
            int stage = CreatePath(ct);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await DownloadMariaDB(ct);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await ExtractMariaDB(ct);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await DownloadHeidi(ct);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await ExtractHeidi(ct);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await DownloadHeidiSettings(ct);
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

        public static int CreatePath(CancellationToken ct)
        {
            Console.WriteLine("creating path db...");
            StatusLibrary.SetStage(status, 10);
            string path = Application.StartupPath + "\\db";

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
                MessageBox.Show(result, "Create Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }

            return 0;
        }

        public static async Task<int> DownloadMariaDB(CancellationToken ct)
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
                MessageBox.Show(result, "MariaDB Download", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            path += "\\mariadb-5.5.29-winx64.zip";
            if (!File.Exists(path))
            {
                StatusLibrary.SetStatusBar("downloading mariadb...");
                result = await UtilityLibrary.DownloadFile(ct, "https://archive.mariadb.org/mariadb-5.5.29/winx64-packages/mariadb-5.5.29-winx64.zip", path);
                if (result != "")
                {
                    result = $"failed to download mariadb from https://archive.mariadb.org/mariadb-5.5.29/winx64-packages/mariadb-5.5.29-winx64.zip: {result}";
                    StatusLibrary.SetStatusBar("downloading mariadb failed");
                    MessageBox.Show(result, "MariaDB Download", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }
            }
            StatusLibrary.SetStatusBar("downloaded mariadb");
            return 0;
        }

        public static async Task<int> ExtractMariaDB(CancellationToken ct)
        {
            Console.WriteLine("extracting mariadb...");
            StatusLibrary.SetStage(status, 30);

            if (File.Exists(Application.StartupPath + "\\db\\mariadb-5.5.29-winx64\\bin\\mysqld.exe"))
            {
                StatusLibrary.SetStatusBar("mariadb exists");
                return 0;
            }
            string result;
            string srcPath = Application.StartupPath + "\\cache";
            string dstPath = Application.StartupPath + "\\db";

            srcPath += "\\mariadb-5.5.29-winx64.zip";

            if (!File.Exists(srcPath))
            {
                result = $"failed to extract mariadb from cache/mariadb-5.5.29-winx64.zip: file not found";
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
                MessageBox.Show(result, "MariaDB Extract", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            StatusLibrary.SetStatusBar("extracted mariadb");
            return 0;
        }

        public static async Task<int> DownloadHeidi(CancellationToken ct)
        {
            Console.WriteLine("creating cache...");
            StatusLibrary.SetStage(status, 40);
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
                MessageBox.Show(result, "Heidi Download", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            path += "\\HeidiSQL_12.1_64_Portable.zip";
            if (!File.Exists(path))
            {
                StatusLibrary.SetStatusBar("downloading heidi...");
                result = await UtilityLibrary.DownloadFile(ct, "https://www.heidisql.com/downloads/releases/HeidiSQL_12.1_64_Portable.zip", path);
                if (result != "")
                {
                    result = $"failed to download heidi from https://www.heidisql.com/downloads/releases/HeidiSQL_12.1_64_Portable.zip: {result}";
                    StatusLibrary.SetStatusBar("downloading heidi failed");
                    MessageBox.Show(result, "Heidi Download", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }
            }

            StatusLibrary.SetStatusBar("downloaded heidi");
            return 0;
        }

        public static async Task<int> ExtractHeidi(CancellationToken ct)
        {
            Console.WriteLine("extracting heidi...");
            StatusLibrary.SetStage(status, 50);

            if (File.Exists(Application.StartupPath + "\\db\\heidi\\heidisql.exe"))
            {
                StatusLibrary.SetStatusBar("heidi exists");
                return 0;
            }
            string result;
            string srcPath = Application.StartupPath + "\\cache";
            string dstPath = Application.StartupPath + "\\db\\heidi";

            srcPath += "\\HeidiSQL_12.1_64_Portable.zip";

            if (!File.Exists(srcPath))
            {
                result = $"failed to extract heidi from cache/HeidiSQL_12.1_64_Portable.zip: file not found";
                StatusLibrary.SetStatusBar("extract heidi failed");
                MessageBox.Show(result, "Heidi Extract", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show(result, "Heidi Extract", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }

            StatusLibrary.SetStatusBar("extracted heidi");
            return 0;
        }


        public static async Task<int> DownloadHeidiSettings(CancellationToken ct)
        {
            StatusLibrary.SetStage(status, 60);
            string result;
            string path = Application.StartupPath + "\\db\\heidi";
            path += "\\portable_settings.txt";
            if (!File.Exists(path))
            {
                StatusLibrary.SetStatusBar("downloading heidi settings...");
                result = await UtilityLibrary.DownloadFile(ct, "https://raw.githubusercontent.com/xackery/emulauncher/main/launcher/Assets/portable_settings.txt", path);
                if (result != "")
                {
                    result = $"failed to download heidi settings from https://raw.githubusercontent.com/xackery/emulauncher/main/launcher/Assets/portable_settings.txt: {result}";
                    StatusLibrary.SetStatusBar("downloading heidi settings failed");
                    MessageBox.Show(result, "Heidi Settings Download", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }
            }
            path = Application.StartupPath + "\\db\\heidi\\portable.lock";
            try
            {
                File.Create(path);
            }
            catch (Exception ex)
            {
                result = $"failed to create {path}: {ex.Message}";
                StatusLibrary.SetStatusBar(result);
                MessageBox.Show(result, "Heidi Settings Lock Create", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            StatusLibrary.SetStatusBar("downloaded heidi settings");
            return 0;
        }
    }
}
