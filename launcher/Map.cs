using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EQEmu_Launcher
{
    internal class Map
    {
        public static void Check()
        {
            StatusType status = StatusType.Map;
            StatusLibrary.SetIsFixNeeded(status, true);

            string path = Application.StartupPath + "\\maps";
            if (!Directory.Exists(path))
            {
                StatusLibrary.SetText(status, "map subfolder not found");
                return;
            }

            StatusLibrary.SetIsFixNeeded(status, false);
            StatusLibrary.SetText(status, "maps found");
        }

        public static void Fix()
        {
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
                Check();
                return;
            }

            Check();
        }
    }
}
