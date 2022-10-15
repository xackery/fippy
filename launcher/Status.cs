using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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
        Database, // checkup section for database prep work
    }

    /// <summary>
    /// StatusLibrary is used to manage the various statuses tracked in launcher and is thread safe
    /// </summary>
    internal class StatusLibrary
    {        
        readonly static Mutex mux = new Mutex();

        readonly static Dictionary<StatusType, Status> checks = new Dictionary<StatusType, Status>();

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

        public static int Stage(StatusType name)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"status get stage for {name} not found in dictionary");
            }
            int value = checks[name].Stage;
            mux.ReleaseMutex();
            return value;
        }

        public static void SetStage(StatusType name, int value)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"status set stage for {name} not found in dictionary");
            }

            checks[name].Stage = value;
            mux.ReleaseMutex();
        }

        public static void SubscribeStage(StatusType name, EventHandler<int> f)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"status subscribe stage for {name} not found in dictionary");
            }
            Status status = checks[name];
            status.StageChange += f;
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
                throw new System.Exception($"status set descriptionfor {name} not found in dictionary");
            }

            checks[name].Description = value;
            mux.ReleaseMutex();
        }

        public static string Link(StatusType name)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"status get link for {name} not found in dictionary");
            }
            string value = checks[name].Link;
            mux.ReleaseMutex();
            return value;
        }

        public static void SetLink(StatusType name, string value)
        {
            mux.WaitOne();
            if (!checks.ContainsKey(name))
            {
                mux.ReleaseMutex();
                throw new System.Exception($"status set link for {name} not found in dictionary");
            }

            checks[name].Link = value;
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

            public bool IsFixNeeded { get { return stage != 100; } set { stage = 100; IsFixNeededChange?.BeginInvoke(this, value, null, null); } }
            public event EventHandler<bool> IsFixNeededChange;

            int stage;
            public int Stage { get { return stage; } set { stage = value; StageChange?.BeginInvoke(this, value, null, null); if (stage == 100) IsFixNeeded = false; } }
            public event EventHandler<int> StageChange;

            string description;
            public string Description { get { return description; } set { description = value; } }

            string link;
            public string Link { get { return link; } set { link = value; } }
        }
    }
}
