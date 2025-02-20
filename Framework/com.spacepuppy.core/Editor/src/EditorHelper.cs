using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor
{

    [InitializeOnLoad]
    public static class EditorHelper
    {

        public const string PROP_SCRIPT = "m_Script";
        public const string PROP_ORDER = "_order";
        public const string PROP_ACTIVATEON = "_activateOn";
        public const string PROP_SERVICEREGISTRATIONOPTS = "_serviceRegistrationOptions";


        public const float OBJFIELD_DOT_WIDTH = 18f;


        private static Texture2D s_WhiteTexture;
        public static Texture2D WhiteTexture
        {
            get
            {
                if (s_WhiteTexture == null)
                {
                    s_WhiteTexture = new Texture2D(1, 1);
                    s_WhiteTexture.SetPixel(0, 0, Color.white);
                    s_WhiteTexture.Apply();
                }
                return s_WhiteTexture;
            }
        }
        private static GUIStyle s_WhiteTextureStyle;
        public static GUIStyle WhiteTextureStyle
        {
            get
            {
                if (s_WhiteTextureStyle == null)
                {
                    s_WhiteTextureStyle = new GUIStyle();
                    s_WhiteTextureStyle.normal.background = EditorHelper.WhiteTexture;
                }
                return s_WhiteTextureStyle;
            }
        }


        static EditorHelper()
        {
            _mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            EditorApplication.update += EditorUpdate;

            //SceneView.onSceneGUIDelegate -= OnSceneGUI;
            //SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        #region Asserts

        public static void MalformedProperty(Rect position)
        {
            EditorGUI.LabelField(position, "Malformed serialized property.");
        }

        public static void MalformedProperty(Rect position, GUIContent label)
        {
            EditorGUI.LabelField(position, label, TempContent("Malformed serialized property."));
        }

        public static bool AssertMultiObjectEditingNotSupportedHeight(SerializedProperty property, GUIContent label, out float height)
        {
            if (property.serializedObject.isEditingMultipleObjects)
            {
                height = EditorGUIUtility.singleLineHeight;
                return true;
            }

            height = 0f;
            return false;
        }

        public static bool AssertMultiObjectEditingNotSupported(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.LabelField(position, label, EditorHelper.TempContent("Multi-Object editing is not supported."));
                return true;
            }

            return false;
        }
        public static bool AssertMultiObjectEditingNotSupported(SerializedObject serializedObject)
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.LabelField(EditorHelper.TempContent("Multi-Object editing is not supported."));
                return true;
            }

            return false;
        }
        public static bool AssertMultiObjectEditingNotSupported(SerializedObject serializedObject, GUIContent label)
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.LabelField(label, EditorHelper.TempContent("Multi-Object editing is not supported."));
                return true;
            }

            return false;
        }

        #endregion

        #region Dirty Helpers

        public static void CommitDirectChanges(this SerializedProperty property, bool usedUndo) => CommitDirectChanges(property.serializedObject, usedUndo);
        public static void CommitDirectChanges(this SerializedObject serializedObject, bool usedUndo)
        {
            foreach (var targ in serializedObject.targetObjects)
            {
                CommitDirectChanges(targ, usedUndo);
            }
        }

        public static void CommitDirectChanges(UnityEngine.Object targ, bool usedUndo)
        {
            if (GameObjectUtil.IsGameObjectSource(targ))
            {
                if (PrefabUtility.IsPartOfAnyPrefab(targ))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(targ);
                }
                else if (!usedUndo)
                {
                    EditorUtility.SetDirty(targ);
                }
            }
            else
            {
                EditorUtility.SetDirty(targ);
            }
        }

        #endregion

        #region SerializedProperty Helpers

        public static SerializedPropertyChangeCheckToken BeginChangeCheck(this SerializedProperty property) => new SerializedPropertyChangeCheckToken(property);

        public static bool TryFindPropertyRelative(this SerializedProperty property, string relativePropertyPath, out SerializedProperty result)
        {
            result = property.FindPropertyRelative(relativePropertyPath);
            return result != null;
        }

        public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty property)
        {
            property = property.Copy();
            var nextElement = property.Copy();
            bool hasNextElement = nextElement.NextVisible(false);
            if (!hasNextElement)
            {
                nextElement = null;
            }

            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if ((SerializedProperty.EqualContents(property, nextElement))) yield break;
                yield return property;
            }
        }

        public static System.Type GetManagedReferenceType(this SerializedProperty property)
        {
            var sfull = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(sfull)) return null;

            var arr = sfull.Split(' ');
            if (arr.Length != 2) return null;

            return System.Type.GetType(string.Format("{0}, {1}", arr[1], arr[0]));
        }

        public static System.Type GetManagedReferenceFieldType(this SerializedProperty property)
        {
            var sfull = property.managedReferenceFieldTypename;
            if (string.IsNullOrEmpty(sfull)) return null;

            var arr = sfull.Split(' ');
            if (arr.Length != 2) return null;

            return System.Type.GetType(string.Format("{0}, {1}", arr[1], arr[0]));
        }

        public static System.Type GetTargetType(this SerializedObject obj)
        {
            if (obj == null) return null;

            if (obj.isEditingMultipleObjects)
            {
                var c = obj.targetObjects[0];
                return c.GetType();
            }
            else
            {
                return obj.targetObject.GetType();
            }
        }

        public static System.Type GetTargetType(this SerializedProperty prop)
        {
            if (prop == null) return null;

            System.Reflection.FieldInfo field;
            System.Type fieldType;
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Generic:
                    {
                        field = GetFieldOfProperty(prop, out fieldType);
                        if (fieldType != null) return fieldType;
                        return typeof(object);
                        //return TypeUtil.FindType(prop.type) ?? typeof(object); //NOTE - prop.type is unreliable
                    }
                case SerializedPropertyType.Integer:
                    return prop.type == "long" ? typeof(long) : typeof(int);
                case SerializedPropertyType.Boolean:
                    return typeof(bool);
                case SerializedPropertyType.Float:
                    return prop.type == "double" ? typeof(double) : typeof(float);
                case SerializedPropertyType.String:
                    return typeof(string);
                case SerializedPropertyType.Color:
                    {
                        field = GetFieldOfProperty(prop, out fieldType);
                        if (fieldType != null) return fieldType;
                        return typeof(Color);
                        //return TypeUtil.FindType(prop.type) ?? typeof(Color); //NOTE - prop.type is unreliable
                    }
                case SerializedPropertyType.ObjectReference:
                    {
                        field = GetFieldOfProperty(prop, out fieldType);
                        if (fieldType != null) return fieldType;
                        return typeof(UnityEngine.Object);
                        //return TypeUtil.FindType(prop.type) ?? typeof(UnityEngine.Object); //NOTE - prop.type is unreliable
                    }
                case SerializedPropertyType.LayerMask:
                    return typeof(LayerMask);
                case SerializedPropertyType.Enum:
                    {
                        field = GetFieldOfProperty(prop, out fieldType);
                        if (fieldType != null) return fieldType;
                        return typeof(System.Enum);
                        //return TypeUtil.FindType(prop.type) ?? typeof(System.Enum); //NOTE - prop.type is unreliable
                    }
                case SerializedPropertyType.Vector2:
                    return typeof(Vector2);
                case SerializedPropertyType.Vector3:
                    return typeof(Vector3);
                case SerializedPropertyType.Vector4:
                    return typeof(Vector4);
                case SerializedPropertyType.Rect:
                    return typeof(Rect);
                case SerializedPropertyType.ArraySize:
                    return typeof(int);
                case SerializedPropertyType.Character:
                    return typeof(char);
                case SerializedPropertyType.AnimationCurve:
                    return typeof(AnimationCurve);
                case SerializedPropertyType.Bounds:
                    return typeof(Bounds);
                case SerializedPropertyType.Gradient:
                    return typeof(Gradient);
                case SerializedPropertyType.Quaternion:
                    return typeof(Quaternion);
                case SerializedPropertyType.ExposedReference:
                    {
                        field = GetFieldOfProperty(prop, out fieldType);
                        if (fieldType != null) return fieldType;
                        return typeof(UnityEngine.Object);
                        //return TypeUtil.FindType(prop.type) ?? typeof(UnityEngine.Object); //NOTE - prop.type is unreliable
                    }
                case SerializedPropertyType.FixedBufferSize:
                    return typeof(int);
                case SerializedPropertyType.Vector2Int:
                    return typeof(Vector2Int);
                case SerializedPropertyType.Vector3Int:
                    return typeof(Vector3Int);
                case SerializedPropertyType.RectInt:
                    return typeof(RectInt);
                case SerializedPropertyType.BoundsInt:
                    return typeof(BoundsInt);
                default:
                    {
                        field = GetFieldOfProperty(prop, out fieldType);
                        if (fieldType != null) return fieldType;
                        return typeof(object);
                        //return TypeUtil.FindType(prop.type) ?? typeof(object); //NOTE - prop.type is unreliable
                    }
            }
        }

        /// <summary>
        /// Gets the object the property represents.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null) return null;

            System.Reflection.FieldInfo field;
            System.Type fieldType;
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Generic:
                    //must be walked
                    break;
