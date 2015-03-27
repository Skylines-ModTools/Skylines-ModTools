using System.IO;
using UnityEngine;

namespace ModTools
{

    public class ModTools : GUIWindow
    {

        private Vector2 mainScroll = Vector2.zero;

        public Console console;
        public SceneExplorer sceneExplorer;
        public SceneExplorerColorConfig sceneExplorerColorConfig;

     //   public ScriptEditor scriptEditor;

        public Watches watches;
        public ColorPicker colorPicker;

        private GamePanelExtender panelExtender;

        public static bool logExceptionsToConsole = true;
        public static bool extendGamePanels = true;
        public static bool useModToolsConsole = true;

        public Configuration config = new Configuration();
        public static readonly string configPath = "ModToolsConfig.xml";

        private static ModTools instance = null;

        public void OnDestroy()
        {
            Destroy(console);
            Destroy(sceneExplorer);
            Destroy(sceneExplorerColorConfig);
         //   Destroy(scriptEditor);
            Destroy(watches);
            Destroy(panelExtender);
            Destroy(colorPicker);
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
            useModToolsConsole = config.useModToolsConsole;

            rect = config.mainWindowRect;
            visible = config.mainWindowVisible;

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
                config.useModToolsConsole = useModToolsConsole;

                config.mainWindowRect = rect;
                config.mainWindowVisible = visible;

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

                if (instance.console != null)
                {
                    instance.console.AddMessage(condition, type);
                    return;
                }

                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                {
                    Log.Error(condition);
                }
                else if (type == LogType.Warning)
                {
                    Log.Warning(condition);
                }
                else
                {
                    Log.Message(condition);
                }
            };

            sceneExplorer = gameObject.AddComponent<SceneExplorer>();

            //scriptEditor = gameObject.AddComponent<ScriptEditor>();

            watches = gameObject.AddComponent<Watches>();
            colorPicker = gameObject.AddComponent<ColorPicker>();

            LoadConfig();

            sceneExplorerColorConfig = gameObject.AddComponent<SceneExplorerColorConfig>();

            if (extendGamePanels)
            {
                panelExtender = gameObject.AddComponent<GamePanelExtender>();
            }

            if (useModToolsConsole)
            {
                console = gameObject.AddComponent<Console>();
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

            if (useModToolsConsole && Input.GetKeyDown(KeyCode.F7))
            {
                console.visible = !console.visible;
            }
        }

        void DoMainWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Use ModTools console");
            var newUseConsole = GUILayout.Toggle(useModToolsConsole, "");
            GUILayout.EndHorizontal();

            if (newUseConsole != useModToolsConsole)
            {
                useModToolsConsole = newUseConsole;
                SaveConfig();

                if (useModToolsConsole)
                {
                    console = gameObject.AddComponent<Console>();
                }
                else
                {
                    Destroy(console);
                    console = null;
                }
            }

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
        }

    }

}
