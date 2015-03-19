using System;
using System.IO;
using System.Reflection;
using ColossalFramework;
using UnityEngine;
using UnityExtension;

namespace ModTools
{

    public static class Util
    {

        public static bool IsLayout()
        {
            return Event.current.type == EventType.Layout;
        }

        public static void DumpRenderTexture(RenderTexture rt, string pngOutPath)
        {
            var oldRT = RenderTexture.active;

            var tex = new Texture2D(rt.width, rt.height);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            File.WriteAllBytes(pngOutPath, tex.EncodeToPNG());
            RenderTexture.active = oldRT;
        }

        public static void DumpMeshOBJ(Mesh mesh, string outputPath)
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            using (var stream = new FileStream(outputPath, FileMode.Create))
            {
                OBJLoader.ExportOBJ(mesh.EncodeOBJ(), stream);
                stream.Close();
                Log.Warning(String.Format("Dumped mesh \"{0}\" to \"{1}\"", ((Mesh)mesh).name, outputPath));
            }
        }

        public static FieldInfo FindField<T>(T o, string fieldName)
        {
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var f in fields)
            {
                if (f.Name == fieldName)
                {
                    return f;
                }
            }

            return null;
        }

        public static T GetFieldValue<T>(FieldInfo field, object o)
        {
            return (T)field.GetValue(o);
        }

        public static Q ReadPrivate<T, Q>(T o, string fieldName)
        {
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo field = null;

            foreach (var f in fields)
            {
                if (f.Name == fieldName)
                {
                    field = f;
                    break;
                }
            }

            return (Q)field.GetValue(o);
        }

        public static void WritePrivate<T, Q>(T o, string fieldName, object value)
        {
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo field = null;

            foreach (var f in fields)
            {
                if (f.Name == fieldName)
                {
                    field = f;
                    break;
                }
            }

            field.SetValue(o, value);
        }

        public static void SetMouseScrolling(bool isEnabled)
        {
            var cameraController = GameObject.FindObjectOfType<CameraController>();
            var mouseWheelZoom = ReadPrivate<CameraController, SavedBool>(cameraController, "m_mouseWheelZoom");
            WritePrivate<SavedBool, bool>(mouseWheelZoom, "m_Value", isEnabled);
        }

    }

}
