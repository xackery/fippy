using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Core.Tokens;

namespace EQEmu_Launcher
{


    internal class Config
    {
        public static dynamic Data;

        public delegate void NullHandler();
        static event NullHandler loadChanged;

        public static void Load()
        {
            try
            {
                string content = File.ReadAllText($"{Application.StartupPath}\\server\\eqemu_config.json");
                Data = JObject.Parse(content);
                loadChanged?.BeginInvoke(null, null);
            } catch (Exception ex)
            {
                StatusLibrary.Log($"Config load failed: {ex.Message}");
            }
        }

        public static void Save()
        {
            try 
            { 
                File.WriteAllText($"{Application.StartupPath}\\server\\eqemu_config.json", Data.ToString());
            }
            catch (Exception ex)
            {
                StatusLibrary.Log($"Config save failed: {ex.Message}");
            }
        }

        public static void SubscribeOnLoad(NullHandler f)
        {
            loadChanged += f;
        }

    }
}
