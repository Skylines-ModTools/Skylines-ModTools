using System;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

namespace ModTools
{
    public class ColorPicker : GUIWindow
    {

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

        private Rect colorPickerRect;
        private Rect huesBarRect;

        private Texture2D lineTex;
        private static Color lineColor = Color.white;

        public ColorPicker() : base("ColorPicker", new Rect(16.0f, 16.0f, 180.0f, 148.0f), skin)
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

            colorPickerRect = new Rect(4.0f, 4.0f, colorPickerSize, colorPickerSize);
            huesBarRect = new Rect(colorPickerRect.x + colorPickerSize + 4.0f, colorPickerRect.y, huesBarWidth, colorPickerRect.height);
            visible = false;
        }

        public void SetColor(Color color)
        {
            currentHSV = new ColorUtil.HSV(color);
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

            float huesBarLineY = huesBarRect.y + (1.0f - (currentHSV.h / 360.0f)) * huesBarRect.height;
            GUI.DrawTexture(new Rect(huesBarRect.x-2.0f, huesBarLineY, huesBarRect.width+4.0f, 2.0f), lineTex);

            float colorPickerLineY = colorPickerRect.x + currentHSV.v * colorPickerRect.width;
            GUI.DrawTexture(new Rect(colorPickerRect.x - 1.0f, colorPickerLineY, colorPickerRect.width + 2.0f, 1.0f), lineTex);

            float colorPickerLineX = colorPickerRect.y + currentHSV.s * colorPickerRect.height;
            GUI.DrawTexture(new Rect(colorPickerLineX, colorPickerRect.y - 1.0f, 1.0f, colorPickerRect.height + 2.0f), lineTex);
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
                    currentHSV.h = (1.0f - (mouse.y - huesBarRect.y) / huesBarRect.height) * 360.0f;
                    RedrawPicker();
                }

                if (colorPickerRect.Contains(mouse))
                {
                    currentHSV.v = (mouse.x - colorPickerRect.x)/colorPickerRect.width;
                    currentHSV.s = (mouse.y - colorPickerRect.y)/colorPickerRect.height;
                }
            }
        }

        public static void DrawColorPicker(Texture2D texture, float hue)
        {
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    texture.SetPixel(x, y, GetColorAtXY(hue, (float)x / (float)texture.width, (float)y / (float)texture.height));
                }
            }

            texture.Apply();
        }

        public static Texture2D DrawHuesBar(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height);

            for (int y = 0; y < height; y++)
            {
                var color = GetColorAtT(((float)y/(float)height)*360.0f);

                for (int x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, color);
                }
            }

            tex.Apply();
            return tex;
        }

        public static Texture2D DrawLineTex()
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, lineColor);
            tex.Apply();
            return tex;
        }

        public static Color GetColorAtT(float hue)
        {
            return new ColorUtil.HSV { h = hue, s = 1.0f, v = 1.0f }.ToColor();
        }

        public static Color GetColorAtXY(float hue, float xT, float yT)
        {
            return new ColorUtil.HSV {h = hue, s = xT, v = yT}.ToColor();
        }

    }

}
