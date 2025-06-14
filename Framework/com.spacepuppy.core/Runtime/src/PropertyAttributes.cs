﻿using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public abstract class SPPropertyAttribute : PropertyAttribute
    {

        public SPPropertyAttribute()
        {

        }

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class DisplayNameAttribute : SPPropertyAttribute
    {

        public string DisplayName;

        public DisplayNameAttribute(string displayName)
        {
            this.DisplayName = displayName;
        }

    }

    #region ComponentAttributes

    public abstract class ComponentHeaderAttribute : PropertyAttribute
    {

    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RequireComponentInEntityAttribute : ComponentHeaderAttribute
    {

        private System.Type[] _types;

        public RequireComponentInEntityAttribute(params System.Type[] tps)
        {
            _types = tps;
        }

        public System.Type[] Types
        {
            get { return _types; }
        }

    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RequireComponentInParentAttribute : ComponentHeaderAttribute
    {

        private System.Type[] _types;

        public RequireComponentInParentAttribute(params System.Type[] tps)
        {
            _types = tps;
        }

        public System.Type[] Types
        {
            get { return _types; }
        }

    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RequireLikeComponentAttribute : ComponentHeaderAttribute
    {

        private System.Type[] _types;

        public RequireLikeComponentAttribute(params System.Type[] tps)
        {
            _types = tps;
        }

        public System.Type[] Types
        {
            get { return _types; }
        }

    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RequireLayerAttribute : ComponentHeaderAttribute
    {
        public int Layer;

        public RequireLayerAttribute(int layer)
        {
            this.Layer = layer;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RequireTagAttribute : ComponentHeaderAttribute
    {
        public string[] Tags;
        public bool HideInfoBox;

        public RequireTagAttribute(string tag)
        {
            this.Tags = new string[] { tag };
        }

        public RequireTagAttribute(params string[] tags)
        {
            this.Tags = tags ?? new string[] { }; 
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UniqueToEntityAttribute : ComponentHeaderAttribute
    {

        public bool MustBeAttachedToRoot;
        public bool IgnoreInactive;

    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ConstantlyRepaintEditorAttribute : System.Attribute
    {
        public bool RuntimeOnly;
    }

    #endregion

    #region Property Drawer Attributes

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class RefPickerConfigAttribute : SPPropertyAttribute
    {
        public bool AllowNull;
        public bool AlwaysExpanded;
        public bool DisplayBox;
        public string NullLabel;
    }

    [System.Obsolete("User InterfaceRef, InterfaceRefOrPicker, or InterfacePicker in place of this combined with the 'RefPickerConfigAttribute' for configuration.")]
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class SerializeRefPickerAttribute : RefPickerConfigAttribute
    {
        public readonly System.Type RefType;

        public SerializeRefPickerAttribute() { }
        public SerializeRefPickerAttribute(System.Type tp)
        {
            this.RefType = tp;
        }
    }

    public class SerializeRefLabelAttribute : System.Attribute
    {
        public string Label { get; set; }
        public int Order { get; set; }

        public SerializeRefLabelAttribute() { }
        public SerializeRefLabelAttribute(string label)
        {
            this.Label = label;
        }
    }

    public class DisplayFlatAttribute : SPPropertyAttribute
    {

        public bool AlwaysExpanded = true;
        public bool DisplayBox;
        public bool IgnoreIfNoChildren;

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class DisplayNestedPropertyAttribute : SPPropertyAttribute
    {

        public readonly string InnerPropName;
        public readonly string Label;
        public readonly string Tooltip;

        public DisplayNestedPropertyAttribute(string innerPropName)
        {
            InnerPropName = innerPropName;
        }

        public DisplayNestedPropertyAttribute(string innerPropName, string label)
        {
            InnerPropName = innerPropName;
            Label = label;
        }

        public DisplayNestedPropertyAttribute(string innerPropName, string label, string tooltip)
        {
            InnerPropName = innerPropName;
            Label = label;
            Tooltip = tooltip;
        }

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class EnumFlagsAttribute : SPPropertyAttribute
    {

        public System.Type EnumType;
        public int[] excluded;

        public EnumFlagsAttribute()
        {

        }

        public EnumFlagsAttribute(System.Type enumType)
        {
            this.EnumType = enumType;
        }

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class EnumPopupExcludingAttribute : SPPropertyAttribute
    {

        public readonly int[] excludedValues;

        public EnumPopupExcludingAttribute(params int[] excluded)
        {
            excludedValues = excluded;
        }

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class EnumInCustomOrderAttribute : SPPropertyAttribute
    {

        public readonly int[] customOrder;

        public EnumInCustomOrderAttribute(params int[] enumOrder)
        {
            this.customOrder = enumOrder;
        }

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class NegativeIsInfinityAttribute : SPPropertyAttribute
    {

        public string ShortInfinityLabel;
        public string InfinityLabel;
        public bool ZeroIsAlsoInfinity;

    }

    /// <summary>
    /// Restrict a value to be no greater than max.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class MaxRangeAttribute : SPPropertyAttribute
    {
        public float Max;

        public MaxRangeAttribute(float max)
        {
            this.Max = max;
        }
    }

    /// <summary>
    /// Restrict a value to be no lesser than min.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class MinRangeAttribute : SPPropertyAttribute
    {
        public float Min;

        public MinRangeAttribute(float min)
        {
            this.Min = min;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class OneOrManyAttribute : SPPropertyAttribute
    {
        public OneOrManyAttribute()
        {

        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class ReorderableArrayAttribute : SPPropertyAttribute
    {

        public string ElementLabelFormatString = null;
        public bool OneBasedLabelIndex = false;
        public bool AlwaysExpanded;
        public bool RemoveBackgroundWhenCollapsed;
        public bool Draggable = true;
        public float ElementPadding = 0f;
        public bool DrawElementAtBottom = false;
        public bool HideElementLabel = false;
        public bool ShowTooltipInHeader = false;
        public bool HideLengthField = false;
        public bool ElementLabelIsEditable = false;

        /// <summary>
        /// If DrawElementAtBottom is true OR ElementLabelIsEditable is true, this child element can be displayed as the label in the reorderable list.
        /// </summary>
        public string ChildPropertyToDrawAsElementLabel;

        /// <summary>
        /// If DrawElementAtBottom is true OR ElementLabelIsEditable is true, this child element can be displayed as the modifiable entry in the reorderable list.
        /// </summary>
        public string ChildPropertyToDrawAsElementEntry;

        /// <summary>
        /// A method on the serialized object that is called when a new entry is added to the list/array. Should accept the list member type 
        /// as a parameter, and then also return it (used for updating).
        /// 
        /// Like:
        /// object OnObjectAddedToList(object obj)
        /// </summary>
        public string OnAddCallback;

        /// <summary>
        /// If the array/list accepts UnityEngine.Objects, this will allow the dragging of objects onto the inspector to auto add without needing to click the + button.
        /// </summary>
        public bool AllowDragAndDrop = true;

        public bool AllowDragAndDropSceneObjects = true;

        public ReorderableArrayAttribute()
        {

        }

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class SelectableComponentAttribute : SPPropertyAttribute
    {
        public System.Type InheritsFromType;
        public bool AllowSceneObjects = true;
        public bool ForceOnlySelf = false;
        public bool SearchChildren = false;
        public bool AllowProxy;

        public SelectableComponentAttribute()
        {

        }

        public SelectableComponentAttribute(System.Type inheritsFromType)
        {
            this.InheritsFromType = inheritsFromType;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class SelectableObjectAttribute : SPPropertyAttribute
    {
        public System.Type InheritsFromType;
        public bool AllowSceneObjects = true;
        public bool AllowProxy;

        public SelectableObjectAttribute()
        {

        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class TagSelectorAttribute : SPPropertyAttribute
    {
        public bool AllowUntagged;
        public bool AllowBlank;
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class TimeUnitsSelectorAttribute : SPPropertyAttribute
    {
        public string DefaultUnits;

        public TimeUnitsSelectorAttribute()
        {
        }

        public TimeUnitsSelectorAttribute(string defaultUnits)
        {
            DefaultUnits = defaultUnits;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class TypeRestrictionAttribute : SPPropertyAttribute
    {
        public System.Type[] InheritsFromTypes;
        public bool HideTypeDropDown;
        public bool HideTypeDropDownIfSingle;
        public bool AllowProxy;
        public bool RestrictProxyResolvedType; //if IProxy is allowed, should we test if the type returned by IProxy.GetReturnedType matches the accepted types
        public bool AllowSceneObjects = true;

        public TypeRestrictionAttribute()
        {
            this.InheritsFromTypes = null;
        }

        public TypeRestrictionAttribute(System.Type inheritsFromType)
        {
            this.InheritsFromTypes = new System.Type[] { inheritsFromType };
        }

        public TypeRestrictionAttribute(params System.Type[] inheritsFromTypes)
        {
            this.InheritsFromTypes = inheritsFromTypes;
        }

    }

    /// <summary>
    /// ScriptableObject doesn't draw vectors correctly for some reason... this allows you to coerce it to.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class VectorInspectorAttribute : SPPropertyAttribute
    {

        public VectorInspectorAttribute()
        {

        }

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class UnitVectorAttribute : SPPropertyAttribute
    {

        public UnitVectorAttribute() : base()
        {

        }

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class EulerRotationInspectorAttribute : SPPropertyAttribute
    {

        public bool UseRadians = false;

        public EulerRotationInspectorAttribute()
        {

        }

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class AnimationCurveConstraintAttribute : PropertyAttribute
    {
        public float x;
        public float y;
        public float width = float.PositiveInfinity;
        public float height = float.PositiveInfinity;
        public uint color = Color.green.ToARGB();

        public Color GetColor() => ColorUtil.ARGBToColor(color);
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class AnimationCurveEaseScaleAttribute : PropertyAttribute
    {
        public float overscan = 1f;
        public uint color = Color.green.ToARGB();

        public Color GetColor() => ColorUtil.ARGBToColor(color);
    }

    /// <summary>
    /// A specialized PropertyDrawer that draws a struct/class in the shape:
    /// struct Pair
    /// {
    ///     float Weight;
    ///     UnityEngine.Object Value;
    /// }
    /// 
    /// It is drawn in the inspector as a single row as weight : value. 
    /// It is intended for use with arrays/lists of values that can be randomly selected by some weight.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class WeightedValueCollectionAttribute : ReorderableArrayAttribute
    {
        public string WeightPropertyName = "Weight";

        public WeightedValueCollectionAttribute(string weightPropName, string valuePropName)
        {
            this.WeightPropertyName = weightPropName;
            this.ChildPropertyToDrawAsElementEntry = valuePropName;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class InputIDAttribute : SPPropertyAttribute
    {

        public string[] RestrictedTo;
        public string[] Exclude;

        public InputIDAttribute()
        {

        }

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class PropertyNameSelectorAttribute : SPPropertyAttribute
    {

        public System.Type[] TargetTypes;
        public bool AllowCustom;
        public string[] IgnorePropNames;
        public bool AllowReadOnly;

        /// <summary>
        /// A callback on the scriptableobject target in the shape of 'bool (MemberInfo m)' to be used as a predicate.
        /// </summary>
        public string IgnoreCallback;

        public PropertyNameSelectorAttribute(params System.Type[] targetTypes)
        {
            this.TargetTypes = targetTypes ?? ArrayUtil.Empty<System.Type>();
        }

    }

    public class LayerSelectorAttribute : SPPropertyAttribute
    {

    }

    #endregion

    #region ModifierDrawer Attributes

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public abstract class PropertyModifierAttribute : SPPropertyAttribute
    {
        public bool IncludeChidrenOnDraw;
    }

    /// <summary>
    /// While in the editor, if the value is ever null, an attempt is made to get the value from self. You will still 
    /// have to initialize the value on Awake if null. The cost of doing it automatically is too high for all components 
    /// to test themselves for this attribute.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class DefaultFromSelfAttribute : PropertyModifierAttribute
    {
        public EntityRelativity Relativity = EntityRelativity.Self;
        public bool HandleOnce = true;

        public DefaultFromSelfAttribute(EntityRelativity relativity = EntityRelativity.Self)
        {
            this.Relativity = relativity;
        }

    }

    /// <summary>
    /// While in the editor, if the value is ever null, an attempt is made to find the value on a GameObject in itself 
    /// that matches the name given.
    /// 
    /// You whil still have to initialize the value on Awake if null. The cost of doing it automatically is too high for all 
    /// components to test themselves for this attribute.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class FindInSelfAttribute : PropertyModifierAttribute
    {
        public string Name;
        public bool UseEntity = false;

        public FindInSelfAttribute(string name)
        {
            this.Name = name;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class DisableOnPlayAttribute : PropertyModifierAttribute
    {

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class DisableIfAttribute : PropertyModifierAttribute
    {
        public readonly string MemberName;
        public bool DisableIfNot;
        public bool DisableAlways;

        public DisableIfAttribute(bool always)
        {
            this.DisableAlways = always;
        }

        public DisableIfAttribute(string memberName)
        {
            this.MemberName = memberName;
        }

    }

    /// <summary>
    /// Display a field in the inspector only if the property/method returns true (supports private).
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class DisplayIfAttribute : PropertyModifierAttribute
    {
        public readonly string MemberName;
        public bool DisplayIfNot;

        public DisplayIfAttribute(string memberName)
        {
            this.MemberName = memberName;
        }

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class ForceFromSelfAttribute : PropertyModifierAttribute
    {

        public EntityRelativity Relativity = EntityRelativity.Self;

        public ForceFromSelfAttribute(EntityRelativity relativity = EntityRelativity.Self)
        {
            this.Relativity = relativity;
        }

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class OnChangedInEditorAttribute : PropertyModifierAttribute
    {

        public readonly string MethodName;
        public bool OnlyAtRuntime;

        public OnChangedInEditorAttribute(string methodName)
        {
            this.MethodName = methodName;
        }

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class ReadOnlyAttribute : PropertyModifierAttribute
    {

    }

    #endregion

    #region DecoratorDrawer Attributes

    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple = true)]
    public class InsertButtonAttribute : SPPropertyAttribute
    {

        public string Label;
        public string OnClick;
        public bool PrecedeProperty;
        public bool RuntimeOnly;
        public bool SupportsMultiObjectEditing;
        public bool Validate;
        public string ValidateMessage;
        public string ValidateShowCallback;
        public bool RecordUndo;
        public string UndoLabel;
        public float Space;

        public InsertButtonAttribute(string label)
        {
            this.Label = label;
            this.OnClick = string.Empty;
        }

        public InsertButtonAttribute(string label, string onClick)
        {
            this.Label = label;
            this.OnClick = onClick;
        }

    }

    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Class, AllowMultiple = false)]
    public class InfoboxAttribute : ComponentHeaderAttribute
    {
        public string Message;
        public InfoBoxMessageType MessageType = InfoBoxMessageType.Info;

        public InfoboxAttribute(string msg)
        {
            this.Message = msg;
        }

    }

    #endregion

    #region NonSerialized Property Drawer Attributes

    public class ShowNonSerializedPropertyAttribute : System.Attribute
    {
        public string Label;
        public string Tooltip;
        public bool Readonly;
        public bool ShowAtEditorTime;
        public bool ShowOutsideRuntimeValuesFoldout;

        public ShowNonSerializedPropertyAttribute(string label)
        {
            this.Label = label;
        }
    }

    #endregion

}