using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

using com.spacepuppyeditor.Internal;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(ReorderableArrayAttribute))]
    public class ReorderableArrayPropertyDrawer : PropertyDrawer, IArrayHandlingPropertyDrawer
    {

        public event System.EventHandler ElementAdded;

        public delegate string FormatElementLabelCallback(SerializedProperty property, int index, bool isActive, bool isFocused);

        private static readonly float TOP_PAD = 2f + EditorGUIUtility.singleLineHeight;
        private const float BOTTOM_PAD = 2f;
        private const float MARGIN = 2f;
        private const float LENGTHFIIELD_WIDTH = 50f;
        private const float LENGTHFIIELD_MARGIN = 5f;

        #region Fields

        public GUIContent CustomLabel;
        private CachedReorderableList _lst;
        private GUIContent _labelContent;
        private bool _disallowFoldout;
        private bool _removeBackgroundWhenCollapsed;
        private bool _draggable = true;
        private bool _drawElementAtBottom;
        private bool _hideElementLabel = false;
        private string _childPropertyAsLabel;
        private string _childPropertyAsEntry;
        private string _elementLabelFormatString;
        private float _elementPadding;
        private bool _allowDragAndDrop = true;
        private bool _allowDragAndDropSceneObjects = true;
        private bool _showTooltipInHeader;
        private bool _hideLengthField;

        private PropertyDrawer _internalDrawer;

        #endregion

        #region CONSTRUCTOR

        public ReorderableArrayPropertyDrawer()
        {

        }

        /// <summary>
        /// Use this to set the element type of the list for drag & drop, if you're manually calling the drawer.
        /// </summary>
        /// <param name="elementType"></param>
        public ReorderableArrayPropertyDrawer(System.Type dragDropElementType)
        {
            this.DragDropElementType = dragDropElementType;
        }



        protected virtual CachedReorderableList GetList(SerializedProperty property, GUIContent label)
        {
            var lst = CachedReorderableList.GetListDrawer(property, _maskList_DrawHeader, _maskList_DrawElement, _maskList_OnElementAdded);
            lst.draggable = this.Draggable;

            if (property.arraySize > 0)
            {
                if (this.DrawElementAtBottom)
                {
                    lst.elementHeight = EditorGUIUtility.singleLineHeight;
                }
                else
                {
                    var pchild = property.GetArrayElementAtIndex(0);
                    /*
                    if (_internalDrawer != null)
                    {
                        lst.elementHeight = _internalDrawer.GetPropertyHeight(pchild, label);
                    }
                    else if (ElementIsFlatChildField(pchild))
                    {
                        //we don't draw this way if it's a built-in type from Unity
                        pchild.isExpanded = true;
                        if (_hideElementLabel)
                        {
                            lst.elementHeight = SPEditorGUI.GetDefaultPropertyHeight(pchild, label, true) + 2f - EditorGUIUtility.singleLineHeight;
                        }
                        else
                        {
                            lst.elementHeight = SPEditorGUI.GetDefaultPropertyHeight(pchild, label, true) + 2f; //height when showing label
                        }
                    }
                    else
                    {
                        lst.elementHeight = SPEditorGUI.GetDefaultPropertyHeight(pchild, label) + 1f;
                    }
                    */
                    lst.elementHeight = this.GetElementHeight(pchild, label, false) + 2f;
                }
            }
            else
            {
                lst.elementHeight = EditorGUIUtility.singleLineHeight;
            }

            return lst;
        }

        private void StartOnGUI(SerializedProperty property, GUIContent label)
        {
            _labelContent = label;

            _lst = this.GetList(property, label);
            if (_lst.index >= _lst.count) _lst.index = -1;

            if (this.fieldInfo != null)
            {
                this.DragDropElementType = TypeUtil.GetElementTypeOfListType(this.fieldInfo.FieldType);

                if (!string.IsNullOrEmpty(this.ChildPropertyAsEntry) && this.DragDropElementType != null)
                {
                    var field = this.DragDropElementType.GetMember(this.ChildPropertyAsEntry,
                                                                   System.Reflection.MemberTypes.Field,
                                                                   System.Reflection.BindingFlags.Public |
                                                                   System.Reflection.BindingFlags.NonPublic |
                                                                   System.Reflection.BindingFlags.Instance).FirstOrDefault() as System.Reflection.FieldInfo;
                    if (field != null) this.DragDropElementType = field.FieldType;
                }
            }
        }

        private void EndOnGUI(SerializedProperty property, GUIContent label)
        {
            //_lst.serializedProperty = null;
            _labelContent = null;
        }

        #endregion

        #region Properties

        /*
         * Current State
         */

        public ReorderableList CurrentReorderableList { get { return _lst; } }

        public SerializedProperty CurrentArrayProperty { get { return _lst != null ? _lst.serializedProperty : null; } }

        /// <summary>
        /// During the drawelement callback this is set to the currently drawn array index. A custom 'InternalDrawer' can read this property during its OnGUI event to get the index it's on.
        /// </summary>
        public int CurrentDrawingArrayElementIndex { get; private set; }

        /*
         * Configuration
         */

        public bool DisallowFoldout
        {
            get { return (this.attribute as ReorderableArrayAttribute)?.DisallowFoldout ?? _disallowFoldout; }
            set { _disallowFoldout = value; }
        }

        public bool RemoveBackgroundWhenCollapsed
        {
            get { return (this.attribute as ReorderableArrayAttribute)?.RemoveBackgroundWhenCollapsed ?? _removeBackgroundWhenCollapsed; }
            set { _removeBackgroundWhenCollapsed = value; }
        }

        public bool Draggable
        {
            get { return (this.attribute as ReorderableArrayAttribute)?.Draggable ?? _draggable; }
            set { _draggable = value; }
        }

        public bool DrawElementAtBottom
        {
            get { return (this.attribute as ReorderableArrayAttribute)?.DrawElementAtBottom ?? _drawElementAtBottom; }
            set { _drawElementAtBottom = value; }
        }

        public bool HideElementLabel
        {
            get { return (this.attribute as ReorderableArrayAttribute)?.HideElementLabel ?? _hideElementLabel; }
            set { _hideElementLabel = value; }
        }

        public string ChildPropertyAsLabel
        {
            get { return (this.attribute as ReorderableArrayAttribute)?.ChildPropertyToDrawAsElementLabel ?? _childPropertyAsLabel; }
            set { _childPropertyAsLabel = value; }
        }

        public string ChildPropertyAsEntry
        {
            get { return (this.attribute as ReorderableArrayAttribute)?.ChildPropertyToDrawAsElementEntry ?? _childPropertyAsEntry; }
            set { _childPropertyAsEntry = value; }
        }

        public string ElementLabelFormatString
        {
            get { return (this.attribute as ReorderableArrayAttribute)?.ElementLabelFormatString ?? _elementLabelFormatString; }
            set { _elementLabelFormatString = value; }
        }

        public float ElementPadding
        {
            get { return (this.attribute as ReorderableArrayAttribute)?.ElementPadding ?? _elementPadding; }
            set { _elementPadding = value; }
        }

        public FormatElementLabelCallback FormatElementLabel
        {
            get;
            set;
        }

        /// <summary>
        /// Can drag entries onto the inspector without needing to click + button. Only works for array/list of UnityEngine.Object sub/types.
        /// </summary>
        public bool AllowDragAndDrop
        {
            get { return (this.attribute as ReorderableArrayAttribute)?.AllowDragAndDrop ?? _allowDragAndDrop; }
            set { _allowDragAndDrop = value; }
        }

        public bool AllowDragAndDropSceneObjects
        {
            get { return (this.attribute as ReorderableArrayAttribute)?.AllowDragAndDropSceneObjects ?? _allowDragAndDropSceneObjects; }
            set { _allowDragAndDropSceneObjects = value; }
        }

        public bool ShowTooltipInHeader
        {
            get { return (this.attribute as ReorderableArrayAttribute)?.ShowTooltipInHeader ?? _showTooltipInHeader; }
            set { _showTooltipInHeader = value; }
        }

        public bool HideLengthField
        {
            get => (this.attribute as ReorderableArrayAttribute)?.HideLengthField ?? _hideLengthField;
            set => _hideLengthField = value;
        }

        /// <summary>
        /// The type of the element in the array/list, will effect drag & drop filtering (unless overriden).
        /// </summary>
        public System.Type DragDropElementType
        {
            get;
            set;
        }

        public System.Func<UnityEngine.Object, UnityEngine.Object> DragDropElementFilter { get; set; }

        #endregion

        #region OnGUI

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h;
            if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;

            if (property.isArray)
            {
                this.StartOnGUI(property, label);
                if (this.DisallowFoldout || property.isExpanded)
                {
                    h = _lst.GetHeight();
                    if (this.DrawElementAtBottom && _lst.index >= 0 && _lst.index < property.arraySize)
                    {
                        var pchild = property.GetArrayElementAtIndex(_lst.index);
                        /*
                        if (_internalDrawer != null)
                        {
                            h += _internalDrawer.GetPropertyHeight(pchild, label) + BOTTOM_PAD + TOP_PAD;
                        }
                        else if (ElementIsFlatChildField(pchild))
                        {
                            //we don't draw this way if it's a built-in type from Unity
                            pchild.isExpanded = true;
                            h += SPEditorGUI.GetDefaultPropertyHeight(pchild, label, true) + BOTTOM_PAD + TOP_PAD - EditorGUIUtility.singleLineHeight;
                        }
                        else
                        {
                            h += SPEditorGUI.GetDefaultPropertyHeight(pchild, label, false) + BOTTOM_PAD + TOP_PAD;
                        }
                        */
                        h += this.GetElementHeight(pchild, label, true) + BOTTOM_PAD + TOP_PAD;
                    }
                }
                else
                {
                    h = EditorGUIUtility.singleLineHeight;
                }
            }
            else
            {
                h = EditorGUIUtility.singleLineHeight;
            }
            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            this.CurrentDrawingArrayElementIndex = -1;
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

            if (property.isArray)
            {
                if (this.CustomLabel != null)
                {
                    label = this.CustomLabel;
                }
                else if (label != null)
                {
                    label = label.Clone();
                    if (this.ShowTooltipInHeader)
                    {
                        label.text = string.Format("{0} [{1:0}] - {2}", label.text, property.arraySize, (string.IsNullOrEmpty(label.tooltip) ? property.tooltip : label.tooltip));
                    }
                    else
                    {
                        label.text = string.Format("{0} [{1:0}]", label.text, property.arraySize);
                    }

                    if (string.IsNullOrEmpty(label.tooltip)) label.tooltip = property.tooltip;
                }
                else
                {
                    label = EditorHelper.TempContent(property.displayName, property.tooltip);
                }

                //const float WIDTH_FOLDOUT = 5f;
                var foldoutRect = new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight);
                var lengthFieldRect = new Rect(position.xMax - (LENGTHFIIELD_WIDTH + LENGTHFIIELD_MARGIN), position.yMin, LENGTHFIIELD_WIDTH, EditorGUIUtility.singleLineHeight);
                position = EditorGUI.IndentedRect(position);
                Rect listArea = position;

                if (this.DisallowFoldout)
                {
                    this.StartOnGUI(property, label);
                    listArea = new Rect(position.xMin, position.yMin, position.width, _lst.GetHeight());
                    //_lst.DoList(EditorGUI.IndentedRect(position));
                    _lst.DoList(listArea);
                    this.EndOnGUI(property, label);
                }
                else
                {
                    if (property.isExpanded)
                    {
                        this.StartOnGUI(property, label);
                        listArea = new Rect(position.xMin, position.yMin, position.width, _lst.GetHeight());
                        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none);
                        //_lst.DoList(EditorGUI.IndentedRect(position));
                        _lst.DoList(listArea);
                        this.EndOnGUI(property, label);
                    }
                    else
                    {
                        if (this.RemoveBackgroundWhenCollapsed)
                        {
                            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);
                        }
                        else
                        {
                            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none);
                            //ReorderableListHelper.DrawRetractedHeader(EditorGUI.IndentedRect(position), label);
                            ReorderableListHelper.DrawRetractedHeader(position, label);
                        }
                    }
                }

                if (!this.HideLengthField)
                {
                    var style = new GUIStyle(GUI.skin.textField);
                    style.alignment = TextAnchor.MiddleRight;
                    property.arraySize = EditorGUI.DelayedIntField(lengthFieldRect, property.arraySize, style);
                }

                this.DoDragAndDrop(property, listArea);

                if (property.isExpanded && this.DrawElementAtBottom && _lst.index >= 0 && _lst.index < property.arraySize)
                {
                    var pchild = property.GetArrayElementAtIndex(_lst.index);
                    var label2 = TempElementLabel(pchild, _lst.index); //(string.IsNullOrEmpty(_childPropertyAsLabel)) ? TempElementLabel(_lst.index) : GUIContent.none;

                    pchild.isExpanded = true;
                    float h;
                    if (_internalDrawer != null)
                    {
                        h = _internalDrawer.GetPropertyHeight(pchild, label2) + BOTTOM_PAD + TOP_PAD;
                    }
                    else if (pchild.hasChildren)
                    {
                        h = SPEditorGUI.GetDefaultPropertyHeight(pchild, label, true) + BOTTOM_PAD + TOP_PAD - EditorGUIUtility.singleLineHeight;
                    }
                    else
                    {
                        h = SPEditorGUI.GetDefaultPropertyHeight(pchild, label2, true) + BOTTOM_PAD + TOP_PAD;
                    }
                    var area = new Rect(position.xMin, position.yMax - h, position.width, h);
                    var drawArea = new Rect(area.xMin, area.yMin + TOP_PAD, area.width - MARGIN, area.height - TOP_PAD);

                    GUI.BeginGroup(area, label2, GUI.skin.box);
                    GUI.EndGroup();

                    EditorGUI.indentLevel++;
                    if (_internalDrawer != null)
                    {
                        _internalDrawer.OnGUI(drawArea, pchild, label2);
                    }
                    else if (pchild.hasChildren)
                    {
                        SPEditorGUI.FlatChildPropertyField(drawArea, pchild);
                    }
                    else
                    {
                        SPEditorGUI.DefaultPropertyField(drawArea, pchild, GUIContent.none, false);
                    }
                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                SPEditorGUI.DefaultPropertyField(position, property, label, false);
            }

            this.CurrentDrawingArrayElementIndex = -1;
        }

        #endregion

        #region Masks ReorderableList Handlers

        protected virtual void _maskList_DrawHeader(Rect area)
        {
            if (_labelContent != null)
            {
                EditorGUI.LabelField(area, _labelContent);
            }
        }

        protected virtual void _maskList_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            var element = _lst.serializedProperty.GetArrayElementAtIndex(index);
            if (element == null) return;

            var label = this.GetFormattedElementLabel(area, index, isActive, isFocused);
            this.DrawElement(area, element, label, index);

            if (GUI.enabled) ReorderableListHelper.DrawDraggableElementDeleteContextMenu(_lst, area, index, isActive, isFocused);
        }

        private void _maskList_OnElementAdded(ReorderableList lst)
        {
            lst.serializedProperty.arraySize++;
            lst.index = lst.serializedProperty.arraySize - 1;

            var attrib = this.attribute as ReorderableArrayAttribute;
            if (attrib != null && !string.IsNullOrEmpty(attrib.OnAddCallback))
            {
                lst.serializedProperty.serializedObject.ApplyModifiedProperties();

                var prop = lst.serializedProperty.GetArrayElementAtIndex(lst.index);
                var obj = EditorHelper.GetTargetObjectOfProperty(prop);
                obj = com.spacepuppy.Dynamic.DynamicUtil.InvokeMethod(lst.serializedProperty.serializedObject.targetObject, attrib.OnAddCallback, obj);
                EditorHelper.SetTargetObjectOfProperty(prop, obj);
                lst.serializedProperty.serializedObject.Update();
            }

            this.OnElementAdded(lst);
        }

        protected virtual void OnElementAdded(ReorderableList lst)
        {
            var d = this.ElementAdded;
            if (d != null) d(this, System.EventArgs.Empty);
        }

        protected virtual GUIContent GetFormattedElementLabel(Rect area, int index, bool isActive, bool isFocused)
        {
            var element = _lst.serializedProperty.GetArrayElementAtIndex(index);
            if (element == null) return GUIContent.none;

            GUIContent label = null;
            if (this.FormatElementLabel != null)
            {
                string slbl = this.FormatElementLabel(element, index, isActive, isFocused);
                if (slbl != null) label = EditorHelper.TempContent(slbl);
            }
            else if (!string.IsNullOrEmpty(this.ElementLabelFormatString))
            {
                label = EditorHelper.TempContent(string.Format(this.ElementLabelFormatString, index));
            }
            if (this.ElementPadding > 0f)
            {
                area = new Rect(area.xMin + this.ElementPadding, area.yMin, Mathf.Max(0f, area.width - this.ElementPadding), area.height);
            }
            if (label == null) label = (this.HideElementLabel) ? GUIContent.none : TempElementLabel(element, index);

            return label;
        }

        protected virtual void DrawElement(Rect area, SerializedProperty element, GUIContent label, int elementIndex)
        {
            this.CurrentDrawingArrayElementIndex = elementIndex;
            if (this.DrawElementAtBottom)
            {
                SerializedProperty prop = string.IsNullOrEmpty(this.ChildPropertyAsEntry) ? null : element.FindPropertyRelative(this.ChildPropertyAsEntry);

                if (prop != null)
                {
                    SPEditorGUI.PropertyField(area, prop, label);
                }
                else
                {
                    EditorGUI.LabelField(area, label);
                }
            }
            else
            {
                if (_internalDrawer != null)
                {
                    _internalDrawer.OnGUI(area, element, label);
                }
                else if (ElementIsFlatChildField(element))
                {
                    //we don't draw this way if it's a built-in type from Unity

                    if (this.HideElementLabel)
                    {
                        //no label
                        SPEditorGUI.FlatChildPropertyField(area, element);
                    }
                    else
                    {
                        //showing label
                        var labelArea = new Rect(area.xMin, area.yMin, area.width, EditorGUIUtility.singleLineHeight);
                        EditorGUI.LabelField(labelArea, label);
                        var childArea = new Rect(area.xMin, area.yMin + EditorGUIUtility.singleLineHeight + 1f, area.width, area.height - EditorGUIUtility.singleLineHeight);
                        SPEditorGUI.FlatChildPropertyField(childArea, element);
                    }
                }
                else
                {
                    SPEditorGUI.DefaultPropertyField(area, element, label, false);
                }
            }
        }

        protected virtual float GetElementHeight(SerializedProperty element, GUIContent label, bool elementIsAtBottom)
        {
            if (_internalDrawer != null)
            {
                return _internalDrawer.GetPropertyHeight(element, label);
            }
            else if (ElementIsFlatChildField(element))
            {
                //we don't draw this way if it's a built-in type from Unity
                element.isExpanded = true;
                if (this.HideElementLabel || elementIsAtBottom)
                {
                    return SPEditorGUI.GetDefaultPropertyHeight(element, label, true) - EditorGUIUtility.singleLineHeight;
                }
                else
                {
                    return SPEditorGUI.GetDefaultPropertyHeight(element, label, true);
                }
            }
            else
            {
                return SPEditorGUI.GetDefaultPropertyHeight(element, label, false);
            }
        }

        #endregion

        #region Drag & Drop

        protected virtual void DoDragAndDrop(SerializedProperty property, Rect listArea)
        {
            if (this.AllowDragAndDrop && (this.DragDropElementType != null || this.DragDropElementFilter != null) && Event.current != null)
            {
                var ev = Event.current;
                switch (ev.type)
                {
                    case EventType.DragUpdated:
                    case EventType.DragPerform:
                        {
                            if (listArea.Contains(ev.mousePosition))
                            {
                                IEnumerable<UnityEngine.Object> refsource;
                                if (this.AllowDragAndDropSceneObjects)
                                {
                                    refsource = DragAndDrop.objectReferences;
                                }
                                else
                                {
                                    refsource = DragAndDrop.objectReferences.Where(o => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o)));
                                }

                                IEnumerable<object> refs;
                                if (this.DragDropElementFilter != null)
                                {
                                    refs = (from o in refsource let obj = this.DragDropElementFilter(o) where obj != null select obj);
                                }
                                else
                                {
                                    refs = (from o in refsource let obj = ObjUtil.GetAsFromSource(this.DragDropElementType, o, false) where obj != null select obj);
                                }

                                DragAndDrop.visualMode = refs.Any() ? DragAndDropVisualMode.Link : DragAndDropVisualMode.Rejected;

                                if (ev.type == EventType.DragPerform && refs.Any())
                                {
                                    DragAndDrop.AcceptDrag();
                                    AddObjectsToArray(property, refs.ToArray(), this.ChildPropertyAsEntry);
                                    GUI.changed = true;
                                }
                            }
                        }
                        break;
                }
            }
        }

        #endregion


        protected GUIContent TempElementLabel(SerializedProperty element, int index)
        {
            var target = EditorHelper.GetTargetObjectOfProperty(element);
            string slbl = ConvertUtil.ToString(com.spacepuppy.Dynamic.DynamicUtil.GetValue(target, this.ChildPropertyAsLabel));

            if (string.IsNullOrEmpty(slbl))
            {
                var propLabel = (!string.IsNullOrEmpty(this.ChildPropertyAsLabel)) ? element.FindPropertyRelative(this.ChildPropertyAsLabel) : null;
                if (propLabel != null)
                    slbl = ConvertUtil.ToString(EditorHelper.GetPropertyValue(propLabel));
            }

            if (string.IsNullOrEmpty(slbl))
                slbl = string.Format("Element {0:00}", index);

            return EditorHelper.TempContent(slbl);
        }

        #region IArrayHandlingPropertyDrawer Interface

        public PropertyDrawer InternalDrawer
        {
            get
            {
                return _internalDrawer;
            }
            set
            {
                _internalDrawer = value;
            }
        }

        #endregion

        #region Static Utils

        protected static bool ElementIsFlatChildField(SerializedProperty property)
        {
            //return property.hasChildren && property.objectReferenceValue is MonoBehaviour;
            return property.hasChildren && property.propertyType == SerializedPropertyType.Generic;
        }

        private static void AddObjectsToArray(SerializedProperty listProp, object[] objs, string optionalChildProp = null)
        {
            if (listProp == null) throw new System.ArgumentNullException("listProp");
            if (!listProp.isArray) throw new System.ArgumentException("Must be a SerializedProperty for an array/list.", "listProp");
            if (objs == null || objs.Length == 0) return;

            try
            {
                int start = listProp.arraySize;
                listProp.arraySize += objs.Length;
                for (int i = 0; i < objs.Length; i++)
                {
                    var element = listProp.GetArrayElementAtIndex(start + i);
                    if (!string.IsNullOrEmpty(optionalChildProp)) element = element.FindPropertyRelative(optionalChildProp);

                    if (element != null && element.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        element.objectReferenceValue = objs[i] as UnityEngine.Object;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        #endregion

    }
}
