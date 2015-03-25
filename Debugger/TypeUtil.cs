using System;

namespace ModTools
{

    public static class TypeUtil
    {

        public static bool IsBuiltInType(Type t)
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

        public static bool IsReflectableType(Type t)
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

        public static bool IsTextureType(Type t)
        {
            if (t == typeof(UnityEngine.Texture) || t == typeof(UnityEngine.Texture2D) ||
                t == typeof(UnityEngine.RenderTexture))
            {
                return true;
            }

            return false;
        }

        public static bool IsMeshType(Type t)
        {
            if (t == typeof(UnityEngine.Mesh))
            {
                return true;
            }

            return false;
        }

    }

}
