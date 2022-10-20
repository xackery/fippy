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

        public static int Check()
        {
            StatusLibrary.SetIsFixNeeded(status, true);
            string path = $"{Application.StartupPath}\\server\\maps\\base\\nro.map";
            if (!Directory.Exists(path))
            {
                StatusLibrary.SetText(status, "maps not found");
                return 0;
            }

            StatusLibrary.SetText(status, "maps found");
            return 100;
        }

        public static void FixCheck()
        {
            FixTask = Task.Run(() => { StatusLibrary.LockUI(); Fix(false); Check(); StatusLibrary.UnlockUI(); });
        }

        public static async void Fix(bool fixAll)
        {
            int startStage = Check();
            int stage = 0;

            stage = await UtilityLibrary.Download(40, 80, "https://github.com/Akkadius/EQEmuMaps/archive/refs/heads/master.zip", "cache", "maps.zip", 1046);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }

            stage = await UtilityLibrary.Extract(40, 80, "cache", "maps.zip", "server\\maps", $"{Application.StartupPath}\\server\\maps\\base\\nro.map", 1046);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }
        }

        public static void FixAll()
        {
            FixTask = Task.Run(() => { StatusLibrary.LockUI(); Fix(true); Check(); StatusLibrary.UnlockUI(); });
        }

    }
}
