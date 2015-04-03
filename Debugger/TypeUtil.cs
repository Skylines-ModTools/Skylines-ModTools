using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModTools
{

    public static class TypeUtil
    {

        public static bool IsBuiltInType(Type t)
        {
            if (t == typeof (System.Char)) return true;
            if (t == typeof (System.String)) return true;
            if (t == typeof (System.Boolean)) return true;
            if (t == typeof (System.Single)) return true;
            if (t == typeof (System.Double)) return true;
            if (t == typeof (System.Byte)) return true;
            if (t == typeof (System.Int32)) return true;
            if (t == typeof (System.UInt32)) return true;
            if (t == typeof (System.Int64)) return true;
            if (t == typeof (System.UInt64)) return true;
            if (t == typeof (System.Int16)) return true;
            if (t == typeof (System.UInt16)) return true;
            if (t == typeof (UnityEngine.Vector2)) return true;
            if (t == typeof (UnityEngine.Vector3)) return true;
            if (t == typeof (UnityEngine.Vector4)) return true;
            if (t == typeof (UnityEngine.Quaternion)) return true;
            if (t == typeof (UnityEngine.Color)) return true;
            if (t == typeof (UnityEngine.Color32)) return true;
            if (t.IsEnum) return true;
            return false;
        }

        public static bool IsBitmaskEnum(Type t)
        {
            return t.IsDefined(typeof(FlagsAttribute), false);
        }

        public static bool IsReflectableType(Type t)
        {
            if (IsBuiltInType(t))
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

        public static MemberInfo[] GetAllMembers(Type type, bool recursive = false)
        {
            return GetMembersInternal(type, recursive, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        }

        public static MemberInfo[] GetPublicMembers(Type type, bool recursive = false)
        {
            return GetMembersInternal(type, recursive, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
        }

        public static MemberInfo[] GetPrivateMembers(Type type, bool recursive = false)
        {
            return GetMembersInternal(type, recursive, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
        }

        public static void ClearTypeCache()
        {
            _typeCache = new Dictionary<Type, MemberInfo[]>();
        }

        private static Dictionary<Type, MemberInfo[]> _typeCache = new Dictionary<Type, MemberInfo[]>(); 

        private static MemberInfo[] GetMembersInternal(Type type, bool recursive, BindingFlags bindingFlags)
        {
            if (_typeCache.ContainsKey(type))
            {
                return _typeCache[type];
            }

            var results = new Dictionary<string, MemberInfo>();
            GetMembersInternal2(type, recursive, bindingFlags, results);
            var members = results.Values.ToArray();
            _typeCache[type] = members;
            return members;
        }

        private static void GetMembersInternal2(Type type, bool recursive, BindingFlags bindingFlags,
            Dictionary<string, MemberInfo> outResults)
        {
            var selfMembers = type.GetMembers(bindingFlags);
            foreach (var member in selfMembers)
            {
                if (!outResults.ContainsKey(member.Name))
                {
                    outResults.Add(member.Name, member);
                }
            }

            if (recursive)
            {
                if (type.BaseType != null)
                {
                    GetMembersInternal2(type.BaseType, true, bindingFlags, outResults);
                }
            }
        }

    }

}
