using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Net.NetworkInformation;
using System.Threading;
using System.Xml.Linq;
using YamlDotNet.Core.Tokens;

namespace EQEmu_Launcher
{

    enum StatusType
    {        
        Lua, // checkup section for lua prep work
        Quest, // checkup section for quest prep work
        Perl, // checkup section for perl prep work
        Map, // checkup section for maps prep work
        Server, // checkup section for server binaries prep work
        StatusBar, // controls the bottom status bar text
        SQL, // manage section used for sql status text
        Zone, // manage section used for zone status text
        World, // manage section used for world status text
        UCS, // manage section used for ucs status text
        QueryServ, // manage section used for queryServ status text
        SharedMemory, // manage section used for sharedMemory status text
        Database, // checkup section for database prep work
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

        public delegate void DescriptionHandler(string value);
        static event DescriptionHandler descriptionChange;


        /// <summary>
        /// When the UI is locked/unlocked, cancellation is fired. This is a thread safe operation to access
        /// </summary>
        static CancellationTokenSource cancelTokenSource;

        public static void Initialize()
        {
            foreach (StatusType key in System.Enum.GetValues(typeof(StatusType)))
            {
                Status value = new Status
                {
                    Name = key.ToString()
                };
                Add(key, value);
            }
        }

        public static Status Get(StatusType name)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"status get {name} invalid, not found in dictionary");
            }
            mux.ReleaseMutex();
            return checks[name];
        }

        public static void Add(StatusType name, Status value)
        {
            mux.WaitOne();
            if (checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"status add {name} already exists in dictionary");
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
            Console.WriteLine("UnlockUI called");
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
            return cancelTokenSource.Token;
        }


        public static bool IsFixNeeded(StatusType name)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"status get isfixneeded for {name} not found in dictionary");
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
                mux.ReleaseMutex();
                throw new System.Exception($"status set text for {name} not found in dictionary");
            }

            checks[name].Text = value;
            mux.ReleaseMutex();
        }

        public static void SetIsFixNeeded(StatusType name, bool value)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"status set isfixneeded for {name} not found in dictionary");
            }

            checks[name].IsFixNeeded = value;
            mux.ReleaseMutex();
        }

        public static void SubscribeIsFixNeeded(StatusType name, EventHandler<bool> f)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"status subscribe stage for {name} not found in dictionary");
            }
            Status status = checks[name];
            status.IsFixNeededChange += f;
            mux.ReleaseMutex();
        }

        public static string Name(StatusType name)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"status get name for {name} not found in dictionary");
            }
            string value = checks[name].Name;
            mux.ReleaseMutex();
            return value;
        }

        public static void SetName(StatusType name, string value)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"status set name for {name} not found in dictionary");
            }

            checks[name].Name = value;
            mux.ReleaseMutex();
        }

        public static void SubscribeName(StatusType name, EventHandler<string> f)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"status get text for {name} not found in dictionary");
            }
            Status status = checks[name];
            status.NameChange += f;
            mux.ReleaseMutex();
        }


        public static string Text(StatusType name)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"status get text for {name} not found in dictionary");
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
                mux.ReleaseMutex();
                throw new System.Exception($"status set text for {name} not found in dictionary");
            }

            checks[name].Text = value;
            mux.ReleaseMutex();
        }

        public static void SubscribeText(StatusType name, EventHandler<string> f)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"status subscribe stage for {name} not found in dictionary");
            }
            Status status = checks[name];
            status.TextChange += f;
            mux.ReleaseMutex();
        }

        public static void SetProgress(int value)
        {
            mux.WaitOne();
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
                throw new System.Exception($"status get description for {name} not found in dictionary");
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
                throw new System.Exception($"status set description for {name} not found in dictionary");
            }

            checks[name].Description = value;
            mux.ReleaseMutex();
        }

        /// <summary>
        /// Status represents a specific status of a tracked object, and is accessed via the StatusLibrary
        /// </summary>
        internal class Status
        {
            string name;
            public string Name { get { return name; } set { name = value; NameChange?.BeginInvoke(this, value, null, null); } }
            public event EventHandler<string> NameChange;

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
