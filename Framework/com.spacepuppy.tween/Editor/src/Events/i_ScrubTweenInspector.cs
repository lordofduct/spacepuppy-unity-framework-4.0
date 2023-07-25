using UnityEngine;
using UnityEditor;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Tween;
using com.spacepuppy.Tween.Events;
using com.spacepuppy.Utils;

using com.spacepuppyeditor.Core;
using com.spacepuppyeditor.Internal;

namespace com.spacepuppyeditor.Tween.Events
{

    [CustomEditor(typeof(i_ScrubTween))]
    public class i_ScrubTweenInspector : SPEditor
    {

        const string PROP_SCRUBTIME = "_scrubTime";
        const string PROP_TARGET = "_target";
        const string PROP_TWEENDATA = "_data";

        private const string PROP_DATA_MEMBER = "MemberName";
        private const string PROP_DATA_EASE = "Ease";
        private const string PROP_DATA_VALUES = "ValueS";
        private const string PROP_DATA_VALUEE = "ValueE";
        private const string PROP_DATA_DUR = "Duration";
        private const string PROP_DATA_OPTION = "Option";

        private SPReorderableList _dataList;
        private SerializedProperty _targetProp;
        private VariantReferencePropertyDrawer _variantDrawer = new VariantReferencePropertyDrawer();

        protected override void OnEnable()
        {
            base.OnEnable();

            _dataList = new SPReorderableList(this.serializedObject, this.serializedObject.FindProperty(PROP_TWEENDATA));
            _dataList.drawHeaderCallback = _dataList_DrawHeader;
            _dataList.drawElementCallback = _dataList_DrawElement;
            _dataList.elementHeight = EditorGUIUtility.singleLineHeight * 7f + 7f;

        }

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            _targetProp = this.serializedObject.FindProperty(PROP_TARGET);

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(EditorHelper.PROP_ORDER);
            this.DrawPropertyField(EditorHelper.PROP_ACTIVATEON);
            this.DrawPropertyField(PROP_SCRUBTIME);
            SPEditorGUILayout.PropertyField(_targetProp);

            _dataList.DoLayoutList();

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, EditorHelper.PROP_ORDER, EditorHelper.PROP_ACTIVATEON, PROP_SCRUBTIME, PROP_TARGET, PROP_TARGET, PROP_TWEENDATA);

            this.serializedObject.ApplyModifiedProperties();
        }

        #region ReorderableList Handlers

        private void _dataList_DrawHeader(Rect area)
        {
            EditorGUI.LabelField(area, "Tween Data");
        }

        private void _dataList_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            Rect position;
            var el = _dataList.serializedProperty.GetArrayElementAtIndex(index);
            var mtp = el.GetManagedReferenceType();
            if (mtp == null)
            {
                el.managedReferenceValue = new i_ScrubTween.GenericTweenData();
                GUI.changed = true;
                return;
            }
            else if (mtp != typeof(i_ScrubTween.GenericTweenData))
            {
                EditorGUI.LabelField(area, "Unsupported ITweenData Type '" + mtp.Name + "' in editor.");
                return;
            }

            position = CalcNextRect(ref area);

            //TODO - member
            position = CalcNextRect(ref area);
            var memberProp = el.FindPropertyRelative(PROP_DATA_MEMBER);
            object targObj = _targetProp.objectReferenceValue;
            var targTp = targObj?.GetType(true);

            System.Type propType;
            memberProp.stringValue = i_TweenInspector.ReflectedPropertyAndCustomTweenAccessorField(position,
                                                                                                   EditorHelper.TempContent("Property", "The property on the target to set."),
                                                                                                   targTp, targObj,
                                                                                                   memberProp.stringValue,
                                                                                                   com.spacepuppy.Dynamic.DynamicMemberAccess.ReadWrite,
                                                                                                   out propType);
            var curveGenerator = SPTween.CurveFactory.LookupTweenCurveGenerator(targObj?.GetType(), memberProp.stringValue, propType);

            position = CalcNextRect(ref area);
            SPEditorGUI.PropertyField(position, el.FindPropertyRelative(PROP_DATA_EASE));

            position = CalcNextRect(ref area);
            var propOption = el.FindPropertyRelative(PROP_DATA_OPTION);
            this.DrawOption(position, curveGenerator, propOption);

            position = CalcNextRect(ref area);
            SPEditorGUI.PropertyField(position, el.FindPropertyRelative(PROP_DATA_DUR));

            propType = curveGenerator?.GetExpectedMemberType(propOption.intValue) ?? propType;
            if (propType != null)
            {
                //Only From-To supported
                position = CalcNextRect(ref area);
                this.DrawVariant(position, EditorHelper.TempContent("Start Value"), propType, el.FindPropertyRelative(PROP_DATA_VALUES));

                position = CalcNextRect(ref area);
                this.DrawVariant(position, EditorHelper.TempContent("End Value"), propType, el.FindPropertyRelative(PROP_DATA_VALUEE));
            }


        }

        private void DrawVariant(Rect position, GUIContent label, System.Type propType, SerializedProperty valueProp)
        {
            if (com.spacepuppy.Dynamic.DynamicUtil.TypeIsVariantSupported(propType))
            {
                //draw the default variant as the method accepts anything
                _variantDrawer.RestrictVariantType = false;
                _variantDrawer.ForcedObjectType = null;
                _variantDrawer.OnGUI(position, valueProp, label);
            }
            else
            {
                _variantDrawer.RestrictVariantType = true;
                _variantDrawer.TypeRestrictedTo = propType;
                _variantDrawer.ForcedObjectType = (TypeUtil.IsType(propType, typeof(Component))) ? propType : null;
                _variantDrawer.OnGUI(position, valueProp, label);
            }
        }


        private static Rect CalcNextRect(ref Rect area)
        {
            var pos = new Rect(area.xMin, area.yMin + 1f, area.width, EditorGUIUtility.singleLineHeight);
            area = new Rect(pos.xMin, pos.yMax, area.width, area.height - EditorGUIUtility.singleLineHeight + 1f);
            return pos;
        }

        private void DrawOption(Rect position, ITweenCurveGenerator generator, SerializedProperty optionProp)
        {
            var etp = generator?.GetOptionEnumType();
            if (etp != null)
            {
                System.Enum evalue = null;
                if (!System.Enum.IsDefined(etp, optionProp.intValue))
                {
                    var arr = System.Enum.GetValues(etp) as System.Enum[];
                    evalue = arr?.FirstOrDefault();
                }
                else
                {
                    evalue = System.Enum.ToObject(etp, optionProp.intValue) as System.Enum;
                }

                if (evalue != null)
                {
                    evalue = EditorGUI.EnumPopup(position, "Option", evalue);
                    optionProp.intValue = ConvertUtil.ToInt(evalue);
                    return;
                }
            }

            optionProp.intValue = 0;
            EditorGUI.LabelField(position, "Option", "(no option available)");
        }

        #endregion

    }

}
