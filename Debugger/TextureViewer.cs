using System;
using UnityEngine;

namespace ModTools
{
    public class TextureViewer : GUIWindow
    {

        public Texture previewTexture = null;
        public ReferenceChain caller = null;

        public TextureViewer() : base("Texture Viewer", new Rect(512, 128, 512, 512), skin)
        {
            onDraw = DrawWindow;
        }

        void DrawWindow()
        {
            if (previewTexture != null)
            {
                title = String.Format("Previewing {0} \"{1}\"", caller.ToString(), previewTexture.name);

                if (GUILayout.Button("Dump .png", GUILayout.Width(128)))
                {
                    Util.DumpTextureToPNG(previewTexture);
                }

                float aspect = (float)previewTexture.width / ((float)previewTexture.height + 38.0f);
                rect.width = rect.height * aspect;
                GUI.DrawTexture(new Rect(0.0f, 38.0f, rect.width, rect.height), previewTexture, ScaleMode.ScaleToFit, false);
            }
            else
            {
                title = "Texture Viewer";
                GUILayout.Label("Use the Scene Explorer to select a Texture for preview");
            }
        }

        

    }
}
