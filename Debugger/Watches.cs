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
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            visible = GUILayout.Toggle(visible, "");
            GUILayout.EndHorizontal();

            watchesScroll = GUILayout.BeginScrollView(watchesScroll);

            foreach (var watch in GetWatches())
            {
                GUILayout.BeginHorizontal();

                var type = GetWatchType(watch);

                GUI.contentColor = Color.red;
                GUILayout.Label(type.ToString());
                GUI.contentColor = Color.green;
                GUILayout.Label(watch);
                GUI.contentColor = Color.white;
                GUILayout.Label(" = ");

                var value = ReadWatch(watch);

                if (type.ToString() == "System.Single")
                {
                    var f = (float)value;
                    GUIControls.FloatField("", ref f);
                    if (f != (float)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.Int32")
                {
                    var f = (int)value;
                    GUIControls.IntField("", ref f);
                    if (f != (int)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.Boolean")
                {
                    var f = (bool)value;
                    GUIControls.BoolField("", ref f);
                    if (f != (bool)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else if (type.ToString() == "System.String")
                {
                    var f = (string)value;
                    GUIControls.StringField("", ref f);
                    if (f != (string)value)
                    {
                        WriteWatch(watch, f);
                    }
                }
                else
                {
                    GUILayout.Label(value.ToString());
                }

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
