using System;
using ICities;
using UnityEngine;

namespace ModTools
{
    public class MeshViewer : GUIWindow
    {

        public Mesh previewMesh = null;
        public ReferenceChain caller = null;

        private RenderTexture targetRT;

        private float zoom = 100.0f;

        private Quaternion rotation = Quaternion.identity;
        private Vector2 lastMousePos = Vector2.zero;

        private Camera meshViewerCamera;

        private Material material;
        private Light light;

        private MeshViewer()
            : base("Mesh Viewer", new Rect(512, 128, 512, 512), skin)
        {
            onDraw = DrawWindow;

            material = new Material(Shader.Find("Diffuse"));
           
            try
            {
                light = GameObject.Find("Directional Light").GetComponent<Light>();
            }
            catch (Exception)
            {
                light = null;
            }

            meshViewerCamera = gameObject.AddComponent<Camera>();
            meshViewerCamera.transform.position = new Vector3(-10000.0f, -10000.0f, -10000.0f);
            meshViewerCamera.fieldOfView = 20.0f;
            meshViewerCamera.backgroundColor = Color.grey;
            meshViewerCamera.nearClipPlane = 1.0f;
            meshViewerCamera.farClipPlane = 1000.0f;
            meshViewerCamera.enabled = false;

            targetRT = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            meshViewerCamera.targetTexture = targetRT;
        }

        public static MeshViewer CreateMeshViewer(ReferenceChain refChain, Mesh mesh)
        {
            var go = new GameObject("MeshViewer");
            go.transform.parent = ModTools.Instance.transform;
            var meshViewer = go.AddComponent<MeshViewer>();
            meshViewer.caller = refChain;
            meshViewer.previewMesh = mesh;
            meshViewer.visible = true;
            return meshViewer;
        }

        void Update()
        {
            if (previewMesh == null)
            {
                return;
            }

            float intensity = 1.0f;

            if (light != null)
            {
                intensity = light.intensity;
                light.intensity = 0.6f;
            }

            var pos = meshViewerCamera.transform.position + new Vector3(0.0f, -zoom, -zoom);
            var trs = Matrix4x4.TRS(pos, rotation, new Vector3(1.0f, 1.0f, 1.0f));
            meshViewerCamera.transform.LookAt(pos, Vector3.up);

            Graphics.DrawMesh(previewMesh, trs, material, 0, meshViewerCamera, 0, null, false, false);
            meshViewerCamera.RenderWithShader(material.shader, "");

            if (light != null)
            {
                light.intensity = intensity;
            }
        }

        void DrawWindow()
        {
            if (previewMesh != null)
            {
                title = String.Format("Previewing \"{0}\"", previewMesh.name);
                
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Dump .obj", GUILayout.Width(128)))
                {
                    Util.DumpMeshOBJ(previewMesh, previewMesh.name + ".obj");
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (Event.current.type == EventType.MouseDown)
                {
                    lastMousePos = Event.current.mousePosition;
                }
                else if (Event.current.type == EventType.MouseDrag)
                {
                    var pos = Event.current.mousePosition;
                    if (lastMousePos != Vector2.zero)
                    {
                        var delta = pos - lastMousePos;
                        zoom += delta.y;
                        rotation *= Quaternion.Euler(0.0f, -delta.x, 0.0f);
                    }
                    lastMousePos = pos;
                }
            
                GUI.DrawTexture(new Rect(0.0f, 64.0f, rect.width, rect.height - 64.0f), targetRT, ScaleMode.StretchToFill, false);
            }
            else
            {
                title = "Mesh Viewer";
                GUILayout.Label("Use the Scene Explorer to select a Mesh for preview");
            }
        }



    }
}