#if UNITY_2021_2_OR_NEWER
                case SerializedPropertyType.ManagedReference:
                    return prop.managedReferenceValue;
#endif
#if UNITY_2022_1_OR_NEWER
                default:
                    return prop.boxedValue;
#else
                case SerializedPropertyType.Integer:
                    return prop.type == "long" ? prop.longValue : prop.intValue;
                case SerializedPropertyType.Boolean:
                    return typeof(bool);
                case SerializedPropertyType.Float:
                    return prop.type == "double" ? prop.doubleValue : prop.floatValue;
                case SerializedPropertyType.String:
                    return prop.stringValue;
                case SerializedPropertyType.Color:
                    return prop.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return (LayerMask)prop.intValue;
                case SerializedPropertyType.Enum:
                    {
                        field = GetFieldOfProperty(prop, out fieldType);
                        if (fieldType != null && fieldType.IsEnum) return ConvertUtil.ToEnumOfType(fieldType, prop.intValue);
                        return prop.intValue;
                    }
                case SerializedPropertyType.Vector2:
                    return prop.vector2Value;
                case SerializedPropertyType.Vector3:
                    return prop.vector3Value;
                case SerializedPropertyType.Vector4:
                    return prop.vector4Value;
                case SerializedPropertyType.Rect:
                    return prop.rectValue;
                case SerializedPropertyType.ArraySize:
                    return prop.arraySize;
                case SerializedPropertyType.Character:
                    return (char)prop.intValue;
                case SerializedPropertyType.AnimationCurve:
                    return prop.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return prop.boundsValue;
#if UNITY_2022_1_OR_NEWER
                case SerializedPropertyType.Gradient:
                    return prop.gradientValue;
#endif
                case SerializedPropertyType.Quaternion:
                    return prop.quaternionValue;
                case SerializedPropertyType.ExposedReference:
                    return prop.exposedReferenceValue;
                case SerializedPropertyType.FixedBufferSize:
                    return prop.fixedBufferSize;
                case SerializedPropertyType.Vector2Int:
                    return prop.vector2IntValue;
                case SerializedPropertyType.Vector3Int:
                    return prop.vector3IntValue;
                case SerializedPropertyType.RectInt:
                    return prop.rectIntValue;
                case SerializedPropertyType.BoundsInt:
                    return prop.boundsIntValue;
