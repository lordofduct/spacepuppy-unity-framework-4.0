using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

using com.spacepuppy.Utils;

using Regex = System.Text.RegularExpressions.Regex;

namespace com.spacepuppyeditor.Internal
{

    public class SPReorderableList : ReorderableList
    {

        #region CONSTRUCTOR

        public SPReorderableList(SerializedObject serializedObj, SerializedProperty property)
            : base(serializedObj, property)
        {

        }
        public SPReorderableList(System.Collections.IList elements, System.Type elementType)
            : base(elements, elementType)
        {

        }
        public SPReorderableList(System.Collections.IList elements, System.Type elementType, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton)
            : base(elements, elementType, draggable, displayHeader, displayAddButton, displayRemoveButton)
        {

        }
        public SPReorderableList(SerializedObject serializedObject, SerializedProperty elements, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton)
            : base(serializedObject, elements, draggable, displayHeader, displayAddButton, displayRemoveButton)
        {

        }

        #endregion

        #region Properties

        public System.Action<ReorderableList> onClearContextMenu { get; set; } = null;

        #endregion

        #region Methods

        public new void DoLayoutList()
        {
            //var beforeRect = GUILayoutUtility.GetLastRect();
            //base.DoLayoutList();
            //var afterRect = GUILayoutUtility.GetLastRect();

            //var area = new Rect(afterRect.xMin, beforeRect.yMax, afterRect.width, this.headerHeight);
            //this.DoHeaderContextMenu(area);

            var area = EditorGUILayout.GetControlRect(false, this.GetHeight());
            this.DoList(area);

            var headerArea = new Rect(area.xMin, area.yMin, area.width, this.headerHeight);
            this.DoHeaderContextMenu(headerArea);
        }

        public new void DoList(Rect rect)
        {
            base.DoList(rect);

            var area = new Rect(rect.xMin, rect.yMin, rect.width, this.headerHeight);
            this.DoHeaderContextMenu(area);
        }

        public void DoHeaderContextMenu(Rect area)
        {
            if (ReorderableListHelper.IsClickingArea(area, MouseUtil.BTN_RIGHT))
            {
                Event.current.Use();

                var menu = new GenericMenu();

                if (this.serializedProperty != null)
                {
                    menu.AddItem(new GUIContent("Copy Property Path"), false, () =>
                    {
                        GUIUtility.systemCopyBuffer = this.serializedProperty?.propertyPath;
                    });
                    menu.AddSeparator("");
                }

#if UNITY_2022_2_OR_NEWER
                if (this.serializedProperty != null)
                {
                    menu.AddItem(new GUIContent("Copy"), false, () =>
                    {
                        GUIUtility.systemCopyBuffer = CopyPasteJsonEmulator.Stringify(this.serializedProperty);
                    });
                    if (CopyPasteJsonEmulator.Validate(this.serializedProperty.propertyType, GUIUtility.systemCopyBuffer))
                    {
                        menu.AddItem(new GUIContent("Paste"), false, () =>
                        {
                            if (CopyPasteJsonEmulator.TryPaste(GUIUtility.systemCopyBuffer, this.serializedProperty))
                            {
                                this.serializedProperty.isExpanded = true;
                            }
                        });
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent("Paste"));
                    }
                    menu.AddSeparator("");
                }
#endif

                if (this.onClearContextMenu != null)
                {
                    var lst = this.list;
                    menu.AddItem(new GUIContent("Clear"), false, () =>
                    {
                        onClearContextMenu?.Invoke(this);
                    });
                }
                else if (this.serializedProperty != null)
                {
                    var prop = this.serializedProperty;
                    menu.AddItem(new GUIContent("Clear"), false, () =>
                    {
                        prop.serializedObject.Update();
                        prop.arraySize = 0;
                        prop.serializedObject.ApplyModifiedProperties();
                        this.index = -1;
                    });
                }
                else if (this.list != null)
                {
                    var lst = this.list;
                    menu.AddItem(new GUIContent("Clear"), false, () =>
                    {
                        lst.Clear();
                        this.index = -1;
                    });
                }

                menu.ShowAsContext();
            }
        }

