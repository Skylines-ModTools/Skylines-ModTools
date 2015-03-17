using System.Runtime.InteropServices;
using UnityEngine;

namespace ModTools
{
    public class GUIWindow : MonoBehaviour
    {

        public delegate void OnDraw();

        public OnDraw onDraw = null;

        public Rect rect = new Rect(0, 0, 64, 64);

        public static GUISkin skin = null;
        public static Texture2D bgTexture = null;
        public static float uiScale = 1.0f;

        public bool visible = true;

        public string title = "Window";

        private int id = 0;

        public GUIWindow(string _title, Rect _rect, GUISkin _skin)
        {
            id = UnityEngine.Random.Range(1024, int.MaxValue);
            title = _title;
            rect = _rect;
            skin = _skin;
        }

        void OnGUI()
        {
            if (skin == null)
            {
                bgTexture = new Texture2D(1, 1);
                bgTexture.SetPixel(0, 0, Color.grey);
                bgTexture.Apply();

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

            if (visible)
            {
                var oldSkin = GUI.skin;
                if (skin != null)
                {
                    GUI.skin = skin;
                }

                var matrix = GUI.matrix;
                GUI.matrix = Matrix4x4.Scale(new Vector3(uiScale, uiScale, uiScale));

                rect = GUI.Window(id, rect, i =>
                {
                    if (onDraw != null)
                    {
                        GUI.DragWindow(new Rect(0, 0, 100000.0f, 16.0f));
                        onDraw();
                    }
                }, title);

                GUI.matrix = matrix;

                GUI.skin = oldSkin;
            }
        }

    }

}
