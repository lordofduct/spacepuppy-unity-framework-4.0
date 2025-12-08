using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace com.spacepuppy.Utils
{
    public static class TypeUtil
    {

        #region Static Fields

        private static readonly Assembly ASSEMB_UNITY = typeof(UnityEngine.Object).Assembly;
        private static readonly Assembly[] PREFERRED_ASSEMBLIES;

        #endregion

        #region Static Constructor

        static TypeUtil()
        {
            using(var lst = com.spacepuppy.Collections.TempCollection.GetList<Assembly>())
            {
                lst.Add(typeof(UnityEngine.Object).Assembly);

                foreach(var assemb in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if(assemb.FullName.StartsWith("com.spacepuppy."))
                    {
                        lst.Add(assemb);
                    }
                    else if(assemb.FullName == "Assembly-CSharp")
                    {
                        lst.Add(assemb);
                    }
                }

                PREFERRED_ASSEMBLIES = lst.ToArray();
            }
        }

        #endregion

        #region Hash

        public static string HashType(System.Type tp)
        {
            if (tp != null)
            {
                return tp.Assembly.GetName().Name + "|" + tp.FullName;
            }
            else
            {
                return null;
            }
        }

        public static System.Type UnHashType(string hash)
        {
            if (!string.IsNullOrEmpty(hash))
            {
                var arr = hash.Split('|');
                var tp = TypeUtil.ParseType(arr.Length > 0 ? arr[0] : string.Empty,
                                            arr.Length > 1 ? arr[1] : string.Empty);
                return tp;
            }
            else
            {
                return null;
            }
        }

        #endregion

        public static bool IsNullableType(System.Type tp)
        {
            var ntp = Nullable.GetUnderlyingType(tp);
            return ntp != null;
        }

        public static bool IsNullableType(System.Type tp, out System.Type ntp)
        {
            ntp = Nullable.GetUnderlyingType(tp);
            return ntp != null;
        }

        public static IEnumerable<Type> GetTypes()
        {
            foreach (var assemb in PREFERRED_ASSEMBLIES)
            {
                foreach (var tp in assemb.DefinedTypes)
                {
                    yield return tp;
                }
            }

            foreach (var assemb in AppDomain.CurrentDomain.GetAssemblies().Except(PREFERRED_ASSEMBLIES))
            {
                foreach (var tp in assemb.DefinedTypes)
                {
                    yield return tp;
                }
            }
        }

        public static IEnumerable<Type> GetTypes(System.Func<System.Type, bool> predicate)
        {
            foreach(var tp in GetTypes())
            {
                if (predicate?.Invoke(tp) ?? true) yield return tp;
            }
        }

        public static IEnumerable<Type> GetTypesAssignableFrom(System.Type rootType)
        {
            return GetTypes(t => IsType(t, rootType));
        }

        public static IEnumerable<Type> GetTypesAssignableFrom(System.Reflection.Assembly assemb, System.Type rootType)
        {
            foreach (var tp in assemb.DefinedTypes)
            {
                if (IsType(tp, rootType) && rootType != tp) yield return tp;
            }
        }

        public static bool IsType(System.Type tp, System.Type assignableType)
        {
            if (tp == null) return false;
            if (tp == assignableType) return true;

            System.Type ctp;
            if (assignableType.IsGenericTypeDefinition)
            {
                if(assignableType.IsInterface)
                {
                    ctp = tp.IsGenericType ? tp.GetGenericTypeDefinition() : tp;
                    if (ctp == assignableType) return true;

                    foreach (var itp in tp.GetInterfaces())
                    {
                        ctp = itp.IsGenericType ? itp.GetGenericTypeDefinition() : itp;
                        if (ctp == assignableType) return true;
                    }
                    return false;
                }
                else
                {
                    while (tp != null && tp != typeof(object))
                    {
                        ctp = tp.IsGenericType ? tp.GetGenericTypeDefinition() : tp;
                        if (ctp == assignableType) return true;
                        tp = tp.BaseType;
                    }
                    return false;
                }
            }
            else
            {
                return assignableType.IsAssignableFrom(tp);
            }
        }

        public static bool IsType(System.Type tp, params System.Type[] assignableTypes)
        {
            foreach (var otp in assignableTypes)
            {
                if (TypeUtil.IsType(tp, otp)) return true;
            }

            return false;
        }

        public static object GetDefaultValue(this System.Type tp)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");

            if (tp.IsValueType)
                return System.Activator.CreateInstance(tp);
            else
                return null;
        }


        public static System.Type ParseType(string assembName, string typeName)
        {
            var assemb = (from a in System.AppDomain.CurrentDomain.GetAssemblies()
                          where a.GetName().Name == assembName || a.FullName == assembName
                          select a).FirstOrDefault();
            if (assemb != null)
            {
                return (from t in assemb.GetTypes()
                        where t.FullName == typeName
                        select t).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public static System.Type FindType(string typeName, bool useFullName = false, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(typeName)) return null;

            bool isArray = typeName.EndsWith("[]");
            if (isArray)
                typeName = typeName.Substring(0, typeName.Length - 2);

            StringComparison e = (ignoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (useFullName)
            {
                foreach (var t in GetTypes())
                {
                    if (string.Equals(t.FullName, typeName, e))
                    {
                        if (isArray)
                            return t.MakeArrayType();
                        else
                            return t;
                    }
                }
            }
            else
            {
                foreach (var t in GetTypes())
                {
                    if (string.Equals(t.Name, typeName, e) || string.Equals(t.FullName, typeName, e))
                    {
                        if (isArray)
                            return t.MakeArrayType();
                        else
                            return t;
                    }
                }
            }
            return null;
        }

        public static System.Type FindType(string typeName, System.Type baseType, bool useFullName = false, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(typeName)) return null;
            if (baseType == null) throw new System.ArgumentNullException("baseType");

            bool isArray = typeName.EndsWith("[]");
            if (isArray)
                typeName = typeName.Substring(0, typeName.Length - 2);

            StringComparison e = (ignoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if(useFullName)
            {
                foreach (var t in GetTypes())
                {
                    if (baseType.IsAssignableFrom(t) && string.Equals(t.FullName, typeName, e))
                    {
                        if (isArray)
                            return t.MakeArrayType();
                        else
                            return t;
                    }
                }
            }
            else
            {
                foreach (var t in GetTypes())
                {
                    if (baseType.IsAssignableFrom(t) && (string.Equals(t.Name, typeName, e) || string.Equals(t.FullName, typeName, e)))
                    {
                        if (isArray)
                            return t.MakeArrayType();
                        else
                            return t;
                    }
                }
            }

            return null;
        }

        public static bool IsListType(this System.Type tp)
        {
            if (tp == null) return false;

            if (tp.IsArray) return tp.GetArrayRank() == 1;

            var interfaces = tp.GetInterfaces();
            //if (interfaces.Contains(typeof(System.Collections.IList)) || interfaces.Contains(typeof(IList<>)))
            if (Array.IndexOf(interfaces, typeof(System.Collections.IList)) >= 0 || Array.IndexOf(interfaces, typeof(IList<>)) >= 0)
            {
                return true;
            }

            return false;
        }

        public static bool IsListType(this System.Type tp, bool ignoreAsInterface)
        {
            if (tp == null) return false;

            if (tp.IsArray) return tp.GetArrayRank() == 1;

            if (ignoreAsInterface)
            {
                //if (tp == typeof(System.Collections.ArrayList) || (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(List<>))) return true;
                if (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(List<>)) return true;
            }
            else
            {
                var interfaces = tp.GetInterfaces();
                //if (interfaces.Contains(typeof(System.Collections.IList)) || interfaces.Contains(typeof(IList<>)))
                if (Array.IndexOf(interfaces, typeof(System.Collections.IList)) >= 0 || Array.IndexOf(interfaces, typeof(IList<>)) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsListType(this System.Type tp, out System.Type innerType)
        {
            innerType = null;
            if (tp == null) return false;

            if (tp.IsArray)
            {
                if (tp.GetArrayRank() == 1)
                {
                    innerType = tp.GetElementType();
                    return true;
                }
                else
                    return false;
            }

            var interfaces = tp.GetInterfaces();
            if (Array.IndexOf(interfaces, typeof(System.Collections.IList)) >= 0 || Array.IndexOf(interfaces, typeof(IList<>)) >= 0)
            {
                if (tp.IsGenericType)
                {
                    innerType = tp.GetGenericArguments()[0];
                }
                else
                {
                    innerType = typeof(object);
                }
                return true;
            }

            return false;
        }

        public static bool IsListType(this System.Type tp, bool ignoreAsInterface, out System.Type innerType)
        {
            innerType = null;
            if (tp == null) return false;

            if (tp.IsArray)
            {
                if (tp.GetArrayRank() == 1)
                {
                    innerType = tp.GetElementType();
                    return true;
                }
                else
                    return false;
            }

            if (ignoreAsInterface)
            {
                if (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(List<>))
                {
                    innerType = tp.GetGenericArguments()[0];
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                var interfaces = tp.GetInterfaces();
                if (Array.IndexOf(interfaces, typeof(System.Collections.IList)) >= 0 || Array.IndexOf(interfaces, typeof(IList<>)) >= 0)
                {
                    if (tp.IsGenericType)
                    {
                        innerType = tp.GetGenericArguments()[0];
                    }
                    else
                    {
                        innerType = typeof(object);
                    }
                    return true;
                }
            }

            return false;
        }

        public static System.Type GetElementTypeOfListType(this System.Type tp)
        {
            if (tp == null) return null;

            if (tp.IsArray) return tp.GetElementType();

            var interfaces = tp.GetInterfaces();
            //if (interfaces.Contains(typeof(System.Collections.IList)) || interfaces.Contains(typeof(IList<>)))
            if (Array.IndexOf(interfaces, typeof(System.Collections.ICollection)) >= 0 || Array.IndexOf(interfaces, typeof(ICollection<>)) >= 0)
            {
                if (tp.IsGenericType) return tp.GetGenericArguments()[0];
                else return typeof(object);
            }
            else if (Array.IndexOf(interfaces, typeof(System.Collections.IEnumerable)) >= 0 || Array.IndexOf(interfaces, typeof(IEnumerable<>)) >= 0)
            {
                if (tp.IsGenericType) return tp.GetGenericArguments()[0];
                else return typeof(object);
            }

            return null;
        }



        private static System.Type _obsoleteAttribType = typeof(System.ObsoleteAttribute);
        public static bool IsObsolete(this System.Reflection.MemberInfo member)
        {
            return Attribute.IsDefined(member, _obsoleteAttribType);
        }

    }
}
