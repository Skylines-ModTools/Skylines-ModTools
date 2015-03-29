using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ColossalFramework.UI;
using UnityEngine;

namespace ModTools
{

    public class ConsoleMessage
    {
        public string caller;
        public string message;
        public LogType type;
        public int count;
        public StackTrace trace;
    }

    public class Console : GUIWindow
    {

        private static Configuration config
        {
            get { return ModTools.Instance.config; }
        }

        private GUIArea headerArea;
        private GUIArea consoleArea;
        private GUIArea commandLineArea;

        private float headerHeightCompact = 0.5f;
        private float headerHeightExpanded = 8.0f;
        private bool headerExpanded = false;

        private float commandLineAreaHeightCompact = 45.0f;
        private float commandLineAreaHeightExpanded = 120.0f;

        private bool commandLineAreaExpanded 
        {
            get 
            {
                if (commandHistory[currentCommandHistoryIndex].Contains('\n'))
                {
                    return true;
                }

                return commandHistory[currentCommandHistoryIndex].Length >= 64;
            }
        }

        private object historyLock = new object();
        private List<ConsoleMessage> history = new List<ConsoleMessage>();
        private List<string> commandHistory = new List<string>() { "" };
        private int currentCommandHistoryIndex = 0;

        private Vector2 consoleScrollPosition = Vector2.zero;
        private Vector2 commandLineScrollPosition = Vector2.zero;

        private DebugOutputPanel vanillaPanel;
        private Transform oldVanillaPanelParent;

        private List<KeyValuePair<int, string>> userNotifications; 

        public Console() : base("Debug console", config.consoleRect, skin)
        {
            onDraw = DrawWindow;
            onException = HandleException;
            onUnityDestroy = HandleDestroy;

            headerArea = new GUIArea(this);
            consoleArea = new GUIArea(this);
            commandLineArea = new GUIArea(this);

            RecalculateAreas();

            vanillaPanel = UIView.library.Get<DebugOutputPanel>("DebugOutputPanel");
            oldVanillaPanelParent = vanillaPanel.transform.parent;
            vanillaPanel.transform.parent = transform;
        }

