using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModTools
{

    public class ModTools : MonoBehaviour
    {

        private Rect mainWindowRect = new Rect(128, 128, 356, 260);
        private Vector2 mainScroll = Vector2.zero;
        private bool showMain = false;

        private Rect watchesWindowRect = new Rect(504, 128, 800, 300);
        private Vector2 watchesScroll = Vector2.zero;

        private Rect sceneExplorerRect = new Rect(128, 440, 800, 500);
        private bool showSceneExplorer = false;

        private float uiScale = 1.0f;
        private float uiScaleActual = 1.0f;

        private Texture2D bgTexture = new Texture2D(1, 1);

        private GUISkin skin;

        void Awake()
        {
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

            bgTexture.SetPixel(0, 0, Color.grey);
            bgTexture.Apply();
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
            if (skin == null)
            {
                skin = ScriptableObject.CreateInstance<GUISkin>();
                skin.box = new GUIStyle(GUI.skin.box);
                skin.button = new GUIStyle(GUI.skin.button);
                skin.horizontalScrollbar = new GUIStyle(GUI.skin.horizontalScrollbar);
                skin.horizontalScrollbarLeftButton = new GUIStyle(GUI.skin.horizontalScrollbarLeftButton);
                skin.horizontalScrollbarRightButton = new GUIStyle(GUI.skin.horizontalScrollbarRightButton);
                skin.horizontalScrollbarThumb = new GUIStyle(GUI.skin.horizontalScrollbarThumb);
                skin.horizontalSlider = new GUIStyle(GUI.skin.horizontalSlider);
                skin.horizontalSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
                skin.label = new GUIStyle(GUI.skin.label);
                skin.scrollView = new GUIStyle(GUI.skin.scrollView);
                skin.textArea = new GUIStyle(GUI.skin.textArea);
                skin.textField = new GUIStyle(GUI.skin.textField);
                skin.toggle = new GUIStyle(GUI.skin.toggle);
                skin.verticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);
                skin.verticalScrollbarDownButton = new GUIStyle(GUI.skin.verticalScrollbarDownButton);
                skin.verticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);
                skin.verticalScrollbarUpButton = new GUIStyle(GUI.skin.verticalScrollbarUpButton);
                skin.verticalSlider = new GUIStyle(GUI.skin.verticalSlider);
                skin.verticalSliderThumb = new GUIStyle(GUI.skin.verticalSliderThumb);
                skin.window = new GUIStyle(GUI.skin.window);
                skin.window.normal.background = bgTexture;
                skin.window.onNormal.background = bgTexture;
            }

            var skinOld = GUI.skin;
            GUI.skin = skin;

            var matrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(uiScaleActual, uiScaleActual, uiScaleActual));

            if (showMain)
            {
                mainWindowRect = GUILayout.Window(12512, mainWindowRect, DoMainWindow, "Debugger");
            }

            if (Watches.showWindow)
            {
                watchesWindowRect = GUILayout.Window(6313, watchesWindowRect, DoWatchesWindow, "Watches");
            }

            if (showSceneExplorer)
            {
                sceneExplorerRect = GUILayout.Window(5121, sceneExplorerRect, DoSceneExplorerWindow, "Scene explorer");
            }

            GUI.matrix = matrix;
            GUI.skin = skinOld;
        }

        void DoMainWindow(int wnd)
        {
            GUI.DragWindow(new Rect(0, 0, 100000.0f, 16.0f));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            showMain = GUILayout.Toggle(showMain, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("UI Scale");
            uiScale = GUILayout.HorizontalSlider(uiScale, 0.1f, 1.25f, GUILayout.Width(220));

            if (GUILayout.Button("Apply"))
            {
                uiScaleActual = uiScale;
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Hide"))
            {
                showMain = false;
            }

            if (GUILayout.Button("Watches"))
            {
                Watches.showWindow = !Watches.showWindow;
            }

            if (GUILayout.Button("Scene explorer"))
            {
                showSceneExplorer = !showSceneExplorer;
                if (showSceneExplorer)
                {
                    SceneExplorer.Refresh();
                }
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

        void DoWatchesWindow(int wnd)
        {
            GUI.DragWindow(new Rect(0, 0, 100000.0f, 16.0f));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Watches.showWindow = GUILayout.Toggle(Watches.showWindow, "");
            GUILayout.EndHorizontal();

            watchesScroll = GUILayout.BeginScrollView(watchesScroll);

            foreach (var watch in Watches.GetWatches())
            {
                GUILayout.BeginHorizontal();

                var type = Watches.GetWatchType(watch);

                GUI.contentColor = Color.red;
                GUILayout.Label(type.ToString());
                GUI.contentColor = Color.green;
                GUILayout.Label(watch);
                GUI.contentColor = Color.white;
                GUILayout.Label(" = ");
                
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
                else
                {
                    GUILayout.Label(value.ToString());
                }

                GUILayout.FlexibleSpace();

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

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            showSceneExplorer = GUILayout.Toggle(showSceneExplorer, "");
            GUILayout.EndHorizontal();

            SceneExplorer.DrawWindow();
        }

    }

}
