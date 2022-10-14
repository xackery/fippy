using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Xml.Linq;
using YamlDotNet.Core.Tokens;

namespace EQEmu_Launcher
{
    internal class Lua
    {

        public static void Check()
        {
            var status = StatusType.Lua;
            StatusLibrary.SetStage(status, 0);
            StatusLibrary.SetText(status, "lua not found");
            StatusLibrary.SetDescription(status, "Lua is a library used for quests. This is required to use PEQ's latest quest files.\nClicking Fix will download and install lua");
            StatusLibrary.SetIsFixNeeded(status, true);
        }
        public static void Fix()
        {
            Check();
        }
    }
}