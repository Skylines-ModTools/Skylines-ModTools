using System;
using UnityEngine;

namespace Debugger
{

    public class Debugger : MonoBehaviour
    {

        private Rect mainWindowRect = new Rect(128, 128, 356, 300);
        private bool showMain = false;

        private Rect watchesWindowRect = new Rect(400, 128, 800, 300);
        private Vector2 watchesScroll = Vector2.zero;

        private Rect sceneExplorerRect = new Rect(128, 440, 800, 500);
        private bool showSceneExplorer = false;

        private float uiScale = 1.0f;
        private float uiScaleActual = 1.0f;

        private Texture2D blackTexture = new Texture2D(1, 1);
        void Awake()
        {
            blackTexture.SetPixel(0, 0, Color.grey);
            blackTexture.Apply();

            Application.logMessageReceived += (condition, trace, type) =>
            {
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
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Backspace))
            {
                showMain = !showMain;
            }
        }

        void OnGUI()
        {
            var defaultWindowBackgroundTexture = GUI.skin.window.normal.background;
            GUI.skin.window.normal.background = blackTexture;
            GUI.skin.window.onNormal.background = blackTexture;

            var matrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(uiScaleActual, uiScaleActual, uiScaleActual));

            if (!showMain)
            {
                return;
            }

            mainWindowRect = GUILayout.Window(12512, mainWindowRect, DoMainWindow, "Debugger");

            if (Watches.showWindow)
            {
                watchesWindowRect = GUILayout.Window(6313, watchesWindowRect, DoWatchesWindow, "Watches");
            }

            if (showSceneExplorer)
            {
                sceneExplorerRect = GUILayout.Window(5121, sceneExplorerRect, DoSceneExplorerWindow, "Scene explorer");
            }

            GUI.skin.window.normal.background = defaultWindowBackgroundTexture;
            GUI.skin.window.onNormal.background = defaultWindowBackgroundTexture;
            GUI.matrix = matrix;
        }

        void DoMainWindow(int wnd)
        {
            GUI.DragWindow(new Rect(0, 0, 100000.0f, 16.0f));

            GUILayout.BeginHorizontal();
            GUILayout.Label("UI Scale");
            uiScale = GUILayout.HorizontalSlider(uiScale, 0.1f, 1.25f, GUILayout.Width(220));

            if (GUILayout.Button("Apply"))
            {
                uiScaleActual = uiScale;
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Watches"))
            {
                Watches.showWindow = !Watches.showWindow;
            }

            if (GUILayout.Button("Scene explorer"))
            {
                showSceneExplorer = !showSceneExplorer;
            }

            if (GUILayout.Button("Throw exception!"))
            {
                throw new Exception("Hello world!");
            }
        }

        void DoWatchesWindow(int wnd)
        {
            GUI.DragWindow(new Rect(0, 0, 100000.0f, 16.0f));
            watchesScroll = GUILayout.BeginScrollView(watchesScroll);

            foreach (var watch in Watches.GetWatches())
            {
                GUILayout.BeginHorizontal();

                var type = Watches.GetWatchType(watch);

                GUILayout.Label(String.Format("{0} - {1} - Value:", watch, type.ToString()));

                var value = Watches.ReadWatch(watch);

                if (type.ToString() == "System.Single")
                {
                    var f = (float)value;
                    GUIControls.FloatField("", ref f);
                    if (f != (float)value)
                    {
                        Watches.WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.Int32")
                {
                    var f = (int)value;
                    GUIControls.IntField("", ref f);
                    if (f != (int)value)
                    {
                        Watches.WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.Boolean")
                {
                    var f = (bool)value;
                    GUIControls.BoolField("", ref f);
                    if (f != (bool)value)
                    {
                        Watches.WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.String")
                {
                    var f = (string)value;
                    GUIControls.StringField("", ref f);
                    if (f != (string)value)
                    {
                        Watches.WriteWatch(watch, f);
                    }
                }

                if (GUILayout.Button("x", GUILayout.Width(24)))
                {
                    Watches.RemoveWatch(watch);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        void DoSceneExplorerWindow(int wnd)
        {
            GUI.DragWindow(new Rect(0, 0, 100000.0f, 16.0f));
            SceneExplorer.DrawWindow();
        }

    }

}