        void HandleDestroy()
        {
            vanillaPanel.transform.parent = oldVanillaPanelParent;
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.Tab))
            {
                UnityEngine.Debug.LogWarning("Tab");
            }
        }

        public void AddMessage(string message, LogType type = LogType.Log, bool global = false, bool _internal = false)
        {
            lock (historyLock)
            {
                if (history.Count > 0)
                {
                    var last = history.Last();
                    if (message == last.message && type == last.type)
                    {
                        last.count++;
                        return;
                    }
                }
            }

            string caller = "";

            StackTrace trace = new StackTrace();

            if (!global)
            {
                int i;
                for (i = 0; i < trace.FrameCount; i++)
                {
                    MethodBase callingMethod = null;

                    var frame = trace.GetFrame(i);
                    if (frame != null)
                    {
                        callingMethod = frame.GetMethod();
                    }

                    if (callingMethod == null)
                    {
                        continue;
                    }

                    if (callingMethod.DeclaringType != null)
                    {
                        var typeName = callingMethod.DeclaringType.ToString();
                        if (typeName.StartsWith("UnityEngine"))
                        {
                            continue;
                        }

                        if (typeName.StartsWith("ModTools"))
                        {
                            continue;
                        }

                        caller = String.Format("{0}.{1}()", callingMethod.DeclaringType, callingMethod.Name);
                    }
                    else
                    {
                        caller = String.Format("{0}()", callingMethod.ToString());

                    }

                    break;
                }
            }
            else
            {
                caller = _internal ? "ModTools" : "";
            }

            lock (historyLock)
            {
                history.Add(new ConsoleMessage() { count = 1, caller = caller, message = message, type = type, trace = trace });

                if (history.Count >= config.consoleMaxHistoryLength)
                {
                    history.RemoveAt(0);
                }
            }

            if (type == LogType.Log && config.showConsoleOnMessage)
            {
                visible = true;
            }
            else if (type == LogType.Warning && config.showConsoleOnWarning)
            {
                visible = true;
            }
            else if ((type == LogType.Exception || type == LogType.Error) && config.showConsoleOnError)
            {
                visible = true;
            }

            if (config.consoleAutoScrollToBottom)
            {
                consoleScrollPosition.y = float.MaxValue;
            }
        }

        void RecalculateAreas()
        {
            float headerHeight = headerExpanded ? headerHeightExpanded : headerHeightCompact;
            headerHeight *= config.fontSize;
            headerHeight += 32.0f;

            headerArea.relativeSize.x = 1.0f;
            headerArea.absolutePosition.y = 16.0f;
            headerArea.absoluteSize.y = headerHeight;

            var commandLineAreaHeight = commandLineAreaExpanded
                ? commandLineAreaHeightExpanded
                : commandLineAreaHeightCompact;

            consoleArea.absolutePosition.y = 16.0f + headerHeight;
            consoleArea.relativeSize.x = 1.0f;
            consoleArea.relativeSize.y = 1.0f;
            consoleArea.absoluteSize.y = -(commandLineAreaHeight + headerHeight + 16.0f);

            commandLineArea.relativePosition.y = 1.0f;
            commandLineArea.absolutePosition.y = -commandLineAreaHeight;
            commandLineArea.relativeSize.x = 1.0f;
            commandLineArea.absoluteSize.y = commandLineAreaHeight;
        }

        void HandleException(Exception ex)
        {
            AddMessage("Exception in ModTools Console - " + ex.Message, LogType.Exception);
        }

        void DrawCompactHeader()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("▼", GUILayout.ExpandWidth(false)))
            {
                headerExpanded = true;
                RecalculateAreas();
            }

            GUILayout.Label("Show console configuration");

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
            {
                lock (historyLock)
                {
                    history.Clear();
                }
            }

            GUILayout.EndHorizontal();
        }

        void DrawExpandedHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Log message format:", GUILayout.ExpandWidth(false));
            config.consoleFormatString = GUILayout.TextField(config.consoleFormatString, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max items in history:", GUILayout.ExpandWidth(false));
            GUIControls.IntField("ConsoleMaxItemsInHistory", "", ref config.consoleMaxHistoryLength, 0.0f, true, true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Show console on:", GUILayout.ExpandWidth(false));
            GUILayout.FlexibleSpace();
            GUILayout.Label("Message", GUILayout.ExpandWidth(false));
            config.showConsoleOnMessage = GUILayout.Toggle(config.showConsoleOnMessage, "", GUILayout.ExpandWidth(false));
            GUILayout.FlexibleSpace();
            GUILayout.Label("Warning", GUILayout.ExpandWidth(false));
            config.showConsoleOnWarning = GUILayout.Toggle(config.showConsoleOnWarning, "", GUILayout.ExpandWidth(false));
            GUILayout.FlexibleSpace();
            GUILayout.Label("Error", GUILayout.ExpandWidth(false));
            config.showConsoleOnError = GUILayout.Toggle(config.showConsoleOnError, "", GUILayout.ExpandWidth(false));
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Auto-scroll on new messages:");
            config.consoleAutoScrollToBottom = GUILayout.Toggle(config.consoleAutoScrollToBottom, "");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("▲", GUILayout.ExpandWidth(false)))
            {
                headerExpanded = false;
                RecalculateAreas();
            }

            GUILayout.Label("Hide console configuration");

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Save"))
            {
                ModTools.Instance.SaveConfig();
            }

            if (GUILayout.Button("Reset"))
            {
                var template = new Configuration();
                config.consoleMaxHistoryLength = template.consoleMaxHistoryLength;
                config.consoleFormatString = template.consoleFormatString;
                config.showConsoleOnMessage = template.showConsoleOnMessage;
                config.showConsoleOnWarning = template.showConsoleOnWarning;
                config.showConsoleOnError = template.showConsoleOnError;

                ModTools.Instance.SaveConfig();
            }

            if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
            {
                lock (historyLock)
                {
                    history.Clear();
                }
            }

            GUILayout.EndHorizontal();
        }

        public void DrawHeader()
        {
            headerArea.Begin();

            if (headerExpanded)
            {
                DrawExpandedHeader();
            }
            else
            {
                DrawCompactHeader();
            }

            headerArea.End();
        }

        private Color orangeColor = new Color(1.0f, 0.647f, 0.0f, 1.0f);

        void DrawConsole()
        {
            userNotifications = UserNotifications.GetNotifications();

            consoleArea.Begin();

            consoleScrollPosition = GUILayout.BeginScrollView(consoleScrollPosition);

            foreach (var item in userNotifications)
            {
                GUILayout.BeginHorizontal(skin.box);

                GUI.contentColor = Color.blue;
                GUILayout.Label(item.Value);
                GUI.contentColor = Color.white;

                if (GUILayout.Button("Hide"))
                {
                    UserNotifications.HideNotification(item.Key);
                }

                GUILayout.EndHorizontal();
            }

            ConsoleMessage[] messages = null;

            lock (historyLock)
            {
                messages = history.ToArray();
            }

            foreach (ConsoleMessage item in messages)
            {
                GUILayout.BeginHorizontal(skin.box);

                string msg = config.consoleFormatString.Replace("{{type}}", item.type.ToString())
                        .Replace("{{caller}}", item.caller)
                        .Replace("{{message}}", item.message);

                switch (item.type)
                {
                    case LogType.Log:
                        GUI.contentColor = config.consoleMessageColor;
                        break;
                    case LogType.Warning:
                        GUI.contentColor = config.consoleWarningColor;
                        break;
                    case LogType.Error:
                        GUI.contentColor = config.consoleErrorColor;
                        break;
                    case LogType.Assert:
                    case LogType.Exception:
                        GUI.contentColor = config.consoleExceptionColor;
                        break;
                }

                GUILayout.Label(msg);

                GUILayout.FlexibleSpace();

                if (item.count > 1)
                {
                    GUI.contentColor = orangeColor;
                    if (item.count > 1024)
                    {
                        GUI.contentColor = Color.red;
                    }

                    GUILayout.Label(item.count.ToString(), skin.box);
                }
                else
                {
                    GUILayout.Label("");
                }

                GUI.contentColor = Color.white;

                if (item.trace != null)
                {
                    if (GUILayout.Button("Stack trace", GUILayout.ExpandWidth(false)))
                    {
                        var viewer = StackTraceViewer.CreateStackTraceViewer(item.trace);
                        var mouse = Input.mousePosition;
                        mouse.y = Screen.height - mouse.y;
                        viewer.rect.position = mouse;
                        viewer.visible = true;
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            consoleArea.End();
        }

        void DrawCommandLineArea()
        {
            commandLineArea.Begin();

            commandLineScrollPosition = GUILayout.BeginScrollView(commandLineScrollPosition, false, false);

            GUILayout.BeginHorizontal();

            GUI.SetNextControlName("ModToolsConsoleCommandLine");

            commandHistory[currentCommandHistoryIndex] = GUILayout.TextArea(commandHistory[currentCommandHistoryIndex], GUILayout.ExpandHeight(true));

            if (commandHistory[currentCommandHistoryIndex].Trim().Length == 0)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Run", GUILayout.ExpandWidth(false)))
            {
                RunCommandLine();
            }

            GUI.enabled = true;

            if (currentCommandHistoryIndex == 0)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("↑", GUILayout.ExpandWidth(false)))
            {
                currentCommandHistoryIndex--;
            }

            GUI.enabled = true;

            if (currentCommandHistoryIndex >= commandHistory.Count - 1)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("↓", GUILayout.ExpandWidth(false)))
            {
                currentCommandHistoryIndex++;
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            commandLineArea.End();
        }

        void RunCommandLine()
        {
            var commandLine = commandHistory[currentCommandHistoryIndex];

            if (commandHistory.Last() != "")
            {
                commandHistory.Add("");
                currentCommandHistoryIndex = commandHistory.Count - 1;
            }
            else
            {
                currentCommandHistoryIndex = commandHistory.Count - 1;
            }

            var source = String.Format(defaultSource, commandLine);
            var file = new ScriptEditorFile() { path = "ModToolsCommandLineScript.cs", source = source };
            string errorMessage;
            IModEntryPoint instance;
            if (!ScriptCompiler.RunSource(new List<ScriptEditorFile>() { file }, out errorMessage, out instance))
            {
                Log.Error("Failed to compile command-line!");
            }
            else
            {
                if (instance != null)
                {
                    Log.Message("Executing command-line..");
                    instance.OnModLoaded();
                }
                else
                {
                    Log.Error("Error executing command-line..");
                }
            }
            commandLine = "";
        }

        void DrawWindow()
        {
            RecalculateAreas();
            DrawHeader();
            DrawConsole();
            DrawCommandLineArea();
        }

        private readonly string defaultSource = @"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ModTools
{{
    class ModToolsCommandLineRunner : IModEntryPoint
    {{
        public void OnModLoaded()
        {{
            {0}
        }}

        public void OnModUnloaded()
        {{
        }}
    }}
}}";

    }
}