#endif
            }

            object obj = prop.serializedObject.targetObject;
            try
            {
                foreach (var (p, fi, ftp) in WalkPropertyPath(prop))
                {
                    if (fi == null)
                    {
                        //this is an array element
                        int index = p.propertyPath.LastIndexOf('[') + 1;
                        index = int.Parse(p.propertyPath.Substring(index, p.propertyPath.Length - index - 1));
                        obj = ((System.Collections.IList)obj)[index];
                    }
                    else
                    {
                        obj = fi.GetValue(obj);
                    }
                }
            }
            catch
            {
                //fatal error, quit now
                return null;
            }
            return obj;
        }

        public static void SetTargetObjectOfProperty(SerializedProperty prop, object value)
        {
            if (prop == null) return;

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Generic:
                    //must be walked
                    break;
#if UNITY_2021_2_OR_NEWER
                case SerializedPropertyType.ManagedReference:
                    prop.managedReferenceValue = value;
                    return;
#endif
#if UNITY_2022_1_OR_NEWER
                default:
                    prop.boxedValue = value;
                    return;
#else
                    //TODO - do we want to support pre-2022 here the same we boxedValue does? It's 2025 now and honestly 2021 support is minimal, the below logic should suffice if slow
#endif
            }

            object obj = prop.serializedObject.targetObject;
            try
            {
                var arr = WalkPropertyPath(prop).ToArray();
                if (arr.Length == 0) return; //how'd this happen?

                for (int i = 0; i < arr.Length - 1; i++)
                {
                    var (p, fi, ftp) = arr[i];

                    if (fi == null)
                    {
                        //this is an array element
                        int index = p.propertyPath.LastIndexOf('[') + 1;
                        index = int.Parse(p.propertyPath.Substring(index, p.propertyPath.Length - index - 1));
                        obj = ((System.Collections.IList)obj)[index];
                    }
                    else
                    {
                        obj = fi.GetValue(obj);
                    }
                }

                {
                    var (p, fi, ftp) = arr[arr.Length - 1];
                    if (fi == null)
                    {
                        //this is an array element
                        int index = p.propertyPath.LastIndexOf('[') + 1;
                        index = int.Parse(p.propertyPath.Substring(index, p.propertyPath.Length - index - 1));
                        ((System.Collections.IList)obj)[index] = ConvertUtil.Coerce(value, ftp);
                    }
                    else
                    {
                        fi.SetValue(obj, ConvertUtil.Coerce(value, ftp));
                    }
                }
            }
            catch
            {
                //fatal error, quit now
                return;
            }
        }

        /// <summary>
        /// Gets the object that the property is a member of
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        [System.Obsolete("Use GetParentProperty instead and get the targetobject from that. eg: GetTargetObjectOfProperty(GetParentProperty(prop))")]
        public static object GetTargetObjectWithProperty(SerializedProperty prop)
        {
            if (prop == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        [System.Obsolete("Use GetParentProperty instead and get the targetobject from that. eg: GetTargetObjectOfProperty(GetParentProperty(prop))")]
        public static object[] GetTargetObjectsWithProperty(SerializedProperty prop)
        {
            if (prop == null) return null;

            object[] arr = prop.serializedObject.targetObjects;
            if (arr.Length == 0) return arr;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = GetValue_Imp(arr[i], elementName, index);
                    }
                }
                else
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = GetValue_Imp(arr[i], element);
                    }
                }
            }
            return arr;
        }

        [System.Obsolete("Obsoleted with GetTargetObjectWithProperty/GetTargetObjectsWithProperty")]
        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        [System.Obsolete("Obsoleted with GetTargetObjectWithProperty/GetTargetObjectsWithProperty")]
        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }


        public static void SetEnumValue<T>(this SerializedProperty prop, T value) where T : struct
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (prop.propertyType != SerializedPropertyType.Enum) throw new System.ArgumentException("SerializedProperty is not an enum type.", "prop");

            //var tp = typeof(T);
            //if(tp.IsEnum)
            //{
            //    prop.enumValueIndex = prop.enumNames.IndexOf(System.Enum.GetName(tp, value));
            //}
            //else
            //{
            //    int i = ConvertUtil.ToInt(value);
            //    if (i < 0 || i >= prop.enumNames.Length) i = 0;
            //    prop.enumValueIndex = i;
            //}
            prop.intValue = ConvertUtil.ToInt(value);
        }

        public static void SetEnumValue(this SerializedProperty prop, System.Enum value)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (prop.propertyType != SerializedPropertyType.Enum) throw new System.ArgumentException("SerializedProperty is not an enum type.", "prop");

            if (value == null)
            {
                prop.enumValueIndex = 0;
                return;
            }

            //int i = prop.enumNames.IndexOf(System.Enum.GetName(value.GetType(), value));
            //if (i < 0) i = 0;
            //prop.enumValueIndex = i;
            prop.intValue = ConvertUtil.ToInt(value);
        }

        public static void SetEnumValue(this SerializedProperty prop, object value)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (prop.propertyType != SerializedPropertyType.Enum) throw new System.ArgumentException("SerializedProperty is not an enum type.", "prop");

            if (value == null)
            {
                prop.enumValueIndex = 0;
                return;
            }

            //var tp = value.GetType();
            //if (tp.IsEnum)
            //{
            //    int i = prop.enumNames.IndexOf(System.Enum.GetName(tp, value));
            //    if (i < 0) i = 0;
            //    prop.enumValueIndex = i;
            //}
            //else
            //{
            //    int i = ConvertUtil.ToInt(value);
            //    if (i < 0 || i >= prop.enumNames.Length) i = 0;
            //    prop.enumValueIndex = i;
            //}
            prop.intValue = ConvertUtil.ToInt(value);
        }

#if UNITY_2021_3_OR_NEWER
        public static T GetEnumValue<T>(this SerializedProperty prop) where T : struct, System.Enum
#else
        public static T GetEnumValue<T>(this SerializedProperty prop) where T : struct, System.IConvertible
