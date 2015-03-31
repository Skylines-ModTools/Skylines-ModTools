using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModTools
{
    public class Watches : GUIWindow
    {

        private Configuration config
        {
            get { return ModTools.Instance.config; }
        }

        private Vector2 watchesScroll = Vector2.zero;

        public Watches()
            : base("Watches", new Rect(504, 128, 800, 300), skin)
        {
            onDraw = DoWatchesWindow;
        }

        public void AddWatch(ReferenceChain refChain, FieldInfo field, object o)
        {
            fieldWatches.Add(refChain, new KeyValuePair<FieldInfo, object>(field, o));
            visible = true;
        }

        public void AddWatch(ReferenceChain refChain, PropertyInfo property, object o)
        {
            propertyWatches.Add(refChain, new KeyValuePair<PropertyInfo, object>(property, o));
            visible = true;
        }

        public void RemoveWatch(ReferenceChain refChain)
        {
            if (fieldWatches.ContainsKey(refChain))
            {
                fieldWatches.Remove(refChain);
            }

            if (propertyWatches.ContainsKey(refChain))
            {
                propertyWatches.Remove(refChain);
            }
        }

        public Type GetWatchType(ReferenceChain refChain)
        {
            Type ret = null;

            if (fieldWatches.ContainsKey(refChain))
            {
                ret = fieldWatches[refChain].Key.FieldType;
            }

            if (propertyWatches.ContainsKey(refChain))
            {
                ret = propertyWatches[refChain].Key.PropertyType;
            }

            return ret;
        }

        public object ReadWatch(ReferenceChain refChain)
        {
            object ret = null;

            if (fieldWatches.ContainsKey(refChain))
            {
                try
                {
                    ret = fieldWatches[refChain].Key.GetValue(fieldWatches[refChain].Value);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            if (propertyWatches.ContainsKey(refChain))
            {
                try
                {
                    ret = propertyWatches[refChain].Key.GetValue(propertyWatches[refChain].Value, null);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return ret;
        }

        public bool IsConstWatch(ReferenceChain refChain)
        {
            if (fieldWatches.ContainsKey(refChain))
            {
                return fieldWatches[refChain].Key.IsInitOnly;
            }

            if (propertyWatches.ContainsKey(refChain))
            {
                return !propertyWatches[refChain].Key.CanWrite;
            }

            return true;
        }

        public void WriteWatch(ReferenceChain refChain, object value)
        {
            if (fieldWatches.ContainsKey(refChain))
            {
                try
                {
                    fieldWatches[refChain].Key.SetValue(fieldWatches[refChain].Value, value);
                }
                catch (Exception)
                {
                    return;
                }
            }

            if (propertyWatches.ContainsKey(refChain))
            {
                try
                {
                    propertyWatches[refChain].Key.SetValue(propertyWatches[refChain].Value, value, null);
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        public ReferenceChain[] GetWatches()
        {
            ReferenceChain[] watches = new ReferenceChain[fieldWatches.Count + propertyWatches.Count];
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

        private Dictionary<ReferenceChain, KeyValuePair<FieldInfo, object>> fieldWatches = new Dictionary<ReferenceChain, KeyValuePair<FieldInfo, object>>();
        private Dictionary<ReferenceChain, KeyValuePair<PropertyInfo, object>> propertyWatches = new Dictionary<ReferenceChain, KeyValuePair<PropertyInfo, object>>();

        void DoWatchesWindow()
        {
            watchesScroll = GUILayout.BeginScrollView(watchesScroll);

            foreach (var watch in GetWatches())
            {
                GUILayout.BeginHorizontal();

                var type = GetWatchType(watch);

                GUI.contentColor = config.typeColor;
                GUILayout.Label(type.ToString());
                GUI.contentColor = config.nameColor;
                GUILayout.Label(watch.ToString());
                GUI.contentColor = Color.white;
                GUILayout.Label(" = ");

                if (IsConstWatch(watch))
                {
                    GUI.enabled = false;
                }

                var value = ReadWatch(watch);
                GUI.contentColor = config.valueColor;

                if (value == null || !TypeUtil.IsBuiltInType(type))
                {
                    GUILayout.Label(value == null ? "null" : value.ToString());
                }
                else
                {
                    try
                    {
                        var newValue = GUIControls.EditorValueField(watch, "watch."+watch, type, value);
                        if (newValue != value)
                        {
                            WriteWatch(watch, newValue);
                        }
                    }
                    catch (Exception)
                    {
                        GUILayout.Label(value == null ? "null" : value.ToString());
                    }
                }

                GUI.contentColor = Color.white;

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
