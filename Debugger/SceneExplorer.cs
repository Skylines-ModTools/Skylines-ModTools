using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace ModTools
{

    public class SceneExplorer
    {

        public enum FilterType
        {
            FieldsAndProps,
            GameObjects,
            Components
        }

        private static float treeIdentSpacing = 16.0f;

        private static Dictionary<int, bool> expanded = new Dictionary<int, bool>();
        private static Dictionary<int, bool> expandedComponents = new Dictionary<int, bool>();

        private static Dictionary<GameObject, bool> sceneRoots = new Dictionary<GameObject, bool>();

        private static Vector2 scrollPosition = Vector2.zero;

        private static bool showFields = true;
        private static bool showProperties = true;
        private static bool showMethods = false;

        private static string nameFilter = "";
        private static FilterType filterType = FilterType.GameObjects;

        private static Dictionary<GameObject, bool> FindSceneRoots()
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

        private static void OnSceneTreeReflectField(string caller, System.Object obj, FieldInfo field, int ident)
        {
            if (obj == null || field == null)
            {
                GUILayout.Label("null");
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(treeIdentSpacing * ident);

            GUI.contentColor = Color.white;

            if (field.IsPublic)
            {
                GUILayout.Label("public ");
            }
            else if (field.IsPrivate)
            {
                GUILayout.Label("private ");
            }

            GUI.contentColor = Color.green;

            GUILayout.Label("field ");
            GUILayout.Label(field.FieldType.ToString() + " ");

            GUI.contentColor = Color.red;

            GUILayout.Label(field.Name);

            GUI.contentColor = Color.white;

            var value = field.GetValue(obj);

            if (field.FieldType.ToString() == "System.Single")
            {
                var f = (float) value;
                GUIControls.FloatField("", ref f);
                if(f != (float)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.Int32")
            {
                var f = (int)value;
                GUIControls.IntField("", ref f);
                if (f != (int)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.Boolean")
            {
                var f = (bool)value;
                GUIControls.BoolField("", ref f);
                if (f != (bool)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.String")
            {
                var f = (string)value;
                GUIControls.StringField("", ref f);
                if (f != (string)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else
            {
                GUI.contentColor = Color.white;
                GUILayout.Label(" = ");
                GUI.contentColor = Color.white;
                GUILayout.Label(value == null ? "null" : value.ToString());
                GUI.contentColor = Color.white;
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Watch"))
            {
                Watches.AddWatch(caller + "." + field.Name, field, obj);
            }

            GUILayout.EndHorizontal();
            
        }

        private static void OnSceneTreeReflectProperty(string caller, System.Object obj, PropertyInfo property, int ident)
        {
            if (obj == null || property == null)
            {
                GUILayout.Label("null");
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(treeIdentSpacing * ident);

            GUI.contentColor = Color.green;

            GUILayout.Label("property ");
            GUILayout.Label(property.PropertyType.ToString() + " ");

            GUI.contentColor = Color.red;

            GUILayout.Label(property.Name);

            GUI.contentColor = Color.white;

            var value = property.GetValue(obj, null);

            if (property.PropertyType.ToString() == "System.Single")
            {
                var f = (float)value;
                GUIControls.FloatField("", ref f);
                if (f != (float)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "System.Int32")
            {
                var f = (int)value;
                GUIControls.IntField("", ref f);
                if (f != (int)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "System.Boolean")
            {
                var f = (bool)value;
                GUIControls.BoolField("", ref f);
                if (f != (bool)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "System.String")
            {
                var f = (string)value;
                GUIControls.StringField("", ref f);
                if (f != (string)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else
            {
                GUILayout.Label(" = ");

                GUI.contentColor = Color.white;

                GUILayout.Label(value == null ? "null" : value.ToString());

                GUI.contentColor = Color.white;
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Watch"))
            {
                Watches.AddWatch(caller + "." + property.Name, property, obj);
            }

            GUILayout.EndHorizontal();
        }

        private static void OnSceneTreeReflectMethod(System.Object obj, MethodInfo method, int ident)
        {
            if (obj == null || method == null)
            {
                GUILayout.Label("null");
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(treeIdentSpacing * ident);

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

        private static void OnSceneTreeReflectUnityEngineVector2(string name, ref UnityEngine.Vector2 vec, int ident)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Space(treeIdentSpacing * ident);

            GUILayout.Label("Vector2");
            GUILayout.Label(name);

            GUILayout.BeginVertical();
            GUIControls.FloatField("x", ref vec.x);
            GUIControls.FloatField("y", ref vec.y);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static void OnSceneTreeReflectUnityEngineVector3(string name, ref UnityEngine.Vector3 vec, int ident)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(treeIdentSpacing * ident);
            GUIControls.Vector3Field(name, ref vec);
            GUILayout.EndHorizontal();
        }

        private static void OnSceneTreeReflectUnityEngineVector4(string name, ref UnityEngine.Vector4 vec, int ident)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(treeIdentSpacing * ident);
            GUIControls.Vector4Field(name, ref vec);
            GUILayout.EndHorizontal();
        }

        private static void OnSceneTreeReflectFloat(string name, ref float value, int ident)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(treeIdentSpacing * ident);
            GUILayout.Label("float");
            GUIControls.FloatField(name, ref value);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static void OnSceneTreeReflectInt(string name, ref int value, int ident)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(treeIdentSpacing * ident);
            GUILayout.Label("int");
            GUIControls.IntField(name, ref value);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static void OnSceneTreeReflectBool(string name, ref bool value, int ident)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(treeIdentSpacing * ident);
            GUILayout.Label("bool");
            GUILayout.FlexibleSpace();

            GUIControls.BoolField(name, ref value);
            GUILayout.EndHorizontal();
        }

        private static void OnSceneTreeReflectUnityEngineTransform(UnityEngine.Transform transform, int ident)
        {
            if (transform == null)
            {
                GUILayout.Label("null");
                return;
            }

            var childCount = transform.childCount;
            OnSceneTreeReflectInt("childCount", ref childCount, ident + 1);

            var eulerAngles = transform.eulerAngles;
            OnSceneTreeReflectUnityEngineVector3("eulerAngles", ref eulerAngles, ident + 1);
            transform.eulerAngles = eulerAngles;

            var forward = transform.forward;
            OnSceneTreeReflectUnityEngineVector3("forward", ref forward, ident + 1);
            transform.forward = forward;

            var hasChanged = transform.hasChanged;
            OnSceneTreeReflectBool("hasChanged", ref hasChanged, ident + 1);
            transform.hasChanged = hasChanged;

            var localEulerAngles = transform.localEulerAngles;
            OnSceneTreeReflectUnityEngineVector3("localEulerAngles", ref eulerAngles, ident + 1);
            transform.localEulerAngles = localEulerAngles;

            var localPosition = transform.localPosition;
            OnSceneTreeReflectUnityEngineVector3("localPosition", ref localPosition, ident + 1);
            transform.localPosition = localPosition;

            var localRotation = transform.localRotation;

            var localScale = transform.localScale;
            OnSceneTreeReflectUnityEngineVector3("localScale", ref localScale, ident + 1);
            transform.localScale = localScale;

            var localToWorldMatrix = transform.localToWorldMatrix;

            var lossyScale = transform.lossyScale;
            OnSceneTreeReflectUnityEngineVector3("lossyScale", ref lossyScale, ident + 1);

            var right = transform.right;
            OnSceneTreeReflectUnityEngineVector3("right", ref lossyScale, ident + 1);

            var rotation = transform.rotation;

            var up = transform.up;
            OnSceneTreeReflectUnityEngineVector3("up", ref up, ident + 1);

            var worldToLocalMatrix = transform.worldToLocalMatrix;
        }

        private static bool IsEnumerable(object myProperty)
        {
            if (typeof(IEnumerable).IsAssignableFrom(myProperty.GetType())
                || typeof(IEnumerable<>).IsAssignableFrom(myProperty.GetType()))
                return true;

            return false;
        }

        private static void OnSceneTreeReflectIEnumerable(string caller, System.Object myProperty, int ident)
        {
            var ie = myProperty as IEnumerable;
            string s = string.Empty;
            if (null != ie)
            {
                bool first = true;
                foreach (var p in ie)
                {
                    if (!first)
                        s += ", ";

                    OnSceneTreeReflect(caller, myProperty, ident + 1);
                    first = false;
                }
            }
        }

        private static void OnSceneTreeReflect(string caller, System.Object obj, int ident)
        {
            if (obj == null)
            {
                GUILayout.Label("null");
                return;
            }

            Type type = obj.GetType();

            if (type == typeof(UnityEngine.Transform))
            {
                OnSceneTreeReflectUnityEngineTransform((UnityEngine.Transform)obj, ident);
                return;
            }
            else if (IsEnumerable(obj))
            {
                OnSceneTreeReflectIEnumerable(caller, obj, ident);
                return;
            }

            MemberInfo[] fields = type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            foreach (MemberInfo member in fields)
            {
                if (filterType == FilterType.FieldsAndProps && !member.Name.Contains(nameFilter))
                {
                    continue;
                }

                FieldInfo field = null;
                PropertyInfo property = null;
                MethodInfo method = null;

                if (member.MemberType == MemberTypes.Field && showFields)
                {
                    field = (FieldInfo)member;
                    OnSceneTreeReflectField(caller, obj, field, ident);
                }
                else if (member.MemberType == MemberTypes.Property && showProperties)
                {
                    property = (PropertyInfo)member;
                    OnSceneTreeReflectProperty(caller, obj, property, ident);
                }
                else if (member.MemberType == MemberTypes.Method && showMethods)
                {
                    method = (MethodInfo)member;
                    OnSceneTreeReflectMethod(obj, method, ident);
                }

                if (field == null)
                {
                    continue;
                }
            }
        }

        private static void OnSceneTreeComponents(string caller, GameObject obj, int ident)
        {
            var components = obj.GetComponents(typeof(Component));
            foreach (var component in components)
            {
                if (filterType == FilterType.Components && !component.name.Contains(nameFilter))
                {
                    continue;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);

                if (expandedComponents.ContainsKey(component.GetHashCode()))
                {
                    if (GUILayout.Button("-", GUILayout.Width(16)))
                    {
                        expandedComponents.Remove(component.GetHashCode());
                    }

                    GUILayout.Label(component.name + " (" + component.GetType().ToString() + ")");

                    GUILayout.EndHorizontal();

                    OnSceneTreeReflect(caller + "." + component.name, component, ident + 1);
                }
                else
                {
                    if (GUILayout.Button("+", GUILayout.Width(16)))
                    {
                        expandedComponents.Add(component.GetHashCode(), true);
                    }

                    GUILayout.Label(component.name + " (" + component.GetType().ToString() + ")");
                    GUILayout.EndHorizontal();
                }
            }
        }

        private static void OnSceneTreeRecursive(string caller, GameObject obj, int ident = 0)
        {
            if (filterType == FilterType.GameObjects && !obj.name.Contains(nameFilter))
            {
                return;
            }

            if (obj == null)
            {
                GUILayout.Label("null");
                return;
            }

            if (expanded.ContainsKey(obj.GetHashCode()))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);

                if (GUILayout.Button("-", GUILayout.Width(16)))
                {
                    expanded.Remove(obj.GetHashCode());
                }

                GUILayout.Label(obj.name);
                GUILayout.EndHorizontal();

                OnSceneTreeComponents(caller + "." + obj.name, obj, ident + 1);

                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    OnSceneTreeRecursive(caller + "." + obj.name, obj.transform.GetChild(i).gameObject, ident + 1);
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);

                if (GUILayout.Button("+", GUILayout.Width(16)))
                {
                    expanded.Add(obj.GetHashCode(), true);
                }

                GUILayout.Label(obj.name);
                GUILayout.EndHorizontal();
            }
        }

        public static void DrawWindow()
        {
            if (GUILayout.Button("Refresh"))
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

            GUIControls.StringField("Filter", ref nameFilter);

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
                filterGameObject = false;
                filterFields = false;
            }

            GUILayout.FlexibleSpace();

            GUILayout.Label("Fields and properties");
            filterFields = GUILayout.Toggle(filterFields, "");
            if (filterFields)
            {
                filterGameObject = false;
                filterComponent = false;
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

            try
            {
                foreach (var obj in sceneRoots)
                {
                    OnSceneTreeRecursive("", obj.Key);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Debugger: Exception - " + ex.Message);
                expanded.Clear();
                expandedComponents.Clear();
            }

            GUI.DragWindow();

            GUILayout.EndScrollView();
        }

    }

}