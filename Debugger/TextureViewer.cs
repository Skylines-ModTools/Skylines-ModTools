using System;
using UnityEngine;

namespace ModTools
{
    public class TextureViewer : GUIWindow
    {

        public Texture previewTexture = null;
        public ReferenceChain caller = null;

        private TextureViewer() : base("Texture Viewer", new Rect(512, 128, 512, 512), skin)
        {
            onDraw = DrawWindow;
            onClose = HandleClose;
        }

        public static TextureViewer CreateTextureViewer(ReferenceChain refChain, Texture texture)
        {
            var go = new GameObject("TextureViewer");
            go.transform.parent = ModTools.Instance.transform;
            var textureViewer = go.AddComponent<TextureViewer>();
            textureViewer.caller = refChain;
            textureViewer.previewTexture = texture;
            textureViewer.visible = true;
            return textureViewer;
        }

        void HandleClose()
        {
            Destroy(this);
        }

        void DrawWindow()
        {
            if (previewTexture != null)
            {
                title = String.Format("Previewing \"{0}\"", previewTexture.name);

                if (GUILayout.Button("Dump .png", GUILayout.Width(128)))
                {
                    Util.DumpTextureToPNG(previewTexture);
                }

                float aspect = (float)previewTexture.width / ((float)previewTexture.height + 60.0f);
                rect.width = rect.height * aspect;
                GUI.DrawTexture(new Rect(0.0f, 60.0f, rect.width, rect.height - 60.0f), previewTexture, ScaleMode.ScaleToFit, false);
            }
            else
            {
                title = "Texture Viewer";
                GUILayout.Label("Use the Scene Explorer to select a Texture for preview");
            }
        }

    }
}
