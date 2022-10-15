using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EQEmu_Launcher
{
    internal class Map
    {
        private readonly static StatusType status = StatusType.Map;
        public static Task FixTask { get; private set; }

        public static void Check()
        {
            StatusLibrary.SetIsFixNeeded(status, true);
            string path = Application.StartupPath + "\\maps";
            if (!Directory.Exists(path))
            {
                StatusLibrary.SetText(status, "map subfolder not found");
                return;
            }

            StatusLibrary.SetIsFixNeeded(status, false);
            StatusLibrary.SetStage(status, 100);
            StatusLibrary.SetText(status, "maps found");
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

            int stage = FixPath(ct);
            if (stage == -1) {  return; }
            if (!fixAll && stage > startStage) {  return; }

            stage = await FixDownloadMaps(ct);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }
        }

        public static void FixAll()
        {
            Console.WriteLine("fixing all map issues");
            CancellationToken ct = new CancellationToken();
            FixTask = Task.Run(() => Fix(ct, true));
            Check();
        }

        public static int FixPath(CancellationToken ct)
        {
            Console.WriteLine("fixing path...");
            StatusLibrary.SetStage(status, 10);
            string path = Application.StartupPath + "\\maps";

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
                MessageBox.Show(result, "Maps Fix", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            
            return 0;
        }

        public static async Task<int> FixDownloadMaps(CancellationToken ct)
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
            path += "\\maps.zip";
            if (!File.Exists(path))
            {
                StatusLibrary.SetStatusBar("downloading maps...");
                result = await UtilityLibrary.DownloadFile(ct, "https://github.com/Akkadius/EQEmuMaps/archive/refs/heads/master.zip", path);
                if (result != "")
                {
                    result = $"failed to download maps from https://github.com/Akkadius/EQEmuMaps/archive/refs/heads/master.zip: {result}";
                    StatusLibrary.SetStatusBar("downloading maps failed");
                    MessageBox.Show(result, "Maps Fix", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }
            }
            StatusLibrary.SetStatusBar("downloaded maps");
            return 0;
        }
    }
}
