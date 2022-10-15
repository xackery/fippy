using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EQEmu_Launcher
{


    internal class Config
    {
        public static dynamic Data;
        public static void Load()
        {
            try
            {
                string content = File.ReadAllText($"{Application.StartupPath}\\server\\eqemu_config.json");
                Data = JObject.Parse(content);
            } catch (Exception ex)
            {
                Console.WriteLine($"config load failed: {ex.Message}");
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
                Console.WriteLine($"config save failed: {ex.Message}");
            }
        }

    }
}
