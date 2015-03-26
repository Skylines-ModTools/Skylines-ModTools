using System;
using System.Linq;
using UnityEngine;

namespace ModTools
{
    class GUIControls
    {

        public static float numberFieldSize = 100;
        public static float stringFieldSize = 200;
        public static float byteFieldSize = 40;
        public static float charFieldSize = 25;

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

        public static string BufferedTextField(string hash, string value, float fieldSize)
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

            if (currentHotControl == hash && (HotControl() == null || HotControl() == ""))
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

            string result = BufferedTextField(hash, value.ToString(), numberFieldSize);
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

            string result = BufferedTextField(hash, value.ToString(), numberFieldSize);
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
            string result = BufferedTextField(hash, value.ToString(), byteFieldSize);
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

            string result = BufferedTextField(hash, value.ToString(), numberFieldSize);
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

            string result = BufferedTextField(hash, value.ToString(), numberFieldSize);
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

            string result = BufferedTextField(hash, value.ToString(), numberFieldSize);
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

            string result = BufferedTextField(hash, value.ToString(), numberFieldSize);
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

            string result = BufferedTextField(hash, value.ToString(), numberFieldSize);
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

            string result = BufferedTextField(hash, value.ToString(), numberFieldSize);
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

            string result = BufferedTextField(hash, value.ToString(), stringFieldSize);
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

            string result = BufferedTextField(hash, value.ToString(), charFieldSize);
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

        static public void EnumField(string hash, string name, ref object value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            var enumType = value.GetType();

            if (!noTypeLabel)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label(enumType.FullName);
            }

            GUI.contentColor = Color.red;
            GUILayout.Label(name);
            GUI.contentColor = Color.white;

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            var enumNames = Enum.GetNames(enumType).ToArray();

            if (TypeUtil.IsBitmaskEnum(enumType))
            {
                GUILayout.Label(value.ToString());
            }
            else
            {
                int i = 0;
                for (; i < enumNames.Length; i++)
                {
                    if (value.ToString() == enumNames[i])
                    {
                        break;
                    }
                }

                int newIndex = GUIComboBox.Box(i, enumNames, hash);
                value = Enum.Parse(enumType, enumNames[newIndex]);
            }

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

            var r = (byte)(Mathf.Clamp(value.r * 255.0f, byte.MinValue, byte.MaxValue));
            var g = (byte)(Mathf.Clamp(value.g * 255.0f, byte.MinValue, byte.MaxValue));
            var b = (byte)(Mathf.Clamp(value.b * 255.0f, byte.MinValue, byte.MaxValue));
            var a = (byte)(Mathf.Clamp(value.a * 255.0f, byte.MinValue, byte.MaxValue));

            ByteField(hash + ".r", "r", ref r, 0.0f, true, true);
            ByteField(hash + ".g", "g", ref g, 0.0f, true, true);
            ByteField(hash + ".b", "b", ref b, 0.0f, true, true);
            ByteField(hash + ".a", "a", ref a, 0.0f, true, true);

            value.r = Mathf.Clamp01((float)r / 255.0f);
            value.g = Mathf.Clamp01((float)g / 255.0f);
            value.b = Mathf.Clamp01((float)b / 255.0f);
            value.a = Mathf.Clamp01((float)a / 255.0f);

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