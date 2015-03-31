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

        public static void DumpTextureToPNG(Texture previewTexture, string filename = null)
        {
            if (filename == null)
            {
                filename = "";
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
            }

            if (File.Exists(filename))
            {
                File.Delete(filename);
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
                Log.Error(String.Format("Don't know how to dump type \"{0}\"", previewTexture.GetType()));
            }
        }

        public static void DumpMeshToOBJ(Mesh mesh, string outputPath)
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            Mesh meshToDump = mesh;

            if (!mesh.isReadable)
            {
                Log.Warning(String.Format("Mesh \"{0}\" is marked as non-readable, running workaround..", mesh.name));

                try
                {
                    meshToDump = new Mesh();

                    // copy the relevant data to the temporary mesh 
                    meshToDump.vertices = mesh.vertices;
                    meshToDump.colors = mesh.colors;
                    meshToDump.triangles = mesh.triangles;
                    meshToDump.RecalculateBounds();
                    meshToDump.Optimize();
                }
                catch (Exception ex)
                {
                    Log.Error(String.Format("Workaround failed with error - {0}", ex.Message));
                    return;
                }
            }

            try
            {
                using (var stream = new FileStream(outputPath, FileMode.Create))
                {
                    OBJLoader.ExportOBJ(meshToDump.EncodeOBJ(), stream);
                    stream.Close();
                    Log.Warning(String.Format("Dumped mesh \"{0}\" to \"{1}\"", ((Mesh)mesh).name, outputPath));
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("There was an error while trying to dump mesh \"{0}\" - {1}", mesh.name, ex.Message));
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

        public static void SetFieldValue(FieldInfo field, object o, object value)
        {
            field.SetValue(o, value);
        }

        public static Q GetPrivate<Q>(object o, string fieldName)
        {
            var fields = o.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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

        public static void SetPrivate<Q>(object o, string fieldName, object value)
        {
            var fields = o.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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
            try
            {
                var cameraController = GameObject.FindObjectOfType<CameraController>();
                var mouseWheelZoom = GetPrivate<SavedBool>(cameraController, "m_mouseWheelZoom");
                SetPrivate<bool>(mouseWheelZoom, "m_Value", isEnabled);
            }
            catch (Exception)
            {
            }
        }

        public static bool ComponentIsEnabled(Component component)
        {
            var prop = component.GetType().GetProperty("enabled");
            if (prop == null)
            {
                return true;
            }

            return (bool)prop.GetValue(component, null);
        }

    }

}
