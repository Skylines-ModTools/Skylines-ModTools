using System;
using System.Collections.Generic;
using ColossalFramework;
using UnityEngine;

namespace ModTools
{

    public class ModTools : GUIWindow
    {

        private Vector2 mainScroll = Vector2.zero;

        private SceneExplorer sceneExplorer;
        private Watches watches;
        private RTLiveView rtLiveView;

        public static bool logExceptionsToConsole = true;
        public static bool evaluatePropertiesAutomatically = true;

        public Configuration config = new Configuration();
        public static readonly string configPath = "ModToolsConfig.xml";

        private static ModTools instance = null;

        public static ModTools Instance
        {
            get
            {
                instance = instance ?? FindObjectOfType<ModTools>();
                return instance;
            }
        }

        public void LoadConfig()
        {
            config = Configuration.Deserialize(configPath);
            if (config == null)
            {
                config = new Configuration();
                SaveConfig();
            }

            rect = config.mainWindowRect;
            visible= config.mainWindowVisible;

            rtLiveView.rect = config.rtLiveViewRect ;
            rtLiveView.visible = config.rtLiveViewVisible;

            watches.rect = config.watchesRect;
            watches.visible = config.watchesVisible;

            sceneExplorer.rect = config.sceneExplorerRect;
            sceneExplorer.visible = config.sceneExplorerVisible;
        }

        public void SaveConfig()
        {
            if (config != null)
            {
                config.mainWindowRect = rect;
                config.mainWindowVisible = visible;

                config.rtLiveViewRect = rtLiveView.rect;
                config.rtLiveViewVisible = rtLiveView.visible;

                config.watchesRect = watches.rect;
                config.watchesVisible = watches.visible;

                config.sceneExplorerRect = sceneExplorer.rect;
                config.sceneExplorerVisible = sceneExplorer.visible;

                Configuration.Serialize(configPath, config);
            }
        }

        public ModTools() : base("Mod Tools", new Rect(128, 128, 356, 260), skin)
        {
            onDraw = DoMainWindow;
        }

        void Awake()
        {
            Application.logMessageReceived += (condition, trace, type) =>
            {
                if (!logExceptionsToConsole)
                {
                    return;
                }

                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                {
                    Log.Error(condition);
                    Log.Error(trace);
                }
                else if (type == LogType.Warning)
                {
                    Log.Warning(condition);
                    Log.Warning(trace);
                }
                else
                {
                    Log.Message(condition);
                }
            };

            sceneExplorer = gameObject.AddComponent<SceneExplorer>();
            watches = gameObject.AddComponent<Watches>();
            rtLiveView = gameObject.AddComponent<RTLiveView>();

            LoadConfig();
        }

        void Update()
        {
            UpdateMouseScrolling();

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q))
            {
                visible = !visible;
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.E))
            {
                sceneExplorer.visible = !sceneExplorer.visible;
                if (sceneExplorer.visible)
                {
                    sceneExplorer.Refresh();
                }
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.W))
            {
                watches.visible = !watches.visible;
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            {
                rtLiveView.visible = !rtLiveView.visible;
            }
        }

        void DoMainWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Log exceptions to console");
            logExceptionsToConsole = GUILayout.Toggle(logExceptionsToConsole, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Evaluate properties automatically");
            evaluatePropertiesAutomatically = GUILayout.Toggle(evaluatePropertiesAutomatically, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("SceneExplorer debug mode");
            SceneExplorer.debugMode = GUILayout.Toggle(SceneExplorer.debugMode, "");
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Watches (Ctrl+W)"))
            {
                watches.visible = !watches.visible;
            }

            if (GUILayout.Button("Scene explorer (Ctrl+E)"))
            {
                sceneExplorer.visible = !sceneExplorer.visible;
                if (sceneExplorer.visible)
                {
                    sceneExplorer.Refresh();
                }
            }

            if (GUILayout.Button("RenderTexture LiveView (Ctrl+R)"))
            {
                rtLiveView.visible = !rtLiveView.visible;
            }

            mainScroll = GUILayout.BeginScrollView(mainScroll);

            if (GUILayout.Button("Throw exception!"))
            {
                throw new Exception("Hello world!");
            }

            var subscribers = FindObjectsOfType<MonoBehaviour>();
            Dictionary<string, bool> set = new Dictionary<string, bool>();

            foreach (var subscriber in subscribers)
            {
                if (set.ContainsKey(subscriber.name))
                {
                    continue;
                }
                else
                {
                    set.Add(subscriber.name, true);
                }

                if (subscriber.name.StartsWith("debug:"))
                {
                    var tmp = subscriber.name.Split(':');
                    if (tmp.Length != 3)
                    {
                        continue;
                    }

                    var method = tmp[1];
                    var label = tmp[2];

                    if (GUILayout.Button(label))
                    {
                        subscriber.SendMessage(method);
                    }
                }
            }

            GUILayout.EndScrollView();
        }

    }

}
