using UnityEngine;

namespace ModTools
{
    public class GraphViewer : GUIWindow
    {

        public GraphViewer() : base("Graph Viewer", new Rect(512, 128, 512, 512), skin)
        {
            onDraw = DrawWindow;
        }

        void DrawWindow()
        {
            
        }

    }
}
