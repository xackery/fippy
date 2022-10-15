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
        public static void Check()
        {
            StatusLibrary.SetIsFixNeeded(status, true);

            string path = Application.StartupPath + "\\quests";
            if (!Directory.Exists(path))
            {
                StatusLibrary.SetText(status, "quests subfolder not found");
                StatusLibrary.SetDescription(status, "Emu Launcher could not find a quests folder where the executable was ran from.\nIf you have existing quests, you can copy them manually.\nIf you would like emu launcher to repair and download the latest peq quests, click Fix.");
                return;
            }

            StatusLibrary.SetIsFixNeeded(status, false);
            StatusLibrary.SetText(status, "quests found");
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

        public static int FixPath(CancellationToken ct)
        {
            Console.WriteLine("fixing path quests...");
            StatusLibrary.SetStage(status, 10);
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