using System;
using System.IO;
using UnityEngine;

namespace ModTools
{
    class RTLiveView : GUIWindow
    {

        public Texture previewTexture = null;
        public string caller = "";

        public RTLiveView() : base("RenderTexture LiveView", new Rect(512, 128, 512, 512), skin)
        {
            onDraw = DrawWindow;
        }

        void DrawWindow()
        {
            if (previewTexture != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(String.Format("Previewing {0} \"{1}\"", caller, previewTexture.name));
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Dump .png"))
                {
                    DumpTextureToPNG(previewTexture);
                }

                GUILayout.EndHorizontal();
                GUI.DrawTexture(new Rect(0.0f, 120.0f, rect.width, rect.height), previewTexture, ScaleMode.ScaleToFit, false);
            }
            else
            {
                GUILayout.Label("Use the Scene Explorer to select a RenderTexture for live view");
            }
        }

        public static void DumpTextureToPNG(Texture previewTexture)
        {
            string filename = "";
            var filenamePrefix = String.Format("rt_dump_{0}", previewTexture.name);
            if (!File.Exists(filenamePrefix + ".png"))
            {
                filename = filenamePrefix + ".png";
            }
            else
            {
                int i = 1;
                while (File.Exists(String.Format("{0}_{1}.png", filenamePrefix, i)))
                {
                    i++;
                }

                filename = String.Format("{0}_{1}.png", filenamePrefix, i);
            }

            if (previewTexture is RenderTexture)
            {
                Util.DumpRenderTexture((RenderTexture)previewTexture, filename);
                Log.Warning(String.Format("Texture dumped to \"{0}\"", filename));
            }
            else if (previewTexture is Texture2D)
            {
                var texture = previewTexture as Texture2D;
                byte[] bytes = null;

                try
                {
                    bytes = texture.EncodeToPNG();
                }
                catch (UnityException)
                {
                    Log.Warning(String.Format("Texture \"{0}\" is marked as read-only, running workaround..", texture.name));
                }

                if (bytes == null)
                {
                    try
                    {
                        var rt = RenderTexture.GetTemporary(texture.width, texture.height, 0);
                        Graphics.Blit(texture, rt);
                        Util.DumpRenderTexture(rt, filename);
                        RenderTexture.ReleaseTemporary(rt);
                        Log.Warning(String.Format("Texture dumped to \"{0}\"", filename));
                    }
                    catch (Exception ex)
                    {
                        Log.Error("There was an error while dumping the texture - " + ex.Message);
                    }

                    return;
                }

                File.WriteAllBytes(filename, bytes);
                Log.Warning(String.Format("Texture dumped to \"{0}\"", filename));
            }
            else
            {
                Log.Warning(String.Format("Don't know how to dump tyoe \"{0}\"", previewTexture.GetType()));
            }
        }

    }
}
