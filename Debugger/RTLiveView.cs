using System;
using UnityEngine;

namespace ModTools
{
    class RTLiveView : GUIWindow
    {

        public RenderTexture previewTexture = null;
        public string caller = "";

        public RTLiveView() : base("RenderTexture LiveView", new Rect(512, 128, 512, 512), skin)
        {
            onDraw = DrawWindow;
        }

        void DrawWindow()
        {
            if (previewTexture != null)
            {
                GUILayout.Label(String.Format("Previewing {0} \"{1}\"", caller, previewTexture.name));
                GUI.DrawTexture(new Rect(0.0f, 40.0f, rect.width, rect.height), previewTexture, ScaleMode.ScaleToFit, false);
            }
            else
            {
                GUILayout.Label("Use the Scene Explorer to select a RenderTexture for live view");
            }
        }

    }
}
