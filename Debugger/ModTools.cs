using System;
using ColossalFramework.UI;
using UnityEngine;

namespace ModTools
{

    public class ModTools : GUIWindow
    {
        public SimulationManager.UpdateMode updateMode = SimulationManager.UpdateMode.Undefined;

#if MODTOOLS_DEBUG
        public static readonly bool DEBUG_MODTOOLS = true;
#else
        public static readonly bool DEBUG_MODTOOLS = false;
#endif

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

        public void OnUnityDestroyCallback()
        {
            UnityLoggingHook.DisableHook();

            Destroy(console);
            Destroy(sceneExplorer);
            Destroy(sceneExplorerColorConfig);
         //   Destroy(scriptEditor);
            Destroy(watches);
            Destroy(panelExtender);
            Destroy(colorPicker);

            ImprovedWorkshopIntegration.Revert();

            instance = null;
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

            if (console != null)
            {
                console.rect = config.consoleRect;
                console.visible = config.consoleVisible;
            }

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

                if (console != null)
                {
                    config.consoleRect = console.rect;
                    config.consoleVisible = console.visible;
                }

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
            onUnityDestroy = OnUnityDestroyCallback;
            resizable = false;
        }

        private static bool loggingInitialized = false;

        public void Initialize(SimulationManager.UpdateMode _updateMode)
        {
            updateMode = _updateMode;

            if (!loggingInitialized)
            {
                Application.logMessageReceived += (condition, trace, type) =>
                {
                    if (!logExceptionsToConsole)
                    {
                        return;
                    }

                    if (Instance.console != null)
                    {
                        Instance.console.AddMessage(String.Format("{0} ({1})", condition, trace), type, true);
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

                loggingInitialized = true;
            }

            sceneExplorer = gameObject.AddComponent<SceneExplorer>();
            watches = gameObject.AddComponent<Watches>();
            colorPicker = gameObject.AddComponent<ColorPicker>();

            sceneExplorerColorConfig = gameObject.AddComponent<SceneExplorerColorConfig>();

            LoadConfig();

            if (extendGamePanels && (updateMode == SimulationManager.UpdateMode.NewGame || updateMode == SimulationManager.UpdateMode.LoadGame))
            {
                panelExtender = gameObject.AddComponent<GamePanelExtender>();
            }

            if (useModToolsConsole)
            {
                console = gameObject.AddComponent<Console>();
            }

            if (config.hookUnityLogging)
            {
                UnityLoggingHook.EnableHook();
            }

            if (updateMode == SimulationManager.UpdateMode.Undefined && config.improvedWorkshopIntegration)
            {
                ImprovedWorkshopIntegration.Bootstrap();
            }
        }

        private bool modListUpdated = false;

        void Update()
        {
            if (!modListUpdated && config.improvedWorkshopIntegration)
            {
                ImprovedWorkshopIntegration.RefreshPlugins();
                modListUpdated = true;
            }

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

                if (useModToolsConsole)
                {
                    console = gameObject.AddComponent<Console>();
                }
                else
                {
                    config.hookUnityLogging = false;
                    UnityLoggingHook.DisableHook();
                    Destroy(console);
                    console = null;
                }

                SaveConfig();
            }

            if (!config.useModToolsConsole)
            {
                GUI.enabled = false;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Hook Unity's logging");
            var newHookLogging = GUILayout.Toggle(config.hookUnityLogging, "");
            GUILayout.EndHorizontal();

            if (newHookLogging != config.hookUnityLogging)
            {
                config.hookUnityLogging = newHookLogging;
                SaveConfig();

                if (config.hookUnityLogging)
                {
                    UnityLoggingHook.EnableHook();
                }
                else
                {
                    UnityLoggingHook.DisableHook();
                }
            }

            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Log exceptions to console");
            logExceptionsToConsole = GUILayout.Toggle(logExceptionsToConsole, "");
            GUILayout.EndHorizontal();
            if (logExceptionsToConsole != config.logExceptionsToConsole)
            {
                SaveConfig();
            }

            if ((updateMode == SimulationManager.UpdateMode.NewGame || updateMode == SimulationManager.UpdateMode.LoadGame))
            {
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
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Improved Workshop integration");
            var improvedWorkshopIntegration = GUILayout.Toggle(config.improvedWorkshopIntegration, "");
            GUILayout.EndHorizontal();
            if (improvedWorkshopIntegration != config.improvedWorkshopIntegration)
            {
                config.improvedWorkshopIntegration = improvedWorkshopIntegration;
                if (config.improvedWorkshopIntegration && updateMode == SimulationManager.UpdateMode.Undefined)
                {
                    ImprovedWorkshopIntegration.Bootstrap();
                    ImprovedWorkshopIntegration.RefreshPlugins();
                }
                else if (updateMode == SimulationManager.UpdateMode.Undefined)
                {
                    ImprovedWorkshopIntegration.Revert();
                }

                SaveConfig();
            }

            if (GUILayout.Button("Debug console (F7)"))
            {
                if (console != null)
                {
                    console.visible = true;
                }
                else
                {
                    var debugOutputPanel = GameObject.Find("(Library) DebugOutputPanel").GetComponent<DebugOutputPanel>();
                    debugOutputPanel.enabled = true;
                    debugOutputPanel.GetComponent<UIPanel>().isVisible = true;
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

            if (GUILayout.Button("Throw exception!"))
            {
                throw new Exception("Hello world!");
            }
        }

    }

}
