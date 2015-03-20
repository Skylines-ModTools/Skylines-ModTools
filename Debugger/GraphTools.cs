using UnityEngine;

namespace ModTools
{

    public static class GraphRenderer
    {

        public static Texture2D Render2DGraph(int width, int height, string xLabel, string yLabel, float[] xData, float[] yData)
        {
            var texture = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
            Color32[] pixels = new Color32[width * height];

            for (int i = 0; i < width*height; i++)
            {
                pixels[i] = new Color32(0, 0, 0, 1);
            }

            RenderAxes(width, height, pixels);


            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        private static void RenderAxes(int width, int height, Color32[] pixels)
        {
            
        }

        private static void RenderLabels(int width, int height, Color32[] pixels, string xLabel, string yLabel)
        {
            
        }

        private static void RenderData(int width, int height, Color32[] pixels, float[] xData, float[] yData)
        {
            
        }

    }

}
