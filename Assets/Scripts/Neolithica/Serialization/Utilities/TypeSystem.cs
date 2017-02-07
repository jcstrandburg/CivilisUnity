//This is used to get teh element type of a collection
//source: http://stackoverflow.com/questions/1900353/how-to-get-the-type-contained-in-a-collection-through-reflection
//via http://blogs.msdn.com/b/mattwar/archive/2007/07/30/linq-building-an-iqueryable-provider-part-i.aspx
//The methods IsEnumerableType and IsCollectionType were added by myself (Cherno). Use them to find out wether a Type is a collection (array, List,...)

using System;
using System.Collections.Generic;

namespace Neolithica.Serialization.Utilities {
    /// <summary>
    /// Part of the SerializeHelper package by Cherno.
    /// http://forum.unity3d.com/threads/serializehelper-free-save-and-load-utility-de-serialize-all-objects-in-your-scene.338148/
    /// </summary>
    public static class TypeSystem {
        public static Type GetElementType(Type seqType) {
            Type ienum = FindIEnumerable(seqType);
            if (ienum == null) return seqType;
            return ienum.GetGenericArguments()[0];
        }
        private static Type FindIEnumerable(Type seqType) {
            if (seqType == null || seqType == typeof(string))
                return null;
            if (seqType.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            if (seqType.IsGenericType) {
                foreach (Type arg in seqType.GetGenericArguments()) {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType)) {
                        return ienum;
                    }
                }
            }
            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0) {
                foreach (Type iface in ifaces) {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null) return ienum;
                }
            }
            if (seqType.BaseType != null && seqType.BaseType != typeof(object)) {
                return FindIEnumerable(seqType.BaseType);
            }
            return null;
        }
	
        //is a type a collection?
        public static bool IsEnumerableType(Type type) {
            return (type.GetInterface("IEnumerable") != null);
        }
	
        public static bool IsCollectionType(Type type) {
            return (type.GetInterface("ICollection") != null);
        }
    }
}