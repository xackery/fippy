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
using System.Windows.Forms;
using System.IO.Compression;

namespace EQEmu_Launcher
{
    /* General Utility Methods */
    class UtilityLibrary
    {

        public static async Task<int> Download(int startProgress, int endProgress, string source, string outDir, string fileName, int sizeMB)
        {
            string result;
            StatusLibrary.SetProgress(startProgress);
            string path = $"{Application.StartupPath}\\{outDir}\\{fileName}";

            if (File.Exists(path))
            {
                StatusLibrary.SetStatusBar($"Skipping \\{outDir}\\{fileName} download, already exists");
                return endProgress;
            }

            StatusLibrary.SetStatusBar($"Downloading {fileName} ({sizeMB} MB) to {outDir}...");
            try
            {
                WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;
                StatusLibrary.CancelToken().Register(client.CancelAsync);
                client.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) => {
                    StatusLibrary.SetProgress(startProgress + (int)((endProgress - startProgress) * (float)((float)e.ProgressPercentage / (float)100)));
                };
                Console.WriteLine($"download source: {source}, destination: {path}");
                await client.DownloadFileTaskAsync(source, path);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("request was cancelled"))
                {
                    if (File.Exists($"{outDir}\\{fileName}")) {
                        File.Delete($"{outDir}\\{fileName}");
                    }
                    StatusLibrary.SetStatusBar("Cancelled request");
                    return -1;
                }
                result = $"Failed to download {fileName}: {ex.Message}";
                StatusLibrary.SetStatusBar(result);
                MessageBox.Show(result, "Download {fileName}", MessageBoxButtons.OK, MessageBoxIcon.Error);                    
                return -1;
            }
            StatusLibrary.SetStatusBar($"Downloaded {fileName} successfully");
            StatusLibrary.SetProgress(endProgress);
            return endProgress;
        }

        public static async Task<int> Extract(int startProgress, int endProgress, string srcDir, string fileName, string outDir, string targetCheckPath, int sizeMB)
        {
            StatusLibrary.SetProgress(startProgress);
            StatusLibrary.SetStatusBar($"Extracting {fileName} ({sizeMB} MB) to {outDir}...");
            string srcPath = $"{Application.StartupPath}\\{srcDir}\\{fileName}";
            
            try
            {
                if (File.Exists(targetCheckPath))
                {
                    Console.WriteLine("File already exists, skipping extract");
                    StatusLibrary.SetStatusBar($"Extracted {fileName} successfully (skipped)");
                    StatusLibrary.SetProgress(endProgress);
                    return endProgress;
                }

                if (!Directory.Exists($"{Application.StartupPath}\\{outDir}"))
                {
                    Console.WriteLine($"{Application.StartupPath}\\{outDir} doesn't exist, creating it");
                    Directory.CreateDirectory($"{Application.StartupPath}\\{outDir}");
                }


                ZipArchive archive = ZipFile.OpenRead(srcPath);
                int entryCount = archive.Entries.Count;
                int entryReportStep = 1;
                int entryReportCounter = 0;
                if (entryCount > 100)
                {
                    entryReportStep = 10;
                }

                for (int i = 0; i < entryCount; i++)
                {
                    entryReportCounter++;
                    bool isReportNeeded = false;
                    if (entryReportCounter >= entryReportStep)
                    {
                        isReportNeeded = true;
                        entryReportCounter = 0;
                    }
                    ZipArchiveEntry entry = archive.Entries[i];

                    int value = startProgress + (int)((endProgress - startProgress) * (float)((float)i / (float)entryCount));
                    if (isReportNeeded) {
                        StatusLibrary.SetProgress(value);
                    }

                    string zipPath = entry.FullName.Replace("/", "\\");
                    if (zipPath.StartsWith("c\\"))
                    {
                        zipPath = zipPath.Substring(2);
                    }
                    if (zipPath.Length == 0)
                    {
                        continue;
                    }
                    string outPath = $"{Application.StartupPath}\\{outDir}\\{zipPath}";
                                        
                    if (zipPath.EndsWith("\\"))
                    {
                        Console.WriteLine($"Creating directory to {outPath}");
                        Directory.CreateDirectory(outPath);
                        continue;
                    }
                    if (isReportNeeded)
                    {
                        StatusLibrary.SetStatusBar($"Extracting {outDir}\\{zipPath}...");
                        Console.WriteLine($"Extracting to {outPath}");
                    }
                    Stream zipStream = entry.Open();
                    FileStream fileStream = File.Create(outPath);
                    await zipStream.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                string result = $"Failed to extract {srcPath}: {ex.Message}";
                StatusLibrary.SetStatusBar(result);
                MessageBox.Show(result, $"Extract {fileName}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            StatusLibrary.SetStatusBar($"Extracted {fileName} successfully");
            StatusLibrary.SetProgress(endProgress);
            return endProgress;
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

        
    }
}
