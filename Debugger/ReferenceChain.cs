using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ModTools
{

    public class ReferenceChain
    {
        public enum ReferenceType
        {
            GameObject = 0,
            Component = 1,
            Field = 2,
            Property = 3,
            Method = 4,
            EnumerableItem = 5,
            SpecialNamedProperty = 6
        }

        public object[] chainObjects = new object[SceneExplorer.maxHierarchyDepth];
        public ReferenceType[] chainTypes = new ReferenceType[SceneExplorer.maxHierarchyDepth];
        public int count = 0;
        public int identOffset = 0;

        public int Ident
        {
            get { return count + identOffset - 1; }
        }

        public int Length
        {
            get { return count; }
        }

        public object LastItem
        {
            get { return chainObjects[count - 1]; }
        }

        public string LastItemName
        {
            get
            {
                return ItemToString(count - 1);
            }
        }

        public ReferenceType LastItemType
        {
            get { return chainTypes[count - 1]; }
        }

        public bool CheckDepth()
        {
            if (count >= SceneExplorer.maxHierarchyDepth)
            {
                return true;
            }

            return false;
        }

        public ReferenceChain Copy()
        {
            ReferenceChain copy = new ReferenceChain();
            copy.count = count;
            for (int i = 0; i < count; i++)
            {
                copy.chainObjects[i] = chainObjects[i];
                copy.chainTypes[i] = chainTypes[i];
            }

            copy.identOffset = identOffset;

            return copy;
        }

        public ReferenceChain Add(GameObject go)
        {
            ReferenceChain copy = Copy();
            copy.chainObjects[count] = go;
            copy.chainTypes[count] = ReferenceType.GameObject;
            copy.count++;
            return copy;
        }

        public ReferenceChain Add(Component component)
        {
            ReferenceChain copy = Copy();
            copy.chainObjects[count] = component;
            copy.chainTypes[count] = ReferenceType.Component;
            copy.count++;
            return copy;
        }

        public ReferenceChain Add(FieldInfo fieldInfo)
        {
            ReferenceChain copy = Copy();
            copy.chainObjects[count] = fieldInfo;
            copy.chainTypes[count] = ReferenceType.Field;
            copy.count++;
            return copy;
        }

        public ReferenceChain Add(PropertyInfo propertyInfo)
        {
            ReferenceChain copy = Copy();
            copy.chainObjects[count] = propertyInfo;
            copy.chainTypes[count] = ReferenceType.Property;
            copy.count++;
            return copy;
        }

        public ReferenceChain Add(MethodInfo methodInfo)
        {
            ReferenceChain copy = Copy();
            copy.chainObjects[count] = methodInfo;
            copy.chainTypes[count] = ReferenceType.Method;
            copy.count++;
            return copy;
        }

        public ReferenceChain Add(int index)
        {
            ReferenceChain copy = Copy();
            copy.chainObjects[count] = index;
            copy.chainTypes[count] = ReferenceType.EnumerableItem;
            copy.count++;
            return copy;
        }

        public ReferenceChain Add(string namedProperty)
        {
            ReferenceChain copy = Copy();
            copy.chainObjects[count] = namedProperty;
            copy.chainTypes[count] = ReferenceType.SpecialNamedProperty;
            copy.count++;
            return copy;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ReferenceChain))
            {
                return false;
            }

            var other = (ReferenceChain)obj;

            if (other.count != count)
            {
                return false;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (chainTypes[i] != other.chainTypes[i])
                {
                    return false;
                }

                if (chainObjects[i].GetHashCode() != other.chainObjects[i].GetHashCode())
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hash = HashCodeHelper.Initialize();

            for (int i = 0; i < count; i++)
            {
                hash = HashCodeHelper.Hash(hash, chainTypes[i]);
                hash = HashCodeHelper.Hash(hash, chainObjects[i]);
            }

            return hash;
        }

        private string ItemToString(int i)
        {
            switch (chainTypes[i])
            {
                case ReferenceType.GameObject:
                    return ((GameObject)chainObjects[i]).name;
                case ReferenceType.Component:
                    return ((Component)chainObjects[i]).name;
                case ReferenceType.Field:
                    return ((FieldInfo)chainObjects[i]).Name;
                case ReferenceType.Property:
                    return ((PropertyInfo)chainObjects[i]).Name;
                case ReferenceType.Method:
                    return ((MethodInfo)chainObjects[i]).Name;
                case ReferenceType.EnumerableItem:
                    return String.Format("[{0}]", (int)chainObjects[i]);
                case ReferenceType.SpecialNamedProperty:
                    return (string)chainObjects[i];
            }

            return "";
        }

        public override string ToString()
        {
            string result = "";

            for (int i = 0; i < count; i++)
            {
                result += ItemToString(i);

                if (i != count - 1)
                {
                    result += '.';
                }
            }

            return result;
        }

        public ReferenceChain Reverse
        {
            get
            {
                ReferenceChain chain = Copy();
                chain.chainObjects = chain.chainObjects.Reverse().ToArray();
                chain.chainTypes = chain.chainTypes.Reverse().ToArray();
                return chain;
            }
        }

    }

}
