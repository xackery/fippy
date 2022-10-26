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
using MySqlConnector;
using System.Diagnostics;
using System.Xml.Linq;
using System.Net.Sockets;
using YamlDotNet.Core.Tokens;

namespace EQEmu_Launcher
{
    /* General Utility Methods */
    class UtilityLibrary
    {
        public static string EnvironmentPath()
        {
            return $"{Environment.GetEnvironmentVariable("PATH")};{Application.StartupPath}\\db\\mariadb-10.6.10-winx64\\bin;{Application.StartupPath}\\server\\perl\\perl\\bin";
        }

        public static async Task<int> Download(int startProgress, int endProgress, string source, string outDir, string fileName, int sizeMB)
        {
            string result;
            StatusLibrary.SetProgress(startProgress);
            string path = $"{Application.StartupPath}\\{outDir}\\{fileName}";

            string outFullDir = path.Substring(0, path.LastIndexOf("\\"));
            if (!Directory.Exists(outFullDir))
            {
                StatusLibrary.Log($"Creating directory {outFullDir}");
                Directory.CreateDirectory(outFullDir);
            }

            if (File.Exists(path))
            {
                StatusLibrary.SetStatusBar($"Skipping {outDir}\\{fileName} download, already exists");
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
                StatusLibrary.Log($"download source: {source}, destination: {path}");
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
            string srcPath = $"{Application.StartupPath}\\{srcDir}\\{fileName}";

            try
            {
                if (File.Exists(targetCheckPath))
                {
                    StatusLibrary.SetStatusBar($"Skipping {srcDir}\\{fileName} extract, already exists");
                    StatusLibrary.SetProgress(endProgress);
                    return endProgress;
                }

                StatusLibrary.SetStatusBar($"Extracting {fileName} ({sizeMB} MB) to {outDir}...");
                if (!Directory.Exists($"{Application.StartupPath}\\{outDir}"))
                {
                    StatusLibrary.Log($"{Application.StartupPath}\\{outDir} doesn't exist, creating it");
                    Directory.CreateDirectory($"{Application.StartupPath}\\{outDir}");
                }

                using (ZipArchive archive = ZipFile.OpenRead(srcPath))
                {
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
                        if (isReportNeeded)
                        {
                            StatusLibrary.SetProgress(value);
                        }

                        string zipPath = entry.FullName.Replace("/", "\\").Replace("EQEmuMaps-master\\", "").Replace("projecteqquests-master\\", "");
                        if (zipPath.StartsWith("c\\"))
                        {
                            zipPath = zipPath.Substring(2);
                        }
                        if (zipPath.Length == 0)
                        {
                            continue;
                        }
                        string outPath = $"{Application.StartupPath}\\{outDir}\\{zipPath}";

                        string zipDir = outPath.Substring(0, outPath.LastIndexOf("\\"));
                        if (!Directory.Exists(zipDir))
                        {
                            StatusLibrary.Log($"Creating directory {zipDir}");
                            Directory.CreateDirectory(zipDir);
                        }

                        if (outPath.EndsWith("\\"))
                        {
                            continue;
                        }
                        if (isReportNeeded)
                        {
                            StatusLibrary.SetStatusBar($"Extracting {outDir}\\{zipPath}...");
                            StatusLibrary.Log($"Extracting to {outPath}");
                        }
                        using (Stream zipStream = entry.Open())
                        {
                            FileStream fileStream = File.Create(outPath);
                            await zipStream.CopyToAsync(fileStream);
                        }
                    }
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

        public static async Task<bool> CreatePEQDB(int startProgress, int endProgress)
        {
            bool isDBCreated = false;
            string dbName = Config.Data?["server"]?["database"]?["db"];
            string user = "root";
            string password = Config.Data?["server"]?["database"]?["password"];
            StatusLibrary.Log($"Checking if {dbName} exists");
            MySqlConnection connection = new MySqlConnection($"Server=localhost;User ID={user};Password={password}");
            await connection.OpenAsync(StatusLibrary.CancelToken());

            MySqlCommand command = new MySqlCommand("SHOW databases;", connection);

            var reader = await command.ExecuteReaderAsync(StatusLibrary.CancelToken());
            while (await reader.ReadAsync(StatusLibrary.CancelToken()))
            {
                string line = reader.GetString(0);
                if (line.Equals(dbName))
                {
                    isDBCreated = true;
                    break;
                }
            }
            reader.Close();

            if (!isDBCreated)
            {
                StatusLibrary.Log($"Creating database {dbName}");
                command = new MySqlCommand($"CREATE DATABASE {dbName};", connection);
                reader = await command.ExecuteReaderAsync(StatusLibrary.CancelToken());
                while (await reader.ReadAsync())
                {
                    string line = reader.GetString(0);
                    StatusLibrary.Log(line);
                }
            }
            connection.Close();


            connection = new MySqlConnection($"Server=localhost;User ID={user};Password={password};Database={dbName}");
            await connection.OpenAsync(StatusLibrary.CancelToken());
            command = new MySqlCommand("SHOW tables;", connection);
            reader = await command.ExecuteReaderAsync(StatusLibrary.CancelToken());
            bool isTableOK = false;
            while (await reader.ReadAsync(StatusLibrary.CancelToken()))
            {
                string line = reader.GetString(0);
                if (line.Equals("account"))
                {
                    isTableOK = true;
                    break;
                }
            }
            reader.Close();
            connection.Close();

            if (isTableOK)
            {
                StatusLibrary.SetStatusBar("Database is already sourced, skipping");
                StatusLibrary.SetProgress(endProgress);
                return false;
            }
            return true;
        }

        public static async Task<int> SourcePEQDB(int startProgress, int endProgress)
        {

            try
            {
                StatusLibrary.SetProgress(startProgress);
                StatusLibrary.SetStatusBar("Sourcing database...");
                string path = $"{Application.StartupPath}\\server\\perl\\perl\\bin\\perl.exe";
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = $"{Application.StartupPath}\\server",
                        FileName = path,
                        Arguments = $"peq_download.pl source_peq_db",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    }
                };

                proc.StartInfo.EnvironmentVariables["PATH"] = EnvironmentPath();

                StatusLibrary.Log($"Running command {path} {proc.StartInfo.Arguments}");

                proc.OutputDataReceived += new DataReceivedEventHandler((object sender, DataReceivedEventArgs e) =>
                {
                    if (StatusLibrary.CancelToken().IsCancellationRequested)
                    {
                        StatusLibrary.Log("Killing peq downloader perl script, cancellation token invoked");
                        var process = Process.GetProcessById(proc.Id);
                        process.Kill();
                        return;
                    }
                    string line = e.Data;
                    if (line == null)
                    {
                        return;
                    }
                    StatusLibrary.Log($"peq: {line}");

                    if (line.Contains("create_tables_content"))
                    {
                        StatusLibrary.SetStatusBar("Database sourcing [create_tables_content.sql] (this will take a while)...");
                        StatusLibrary.SetProgress(startProgress + (int)((endProgress - startProgress) * 0.1));
                    }
                    if (line.Contains("create_tables_login.sql"))
                    {
                        StatusLibrary.SetStatusBar("Database sourcing [create_tables_login.sql]...");
                        StatusLibrary.SetProgress(startProgress + (int)((endProgress - startProgress) * 0.5));
                    }
                    if (line.Contains("create_tables_player.sql"))
                    {
                        StatusLibrary.SetStatusBar("Database sourcing [create_tables_player.sql]...");
                        StatusLibrary.SetProgress(startProgress + (int)((endProgress - startProgress) * 0.6));
                    }
                    if (line.Contains("create_tables_queryserv.sql"))
                    {
                        StatusLibrary.SetStatusBar("Database sourcing [create_tables_queryserv.sql]...");
                        StatusLibrary.SetProgress(startProgress + (int)((endProgress - startProgress) * 0.8));
                    }
                    if (line.Contains("create_tables_state.sql"))
                    {
                        StatusLibrary.SetStatusBar("Database sourcing [create_tables_state.sql]...");
                        StatusLibrary.SetProgress(startProgress + (int)((endProgress - startProgress) * 0.9));
                    }
                    if (line.Contains("create_tables_system.sql"))
                    {
                        StatusLibrary.SetStatusBar("Database sourcing [create_tables_system.sql]...");
                        StatusLibrary.SetProgress(startProgress + (int)((endProgress - startProgress) * 0.95));
                    }
                });
                proc.ErrorDataReceived += new DataReceivedEventHandler((object sender, DataReceivedEventArgs e) =>
                {
                    string line = e.Data;
                    if (line == null)
                    {
                        return;
                    }
                    StatusLibrary.Log($"peq error: {line}");
                });
                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

                proc.WaitForExit();
                StatusLibrary.SetStatusBar($"Injected successfully");
                StatusLibrary.SetProgress(endProgress);
                return endProgress;
            }
            catch (Exception ex)
            {
                string result = $"Failed to inject: {ex.Message}";
                StatusLibrary.SetStatusBar(result);
                MessageBox.Show(result, $"Inject PEQ DB", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        public static async Task<int> SourceDatabaseNotUsed(int startProgress, int endProgress, string srcDir, string fileName)
        {
            StatusLibrary.SetProgress(startProgress);
            string srcPath = $"{Application.StartupPath}\\{srcDir}\\{fileName}";

            try
            {
                bool isDBCreated = false;
                string dbName = Config.Data?["server"]?["database"]?["db"];
                string user = "root";
                string password = Config.Data?["server"]?["database"]?["password"];
                StatusLibrary.Log($"checking if {dbName} exists");
                MySqlConnection connection = new MySqlConnection($"Server=localhost;User ID={user};Password={password}"); //;Database={Config.Data?["server"]?["database"]?["db"]}
                await connection.OpenAsync(StatusLibrary.CancelToken());
                
                MySqlCommand command = new MySqlCommand("SHOW databases;", connection);

                var reader = await command.ExecuteReaderAsync(StatusLibrary.CancelToken());                
                while (await reader.ReadAsync(StatusLibrary.CancelToken()))
                {
                    string line = reader.GetString(0);
                    if (line.Equals(dbName))
                    {
                        isDBCreated = true;
                        break;
                    }
                }
                reader.Close();

                if (isDBCreated)
                {
                    connection.Close();
                    StatusLibrary.SetStatusBar($"Skipping source database {fileName}, already exists");
                    StatusLibrary.SetProgress(endProgress);
                    return endProgress;
                }

                command = new MySqlCommand($"CREATE DATABASE {dbName};", connection);
                reader = await command.ExecuteReaderAsync(StatusLibrary.CancelToken());
                while (await reader.ReadAsync())
                {
                    string line = reader.GetString(0);
                    StatusLibrary.Log(line);
                }
                reader.Close();
                connection.Close();

                ZipArchive archive = ZipFile.OpenRead(srcPath);
                int entryCount = archive.Entries.Count;
                int entryReportStep = 1;
                int entryReportCounter = 0;
                if (entryCount > 100)
                {
                    entryReportStep = 10;
                }


                List<ZipArchiveEntry> entries = new List<ZipArchiveEntry>();
                
                for (int i = 0; i < entryCount; i++)
                {
                    ZipArchiveEntry entry = archive.Entries[i];
                    string zipPath = entry.FullName.Replace("/", "\\").Replace("peq-dump\\", "");

                    if (zipPath.Contains("create_all_tables.sql")) {
                        entries.Insert(0, entry);
                    }

                    if (zipPath.Contains("create_tables_content") ||
                       zipPath.Contains("create_tables_login") ||
                       zipPath.Contains("create_tables_player") ||
                       zipPath.Contains("create_tables_queryserv") ||
                       zipPath.Contains("create_tables_state") ||
                       zipPath.Contains("create_tables_system")
                       )
                    {
                        continue;
                    }
                    entries.Add(entry);
                }

                for (int i = 0; i < entries.Count; i++)
                {
                    entryReportCounter++;
                    bool isReportNeeded = false;
                    if (entryReportCounter >= entryReportStep)
                    {
                        isReportNeeded = true;
                        entryReportCounter = 0;
                    }
                    
                    ZipArchiveEntry entry = entries[i];

                    int value = startProgress + (int)((endProgress - startProgress) * (float)((float)i / (float)entryCount));
                    if (isReportNeeded)
                    {
                        StatusLibrary.SetProgress(value);
                    }

                    string zipPath = entry.FullName.Replace("/", "\\").Replace("peq-dump\\", "");
                    if (zipPath.StartsWith("c\\"))
                    {
                        zipPath = zipPath.Substring(2);
                    }
                    if (zipPath.Length == 0)
                    {
                        continue;
                    }

                    string outPath = $"{Application.StartupPath}\\{srcDir}\\{zipPath}";

                    string zipDir = outPath.Substring(0, outPath.LastIndexOf("\\"));
                    if (!Directory.Exists(zipDir))
                    {
                        StatusLibrary.Log($"Creating directory {zipDir}");
                        Directory.CreateDirectory(zipDir);
                    }

                    if (outPath.EndsWith("\\"))
                    {
                        continue;
                    }
                    if (isReportNeeded)
                    {
                        StatusLibrary.SetStatusBar($"Injecting to SQL {zipPath}...");
                        StatusLibrary.Log($"Injecting to SQL {zipPath}");
                    }

                    entry.ExtractToFile(outPath);
                    string path = $"{Application.StartupPath}\\db\\mariadb-10.6.10-winx64\\bin\\mysqld.exe";
                    var proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = path,
                            Arguments = $"-u {user} -p{password} {dbName} < {outPath}",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        }
                    };
                    proc.OutputDataReceived += new DataReceivedEventHandler((object src, DataReceivedEventArgs earg) =>
                    {
                        string line = earg.Data;
                        if (line == null)
                        {
                            return;
                        }
                        StatusLibrary.Log($"sql: {line}");
                    });

                    proc.ErrorDataReceived += new DataReceivedEventHandler((object src, DataReceivedEventArgs earg) =>
                    {
                        string line = earg.Data;
                        if (line == null)
                        {
                            return;
                        }
                        StatusLibrary.Log($"sql error: {line}");
                    });

                    proc.Start();
                    proc.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                string result = $"Failed to inject {srcPath}: {ex.Message}";
                StatusLibrary.SetStatusBar(result);
                MessageBox.Show(result, $"Inject {fileName}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            StatusLibrary.SetStatusBar($"Injected {fileName} successfully");
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

        public static void TelnetSend(string message)
        {
            StatusLibrary.Log("Connecting to telnet...");
            using (TcpClient client = new TcpClient("127.0.0.1", 9000))
            {
                using (NetworkStream stream = client.GetStream())
                {
                    stream.ReadTimeout = 6000;
                    stream.WriteTimeout = 6000;

                    byte[] buf = new byte[1024];
                    int bufLen = stream.Read(buf, 0, buf.Length);
                    
                    string data = Encoding.ASCII.GetString(buf, 0, bufLen);
                    if (!data.Contains("Establishing connection"))
                    {
                        StatusLibrary.Log($"Unexpected telnet (wanted establishing connection): {data}");
                    }
                    
                    bufLen = stream.Read(buf, 0, buf.Length);
                    data = Encoding.ASCII.GetString(buf, 0, bufLen);
                    if (!data.Contains("Connection established from localhost, assuming admin"))
                    {
                        StatusLibrary.Log($"Unexpected telnet (wanted admin note): {data}");
                    }

                    bufLen = stream.Read(buf, 0, buf.Length);
                    data = Encoding.ASCII.GetString(buf, 0, bufLen);
                    if (!data.Contains("Connection established from localhost, assuming admin"))
                    {
                        StatusLibrary.Log($"Unexpected telnet (wanted admin note): {data}");
                    }
                    
                    stream.Write(Encoding.ASCII.GetBytes("echo off\n"), 0, 8);
                    bufLen = stream.Read(buf, 0, buf.Length);
                    data = Encoding.ASCII.GetString(buf, 0, bufLen);
                    if (!data.Contains(">"))
                    {
                        StatusLibrary.Log($"Unexpected telnet (wanted >): {data}");
                    }
                    stream.Write(Encoding.ASCII.GetBytes(message+"\n"), 0, message.Length+1);
                    bufLen = stream.Read(buf, 0, buf.Length);
                    data = Encoding.ASCII.GetString(buf, 0, bufLen);
                    if (!data.Contains(">"))
                    {
                        StatusLibrary.Log($"Unexpected telnet (wanted >): {data}");
                    }
                    Thread.Sleep(1000);
                    bufLen = stream.Read(buf, 0, buf.Length);
                    data = Encoding.ASCII.GetString(buf, 0, bufLen);
                    StatusLibrary.Log($"Telnet response: {data}");
                }
            }
        }
    }
}
