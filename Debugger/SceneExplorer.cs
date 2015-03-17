using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace ModTools
{

    public class SceneExplorer : GUIWindow
    {

        public enum FilterType
        {
            FieldsAndProps,
            GameObjects,
            Components
        }

        private float treeIdentSpacing = 16.0f;

        private Dictionary<int, bool> expanded = new Dictionary<int, bool>();
        private Dictionary<int, bool> expandedComponents = new Dictionary<int, bool>();
        private Dictionary<int, bool> expandedObjects = new Dictionary<int, bool>();

        private Dictionary<GameObject, bool> sceneRoots = new Dictionary<GameObject, bool>();

        private Vector2 scrollPosition = Vector2.zero;

        private bool showFields = true;
        private bool showProperties = true;
        private bool showMethods = false;

        private string nameFilter = "";
        private FilterType filterType = FilterType.GameObjects;

        private Watches watches;

        public SceneExplorer()
            : base("Scene Explorer", new Rect(128, 440, 800, 500), ModTools.skin)
        {
            onDraw = DrawWindow;
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

        private void OnSceneTreeReflectField(string caller, System.Object obj, FieldInfo field, int ident)
        {
            if (ident >= 12)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);
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
            GUILayout.Space(treeIdentSpacing * ident);

            GUI.contentColor = Color.white;

            var value = field.GetValue(obj);

            if (value != null && IsReflectableType(field.FieldType) && !IsEnumerable(obj))
            {
                if (expandedObjects.ContainsKey(value.GetHashCode()))
                {
                    if (GUILayout.Button("-", GUILayout.Width(16)))
                    {
                        expandedObjects.Remove(value.GetHashCode());
                    }
                }
                else
                {
                    if (GUILayout.Button("+", GUILayout.Width(16)))
                    {
                        expandedObjects.Add(value.GetHashCode(), true);
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

            GUI.contentColor = Color.green;

            GUILayout.Label("field ");
            GUILayout.Label(field.FieldType.ToString() + " ");

            if (field.IsInitOnly)
            {
                GUI.contentColor = Color.blue;
                GUI.enabled = false;
                GUILayout.Label("const ");
            }

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
                var f = (float) value;
                GUIControls.FloatField("", ref f, 0.0f, true, true);
                if(f != (float)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.Double")
            {
                var f = (double)value;
                GUIControls.DoubleField("", ref f, 0.0f, true, true);
                if (f != (double)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.Byte")
            {
                var f = (byte)value;
                GUIControls.ByteField("", ref f, 0.0f, true, true);
                if (f != (byte)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.Int32")
            {
                var f = (int)value;
                GUIControls.IntField("", ref f, 0.0f, true, true);
                if (f != (int)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.UInt32")
            {
                var f = (uint)value;
                GUIControls.UIntField("", ref f, 0.0f, true, true);
                if (f != (uint)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.Int64")
            {
                var f = (Int64)value;
                GUIControls.Int64Field("", ref f, 0.0f, true, true);
                if (f != (Int64)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.UInt64")
            {
                var f = (UInt64)value;
                GUIControls.UInt64Field("", ref f, 0.0f, true, true);
                if (f != (UInt64)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.Int16")
            {
                var f = (Int16)value;
                GUIControls.Int16Field("", ref f, 0.0f, true, true);
                if (f != (Int16)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.UInt16")
            {
                var f = (UInt16)value;
                GUIControls.UInt16Field("", ref f, 0.0f, true, true);
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
                GUIControls.StringField("", ref f, 0.0f, true, true);
                if (f != (string)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "System.Char")
            {
                var f = (char)value;
                GUIControls.CharField("", ref f, 0.0f, true, true);
                if (f != (char)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "UnityEngine.Vector2")
            {
                var f = (Vector2)value;
                GUIControls.Vector2Field("", ref f, 0.0f, null, true, true);
                if (f != (Vector2)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "UnityEngine.Vector3")
            {
                var f = (Vector3)value;
                GUIControls.Vector3Field("", ref f, 0.0f, null, true, true);
                if (f != (Vector3)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "UnityEngine.Vector4")
            {
                var f = (Vector4)value;
                GUIControls.Vector4Field("", ref f, 0.0f, null, true, true);
                if (f != (Vector4)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "UnityEngine.Quaternion")
            {
                var f = (Quaternion)value;
                GUIControls.QuaternionField("", ref f, 0.0f, null, true, true);
                if (f != (Quaternion)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "UnityEngine.Color")
            {
                var f = (Color)value;
                GUIControls.ColorField("", ref f, 0.0f, null, true, true);
                if (f != (Color)value)
                {
                    field.SetValue(obj, f);
                }
            }
            else if (field.FieldType.ToString() == "UnityEngine.Color32")
            {
                var f = (Color32)value;
                GUIControls.Color32Field("", ref f, 0.0f, null, true, true);
                var v = (Color32) value;
                if (f.r != v.r || f.g != v.g || f.b != v.b || f.a != v.a)
                {
                    field.SetValue(obj, f);
                }
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Watch"))
            {
                watches.AddWatch(caller + "." + field.Name, field, obj);
            }

            GUILayout.EndHorizontal();

            if (value != null && IsReflectableType(field.FieldType) && expandedObjects.ContainsKey(value.GetHashCode()))
            {
                if (value is GameObject)
                {
                    OnSceneTreeComponents(caller + "." + field.Name, (GameObject)value, ident + 1);   
                }
                else if (value is Transform)
                {
                    OnSceneTreeReflectUnityEngineTransform(caller + "." + field.Name, (Transform)value, ident + 1);
                }
                else
                {
                    OnSceneTreeReflect(caller + "." + field.Name, value, ident + 1);
                }
            }
        }

        private void OnSceneTreeReflectProperty(string caller, System.Object obj, PropertyInfo property, int ident)
        {
            if (ident >= 12)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);
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
            GUILayout.Space(treeIdentSpacing * ident);

            var value = property.GetValue(obj, null);

            if (value != null && IsReflectableType(property.PropertyType) && !IsEnumerable(obj))
            {
                if (expandedObjects.ContainsKey(value.GetHashCode()))
                {
                    if (GUILayout.Button("-", GUILayout.Width(16)))
                    {
                        expandedObjects.Remove(value.GetHashCode());
                    }
                }
                else
                {
                    if (GUILayout.Button("+", GUILayout.Width(16)))
                    {
                        expandedObjects.Add(value.GetHashCode(), true);
                    }
                }
            }

            GUI.contentColor = Color.green;

            GUILayout.Label("property ");
            GUILayout.Label(property.PropertyType.ToString() + " ");

            GUI.contentColor = Color.red;

            GUILayout.Label(property.Name);

            GUI.contentColor = Color.white;

            if (value == null || !IsBuiltInType(property.PropertyType))
            {
                GUI.contentColor = Color.white;
                GUILayout.Label(" = ");
                GUI.contentColor = Color.white;
                GUILayout.Label(value == null ? "null" : value.ToString());
                GUI.contentColor = Color.white;
            }
            else if (property.PropertyType.ToString() == "System.Single")
            {
                var f = (float)value;
                GUIControls.FloatField("", ref f, 0.0f, true, true);
                if (f != (float)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "System.Double")
            {
                var f = (double)value;
                GUIControls.DoubleField("", ref f, 0.0f, true, true);
                if (f != (double)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "System.Byte")
            {
                var f = (byte)value;
                GUIControls.ByteField("", ref f, 0.0f, true, true);
                if (f != (byte)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "System.Int32")
            {
                var f = (int)value;
                GUIControls.IntField("", ref f, 0.0f, true, true);
                if (f != (int)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "System.UInt32")
            {
                var f = (uint)value;
                GUIControls.UIntField("", ref f, 0.0f, true, true);
                if (f != (uint)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "System.Int64")
            {
                var f = (Int64)value;
                GUIControls.Int64Field("", ref f, 0.0f, true, true);
                if (f != (Int64)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "System.UInt64")
            {
                var f = (UInt64)value;
                GUIControls.UInt64Field("", ref f, 0.0f, true, true);
                if (f != (UInt64)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "System.Int16")
            {
                var f = (Int16)value;
                GUIControls.Int16Field("", ref f, 0.0f, true, true);
                if (f != (Int16)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "System.UInt16")
            {
                var f = (UInt16)value;
                GUIControls.UInt16Field("", ref f, 0.0f, true, true);
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
                GUIControls.StringField("", ref f, 0.0f, true, true);
                if (f != (string)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "System.Char")
            {
                var f = (char)value;
                GUIControls.CharField("", ref f, 0.0f, true, true);
                if (f != (char)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "UnityEngine.Vector2")
            {
                var f = (Vector2)value;
                GUIControls.Vector2Field("", ref f, 0.0f, null, true, true);
                if (f != (Vector2)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "UnityEngine.Vector3")
            {
                var f = (Vector3)value;
                GUIControls.Vector3Field("", ref f, 0.0f, null, true, true);
                if (f != (Vector3)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "UnityEngine.Vector4")
            {
                var f = (Vector4)value;
                GUIControls.Vector4Field("", ref f, 0.0f, null, true, true);
                if (f != (Vector4)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "UnityEngine.Quaternion")
            {
                var f = (Quaternion)value;
                GUIControls.QuaternionField("", ref f, 0.0f, null, true, true);
                if (f != (Quaternion)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "UnityEngine.Color")
            {
                var f = (Color)value;
                GUIControls.ColorField("", ref f, 0.0f, null, true, true);
                if (f != (Color)value)
                {
                    property.SetValue(obj, f, null);
                }
            }
            else if (property.PropertyType.ToString() == "UnityEngine.Color32")
            {
                var f = (Color32)value;
                GUIControls.Color32Field("", ref f, 0.0f, null, true, true);
                var v = (Color32)value;
                if (f.r != v.r || f.g != v.g || f.b != v.b || f.a != v.a)
                {
                    property.SetValue(obj, f, null);
                }
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Watch"))
            {
                watches.AddWatch(caller + "." + property.Name, property, obj);
            }

            GUILayout.EndHorizontal();

            if (value != null && expandedObjects.ContainsKey(value.GetHashCode()))
            {
                if (value is GameObject)
                {
                    OnSceneTreeComponents(caller + "." + property.Name, (GameObject)value, ident + 1);
                }
                else if (value is Transform)
                {
                    OnSceneTreeReflectUnityEngineTransform(caller + "." + property.Name, (Transform)value, ident + 1);
                }
                else
                {
                    OnSceneTreeReflect(caller + "." + property.Name, value, ident + 1);
                }
            }
        }

        private void OnSceneTreeReflectMethod(System.Object obj, MethodInfo method, int ident)
        {
            if (ident >= 12)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);
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

        private void OnSceneTreeReflectUnityEngineVector2(string name, ref UnityEngine.Vector2 vec, int ident)
        {
            if (ident >= 12)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

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

        private void OnSceneTreeReflectUnityEngineVector3<T>(string caller, T obj, string name, ref UnityEngine.Vector3 vec, int ident)
        {
            if (ident >= 12)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            GUIControls.Vector3Field(name, ref vec, treeIdentSpacing * ident, () =>
            {
                try
                {
                    watches.AddWatch(caller + "." + name, typeof(T).GetProperty(name), obj);
                }
                catch (Exception ex)
                {
                    Log.Error("Exception in ModTools:OnSceneTreeReflectUnityEngineVector3 - " + ex.Message);
                }
            });
        }

        private void OnSceneTreeReflectUnityEngineVector4(string name, ref UnityEngine.Vector4 vec, int ident)
        {
            if (ident >= 12)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            GUIControls.Vector4Field(name, ref vec, treeIdentSpacing * ident);
        }

        private void OnSceneTreeReflectFloat(string name, ref float value, int ident)
        {
            if (ident >= 12)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            GUIControls.FloatField(name, ref value, treeIdentSpacing * ident);
        }

        private void OnSceneTreeReflectInt(string name, ref int value, int ident)
        {
            if (ident >= 12)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            GUIControls.IntField(name, ref value, treeIdentSpacing * ident);
        }

        private void OnSceneTreeReflectBool(string name, ref bool value, int ident)
        {
            GUIControls.BoolField(name, ref value, treeIdentSpacing * ident);
        }

        private void OnSceneTreeReflectUnityEngineTransform(string caller, UnityEngine.Transform transform, int ident)
        {
            if (ident >= 12)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            if (transform == null)
            {
                GUILayout.Label("null");
                return;
            }

            GUI.enabled = false;
            var childCount = transform.childCount;
            OnSceneTreeReflectInt("childCount", ref childCount, ident + 1);

            var eulerAngles = transform.eulerAngles;
            OnSceneTreeReflectUnityEngineVector3(caller, transform, "eulerAngles", ref eulerAngles, ident + 1);
            GUI.enabled = true;

            var localPosition = transform.localPosition;
            OnSceneTreeReflectUnityEngineVector3(caller, transform, "localPosition", ref localPosition, ident + 1);
            transform.localPosition = localPosition;

            var localEulerAngles = transform.localEulerAngles;
            OnSceneTreeReflectUnityEngineVector3(caller, transform, "localEulerAngles", ref eulerAngles, ident + 1);
            transform.localEulerAngles = localEulerAngles;

            var localScale = transform.localScale;
            OnSceneTreeReflectUnityEngineVector3(caller, transform, "localScale", ref localScale, ident + 1);
            transform.localScale = localScale;

            var lossyScale = transform.lossyScale;
            OnSceneTreeReflectUnityEngineVector3(caller, transform, "lossyScale", ref lossyScale, ident + 1);

            var forward = transform.forward;
            OnSceneTreeReflectUnityEngineVector3(caller, transform, "forward", ref forward, ident + 1);
            transform.forward = forward;

            var right = transform.right;
            OnSceneTreeReflectUnityEngineVector3(caller, transform, "right", ref right, ident + 1);

            var up = transform.up;
            OnSceneTreeReflectUnityEngineVector3(caller, transform, "up", ref up, ident + 1);
        }

        private bool IsEnumerable(object myProperty)
        {
            if (typeof(IEnumerable).IsAssignableFrom(myProperty.GetType())
                || typeof(IEnumerable<>).IsAssignableFrom(myProperty.GetType()))
                return true;

            return false;
        }

        private void OnSceneTreeReflectIEnumerable(string caller, System.Object myProperty, int ident)
        {
            if (ident >= 12)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);
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
            foreach (var value in enumerable)
            {
                var type = value.GetType();

                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);

                GUI.contentColor = Color.white;

                if (value != null && IsReflectableType(type) && !IsEnumerable(type))
                {
                    if (expandedObjects.ContainsKey(value.GetHashCode()))
                    {
                        if (GUILayout.Button("-", GUILayout.Width(16)))
                        {
                            expandedObjects.Remove(value.GetHashCode());
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("+", GUILayout.Width(16)))
                        {
                            expandedObjects.Add(value.GetHashCode(), true);
                        }
                    }
                }

                GUI.contentColor = Color.green;

                GUILayout.Label(type.ToString() + " ");

                GUI.contentColor = Color.red;

                GUILayout.Label(caller + ".[" + count.ToString() + "]");

                GUI.contentColor = Color.white;

                GUILayout.Label(" = ");
                GUILayout.Label(value == null ? "null" : value.ToString());

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (value != null && IsReflectableType(type) && expandedObjects.ContainsKey(value.GetHashCode()))
                {
                    if (value is GameObject)
                    {
                        OnSceneTreeComponents(caller + ".[" + count.ToString() + "]", (GameObject)value, ident + 1);
                    }
                    else if (value is Transform)
                    {
                        OnSceneTreeReflectUnityEngineTransform(caller + ".[" + count.ToString() + "]", (Transform)value, ident + 1);
                    }
                    else
                    {
                        OnSceneTreeReflect(caller + ".[" + count.ToString() + "]", value, ident + 1);
                    }
                }

                count++;
                if (count >= 128)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(treeIdentSpacing * ident);
                    GUILayout.Label("Array too large to display");
                    GUILayout.EndHorizontal();
                    break;
                }
            }
        }

        private void OnSceneTreeReflect(string caller, System.Object obj, int ident)
        {
            if (ident >= 12)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

            if (obj == null)
            {
                GUILayout.Label("null");
                return;
            }

            Type type = obj.GetType();

            if (type == typeof(UnityEngine.Transform))
            {
                OnSceneTreeReflectUnityEngineTransform(caller, (UnityEngine.Transform)obj, ident);
                return;
            }
            
            if (IsEnumerable(obj))
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

        private void OnSceneTreeComponents(string caller, GameObject obj, int ident)
        {
            if (ident >= 12)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

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

        private void OnSceneTreeRecursive(string caller, GameObject obj, int ident = 0)
        {
            if (ident >= 12)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(treeIdentSpacing * ident);
                GUILayout.Label("Hierarchy too deep, sorry :(");
                GUILayout.EndHorizontal();
                return;
            }

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

                OnSceneTreeComponents(obj.name, obj, ident + 1);

                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    OnSceneTreeRecursive(obj.name, obj.transform.GetChild(i).gameObject, ident + 1);
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

        public void DrawWindow()
        {
            watches = watches ?? FindObjectOfType<Watches>();
            if (watches == null)
            {
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

            GUIControls.StringField("Filter", ref nameFilter, 0.0f, true, true);

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

            GUILayout.EndScrollView();

            if (GUILayout.Button("Fold all"))
            {
                expanded.Clear();
                expandedComponents.Clear();
                expandedObjects.Clear();
            }
        }

    }

}