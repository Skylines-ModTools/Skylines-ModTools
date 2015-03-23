using System;
using System.IO;
using UnityEngine;

namespace ModTools
{
    class RTLiveView : GUIWindow
    {

        public Texture previewTexture = null;
        public ReferenceChain caller = null;

        public RTLiveView() : base("RenderTexture LiveView", new Rect(512, 128, 512, 512), skin)
        {
            onDraw = DrawWindow;
        }

        void DrawWindow()
        {
            if (previewTexture != null)
            {
                title = String.Format("Previewing {0} \"{1}\"", caller, previewTexture.name);

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
                title = "RenderTexture LiveView";
                GUILayout.Label("Use the Scene Explorer to select a RenderTexture for live view");
            }
        }

        

    }
}