        #endregion

    }

    public sealed class CachedReorderableList : SPReorderableList
    {

        public GUIContent Label;

        private CachedReorderableList(SerializedObject serializedObj, SerializedProperty property)
            : base(serializedObj, property)
        {
        }

        private CachedReorderableList(System.Collections.IList memberList)
            : base(memberList, null)
        {
        }

        #region Methods

        //private static FieldInfo _m_SerializedObject;
        private void ReInit(SerializedObject obj, SerializedProperty prop)
        {
            //try
            //{
            //    if (_m_SerializedObject == null)
            //        _m_SerializedObject = typeof(ReorderableList).GetField("m_SerializedObject", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            //    _m_SerializedObject.SetValue(this, obj);
            //}
            //catch
            //{
            //    UnityEngine.Debug.LogWarning("This version of Spacepuppy Framework does not support the version of Unity it's being used with (CachedReorderableList).");
            //}

            this.serializedProperty = prop;
            this.list = null;
        }

        private void ReInit(System.Collections.IList memberList, SerializedProperty tokenProperty)
        {
            //try
            //{
            //    if (_m_SerializedObject == null)
            //        _m_SerializedObject = typeof(ReorderableList).GetField("m_SerializedObject", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            //    _m_SerializedObject.SetValue(this, null);
            //}
            //catch
            //{
            //    UnityEngine.Debug.LogWarning("This version of Spacepuppy Framework does not support the version of Unity it's being used with (CachedReorderableList).");
            //}

            //this.serializedProperty = null;
            if (this.serializedProperty != null) this.serializedProperty = tokenProperty;
            this.list = memberList;
        }

        #endregion


        #region Static Factory

        private static Dictionary<int, CachedReorderableList> _lstCache = new Dictionary<int, CachedReorderableList>();

        public static CachedReorderableList GetListDrawer(SerializedProperty property, ReorderableList.HeaderCallbackDelegate drawHeaderCallback, ReorderableList.ElementCallbackDelegate drawElementCallback,
                                                          ReorderableList.AddCallbackDelegate onAddCallback = null, ReorderableList.RemoveCallbackDelegate onRemoveCallback = null, ReorderableList.SelectCallbackDelegate onSelectCallback = null,
                                                          ReorderableList.ChangedCallbackDelegate onChangedCallback = null, ReorderableList.ReorderCallbackDelegate onReorderCallback = null, ReorderableList.CanRemoveCallbackDelegate onCanRemoveCallback = null,
                                                          ReorderableList.AddDropdownCallbackDelegate onAddDropdownCallback = null)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));
            if (!property.isArray) throw new System.ArgumentException("SerializedProperty must be a property for an Array or List", nameof(property));

            int hash = com.spacepuppyeditor.Internal.PropertyHandlerCache.GetIndexRespectingPropertyHash(property);
            CachedReorderableList lst;
            if (_lstCache.TryGetValue(hash, out lst))
            {
                lst.ReInit(property.serializedObject, property);
                try
                {
                    lst.GetHeight();
                }
                catch
                {
                    _lstCache.Remove(hash);
                    lst = null;
                    Debug.LogWarning("Spacepuppy failed to retrieve a cached ReorderableList - internal note for looking into in the future.");
                }
            }

            if (lst == null)
            {
                lst = new CachedReorderableList(property.serializedObject, property);
                _lstCache[hash] = lst;
            }

            lst.drawHeaderCallback = drawHeaderCallback;
            lst.drawElementCallback = drawElementCallback;
            lst.onAddCallback = onAddCallback;
            lst.onRemoveCallback = onRemoveCallback;
            lst.onSelectCallback = onSelectCallback;
            lst.onChangedCallback = onChangedCallback;
            lst.onReorderCallback = onReorderCallback;
            lst.onCanRemoveCallback = onCanRemoveCallback;
            lst.onAddDropdownCallback = onAddDropdownCallback;

            return lst;
        }

        /// <summary>
        /// Creates a cached ReorderableList that can be used on a IList. The serializedProperty passed is used for look-up and is not used in the ReorderableList itself.
        /// </summary>
        /// <param name="memberList"></param>
        /// <param name="tokenProperty"></param>
        /// <param name="drawHeaderCallback"></param>
        /// <param name="drawElementCallback"></param>
        /// <param name="onAddCallback"></param>
        /// <param name="onRemoveCallback"></param>
        /// <param name="onSelectCallback"></param>
        /// <param name="onChangedCallback"></param>
        /// <param name="onReorderCallback"></param>
        /// <param name="onCanRemoveCallback"></param>
        /// <param name="onAddDropdownCallback"></param>
        /// <returns></returns>
        public static CachedReorderableList GetListDrawer(System.Collections.IList memberList, SerializedProperty tokenProperty, ReorderableList.HeaderCallbackDelegate drawHeaderCallback, ReorderableList.ElementCallbackDelegate drawElementCallback,
                                                  ReorderableList.AddCallbackDelegate onAddCallback = null, ReorderableList.RemoveCallbackDelegate onRemoveCallback = null, ReorderableList.SelectCallbackDelegate onSelectCallback = null,
                                                  ReorderableList.ChangedCallbackDelegate onChangedCallback = null, ReorderableList.ReorderCallbackDelegate onReorderCallback = null, ReorderableList.CanRemoveCallbackDelegate onCanRemoveCallback = null,
                                                  ReorderableList.AddDropdownCallbackDelegate onAddDropdownCallback = null)
        {
            if (memberList == null) throw new System.ArgumentNullException("memberList");
            if (tokenProperty == null) throw new System.ArgumentNullException("property");

            int hash = com.spacepuppyeditor.Internal.PropertyHandlerCache.GetIndexRespectingPropertyHash(tokenProperty);
            CachedReorderableList lst;
            if (_lstCache.TryGetValue(hash, out lst))
            {
                lst.ReInit(memberList, tokenProperty);
            }
            else
            {
                lst = new CachedReorderableList(memberList);
                _lstCache[hash] = lst;
            }

            lst.drawHeaderCallback = drawHeaderCallback;
            lst.drawElementCallback = drawElementCallback;
            lst.onAddCallback = onAddCallback;
            lst.onRemoveCallback = onRemoveCallback;
            lst.onSelectCallback = onSelectCallback;
            lst.onChangedCallback = onChangedCallback;
            lst.onReorderCallback = onReorderCallback;
            lst.onCanRemoveCallback = onCanRemoveCallback;
            lst.onAddDropdownCallback = onAddDropdownCallback;

            return lst;
        }

        #endregion

    }