#endif
        {
            if (prop == null) throw new System.ArgumentNullException("prop");

            try
            {
                //var name = prop.enumNames[prop.enumValueIndex];
                //return ConvertUtil.ToEnum<T>(name);
                return ConvertUtil.ToEnum<T>(prop.intValue);
            }
            catch
            {
                return default(T);
            }
        }

        public static System.Enum GetEnumValue(this SerializedProperty prop, System.Type tp)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (tp == null) throw new System.ArgumentNullException("tp");
            if (!tp.IsEnum) throw new System.ArgumentException("Type must be an enumerated type.");

            try
            {
                //var name = prop.enumNames[prop.enumValueIndex];
                //return System.Enum.Parse(tp, name) as System.Enum;
                return ConvertUtil.ToEnumOfType(tp, prop.intValue);
            }
            catch
            {
                return System.Enum.GetValues(tp).Cast<System.Enum>().First();
            }
        }

        public static void SetPropertyValue(this SerializedProperty prop, object value, bool ignoreSpecialWrappers = false)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    prop.intValue = ConvertUtil.ToInt(value);
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = ConvertUtil.ToBool(value);
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = ConvertUtil.ToSingle(value);
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = ConvertUtil.ToString(value);
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = ConvertUtil.ToColor(value);
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = value as UnityEngine.Object;
                    break;
                case SerializedPropertyType.LayerMask:
                    prop.intValue = (value is LayerMask) ? ((LayerMask)value).value : ConvertUtil.ToInt(value);
                    break;
                case SerializedPropertyType.Enum:
                    //prop.enumValueIndex = ConvertUtil.ToInt(value);
                    prop.SetEnumValue(value);
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = ConvertUtil.ToVector2(value);
                    break;
                case SerializedPropertyType.Vector3:
                    prop.vector3Value = ConvertUtil.ToVector3(value);
                    break;
                case SerializedPropertyType.Vector4:
                    prop.vector4Value = ConvertUtil.ToVector4(value);
                    break;
                case SerializedPropertyType.Rect:
                    prop.rectValue = (Rect)value;
                    break;
                case SerializedPropertyType.ArraySize:
                    prop.arraySize = ConvertUtil.ToInt(value);
                    break;
                case SerializedPropertyType.Character:
                    prop.intValue = ConvertUtil.ToInt(value);
                    break;
                case SerializedPropertyType.AnimationCurve:
                    prop.animationCurveValue = value as AnimationCurve;
                    break;
                case SerializedPropertyType.Bounds:
                    prop.boundsValue = (Bounds)value;
                    break;
                case SerializedPropertyType.Generic:
                    {
                        if (!ignoreSpecialWrappers)
                        {
                            if (com.spacepuppyeditor.Internal.ScriptAttributeUtility.TryGetInternalPropertyDrawer(prop, out PropertyDrawer drawer) && drawer is ISerializedWrapperHelper ish)
                            {
                                if (ish.SetValue(prop, value)) return;
                            }

                            SetTargetObjectOfProperty(prop, value);
                        }
                    }
                    break;
            }
        }

        public static object GetPropertyValue(this SerializedProperty prop, bool ignoreSpecialWrappers = false)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return prop.intValue;
                case SerializedPropertyType.Boolean:
                    return prop.boolValue;
                case SerializedPropertyType.Float:
                    return prop.floatValue;
                case SerializedPropertyType.String:
                    return prop.stringValue;
                case SerializedPropertyType.Color:
                    return prop.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return (LayerMask)prop.intValue;
                case SerializedPropertyType.Enum:
                    return prop.intValue;
                case SerializedPropertyType.Vector2:
                    return prop.vector2Value;
                case SerializedPropertyType.Vector3:
                    return prop.vector3Value;
                case SerializedPropertyType.Vector4:
                    return prop.vector4Value;
                case SerializedPropertyType.Rect:
                    return prop.rectValue;
                case SerializedPropertyType.ArraySize:
                    return prop.arraySize;
                case SerializedPropertyType.Character:
                    return (char)prop.intValue;
                case SerializedPropertyType.AnimationCurve:
                    return prop.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return prop.boundsValue;
                case SerializedPropertyType.Generic:
#if UNITY_2021_2_OR_NEWER
                case SerializedPropertyType.ManagedReference:
