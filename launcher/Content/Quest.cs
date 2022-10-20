using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using YamlDotNet.Core.Tokens;
using System.Windows.Forms;
using System.Threading.Tasks;
using EQEmu_Launcher;

namespace EQEmu_Launcher
{
    public class Quest
    {
        private readonly static StatusType status = StatusType.Quest;
        public static Task FixTask { get; private set; }

        /// <summary>
        /// Check the status of deployment
        /// </summary>
        public static int Check()
        {
            StatusLibrary.SetIsFixNeeded(status, true);
            string path;

            path = Application.StartupPath + "\\server\\quests\\perl\\bin\\perl.exe";
            if (!File.Exists(path))
            {
                StatusLibrary.SetText(status, "perl not found");
                return 0;
            }

            StatusLibrary.SetIsFixNeeded(status, false);
            StatusLibrary.SetProgress(100);
            StatusLibrary.SetText(status, "quests found");
            return 100;
        }

        public static void FixCheck()
        {
            Console.WriteLine("running fix check");
            CancellationToken ct = new CancellationToken();
            FixTask = Task.Run(() => { Fix(ct, false);  Check();});
        }

        public static async void Fix(CancellationToken ct, bool fixAll)
        {
            int startStage = Check();
            int stage = FixPath(ct);
            if (stage == -1) { return; }
            if (!fixAll && stage > startStage) { return; }
        }
        public static void FixAll()
        {
            Console.WriteLine("fixing all quest issues");
            CancellationToken ct = new CancellationToken();
            FixTask = Task.Run(() => { Fix(ct, true);  Check();});
        }

        public static int FixPath(CancellationToken ct)
        {
            Console.WriteLine("fixing path quests...");
            StatusLibrary.SetProgress(10);
            string path = Application.StartupPath + "\\quests";

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
    }
}