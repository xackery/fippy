using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Runtime.InteropServices;

namespace EQEmu_Launcher
{
    /* General Utility Methods */
    class UtilityLibrary
    {
        //Download a file to current directory
        public static async Task<string> DownloadFile(CancellationToken ct, string url, string outFile)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    ct.Register(client.CancelAsync);
                    await client.DownloadFileTaskAsync(url, outFile);
                }
            } catch( IOException ie)
            {                
                return "IOException: "+ie.Message;
            } catch (WebException we) {
                if (we.Message == "The remote server returned an error: (404) Not Found.")
                {
                    return "404";
                }
                return "WebException: "+we.Message;  
            } catch (Exception e)
            {
                return "Exception: " + e.Message;
            }
            return "";
        }

        public static string GetMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    
                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < hash.Length; i++)
                    {
                        sb.Append(hash[i].ToString("X2"));
                    }

                    return sb.ToString();
                }
            }
        }

        public static string GetJson(string urlPath)
        {
            using (WebClient wc = new WebClient())
            {
                return wc.DownloadString(urlPath);
            }
        }

        public static System.Diagnostics.Process StartEverquest()
        {
            return System.Diagnostics.Process.Start("eqgame.exe", "patchme");
        }


        public static string GetSHA1(string filePath)
        {
            //SHA1 sha = new SHA1CryptoServiceProvider();            
            //var stream = File.OpenRead(filePath);
            //return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty); ;
            /*Encoding enc = Encoding.UTF8;

            var sha = SHA1.Create();
            var stream = File.OpenRead(filePath);

            string hash = "commit " + stream.Length + "\0";
            return enc.GetString(sha.ComputeHash(stream));

            return BitConverter.ToString(sha.ComputeHash(stream));*/
            Encoding enc = Encoding.UTF8;

            string commitBody = File.OpenText(filePath).ReadToEnd();
            StringBuilder sb = new StringBuilder();
            sb.Append("commit " + Encoding.UTF8.GetByteCount(commitBody));
            sb.Append("\0");
            sb.Append(commitBody);

            var sss = SHA1.Create();
            var bytez = Encoding.UTF8.GetBytes(sb.ToString());
            return BitConverter.ToString(sss.ComputeHash(bytez));
            //var myssh = enc.GetString(sss.ComputeHash(bytez));
            //return myssh;
        }

        //Pass the working directory (or later, you can pass another directory) and it returns a hash if the file is found
        public static string GetEverquestExecutableHash(string path)
        {
            var di = new System.IO.DirectoryInfo(path);
            var files = di.GetFiles("eqgame.exe");
            if (files == null || files.Length == 0)
            {
                return "";
            }
            return UtilityLibrary.GetMD5(files[0].FullName);
        }

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void DwmSetWindowAttribute(IntPtr hwnd,
                                                DWMWINDOWATTRIBUTE attribute,
                                                ref int pvAttribute,
                                                uint cbAttribute);

        public enum DWMWINDOWATTRIBUTE : uint
        {
            DWMWA_NCRENDERING_ENABLED,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_PASSIVE_UPDATE_MODE,
            DWMWA_USE_HOSTBACKDROPBRUSH,
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_WINDOW_CORNER_PREFERENCE = 33,
            DWMWA_BORDER_COLOR,
            DWMWA_CAPTION_COLOR,
            DWMWA_TEXT_COLOR,
            DWMWA_VISIBLE_FRAME_BORDER_THICKNESS,
            DWMWA_SYSTEMBACKDROP_TYPE,
            DWMWA_LAST
        }
    }
}