#endif
                    {
                        if (!ignoreSpecialWrappers)
                        {
                            if (com.spacepuppyeditor.Internal.ScriptAttributeUtility.TryGetInternalPropertyDrawer(prop, out PropertyDrawer drawer) && drawer is ISerializedWrapperHelper ish)
                            {
                                return ish.GetValue(prop);
                            }
                        }

                        return GetTargetObjectOfProperty(prop);
                    }
            }

            return null;
        }

        public static T GetPropertyValue<T>(this SerializedProperty prop, bool ignoreSpecialWrappers = false)
        {
            var obj = GetPropertyValue(prop, ignoreSpecialWrappers);
            if (obj is T) return (T)obj;

            var tp = typeof(T);
            try
            {
                return (T)System.Convert.ChangeType(obj, tp);
            }
            catch (System.Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Returns the type that the SerializedProperty is interpreted as when calling SetPropertyValue or GetPropertyValue.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="ignoreSpecialWrappers"></param>
        /// <returns></returns>
        public static System.Type GetPropertyValueType(this SerializedProperty prop, bool ignoreSpecialWrappers = false)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");

            var fieldType = prop.GetTargetType();
            if (fieldType != null && !ignoreSpecialWrappers)
            {
                if (com.spacepuppyeditor.Internal.ScriptAttributeUtility.TryGetInternalPropertyDrawer(prop, out PropertyDrawer drawer) && drawer is ISerializedWrapperHelper ish)
                {
                    fieldType = ish.GetValueType(prop);
                }
            }

            return fieldType ?? typeof(object);
        }

        public static SerializedPropertyType GetPropertyType(System.Type tp)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");

            if (tp.IsEnum) return SerializedPropertyType.Enum;

            var code = System.Type.GetTypeCode(tp);
            switch (code)
            {
                case System.TypeCode.SByte:
                case System.TypeCode.Byte:
                case System.TypeCode.Int16:
                case System.TypeCode.UInt16:
                case System.TypeCode.Int32:
                case System.TypeCode.UInt32:
                case System.TypeCode.Int64:
                case System.TypeCode.UInt64:
                    return SerializedPropertyType.Integer;
                case System.TypeCode.Boolean:
                    return SerializedPropertyType.Boolean;
                case System.TypeCode.Single:
                case System.TypeCode.Double:
                case System.TypeCode.Decimal:
                    return SerializedPropertyType.Float;
                case System.TypeCode.String:
                    return SerializedPropertyType.String;
                case System.TypeCode.Char:
                    return SerializedPropertyType.Character;
                case System.TypeCode.DateTime:
                    return SerializedPropertyType.Generic;
                default:
                    {
                        if (TypeUtil.IsType(tp, typeof(Color)))
                            return SerializedPropertyType.Color;
                        else if (TypeUtil.IsType(tp, typeof(UnityEngine.Object)))
                            return SerializedPropertyType.ObjectReference;
                        else if (TypeUtil.IsType(tp, typeof(LayerMask)))
                            return SerializedPropertyType.LayerMask;
                        else if (TypeUtil.IsType(tp, typeof(Vector2)))
                            return SerializedPropertyType.Vector2;
                        else if (TypeUtil.IsType(tp, typeof(Vector3)))
                            return SerializedPropertyType.Vector3;
                        else if (TypeUtil.IsType(tp, typeof(Vector4)))
                            return SerializedPropertyType.Vector4;
                        else if (TypeUtil.IsType(tp, typeof(Quaternion)))
                            return SerializedPropertyType.Quaternion;
                        else if (TypeUtil.IsType(tp, typeof(Rect)))
                            return SerializedPropertyType.Rect;
                        else if (TypeUtil.IsType(tp, typeof(AnimationCurve)))
                            return SerializedPropertyType.AnimationCurve;
                        else if (TypeUtil.IsType(tp, typeof(Bounds)))
                            return SerializedPropertyType.Bounds;
                        else if (TypeUtil.IsType(tp, typeof(Gradient)))
                            return SerializedPropertyType.Gradient;
                        else if (TypeUtil.IsType(tp, typeof(System.TimeSpan)))
                            return SerializedPropertyType.Generic;
                    }
                    return SerializedPropertyType.Generic;

            }
        }

        public static System.TypeCode GetPropertyTypeCode(this SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return prop.type == "long" ? System.TypeCode.Int64 : System.TypeCode.Int32;
                case SerializedPropertyType.Boolean:
                    return System.TypeCode.Boolean;
                case SerializedPropertyType.Float:
                    return prop.type == "double" ? System.TypeCode.Double : System.TypeCode.Single;
                case SerializedPropertyType.String:
                    return System.TypeCode.String;
                case SerializedPropertyType.LayerMask:
                    return System.TypeCode.Int32;
                case SerializedPropertyType.Enum:
                    return System.TypeCode.Int32;
                case SerializedPropertyType.ArraySize:
                    return System.TypeCode.Int32;
                case SerializedPropertyType.Character:
                    return System.TypeCode.Char;
                default:
                    return System.TypeCode.Object;
            }
        }

        public static double GetNumericValue(this SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return (double)prop.intValue;
                case SerializedPropertyType.Boolean:
                    return prop.boolValue ? 1d : 0d;
                case SerializedPropertyType.Float:
                    return prop.type == "double" ? prop.doubleValue : (double)prop.floatValue;
                case SerializedPropertyType.ArraySize:
                    return (double)prop.arraySize;
                case SerializedPropertyType.Character:
                    return (double)prop.intValue;
                default:
                    return 0d;
            }
        }

        public static void SetNumericValue(this SerializedProperty prop, double value)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    prop.intValue = (int)value;
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = (System.Math.Abs(value) > MathUtil.DBL_EPSILON);
                    break;
                case SerializedPropertyType.Float:
                    if (prop.type == "double")
                        prop.doubleValue = value;
                    else
                        prop.floatValue = (float)value;
                    break;
                case SerializedPropertyType.ArraySize:
                    prop.arraySize = (int)value;
                    break;
                case SerializedPropertyType.Character:
                    prop.intValue = (int)value;
                    break;
            }
        }

        public static bool IsNumericValue(this SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.Character:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsVectorValue(this SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3Int:
                    return true;
                default:
                    return false;
            }
        }

        public static IEnumerable<SerializedProperty> EnumerateArray(this SerializedProperty prop)
        {
            if (!prop.isArray) yield break;

            for (int i = 0; i < prop.arraySize; i++)
            {
                yield return prop.GetArrayElementAtIndex(i);
            }
        }

        public static T[] GetAsArray<T>(this SerializedProperty prop)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (!prop.isArray) throw new System.ArgumentException("SerializedProperty does not represent an Array.", "prop");

            var arr = new T[prop.arraySize];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = GetPropertyValue<T>(prop.GetArrayElementAtIndex(i));
            }
            return arr;
        }

        public static void SetAsArray<T>(this SerializedProperty prop, T[] arr)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (!prop.isArray) throw new System.ArgumentException("SerializedProperty does not represent an Array.", "prop");

            int sz = arr != null ? arr.Length : 0;
            prop.arraySize = sz;
            for (int i = 0; i < sz; i++)
            {
                prop.GetArrayElementAtIndex(i).SetPropertyValue(arr[i]);
            }
        }



        public static int GetChildPropertyCount(SerializedProperty property, bool includeGrandChildren = false)
        {
            var pstart = property.Copy();
            var pend = property.GetEndProperty();
            int cnt = 0;

            pstart.Next(true);
            while (!SerializedProperty.EqualContents(pstart, pend))
            {
                cnt++;
                pstart.Next(includeGrandChildren);
            }

            return cnt;
        }

        #endregion

        #region Serialized Field Helpers

        public static SerializedProperty GetParentProperty(SerializedProperty prop)
        {
            if (prop == null) return null;

            var elements = prop.propertyPath.Split('.');
            if (elements.Length <= 1) return null; //we're at the base of the serializedobject, there is no parent.

            if (elements[elements.Length - 1].StartsWith("data["))
            {
                //we're an array element, go back 2
                return prop.serializedObject.FindProperty(string.Join('.', elements.Take(elements.Length - 2)));
            }
            else
            {
                return prop.serializedObject.FindProperty(string.Join('.', elements.Take(elements.Length - 1)));
            }
        }

        public static System.Reflection.FieldInfo GetFieldOfProperty(SerializedProperty prop) => GetFieldOfProperty(prop, out _);
        public static System.Reflection.FieldInfo GetFieldOfProperty(SerializedProperty prop, out System.Type fieldType) //NOTE - this exists because we can't actually return the FieldInfo of an array element, this way a caller who really wants the fieldtype can just access that.
        {
            fieldType = null;
            if (prop == null) return null;

            var roottype = GetTargetType(prop.serializedObject);
            if (roottype == null) return null;

            if (!prop.propertyPath.Contains('.'))
            {
                //we're at the root of the serializedObject, just look at it
                var result = roottype != null ? DynamicUtil.GetMemberFromType(roottype, prop.propertyPath, true, MemberTypes.Field) as System.Reflection.FieldInfo : null;
                fieldType = result?.FieldType;
                return result;
            }

            SerializedProperty parent;
#if UNITY_2021_2_OR_NEWER
            parent = GetParentProperty(prop);
            if (parent == null) return null; //this shouldn't happen since previous check, but here just in case

            switch (parent.propertyType)
            {
                case SerializedPropertyType.ManagedReference:
                    {
                        fieldType = parent.GetManagedReferenceType() ?? parent.GetManagedReferenceFieldType();
                        var result = DynamicUtil.GetMemberFromType(fieldType, prop.name, true, MemberTypes.Field) as System.Reflection.FieldInfo;
                        fieldType = result?.FieldType;
                        return result;
                    }
                default:
#if UNITY_2022_1_OR_NEWER
                    if (parent.propertyType != SerializedPropertyType.Generic && parent.boxedValue != null)
                    {
                        var result = DynamicUtil.GetMemberFromType(parent.boxedValue.GetType(), prop.name, true, MemberTypes.Field) as System.Reflection.FieldInfo;
                        fieldType = result?.FieldType;
                        return result;
                    }
#endif
                    break;
            }
#endif

            var elements = prop.propertyPath.Split('.');
            var field = DynamicUtil.GetMemberFromType(roottype, elements[0], true, MemberTypes.Field) as System.Reflection.FieldInfo;
            fieldType = field.FieldType;
            parent = prop.serializedObject.FindProperty(elements[0]);
            for (int i = 1; i < elements.Length; i++)
            {
                var element = elements[i];
                if (element == "Array" && (i + 1) < elements.Length && elements[i + 1].StartsWith("data") && TypeUtil.IsListType(fieldType))
                {
                    var sindex = elements[i + 1];
                    element = "Array." + sindex;
                    int index = System.Convert.ToInt32(sindex.Substring(sindex.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    i++; //skip one

                    field = null; //we don't actually support ref'n the field of an array/list element because array doesn't have a FieldInfo for an indexed entry, and List<> is just wrapping an array.
                    fieldType = TypeUtil.GetElementTypeOfListType(fieldType);
                    parent = parent.FindPropertyRelative(element);
                }
                else
                {
                    field = DynamicUtil.GetMemberFromType(fieldType, element, true, MemberTypes.Field) as System.Reflection.FieldInfo;
                    if (field == null)
                    {
                        fieldType = null;
                        return null; //we failed in some critical way, quit now
                    }

                    parent = parent.FindPropertyRelative(elements[i]);
#if UNITY_2021_2_OR_NEWER
                    if (parent.propertyType == SerializedPropertyType.ManagedReference)
                    {
                        fieldType = parent.GetManagedReferenceType() ?? field.FieldType;
                    }
                    else
#endif
                    {
                        fieldType = field.FieldType;
                    }
                }
            }

            return field;
        }

        static IEnumerable<System.ValueTuple<SerializedProperty, FieldInfo, System.Type>> WalkPropertyPath(SerializedProperty prop)
        {
            var roottype = GetTargetType(prop.serializedObject);
            var elements = prop.propertyPath.Split('.');
            var field = DynamicUtil.GetMemberFromType(roottype, elements[0], true, MemberTypes.Field) as System.Reflection.FieldInfo;
            var fieldType = field.FieldType;
            var parent = prop.serializedObject.FindProperty(elements[0]);
            yield return (parent, field, fieldType);

            for (int i = 1; i < elements.Length; i++)
            {
                var element = elements[i];
                if (element == "Array" && (i + 1) < elements.Length && elements[i + 1].StartsWith("data") && TypeUtil.IsListType(fieldType))
                {
                    var sindex = elements[i + 1];
                    element = "Array." + sindex;
                    int index = System.Convert.ToInt32(sindex.Substring(sindex.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    i++; //skip one

                    field = null; //we don't actually support ref'n the field of an array/list element because array doesn't have a FieldInfo for an indexed entry, and List<> is just wrapping an array.
                    fieldType = TypeUtil.GetElementTypeOfListType(fieldType);
                    parent = parent.FindPropertyRelative(element);
                    yield return (parent, field, fieldType);
                }
                else
                {
                    field = DynamicUtil.GetMemberFromType(fieldType, element, true, MemberTypes.Field) as System.Reflection.FieldInfo;
                    if (field == null)
                    {
                        fieldType = null;
                        yield break;
                    }

                    parent = parent.FindPropertyRelative(elements[i]);
#if UNITY_2021_2_OR_NEWER
                    if (parent.propertyType == SerializedPropertyType.ManagedReference)
                    {
                        fieldType = parent.GetManagedReferenceType() ?? field.FieldType;
                    }
                    else
#endif
                    {
                        fieldType = field.FieldType;
                    }

                    yield return (parent, field, fieldType);
                }
            }

        }

        #endregion

        #region Path

        public static string GetFullPathForAssetPath(string assetPath)
        {
            var dataPath = Application.dataPath;
            if (dataPath.EndsWith("Assets")) dataPath = dataPath.Substring(0, dataPath.Length - 6);
            return System.IO.Path.Combine(dataPath, assetPath);
        }

        #endregion

        #region GUIContent

        //private static TrackablObjectCachePool<GUIContent> _temp_text = new TrackablObjectCachePool<GUIContent>(50);

        public static bool HasContent(this GUIContent label)
        {
            return label != null && (!string.IsNullOrEmpty(label.text) || label.image != null);
        }

        public static GUIContent GetLabelContent(SerializedProperty prop)
        {
            var c = new GUIContent(); // _temp_text.GetInstance();
            c.text = prop.displayName;
            c.tooltip = prop.tooltip;
            return c;
        }

        /// <summary>
        /// Single immediate use GUIContent for a label. Should be used immediately and not stored/referenced for later use.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static GUIContent TempContent(string text)
        {
            var c = new GUIContent(); // _temp_text.GetInstance();
            c.text = text;
            c.tooltip = null;
            return c;
        }

        public static GUIContent TempContent(string text, string tooltip)
        {
            var c = new GUIContent(); // _temp_text.GetInstance();
            c.text = text;
            c.tooltip = tooltip;
            return c;
        }

        public static GUIContent Clone(this GUIContent content)
        {
            var c = new GUIContent(); // _temp_text.GetInstance();
            c.text = content.text;
            c.tooltip = content.tooltip;
            c.image = content.image;
            return c;
        }

        public static GUIContent ObjectContent(UnityEngine.Object obj, System.Type expectedType, bool useProxyIconIfRelevant)
        {
            var content = EditorGUIUtility.ObjectContent(obj, obj != null ? obj.GetType() : expectedType).Clone();
            if (obj != null && expectedType != null && !expectedType.IsInstanceOfType(obj) && useProxyIconIfRelevant)
            {
                content.image = EditorGUIUtility.IconContent("_Help")?.image ?? IconHelper.GetIcon(IconHelper.Icon.DiamondPurple);
            }
            return content;
        }

        #endregion

        #region Indent Helper

        private static Stack<int> _indents = new Stack<int>();

        public static void SuppressIndentLevel()
        {
            _indents.Push(EditorGUI.indentLevel);
            EditorGUI.indentLevel = 0;
        }

        public static void SuppressIndentLevel(int tempLevel)
        {
            _indents.Push(EditorGUI.indentLevel);
            EditorGUI.indentLevel = tempLevel;
        }

        public static void ResumeIndentLevel()
        {
            if (_indents.Count > 0)
            {
                EditorGUI.indentLevel = _indents.Pop();
            }
        }

        #endregion

        #region Event Handlers

        //private static void OnSceneGUI(SceneView scene)
        //{
        //    foreach (var c in _temp_text.ActiveMembers.ToArray())
        //    {
        //        _temp_text.Release(c);
        //    }

        //    _indents.Clear();
        //}

        #endregion

        #region Value Utils

        /// <summary>
        /// An editor time safe version of DynamicUtil.GetValueWithMember that attempts to not leak various values into the scene (like materials).
        /// </summary>
        /// <param name="info"></param>
        /// <param name="targObj"></param>
        /// <param name="ignoreMethod"></param>
        /// <returns></returns>
        public static object GetValueWithMemberSafe(MemberInfo info, object targObj, bool ignoreMethod)
        {
            if (info == null) return null;
            targObj = IProxyExtensions.ReduceIfProxy(targObj);
            if (targObj == null) return null;

            var tp = info.DeclaringType;
            if (TypeUtil.IsType(tp, typeof(Renderer)))
            {
                switch (info.Name)
                {
                    case "material":
                        return DynamicUtil.GetValue(targObj, "sharedMaterial");
                    case "materials":
                        return DynamicUtil.GetValue(targObj, "sharedMaterials");
                }
            }
            else if (TypeUtil.IsType(tp, typeof(MeshFilter)))
            {
                switch (info.Name)
                {
                    case "mesh":
                        return DynamicUtil.GetValue(targObj, "sharedMesh");
                }
            }

            return DynamicUtil.GetValueWithMember(info, targObj, ignoreMethod);
        }

        public static int ConvertPopupMaskToEnumMask(int mask, System.Enum[] enumFlagValues)
        {
            if (enumFlagValues == null || enumFlagValues.Length == 0) return 0;
            if (mask == 0) return 0;
            if (mask == -1) return -1;

            int result = 0;
            for (int i = 0; i < enumFlagValues.Length; i++)
            {
                int flag = 1 << i;
                if ((mask & flag) != 0)
                {
                    result |= ConvertUtil.ToInt(enumFlagValues[i]);
                }
            }
            return result;
        }

        public static int ConvertEnumMaskToPopupMask(int mask, System.Enum[] enumFlagValues)
        {
            if (enumFlagValues == null || enumFlagValues.Length == 0) return 0;
            if (mask == 0) return 0;
            if (mask == -1) return -1;

            int result = 0;
            for (int i = 0; i < enumFlagValues.Length; i++)
            {
                int e = ConvertUtil.ToInt(enumFlagValues[i]);
                if ((mask & e) != 0)
                {
                    result |= (1 << i);
                }
            }
            return result;
        }

        #endregion

        #region Guid/GlobalId

        public static System.Guid ToGuid(this GUID gid)
        {
            System.Guid result;
            System.Guid.TryParse(gid.ToString(), out result);
            return result;
        }

        public static bool TryGetLinkedGuid(UnityEngine.Object obj, out System.Guid guid, LinkedGuidMode mode)
        {
            if (mode == LinkedGuidMode.Auto)
            {
                if (obj is Component || obj is GameObject)
                {
                    mode = LinkedGuidMode.Convolusion;
                }
                else if (obj is ScriptableObject)
                {
                    mode = LinkedGuidMode.Asset;
                }
                else
                {
                    //we really shouldn't get here ever, but we're going to just assume it's some rando asset
                    mode = LinkedGuidMode.Asset;
                }
            }

            switch (mode)
            {
                case LinkedGuidMode.None:
                    guid = default;
                    return false;
                case LinkedGuidMode.Asset:
                    if (EditorHelper.TryGetNearestAssetGuid(obj, out guid) && guid != default)
                    {
                        return true;
                    }
                    break;
                case LinkedGuidMode.GlobIdPair:
                    if (EditorHelper.TryGetNearestAssetGlobalObjectId(obj, out GlobalObjectId gid1))
                    {
                        if (gid1.targetObjectId != 0UL)
                        {
                            guid = (new SerializableGuid(gid1.targetObjectId, gid1.targetPrefabId)).ToGuid();
                            return true;
                        }
                    }
                    break;
                case LinkedGuidMode.Convolusion:
                    if (EditorHelper.TryGetNearestAssetGlobalObjectId(obj, out GlobalObjectId gid2))
                    {
                        ulong high = (gid2.targetObjectId << 32) | (gid2.targetObjectId >> 32) | (gid2.targetObjectId >> 48);
                        if (gid2.targetPrefabId != 0UL)
                        {
                            guid = new SerializableGuid(high, gid2.targetPrefabId);
                        }
                        else
                        {
                            ulong h, l;
                            ((SerializableGuid)gid2.assetGUID.ToGuid()).ToHighLow(out h, out l);
                            guid = new SerializableGuid(high, h);
                        }
                        return guid != default;
                    }
                    break;

            }

            guid = default;
            return false;
        }

        public static bool TryGetNearestAssetGlobalObjectId(UnityEngine.Object obj, out GlobalObjectId gid)
        {
            if (obj == null)
            {
                gid = default;
                return false;
            }

            var go = obj as GameObject;
            if (!go) go = obj is Component c ? c.gameObject : null;

            if (go)
            {
                var root = PrefabUtility.GetNearestPrefabInstanceRoot(go);
                if (root == null) root = go;

                //first check if we're on a stage
                //var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                //if (stage != null && stage.prefabContentsRoot == root)
                var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(root);
                if (stage != null)
                {
                    gid = GlobalObjectId.GetGlobalObjectIdSlow(AssetDatabase.LoadAssetAtPath(stage.assetPath, typeof(UnityEngine.Object)));
                    if (gid.assetGUID != default)
                    {
                        return true;
                    }
                }

                var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
                if (!string.IsNullOrEmpty(path))
                {
                    gid = GlobalObjectId.GetGlobalObjectIdSlow(AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)));
                    if (gid.assetGUID != default)
                    {
                        return true;
                    }
                }

                gid = GlobalObjectId.GetGlobalObjectIdSlow(obj);
                if (gid.assetGUID != default)
                {
                    return true;
                }
            }
            else
            {
                gid = GlobalObjectId.GetGlobalObjectIdSlow(obj);
                if (gid.assetGUID != default)
                {
                    return true;
                }
            }

            gid = default;
            return false;
        }

        public static bool TryGetNearestAssetGuid(UnityEngine.Object obj, out System.Guid guid)
        {
            if (obj == null)
            {
                guid = default;
                return false;
            }

            var go = obj as GameObject;
            if (!go) go = obj is Component c ? c.gameObject : null;

            if (go)
            {
                var root = PrefabUtility.GetNearestPrefabInstanceRoot(go);
                if (root == null) root = go;

                //first check if we're on a stage
                //var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                //if (stage != null && stage.prefabContentsRoot == root &&
                var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(root);
                if (stage != null &&
                    System.Guid.TryParse(AssetDatabase.AssetPathToGUID(stage.assetPath), out guid) &&
                    guid != default)
                {
                    return true;
                }

                var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
                if (!string.IsNullOrEmpty(path))
                {
                    if (System.Guid.TryParse(AssetDatabase.AssetPathToGUID(path), out guid))
                    {
                        return true;
                    }
                }

                var gid = GlobalObjectId.GetGlobalObjectIdSlow(obj);
                if (gid.assetGUID != default)
                {
                    guid = gid.assetGUID.ToGuid();
                    return true;
                }
            }
            else
            {
                var gid = GlobalObjectId.GetGlobalObjectIdSlow(obj);
                if (gid.assetGUID != default)
                {
                    guid = gid.assetGUID.ToGuid();
                    return true;
                }
            }

            guid = default;
            return false;
        }

        public static GameObject GetNearestPrefabInstanceRoot_PrefabStageAware(UnityEngine.Object obj)
        {
            var root = PrefabUtility.GetNearestPrefabInstanceRoot(obj);
            if (root) return root;

            root = obj as GameObject;
            if (!root && obj is Component c) root = c.gameObject;

            var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(root);
            if (stage != null) return AssetDatabase.LoadAssetAtPath<GameObject>(stage.assetPath);

            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
            if (!string.IsNullOrEmpty(path)) return AssetDatabase.LoadAssetAtPath<GameObject>(stage.assetPath);

            var gid = GlobalObjectId.GetGlobalObjectIdSlow(obj);
            if (gid.assetGUID != default)
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(gid.assetGUID));
            }

            return null;
        }

        #endregion



        #region State Cache

        private static Dictionary<int, object> _states = new Dictionary<int, object>();

        /// <summary>
        /// Get a state object that can be stored between PropertyHandler.OnGUI calls.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static T GetCachedState<T>(SerializedProperty property)
        {
            if (property == null) return default(T);

            int hash = com.spacepuppyeditor.Internal.PropertyHandlerCache.GetIndexRespectingPropertyHash(property);
            object result;
            if (_states.TryGetValue(hash, out result) && result is T)
                return (T)result;
            else
                return default(T);
        }

        /// <summary>
        /// Set a state object that can be stored between PropertyHandler.OnGUI calls.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="state"></param>
        public static void SetCachedState(SerializedProperty property, object state)
        {
            if (property == null) return;

            int hash = com.spacepuppyeditor.Internal.PropertyHandlerCache.GetIndexRespectingPropertyHash(property);
            if (state == null)
                _states.Remove(hash);
            else
                _states[hash] = state;
        }

        #endregion

        #region Invoke Hook

        private static int _mainThreadId;
        private static int _updateToken;
        private static List<InvokeCallback> _invokeCallbacks = new List<InvokeCallback>();

        public static bool InvokeRequired => _mainThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId;

        public static void Invoke(System.Action act, float dur = 0f)
        {
            if (act == null) return;

            lock (_invokeCallbacks)
            {
                _invokeCallbacks.Add(new InvokeCallback()
                {
                    callback = act,
                    duration = dur,
                    timestamp = System.DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Queues up an action only if the token is for a new frame. This allows for you to schedule a single invoke 
        /// with multiple attempts before the same frame guaranteeing it only calling back once.
        /// </summary>
        /// <param name="act"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static int InvokePassive(System.Action act, int token)
        {
            if (_updateToken == token) return _updateToken;

            lock (_invokeCallbacks)
            {
                _invokeCallbacks.Add(new InvokeCallback()
                {
                    callback = act,
                    duration = 0f,
                    timestamp = System.DateTime.UtcNow
                });
            }
            return _updateToken;
        }

        private static void EditorUpdate()
        {
            _updateToken++;
            using (var lst = TempCollection.GetList<InvokeCallback>())
            {
                lock (_invokeCallbacks)
                {
                    var dt = System.DateTime.UtcNow;
                    if (_invokeCallbacks.Count > 0)
                    {
                        for (int i = 0; i < _invokeCallbacks.Count; i++)
                        {
                            if ((dt - _invokeCallbacks[i].timestamp).TotalSeconds >= _invokeCallbacks[i].duration)
                            {
                                lst.Add(_invokeCallbacks[i]);
                                _invokeCallbacks.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }

                if (lst.Count > 0)
                {
                    foreach (var o in lst)
                    {
                        try
                        {
                            o.callback?.Invoke();
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            }
        }

        private struct InvokeCallback
        {
            public System.Action callback;
            public float duration;
            public System.DateTime timestamp;
        }

        #endregion

        #region Special Types

        public interface ISerializedWrapperHelper
        {
            public object GetValue(SerializedProperty property);
            public bool SetValue(SerializedProperty property, object value);
            public System.Type GetValueType(SerializedProperty property);
        }

        public struct SerializedPropertyChangeCheckToken
        {
            /*
             * This method while it works logs strange errors claiming accessing 'hash128Value' is junk. Might be a bug in Unity and may be resolved in the future (I'm on an old unity version, it could already be resolved)
             * 
            private SerializedProperty property;
            private Hash128 hash;

            public SerializedPropertyChangeCheckToken(SerializedProperty property)
            {
                this.property = property;
                this.hash = property != null ? property.hash128Value : default;
            }

            public bool EndChangeCheck() => property != null && property.hash128Value != this.hash;
            */

            private SerializedProperty property;
            private int hash;

            public SerializedPropertyChangeCheckToken(SerializedProperty property)
            {
                this.property = property;
                this.hash = CalculateHash(property);
            }

            public bool EndChangeCheck() => CalculateHash(property) != this.hash;

            static int CalculateHash(SerializedProperty property)
            {
                if (property == null) return 0;

                if (property.hasMultipleDifferentValues)
                {
                    return -1;
                }
                else
                {
                    switch (property.propertyType)
                    {
                        case SerializedPropertyType.ObjectReference:
                            return property.objectReferenceInstanceIDValue;
                        default:
                            return EditorHelper.GetPropertyValue(property)?.GetHashCode() ?? 0;
                    }
                }
            }

        }

        #endregion

    }

}