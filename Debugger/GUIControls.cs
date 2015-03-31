using System;
using System.Linq;
using UnityEngine;

namespace ModTools
{
    class GUIControls
    {

        private static Configuration config
        {
            get { return ModTools.Instance.config; }
        }

        public static float numberFieldSize = 100;
        public static float stringFieldSize = 200;
        public static float byteFieldSize = 40;
        public static float charFieldSize = 25;

        public delegate void WatchButtonCallback();

        private static bool IsHot(string hash)
        {
            return hash == GUI.GetNameOfFocusedControl();
        }

        private static string HotControl()
        {
            return GUI.GetNameOfFocusedControl();
        }

        private static string currentHotControl = null;
        private static string hotControlBuffer = "";

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
                else if(currentHotControl == hash)
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

        public static object EditorValueField(ReferenceChain refChain, string hash, Type type, object value)
        {
            if (type == typeof(System.Single))
            {
                var f = (float)value;
                FloatField(hash, "", ref f, 0.0f, true, true);
                if (f != (float)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(System.Double))
            {
                var f = (double)value;
                DoubleField(hash, "", ref f, 0.0f, true, true);
                if (f != (double)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(System.Byte))
            {
                var f = (byte)value;
                ByteField(hash, "", ref f, 0.0f, true, true);
                if (f != (byte)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(System.Int32))
            {
                var f = (int)value;
                IntField(hash, "", ref f, 0.0f, true, true);
                if (f != (int)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(System.UInt32))
            {
                var f = (uint)value;
                UIntField(hash, "", ref f, 0.0f, true, true);
                if (f != (uint)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(System.Int64))
            {
                var f = (Int64)value;
                Int64Field(hash, "", ref f, 0.0f, true, true);
                if (f != (Int64)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(System.UInt64))
            {
                var f = (UInt64)value;
                UInt64Field(hash, "", ref f, 0.0f, true, true);
                if (f != (UInt64)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(System.Int16))
            {
                var f = (Int16)value;
                Int16Field(hash, "", ref f, 0.0f, true, true);
                if (f != (Int16)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(System.UInt16))
            {
                var f = (UInt16)value;
                UInt16Field(hash, "", ref f, 0.0f, true, true);
                if (f != (UInt16)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(System.Boolean))
            {
                var f = (bool)value;
                BoolField("", ref f, 0.0f, true, true);
                if (f != (bool)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(System.String))
            {
                var f = (string)value;
                StringField(hash, "", ref f, 0.0f, true, true);
                if (f != (string)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(System.Char))
            {
                var f = (char)value;
                CharField(hash, "", ref f, 0.0f, true, true);
                if (f != (char)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(UnityEngine.Vector2))
            {
                var f = (Vector2)value;
                Vector2Field(hash, "", ref f, 0.0f, null, true, true);
                if (f != (Vector2)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(UnityEngine.Vector3))
            {
                var f = (Vector3)value;
                Vector3Field(hash, "", ref f, 0.0f, null, true, true);
                if (f != (Vector3)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(UnityEngine.Vector4))
            {
                var f = (Vector4)value;
                Vector4Field(hash, "", ref f, 0.0f, null, true, true);
                if (f != (Vector4)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(UnityEngine.Quaternion))
            {
                var f = (Quaternion)value;
                QuaternionField(hash, "", ref f, 0.0f, null, true, true);
                if (f != (Quaternion)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(UnityEngine.Color))
            {
                var f = (Color)value;
                ColorField(hash, "", ref f, 0.0f, null, true, true, color => { refChain.SetValue(color); });
                if (f != (Color)value)
                {
                    return f;
                }

                return value;
            }

            if (type == typeof(UnityEngine.Color32))
            {
                var f = (Color32)value;
                Color32Field(hash, "", ref f, 0.0f, null, true, true, color => { refChain.SetValue(color); });
                var v = (Color32)value;
                if (f.r != v.r || f.g != v.g || f.b != v.b || f.a != v.a)
                {
                    return f;
                }

                return value;
            }

            if (type.IsEnum)
            {
                var f = value;
                EnumField(hash, "", ref f, 0.0f, true, true);
                if (f != value)
                {
                    return f;
                }

                return value;
            }

            return value;
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
                GUI.contentColor = config.typeColor;
                GUILayout.Label("float");
            }

            GUI.contentColor = config.nameColor;
            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            GUI.contentColor = config.valueColor;

            string result = BufferedTextField(hash, value.ToString(), numberFieldSize);
            if (result != null)
            {
                float newValue;
                if (Single.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUI.contentColor = Color.white;

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
                GUI.contentColor = config.typeColor;
                GUILayout.Label("double");
            }

            GUI.contentColor = config.nameColor;
            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            GUI.contentColor = config.valueColor;

            string result = BufferedTextField(hash, value.ToString(), numberFieldSize);
            if (result != null)
            {
                double newValue;
                if (Double.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUI.contentColor = Color.white;

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
                GUI.contentColor = config.typeColor;
                GUILayout.Label("int");
            }

            GUI.contentColor = config.nameColor;

            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            byte oldValue = value;

            GUI.contentColor = config.valueColor;
            string result = BufferedTextField(hash, value.ToString(), byteFieldSize);
            if (result != null)
            {
                byte newValue;
                if (Byte.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUI.contentColor = Color.white;

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
                GUI.contentColor = config.typeColor;
                GUILayout.Label("int");
            }
           
            GUI.contentColor = config.nameColor;

            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            GUI.contentColor = config.valueColor;

            string result = BufferedTextField(hash, value.ToString(), numberFieldSize);
            if (result != null)
            {
                int newValue;
                if (Int32.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
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
                GUI.contentColor = config.typeColor;
                GUILayout.Label("uint");
            }

            GUI.contentColor = config.nameColor;

            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            GUI.contentColor = config.valueColor;

            string result = BufferedTextField(hash, value.ToString(), numberFieldSize);
            if (result != null)
            {
                uint newValue;
                if (UInt32.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();
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
                GUI.contentColor = config.typeColor;
                GUILayout.Label("Int64");
            }

            GUI.contentColor = config.nameColor;

            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            GUI.contentColor = config.valueColor;

            string result = BufferedTextField(hash, value.ToString(), numberFieldSize);
            if (result != null)
            {
                Int64 newValue;
                if (Int64.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUI.contentColor = Color.white;

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
                GUI.contentColor = config.typeColor;
                GUILayout.Label("UInt64");
            }

            GUI.contentColor = config.nameColor;

            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            GUI.contentColor = config.valueColor;

            string result = BufferedTextField(hash, value.ToString(), numberFieldSize);
            if (result != null)
            {
                UInt64 newValue;
                if (UInt64.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUI.contentColor = Color.white;

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
                GUI.contentColor = config.typeColor;
                GUILayout.Label("Int16");
            }

            GUI.contentColor = config.nameColor;

            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            Int16 oldValue = value;

            GUI.contentColor = config.valueColor;

            string result = BufferedTextField(hash, value.ToString(), numberFieldSize);
            if (result != null)
            {
                Int16 newValue;
                if (Int16.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();
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
                GUI.contentColor = config.typeColor;
                GUILayout.Label("UInt16");
            }

            GUI.contentColor = config.nameColor;

            GUILayout.Label(name);

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            GUI.contentColor = config.valueColor;

            string result = BufferedTextField(hash, value.ToString(), numberFieldSize);
            if (result != null)
            {
                UInt16 newValue;
                if (UInt16.TryParse(result, out newValue))
                {
                    value = newValue;
                }
            }

            GUI.contentColor = Color.white;

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
                GUI.contentColor = config.typeColor;
                GUILayout.Label("string");
            }
           
            GUI.contentColor = config.nameColor;
            GUILayout.Label(name);
            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            GUI.contentColor = config.valueColor;

            string result = BufferedTextField(hash, value.ToString(), stringFieldSize);
            if (result != null)
            {
                value = result;
            }

            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();
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
                GUI.contentColor = config.typeColor;
                GUILayout.Label("string");
            }

            GUI.contentColor = config.nameColor;
            GUILayout.Label(name);
            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            GUI.contentColor = config.valueColor;

            string result = BufferedTextField(hash, value.ToString(), charFieldSize);
            if (result != null)
            {
                value = result[0];
            }

            GUI.contentColor = Color.white;

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
                GUI.contentColor = config.typeColor;
                GUILayout.Label("bool");
            }

            GUI.contentColor = config.nameColor;
            GUILayout.Label(name);

            GUI.contentColor = config.valueColor;
           
            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            value = GUILayout.Toggle(value, "");

            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();
        }

        static public void EnumField(string hash, string name, ref object value, float ident = 0.0f, bool noSpace = false, bool noTypeLabel = false)
        {
            GUILayout.BeginHorizontal();

            try
            {
                if (ident != 0.0f)
                {
                    GUILayout.Space(ident);
                }

                var enumType = value.GetType();

                if (!noTypeLabel)
                {
                    GUI.contentColor = config.typeColor;
                    GUILayout.Label(enumType.FullName);
                }

                GUI.contentColor = config.nameColor;
                GUILayout.Label(name);
                GUI.contentColor = config.valueColor;

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

                GUI.contentColor = Color.white;
            }
            catch (Exception)
            {
                GUILayout.EndHorizontal();
                throw;
            }

            GUILayout.EndHorizontal();
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
                GUI.contentColor = config.typeColor;
                GUILayout.Label("Vector2");
            }

            GUI.contentColor = config.nameColor;
            GUILayout.Label(name);

            GUI.contentColor = config.valueColor;

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

            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();
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
                GUI.contentColor = config.typeColor;
                GUILayout.Label("Vector3"); 
            }

            GUI.contentColor = config.nameColor;
            GUILayout.Label(name);

            GUI.contentColor = config.valueColor;

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

            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();
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
                GUI.contentColor = config.typeColor;
                GUILayout.Label("Vector4");
            }

            GUI.contentColor = config.nameColor;
            GUILayout.Label(name);

            GUI.contentColor = config.valueColor;

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

            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();
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
                GUI.contentColor = config.typeColor;
                GUILayout.Label("Vector4");
            }

            GUI.contentColor = config.nameColor;
            GUILayout.Label(name);

            GUI.contentColor = config.valueColor;

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

            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();
        }

        static public void ColorField(string hash, string name, ref Color value, float ident = 0.0f, WatchButtonCallback watch = null, bool noSpace = false, bool noTypeLabel = false, ColorPicker.OnColorChanged onColorChanged = null)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = config.typeColor;
                GUILayout.Label("Color");
            }

            GUI.contentColor = config.nameColor;
            GUILayout.Label(name);

            GUI.contentColor = config.valueColor;

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            var r = (byte)(Mathf.Clamp(value.r * 255.0f, Byte.MinValue, Byte.MaxValue));
            var g = (byte)(Mathf.Clamp(value.g * 255.0f, Byte.MinValue, Byte.MaxValue));
            var b = (byte)(Mathf.Clamp(value.b * 255.0f, Byte.MinValue, Byte.MaxValue));
            var a = (byte)(Mathf.Clamp(value.a * 255.0f, Byte.MinValue, Byte.MaxValue));

            ByteField(hash + ".r", "r", ref r, 0.0f, true, true);
            ByteField(hash + ".g", "g", ref g, 0.0f, true, true);
            ByteField(hash + ".b", "b", ref b, 0.0f, true, true);
            ByteField(hash + ".a", "a", ref a, 0.0f, true, true);

            value.r = Mathf.Clamp01((float)r / 255.0f);
            value.g = Mathf.Clamp01((float)g / 255.0f);
            value.b = Mathf.Clamp01((float)b / 255.0f);
            value.a = Mathf.Clamp01((float)a / 255.0f);

            if (onColorChanged != null)
            {
                if (value.r != r || value.g != g || value.b != b || value.a != a)
                {
                    onColorChanged(value);
                }

                if (GUILayout.Button("", GUILayout.Width(72)))
                {
                    var picker = ModTools.Instance.colorPicker;
                    picker.SetColor(value, onColorChanged);

                    Vector2 mouse = Input.mousePosition;
                    mouse.y = Screen.height - mouse.y;

                    picker.rect.position = mouse;
                    picker.visible = true;
                }

                var lastRect = GUILayoutUtility.GetLastRect();
                lastRect.x += 4.0f;
                lastRect.y += 4.0f;
                lastRect.width -= 8.0f;
                lastRect.height -= 8.0f;
                GUI.DrawTexture(lastRect, ColorPicker.GetColorTexture(hash, value), ScaleMode.StretchToFill);
            }
            
            if (watch != null)
            {
                if (GUILayout.Button("Watch"))
                {
                    watch();
                }
            }

            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();
        }

        static public void Color32Field(string hash, string name, ref Color32 value, float ident = 0.0f, WatchButtonCallback watch = null, bool noSpace = false, bool noTypeLabel = false, ColorPicker.OnColor32Changed onColor32Changed = null)
        {
            GUILayout.BeginHorizontal();

            if (ident != 0.0f)
            {
                GUILayout.Space(ident);
            }

            if (!noTypeLabel)
            {
                GUI.contentColor = config.typeColor;
                GUILayout.Label("Color");
            }

            GUI.contentColor = config.nameColor;
            GUILayout.Label(name);

            GUI.contentColor = config.valueColor;

            if (!noSpace)
            {
                GUILayout.FlexibleSpace();
            }

            ByteField(hash+".r", "r", ref value.r, 0.0f, true, true);
            ByteField(hash+".g", "g", ref value.g, 0.0f, true, true);
            ByteField(hash+".b", "b", ref value.b, 0.0f, true, true);
            ByteField(hash+".a", "a", ref value.a, 0.0f, true, true);

            if (onColor32Changed != null)
            {
                if (GUILayout.Button("", GUILayout.Width(72)))
                {
                    var picker = ModTools.Instance.colorPicker;
                    picker.SetColor(value, onColor32Changed);

                    Vector2 mouse = Input.mousePosition;
                    mouse.y = Screen.height - mouse.y;

                    picker.rect.position = mouse;
                    picker.visible = true;
                }

                var lastRect = GUILayoutUtility.GetLastRect();
                lastRect.x += 4.0f;
                lastRect.y += 4.0f;
                lastRect.width -= 8.0f;
                lastRect.height -= 8.0f;
                GUI.DrawTexture(lastRect, ColorPicker.GetColorTexture(hash, value), ScaleMode.StretchToFill);
            }

            if (watch != null)
            {
                if (GUILayout.Button("Watch"))
                {
                    watch();
                }
            }

            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();
        }
    }

}