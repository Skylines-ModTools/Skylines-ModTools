using System.Runtime.InteropServices;
using ColossalFramework;
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
        public static Texture2D resizeNormalTexture = null;
        public static Texture2D resizeHoverTexture = null;

        public static Texture2D closeNormalTexture = null;
        public static Texture2D closeHoverTexture = null;

        public static GUIWindow resizingWindow = null;
        public static Vector2 resizeDragHandle = Vector2.zero;

        public static float uiScale = 1.0f;

        public bool visible = false;

        public string title = "Window";

        private int id = 0;

        private Vector2 minSize = Vector2.zero;

        public GUIWindow(string _title, Rect _rect, GUISkin _skin)
        {
            id = UnityEngine.Random.Range(1024, int.MaxValue);
            title = _title;
            rect = _rect;
            skin = _skin;
            minSize = new Vector2(64.0f, 64.0f);
        }

        void OnGUI()
        {
            if (skin == null)
            {
                bgTexture = new Texture2D(1, 1);
                bgTexture.SetPixel(0, 0, Color.grey);
                bgTexture.Apply();

                resizeNormalTexture = new Texture2D(1, 1);
                resizeNormalTexture.SetPixel(0, 0, Color.white);
                resizeNormalTexture.Apply();

                resizeHoverTexture = new Texture2D(1, 1);
                resizeHoverTexture.SetPixel(0, 0, Color.red);
                resizeHoverTexture.Apply();

                closeNormalTexture = new Texture2D(1, 1);
                closeNormalTexture.SetPixel(0, 0, Color.red);
                closeNormalTexture.Apply();

                closeHoverTexture = new Texture2D(1, 1);
                closeHoverTexture.SetPixel(0, 0, Color.white);
                closeHoverTexture.Apply();

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

                skin.settings.cursorColor = GUI.skin.settings.cursorColor;
                skin.settings.cursorFlashSpeed = GUI.skin.settings.cursorFlashSpeed;
                skin.settings.doubleClickSelectsWord = GUI.skin.settings.doubleClickSelectsWord;
                skin.settings.selectionColor = GUI.skin.settings.selectionColor;
                skin.settings.tripleClickSelectsLine = GUI.skin.settings.tripleClickSelectsLine;

                skin.font = GUI.skin.font;
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
                        GUI.DragWindow(new Rect(0, 0, 100000.0f, 30.0f));

                        onDraw();

                        GUILayout.Space(16.0f);

                        var mouse = Input.mousePosition;
                        mouse.y = Screen.height - mouse.y;

                        var resizeRect = new Rect(rect.x + rect.width - 16.0f, rect.y + rect.height - 16.0f, 16.0f, 16.0f);
                        var resizeTex = resizeNormalTexture;

                        if (resizingWindow != null)
                        {
                            if (resizingWindow == this)
                            {
                                resizeTex = resizeHoverTexture;

                                if (Input.GetMouseButton(0))
                                {
                                    var size = new Vector2(mouse.x, mouse.y) + resizeDragHandle - new Vector2(rect.x, rect.y);

                                    if (size.x < minSize.x)
                                    {
                                        size.x = minSize.x;
                                    }

                                    if (size.y < minSize.y)
                                    {
                                        size.y = minSize.y;
                                    }
                                    
                                    rect.width = size.x;
                                    rect.height = size.y;
                                }
                                else
                                {
                                    resizingWindow = null;
                                }
                            }
                        }
                        else if (resizeRect.Contains(mouse))
                        {
                            resizeTex = resizeHoverTexture;
                            if (Input.GetMouseButton(0))
                            {
                                resizingWindow = this;
                                resizeDragHandle = new Vector2(rect.x + rect.width, rect.y + rect.height) - new Vector2(mouse.x, mouse.y);
                            }
                        }

                        GUI.DrawTexture(new Rect(rect.width - 16.0f, rect.height - 16.0f, 16.0f, 16.0f), resizeTex, ScaleMode.StretchToFill);

                        var closeRect = new Rect(rect.x + rect.width - 20.0f, rect.y, 16.0f, 8.0f);
                        var closeTex = closeNormalTexture;

                        if (closeRect.Contains(mouse))
                        {
                            closeTex = closeHoverTexture;

                            if (Input.GetMouseButton(0))
                            {
                                visible = false;
                            }
                        }

                        GUI.DrawTexture(new Rect(rect.width - 20.0f, 0.0f, 16.0f, 8.0f), closeTex, ScaleMode.StretchToFill);
                    }
                }, title);

                GUI.matrix = matrix;

                GUI.skin = oldSkin;
            }
        }

    }

}
