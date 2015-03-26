using System;
using System.Xml.Schema;
using UnityEngine;

namespace ModTools
{
    public class ColorPicker : GUIWindow
    {

        private readonly int colorPickerSize = 140;
        private readonly int huesBarWidth = 32;

        private Texture2D colorPicker;
        private Texture2D huesBar;
        private float currentHue = 0.0f;

        private Rect colorPickerRect;
        private Rect huesBarRect;

        public ColorPicker() : base("ColorPicker", new Rect(16.0f, 16.0f, 256.0f, 256.0f), skin)
        {
            onDraw = DrawWindow;
            onException = HandleException;

            huesBar = DrawHuesBar(huesBarWidth, colorPickerSize);
            RedrawPicker();

            colorPickerRect = new Rect(8.0f, 16.0f, colorPickerSize, colorPickerSize);
            huesBarRect = new Rect(colorPickerRect.x + colorPickerSize + 8.0f, colorPickerRect.y, huesBarWidth, colorPickerRect.height);
            visible = true;
        }

        void RedrawPicker()
        {
            colorPicker = DrawColorPicker(colorPickerSize, colorPickerSize, currentHue);
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
        }

        public static Texture2D DrawColorPicker(int width, int height, float hue)
        {
            Texture2D tex = new Texture2D(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tex.SetPixel(x, y, GetColorAtXY(hue, (float)x/(float)width, (float)y/(float)height));
                }
            }

            tex.Apply();
            return tex;
        }

        public static Texture2D DrawHuesBar(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height);

            for (int y = 0; y < height; y++)
            {
                var color = GetColorAtT((float)y/(float)height);

                for (int x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, color);
                }
            }

            tex.Apply();
            return tex;
        }

        public static Color GetColorAtT(float t)
        {
            return new ColorUtil.HSV { h = t * 360.0f, s = 1.0f, v = 1.0f }.ToColor();
        }

        public static Color GetColorAtXY(float hue, float xT, float yT)
        {
            return new ColorUtil.HSV {h = hue, s = xT, v = yT}.ToColor();
        }

    }

}
