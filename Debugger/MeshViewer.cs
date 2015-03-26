using System;
using UnityEngine;

namespace ModTools
{
    public class MeshViewer : GUIWindow
    {

        public Mesh previewMesh = null;
        public ReferenceChain caller = null;

        private RenderTexture targetRT;

        private Quaternion rotation = Quaternion.identity;
        private Vector2 lastMousePos = Vector2.zero;

        private Material material;

        public MeshViewer()
            : base("Mesh Viewer", new Rect(512, 128, 512, 512), skin)
        {
            onDraw = DrawWindow;

            material = new Material(Shader.Find("VertexLit"));
        }

        void Update()
        {
            if (previewMesh == null)
            {
                return;
            }

            if (targetRT != null)
            {
                RenderTexture.ReleaseTemporary(targetRT);
            }

            targetRT = RenderTexture.GetTemporary((int)rect.width, (int)(rect.height - 38.0f), 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            var oldRT = RenderTexture.active;

            Matrix4x4 trs = Matrix4x4.TRS(new Vector3(1024.0f, 1024.0f, 0.0f), rotation, new Vector3(32.0f, 32.0f, 32.0f));
            Matrix4x4 projection = Matrix4x4.Perspective(45.0f, rect.width/rect.height, 1.0f, 100000.0f);

            GL.Viewport(new Rect(0.0f, 0.0f, rect.width, rect.height - 38.0f));

            GL.PushMatrix();
            GL.LoadProjectionMatrix(projection);
            GL.PopMatrix();
            
            RenderTexture.active = targetRT;
            GL.Clear(true, true, Color.magenta);
            if (material.SetPass(0))
            {
                Graphics.DrawMeshNow(previewMesh, trs);
            }
            RenderTexture.active = oldRT;
        }

        void DrawWindow()
        {
            if (previewMesh != null)
            {
                title = String.Format("Previewing {0} \"{1}\"", caller, previewMesh.name);

                if (GUILayout.Button("Dump .obj", GUILayout.Width(128)))
                {
                    Util.DumpMeshOBJ(previewMesh, previewMesh.name + ".obj");
                }

                if (Event.current.type == EventType.MouseDrag)
                {
                    var pos = Event.current.mousePosition;
                    if (lastMousePos != Vector2.zero)
                    {
                        var delta = pos - lastMousePos;
                        rotation *= Quaternion.Euler(delta.x, delta.y, 0.0f);
                    }
                    lastMousePos = pos;
                }

                GUI.DrawTexture(new Rect(0.0f, 38.0f, rect.width, rect.height), targetRT, ScaleMode.ScaleToFit, false);
            }
            else
            {
                title = "Mesh Viewer";
                GUILayout.Label("Use the Scene Explorer to select a Mesh for preview");
            }
        }



    }
}
