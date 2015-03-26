using System.IO;
using UnityEngine;

namespace ModTools
{

    public class ModTools : GUIWindow
    {

        private Vector2 mainScroll = Vector2.zero;

        public SceneExplorer sceneExplorer;
        public SceneExplorerColorConfig sceneExplorerColorConfig;

        public Watches watches;
        public TextureViewer textureViewer;
        public MeshViewer meshViewer;
        public ColorPicker colorPicker;

        private GamePanelExtender panelExtender;

        public static bool logExceptionsToConsole = true;
        public static bool extendGamePanels = true;

        public Configuration config = new Configuration();
        public static readonly string configPath = "ModToolsConfig.xml";

        private static ModTools instance = null;

        public void OnDestroy()
        {
            Destroy(sceneExplorer);
            Destroy(watches);
            Destroy(textureViewer);
            Destroy(meshViewer);
            Destroy(panelExtender);
        }

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

            logExceptionsToConsole = config.logExceptionsToConsole;
            extendGamePanels = config.extendGamePanels;

            rect = config.mainWindowRect;
            visible = config.mainWindowVisible;

            textureViewer.rect = config.textureViewerRect;
            textureViewer.visible = config.textureViewerVisible;

            meshViewer.rect = config.meshViewerRect;
            meshViewer.visible = config.meshViewerVisible;

            watches.rect = config.watchesRect;
            watches.visible = config.watchesVisible;

            sceneExplorer.rect = config.sceneExplorerRect;
            sceneExplorer.visible = config.sceneExplorerVisible;
            if (sceneExplorer.visible)
            {
                sceneExplorer.Refresh();
            }
        }

        public void SaveConfig()
        {
            if (config != null)
            {
                config.logExceptionsToConsole = logExceptionsToConsole;
                config.extendGamePanels = extendGamePanels;

                config.mainWindowRect = rect;
                config.mainWindowVisible = visible;

                config.textureViewerRect = textureViewer.rect;
                config.textureViewerVisible = textureViewer.visible;

                config.meshViewerRect = meshViewer.rect;
                config.meshViewerVisible = meshViewer.visible;

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
            sceneExplorerColorConfig = gameObject.AddComponent<SceneExplorerColorConfig>();

            watches = gameObject.AddComponent<Watches>();
            textureViewer = gameObject.AddComponent<TextureViewer>();
            meshViewer = gameObject.AddComponent<MeshViewer>();
            colorPicker = gameObject.AddComponent<ColorPicker>();

            LoadConfig();

            if (extendGamePanels)
            {
                panelExtender = gameObject.AddComponent<GamePanelExtender>();
            }
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
                meshViewer.visible = !meshViewer.visible;
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T))
            {
                textureViewer.visible = !textureViewer.visible;
            }
        }

        void DoMainWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Log exceptions to console");
            logExceptionsToConsole = GUILayout.Toggle(logExceptionsToConsole, "");
            GUILayout.EndHorizontal();
            if (logExceptionsToConsole != config.logExceptionsToConsole)
            {
                SaveConfig();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Game panel extensions");
            var newExtendGamePanels = GUILayout.Toggle(extendGamePanels, "");
            GUILayout.EndHorizontal();

            if (newExtendGamePanels != extendGamePanels)
            {
                extendGamePanels = newExtendGamePanels;
                SaveConfig();

                if (extendGamePanels)
                {
                    gameObject.AddComponent<GamePanelExtender>();
                }
                else
                {
                    Destroy(gameObject.GetComponent<GamePanelExtender>());
                }
            }

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

            if (GUILayout.Button("Texture Viewer (Ctrl+T)"))
            {
                textureViewer.visible = !textureViewer.visible;
            }

            if (GUILayout.Button("Mesh Viewer (Ctrl+R)"))
            {
                meshViewer.visible = !meshViewer.visible;
            }

            if (GUILayout.Button("Dump materials"))
            {
                var materials = Resources.FindObjectsOfTypeAll<Material>();

                string s = "";
                foreach (var material in materials)
                {
                    s += material.shader.name + '\n';
                }

                File.WriteAllText("C:\\Users\\nlight\\Desktop\\shaders223.txt", s);
            }
        }

    }

}