#if UNITY_2022_2_OR_NEWER
    //HACK - this is a work in progress and currently only used here in CachedReorderableList. Once I work out all the ins and outs I'll migrate this elsewhere. - dylane
    static class CopyPasteJsonEmulator
    {

        public static string Stringify(SerializedProperty property)
        {
            const string EMPTY_JSON_OBJECT = "{}";

            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    return ToGenericJsonWrapper(property)?.Stringify() ?? EMPTY_JSON_OBJECT;
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return property.boolValue ? "True" : "False";
                case SerializedPropertyType.Float:
                    return property.floatValue.ToString();
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Color:
                    return $"#{property.colorValue.ToARGB().ToString("X8")}";
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.ExposedReference:
                    {
                        var obj = property.propertyType == SerializedPropertyType.ObjectReference ? property.objectReferenceValue : property.exposedReferenceValue;
                        return new UnityObjectJsonWrapper(obj).Stringify();
                    }
                case SerializedPropertyType.LayerMask:
                    return $"LayerMask({property.intValue}";
                case SerializedPropertyType.Enum:
                    return $"Enum:{property.enumNames[property.enumValueIndex]}";
                case SerializedPropertyType.Vector2:
                    return $"Vector2({property.vector2Value.x},{property.vector2Value.y})";
                case SerializedPropertyType.Vector3:
                    return $"Vector3({property.vector3Value.x},{property.vector3Value.y},{property.vector3Value.z})";
                case SerializedPropertyType.Vector4:
                    return $"Vector4({property.vector4Value.x},{property.vector4Value.y},{property.vector4Value.z},{property.vector4Value.w})";
                case SerializedPropertyType.Rect:
                    return $"Rect({property.rectValue.x},{property.rectValue.y},{property.rectValue.width},{property.rectValue.height})";
                case SerializedPropertyType.ArraySize:
                    return property.intValue.ToString();
                case SerializedPropertyType.Character:
                    return property.stringValue;
                case SerializedPropertyType.AnimationCurve:
                    //TODO - support this!
                    return EMPTY_JSON_OBJECT;
                case SerializedPropertyType.Bounds:
                    return $"Bounds({property.boundsValue.center.x},{property.boundsValue.center.y},{property.boundsValue.center.z},{property.boundsValue.extents.x},{property.boundsValue.extents.y},{property.boundsValue.extents.z})";
                case SerializedPropertyType.Gradient:
                    //TODO - support this!
                    return EMPTY_JSON_OBJECT;
                case SerializedPropertyType.Quaternion:
                    return $"Quaternion({property.quaternionValue.x},{property.quaternionValue.y},{property.quaternionValue.z},{property.quaternionValue.w})";
                case SerializedPropertyType.FixedBufferSize:
                    //NEVER TODO - I don't care about this...
                    return EMPTY_JSON_OBJECT;
                case SerializedPropertyType.Vector2Int:
                    return $"Vector2({property.vector2IntValue.x},{property.vector2IntValue.y})";
                case SerializedPropertyType.Vector3Int:
                    return $"Vector3({property.vector3IntValue.x},{property.vector3IntValue.y},{property.vector3IntValue.z})";
                case SerializedPropertyType.RectInt:
                    return $"Rect({property.rectIntValue.x},{property.rectIntValue.y},{property.rectIntValue.width},{property.rectIntValue.height})";
                case SerializedPropertyType.BoundsInt:
                    return $"Bounds({property.boundsIntValue.position.x},{property.boundsIntValue.position.y},{property.boundsIntValue.position.z},{property.boundsIntValue.size.x},{property.boundsIntValue.size.y},{property.boundsIntValue.size.z})";
                case SerializedPropertyType.ManagedReference:
                    //TODO - support this???
                    return EMPTY_JSON_OBJECT;
                case SerializedPropertyType.Hash128:
                    return property.hash128Value.ToString();
                default:
                    return EMPTY_JSON_OBJECT;
            }

        }

        public static bool Validate(SerializedPropertyType etype, string str)
        {
            switch (etype)
            {
                case SerializedPropertyType.Generic:
                    return str?.StartsWith("GenericPropertyJSON:") ?? false;
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.ArraySize:
                    return int.TryParse(str, out _);
                case SerializedPropertyType.Boolean:
                    return bool.TryParse(str, out _);
                case SerializedPropertyType.Float:
                    return float.TryParse(str, out _);
                case SerializedPropertyType.String:
                    return true;
                case SerializedPropertyType.Color:
                    return Regex.IsMatch(str, @"^#[0-9a-fA-F]{8}$");
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.ExposedReference:
                    return str?.StartsWith("UnityEditor.ObjectWrapperJSON:") ?? false;
                case SerializedPropertyType.LayerMask:
                    return str?.StartsWith("LayerMask(") ?? false;
                case SerializedPropertyType.Enum:
                    return str?.StartsWith("Enum") ?? false;
                case SerializedPropertyType.Vector2:
                    return str?.StartsWith("Vector2(") ?? false;
                case SerializedPropertyType.Vector3:
                    return str?.StartsWith("Vector3(") ?? false;
                case SerializedPropertyType.Vector4:
                    return str?.StartsWith("Vector4(") ?? false;
                case SerializedPropertyType.Rect:
                    return str?.StartsWith("Rect(") ?? false;
                case SerializedPropertyType.Character:
                    return str == null || str.Length <= 1;
                case SerializedPropertyType.AnimationCurve:
                    return false; //TODO - need to add support for this in TryParse
                                  //return str?.StartsWith("UnityEditor.AnimationCurveWrapperJSON:") ?? false;
                case SerializedPropertyType.Bounds:
                    return str?.StartsWith("Bounds(") ?? false;
                case SerializedPropertyType.Gradient:
                    return false; //TODO - need to add support for this in TryParse
                                  //return str?.StartsWith("UnityEditor.GradientWrapperJSON:") ?? false;
                case SerializedPropertyType.Quaternion:
                    return (str?.StartsWith("Quaternion(") ?? false) || (str?.StartsWith("Vector3(") ?? false);
                case SerializedPropertyType.FixedBufferSize:
                    //NEVER TODO - I dont give a fuck about this one.
                    return false;
                case SerializedPropertyType.Vector2Int:
                    return str?.StartsWith("Vector2(") ?? false;
                case SerializedPropertyType.Vector3Int:
                    return str?.StartsWith("Vector3(") ?? false;
                case SerializedPropertyType.RectInt:
                    return str?.StartsWith("Rect(") ?? false;
                case SerializedPropertyType.BoundsInt:
                    return str?.StartsWith("Bounds(") ?? false;
                case SerializedPropertyType.ManagedReference:
                    return false; //TODO - need to add support for this in TryParse? Will we ever support this? Will need to test Unity first.
                case SerializedPropertyType.Hash128:
                    return Regex.IsMatch(str, @"^\d{32}$");
                default:
                    return false;
            }

        }

        public static bool TryPaste(string pasteString, SerializedProperty property)
        {
            if (TryParse(property.propertyType, pasteString, out object value))
            {
                if (property.isArray && value is object[] arr)
                {
                    property.arraySize = arr.Length;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        var element = property.GetArrayElementAtIndex(i);
                        EditorHelper.SetPropertyValue(element, arr[i]);
                    }
                    property.serializedObject.ApplyModifiedProperties();
                    return true;
                }
                else
                {
                    EditorHelper.SetPropertyValue(property, value);
                    property.serializedObject.ApplyModifiedProperties();
                    return true;
                }
            }
            else if (pasteString?.StartsWith("GenericPropertyJSON:") ?? false)
            {
                if (JsonUtility.FromJson<GenericUnwrapper>(pasteString.Substring(20))?.CopyTo(property) ?? false)
                {
                    property.serializedObject.ApplyModifiedProperties();
                    return true;
                }
            }
            return false;
        }

        static bool TryParse(SerializedPropertyType etype, string str, out object value)
        {
            value = default;
            switch (etype)
            {
                case SerializedPropertyType.Generic:
                    if (str?.StartsWith("GenericPropertyJSON:") ?? false)
                    {
                        /*
                        var wrapper = JsonUtility.FromJson<GenericPropertyJsonUnwrapper>(str.Substring(20));
                        if (wrapper.children == null || wrapper.children.Length == 0) return false;

                        var arraywrapper = wrapper.children[0];
                        int arraySize = Mathf.Max(0, (arraywrapper.children?.Length ?? 0) - 1);
                        object[] arr = new object[arraySize];
                        for (int i = 1; i < arraywrapper.children.Length; i++)
                        {
                            int index = i - 1;
                            var elwrapper = arraywrapper.children[i];
                            if (TryParse((SerializedPropertyType)elwrapper.type, elwrapper.val, out object valueAtIndex))
                            {
                                arr[index] = valueAtIndex;
                            }
                        }
                        value = arr;
                        return true;
                        */
                    }
                    return false;
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.ArraySize:
                    if (int.TryParse(str, out int ival))
                    {
                        value = ival;
                        return true;
                    }
                    return false;
                case SerializedPropertyType.Boolean:
                    if (bool.TryParse(str, out bool b))
                    {
                        value = b;
                        return true;
                    }
                    return false;
                case SerializedPropertyType.Float:
                    if (float.TryParse(str, out float f))
                    {
                        value = f;
                        return true;
                    }
                    return false;
                case SerializedPropertyType.String:
                    value = str ?? string.Empty;
                    return true;
                case SerializedPropertyType.Color:
                    if (Regex.IsMatch(str, @"^#[0-9a-fA-F]{8}$") && uint.TryParse(str.Substring(1), out uint ic))
                    {
                        value = ColorUtil.ARGBToColor(ic);
                        return true;
                    }
                    return false;
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.ExposedReference:
                    if (str.StartsWith("UnityEditor.ObjectWrapperJSON:"))
                    {
                        var uobjw = JsonUtility.FromJson<UnityObjectJsonWrapper>(str.Substring("UnityEditor.ObjectWrapperJSON:".Length));
                        if (!string.IsNullOrEmpty(uobjw.guid) && GUID.TryParse(uobjw.guid, out GUID guid))
                        {
                            value = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(UnityEngine.Object));
                            return true;
                        }
                        else
                        {
                            value = EditorUtility.InstanceIDToObject(uobjw.instanceID);
                            return true;
                        }
                    }
                    return false;
                case SerializedPropertyType.LayerMask:
                    if (str?.StartsWith("LayerMask(") ?? false)
                    {
                        int si = "LayerMask(".Length;
                        value = (LayerMask)ConvertUtil.ToInt(str.Substring(si, str.Length - si - 1));
                        return true;
                    }
                    return false;
                case SerializedPropertyType.Enum:
                    if (str?.StartsWith("Enum") ?? false)
                    {
                        //TODO
                    }
                    return false;
                case SerializedPropertyType.Vector2:
                    if (str?.StartsWith("Vector2(") ?? false)
                    {
                        int si = "Vector2(".Length;
                        value = ConvertUtil.ToVector2(str.Substring(si, str.Length - si - 1));
                        return true;
                    }
                    return false;
                case SerializedPropertyType.Vector3:
                    if (str?.StartsWith("Vector3(") ?? false)
                    {
                        int si = "Vector3(".Length;
                        value = ConvertUtil.ToVector3(str.Substring(si, str.Length - si - 1));
                        return true;
                    }
                    return false;
                case SerializedPropertyType.Vector4:
                    if (str?.StartsWith("Vector4(") ?? false)
                    {
                        int si = "Vector3(".Length;
                        value = ConvertUtil.ToVector4(str.Substring(si, str.Length - si - 1));
                        return true;
                    }
                    return false;
                case SerializedPropertyType.Rect:
                    if (str?.StartsWith("Rect(") ?? false)
                    {
                        int si = "Rect(".Length;
                        var v = ConvertUtil.ToVector4(str.Substring(si, str.Length - si - 1));
                        value = new Rect(v.x, v.y, v.z, v.w);
                        return true;
                    }
                    return false;
                case SerializedPropertyType.Character:
                    value = str?.Length > 0 ? str[0] : default(char);
                    return true;
                case SerializedPropertyType.AnimationCurve:
                    //TODO - do we want to support this?
                    return false;
                case SerializedPropertyType.Bounds:
                    if (str?.StartsWith("Bounds(") ?? false)
                    {
                        int si = "Bounds(".Length;
                        string sarr = str.Substring(si, str.Length - si - 1);
                        var arr = sarr.Replace(" ", "").Split(',');
                        var v1 = new Vector3(arr.Length > 0 ? ConvertUtil.ToSingle(arr[0]) : 0f,
                                             arr.Length > 1 ? ConvertUtil.ToSingle(arr[1]) : 0f,
                                             arr.Length > 2 ? ConvertUtil.ToSingle(arr[2]) : 0f);
                        var v2 = new Vector3(arr.Length > 3 ? ConvertUtil.ToSingle(arr[3]) : 0f,
                                             arr.Length > 4 ? ConvertUtil.ToSingle(arr[4]) : 0f,
                                             arr.Length > 5 ? ConvertUtil.ToSingle(arr[5]) : 0f);
                        var bounds = new Bounds();
                        bounds.center = v1;
                        bounds.extents = v2;
                        value = bounds;
                        return true;
                    }
                    return false;
                case SerializedPropertyType.Gradient:
                    //TODO - do we want to support this?
                    return false;
                case SerializedPropertyType.Quaternion:
                    if (str?.StartsWith("Quaternion(") ?? false)
                    {
                        int si = "Quaternion(".Length;
                        var v = ConvertUtil.ToVector4(str.Substring(si, str.Length - si - 1));
                        value = new Quaternion(v.x, v.y, v.z, v.w);
                        return true;
                    }
                    else if (str?.StartsWith("Vector3(") ?? false)
                    {
                        int si = "Vector3(".Length;
                        var v = ConvertUtil.ToVector3(str.Substring(si, str.Length - si - 1));
                        value = Quaternion.Euler(v.x, v.y, v.z);
                        return true;
                    }
                    return false;
                case SerializedPropertyType.FixedBufferSize:
                    //NEVER TODO - I dont give a fuck about this one.
                    return false;
                case SerializedPropertyType.Vector2Int:
                    if (str?.StartsWith("Vector2(") ?? false)
                    {
                        int si = "Vector2(".Length;
                        var v = ConvertUtil.ToVector2(str.Substring(si, str.Length - si - 1));
                        value = new Vector2Int((int)v.x, (int)v.y);
                        return true;
                    }
                    return false;
                case SerializedPropertyType.Vector3Int:
                    if (str?.StartsWith("Vector3(") ?? false)
                    {
                        int si = "Vector3(".Length;
                        var v = ConvertUtil.ToVector3(str.Substring(si, str.Length - si - 1));
                        value = new Vector3Int((int)v.x, (int)v.y, (int)v.z);
                        return true;
                    }
                    return false;
                case SerializedPropertyType.RectInt:
                    if (str?.StartsWith("Rect(") ?? false)
                    {
                        int si = "Rect(".Length;
                        var v = ConvertUtil.ToVector4(str.Substring(si, str.Length - si - 1));
                        value = new RectInt((int)v.x, (int)v.y, (int)v.z, (int)v.w);
                        return true;
                    }
                    return false;
                case SerializedPropertyType.BoundsInt:
                    if (str?.StartsWith("Bounds(") ?? false)
                    {
                        int si = "Bounds(".Length;
                        string sarr = str.Substring(si, str.Length - si - 1);
                        var arr = sarr.Replace(" ", "").Split(',');
                        var v1 = new Vector3Int(arr.Length > 0 ? ConvertUtil.ToInt(arr[0]) : 0,
                                                arr.Length > 1 ? ConvertUtil.ToInt(arr[1]) : 0,
                                                arr.Length > 2 ? ConvertUtil.ToInt(arr[2]) : 0);
                        var v2 = new Vector3Int(arr.Length > 3 ? ConvertUtil.ToInt(arr[3]) : 0,
                                                arr.Length > 4 ? ConvertUtil.ToInt(arr[4]) : 0,
                                                arr.Length > 5 ? ConvertUtil.ToInt(arr[5]) : 0);
                        var bounds = new BoundsInt();
                        bounds.position = v1;
                        bounds.size = v2;
                        value = bounds;
                        return true;
                    }
                    return false;
                case SerializedPropertyType.ManagedReference:
                    //TODO - do we want to support this?
                    return false;
                case SerializedPropertyType.Hash128:
                    // Handle Hash128
                    break;
                default:
                    return false;
            }
            return false;
        }


        public static GenericPropertyJsonWrapper ToGenericJsonWrapper(SerializedProperty property)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));

            Debug.Log($"name:{property.name}, type:{property.propertyType}, hasChildren:{property.hasChildren}, isArray:{property.isArray}");
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return new GenericPropertyJsonWrapper_Int()
                    {
                        name = property.name,
                        type = (int)property.propertyType,
                        val = property.intValue
                    };
                case SerializedPropertyType.ArraySize:
                    return new GenericPropertyJsonWrapper_Int()
                    {
                        name = property.name,
                        type = (int)property.propertyType,
                        val = property.intValue
                    };
                case SerializedPropertyType.Boolean:
                    return new GenericPropertyJsonWrapper_Bool()
                    {
                        name = property.name,
                        type = (int)property.propertyType,
                        val = property.boolValue
                    };
                case SerializedPropertyType.Float:
                    return new GenericPropertyJsonWrapper_Float()
                    {
                        name = property.name,
                        type = (int)property.propertyType,
                        val = property.floatValue
                    };
                case SerializedPropertyType.String:
                    return new GenericPropertyJsonWrapper_String()
                    {
                        name = property.name,
                        type = (int)property.propertyType,
                        val = Stringify(property)
                    };
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.ExposedReference:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Quaternion:
                    //SPECIAL CASE - some properties which 'hasChildren' is true still are stored flat per Unity behavior. Why? Who knows.
                    return new GenericPropertyJsonWrapper_String()
                    {
                        name = property.name,
                        type = (int)property.propertyType,
                        val = Stringify(property)
                    };
                case SerializedPropertyType.Color: //determined to be unwrapped as 'hasChildren'
                case SerializedPropertyType.LayerMask: //determined to be unwrapped as 'hasChildren'
                case SerializedPropertyType.Vector2: //determined to be unwrapped as 'hasChildren'
                case SerializedPropertyType.Vector3: //determined to be unwrapped as 'hasChildren'
                case SerializedPropertyType.Vector4: //determined to be unwrapped as 'hasChildren'
                //TODO - determine if the following are stringified or unwrapped... for now assuming unwrapped
                //case SerializedPropertyType.Rect:
                //case SerializedPropertyType.Character:
                //case SerializedPropertyType.AnimationCurve:
                //case SerializedPropertyType.Bounds:
                //case SerializedPropertyType.Gradient:
                //case SerializedPropertyType.FixedBufferSize:
                //case SerializedPropertyType.Vector2Int:
                //case SerializedPropertyType.Vector3Int:
                //case SerializedPropertyType.RectInt:
                //case SerializedPropertyType.BoundsInt:
                default:
                    if (property.isArray)
                    {
                        var lst = new List<GenericPropertyJsonWrapper>();
                        var iterator = property.Copy();
                        var end = property.GetEndProperty();
                        for (bool enterChildren = true; iterator.Next(enterChildren); enterChildren = false)
                        {
                            if (SerializedProperty.EqualContents(iterator, end))
                                break;
                            lst.Add(ToGenericJsonWrapper(iterator));
                        }
                        return new GenericPropertyJsonWrapper_Array()
                        {
                            name = property.name,
                            type = (int)property.propertyType,
                            arraySize = property.arraySize,
                            arrayType = property.arrayElementType,
                            children = lst.ToArray()
                        };
                    }
                    else if (property.hasChildren)
                    {
                        var lst = new List<GenericPropertyJsonWrapper>();
                        var iterator = property.Copy();
                        var end = property.GetEndProperty();
                        for (bool enterChildren = true; iterator.Next(enterChildren); enterChildren = false)
                        {
                            if (SerializedProperty.EqualContents(iterator, end))
                                break;
                            lst.Add(ToGenericJsonWrapper(iterator));
                        }
                        return new GenericPropertyJsonWrapper_HasChildren()
                        {
                            name = property.name,
                            type = (int)property.propertyType,
                            children = lst.ToArray()
                        };
                    }
                    else
                    {
                        return new GenericPropertyJsonWrapper_String()
                        {
                            name = property.name,
                            type = (int)property.propertyType,
                            val = Stringify(property)
                        };
                    }
            }
        }




        public interface IJsonWrapper
        {
            string Stringify();
        }

        [System.Serializable]
        public class UnityObjectJsonWrapper : IJsonWrapper
        {
            public string guid = string.Empty;
            public ulong localId;
            public int type;
            public int instanceID;

            public UnityObjectJsonWrapper(UnityEngine.Object obj)
            {
                if (!obj)
                {
                    //do nothing
                }
                else if (AssetDatabase.Contains(obj))
                {
                    var glib = obj ? GlobalObjectId.GetGlobalObjectIdSlow(obj) : default;
                    this.guid = glib.assetGUID.ToString();
                    this.localId = glib.targetObjectId;
                    this.type = glib.identifierType;
                    this.instanceID = obj.GetInstanceID();
                }
                else
                {
                    this.guid = string.Empty;
                    this.localId = 0;
                    this.type = 0; //TODO - unity does 0 in copy-paste, but the documentation suggests that this should be 2 since 2 is a scene object
                    this.instanceID = obj.GetInstanceID();
                }
            }

            public string Stringify() => $"UnityEditor.ObjectWrapperJSON:{EditorJsonUtility.ToJson(this)}";

        }


        [System.Serializable]
        public class GenericPropertyJsonWrapper : IJsonWrapper
        {
            public string name;
            public int type;
            public string Stringify() => "GenericPropertyJSON:" + this.Jsonify();

            public virtual string Jsonify()
            {
                var sb = StringUtil.GetTempStringBuilder();
                sb.Append("GenericPropertyJSON:");
                sb.Append("{");
                sb.Append($"\"name\":\"{this.name}\",\"type\":-1");
                sb.Append("}");
                return StringUtil.Release(sb);
            }

        }

        [System.Serializable]
        public class GenericPropertyJsonWrapper_Array : GenericPropertyJsonWrapper
        {
            public int arraySize;
            public string arrayType;
            public GenericPropertyJsonWrapper[] children;

            public override string Jsonify()
            {
                var sb = StringUtil.GetTempStringBuilder();
                sb.Append("{");
                sb.Append($"\"name\":\"{this.name}\",\"type\":{this.type},\"arraySize\":{this.arraySize},\"arrayType\":\"{this.arrayType}\",\"children\":[");
                if (children?.Length > 0)
                {
                    sb.Append(children[0]?.Jsonify());
                    for (int i = 1; i < children.Length; i++)
                    {
                        sb.Append(",");
                        sb.Append(children[i]?.Jsonify());
                    }
                }
                sb.Append("]}");
                return StringUtil.Release(sb);
            }
        }

        [System.Serializable]
        public class GenericPropertyJsonWrapper_HasChildren : GenericPropertyJsonWrapper
        {
            public GenericPropertyJsonWrapper[] children;

            public override string Jsonify()
            {
                var sb = StringUtil.GetTempStringBuilder();
                sb.Append("{");
                sb.Append($"\"name\":\"{this.name}\",\"type\":{this.type},\"children\":[");
                if (children?.Length > 0)
                {
                    sb.Append(children[0]?.Jsonify());
                    for (int i = 1; i < children.Length; i++)
                    {
                        sb.Append(",");
                        sb.Append(children[i]?.Jsonify());
                    }
                }
                sb.Append("]}");
                return StringUtil.Release(sb);
            }
        }

        public class GenericPropertyJsonWrapper_String : GenericPropertyJsonWrapper
        {
            public string val;
            public override string Jsonify() => EditorJsonUtility.ToJson(this);
        }
        public class GenericPropertyJsonWrapper_Int : GenericPropertyJsonWrapper
        {
            public int val;
            public override string Jsonify() => EditorJsonUtility.ToJson(this);
        }
        public class GenericPropertyJsonWrapper_Float : GenericPropertyJsonWrapper
        {
            public float val;
            public override string Jsonify() => EditorJsonUtility.ToJson(this);
        }
        public class GenericPropertyJsonWrapper_Bool : GenericPropertyJsonWrapper
        {
            public bool val;
            public override string Jsonify() => EditorJsonUtility.ToJson(this);
        }





        [System.Serializable]
        public class GenericUnwrapper
        {
            public string name;
            public int type;
            public int arraySize;
            public string arrayType;
            public string val;
            public GenericUnwrapper[] children;

            public bool CopyTo(SerializedProperty property)
            {
                if ((int)property.propertyType != this.type) return false;

                if (!string.IsNullOrEmpty(this.val))
                {
                    //simple value, paste it to the property
                    return TryPaste(this.val, property);
                }
                else if (!string.IsNullOrEmpty(this.arrayType))
                {
                    //we're in an array... these are weird cause of how they line up
                    if (this.name == "Array")
                    {
                        //we're in the array proper, line up the property if necessary and paste
                        if (!property.isArray) return false;

                        int len = this.children?.Length ?? 0;
                        len = Mathf.Max(len - 1, 0); //arrays have a dummy 'size' attribute on the front
                        property.arraySize = len;
                        for (int i = 0; i < len; i++)
                        {
                            this.children[i + 1].CopyTo(property.GetArrayElementAtIndex(i));
                        }
                        return true;
                    }
                    else if (this.children?.Length == 1 && property.hasChildren)
                    {
                        //we're in the weird dummy parent SerializedProperty of the array, we need to continue down
                        return this.children[0]?.CopyTo(property) ?? false;
                    }
                }
                else if (property.hasChildren && this.children?.Length > 0)
                {
                    var iterator = property.Copy();
                    var end = property.GetEndProperty();
                    int i = 0;
                    for (bool enterChildren = true; iterator.Next(enterChildren); enterChildren = false)
                    {
                        if (SerializedProperty.EqualContents(iterator, end))
                            break;
                        if (i < this.children.Length)
                        {
                            this.children[i].CopyTo(iterator);
                        }
                        i++;
                    }
                }

                return false;
            }

        }

    }
#endif

}
