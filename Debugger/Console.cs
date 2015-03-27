using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

        private GUIArea consoleArea;
        private GUIArea commandLineArea;

        private float commandLineAreaHeight = 45.0f;

        private int maxHistoryLength = 4096;
        private List<ConsoleMessage> history = new List<ConsoleMessage>();

        private Vector2 consoleScrollPosition = Vector2.zero;

        private string commandLine = "";
        
        public Console() : base("Debug console", new Rect(16.0f, 16.0f, 512.0f, 256.0f), skin)
        {
            onDraw = DrawWindow;
            onException = HandleException;
            onUnityDestroy = HandleDestroy;

            consoleArea = new GUIArea(this);
            commandLineArea = new GUIArea(this);

            RecalculateAreas();

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
                caller = String.Format("{0}.{1}", callingMethod.DeclaringType, callingMethod.Name, frame.GetFileName(), frame.GetFileLineNumber());
            }

            StackTrace trace = null;
            if (type == LogType.Error || type == LogType.Exception)
            {
                trace = new StackTrace(2);
            }
           
            history.Add(new ConsoleMessage() {caller = caller, message = message, type = type, trace = trace});

            if (history.Count >= maxHistoryLength)
            {
                history.RemoveAt(0);
            }
        }
        void RecalculateAreas()
        {
            consoleArea.absolutePosition.y = 32.0f;
            consoleArea.relativeSize.x = 1.0f;
            consoleArea.relativeSize.y = 1.0f;
            consoleArea.absoluteSize.y = -(commandLineAreaHeight + 32.0f);

            commandLineArea.relativePosition.y = 1.0f;
            commandLineArea.absolutePosition.y = -commandLineAreaHeight;
            commandLineArea.relativeSize.x = 1.0f;
            commandLineArea.absoluteSize.y = commandLineAreaHeight;
        }

        void HandleException(Exception ex)
        {
        }

        void DrawConsoleArea()
        {
            consoleArea.Begin();

            consoleScrollPosition = GUILayout.BeginScrollView(consoleScrollPosition);

            foreach (ConsoleMessage item in history)
            {
                GUILayout.BeginHorizontal(skin.box);
                var msg = String.Format("[{0}] {1} - {2}", item.type.ToString(), item.caller, item.message);

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
                    if (GUILayout.Button("Stack trace", GUILayout.Width(80)))
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
            DrawConsoleArea();
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
