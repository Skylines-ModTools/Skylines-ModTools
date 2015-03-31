using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModTools
{
    public class ColorPicker : GUIWindow
    {

        public delegate void OnColorChanged(Color color);

        public delegate void OnColor32Changed(Color32 color);

        private static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

        public static Texture2D GetColorTexture(string hash, Color color)
        {
            if (!textureCache.ContainsKey(hash))
            {
                textureCache.Add(hash, new Texture2D(1, 1));
            }

            var texture = textureCache[hash];
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private readonly int colorPickerSize = 142;
        private readonly int huesBarWidth = 26;

        private Texture2D colorPicker;
        private Texture2D huesBar;
        private ColorUtil.HSV currentHSV;
        private float originalAlpha = 0.0f;
        
        private OnColorChanged onColorChanged;
        private OnColor32Changed onColor32Changed;

        private Rect colorPickerRect;
        private Rect huesBarRect;

        private Texture2D lineTex;
        private static Color lineColor = Color.white;

        public ColorPicker() : base("ColorPicker", new Rect(16.0f, 16.0f, 188.0f, 156.0f), skin)
        {
            resizable = false;
            hasTitlebar = false;
            hasCloseButton = false;

            onDraw = DrawWindow;
            onException = HandleException;

            huesBar = DrawHuesBar(huesBarWidth, colorPickerSize);
            lineTex = DrawLineTex();

            colorPicker = new Texture2D(colorPickerSize, colorPickerSize);
            RedrawPicker();

            colorPickerRect = new Rect(8.0f, 8.0f, colorPickerSize, colorPickerSize);
            huesBarRect = new Rect(colorPickerRect.x + colorPickerSize + 4.0f, colorPickerRect.y, huesBarWidth, colorPickerRect.height);
            visible = false;
        }

        public void SetColor(Color color, OnColorChanged _onColorChanged)
        {
            originalAlpha = color.a;
            currentHSV = ColorUtil.HSV.RGB2HSV(color);
            currentHSV.h = 360.0f - currentHSV.h;
            onColorChanged = _onColorChanged;
            RedrawPicker();
        }

        public void SetColor(Color32 color, OnColor32Changed _onColor32Changed)
        {
            originalAlpha = color.a/255.0f;
            currentHSV = ColorUtil.HSV.RGB2HSV(color);
            currentHSV.h = 360.0f - currentHSV.h;
            onColor32Changed = _onColor32Changed;
            RedrawPicker();
        }

        void RedrawPicker()
        {
            DrawColorPicker(colorPicker, currentHSV.h);
        }

        void HandleException(Exception ex)
        {
            Log.Error("Exception in ColorPicker - " + ex.Message);
            visible = false;
        }

        void DrawWindow()
        {
            GUI.DrawTexture(colorPickerRect, colorPicker);
            GUI.DrawTexture(huesBarRect, huesBar);

            float huesBarLineY = huesBarRect.y + (1.0f-((float)currentHSV.h / 360.0f)) * huesBarRect.height;
            GUI.DrawTexture(new Rect(huesBarRect.x-2.0f, huesBarLineY, huesBarRect.width+4.0f, 2.0f), lineTex);

            float colorPickerLineY = colorPickerRect.x + (float)currentHSV.v * colorPickerRect.width;
            GUI.DrawTexture(new Rect(colorPickerRect.x - 1.0f, colorPickerLineY, colorPickerRect.width + 2.0f, 1.0f), lineTex);

            float colorPickerLineX = colorPickerRect.y + (float)currentHSV.s * colorPickerRect.height;
            GUI.DrawTexture(new Rect(colorPickerLineX, colorPickerRect.y - 1.0f, 1.0f, colorPickerRect.height + 2.0f), lineTex);
        }

        void InternalOnColorChanged(Color color)
        {
            color.a = originalAlpha;

            if (onColorChanged != null)
            {
                onColorChanged(color);
            }

            if (onColor32Changed != null)
            {
                onColor32Changed(color);
            }
        }

        void Update()
        {
            Vector2 mouse = Input.mousePosition;
            mouse.y = Screen.height - mouse.y;

            if (Input.GetMouseButton(0) && !rect.Contains(mouse))
            {
                visible = false;
                return;
            }

            mouse -= rect.position;

            if (Input.GetMouseButton(0))
            {
                if (huesBarRect.Contains(mouse))
                {
                    currentHSV.h = (1.0f-Mathf.Clamp01((mouse.y - huesBarRect.y) / huesBarRect.height)) * 360.0f;
                    RedrawPicker();

                    InternalOnColorChanged(ColorUtil.HSV.HSV2RGB(currentHSV));
                }

                if (colorPickerRect.Contains(mouse))
                {
                    currentHSV.s = Mathf.Clamp01((mouse.x - colorPickerRect.x)/colorPickerRect.width);
                    currentHSV.v = Mathf.Clamp01((mouse.y - colorPickerRect.y)/colorPickerRect.height);

                    InternalOnColorChanged(ColorUtil.HSV.HSV2RGB(currentHSV));
                }
            }
        }

        public static void DrawColorPicker(Texture2D texture, double hue)
        {
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    texture.SetPixel(x, y, GetColorAtXY(hue, (float)x / (float)texture.width, 1.0f - (float)y / (float)texture.height));
                }
            }

            texture.Apply();
        }

        public static Texture2D DrawHuesBar(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);

            for (int y = 0; y < height; y++)
            {
                var color = GetColorAtT(((float)y/(float)height)*360.0f);

                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return texture;
        }

        public static Texture2D DrawLineTex()
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, lineColor);
            tex.Apply();
            return tex;
        }

        public static Color GetColorAtT(double hue)
        {
            return ColorUtil.HSV.HSV2RGB(new ColorUtil.HSV {h = hue, s = 1.0f, v = 1.0f});
        }

        public static Color GetColorAtXY(double hue, float xT, float yT)
        {
            return ColorUtil.HSV.HSV2RGB(new ColorUtil.HSV {h = hue, s = xT, v = yT});
        }

    }

}
