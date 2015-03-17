using System;
using UnityEngine;

namespace ModTools
{
    class GUIControls
    {

        static float fieldSize = 200;

        public delegate void WatchButtonCallback();

        public static bool IsHot(string hash)
        {
            return hash == GUI.GetNameOfFocusedControl();
        }

        public static string HotControl()
        {
            return GUI.GetNameOfFocusedControl();
        }

        public static string currentHotControl = null;
        public static string hotControlBuffer = "";

        public static string BufferedTextField(string hash, string value)
        {
            GUI.SetNextControlName(hash);
            bool isHot = IsHot(hash);

            string newBuffer = GUILayout.TextField(isHot ? hotControlBuffer : value.ToString(), GUILayout.Width(fieldSize));
            string res = null;

            if (isHot)
            {
                if (currentHotControl == null)
                {
                    currentHotControl = hash;
                    hotControlBuffer = value;
                }
                else
                {
                    hotControlBuffer = newBuffer;
                }
            }
            else if (currentHotControl == hash)
            {
                res = hotControlBuffer;
                currentHotControl = null;
                hotControlBuffer = "";
            }

            return res;
        }

        public static void FloatField(string hash, string name, ref float value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
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

            GUI.contentColor = Color.white;

            string result = BufferedTextField(hash, value.ToString());
            if (result != null)
            {
                float newValue;
                if (float.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUILayout.EndHorizontal();
        }

        static public void DoubleField(string hash, string name, ref double value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
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

            GUI.contentColor = Color.white;

            string result = BufferedTextField(hash, value.ToString());
            if (result != null)
            {
                double newValue;
                if (double.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUILayout.EndHorizontal();
        }

        static public void ByteField(string hash, string name, ref byte value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
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
            string result = BufferedTextField(hash, value.ToString());
            if (result != null)
            {
                byte newValue;
                if (byte.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUILayout.EndHorizontal();
        }

        static public void IntField(string hash, string name, ref int value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
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

            GUI.contentColor = Color.white;

            string result = BufferedTextField(hash, value.ToString());
            if (result != null)
            {
                int newValue;
                if (int.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void UIntField(string hash, string name, ref uint value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
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

            GUI.contentColor = Color.white;

            string result = BufferedTextField(hash, value.ToString());
            if (result != null)
            {
                uint newValue;
                if (uint.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void Int64Field(string hash, string name, ref Int64 value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
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

            GUI.contentColor = Color.white;

            string result = BufferedTextField(hash, value.ToString());
            if (result != null)
            {
                Int64 newValue;
                if (Int64.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUILayout.EndHorizontal();
        }

        static public void UInt64Field(string hash, string name, ref UInt64 value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
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

            GUI.contentColor = Color.white;

            string result = BufferedTextField(hash, value.ToString());
            if (result != null)
            {
                UInt64 newValue;
                if (UInt64.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUILayout.EndHorizontal();
        }

        static public void Int16Field(string hash, string name, ref Int16 value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
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

            string result = BufferedTextField(hash, value.ToString());
            if (result != null)
            {
                Int16 newValue;
                if (Int16.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void UInt16Field(string hash, string name, ref UInt16 value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
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

            string result = BufferedTextField(hash, value.ToString());
            if (result != null)
            {
                UInt16 newValue;
                if (UInt16.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUILayout.EndHorizontal();
        }

        static public void StringField(string hash, string name, ref string value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
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

            string result = BufferedTextField(hash, value.ToString());
            if (result != null)
            {
                value = result;
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        static public void CharField(string hash, string name, ref char value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
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

            string result = BufferedTextField(hash, value.ToString());
            if (result != null)
            {
                value = result[0];
            }

            GUILayout.EndHorizontal();
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

        static public void Vector2Field(string hash, string name, ref Vector2 value, float ident = 0.0f, WatchButtonCallback watch = null, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("Vector2");
            }

            GUI.contentColor = Color.red;
            GUILayout.Label(name);
            GUI.contentColor = Color.white;

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            FloatField(hash+".x", "x", ref value.x, 0.0f, true, true);
            FloatField(hash+".y", "y", ref value.y, 0.0f, true, true);

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

        static public void Vector3Field(string hash, string name, ref Vector3 value, float ident = 0.0f, WatchButtonCallback watch = null, bool noSpace = false, bool noTypeLabel = false)
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

            FloatField(hash+".x", "x", ref value.x, 0.0f, true, true);
            FloatField(hash+".y", "y", ref value.y, 0.0f, true, true);
            FloatField(hash+".z", "z", ref value.z, 0.0f, true, true);

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

        static public void Vector4Field(string hash, string name, ref Vector4 value, float ident = 0.0f, WatchButtonCallback watch = null, bool noSpace = false, bool noTypeLabel = false)
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

            FloatField(hash+".x", "x", ref value.x, 0.0f, true, true);
            FloatField(hash+".y", "y", ref value.y, 0.0f, true, true);
            FloatField(hash+".z", "z", ref value.z, 0.0f, true, true);
            FloatField(hash+".w", "w", ref value.w, 0.0f, true, true);

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

        static public void QuaternionField(string hash, string name, ref Quaternion value, float ident = 0.0f, WatchButtonCallback watch = null, bool noSpace = false, bool noTypeLabel = false)
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

            FloatField(hash+".x", "x", ref euler.x, 0.0f, true, true);
            FloatField(hash+".y", "y", ref euler.y, 0.0f, true, true);
            FloatField(hash+".z", "z", ref euler.z, 0.0f, true, true);

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

        static public void ColorField(string hash, string name, ref Color value, float ident = 0.0f, WatchButtonCallback watch = null, bool noSpace = false, bool noTypeLabel = false)
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

            FloatField(hash+".r", "r", ref value.r, 0.0f, true, true);
            FloatField(hash + ".g", "g", ref value.g, 0.0f, true, true);
            FloatField(hash + ".b", "b", ref value.b, 0.0f, true, true);
            FloatField(hash + ".a", "a", ref value.a, 0.0f, true, true);

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

        static public void Color32Field(string hash, string name, ref Color32 value, float ident = 0.0f, WatchButtonCallback watch = null, bool noSpace = false, bool noTypeLabel = false)
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

            ByteField(hash+".r", "r", ref value.r, 0.0f, true, true);
            ByteField(hash+".g", "g", ref value.g, 0.0f, true, true);
            ByteField(hash+".b", "b", ref value.b, 0.0f, true, true);
            ByteField(hash+".a", "a", ref value.a, 0.0f, true, true);

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