using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using UnityEngine;
using UnityExtension;

namespace ModTools
{

    public class ReferenceChain
    {
        private enum ReferenceType
        {
            GameObject = 0,
            Component = 1,
            Field = 2,
            Property = 3,
            Method = 4,
            EnumerableItem = 5,
            SpecialNamedProperty = 6
        }

        private readonly object[] chainObjects = new object[SceneExplorer.maxHierarchyDepth];
        private readonly ReferenceType[] chainTypes = new ReferenceType[SceneExplorer.maxHierarchyDepth];
        private int count = 0;

        public int Length
        {
            get { return count; }
        }

        public string LastItemName
        {
            get
            {
                return ItemToString(count - 1);
            }
        }

        public bool CheckDepth()
        {
            if (count >= SceneExplorer.maxHierarchyDepth)
            {
                return true;
            }

            return false;
        }

        public ReferenceChain Copy()
        {
            ReferenceChain copy = new ReferenceChain();
            copy.count = count;
            for (int i = 0; i < count; i++)
            {
                copy.chainObjects[i] = chainObjects[i];
                copy.chainTypes[i] = chainTypes[i];
            }

            return copy;
        }
        
        public ReferenceChain Add(GameObject go)
        {
            ReferenceChain copy = Copy();
            copy.chainObjects[count] = go;
            copy.chainTypes[count] = ReferenceType.GameObject;
            copy.count++;
            return copy;
        }

        public ReferenceChain Add(Component component)
        {
            ReferenceChain copy = Copy();
            copy.chainObjects[count] = component;
            copy.chainTypes[count] = ReferenceType.Component;
            copy.count++;
            return copy;
        }

        public ReferenceChain Add(FieldInfo fieldInfo)
        {
            ReferenceChain copy = Copy();
            copy.chainObjects[count] = fieldInfo;
            copy.chainTypes[count] = ReferenceType.Field;
            copy.count++;
            return copy;
        }

        public ReferenceChain Add(PropertyInfo propertyInfo)
        {
            ReferenceChain copy = Copy();
            copy.chainObjects[count] = propertyInfo;
            copy.chainTypes[count] = ReferenceType.Property;
            copy.count++;
            return copy;
        }

        public ReferenceChain Add(MethodInfo methodInfo)
        {
            ReferenceChain copy = Copy();
            copy.chainObjects[count] = methodInfo;
            copy.chainTypes[count] = ReferenceType.Method;
            copy.count++;
            return copy;
        }

        public ReferenceChain Add(int index)
        {
            ReferenceChain copy = Copy();
            copy.chainObjects[count] = index;
            copy.chainTypes[count] = ReferenceType.EnumerableItem;
            copy.count++;
            return copy;
        }

        public ReferenceChain Add(string namedProperty)
        {
            ReferenceChain copy = Copy();
            copy.chainObjects[count] = namedProperty;
            copy.chainTypes[count] = ReferenceType.SpecialNamedProperty;
            copy.count++;
            return copy;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ReferenceChain))
            {
                return false;
            }

            var other = (ReferenceChain) obj;

            if (other.count != count)
            {
                return false;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (chainTypes[i] != other.chainTypes[i])
                {
                    return false;
                }

                if (chainObjects[i].GetHashCode() != other.chainObjects[i].GetHashCode())
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hash = HashCodeHelper.Initialize();

            for(int i = 0; i < count; i++)
            {
                hash = HashCodeHelper.Hash(hash, chainTypes[i]);
                hash = HashCodeHelper.Hash(hash, chainObjects[i]);
            }

            return hash;
        }

        private string ItemToString(int i)
        {
            switch (chainTypes[i])
            {
                case ReferenceType.GameObject:
                    return ((GameObject)chainObjects[i]).name;
                case ReferenceType.Component:
                    return ((Component)chainObjects[i]).name;
                case ReferenceType.Field:
                    return ((FieldInfo)chainObjects[i]).Name;
                case ReferenceType.Property:
                    return ((PropertyInfo)chainObjects[i]).Name;
                case ReferenceType.Method:
                    return ((MethodInfo)chainObjects[i]).Name;
                case ReferenceType.EnumerableItem:
                    return String.Format("[{0}]", (int)chainObjects[i]);
                case ReferenceType.SpecialNamedProperty:
                    return (string)chainObjects[i];
            }

            return "";
        }

