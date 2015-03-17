using System;
using UnityEngine;

namespace ModTools
{
    class GUIControls
    {

        static float fieldSize = 128;

        public delegate void WatchButtonCallback();

        static public void FloatField(string name, ref float value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("float");
            }

            GUI.contentColor = Color.red;
            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            float oldValue = value;
            string oldValueString = value.ToString();
            GUI.contentColor = Color.white;

            string newValue = GUILayout.TextField(oldValueString, GUILayout.Width(fieldSize));

            if (oldValueString != newValue && !float.TryParse(newValue, out value))
            {
                value = oldValue;
            }

            GUILayout.EndHorizontal();
        }

        static public void DoubleField(string name, ref double value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("double");
            }

            GUI.contentColor = Color.red;
            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            double oldValue = value;
            string oldValueString = value.ToString();
            GUI.contentColor = Color.white;

            string newValue = GUILayout.TextField(oldValueString, GUILayout.Width(fieldSize));

            if (oldValueString != newValue && !double.TryParse(newValue, out value))
            {
                value = oldValue;
            }

            GUILayout.EndHorizontal();
        }

        static public void ByteField(string name, ref byte value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("int");
            }

            GUI.contentColor = Color.red;

            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            byte oldValue = value;

            GUI.contentColor = Color.white;
            string newValue = GUILayout.TextField(value.ToString(), GUILayout.Width(fieldSize));
            if (!byte.TryParse(newValue, out value))
            {
                value = oldValue;
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void IntField(string name, ref int value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("int");
            }
           
            GUI.contentColor = Color.red;

            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            int oldValue = value;

            GUI.contentColor = Color.white;
            string newValue = GUILayout.TextField(value.ToString(), GUILayout.Width(fieldSize));
            if (!int.TryParse(newValue, out value))
            {
                value = oldValue;
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void UIntField(string name, ref uint value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("uint");
            }

            GUI.contentColor = Color.red;

            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            uint oldValue = value;

            GUI.contentColor = Color.white;
            string newValue = GUILayout.TextField(value.ToString(), GUILayout.Width(fieldSize));
            if (!uint.TryParse(newValue, out value))
            {
                value = oldValue;
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void Int64Field(string name, ref Int64 value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("Int64");
            }

            GUI.contentColor = Color.red;

            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            Int64 oldValue = value;

            GUI.contentColor = Color.white;
            string newValue = GUILayout.TextField(value.ToString(), GUILayout.Width(fieldSize));
            if (!Int64.TryParse(newValue, out value))
            {
                value = oldValue;
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void UInt64Field(string name, ref UInt64 value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("UInt64");
            }

            GUI.contentColor = Color.red;

            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            UInt64 oldValue = value;

            GUI.contentColor = Color.white;
            string newValue = GUILayout.TextField(value.ToString(), GUILayout.Width(fieldSize));
            if (!UInt64.TryParse(newValue, out value))
            {
                value = oldValue;
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void Int16Field(string name, ref Int16 value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("Int16");
            }

            GUI.contentColor = Color.red;

            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            Int16 oldValue = value;

            GUI.contentColor = Color.white;
            string newValue = GUILayout.TextField(value.ToString(), GUILayout.Width(fieldSize));
            if (!Int16.TryParse(newValue, out value))
            {
                value = oldValue;
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void UInt16Field(string name, ref UInt16 value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("UInt16");
            }

            GUI.contentColor = Color.red;

            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            UInt16 oldValue = value;

            GUI.contentColor = Color.white;
            string newValue = GUILayout.TextField(value.ToString(), GUILayout.Width(fieldSize));
            if (!UInt16.TryParse(newValue, out value))
            {
                value = oldValue;
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void StringField(string name, ref string value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("string");
            }
           
            GUI.contentColor = Color.red;
            GUILayout.Label(name);
            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }
            GUI.contentColor = Color.white;
            value = GUILayout.TextField(value, GUILayout.Width(fieldSize));
            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void CharField(string name, ref char value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("string");
            }

            GUI.contentColor = Color.red;
            GUILayout.Label(name);
            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }
            GUI.contentColor = Color.white;
            value = GUILayout.TextField(""+value, GUILayout.Width(fieldSize))[0];
            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void BoolField(string name, ref bool value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("bool");
            }

            GUI.contentColor = Color.red;
            GUILayout.Label(name);
            GUI.contentColor = Color.white;

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            value = GUILayout.Toggle(value, "");
            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void Vector3Field(string name, ref Vector3 value, float ident = 0.0f, WatchButtonCallback watch = null, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("Vector3"); 
            }

            GUI.contentColor = Color.red;
            GUILayout.Label(name);
            GUI.contentColor = Color.white;

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            FloatField("x", ref value.x, 0.0f, true, true);
            FloatField("y", ref value.y, 0.0f, true, true);
            FloatField("z", ref value.z, 0.0f, true, true);

            if (watch != null)
            {
                if (GUILayout.Button("Watch"))
                {
                    watch();
                }
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void Vector4Field(string name, ref Vector4 value, float ident = 0.0f, WatchButtonCallback watch = null, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("Vector4");
            }

            GUI.contentColor = Color.red;
            GUILayout.Label(name);
            GUI.contentColor = Color.white;

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            FloatField("x", ref value.x, 0.0f, true, true);
            FloatField("y", ref value.y, 0.0f, true, true);
            FloatField("z", ref value.z, 0.0f, true, true);
            FloatField("w", ref value.w, 0.0f, true, true);

            if (watch != null)
            {
                if (GUILayout.Button("Watch"))
                {
                    watch();
                }
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void QuaternionField(string name, ref Quaternion value, float ident = 0.0f, WatchButtonCallback watch = null, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("Vector4");
            }

            GUI.contentColor = Color.red;
            GUILayout.Label(name);
            GUI.contentColor = Color.white;

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            var euler = value.eulerAngles;

            FloatField("x", ref euler.x, 0.0f, true, true);
            FloatField("y", ref euler.y, 0.0f, true, true);
            FloatField("z", ref euler.z, 0.0f, true, true);

            if (euler != value.eulerAngles)
            {
                value = Quaternion.Euler(euler);
            }

            if (watch != null)
            {
                if (GUILayout.Button("Watch"))
                {
                    watch();
                }
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void ColorField(string name, ref Color value, float ident = 0.0f, WatchButtonCallback watch = null, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("Color");
            }

            GUI.contentColor = Color.red;
            GUILayout.Label(name);
            GUI.contentColor = Color.white;

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            FloatField("r", ref value.r, 0.0f, true, true);
            FloatField("g", ref value.g, 0.0f, true, true);
            FloatField("b", ref value.b, 0.0f, true, true);
            FloatField("a", ref value.a, 0.0f, true, true);

            if (watch != null)
            {
                if (GUILayout.Button("Watch"))
                {
                    watch();
                }
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void Color32Field(string name, ref Color32 value, float ident = 0.0f, WatchButtonCallback watch = null, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("Color");
            }

            GUI.contentColor = Color.red;
            GUILayout.Label(name);
            GUI.contentColor = Color.white;

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            ByteField("r", ref value.r, 0.0f, true, true);
            ByteField("g", ref value.g, 0.0f, true, true);
            ByteField("b", ref value.b, 0.0f, true, true);
            ByteField("a", ref value.a, 0.0f, true, true);

            if (watch != null)
            {
                if (GUILayout.Button("Watch"))
                {
                    watch();
                }
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

    }

}