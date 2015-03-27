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
        private float headerHeightExpanded = 6.3f;
        private bool headerExpanded = false;
        
        private float commandLineAreaHeight = 45.0f;

        private List<ConsoleMessage> history = new List<ConsoleMessage>();

        private Vector2 consoleScrollPosition = Vector2.zero;

        private string commandLine = "";
        
        public Console() : base("Debug console", config.consoleRect, skin)
        {
            onDraw = DrawWindow;
          //  onException = HandleException;
            onUnityDestroy = HandleDestroy;

            headerArea = new GUIArea(this);
            consoleArea = new GUIArea(this);
            commandLineArea = new GUIArea(this);

            RecalculateAreas();

            GameObject.Find("(Library) DebugOutputPanel").GetComponent<UIPanel>().isVisible = false;
            GameObject.Find("(Library) DebugOutputPanel").GetComponent<DebugOutputPanel>().enabled = false;
        }

        void HandleDestroy()
        {
            GameObject.Find("(Library) DebugOutputPanel").GetComponent<DebugOutputPanel>().enabled = true;
        }

        public void AddMessage(string message, LogType type = LogType.Log, bool _internal = false)
        {
            if (history.Count > 0)
            {
                var last = history.Last();
                if (message == last.message && type == last.type)
                {
                    last.count++;
                }
            }

            string caller = "ModTools";

            if (!_internal)
            {
                var frame = new StackFrame(2, true);
                var callingMethod = frame.GetMethod();
                caller = String.Format("{0}.{1}()", callingMethod.DeclaringType, callingMethod.Name, frame.GetFileName(), frame.GetFileLineNumber());
            }

            StackTrace trace = null;
            if (type == LogType.Error || type == LogType.Exception)
            {
                trace = new StackTrace(2);
            }
           
            history.Add(new ConsoleMessage() {caller = caller, message = message, type = type, trace = trace});

            if (history.Count >= config.consoleMaxHistoryLength)
            {
                history.RemoveAt(0);
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
        }
        void RecalculateAreas()
        {
            float headerHeight = (headerExpanded ? headerHeightExpanded : headerHeightCompact);
            headerHeight *= config.fontSize;
            headerHeight += 32.0f;

            headerArea.relativeSize.x = 1.0f;
            headerArea.absolutePosition.y = 16.0f;
            headerArea.absoluteSize.y = headerHeight;

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

        void DrawConsole()
        {
            consoleArea.Begin();

            consoleScrollPosition = GUILayout.BeginScrollView(consoleScrollPosition);

            foreach (ConsoleMessage item in history)
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

                GUI.contentColor = Color.white;
                
                GUILayout.FlexibleSpace();

                if (item.trace != null)
                {
                    if (GUILayout.Button("Stack trace", GUILayout.ExpandWidth(false)))
                    {
                        
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

            GUILayout.BeginHorizontal();

            commandLine = GUILayout.TextField(commandLine);

            if (GUILayout.Button("Submit", GUILayout.ExpandWidth(false)))
            {
                var source = String.Format(defaultSource, commandLine);
                var file = new ScriptEditorFile() {path = "ModToolsCommandLineScript.cs", source = source};

                string errorMessage;
                IModEntryPoint instance;
                if (ScriptCompiler.RunSource(new List<ScriptEditorFile>() {file}, out errorMessage, out instance))
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
                        Log.Error("Error xxecuting command-line..");
                    }
                }

                commandLine = "";
            }

            GUILayout.EndHorizontal();

            commandLineArea.End();
        }

        void DrawWindow()
        {
            DrawHeader();
            DrawConsole();
            DrawCommandLineArea();
        }

        private readonly string defaultSource = @"
namespace ModTools
{
    class ModToolsCommandLineRunner : ModTools.IModEntryPoint
    {
        public void OnModLoaded()
        {
            {0}
        }

        public void OnModUnloaded()
        {
        }
    }
}";

    }
}
