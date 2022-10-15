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

            StatusLibrary.SetIsFixNeeded(status, false);
            StatusLibrary.SetText(status, "database found");
        }

        public static void FixCheck()
        {
            Console.WriteLine("running fix check");
            CancellationToken ct = new CancellationToken();
            FixTask = Task.Run(() => Fix(ct, true));
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
    }
}
