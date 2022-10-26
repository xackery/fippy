using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using YamlDotNet.Core.Tokens;

namespace EQEmu_Launcher
{

    enum StatusType
    {
        Server, // checkup section for server binaries prep work
        StatusBar, // controls the bottom status bar text
        SQL, // manage section used for sql status text
        Zone, // manage section used for zone status text
        World, // manage section used for world status text
        UCS, // manage section used for ucs status text
        QueryServ, // manage section used for queryServ status text
        SharedMemory, // manage section used for sharedMemory status text
        Database, // checkup section for database prep work
        Quest,
        Map,
    }

    /// <summary>
    /// StatusLibrary is used to manage the various statuses tracked in launcher and is thread safe
    /// </summary>
    internal class StatusLibrary
    {        
        readonly static Mutex mux = new Mutex();

        readonly static Dictionary<StatusType, Status> checks = new Dictionary<StatusType, Status>();

        public delegate void ProgressHandler(int value);
        static event ProgressHandler progressChange;
        static int progressValue;

        public delegate void DescriptionHandler(string value);
        static event DescriptionHandler descriptionChange;

        /// <summary>
        /// When the UI is locked/unlocked, cancellation is fired. This is a thread safe operation to access
        /// </summary>
        static CancellationTokenSource cancelTokenSource;

        public static void InitLog() 
        {
            mux.WaitOne();
            using (var logw = File.Create("fippy.log"))
            {
                string dirName = new DirectoryInfo($"{Application.StartupPath}").Name;
                string rawMessage = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ff")} INFO Fippy Darklauncher v{Assembly.GetEntryAssembly().GetName().Version} ({dirName} Folder)\n";
                logw.Write(Encoding.ASCII.GetBytes(rawMessage), 0, rawMessage.Length);
                logw.Flush();                               
            }
            mux.ReleaseMutex();
        }

        public static void Log(string message)
        {   
            mux.WaitOne();
            try
            {
                using (var logw = File.Open("fippy.log", FileMode.Append))
                {
                    string rawMessage = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ff")} INFO {message}\n";
                    logw.Write(Encoding.ASCII.GetBytes(rawMessage), 0, rawMessage.Length);
                    logw.Flush();
                }
            } catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log: {ex.Message}");
            }
            Console.WriteLine(message);
            mux.ReleaseMutex();
        }

        public static Status Get(StatusType name)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                checks[name] = new Status();
            }
            mux.ReleaseMutex();
            return checks[name];
        }

        public static void Add(StatusType name, Status value)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                checks[name] = new Status();
            }
            checks[name] = value;
            mux.ReleaseMutex();
        }

        /// <summary>
        /// LockUI should be called before doing any Fix or Download operation
        /// </summary>
        public static void LockUI()
        {
            mux.WaitOne();
            if (cancelTokenSource == null)
            {
                cancelTokenSource = new CancellationTokenSource();
                mux.ReleaseMutex();
                return;
            }
            mux.ReleaseMutex();
        }

        /// <summary>
        /// UnlockUI cancels any currently running operations and restores UI
        /// </summary>
        public static void UnlockUI()
        {
            mux.WaitOne();
            StatusLibrary.Log("UnlockUI called");
            if (cancelTokenSource != null)
            {
                cancelTokenSource.Cancel();
            }
            cancelTokenSource = new CancellationTokenSource();
            SetProgress(100);
            mux.ReleaseMutex();
        }

        /// <summary>
        /// Returns the current CancellationToken. No mutex lock occurs since it is thread safe
        /// </summary>
        public static CancellationToken CancelToken()
        {
            if (cancelTokenSource == null)
            {
                mux.WaitOne();
                cancelTokenSource = new CancellationTokenSource();
                mux.ReleaseMutex();
            }
            return cancelTokenSource.Token;
        }


        public static bool IsFixNeeded(StatusType name)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                checks[name] = new Status();
            }
            Status status = checks[name];
            bool isFixNeeded = status.IsFixNeeded;
            
            mux.ReleaseMutex();
            return isFixNeeded;
        }

        public static void SetStatusBar(string value)
        {
            StatusType name = StatusType.StatusBar;
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                checks[name] = new Status();
            }

            checks[name].Text = value;
            mux.ReleaseMutex();
        }


        public static void SetIsEnabled(StatusType name, bool value)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                checks[name] = new Status();
            }

            checks[name].IsEnabled = value;
            mux.ReleaseMutex();
        }

        public static void SubscribeIsEnabled(StatusType name, EventHandler<bool> f)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                checks[name] = new Status();
            }
            Status status = checks[name];
            status.IsEnabledChange += f;
            mux.ReleaseMutex();
        }

        public static void SetIsFixNeeded(StatusType name, bool value)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                checks[name] = new Status();
            }

            checks[name].IsFixNeeded = value;
            mux.ReleaseMutex();
        }

        public static void SubscribeIsFixNeeded(StatusType name, EventHandler<bool> f)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                checks[name] = new Status();
            }
            Status status = checks[name];
            status.IsFixNeededChange += f;
            mux.ReleaseMutex();
        }

        public static string Text(StatusType name)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                checks[name] = new Status();
            }
            string value = checks[name].Text;
            mux.ReleaseMutex();
            return value;
        }

        public static void SetText(StatusType name, string value)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                checks[name] = new Status();
            }

            checks[name].Text = value;
            mux.ReleaseMutex();
        }

        public static void SubscribeText(StatusType name, EventHandler<string> f)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                checks[name] = new Status();                
            }
            Status status = checks[name];
            status.TextChange += f;
            mux.ReleaseMutex();
        }

        public static int Progress()
        {
            mux.WaitOne();
            int value = progressValue;
            mux.ReleaseMutex();
            return value;
        }

        public static void SetProgress(int value)
        {
            mux.WaitOne();
            progressValue = value;
            progressChange?.BeginInvoke(value, null, null);
            mux.ReleaseMutex();
        }

        public static void SubscribeProgress(ProgressHandler f)
        {
            mux.WaitOne();
            progressChange += f;
            mux.ReleaseMutex();
        }

        public static void SetDescription(string value)
        {
            mux.WaitOne();
            descriptionChange?.BeginInvoke(value, null, null);
            mux.ReleaseMutex();
        }

        public static void SubscribeDescription(DescriptionHandler f)
        {
            mux.WaitOne();
            descriptionChange += f;
            mux.ReleaseMutex();
        }

        public static string Description(StatusType name)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"Status get description for {name} not found in dictionary");
            }
            string value = checks[name].Description;
            mux.ReleaseMutex();
            return value;
        }

        public static void SetDescription(StatusType name, string value)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"Status set description for {name} not found in dictionary");
            }

            checks[name].Description = value;
            mux.ReleaseMutex();
        }

        /// <summary>
        /// Status represents a specific status of a tracked object, and is accessed via the StatusLibrary
        /// </summary>
        internal class Status
        {
            bool isLocked;
            public bool IsEnabled { get { return isLocked; } set { isLocked = value; IsEnabledChange?.BeginInvoke(this, value, null, null); } }
            public event EventHandler<bool> IsEnabledChange;

            string text;
            public string Text { get { return text; } set { text = value; TextChange?.BeginInvoke(this, value, null, null); } }
            public event EventHandler<string> TextChange;

            bool isFixNeeded;
            public bool IsFixNeeded { get { return isFixNeeded; } set { isFixNeeded = value; IsFixNeededChange?.BeginInvoke(this, value, null, null); } }
            public event EventHandler<bool> IsFixNeededChange;

            string description;
            public string Description { get { return description; } set { description = value; } }
        }
    }
}
