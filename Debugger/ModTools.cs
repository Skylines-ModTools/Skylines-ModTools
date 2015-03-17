using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModTools
{

    public class ModTools : GUIWindow
    {

        private Vector2 mainScroll = Vector2.zero;

        private float uiScaleUser = 1.0f;

        private SceneExplorer sceneExplorer;
        private Watches watches;

        public ModTools() : base("Mod Tools", new Rect(128, 128, 356, 260), skin)
        {
            onDraw = DoMainWindow;
        }

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

            sceneExplorer = gameObject.AddComponent<SceneExplorer>();
            watches = gameObject.AddComponent<Watches>();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Backspace))
            {
                visible = !visible;
            }
        }

        void DoMainWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            visible = GUILayout.Toggle(visible, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("UI Scale");
            uiScaleUser = GUILayout.HorizontalSlider(uiScaleUser, 0.1f, 1.25f, GUILayout.Width(160));

            if (GUILayout.Button("Apply"))
            {
                uiScale = uiScaleUser;
            }

            if (GUILayout.Button("Reset"))
            {
                uiScale = uiScaleUser = 1.0f;
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Watches"))
            {
                watches.visible = !watches.visible;
            }

            if (GUILayout.Button("Scene explorer"))
            {
                sceneExplorer.visible = !sceneExplorer.visible;
                if (sceneExplorer.visible)
                {
                    sceneExplorer.Refresh();
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

    }

}
