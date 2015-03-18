using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModTools
{
    public class Watches : GUIWindow
    {

        private Vector2 watchesScroll = Vector2.zero;

        public Watches()
            : base("Watches", new Rect(504, 128, 800, 300), skin)
        {
            onDraw = DoWatchesWindow;
        }

        public void AddWatch(string name, FieldInfo field, object o)
        {
            fieldWatches.Add(name, new KeyValuePair<FieldInfo, object>(field, o));
            visible = true;
        }

        public void AddWatch(string name, PropertyInfo property, object o)
        {
            propertyWatches.Add(name, new KeyValuePair<PropertyInfo, object>(property, o));
            visible = true;
        }

        public void RemoveWatch(string name)
        {
            if (fieldWatches.ContainsKey(name))
            {
                fieldWatches.Remove(name);
            }

            if (propertyWatches.ContainsKey(name))
            {
                propertyWatches.Remove(name);
            }
        }

        public Type GetWatchType(string name)
        {
            Type ret = null;

            if (fieldWatches.ContainsKey(name))
            {
                ret = fieldWatches[name].Key.FieldType;
            }

            if (propertyWatches.ContainsKey(name))
            {
                ret = propertyWatches[name].Key.PropertyType;
            }

            return ret;
        }

        public object ReadWatch(string name)
        {
            object ret = null;

            if (fieldWatches.ContainsKey(name))
            {
                try
                {
                    ret = fieldWatches[name].Key.GetValue(fieldWatches[name].Value);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            if (propertyWatches.ContainsKey(name))
            {
                try
                {
                    ret = propertyWatches[name].Key.GetValue(propertyWatches[name].Value, null);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return ret;
        }

        public bool IsConstWatch(string name)
        {
            if (fieldWatches.ContainsKey(name))
            {
                return fieldWatches[name].Key.IsInitOnly;
            }

            if (propertyWatches.ContainsKey(name))
            {
                return !propertyWatches[name].Key.CanWrite;
            }

            return true;
        }

        public void WriteWatch(string name, object value)
        {
            if (fieldWatches.ContainsKey(name))
            {
                try
                {
                    fieldWatches[name].Key.SetValue(fieldWatches[name].Value, value);
                }
                catch (Exception)
                {
                    return;
                }
            }

            if (propertyWatches.ContainsKey(name))
            {
                try
                {
                    propertyWatches[name].Key.SetValue(propertyWatches[name].Value, value, null);
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        public string[] GetWatches()
        {
            string[] watches = new string[fieldWatches.Count + propertyWatches.Count];
            int i = 0;
            foreach (var item in fieldWatches)
            {
                watches[i++] = item.Key;
            }

            foreach (var item in propertyWatches)
            {
                watches[i++] = item.Key;
            }

            return watches;
        }

        private Dictionary<string, KeyValuePair<FieldInfo, object>> fieldWatches = new Dictionary<string, KeyValuePair<FieldInfo, object>>();
        private Dictionary<string, KeyValuePair<PropertyInfo, object>> propertyWatches = new Dictionary<string, KeyValuePair<PropertyInfo, object>>();

        void DoWatchesWindow()
        {
            watchesScroll = GUILayout.BeginScrollView(watchesScroll);

            foreach (var watch in GetWatches())
            {
                GUILayout.BeginHorizontal();

                var type = GetWatchType(watch);

                GUI.contentColor = Color.green;
                GUILayout.Label(type.ToString());
                GUI.contentColor = Color.red;
                GUILayout.Label(watch);
                GUI.contentColor = Color.white;
                GUILayout.Label(" = ");

                if (IsConstWatch(watch))
                {
                    GUI.enabled = false;
                }

                var value = ReadWatch(watch);

                if (type.ToString() == "System.Single")
                {
                    var f = (float)value;
                    GUIControls.FloatField("watch." + watch, "", ref f, 0.0f, true, true);
                    if (f != (float)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.Double")
                {
                    var f = (double)value;
                    GUIControls.DoubleField("watch." + watch, "", ref f, 0.0f, true, true);
                    if (f != (double)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.Byte")
                {
                    var f = (byte)value;
                    GUIControls.ByteField("watch." + watch, "", ref f, 0.0f, true, true);
                    if (f != (byte)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.Int32")
                {
                    var f = (int)value;
                    GUIControls.IntField("watch." + watch, "", ref f, 0.0f, true, true);
                    if (f != (int)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.UInt32")
                {
                    var f = (uint)value;
                    GUIControls.UIntField("watch." + watch, "", ref f, 0.0f, true, true);
                    if (f != (uint)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.Int64")
                {
                    var f = (Int64)value;
                    GUIControls.Int64Field("watch." + watch, "", ref f, 0.0f, true, true);
                    if (f != (Int64)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.UInt64")
                {
                    var f = (UInt64)value;
                    GUIControls.UInt64Field("watch." + watch, "", ref f, 0.0f, true, true);
                    if (f != (UInt64)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.Int16")
                {
                    var f = (Int16)value;
                    GUIControls.Int16Field("watch." + watch, "", ref f, 0.0f, true, true);
                    if (f != (Int16)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.UInt16")
                {
                    var f = (UInt16)value;
                    GUIControls.UInt16Field("watch." + watch, "", ref f, 0.0f, true, true);
                    if (f != (UInt16)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.Boolean")
                {
                    var f = (bool)value;
                    GUIControls.BoolField("", ref f, 0.0f, true, true);
                    if (f != (bool)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.String")
                {
                    var f = (string)value;
                    GUIControls.StringField("watch." + watch, "", ref f, 0.0f, true, true);
                    if (f != (string)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.Char")
                {
                    var f = (char)value;
                    GUIControls.CharField("watch." + watch, "", ref f, 0.0f, true, true);
                    if (f != (char)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "UnityEngine.Vector3")
                {
                    var f = (Vector3)value;
                    GUIControls.Vector3Field("watch." + watch, "", ref f, 0.0f, null, true, true);
                    if (f != (Vector3)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "UnityEngine.Vector4")
                {
                    var f = (Vector4)value;
                    GUIControls.Vector4Field("watch." + watch, "", ref f, 0.0f, null, true, true);
                    if (f != (Vector4)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "UnityEngine.Quaternion")
                {
                    var f = (Quaternion)value;
                    GUIControls.QuaternionField("watch." + watch, "", ref f, 0.0f, null, true, true);
                    if (f != (Quaternion)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "UnityEngine.Color")
                {
                    var f = (Color)value;
                    GUIControls.ColorField("watch." + watch, "", ref f, 0.0f, null, true, true);
                    if (f != (Color)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "UnityEngine.Color32")
                {
                    var f = (Color32)value;
                    GUIControls.Color32Field("watch." + watch, "", ref f, 0.0f, null, true, true);
                    var v = (Color32)value;
                    if (f.r != v.r || f.g != v.g || f.b != v.b || f.a != v.a)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else
                {
                    GUILayout.Label(value.ToString());
                }

                GUI.enabled = true;

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("x", GUILayout.Width(24)))
                {
                    RemoveWatch(watch);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

    }

}
