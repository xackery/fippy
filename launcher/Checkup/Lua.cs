using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using YamlDotNet.Core.Tokens;

namespace EQEmu_Launcher
{
    internal class Lua
    {
        private readonly static StatusType status = StatusType.Lua;
        public static Task FixTask { get; private set; }

        public static void Check()
        {
            StatusLibrary.SetStage(status, 0);
            StatusLibrary.SetText(status, "lua not found");
            StatusLibrary.SetDescription(status, "Lua is a library used for quests. This is required to use PEQ's latest quest files.\nClicking Fix will download and install lua");
            StatusLibrary.SetIsFixNeeded(status, true);
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
        }

        public static void FixAll()
        {
            Check();
        }
    }
}