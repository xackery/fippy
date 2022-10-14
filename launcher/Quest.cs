using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using YamlDotNet.Core.Tokens;
using System.Windows.Forms;

namespace EQEmu_Launcher
{
    public class Quest
    {
        /// <summary>
        /// Check the status of deployment
        /// </summary>
        public static void Check()
        {
            StatusType status = StatusType.Quest;
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

        public static void Fix()
        {
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
                MessageBox.Show(result, "Quest Fix", MessageBoxButtons.OK, MessageBoxIcon.Error);

                Check();
                return;
            }

            Check();
        }
    }
}