        public override string ToString()
        {
            string result = "";

            for (int i = 0; i < count; i++)
            {
                result += ItemToString(i);

                if (i != count - 1)
                {
                    result += '.';
                }
            }

            return result;
        }

    }

    public class SceneExplorer : GUIWindow
    {

        public enum FilterType
        {
            FieldsAndProps,
            GameObjects,
            Components
        }

        private float treeIdentSpacing = 16.0f;
        public static int maxHierarchyDepth = 20;

        private Dictionary<ReferenceChain, bool> expanded = new Dictionary<ReferenceChain, bool>();
        private Dictionary<ReferenceChain, bool> expandedComponents = new Dictionary<ReferenceChain, bool>();
        private Dictionary<ReferenceChain, bool> expandedObjects = new Dictionary<ReferenceChain, bool>();

        private Dictionary<ReferenceChain, bool> evaluatedProperties = new Dictionary<ReferenceChain, bool>();

        private Dictionary<int, bool> preventCircularReferences = new Dictionary<int, bool>(); 

        private Dictionary<GameObject, bool> sceneRoots = new Dictionary<GameObject, bool>();

        private Vector2 scrollPosition = Vector2.zero;

        private bool showFields = true;
        private bool showProperties = true;
        private bool showMethods = false;

        private string nameFilter = "";
        private FilterType filterType = FilterType.GameObjects;

        private Watches watches;
        private RTLiveView rtLiveView;

        public static bool debugMode = false;
        public static string debugOutput = "";
        public static string lastCrashReport = "";

        private void AddDebugLine(string line, params System.Object[] arg)
        {
            if (debugMode)
            {
                debugOutput += String.Format(line, arg) + '\n';
            }
        }

        public SceneExplorer()
            : base("Scene Explorer", new Rect(128, 440, 800, 500), skin)
        {
            onDraw = DrawWindow;
            onException = ExceptionHandler;
        }

        void ExceptionHandler(Exception ex)
        {
            Log.Error("Exception in Scene Explorer - " + ex.Message);

            if (debugMode)
            {
                var filename = "ModTools_Crash_Report.log";
                File.WriteAllText(filename, lastCrashReport);
                Log.Warning(String.Format("ModTools - crash log dumped to \"{0}\"", filename));
            }

            expanded.Clear();
            expandedComponents.Clear();
            expandedObjects.Clear();
            evaluatedProperties.Clear();
        }

        public void Refresh()
        {
            sceneRoots = FindSceneRoots();
        }

        private Dictionary<GameObject, bool> FindSceneRoots()
        {
            Dictionary<GameObject, bool> roots = new Dictionary<GameObject, bool>();

            GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in objects)
            {
                if (!roots.ContainsKey(obj.transform.root.gameObject))
                {
                    roots.Add(obj.transform.root.gameObject, true);
                }
            }

            return roots;
        }

        private bool IsBuiltInType(Type t)
        {
            switch (t.ToString())
            {
                case "System.Char":
                case "System.String":
                case "System.Boolean":
                case "System.Single":
                case "System.Double":
                case "System.Byte":
                case "System.Int32":
                case "System.UInt32":
                case "System.Int64":
                case "System.UInt64":
                case "System.Int16":
                case "System.UInt16":
                case "UnityEngine.Vector2":
                case "UnityEngine.Vector3":
                case "UnityEngine.Vector4":
                case "UnityEngine.Quaternion":
                case "UnityEngine.Color":
                case "UnityEngine.Color32":
                    return true;
            }

            return false;
        }

        private bool IsReflectableType(Type t)
        {
            if (IsBuiltInType(t))
            {
                return false;
            }

            if (t.IsEnum)
            {
                return false;
            }

            return true;
        }

        private bool IsTextureType(Type t)
        {
            if (t == typeof (UnityEngine.Texture) || t == typeof (UnityEngine.Texture2D) ||
                t == typeof (UnityEngine.RenderTexture))

            {
                return true;
            }

            return false;
        }

        private void OnSceneTreeReflectField(ReferenceChain refChain, System.Object obj, FieldInfo field)
        {
            AddDebugLine("OnSceneTreeReflectField(caller = {0}, obj = {1}, field = {2})",
                refChain, obj, field);

            var hash = refChain.GetHashCode().ToString();

            if (refChain.CheckDepth())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            if (obj == null || field == null)
            {
                GUILayout.Label("null");
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));

            GUI.contentColor = Color.white;

            object value = null;

            try
            {
                value = field.GetValue(obj);
            }
            catch (Exception)
            {
            }

            if (value != null && IsReflectableType(field.FieldType) && !IsEnumerable(obj))
            {
                if (expandedObjects.ContainsKey(refChain))
                {
                    if (GUILayout.Button("-", GUILayout.Width(16)))
                    {
                        expandedObjects.Remove(refChain);
                    }
                }
                else
                {
                    if (GUILayout.Button("+", GUILayout.Width(16)))
                    {
                        expandedObjects.Add(refChain, true);
                    }
                }
            }

            if (field.IsPublic)
            {
                GUILayout.Label("public ");
            }
            else if (field.IsPrivate)
            {
                GUILayout.Label("private ");
            }

            GUI.contentColor = Color.white;

            GUILayout.Label("field ");

            if (field.IsStatic)
            {
                GUI.contentColor = Color.blue;
                GUILayout.Label("static ");
            }

            if (field.IsInitOnly)
            {
                GUI.contentColor = Color.blue;
                GUI.enabled = false;
                GUILayout.Label("const ");
            }

            GUI.contentColor = Color.green;
            GUILayout.Label(field.FieldType.ToString() + " ");

            GUI.contentColor = Color.red;

            GUILayout.Label(field.Name);

            GUI.contentColor = Color.white;

            if (value == null || !IsBuiltInType(field.FieldType))
            {
                GUI.contentColor = Color.white;
                GUILayout.Label(" = ");
                GUI.contentColor = Color.white;
                GUILayout.Label(value == null ? "null" : value.ToString());
                GUI.contentColor = Color.white;
            }
            else if (field.FieldType.ToString() == "System.Single")
            {
                var f = (float)value;
                GUIControls.FloatField(hash, "", ref f, 0.0f, true, true);
                if (f != (float)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.Double")
            {
                var f = (double)value;
                GUIControls.DoubleField(hash, "", ref f, 0.0f, true, true);
                if (f != (double)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.Byte")
            {
                var f = (byte)value;
                GUIControls.ByteField(hash, "", ref f, 0.0f, true, true);
                if (f != (byte)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.Int32")
            {
                var f = (int)value;
                GUIControls.IntField(hash, "", ref f, 0.0f, true, true);
                if (f != (int)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.UInt32")
            {
                var f = (uint)value;
                GUIControls.UIntField(hash, "", ref f, 0.0f, true, true);
                if (f != (uint)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.Int64")
            {
                var f = (Int64)value;
                GUIControls.Int64Field(hash, "", ref f, 0.0f, true, true);
                if (f != (Int64)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.UInt64")
            {
                var f = (UInt64)value;
                GUIControls.UInt64Field(hash, "", ref f, 0.0f, true, true);
                if (f != (UInt64)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.Int16")
            {
                var f = (Int16)value;
                GUIControls.Int16Field(hash, "", ref f, 0.0f, true, true);
                if (f != (Int16)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.UInt16")
            {
                var f = (UInt16)value;
                GUIControls.UInt16Field(hash, "", ref f, 0.0f, true, true);
                if (f != (UInt16)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.Boolean")
            {
                var f = (bool)value;
                GUIControls.BoolField("", ref f, 0.0f, true, true);
                if (f != (bool)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.String")
            {
                var f = (string)value;
                GUIControls.StringField(hash, "", ref f, 0.0f, true, true);
                if (f != (string)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.Char")
            {
                var f = (char)value;
                GUIControls.CharField(hash, "", ref f, 0.0f, true, true);
                if (f != (char)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "UnityEngine.Vector2")
            {
                var f = (Vector2)value;
                GUIControls.Vector2Field(hash, "", ref f, 0.0f, null, true, true);
                if (f != (Vector2)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "UnityEngine.Vector3")
            {
                var f = (Vector3)value;
                GUIControls.Vector3Field(hash, "", ref f, 0.0f, null, true, true);
                if (f != (Vector3)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "UnityEngine.Vector4")
            {
                var f = (Vector4)value;
                GUIControls.Vector4Field(hash, "", ref f, 0.0f, null, true, true);
                if (f != (Vector4)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "UnityEngine.Quaternion")
            {
                var f = (Quaternion)value;
                GUIControls.QuaternionField(hash, "", ref f, 0.0f, null, true, true);
                if (f != (Quaternion)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "UnityEngine.Color")
            {
                var f = (Color)value;
                GUIControls.ColorField(hash, "", ref f, 0.0f, null, true, true);
                if (f != (Color)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "UnityEngine.Color32")
            {
                var f = (Color32)value;
                GUIControls.Color32Field(hash, "", ref f, 0.0f, null, true, true);
                var v = (Color32)value;
                if (f.r != v.r || f.g != v.g || f.b != v.b || f.a != v.a)
                {
                    field.SetValue(obj, f);
                }
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Watch"))
            {
                watches.AddWatch(refChain, field, obj);
            }

            if (IsTextureType(field.FieldType) && value != null)
            {
                if (GUILayout.Button("LiveView"))
                {
                    rtLiveView.previewTexture = (Texture)value;
                    rtLiveView.caller = refChain;
                    rtLiveView.visible = true;
                }

                if (GUILayout.Button("Dump .png"))
                {
                    RTLiveView.DumpTextureToPNG((Texture)value);
                }
            }
            else if (field.FieldType.ToString() == "UnityEngine.Mesh" && value != null)
            {
                if (((Mesh)value).isReadable)
                {
                    if (GUILayout.Button("Dump .obj"))
                    {
                        var outPath = refChain.ToString() + ".obj";
                        outPath = outPath.Replace(' ', '_');
                        Util.DumpMeshOBJ(value as Mesh, outPath);
                    }
                }
            }

            GUILayout.EndHorizontal();
            if (value != null && IsReflectableType(field.FieldType) && expandedObjects.ContainsKey(refChain))
            {
                if (value is GameObject)
                {
                    var go = value as GameObject;
                    foreach (var component in go.GetComponents<Component>())
                    {
                        OnSceneTreeComponent(refChain, component);
                    }
                }
                else if (value is Transform)
                {
                    OnSceneTreeReflectUnityEngineTransform(refChain, (Transform)value);
                }
                else
                {
                    OnSceneTreeReflect(refChain, value);
                }
            }
        }

        private void OnSceneTreeReflectProperty(ReferenceChain refChain, System.Object obj, PropertyInfo property)
        {
            AddDebugLine("OnSceneTreeReflectProperty(caller = {0}, obj = {1}, property = {2})",
                refChain, obj, property);

            var hash = refChain.GetHashCode().ToString();

            if (refChain.CheckDepth())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            if (obj == null || property == null)
            {
                GUILayout.Label("null");
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));

            bool propertyWasEvaluated = false;
            object value = null;

            if (property.CanRead && ModTools.evaluatePropertiesAutomatically || evaluatedProperties.ContainsKey(refChain))
            {
                try
                {
                    value = property.GetValue(obj, null);
                    propertyWasEvaluated = true;
                }
                catch (Exception)
                {
                }

                if (value != null && IsReflectableType(property.PropertyType) && !IsEnumerable(obj))
                {
                    if (expandedObjects.ContainsKey(refChain))
                    {
                        if (GUILayout.Button("-", GUILayout.Width(16)))
                        {
                            expandedObjects.Remove(refChain);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("+", GUILayout.Width(16)))
                        {
                            expandedObjects.Add(refChain, true);
                        }
                    }
                }
            }
            
            GUI.contentColor = Color.white;

            GUILayout.Label("property ");
            
            if (!property.CanWrite)
            {
                GUI.contentColor = Color.blue;
                GUI.enabled = false;
                GUILayout.Label("const ");
            }
            GUI.contentColor = Color.green;

            GUILayout.Label(property.PropertyType.ToString() + " ");

            GUI.contentColor = Color.red;

            GUILayout.Label(property.Name);

            GUI.contentColor = Color.white;

            if (!ModTools.evaluatePropertiesAutomatically && !evaluatedProperties.ContainsKey(refChain))
            {
                GUI.enabled = true;

                if(GUILayout.Button("Evaluate"))
                {
                    evaluatedProperties.Add(refChain, true);    
                }
            }
            else
            {
                if (!propertyWasEvaluated && property.CanRead)
                {
                    try
                    {
                        value = property.GetValue(obj, null);
                        propertyWasEvaluated = true;
                    }
                    catch (Exception)
                    {
                    }
                }


                if (value == null || !IsBuiltInType(property.PropertyType))
                {
                    GUI.contentColor = Color.white;
                    GUILayout.Label(" = ");
                    GUI.contentColor = Color.white;

                    if (property.CanRead)
                    {
                        GUILayout.Label(value == null ? "null" : value.ToString());
                    }
                    else
                    {
                        GUILayout.Label("(no get method)");
                    }

                    GUI.contentColor = Color.white;
                }
                else if (property.PropertyType.ToString() == "System.Single")
                {
                    var f = (float)value;
                    GUIControls.FloatField(hash, "", ref f, 0.0f, true, true);
                    if (f != (float)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "System.Double")
                {
                    var f = (double)value;
                    GUIControls.DoubleField(hash, "", ref f, 0.0f, true, true);
                    if (f != (double)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "System.Byte")
                {
                    var f = (byte)value;
                    GUIControls.ByteField(hash, "", ref f, 0.0f, true, true);
                    if (f != (byte)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "System.Int32")
                {
                    var f = (int)value;
                    GUIControls.IntField(hash, "", ref f, 0.0f, true, true);
                    if (f != (int)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "System.UInt32")
                {
                    var f = (uint)value;
                    GUIControls.UIntField(hash, "", ref f, 0.0f, true, true);
                    if (f != (uint)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "System.Int64")
                {
                    var f = (Int64)value;
                    GUIControls.Int64Field(hash, "", ref f, 0.0f, true, true);
                    if (f != (Int64)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "System.UInt64")
                {
                    var f = (UInt64)value;
                    GUIControls.UInt64Field(hash, "", ref f, 0.0f, true, true);
                    if (f != (UInt64)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "System.Int16")
                {
                    var f = (Int16)value;
                    GUIControls.Int16Field(hash, "", ref f, 0.0f, true, true);
                    if (f != (Int16)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "System.UInt16")
                {
                    var f = (UInt16)value;
                    GUIControls.UInt16Field(hash, "", ref f, 0.0f, true, true);
                    if (f != (UInt16)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "System.Boolean")
                {
                    var f = (bool)value;
                    GUIControls.BoolField("", ref f, 0.0f, true, true);
                    if (f != (bool)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "System.String")
                {
                    var f = (string)value;
                    GUIControls.StringField(hash, "", ref f, 0.0f, true, true);
                    if (f != (string)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "System.Char")
                {
                    var f = (char)value;
                    GUIControls.CharField(hash, "", ref f, 0.0f, true, true);
                    if (f != (char)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "UnityEngine.Vector2")
                {
                    var f = (Vector2)value;
                    GUIControls.Vector2Field(hash, "", ref f, 0.0f, null, true, true);
                    if (f != (Vector2)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "UnityEngine.Vector3")
                {
                    var f = (Vector3)value;
                    GUIControls.Vector3Field(hash, "", ref f, 0.0f, null, true, true);
                    if (f != (Vector3)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "UnityEngine.Vector4")
                {
                    var f = (Vector4)value;
                    GUIControls.Vector4Field(hash, "", ref f, 0.0f, null, true, true);
                    if (f != (Vector4)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "UnityEngine.Quaternion")
                {
                    var f = (Quaternion)value;
                    GUIControls.QuaternionField(hash, "", ref f, 0.0f, null, true, true);
                    if (f != (Quaternion)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "UnityEngine.Color")
                {
                    var f = (Color)value;
                    GUIControls.ColorField(hash, "", ref f, 0.0f, null, true, true);
                    if (f != (Color)value)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
                else if (property.PropertyType.ToString() == "UnityEngine.Color32")
                {
                    var f = (Color32)value;
                    GUIControls.Color32Field(hash, "", ref f, 0.0f, null, true, true);
                    var v = (Color32)value;
                    if (f.r != v.r || f.g != v.g || f.b != v.b || f.a != v.a)
                    {
                        property.SetValue(obj, f, null);
                    }
                }
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Watch"))
            {
                watches.AddWatch(refChain, property, obj);
            }

            if (IsTextureType(property.PropertyType) && value != null)
            {
                if (GUILayout.Button("LiveView"))
                {
                    rtLiveView.previewTexture = (Texture)value;
                    rtLiveView.caller = refChain;
                    rtLiveView.visible = true;
                }

                if (GUILayout.Button("Dump .png"))
                {
                    RTLiveView.DumpTextureToPNG((Texture)value);
                }
            }
            else if (property.PropertyType.ToString() == "UnityEngine.Mesh" && value != null)
            {
                if (((Mesh) value).isReadable)
                {
                    if (GUILayout.Button("Dump .obj"))
                    {
                        var outPath = refChain.ToString() + ".obj";
                        outPath = outPath.Replace(' ', '_');
                        Util.DumpMeshOBJ(value as Mesh, outPath);
                    }
                }
            }
            
            GUILayout.EndHorizontal();

            if (value != null && expandedObjects.ContainsKey(refChain))
            {
                if (value is GameObject)
                {
                    var go = value as GameObject;
                    foreach (var component in go.GetComponents<Component>())
                    {
                       OnSceneTreeComponent(refChain, component);
                    }
                }
                else if (value is Transform)
                {
                    OnSceneTreeReflectUnityEngineTransform(refChain, (Transform)value);
                }
                else
                {
                    OnSceneTreeReflect(refChain, value);
                }
            }
        }

        private void OnSceneTreeReflectMethod(ReferenceChain refChain, System.Object obj, MethodInfo method)
        {
            AddDebugLine("OnSceneTreeReflectMethod(obj = {0}, method = {1})", obj, method);

            if (refChain.CheckDepth())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            if (obj == null || method == null)
            {
                GUILayout.Label("null");
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));

            GUILayout.Label("method ");
            string signature = method.ReturnType.ToString() + " " + method.Name + "(";

            bool first = true;
            var parameters = method.GetParameters();
            foreach (var param in parameters)
            {
                if (!first)
                {
                    signature += ", ";
                }
                else
                {
                    first = false;
                }

                signature += param.ParameterType.ToString() + " " + param.Name;
            }

            signature += ")";

            GUILayout.Label(signature);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void OnSceneTreeReflectUnityEngineVector3<T>(ReferenceChain refChain, T obj, string name, ref UnityEngine.Vector3 vec)
        {
            AddDebugLine("OnSceneTreeReflectUnityEngineVector3(caller = {0}, obj = {1}, name = {2}, vec = {3})",
                refChain, obj, name, vec);
            
            if (refChain.CheckDepth())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            GUIControls.Vector3Field(refChain.ToString(), name, ref vec, treeIdentSpacing * (refChain.Length - 1), () =>
            {
                try
                {
                    watches.AddWatch(refChain, typeof(T).GetProperty(name), obj);
                }
                catch (Exception ex)
                {
                    Log.Error("Exception in ModTools:OnSceneTreeReflectUnityEngineVector3 - " + ex.Message);
                }
            });
        }

        private void OnSceneTreeReflectUnityEngineTransform(ReferenceChain refChain, UnityEngine.Transform transform)
        {
            AddDebugLine("OnSceneTreeReflectUnityEngineTransform(caller = {0}, transform = {1})", refChain, transform);

            if (refChain.CheckDepth())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            if (transform == null)
            {
                GUILayout.Label("null");
                return;
            }

            var hash = refChain.GetHashCode().ToString();

            var localPosition = transform.localPosition;
            OnSceneTreeReflectUnityEngineVector3(refChain.Add("localPosition"), transform, "localPosition", ref localPosition);
            transform.localPosition = localPosition;

            var localEulerAngles = transform.eulerAngles;
            OnSceneTreeReflectUnityEngineVector3(refChain.Add("localEulerAngles"), transform, "localEulerAngles", ref localEulerAngles);
            transform.eulerAngles = localEulerAngles;

            var localScale = transform.localScale;
            OnSceneTreeReflectUnityEngineVector3(refChain.Add("localScale"), transform, "localScale", ref localScale);
            transform.localScale = localScale;
        }

        private static readonly string[] textureProps = new string[28]
        {
          "_BackTex",
          "_BumpMap",
          "_BumpSpecMap",
          "_Control",
          "_DecalTex",
          "_Detail",
          "_DownTex",
          "_FrontTex",
          "_GlossMap",
          "_Illum",
          "_LeftTex",
          "_LightMap",
          "_LightTextureB0",
          "_MainTex",
          "_XYSMap",
          "_ACIMap",
          "_XYCAMap",
          "_ParallaxMap",
          "_RightTex",
          "_ShadowOffset",
          "_Splat0",
          "_Splat1",
          "_Splat2",
          "_Splat3",
          "_TranslucencyMap",
          "_UpTex",
          "_Tex",
          "_Cube"
        };
        private static readonly string[] colorProps = new string[5]
        {
          "_Color",
          "_ColorV0",
          "_ColorV1",
          "_ColorV2",
          "_ColorV3"
        };
        private static readonly string[] vectorProps = new string[4]
        {
          "_FloorParams",
          "_UvAnimation",
          "_WindAnimation",
          "_WindAnimationB"
        };

        private void OnSceneReflectUnityEngineMaterial(ReferenceChain refChain, UnityEngine.Material material)
        {
            AddDebugLine("OnSceneTreeReflectUnityEngineTransform(caller = {0}, material = {1})", refChain, material);

            var hash = refChain.GetHashCode().ToString();

            if (refChain.CheckDepth())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            if(material == null)
            {
                GUILayout.Label("null");
                return;
            }

            ReferenceChain oldRefChain = refChain;

            foreach (string prop in textureProps)
            {
                Texture value = material.GetTexture(prop);
                if (value == null)
                    continue;
                refChain = oldRefChain.Add(prop);

                var type = value.GetType();

                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * refChain.Length);

                GUI.contentColor = Color.white;

                if (IsReflectableType(type) && !IsEnumerable(type))
                {
                    if (expandedObjects.ContainsKey(refChain))
                    {
                        if (GUILayout.Button("-", GUILayout.Width(16)))
                        {
                            expandedObjects.Remove(refChain);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("+", GUILayout.Width(16)))
                        {
                            expandedObjects.Add(refChain, true);
                        }
                    }
                }

                GUI.contentColor = Color.green;

                GUILayout.Label(type.ToString() + " ");

                GUI.contentColor = Color.red;

                GUILayout.Label(prop);

                GUI.contentColor = Color.white;

                GUILayout.Label(" = ");
                GUILayout.Label(value == null ? "null" : value.ToString());

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("LiveView"))
                {
                    rtLiveView.previewTexture = (Texture)value;
                    rtLiveView.caller = refChain;
                    rtLiveView.visible = true;
                }

                if (GUILayout.Button("Dump .png"))
                {
                    RTLiveView.DumpTextureToPNG((Texture)value);
                }
                GUILayout.EndHorizontal();

                if (IsReflectableType(type) && expandedObjects.ContainsKey(refChain))
                {
                    OnSceneTreeReflect(refChain, value);
                }

            }
            foreach (string prop in colorProps)
            {
                Color value = material.GetColor(prop);
                if (value == null)
                    continue;
                refChain = oldRefChain.Add(prop);

                var type = value.GetType();

                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * refChain.Length);

                GUI.contentColor = Color.white;

                if (IsReflectableType(type) && !IsEnumerable(type))
                {
                    if (expandedObjects.ContainsKey(refChain))
                    {
                        if (GUILayout.Button("-", GUILayout.Width(16)))
                        {
                            expandedObjects.Remove(refChain);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("+", GUILayout.Width(16)))
                        {
                            expandedObjects.Add(refChain, true);
                        }
                    }
                }

                GUI.contentColor = Color.green;

                GUILayout.Label(type.ToString() + " ");

                GUI.contentColor = Color.red;

                GUILayout.Label(prop);

                GUI.contentColor = Color.white;

                GUILayout.Label(" = ");
                var f = value;
                GUIControls.ColorField(hash, "", ref f, 0.0f, null, true, true);
                if (f != value)
                {
                    material.SetColor(prop, f);
                }
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();

                if (IsReflectableType(type) && expandedObjects.ContainsKey(refChain))
                {
                    OnSceneTreeReflect(refChain, value);
                }

            }

        }

        private bool IsEnumerable(object myProperty)
        {
            if (typeof(IEnumerable).IsAssignableFrom(myProperty.GetType())
                || typeof(IEnumerable<>).IsAssignableFrom(myProperty.GetType()))
                return true;

            return false;
        }

        private void OnSceneTreeReflectIEnumerable(ReferenceChain refChain, System.Object myProperty)
        {
            AddDebugLine("OnSceneTreeReflectIEnumerable(caller = {0}, obj = {1})", refChain, myProperty);

            if (refChain.CheckDepth())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            var enumerable = myProperty as IEnumerable;
            if (enumerable == null)
            {
                return;
            }

            int count = 0;
            var oldRefChain = refChain;

            foreach (var value in enumerable)
            {
                refChain = oldRefChain.Add(count);

                var type = value.GetType();

                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * refChain.Length);

                GUI.contentColor = Color.white;

                if (IsReflectableType(type) && !IsEnumerable(type))
                {
                    if (expandedObjects.ContainsKey(refChain))
                    {
                        if (GUILayout.Button("-", GUILayout.Width(16)))
                        {
                            expandedObjects.Remove(refChain);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("+", GUILayout.Width(16)))
                        {
                            expandedObjects.Add(refChain, true);
                        }
                    }
                }

                GUI.contentColor = Color.green;

                GUILayout.Label(type.ToString() + " ");

                GUI.contentColor = Color.red;

                GUILayout.Label(String.Format("{0}.[{1}]", oldRefChain.LastItemName, count));

                GUI.contentColor = Color.white;

                GUILayout.Label(" = ");
                GUILayout.Label(value == null ? "null" : value.ToString());

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (IsReflectableType(type) && expandedObjects.ContainsKey(refChain))
                {
                    if (value is GameObject)
                    {
                        var go = value as GameObject;
                        foreach (var component in go.GetComponents<Component>())
                        {
                            OnSceneTreeComponent(refChain, component);
                        }
                    }
                    else if (value is Transform)
                    {
                        OnSceneTreeReflectUnityEngineTransform(refChain, (Transform)value);
                    }
                    else
                    {
                        OnSceneTreeReflect(refChain, value);
                    }
                }

                count++;
                if (count >= 128)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(treeIdentSpacing * refChain.Length);
                    GUILayout.Label("Array too large to display");
                    GUILayout.EndHorizontal();
                    break;
                }
            }
        }

        private void OnSceneTreeReflect(ReferenceChain refChain, System.Object obj)
        {
            if (refChain.CheckDepth())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            if (obj == null)
            {
                GUILayout.Label("null");
                return;
            }

            if (preventCircularReferences.ContainsKey(obj.GetHashCode()))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));
                GUILayout.Label("Circular reference detected");
                GUILayout.EndHorizontal();
                return;
            }

            preventCircularReferences.Add(obj.GetHashCode(), true);

            Type type = obj.GetType();

            if (type == typeof(UnityEngine.Transform))
            {
                OnSceneTreeReflectUnityEngineTransform(refChain, (UnityEngine.Transform)obj);
                return;
            }
            
            if (IsEnumerable(obj))
            {
                OnSceneTreeReflectIEnumerable(refChain, obj);
                return;
            }

            if(type == typeof(Material))
            {
                OnSceneReflectUnityEngineMaterial(refChain, (UnityEngine.Material)obj);
                return;
            }

            MemberInfo[] fields = type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            foreach (MemberInfo member in fields)
            {
                if (filterType == FilterType.FieldsAndProps && !member.Name.ToLower().Contains(nameFilter))
                {
                    continue;
                }

                FieldInfo field = null;
                PropertyInfo property = null;
                MethodInfo method = null;

                if (member.MemberType == MemberTypes.Field && showFields)
                {
                    field = (FieldInfo)member;

                    try
                    {
                        OnSceneTreeReflectField(refChain.Add(field), obj, field);
                    }
                    catch (Exception ex)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));
                        GUILayout.Label(String.Format("Exception when fetching field \"{0}\" - {1}", field.Name, ex.Message));
                        GUILayout.EndHorizontal();
                    }
                }
                else if (member.MemberType == MemberTypes.Property && showProperties)
                {
                    property = (PropertyInfo)member;

                    try
                    {
                        OnSceneTreeReflectProperty(refChain.Add(property), obj, property);
                    }
                    catch (Exception ex)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));
                        GUILayout.Label(String.Format("Exception when fetching property \"{0}\" - {1}", property.Name, ex.Message));
                        GUILayout.EndHorizontal();
                    }
                }
                else if (member.MemberType == MemberTypes.Method && showMethods)
                {
                    method = (MethodInfo)member;

                    try
                    {
                        OnSceneTreeReflectMethod(refChain.Add(method), obj, method);
                    }
                    catch (Exception ex)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));
                        GUILayout.Label(String.Format("Exception when fetching method \"{0}\" - {1}", method.Name, ex.Message));
                        GUILayout.EndHorizontal();
                    }
                }

                if (field == null)
                {
                    continue;
                }
            }
        }

        private void OnSceneTreeComponent(ReferenceChain refChain, Component component)
        {
            if (refChain.CheckDepth())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

          
            if (filterType == FilterType.Components && !component.name.ToLower().Contains(nameFilter))
            {
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));

            if (expandedComponents.ContainsKey(refChain))
            {
                if (GUILayout.Button("-", GUILayout.Width(16)))
                {
                    expandedComponents.Remove(refChain);
                }

                GUILayout.Label(component.name + " (" + component.GetType().ToString() + ")");

                GUILayout.EndHorizontal();

                OnSceneTreeReflect(refChain, component);
            }
            else
            {
                if (GUILayout.Button("+", GUILayout.Width(16)))
                {
                    expandedComponents.Add(refChain, true);
                }

                GUILayout.Label(component.name + " (" + component.GetType().ToString() + ")");
                GUILayout.EndHorizontal();
            }
        }

        private void OnSceneTreeRecursive(ReferenceChain refChain, GameObject obj)
        {
            AddDebugLine("OnSceneTreeRecursive(caller = {0}, obj = {1})", refChain, obj);

            if (refChain.CheckDepth())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            if (obj == null)
            {
                GUILayout.Label("null");
                return;
            }

            if (filterType == FilterType.GameObjects && !obj.name.ToLower().Contains(nameFilter))
            {
                return;
            }

            if (expanded.ContainsKey(refChain))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));

                if (GUILayout.Button("-", GUILayout.Width(16)))
                {
                    expanded.Remove(refChain);
                }

                GUILayout.Label(obj.name);
                GUILayout.EndHorizontal();

                var components = obj.GetComponents(typeof(Component));
                foreach (var component in components)
                {
                    OnSceneTreeComponent(refChain.Add(component), component);
                }

                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    OnSceneTreeRecursive(refChain.Add(obj.transform.GetChild(i)), obj.transform.GetChild(i).gameObject);
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * (refChain.Length - 1));

                if (GUILayout.Button("+", GUILayout.Width(16)))
                {
                    expanded.Add(refChain, true);
                }

                GUILayout.Label(obj.name);
                GUILayout.EndHorizontal();
            }
        }

        public void DrawWindow()
        {
            if (debugMode)
            {
                lastCrashReport = debugOutput;
                debugOutput = "";
            }

            preventCircularReferences.Clear();

            watches = watches ?? FindObjectOfType<Watches>();
            if (watches == null)
            {
                GUILayout.Label("Required component \"Watches\" is missing.");
                return;
            }

            rtLiveView = rtLiveView ?? FindObjectOfType<RTLiveView>();
            if (rtLiveView == null)
            {
                GUILayout.Label("Required component \"RTLiveView\" is missing.");
                return;
            }

            if (GUILayout.Button("Refresh", GUILayout.Width(256)))
            {
                sceneRoots = FindSceneRoots();
            }

            GUILayout.BeginHorizontal();

            GUILayout.Label("Show: ");
            GUILayout.FlexibleSpace();

            GUILayout.Label("Fields");
            showFields = GUILayout.Toggle(showFields, "");
            GUILayout.FlexibleSpace();

            GUILayout.Label("Properties");
            showProperties = GUILayout.Toggle(showProperties, "");
            GUILayout.FlexibleSpace();

            GUILayout.Label("Methods");
            showMethods = GUILayout.Toggle(showMethods, "");
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUIControls.StringField("ModTools.NameFilter", "Filter", ref nameFilter, 0.0f, true, true);
            nameFilter = nameFilter.Trim().ToLower();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            bool filterGameObject = filterType == FilterType.GameObjects;
            bool filterComponent = filterType == FilterType.Components;
            bool filterFields = filterType == FilterType.FieldsAndProps;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Filter type:");

            GUILayout.FlexibleSpace();

            GUILayout.Label("GameObject");
            filterGameObject = GUILayout.Toggle(filterGameObject, "");
            if (filterGameObject)
            {
                filterComponent = false;
                filterFields = false;
            }

            GUILayout.FlexibleSpace();

            GUILayout.Label("Component");
            filterComponent = GUILayout.Toggle(filterComponent, "");
            if (filterComponent)
            {
                filterFields = false;
            }

            GUILayout.FlexibleSpace();

            GUILayout.Label("Fields and properties");
            filterFields = GUILayout.Toggle(filterFields, "");
            if (filterFields)
            {
                filterType = FilterType.FieldsAndProps;
            }
            else if (filterComponent)
            {
                filterType = FilterType.Components;
            }
            else
            {
                filterType = FilterType.GameObjects;
            }

            GUILayout.EndHorizontal();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (var obj in sceneRoots)
            {
                OnSceneTreeRecursive(new ReferenceChain().Add(obj.Key), obj.Key);
            }

            GUILayout.EndScrollView();

            if (GUILayout.Button("Fold all"))
            {
                expanded.Clear();
                expandedComponents.Clear();
                expandedObjects.Clear();
                evaluatedProperties.Clear();
            }
        }

    }

}