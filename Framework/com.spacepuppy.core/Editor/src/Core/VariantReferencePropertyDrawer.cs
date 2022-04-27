using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;

using com.spacepuppyeditor.Core;
using com.spacepuppy.Collections;
using com.spacepuppy.Events;
using System.Reflection;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(VariantReference))]
    public class VariantReferencePropertyDrawer : PropertyDrawer
    {

        public const string PROP_MODE = "_mode";
        public const string PROP_TYPE = "_type";
        public const string PROP_X = "_x";
        public const string PROP_Y = "_y";
        public const string PROP_Z = "_z";
        public const string PROP_W = "_w";
        public const string PROP_STRING = "_string";
        public const string PROP_OBJREF = "_unityObjectReference";

        #region Fields

        private const float REF_SELECT_WIDTH = 70f;

        public bool RestrictVariantType = false;

        private VariantType _variantTypeRestrictedTo;
        private System.Type _typeRestrictedTo = null;

        private System.Type _forcedObjectType = typeof(object);

        protected readonly EditorVariantReference _helper = new EditorVariantReference();
        private SelectableObjectPropertyDrawer _selectObjectPropertyDrawer = new SelectableObjectPropertyDrawer();

        #endregion

        #region Properties

        public VariantType VariantTypeRestrictedTo
        {
            get { return _variantTypeRestrictedTo; }
        }

        /// <summary>
        /// The type that dictates what the VariantReference is restricted to. 
        /// The property 'VariantTypeRestrictedTo' will be udpated to reflect this type's category in VarianteReference.
        /// </summary>
        public System.Type TypeRestrictedTo
        {
            get { return _typeRestrictedTo; }
            set
            {
                _typeRestrictedTo = value ?? typeof(object);
                _variantTypeRestrictedTo = value != null ? VariantReference.GetVariantType(_typeRestrictedTo) : VariantType.Null;
            }
        }

        /// <summary>
        /// The type of objects that the Object/Component selector field is restricted to. 
        /// This limits what is allowed to be dragged onto, or display in the popup.
        /// </summary>
        public System.Type ForcedObjectType
        {
            get { return _forcedObjectType; }
            set
            {
                _forcedObjectType = value ?? typeof(object);
            }
        }

        #endregion

        #region Methods

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var labelWidthCache = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = Mathf.Max(0f, labelWidthCache - REF_SELECT_WIDTH);
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);
            EditorHelper.SuppressIndentLevel();
            this.DrawValueField(position, property);
            EditorHelper.ResumeIndentLevel();

            EditorGUI.EndProperty();
            EditorGUIUtility.labelWidth = labelWidthCache;

            this.HandleDragDrop(position, property);
        }

        public virtual void DrawValueField(Rect position, SerializedProperty property)
        {
            CopyValuesToHelper(property, _helper);

            //draw ref selection
            position = this.DrawRefModeSelectionDropDown(position, property, _helper);

            //draw value
            switch (_helper._mode)
            {
                case VariantReference.RefMode.Value:
                    {
                        EditorGUI.BeginChangeCheck();
                        this.DrawValueFieldInValueMode(position, property, _helper);
                        if (EditorGUI.EndChangeCheck())
                        {
                            CopyValuesFromHelper(property, _helper);
                        }
                    }
                    break;
                case VariantReference.RefMode.Property:
                    this.DrawValueFieldInPropertyMode(position, property, _helper);
                    break;
                case VariantReference.RefMode.Eval:
                    this.DrawValueFieldInEvalMode(position, property, _helper);
                    break;
            }
        }

        protected virtual Rect DrawRefModeSelectionDropDown(Rect position, SerializedProperty property, EditorVariantReference helper)
        {
            var r0 = new Rect(position.xMin, position.yMin, Mathf.Min(REF_SELECT_WIDTH, position.width), position.height);

            EditorGUI.BeginChangeCheck();
            var mode = (VariantReference.RefMode)EditorGUI.EnumPopup(r0, GUIContent.none, _helper._mode);
            if (EditorGUI.EndChangeCheck())
            {
                _helper.PrepareForRefModeChange(mode);
                CopyValuesFromHelper(property, helper);
            }

            return new Rect(r0.xMax, r0.yMin, position.width - r0.width, r0.height);
        }

        protected virtual void DrawValueFieldInValueMode(Rect position, SerializedProperty property, EditorVariantReference helper)
        {
            var variant = helper as VariantReference;

            if (this.RestrictVariantType && helper._type != this.VariantTypeRestrictedTo)
            {
                helper.PrepareForValueTypeChange(this.VariantTypeRestrictedTo);
                GUI.changed = true; //force change
            }

            var r0 = new Rect(position.xMin, position.yMin, 90.0f, EditorGUIUtility.singleLineHeight);
            var r1 = new Rect(r0.xMax, position.yMin, position.xMax - r0.xMax, EditorGUIUtility.singleLineHeight);

            var cache = SPGUI.DisableIf(this.RestrictVariantType);
            EditorGUI.BeginChangeCheck();
            var valueType = variant.ValueType;
            valueType = (VariantType)EditorGUI.EnumPopup(r0, GUIContent.none, valueType);
            if (EditorGUI.EndChangeCheck())
            {
                helper.PrepareForValueTypeChange(valueType);
            }
            cache.Reset();

            if (_typeRestrictedTo?.IsEnum ?? false)
            {
                variant.IntValue = ConvertUtil.ToInt(EditorGUI.EnumPopup(r1, ConvertUtil.ToEnumOfType(_typeRestrictedTo, variant.IntValue)));
            }
            else
            {
                switch (valueType)
                {
                    case VariantType.Null:
                        cache = SPGUI.Disable();
                        EditorGUI.TextField(r1, "Null");
                        cache.Reset();
                        break;
                    case VariantType.String:
                        variant.StringValue = EditorGUI.TextField(r1, variant.StringValue);
                        break;
                    case VariantType.Boolean:
                        variant.BoolValue = EditorGUI.Toggle(new Rect(r1.xMin + 10f, r1.yMin, r1.width - 10f, r1.height), variant.BoolValue);
                        break;
                    case VariantType.Integer:
                        variant.IntValue = EditorGUI.IntField(r1, variant.IntValue);
                        break;
                    case VariantType.Float:
                        variant.FloatValue = EditorGUI.FloatField(r1, variant.FloatValue);
                        break;
                    case VariantType.Double:
                        //variant.DoubleValue = ConvertUtil.ToDouble(EditorGUI.TextField(r1, variant.DoubleValue.ToString()));
                        variant.DoubleValue = EditorGUI.DoubleField(r1, variant.DoubleValue);
                        break;
                    case VariantType.Vector2:
                        variant.Vector2Value = EditorGUI.Vector2Field(r1, GUIContent.none, variant.Vector2Value);
                        break;
                    case VariantType.Vector3:
                        variant.Vector3Value = EditorGUI.Vector3Field(r1, GUIContent.none, variant.Vector3Value);
                        break;
                    case VariantType.Vector4:
                        variant.Vector4Value = EditorGUI.Vector4Field(r1, (string)null, variant.Vector4Value);
                        break;
                    case VariantType.Quaternion:
                        variant.QuaternionValue = SPEditorGUI.QuaternionField(r1, GUIContent.none, variant.QuaternionValue);
                        break;
                    case VariantType.Color:
                        variant.ColorValue = EditorGUI.ColorField(r1, variant.ColorValue);
                        break;
                    case VariantType.DateTime:
                        variant.DateValue = ConvertUtil.ToDate(EditorGUI.TextField(r1, variant.DateValue.ToString()));
                        break;
                    case VariantType.GameObject:
                    case VariantType.Component:
                    case VariantType.Object:
                        {
                            _selectObjectPropertyDrawer.AllowProxy = true;
                            _selectObjectPropertyDrawer.AllowSceneObjects = true;
                            _selectObjectPropertyDrawer.InheritsFromType = _forcedObjectType;
                            var targProp = property.FindPropertyRelative(PROP_OBJREF);
                            EditorGUI.BeginChangeCheck();
                            _selectObjectPropertyDrawer.OnGUI(r1, targProp, GUIContent.none);
                            if (EditorGUI.EndChangeCheck())
                            {
                                variant.ObjectValue = targProp.objectReferenceValue;
                            }
                        }
                        break;
                    case VariantType.LayerMask:
                        {
                            variant.LayerMaskValue = SPEditorGUI.LayerMaskField(r1, GUIContent.none, (int)variant.LayerMaskValue);
                        }
                        break;
                    case VariantType.Rect:
                        {
                            variant.RectValue = EditorGUI.RectField(r1, variant.RectValue);
                        }
                        break;
                    case VariantType.Numeric:
                        {
                            //we just treat numeric types as double and let the numeric deal with it
                            var tp = this.TypeRestrictedTo;
                            if (tp != null && typeof(INumeric).IsAssignableFrom(tp))
                            {
                                var n = variant.NumericValue;
                                double d = n != null ? n.ToDouble(null) : 0d;
                                EditorGUI.BeginChangeCheck();
                                d = EditorGUI.DoubleField(r1, d);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    variant.NumericValue = Numerics.CreateNumeric(tp, d);
                                }
                            }
                            else
                            {
                                variant.DoubleValue = EditorGUI.DoubleField(r1, variant.DoubleValue);
                            }
                        }
                        break;
                }

            }
        }

        protected virtual void DrawValueFieldInPropertyMode(Rect position, SerializedProperty property, EditorVariantReference helper)
        {
            _selectObjectPropertyDrawer.AllowProxy = true;
            _selectObjectPropertyDrawer.AllowSceneObjects = true;
            _selectObjectPropertyDrawer.InheritsFromType = null;
            var targProp = property.FindPropertyRelative(PROP_OBJREF);
            var memberProp = property.FindPropertyRelative(PROP_STRING);
            var vtypeProp = property.FindPropertyRelative(PROP_TYPE);

            if (targProp.objectReferenceValue == null)
            {
                _selectObjectPropertyDrawer.OnGUI(position, targProp, GUIContent.none);
            }
            else
            {
                const System.Reflection.MemberTypes MASK = System.Reflection.MemberTypes.Field | System.Reflection.MemberTypes.Property;
                const DynamicMemberAccess ACCESS = DynamicMemberAccess.Read;

                if (SPEditorGUI.XButton(ref position, "Clear Selected Object", true))
                {
                    targProp.objectReferenceValue = null;
                    memberProp.stringValue = string.Empty;
                    vtypeProp.SetEnumValue(VariantType.Null);
                    return;
                }
                SPEditorGUI.RefButton(ref position, targProp.objectReferenceValue, true);

                var targObj = targProp.objectReferenceValue;
                var memberName = memberProp.stringValue;

                IEnumerable<MemberInfo> GetMembersFromTarget(object o)
                {
                    return o is IDynamic ? DynamicUtil.GetMembers(o, false, MASK) : DynamicUtil.GetMembersFromType((o.IsProxy_ParamsRespecting() ? (o as IProxy).GetTargetType() : o.GetType()), false, MASK);
                }

                var go = GameObjectUtil.GetGameObjectFromSource(targObj);
                if (go != null)
                {
                    using (var lst = TempCollection.GetList<Component>())
                    {
                        go.GetComponents(lst);
                        //var members = (from o in lst.Cast<object>().Prepend(go)
                        //               let mtp = (o.IsProxy_ParamsRespecting() ? (o as IProxy).GetTargetType() : o.GetType())
                        //               from m in DynamicUtil.GetMembersFromType(mtp, false, MASK).Where(m => DynamicUtil.GetMemberAccessLevel(m).HasFlag(ACCESS) && !m.IsObsolete())
                        //               select (o, m)).ToArray();
                        var members = (from o in lst.Cast<object>().Prepend(go)
                                       from m in GetMembersFromTarget(o).Where(m => DynamicUtil.GetMemberAccessLevel(m).HasFlag(ACCESS) && !m.IsObsolete())
                                       select (o, m)).ToArray();
                        var entries = members.Select(t =>
                        {
                            if (t.o.IsProxy_ParamsRespecting())
                            {
                                return EditorHelper.TempContent(string.Format("{0} [{1}]/{2} [{3}]", go.name, t.o.GetType().Name, t.m.Name, DynamicUtil.GetReturnType(t.m).Name));
                            }
                            else if ((DynamicUtil.GetMemberAccessLevel(t.m) & DynamicMemberAccess.Write) != 0)
                            {
                                return EditorHelper.TempContent(string.Format("{0} [{1}]/{2} [{3}] -> {4}", go.name, t.o.GetType().Name, t.m.Name, DynamicUtil.GetReturnType(t.m).Name, EditorHelper.GetValueWithMemberSafe(t.m, t.o, true)));
                            }
                            else
                            {
                                return EditorHelper.TempContent(string.Format("{0} [{1}]/{2} (readonly - {3}) -> {4}", go.name, t.o.GetType().Name, t.m.Name, DynamicUtil.GetReturnType(t.m).Name, EditorHelper.GetValueWithMemberSafe(t.m, t.o, true)));
                            }
                        }).Prepend(EditorHelper.TempContent(string.Format("{0} --no selection--", go.name))).ToArray();
                        int index = members.IndexOf(t => object.ReferenceEquals(t.o, targObj) && t.m.Name == memberName) + 1;

                        EditorGUI.BeginChangeCheck();
                        index = EditorGUI.Popup(position, index, entries);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (index > 0)
                            {
                                index--;
                                targProp.objectReferenceValue = members[index].o as UnityEngine.Object;
                                memberProp.stringValue = members[index].m.Name;
                                vtypeProp.SetEnumValue(VariantReference.GetVariantType(DynamicUtil.GetReturnType(members[index].m)));
                            }
                            else if (index == 0)
                            {
                                targProp.objectReferenceValue = targObj ?? go;
                                memberProp.stringValue = string.Empty;
                                vtypeProp.SetEnumValue(VariantType.Null);
                            }
                            else
                            {
                                targProp.objectReferenceValue = null;
                                memberProp.stringValue = string.Empty;
                                vtypeProp.SetEnumValue(VariantType.Null);
                            }
                        }
                    }
                }
                else
                {
                    //var mtp = targObj.IsProxy_ParamsRespecting() ? (targObj as IProxy).GetTargetType() : targObj.GetType();
                    //var members = DynamicUtil.GetMembersFromType(mtp, false, MASK).Where(m => DynamicUtil.GetMemberAccessLevel(m).HasFlag(ACCESS) && !m.IsObsolete()).ToArray();
                    var members = GetMembersFromTarget(targObj).Where(m => DynamicUtil.GetMemberAccessLevel(m).HasFlag(ACCESS) && !m.IsObsolete()).ToArray();
                    var entries = members.Select(m =>
                    {
                        if (targObj.IsProxy_ParamsRespecting())
                        {
                            return EditorHelper.TempContent(string.Format("{0} [{1}].{2} [{3}]", targObj.name, targObj.GetType().Name, m.Name, DynamicUtil.GetReturnType(m).Name));
                        }
                        else if ((DynamicUtil.GetMemberAccessLevel(m) & DynamicMemberAccess.Write) != 0)
                        {
                            return EditorHelper.TempContent(string.Format("{0} [{1}].{2} [{3}] -> {4}", targObj.name, targObj.GetType().Name, m.Name, DynamicUtil.GetReturnType(m).Name, EditorHelper.GetValueWithMemberSafe(m, targObj, true)));
                        }
                        else
                        {
                            return EditorHelper.TempContent(string.Format("{0} [{1}].{2} (readonly - {3}) -> {4}", targObj.name, targObj.GetType().Name, m.Name, DynamicUtil.GetReturnType(m).Name, EditorHelper.GetValueWithMemberSafe(m, targObj, true)));
                        }
                    }).Prepend(EditorHelper.TempContent(string.Format("{0} --no selection--", targObj.name))).ToArray();

                    int index = members.IndexOf(m => m.Name == memberName) + 1;

                    EditorGUI.BeginChangeCheck();
                    index = EditorGUI.Popup(position, index, entries);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (index > 0)
                        {
                            index--;
                            targProp.objectReferenceValue = targObj;
                            memberProp.stringValue = members[index].Name;
                            vtypeProp.SetEnumValue(VariantReference.GetVariantType(DynamicUtil.GetReturnType(members[index])));
                        }
                        else if (index == 0)
                        {
                            targProp.objectReferenceValue = targObj;
                            memberProp.stringValue = string.Empty;
                            vtypeProp.SetEnumValue(VariantType.Null);
                        }
                        else
                        {
                            targProp.objectReferenceValue = null;
                            memberProp.stringValue = string.Empty;
                            vtypeProp.SetEnumValue(VariantType.Null);
                        }
                    }
                }
            }
        }

        protected virtual void DrawValueFieldInEvalMode(Rect position, SerializedProperty property, EditorVariantReference helper)
        {
            _selectObjectPropertyDrawer.AllowProxy = true;
            _selectObjectPropertyDrawer.AllowSceneObjects = true;
            _selectObjectPropertyDrawer.InheritsFromType = null;
            var targProp = property.FindPropertyRelative(PROP_OBJREF);
            var evalProp = property.FindPropertyRelative(PROP_STRING);
            var vtypeProp = property.FindPropertyRelative(PROP_TYPE);

            var r1 = new Rect(position.xMin, position.yMin, position.width * 0.4f, position.height);
            var r2 = new Rect(r1.xMax, position.yMin, position.width - r1.width, position.height);
            _selectObjectPropertyDrawer.OnGUI(r1, targProp, GUIContent.none);
            evalProp.stringValue = EditorGUI.TextField(r2, evalProp.stringValue);
            vtypeProp.SetEnumValue<VariantType>(VariantType.Null);
        }

        protected virtual void HandleDragDrop(Rect position, SerializedProperty property)
        {
            if (_helper._mode == VariantReference.RefMode.Eval) return;

            var ev = Event.current;
            switch (ev.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (position.Contains(ev.mousePosition))
                    {
                        var draggedobj = DragAndDrop.objectReferences.FirstOrDefault((o) => o is IProxy || ObjUtil.GetAsFromSource(_forcedObjectType, o) != null);
                        DragAndDrop.visualMode = draggedobj != null ? DragAndDropVisualMode.Link : DragAndDropVisualMode.Rejected;

                        if (draggedobj != null && ev.type == EventType.DragPerform)
                        {
                            var targProp = property.FindPropertyRelative(PROP_OBJREF);
                            if (targProp.objectReferenceValue == draggedobj) return;

                            var memberProp = property.FindPropertyRelative(PROP_STRING);
                            var vtypeProp = property.FindPropertyRelative(PROP_TYPE);
                            switch (_helper._mode)
                            {
                                case VariantReference.RefMode.Value:
                                    targProp.objectReferenceValue = draggedobj;
                                    memberProp.stringValue = string.Empty;
                                    vtypeProp.SetEnumValue(VariantReference.GetVariantType(draggedobj.GetType()));
                                    break;
                                case VariantReference.RefMode.Property:
                                    targProp.objectReferenceValue = draggedobj;
                                    memberProp.stringValue = string.Empty;
                                    vtypeProp.SetEnumValue(VariantType.Null);
                                    break;
                            }
                        }
                    }
                    break;
            }
        }


        #endregion

        #region Static Utils

        public static void CopyValuesToHelper(SerializedProperty property, EditorVariantReference helper)
        {
            helper._mode = property.FindPropertyRelative(PROP_MODE).GetEnumValue<VariantReference.RefMode>();
            helper._type = property.FindPropertyRelative(PROP_TYPE).GetEnumValue<VariantType>();
            helper._x = property.FindPropertyRelative(PROP_X).floatValue;
            helper._y = property.FindPropertyRelative(PROP_Y).floatValue;
            helper._z = property.FindPropertyRelative(PROP_Z).floatValue;
            helper._w = property.FindPropertyRelative(PROP_W).doubleValue;
            helper._string = property.FindPropertyRelative(PROP_STRING).stringValue;
            helper._unityObjectReference = property.FindPropertyRelative(PROP_OBJREF).objectReferenceValue;
        }

        public static void CopyValuesFromHelper(SerializedProperty property, EditorVariantReference helper)
        {
            property.FindPropertyRelative(PROP_MODE).SetEnumValue(helper._mode);
            property.FindPropertyRelative(PROP_TYPE).SetEnumValue(helper._type);
            property.FindPropertyRelative(PROP_X).floatValue = helper._x;
            property.FindPropertyRelative(PROP_Y).floatValue = helper._y;
            property.FindPropertyRelative(PROP_Z).floatValue = helper._z;
            property.FindPropertyRelative(PROP_W).doubleValue = helper._w;
            property.FindPropertyRelative(PROP_STRING).stringValue = helper._string;
            property.FindPropertyRelative(PROP_OBJREF).objectReferenceValue = helper._unityObjectReference;
        }

        public static void SetSerializedProperty(SerializedProperty property, object obj)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));

            var variant = new EditorVariantReference();
            variant.Value = obj;
            CopyValuesFromHelper(property, variant);
        }

        public static object GetFromSerializedProperty(SerializedProperty property)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));

            var helper = new EditorVariantReference();
            CopyValuesToHelper(property, helper);
            return helper.Value;
        }

        #endregion

        #region Specail Types

        public class EditorVariantReference : VariantReference
        {

            public new RefMode _mode
            {
                get { return base._mode; }
                set { base._mode = value; }
            }

            public new VariantType _type
            {
                get { return base._type; }
                set { base._type = value; }
            }

            public new float _x
            {
                get { return base._x; }
                set { base._x = value; }
            }

            public new float _y
            {
                get { return base._y; }
                set { base._y = value; }
            }

            public new float _z
            {
                get { return base._z; }
                set { base._z = value; }
            }

            public new double _w
            {
                get { return base._w; }
                set { base._w = value; }
            }

            public new string _string
            {
                get { return base._string; }
                set { base._string = value; }
            }

            public new UnityEngine.Object _unityObjectReference
            {
                get { return base._unityObjectReference; }
                set { base._unityObjectReference = value; }
            }


            public void PrepareForValueTypeChange(VariantType type)
            {
                base._type = type;
                base._mode = RefMode.Value;
                base._x = 0f;
                base._y = 0f;
                base._z = 0f;
                base._w = 0d;
                base._string = string.Empty;
                switch (type)
                {
                    case VariantType.Object:
                        break;
                    case VariantType.Null:
                    case VariantType.String:
                    case VariantType.Boolean:
                    case VariantType.Integer:
                    case VariantType.Float:
                    case VariantType.Double:
                    case VariantType.Vector2:
                    case VariantType.Vector3:
                    case VariantType.Quaternion:
                    case VariantType.Color:
                    case VariantType.DateTime:
                        base._unityObjectReference = null;
                        break;
                    case VariantType.GameObject:
                        base._unityObjectReference = GameObjectUtil.GetGameObjectFromSource(base._unityObjectReference);
                        break;
                    case VariantType.Component:
                        base._unityObjectReference = base._unityObjectReference as Component;
                        break;
                    case VariantType.Numeric:
                        base._unityObjectReference = null;
                        break;
                }
            }

            public void PrepareForRefModeChange(RefMode mode)
            {
                base._mode = mode;
                base._x = 0f;
                base._y = 0f;
                base._z = 0f;
                base._w = 0d;
                base._string = string.Empty;
                switch (mode)
                {
                    case RefMode.Value:
                        //_variant._type = ...;
                        base._unityObjectReference = null;
                        break;
                    case RefMode.Property:
                        base._type = VariantType.Null;
                        break;
                    case RefMode.Eval:
                        //_variant._type = VariantType.Double;
                        break;
                }
            }


        }

        #endregion

    }

}
