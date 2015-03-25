using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ModTools
{

    public class SceneExplorer : GUIWindow
    {
        private const float treeIdentSpacing = 16.0f;
        public static int maxHierarchyDepth = 20;

        private readonly Dictionary<ReferenceChain, bool> expandedGameObjects = new Dictionary<ReferenceChain, bool>();
        private readonly Dictionary<ReferenceChain, bool> expandedComponents = new Dictionary<ReferenceChain, bool>();
        private readonly Dictionary<ReferenceChain, bool> expandedObjects = new Dictionary<ReferenceChain, bool>();

        private readonly Dictionary<ReferenceChain, bool> evaluatedProperties = new Dictionary<ReferenceChain, bool>();

        private readonly Dictionary<ReferenceChain, int> selectedArrayStartIndices = new Dictionary<ReferenceChain, int>();
        private readonly Dictionary<ReferenceChain, int> selectedArrayEndIndices = new Dictionary<ReferenceChain, int>();

        private readonly Dictionary<int, bool> preventCircularReferences = new Dictionary<int, bool>();

        private Dictionary<GameObject, bool> sceneRoots = new Dictionary<GameObject, bool>();

        private bool showFields = true;
        private bool showProperties = true;
        private bool showMethods = false;

        private string findGameObjectFilter = "";
        private string findObjectTypeFilter = "";
        private string searchDisplayString = "";

        public static bool debugMode = false;
        public static string debugOutput = "";
        public static string lastCrashReport = "";

        private GUIArea headerArea;
        private GUIArea sceneTreeArea;
        private GUIArea componentArea;

        private Vector2 sceneTreeScrollPosition = Vector2.zero;
        private Vector2 componentScrollPosition = Vector2.zero;

        private ReferenceChain currentRefChain = null;

        private bool showSearchBar = false;
        private bool sortAlphabetically = true;

        private float windowTopMargin = 16.0f;
        private float windowBottomMargin = 8.0f;
        private float headerHeight = 140.0f;
        private float sceneTreeWidth = 300.0f;

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

            RecalculateAreas();
        }

        void RecalculateAreas()
        {
            headerArea = new GUIArea(this);
            headerArea.absolutePosition.y = windowTopMargin;
            headerArea.relativeSize.x = 1.0f;
            headerArea.absoluteSize.y = headerHeight - windowTopMargin;

            sceneTreeArea = new GUIArea(this);
            sceneTreeArea.absolutePosition.y = headerHeight - windowTopMargin;
            sceneTreeArea.relativeSize.y = 1.0f;
            sceneTreeArea.absoluteSize.x = sceneTreeWidth;
            sceneTreeArea.absoluteSize.y = -(headerHeight - windowTopMargin) - windowBottomMargin;

            componentArea = new GUIArea(this);
            componentArea.absolutePosition.x = sceneTreeWidth;
            componentArea.absolutePosition.y = headerHeight - windowTopMargin;
            componentArea.relativeSize.x = 1.0f;
            componentArea.relativeSize.y = 1.0f;
            componentArea.absoluteSize.x = -sceneTreeWidth;
            componentArea.absoluteSize.y = -(headerHeight - windowTopMargin) - windowBottomMargin;
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

            expandedGameObjects.Clear();
            expandedComponents.Clear();
            expandedObjects.Clear();
            evaluatedProperties.Clear();
        }

        public void Refresh()
        {
            sceneRoots = GameObjectUtil.FindSceneRoots();
        }

        public void ExpandFromRefChain(ReferenceChain refChain)
        {
            if (refChain.Length == 0)
            {
                Log.Error("SceneExplorer: ExpandFromRefChain(): Invalid refChain, expected Length >= 0");
                return;
            }

            if (refChain.chainTypes[0] != ReferenceChain.ReferenceType.GameObject)
            {
                Log.Error(String.Format("SceneExplorer: ExpandFromRefChain(): invalid chain type for element [0] - expected {0}, got {1}",
                    ReferenceChain.ReferenceType.GameObject, refChain.chainTypes[0]));
                return;
            }

            sceneRoots.Clear();
            ClearExpanded();
            searchDisplayString = String.Format("Showing results for \"{0}\"", refChain.ToString());

            var rootGameObject = (GameObject) refChain.chainObjects[0];
            sceneRoots.Add(rootGameObject, true);

            var expandedRefChain = new ReferenceChain().Add(rootGameObject);
            expandedGameObjects.Add(expandedRefChain, true);

            for (int i = 1; i < refChain.Length; i++)
            {
                switch (refChain.chainTypes[i])
                {
                    case ReferenceChain.ReferenceType.GameObject:
                        var go = (GameObject) refChain.chainObjects[i];
                        expandedRefChain = expandedRefChain.Add(go);
                        expandedGameObjects.Add(expandedRefChain, true);
                        break;
                    case ReferenceChain.ReferenceType.Component:
                        var component = (Component) refChain.chainObjects[i];
                        expandedRefChain = expandedRefChain.Add(component);
                        expandedComponents.Add(expandedRefChain, true);
                        break;
                    case ReferenceChain.ReferenceType.Field:
                        var field = (FieldInfo) refChain.chainObjects[i];
                        expandedRefChain = expandedRefChain.Add(field);
                        expandedObjects.Add(expandedRefChain, true);
                        break;
                    case ReferenceChain.ReferenceType.Property:
                        var property = (PropertyInfo) refChain.chainObjects[i];
                        expandedRefChain = expandedRefChain.Add(property);
                        expandedObjects.Add(expandedRefChain, true);
                        break;
                    case ReferenceChain.ReferenceType.Method:
                        break;
                    case ReferenceChain.ReferenceType.EnumerableItem:
                        var index = (int) refChain.chainObjects[i];
                        selectedArrayStartIndices[expandedRefChain] = index;
                        selectedArrayEndIndices[expandedRefChain] = index;
                        expandedRefChain = expandedRefChain.Add(index);
                        expandedObjects.Add(expandedRefChain, true);
                        break;
                }
            }

            currentRefChain = refChain.Copy();
            currentRefChain.identOffset = -currentRefChain.Length;
        }

        private object EditorValueField(string hash, Type type, object value)
        {
            if (type == typeof(System.Single))
            {
                var f = (float)value;
                GUIControls.FloatField(hash, "", ref f, 0.0f, true, true);
                return f;
            }

            if (type == typeof(System.Double))
            {
                var f = (double)value;
                GUIControls.DoubleField(hash, "", ref f, 0.0f, true, true);
                return f;
            }

            if (type == typeof(System.Byte))
            {
                var f = (byte)value;
                GUIControls.ByteField(hash, "", ref f, 0.0f, true, true);
                return f;
            }

            if (type == typeof(System.Int32))
            {
                var f = (int)value;
                GUIControls.IntField(hash, "", ref f, 0.0f, true, true);
                return f;
            }
            
            if (type == typeof(System.UInt32))
            {
                var f = (uint)value;
                GUIControls.UIntField(hash, "", ref f, 0.0f, true, true);
                return f;
            }
            
            if (type == typeof(System.Int64))
            {
                var f = (Int64)value;
                GUIControls.Int64Field(hash, "", ref f, 0.0f, true, true);
                return f;
            }
            
            if (type == typeof(System.UInt64))
            {
                var f = (UInt64)value;
                GUIControls.UInt64Field(hash, "", ref f, 0.0f, true, true);
                return f;
            }
            
            if (type == typeof(System.Int16))
            {
                var f = (Int16)value;
                GUIControls.Int16Field(hash, "", ref f, 0.0f, true, true);
                return f;
            }
            
            if (type == typeof(System.UInt16))
            {
                var f = (UInt16)value;
                GUIControls.UInt16Field(hash, "", ref f, 0.0f, true, true);
                return f;
            }
            
            if (type == typeof(System.Boolean))
            {
                var f = (bool)value;
                GUIControls.BoolField("", ref f, 0.0f, true, true);
                return f;
            }
            
            if (type == typeof(System.String))
            {
                var f = (string)value;
                GUIControls.StringField(hash, "", ref f, 0.0f, true, true);
                return f;
            }
            
            if (type == typeof(System.Char))
            {
                var f = (char)value;
                GUIControls.CharField(hash, "", ref f, 0.0f, true, true);
                return f;
            }
            
            if (type == typeof(UnityEngine.Vector2))
            {
                var f = (Vector2)value;
                GUIControls.Vector2Field(hash, "", ref f, 0.0f, null, true, true);
                return f;
            }
            
            if (type == typeof(UnityEngine.Vector3))
            {
                var f = (Vector3)value;
                GUIControls.Vector3Field(hash, "", ref f, 0.0f, null, true, true);
                return f;
            }
            
            if (type == typeof(UnityEngine.Vector4))
            {
                var f = (Vector4)value;
                GUIControls.Vector4Field(hash, "", ref f, 0.0f, null, true, true);
                return f;
            }
            
            if (type == typeof(UnityEngine.Quaternion))
            {
                var f = (Quaternion)value;
                GUIControls.QuaternionField(hash, "", ref f, 0.0f, null, true, true);
                return f;
            }
            
            if (type == typeof(UnityEngine.Color))
            {
                var f = (Color)value;
                GUIControls.ColorField(hash, "", ref f, 0.0f, null, true, true);
                return f;
            }
            
            if (type == typeof(UnityEngine.Color32))
            {
                var f = (Color32)value;
                GUIControls.Color32Field(hash, "", ref f, 0.0f, null, true, true);
                return f;
            }

            return value;
        }

        private void OnSceneTreeReflectField(ReferenceChain refChain, System.Object obj, FieldInfo field)
        {
            AddDebugLine("OnSceneTreeReflectField(caller = {0}, obj = {1}, field = {2})",
                refChain, obj, field);

            var hash = refChain.GetHashCode().ToString();

            if (refChain.CheckDepth())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * refChain.Ident);
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
            GUILayout.Space(treeIdentSpacing * refChain.Ident);

            GUI.contentColor = Color.white;

            object value = null;

            try
            {
                value = field.GetValue(obj);
            }
            catch (Exception)
            {
            }

            if (value != null && TypeUtil.IsReflectableType(field.FieldType) && !IsEnumerable(obj))
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
            GUILayout.Label(" = ");

            if (value == null || !TypeUtil.IsBuiltInType(field.FieldType))
            {
                GUILayout.Label(value == null ? "null" : value.ToString());
            }
            else
            {
                var newValue = EditorValueField(hash, field.FieldType, value);
                if (newValue != value)
                {
                    field.SetValue(obj, newValue);
                }
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Watch"))
            {
                ModTools.Instance.watches.AddWatch(refChain, field, obj);
            }

            if (TypeUtil.IsTextureType(field.FieldType) && value != null)
            {
                if (GUILayout.Button("Preview"))
                {
                    ModTools.Instance.textureViewer.previewTexture = (Texture)value;
                    ModTools.Instance.textureViewer.caller = refChain;
                    ModTools.Instance.textureViewer.visible = true;
                }

                if (GUILayout.Button("Dump .png"))
                {
                    Util.DumpTextureToPNG((Texture)value);
                }
            }
            else if (TypeUtil.IsMeshType(field.FieldType) && value != null)
            {
                /*if (GUILayout.Button("Preview"))
                {
                    ModTools.Instance.meshViewer.previewMesh = (Mesh)value;
                    ModTools.Instance.meshViewer.caller = refChain;
                    ModTools.Instance.meshViewer.visible = true;
                }*/

                if (GUILayout.Button("Dump .obj"))
                {
                    var outPath = refChain.ToString() + ".obj";
                    outPath = outPath.Replace(' ', '_');
                    Util.DumpMeshOBJ(value as Mesh, outPath);
                }
            }

            GUILayout.EndHorizontal();
            if (value != null && TypeUtil.IsReflectableType(field.FieldType) && expandedObjects.ContainsKey(refChain))
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
                GUILayout.Space(treeIdentSpacing * refChain.Ident);
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
            GUILayout.Space(treeIdentSpacing * refChain.Ident);

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

                if (value != null && TypeUtil.IsReflectableType(property.PropertyType) && !IsEnumerable(obj))
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
            GUILayout.Label(" = ");

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

                if (value == null || !TypeUtil.IsBuiltInType(property.PropertyType))
                {
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
                else
                {
                    var newValue = EditorValueField(hash, property.PropertyType, value);
                    if (newValue != value)
                    {
                        property.SetValue(obj, newValue, null);
                    }
                }
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Watch"))
            {
                ModTools.Instance.watches.AddWatch(refChain, property, obj);
            }

            if (TypeUtil.IsTextureType(property.PropertyType) && value != null)
            {
                if (GUILayout.Button("Preview"))
                {
                    ModTools.Instance.textureViewer.previewTexture = (Texture)value;
                    ModTools.Instance.textureViewer.caller = refChain;
                    ModTools.Instance.textureViewer.visible = true;
                }

                if (GUILayout.Button("Dump .png"))
                {
                    Util.DumpTextureToPNG((Texture)value);
                }
            }
            else if (TypeUtil.IsMeshType(property.PropertyType) && value != null)
            {
               /* if (GUILayout.Button("Preview"))
                {
                    ModTools.Instance.meshViewer.previewMesh = (Mesh)value;
                    ModTools.Instance.meshViewer.caller = refChain;
                    ModTools.Instance.meshViewer.visible = true;
                }*/

                if (GUILayout.Button("Dump .obj"))
                {
                    var outPath = refChain.ToString() + ".obj";
                    outPath = outPath.Replace(' ', '_');
                    Util.DumpMeshOBJ(value as Mesh, outPath);
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
                GUILayout.Space(treeIdentSpacing * refChain.Ident);
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
            GUILayout.Space(treeIdentSpacing * refChain.Ident);

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
                GUILayout.Space(treeIdentSpacing * refChain.Ident);
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            GUIControls.Vector3Field(refChain.ToString(), name, ref vec, treeIdentSpacing * refChain.Ident, () =>
            {
                try
                {
                    ModTools.Instance.watches.AddWatch(refChain, typeof(T).GetProperty(name), obj);
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
                GUILayout.Space(treeIdentSpacing * refChain.Ident);
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
                GUILayout.Space(treeIdentSpacing * refChain.Ident);
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
                if (!material.HasProperty(prop))
                {
                    continue;
                }

                Texture value = material.GetTexture(prop);
                if (value == null)
                {
                    continue;
                }

                refChain = oldRefChain.Add(prop);

                var type = value.GetType();

                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * (refChain.Ident+1));

                GUI.contentColor = Color.white;

                if (TypeUtil.IsReflectableType(type) && !IsEnumerable(type))
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
                GUILayout.Label(value.ToString());

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Preview"))
                {
                    ModTools.Instance.textureViewer.previewTexture = (Texture)value;
                    ModTools.Instance.textureViewer.caller = refChain;
                    ModTools.Instance.textureViewer.visible = true;
                }

                if (GUILayout.Button("Dump .png"))
                {
                    Util.DumpTextureToPNG((Texture)value);
                }
                GUILayout.EndHorizontal();

                if (TypeUtil.IsReflectableType(type) && expandedObjects.ContainsKey(refChain))
                {
                    OnSceneTreeReflect(refChain, value);
                }

            }
            foreach (string prop in colorProps)
            {
                if (!material.HasProperty(prop))
                    continue;
                Color value = material.GetColor(prop);
                refChain = oldRefChain.Add(prop);

                var type = value.GetType();

                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * (refChain.Ident+1));

                GUI.contentColor = Color.white;

                if (TypeUtil.IsReflectableType(type) && !IsEnumerable(type))
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
                GUIControls.ColorField(refChain.ToString(), "", ref f, 0.0f, null, true, true);
                if (f != value)
                {
                    material.SetColor(prop, f);
                }

                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();

                if (TypeUtil.IsReflectableType(type) && expandedObjects.ContainsKey(refChain))
                {
                    OnSceneTreeReflect(refChain, value);
                }
            }

        }

        private bool IsEnumerable(object myProperty)
        {
            if (typeof (IEnumerable).IsAssignableFrom(myProperty.GetType())
                || typeof (IEnumerable<>).IsAssignableFrom(myProperty.GetType()))
                return true;

            return false;
        }

        private void OnSceneTreeReflectIEnumerable(ReferenceChain refChain, System.Object myProperty)
        {
            AddDebugLine("OnSceneTreeReflectIEnumerable(caller = {0}, obj = {1})", refChain, myProperty);

            if (refChain.CheckDepth())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * refChain.Ident);
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

            var collection = enumerable as ICollection;

            if (collection != null)
            {
                var collectionSize = collection.Count;

                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * refChain.Ident);
                GUILayout.Label("Collection size: " + collectionSize);

                if (!selectedArrayStartIndices.ContainsKey(refChain))
                {
                    selectedArrayStartIndices.Add(refChain, 0);
                }

                if (!selectedArrayEndIndices.ContainsKey(refChain))
                {
                    selectedArrayEndIndices.Add(refChain, 32);
                }

                var arrayStart = selectedArrayStartIndices[refChain];
                var arrayEnd = selectedArrayEndIndices[refChain];

                GUIControls.IntField(oldRefChain.ToString() + ".arrayStart", "Start index", ref arrayStart, 0.0f, true, true);
                GUIControls.IntField(oldRefChain.ToString() + ".arrayEnd", "End index", ref arrayEnd, 0.0f, true, true);
                GUILayout.Label("(32 items max)");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                arrayStart = Mathf.Clamp(arrayStart, 0, collectionSize - 1);
                arrayEnd = Mathf.Clamp(arrayEnd, 0, collectionSize - 1);
                if (arrayStart > arrayEnd)
                {
                    arrayEnd = arrayStart;
                }

                if (arrayEnd - arrayStart > 32)
                {
                    arrayEnd = arrayStart + 32;
                    arrayEnd = Mathf.Clamp(arrayEnd, 0, collectionSize - 1);
                }

                selectedArrayStartIndices[refChain] = arrayStart;
                selectedArrayEndIndices[refChain] = arrayEnd;

                foreach (var value in enumerable)
                {
                    if (count < arrayStart)
                    {
                        count++;
                        continue;
                    }
                    
                    refChain = oldRefChain.Add(count);

                    var type = value.GetType();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(treeIdentSpacing * refChain.Ident);

                    GUI.contentColor = Color.white;

                    if (TypeUtil.IsReflectableType(type) && !IsEnumerable(type))
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

                    if (TypeUtil.IsReflectableType(type) && expandedObjects.ContainsKey(refChain))
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
                    if (count > arrayEnd)
                    {
                        break;
                    }
                }
            }
            else
            {
                foreach (var value in enumerable)
                {
                    refChain = oldRefChain.Add(count);

                    var type = value.GetType();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(treeIdentSpacing * refChain.Ident);

                    GUI.contentColor = Color.white;

                    if (TypeUtil.IsReflectableType(type) && !IsEnumerable(type))
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

                    if (TypeUtil.IsReflectableType(type) && expandedObjects.ContainsKey(refChain))
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
                    if (count >= 1024)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(treeIdentSpacing * refChain.Ident);
                        GUILayout.Label("Array too large to display");
                        GUILayout.EndHorizontal();
                        break;
                    }
                }
            }
        }

        private void OnSceneTreeReflect(ReferenceChain refChain, System.Object obj)
        {
            if (refChain.CheckDepth())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * refChain.Ident);
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
                GUILayout.Space(treeIdentSpacing * refChain.Ident);
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

            MemberInfo[] members = type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            if (sortAlphabetically)
            {
                Array.Sort(members, (info, info1) => info.Name.CompareTo(info1.Name));
            }

            foreach (MemberInfo member in members)
            {
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
                        GUILayout.Space(treeIdentSpacing * refChain.Ident);
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
                        GUILayout.Space(treeIdentSpacing * refChain.Ident);
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
                        GUILayout.Space(treeIdentSpacing * refChain.Ident);
                        GUILayout.Label(String.Format("Exception when fetching method \"{0}\" - {1}", method.Name, ex.Message));
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }

        private void OnSceneTreeComponent(ReferenceChain refChain, Component component)
        {
            if (refChain.CheckDepth())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * refChain.Ident);
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(treeIdentSpacing * refChain.Ident);

            if (currentRefChain == null || !currentRefChain.Equals(refChain.Add(component)))
            {
                if (GUILayout.Button(">", GUILayout.Width(16)))
                {
                    currentRefChain = refChain.Add(component);
                    currentRefChain.identOffset = -(refChain.Length + 1);
                }
            }
            else
            {
                if (GUILayout.Button("<", GUILayout.Width(16)))
                {
                    currentRefChain = null;
                }

                GUI.contentColor = Color.green;
            }
            
       
            GUILayout.Label(component.GetType().ToString());

            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();
        }

        private void OnSceneTreeRecursive(ReferenceChain refChain, GameObject obj)
        {
            AddDebugLine("OnSceneTreeRecursive(caller = {0}, obj = {1})", refChain, obj);

            if (refChain.CheckDepth())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * refChain.Ident);
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            if (obj == null)
            {
                GUILayout.Label("null");
                return;
            }

            if (expandedGameObjects.ContainsKey(refChain))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * refChain.Ident);

                if (GUILayout.Button("-", GUILayout.Width(16)))
                {
                    expandedGameObjects.Remove(refChain);
                }

                GUILayout.Label(obj.name);
                GUILayout.EndHorizontal();

                var components = obj.GetComponents(typeof(Component));

                if (sortAlphabetically)
                {
                    Array.Sort(components, (component, component1) => component.GetType().ToString().CompareTo(component1.GetType().ToString()));
                }

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
                GUILayout.Space(treeIdentSpacing * refChain.Ident);

                if (GUILayout.Button("+", GUILayout.Width(16)))
                {
                    expandedGameObjects.Add(refChain, true);
                }

                GUILayout.Label(obj.name);
                GUILayout.EndHorizontal();
            }
        }

        public void DrawHeader()
        {
            headerArea.Begin();

            GUILayout.BeginHorizontal();

            GUI.contentColor = Color.green;
            GUILayout.Label("Show: ");
            GUI.contentColor = Color.white;

            GUILayout.Label("Fields");
            showFields = GUILayout.Toggle(showFields, "");

            GUILayout.Label("Properties");
            showProperties = GUILayout.Toggle(showProperties, "");

            GUILayout.Label("Methods");
            showMethods = GUILayout.Toggle(showMethods, "");
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.green;
            GUILayout.Label("Show search options:");
            GUI.contentColor = Color.white;
            var showSearchNew = GUILayout.Toggle(showSearchBar, "");
            if (showSearchNew != showSearchBar)
            {
                if (showSearchNew)
                {
                    headerHeight = 200.0f;
                }
                else
                {
                    headerHeight = 140.0f;
                }

                RecalculateAreas();
                showSearchBar = showSearchNew;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (showSearchBar)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("GameObject.Find");
                findGameObjectFilter = GUILayout.TextField(findGameObjectFilter, GUILayout.Width(256));

                if (findGameObjectFilter.Trim().Length == 0)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button("Find"))
                {
                    ClearExpanded();
                    var go = GameObject.Find(findGameObjectFilter.Trim());
                    if (go != null)
                    {
                        sceneRoots.Clear();
                        expandedGameObjects.Add(new ReferenceChain().Add(go), true);
                        sceneRoots.Add(go, true);
                        sceneTreeScrollPosition = Vector2.zero;
                        searchDisplayString = String.Format("Showing results for GameObject.Find(\"{0}\")", findGameObjectFilter);
                    }
                }

                if (GUILayout.Button("Reset"))
                {
                    ClearExpanded();
                    sceneRoots = GameObjectUtil.FindSceneRoots();
                    sceneTreeScrollPosition = Vector2.zero;
                    searchDisplayString = "";
                }

                GUI.enabled = true;

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("GameObject.FindObjectsOfType");
                findObjectTypeFilter = GUILayout.TextField(findObjectTypeFilter, GUILayout.Width(256));

                if (findObjectTypeFilter.Trim().Length == 0)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button("Find"))
                {
                    var gameObjects = GameObjectUtil.FindComponentsOfType(findObjectTypeFilter.Trim());

                    sceneRoots.Clear();
                    foreach (var item in gameObjects)
                    {
                        ClearExpanded();
                        expandedGameObjects.Add(new ReferenceChain().Add(item.Key), true);
                        if (gameObjects.Count == 1)
                        {
                            expandedComponents.Add(new ReferenceChain().Add(item.Key).Add(item.Value), true);
                        }
                        sceneRoots.Add(item.Key, true);
                        sceneTreeScrollPosition = Vector2.zero;
                        searchDisplayString = String.Format("Showing results for GameObject.FindObjectsOfType({0})", findObjectTypeFilter);
                    }
                }

                if (GUILayout.Button("Reset"))
                {
                    ClearExpanded();
                    sceneRoots = GameObjectUtil.FindSceneRoots();
                    sceneTreeScrollPosition = Vector2.zero;
                    searchDisplayString = "";
                }

                GUI.enabled = true;

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.green;
            GUILayout.Label("Sort alphabetically:");
            GUI.contentColor = Color.white;
            sortAlphabetically = GUILayout.Toggle(sortAlphabetically, "");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Refresh/ Clear", GUILayout.Width(200)))
            {
                sceneRoots = GameObjectUtil.FindSceneRoots();
            }

            if (GUILayout.Button("Fold all", GUILayout.Width(200)))
            {
                ClearExpanded();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            headerArea.End();
        }

        public void DrawSceneTree()
        {
            sceneTreeArea.Begin();

            if (searchDisplayString != "")
            {
                GUI.contentColor = Color.green;
                GUILayout.Label(searchDisplayString);
                GUI.contentColor = Color.white;
            }

            sceneTreeScrollPosition = GUILayout.BeginScrollView(sceneTreeScrollPosition);

            var gameObjects = sceneRoots.Keys.ToArray();

            if (sortAlphabetically)
            {
                Array.Sort(gameObjects, (o, o1) => o.name.CompareTo(o1.name));
            }

            foreach (var obj in gameObjects)
            {
                OnSceneTreeRecursive(new ReferenceChain().Add(obj), obj);
            }

            GUILayout.EndScrollView();

            sceneTreeArea.End();
        }

        public void DrawComponent()
        {
            componentArea.Begin();

            componentScrollPosition = GUILayout.BeginScrollView(componentScrollPosition);

            if (currentRefChain != null)
            {
                OnSceneTreeReflect(currentRefChain, currentRefChain.Evaluate());
            }

            GUILayout.EndScrollView();

            componentArea.End();
        }

        public void DrawWindow()
        {
            bool enterPressed = Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);

            if (enterPressed)
            {
                GUI.FocusControl(null);
            }

            if (debugMode)
            {
                lastCrashReport = debugOutput;
                debugOutput = "";
            }

            preventCircularReferences.Clear();

            DrawHeader();
            DrawSceneTree();
            DrawComponent();
        }

        private void ClearExpanded()
        {
            expandedGameObjects.Clear();
            expandedComponents.Clear();
            expandedObjects.Clear();
            evaluatedProperties.Clear();
            selectedArrayStartIndices.Clear();
            selectedArrayEndIndices.Clear();
            searchDisplayString = "";
            sceneTreeScrollPosition = Vector2.zero;
        }

    }

}