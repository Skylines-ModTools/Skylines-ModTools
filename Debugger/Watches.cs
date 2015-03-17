using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModTools
{
    public static class Watches
    {

        public static bool showWindow = false;

        public static void AddWatch(string name, FieldInfo field, object o)
        {
            fieldWatches.Add(name, new KeyValuePair<FieldInfo, object>(field, o));
            showWindow = true;
        }

        public static void AddWatch(string name, PropertyInfo property, object o)
        {
            propertyWatches.Add(name, new KeyValuePair<PropertyInfo, object>(property, o));
            showWindow = true;
        }

        public static void RemoveWatch(string name)
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

        public static Type GetWatchType(string name)
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

        public static object ReadWatch(string name)
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

        public static void WriteWatch(string name, object value)
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

        public static string[] GetWatches()
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

        private static Dictionary<string, KeyValuePair<FieldInfo, object>> fieldWatches = new Dictionary<string, KeyValuePair<FieldInfo, object>>();
        private static Dictionary<string, KeyValuePair<PropertyInfo, object>> propertyWatches = new Dictionary<string, KeyValuePair<PropertyInfo, object>>();

    }

}
