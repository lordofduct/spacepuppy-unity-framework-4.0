using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

using com.spacepuppyeditor.Core;
using com.spacepuppy.Collections;

namespace com.spacepuppyeditor.Events
{

    [CustomPropertyDrawer(typeof(EventTriggerTarget))]
    public class EventTriggerTargetPropertyDrawer : PropertyDrawer
    {

        public const string PROP_TRIGGERABLETARG = "_triggerable";
        public const string PROP_TRIGGERABLEARGS = "_triggerableArgs";
        public const string PROP_ACTIVATIONTYPE = "_activationType";
        public const string PROP_METHODNAME = "_methodName";


        private const float ARG_BTN_WIDTH = 18f;

        #region Fields

        public bool DrawWeight;

        private GUIContent _defaultArgLabel = new GUIContent("Triggerable Arg");
        private GUIContent _undefinedArgLabel = new GUIContent("Undefined Arg", "The argument is not explicitly defined unless the trigger's event defines it.");
        private GUIContent _messageArgLabel = new GUIContent("Message Arg", "A parameter to be passed to the message if one is desired.");
        private GUIContent _argBtnLabel = new GUIContent("||", "Change between accepting a configured argument or not.");
        private readonly SpecialVariantReferencePropertyDrawer _variantDrawer = new SpecialVariantReferencePropertyDrawer();
        private int _callMethodModeExtraLines = 0;

        #endregion

        #region Properties

        public int TriggerArgCount
        {
            get => _variantDrawer.TriggerArgCount;
            set => _variantDrawer.TriggerArgCount = value;
        }

        #endregion

        #region Methods

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var actProp = property.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_ACTIVATIONTYPE);

            float h = 0f;
            switch (actProp.GetEnumValue<TriggerActivationType>())
            {
                case TriggerActivationType.TriggerAllOnTarget:
                    h += EditorGUIUtility.singleLineHeight * 1f; // 3.0f;
                    break;
                case TriggerActivationType.TriggerSelectedTarget:
                    h += EditorGUIUtility.singleLineHeight * 1f; // 3.0f;
                    break;
                case TriggerActivationType.SendMessage:
                    h += EditorGUIUtility.singleLineHeight * 1f; // 4.0f;
                    break;
                case TriggerActivationType.CallMethodOnSelectedTarget:
                    h += EditorGUIUtility.singleLineHeight * _callMethodModeExtraLines; // (3.0f + _callMethodModeExtraLines);
                    break;
                case TriggerActivationType.EnableTarget:
                    h += EditorGUIUtility.singleLineHeight * 1f;
                    break;
                case TriggerActivationType.DestroyTarget:
                    h += EditorGUIUtility.singleLineHeight * 1f;
                    break;
            }

            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            ////Draw ActivationType Popup
            //var r0 = new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight);
            //var act = EventTriggerTargetPropertyDrawer.DrawTriggerActivationTypeDropdown(r0, property, true);

            //Draw Advanced
            //var area = new Rect(position.xMin, r0.yMax, position.width, position.height - r0.height);
            //switch (act)
            var area = position;
            switch (property.FindPropertyRelative(PROP_ACTIVATIONTYPE).GetEnumValue<TriggerActivationType>())
            {
                case TriggerActivationType.TriggerAllOnTarget:
                    this.DrawAdvanced_TriggerAll(area, property);
                    break;
                case TriggerActivationType.TriggerSelectedTarget:
                    this.DrawAdvanced_TriggerSelected(area, property);
                    break;
                case TriggerActivationType.SendMessage:
                    this.DrawAdvanced_SendMessage(area, property);
                    break;
                case TriggerActivationType.CallMethodOnSelectedTarget:
                    this.DrawAdvanced_CallMethodOnSelected(area, property);
                    break;
                case TriggerActivationType.EnableTarget:
                    this.DrawAdvanced_EnableTarget(area, property);
                    break;
                case TriggerActivationType.DestroyTarget:
                    this.DrawAdvanced_DestroyTarget(area, property);
                    break;
            }

            EditorGUI.EndProperty();
        }


        public static TriggerActivationType DrawTriggerActivationTypeDropdown(Rect area, SerializedProperty property, bool drawLabel)
        {
            //var actProp = property.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_ACTIVATIONTYPE);
            //EditorGUI.PropertyField(area, actProp);

            var actInfo = GetTriggerActivationInfo(property);
            EditorGUI.BeginChangeCheck();

            if (drawLabel)
                actInfo.DropdownSelectedIndex = EditorGUI.Popup(area, actInfo.ActivationTypeProperty.displayName, actInfo.DropdownSelectedIndex, actInfo.DropdownDisplayNames as string[]);
            else
                actInfo.DropdownSelectedIndex = EditorGUI.Popup(area, actInfo.DropdownSelectedIndex, actInfo.DropdownDisplayNames as string[]);

            if (EditorGUI.EndChangeCheck())
            {
                if (actInfo.DropdownSelectedIndex <= 3)
                {
                    //the main ones
                    actInfo.ActivationTypeProperty.SetEnumValue<TriggerActivationType>((TriggerActivationType)actInfo.DropdownSelectedIndex);
                }
                else if (actInfo.DropdownSelectedIndex == 4)
                {
                    //enable
                    actInfo.ActivationTypeProperty.SetEnumValue<TriggerActivationType>(TriggerActivationType.EnableTarget);
                    var argProp = property.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_METHODNAME);
                    argProp.stringValue = EnableMode.Enable.ToString();
                }
                else if (actInfo.DropdownSelectedIndex == 5)
                {
                    //disable
                    actInfo.ActivationTypeProperty.SetEnumValue<TriggerActivationType>(TriggerActivationType.EnableTarget);
                    var argProp = property.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_METHODNAME);
                    argProp.stringValue = EnableMode.Disable.ToString();
                }
                else if (actInfo.DropdownSelectedIndex == 6)
                {
                    //toggle
                    actInfo.ActivationTypeProperty.SetEnumValue<TriggerActivationType>(TriggerActivationType.EnableTarget);
                    var argProp = property.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_METHODNAME);
                    argProp.stringValue = EnableMode.Toggle.ToString();
                }
                else if (actInfo.DropdownSelectedIndex == 7)
                {
                    //destroy
                    actInfo.ActivationTypeProperty.SetEnumValue<TriggerActivationType>(TriggerActivationType.DestroyTarget);
                }
                else
                {
                    //unknown
                    actInfo.ActivationTypeProperty.SetEnumValue<TriggerActivationType>(TriggerActivationType.TriggerAllOnTarget);
                }
            }

            return actInfo.ActivationTypeProperty.GetEnumValue<TriggerActivationType>();
        }


        private void DrawAdvanced_TriggerAll(Rect area, SerializedProperty property)
        {
            //Draw Triggerable Arg
            var argRect = new Rect(area.xMin, area.yMin, area.width - ARG_BTN_WIDTH, EditorGUIUtility.singleLineHeight);
            var btnRect = new Rect(argRect.xMax, argRect.yMin, ARG_BTN_WIDTH, EditorGUIUtility.singleLineHeight);
            var argArrayProp = property.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_TRIGGERABLEARGS);
            if (argArrayProp.arraySize == 0)
            {
                EditorGUI.LabelField(argRect, _defaultArgLabel, _undefinedArgLabel);
                if (GUI.Button(btnRect, _argBtnLabel))
                {
                    argArrayProp.arraySize = 1;
                    argArrayProp.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                if (argArrayProp.arraySize > 1) argArrayProp.arraySize = 1;
                var argProp = argArrayProp.GetArrayElementAtIndex(0);
                //EditorGUI.PropertyField(argRect, argProp, _defaultArgLabel);
                _variantDrawer.RestrictVariantType = false;
                _variantDrawer.TypeRestrictedTo = null;
                _variantDrawer.ForcedObjectType = null;
                _variantDrawer.CurrentTriggerArgIndex = 0;
                _variantDrawer.OnGUI(argRect, argProp, _defaultArgLabel);

                if (GUI.Button(btnRect, _argBtnLabel))
                {
                    argArrayProp.arraySize = 0;
                    argArrayProp.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void DrawAdvanced_TriggerSelected(Rect area, SerializedProperty property)
        {
            //Draw Triggerable Arg
            var argRect = new Rect(area.xMin, area.yMin, area.width - ARG_BTN_WIDTH, EditorGUIUtility.singleLineHeight);
            var btnRect = new Rect(argRect.xMax, argRect.yMin, ARG_BTN_WIDTH, EditorGUIUtility.singleLineHeight);
            var argArrayProp = property.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_TRIGGERABLEARGS);
            if (argArrayProp.arraySize == 0)
            {
                EditorGUI.LabelField(argRect, _defaultArgLabel, _undefinedArgLabel);
                if (GUI.Button(btnRect, _argBtnLabel))
                {
                    argArrayProp.arraySize = 1;
                    argArrayProp.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                if (argArrayProp.arraySize > 1) argArrayProp.arraySize = 1;
                var argProp = argArrayProp.GetArrayElementAtIndex(0);
                //EditorGUI.PropertyField(argRect, argProp, _defaultArgLabel);
                _variantDrawer.RestrictVariantType = false;
                _variantDrawer.TypeRestrictedTo = null;
                _variantDrawer.ForcedObjectType = null;
                _variantDrawer.CurrentTriggerArgIndex = 0;
                _variantDrawer.OnGUI(argRect, argProp, _defaultArgLabel);

                if (GUI.Button(btnRect, _argBtnLabel))
                {
                    argArrayProp.arraySize = 0;
                    argArrayProp.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void DrawAdvanced_SendMessage(Rect area, SerializedProperty property)
        {
            //Draw Triggerable Arg
            var argRect = new Rect(area.xMin, area.yMin, area.width - ARG_BTN_WIDTH, EditorGUIUtility.singleLineHeight);
            var btnRect = new Rect(argRect.xMax, argRect.yMin, ARG_BTN_WIDTH, EditorGUIUtility.singleLineHeight);
            var argArrayProp = property.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_TRIGGERABLEARGS);
            if (argArrayProp.arraySize == 0)
            {
                EditorGUI.LabelField(argRect, _messageArgLabel, _undefinedArgLabel);
                if (GUI.Button(btnRect, _argBtnLabel))
                {
                    argArrayProp.arraySize = 1;
                    argArrayProp.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                if (argArrayProp.arraySize > 1) argArrayProp.arraySize = 1;
                var argProp = argArrayProp.GetArrayElementAtIndex(0);
                //EditorGUI.PropertyField(argRect, argProp, _messageArgLabel);
                _variantDrawer.RestrictVariantType = false;
                _variantDrawer.TypeRestrictedTo = null;
                _variantDrawer.ForcedObjectType = null;
                _variantDrawer.CurrentTriggerArgIndex = 0;
                _variantDrawer.OnGUI(argRect, argProp, _messageArgLabel);

                if (GUI.Button(btnRect, _argBtnLabel))
                {
                    argArrayProp.arraySize = 0;
                    argArrayProp.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void DrawAdvanced_CallMethodOnSelected(Rect area, SerializedProperty property)
        {
            var targProp = property.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_TRIGGERABLETARG);
            var targObj = targProp.objectReferenceValue;
            System.Reflection.MemberInfo selectedMember = null;
            if(targObj)
            {
                string memberName = property.FindPropertyRelative(PROP_METHODNAME).stringValue;
                selectedMember = GetCallableMethodsOnTarget(targObj, !GameObjectUtil.IsGameObjectSource(targObj)).FirstOrDefault(o => o.Name == memberName);
            }

            //Draw Triggerable Arg
            var parr = (selectedMember != null) ? com.spacepuppy.Dynamic.DynamicUtil.GetDynamicParameterInfo(selectedMember) : null;
            if (parr == null || parr.Length == 0)
            {
                //NO PARAMETERS
                _callMethodModeExtraLines = 1;

                var argRect = new Rect(area.xMin, area.yMin, area.width, EditorGUIUtility.singleLineHeight);
                var argArrayProp = property.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_TRIGGERABLEARGS);
                if (argArrayProp.arraySize > 0)
                {
                    argArrayProp.arraySize = 0;
                    argArrayProp.serializedObject.ApplyModifiedProperties();
                }

                var cache = SPGUI.Disable();
                EditorGUI.LabelField(argRect, GUIContent.none, new GUIContent("*Zero Parameter Count*"));
                cache.Reset();
            }
            else
            {
                //MULTIPLE PARAMETERS - special case, does not support trigger event arg
                _callMethodModeExtraLines = parr.Length;

                var argArrayProp = property.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_TRIGGERABLEARGS);

                if (argArrayProp.arraySize != parr.Length)
                {
                    argArrayProp.arraySize = parr.Length;
                    argArrayProp.serializedObject.ApplyModifiedProperties();
                }

                for (int i = 0; i < parr.Length; i++)
                {
                    var paramType = parr[i].ParameterType;
                    var argRect = new Rect(area.xMin, area.yMin + i * EditorGUIUtility.singleLineHeight, area.width, EditorGUIUtility.singleLineHeight);
                    var argProp = argArrayProp.GetArrayElementAtIndex(i);

                    if (paramType == typeof(object))
                    {
                        //draw the default variant as the method accepts anything
                        _variantDrawer.RestrictVariantType = false;
                        _variantDrawer.TypeRestrictedTo = null;
                        _variantDrawer.ForcedObjectType = null;
                        _variantDrawer.CurrentTriggerArgIndex = i;

                        string title = string.Format("Arg {0}: {1} ({2})", i, parr[i].ParameterName, parr[i].ParameterType?.Name ?? "dynamic");
                        _variantDrawer.OnGUI(argRect, argProp, EditorHelper.TempContent(title, "A parameter to be passed to the method if needed."));
                    }
                    else
                    {
                        _variantDrawer.RestrictVariantType = true;
                        _variantDrawer.TypeRestrictedTo = paramType;
                        _variantDrawer.ForcedObjectType = (paramType.IsInterface || TypeUtil.IsType(paramType, typeof(Component))) ? paramType : null;
                        _variantDrawer.CurrentTriggerArgIndex = i;

                        string title = string.Format("Arg {0}: {1} ({2})", i, parr[i].ParameterName, parr[i].ParameterType?.Name ?? "dynamic");
                        _variantDrawer.OnGUI(argRect, argProp, EditorHelper.TempContent(title, "A parameter to be passed to the method if needed."));
                    }
                }
            }

        }

        private void DrawAdvanced_EnableTarget(Rect area, SerializedProperty property)
        {
            var cache = SPGUI.Disable();
            EditorGUI.LabelField(area, GUIContent.none, new GUIContent("*Parameterless Action*"));
            cache.Reset();
        }

        private void DrawAdvanced_DestroyTarget(Rect area, SerializedProperty property)
        {
            var cache = SPGUI.Disable();
            EditorGUI.LabelField(area, GUIContent.none, new GUIContent("*Parameterless Action*"));
            cache.Reset();
        }

        #endregion

        #region Utils

        private static string[] _defaultTriggerActivationTypeDisplayNames = new string[]
        {
            "Trigger All On Target",
            "Trigger Selected Target",
            "Send Message",
            "Call Method On Selected Target",
            "Enable Target (GameObject)",
            "Disable Target (GameObject)",
            "Toggle Target (GameObject)",
            "Destroy Target"
        };
        public struct TriggerActivationInfo
        {
            public TriggerActivationType ActivationType;
            public IEnumerable<string> DropdownDisplayNames;
            public int DropdownSelectedIndex;
            public SerializedProperty ActivationTypeProperty;
        }
        public static TriggerActivationInfo GetTriggerActivationInfo(SerializedProperty triggerTargetProperty)
        {
            var result = new TriggerActivationInfo();
            result.ActivationTypeProperty = triggerTargetProperty.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_ACTIVATIONTYPE);
            result.ActivationType = result.ActivationTypeProperty.GetEnumValue<TriggerActivationType>();
            result.DropdownDisplayNames = _defaultTriggerActivationTypeDisplayNames;

            var methodNameProp = triggerTargetProperty.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_METHODNAME);
            var targ = triggerTargetProperty.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_TRIGGERABLETARG)?.objectReferenceValue;
            targ = targ.ReduceIfProxy() as UnityEngine.Object;
            if (targ is Component && !(targ is Transform))
            {
                var arr = result.DropdownDisplayNames.ToArray();
                var nm = targ.GetType().Name;
                arr[4] = string.Format("Enable Target [{0}]", nm);
                arr[5] = string.Format("Disable Target [{0}]", nm);
                arr[6] = string.Format("Toggle Target [{0}]", nm);
                if (result.ActivationType == TriggerActivationType.CallMethodOnSelectedTarget && !string.IsNullOrEmpty(methodNameProp?.stringValue))
                {
                    arr[3] = string.Format("Call Method [{0}->{1}]", nm, methodNameProp.stringValue);
                }
                result.DropdownDisplayNames = arr;
            }
            else if (result.ActivationType == TriggerActivationType.CallMethodOnSelectedTarget && !string.IsNullOrEmpty(methodNameProp?.stringValue))
            {
                var arr = result.DropdownDisplayNames.ToArray();
                var nm = targ?.GetType().Name ?? "null";
                arr[3] = string.Format("Call Method [{0}->{1}]", nm, methodNameProp.stringValue);
                result.DropdownDisplayNames = arr;
            }

            switch (result.ActivationType)
            {
                case TriggerActivationType.TriggerAllOnTarget:
                case TriggerActivationType.TriggerSelectedTarget:
                case TriggerActivationType.SendMessage:
                case TriggerActivationType.CallMethodOnSelectedTarget:
                    result.DropdownSelectedIndex = (int)result.ActivationType;
                    break;
                case TriggerActivationType.EnableTarget:
                    {
                        var argProp = triggerTargetProperty.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_METHODNAME);
                        switch (ConvertUtil.ToEnum<EnableMode>(argProp.stringValue))
                        {
                            case EnableMode.Enable:
                                result.DropdownSelectedIndex = 4;
                                break;
                            case EnableMode.Disable:
                                result.DropdownSelectedIndex = 5;
                                break;
                            case EnableMode.Toggle:
                                result.DropdownSelectedIndex = 6;
                                break;
                        }
                    }
                    break;
                case TriggerActivationType.DestroyTarget:
                    result.DropdownSelectedIndex = 7;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Validates target object is appropriate for the activation type. Null is considered valid.
        /// </summary>
        /// <param name="property"></param>
        /// <returns>Returns false if invalid</returns>
        public static bool ValidateTriggerTargetProperty(SerializedProperty property)
        {
            if (property == null) return false;

            var targProp = property.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_TRIGGERABLETARG);
            if (targProp.objectReferenceValue == null) return true;

            var actProp = property.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_ACTIVATIONTYPE);
            var act = actProp.GetEnumValue<TriggerActivationType>();

            if (!EventTriggerTarget.IsValidTriggerTarget(targProp.objectReferenceValue, act))
            {
                targProp.objectReferenceValue = null;
                return false;
            }
            else
            {
                return true;
            }
        }

        public static UnityEngine.Object TargetObjectField(Rect position, GUIContent label, UnityEngine.Object target)
        {
            EditorGUI.BeginChangeCheck();

            UnityEngine.Object result = target;
            if (GameObjectUtil.IsGameObjectSource(result))
                result = GameObjectUtil.GetGameObjectFromSource(result);
            result = EditorGUI.ObjectField(position, label, result, typeof(UnityEngine.Object), true);
            if (EditorGUI.EndChangeCheck())
            {
                if (GameObjectUtil.IsGameObjectSource(result))
                {
                    return GameObjectUtil.GetGameObjectFromSource(result).transform;
                }
                else
                {
                    return result;
                }
            }
            else
            {
                return target;
            }
        }

        public static void DrawCombinedObjectField(Rect position, SerializedProperty property)
        {
            var targProp = property.FindPropertyRelative(PROP_TRIGGERABLETARG);
            var actProp = property.FindPropertyRelative(PROP_ACTIVATIONTYPE);

            var target = targProp.objectReferenceValue;
            var acttype = actProp.GetEnumValue<TriggerActivationType>();
            switch(acttype)
            {
                case TriggerActivationType.TriggerSelectedTarget:
                    {
                        var drawer = new com.spacepuppyeditor.Windows.ComponentDropDownWindowSelector<UnityEngine.Object>()
                        {
                            ObjectType = typeof(ITriggerable),
                            ComponentFilterPredicate = (c) => typeof(ITriggerable).IsInstanceOfType(c),
                            AllowSceneObjects = true,
                            IncludeGameObject = false,
                        };
                        targProp.objectReferenceValue = drawer.DrawObjectField(position, GUIContent.none, target);
                    }
                    break;
                case TriggerActivationType.SendMessage:
                    {
                        var r0 = new Rect(position.xMin, position.yMin, position.width / 2f, position.height);
                        var r1 = new Rect(position.xMin + position.width / 2f, position.yMin, position.width / 2f, position.height);

                        EditorGUI.PropertyField(r0, property.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_METHODNAME), GUIContent.none, false);
                        targProp.objectReferenceValue = TransformOrProxyField(r1, GUIContent.none, target);
                    }
                    break;
                case TriggerActivationType.CallMethodOnSelectedTarget:
                    {
                        if(target)
                        {
                            if (SPEditorGUI.XButton(ref position, "Clear Selected Object", true))
                            {
                                target = null;
                                targProp.objectReferenceValue = null;
                                GUI.changed = true;
                                return;
                            }
                            SPEditorGUI.RefButton(ref position, target, true);

                            var targGo = GameObjectUtil.GetGameObjectFromSource(target);
                            using (var elements = TempCollection.GetList<System.ValueTuple<UnityEngine.Object, int, System.Reflection.MemberInfo>>())
                            {
                                if (targGo)
                                {
                                    var objs = targGo.GetComponents<Component>().Cast<UnityEngine.Object>().Prepend(targGo);
                                    int i = 0;
                                    foreach(var obj in objs)
                                    {
                                        foreach(var m in GetCallableMethodsOnTarget(obj, false))
                                        {
                                            elements.Add(new System.ValueTuple<UnityEngine.Object, int, System.Reflection.MemberInfo>(obj, i, m));
                                        }
                                        i++;
                                    }
                                }
                                else
                                {
                                    elements.AddRange(GetCallableMethodsOnTarget(target, true).Select(m => new System.ValueTuple<UnityEngine.Object, int, System.Reflection.MemberInfo>(target, 0, m)));
                                }

                                var elementLabels = elements.Select(t => EditorHelper.TempContent(string.Format("{0} : {1} [{2}] / {3}", target.name, t.Item1.GetType().Name, t.Item2, t.Item3.Name)))
                                                            .Prepend(EditorHelper.TempContent(string.Format("{0} --no selection--", target.name)))
                                                            .ToArray();

                                var methodNameProp = property.FindPropertyRelative(PROP_METHODNAME);
                                var methodName = methodNameProp.stringValue;

                                int index = elements.IndexOf(t => object.ReferenceEquals(t.Item1, target) && t.Item3.Name == methodName) + 1;
                                index = EditorGUI.Popup(position, index, elementLabels);
                                switch (index)
                                {
                                    case -1:
                                        targProp.objectReferenceValue = null;
                                        methodNameProp.stringValue = string.Empty;
                                        break;
                                    case 0:
                                        targProp.objectReferenceValue = targGo ? targGo.transform : target;
                                        methodNameProp.stringValue = string.Empty;
                                        break;
                                    default:
                                        targProp.objectReferenceValue = elements[index - 1].Item1;
                                        methodNameProp.stringValue = elements[index - 1].Item3.Name;
                                        break;
                                }
                            }

                        }
                        else
                        {
                            goto DrawDefault;
                        }
                    }
                    break;
                case TriggerActivationType.EnableTarget:
                    {
                        var drawer = new com.spacepuppyeditor.Windows.ComponentDropDownWindowSelector<UnityEngine.Object>()
                        {
                            ComponentFilterPredicate = (c) => EventTriggerEvaluator.IsEnableableComponent(c),
                            AllowSceneObjects = true,
                            IncludeGameObject = true,
                            AllowProxy = true,
                        };
                        targProp.objectReferenceValue = drawer.DrawObjectField(position, GUIContent.none, target);
                    }
                    break;
                default:
                    DrawDefault:
                    {
                        EditorGUI.BeginChangeCheck();
                        if (GameObjectUtil.IsGameObjectSource(target))
                            target = GameObjectUtil.GetGameObjectFromSource(target);
                        target = EditorGUI.ObjectField(position, GUIContent.none, target, typeof(UnityEngine.Object), true);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (GameObjectUtil.IsGameObjectSource(target)) target = GameObjectUtil.GetGameObjectFromSource(target).transform;
                            targProp.objectReferenceValue = EventTriggerTarget.IsValidTriggerTarget(target, acttype) ? target : null;
                        }
                    }
                    break;
            }
        }

        private static Transform TransformField(Rect position, GUIContent label, UnityEngine.Object target)
        {
            if (!GameObjectUtil.IsGameObjectSource(target))
            {
                GUI.changed = true;
                target = null;
            }

            var go = GameObjectUtil.GetGameObjectFromSource(target);
            go = EditorGUI.ObjectField(position, label, go, typeof(GameObject), true) as GameObject;
            return go != null ? go.transform : null;
        }

        private static UnityEngine.Object TransformOrProxyField(Rect position, GUIContent label, UnityEngine.Object target)
        {
            if (!GameObjectUtil.IsGameObjectSource(target))
            {
                GUI.changed = true;
                target = null;
            }

            if (target == null)
            {
                var go = EditorGUI.ObjectField(position, label, target, typeof(GameObject), true) as GameObject;
                return (go != null) ? go.transform : null;
            }
            else
            {
                var targGo = GameObjectUtil.GetGameObjectFromSource(target);
                if (target is IProxy || targGo.HasComponent<IProxy>())
                {
                    using (var lst = com.spacepuppy.Collections.TempCollection.GetList<IProxy>())
                    {
                        targGo.GetComponents<IProxy>(lst);
                        for(int i = 0; i < lst.Count; i++)
                        {
                            if(lst[i].PrioritizesSelfAsTarget())
                            {
                                lst.RemoveAt(i);
                                i--;
                            }
                        }
                        if(lst.Count == 0)
                        {
                            goto DrawBasic;
                        }

                        GUIContent[] entries = new GUIContent[lst.Count + 1];
                        int index = -1;
                        entries[0] = EditorHelper.TempContent("GameObject");
                        for (int i = 0; i < lst.Count; i++)
                        {
                            entries[i + 1] = EditorHelper.TempContent(string.Format("Proxy -> [{0}]", lst[i].GetType().Name));
                            if (index < 0 && object.ReferenceEquals(target, lst[i]))
                                index = i + 1;
                        }
                        if (index < 0)
                            index = 0;

                        index = EditorGUI.Popup(position, label, index, entries);
                        if (index < 0 || index >= entries.Length)
                            return null;

                        return (index == 0) ? targGo.transform : lst[index - 1] as UnityEngine.Object;
                    }
                }

                DrawBasic:
                var go = EditorGUI.ObjectField(position, label, targGo, typeof(GameObject), true) as GameObject;
                return (go != null) ? go.transform : null;
            }
        }

        private static System.Reflection.MemberInfo[] GetCallableMethodsOnTarget(object target, bool respectProxy)
        {
            var members = target.IsProxy() ?
                          com.spacepuppy.Dynamic.DynamicUtil.GetEasilySerializedMembersFromType((target as IProxy).GetTargetType(), System.Reflection.MemberTypes.All, spacepuppy.Dynamic.DynamicMemberAccess.Write).ToArray() :
                          com.spacepuppy.Dynamic.DynamicUtil.GetEasilySerializedMembers(target, System.Reflection.MemberTypes.All, spacepuppy.Dynamic.DynamicMemberAccess.Write).ToArray();
            System.Array.Sort(members, (a, b) => string.Compare(a.Name, b.Name, true));
            return members;
        }

        #endregion


        private class SpecialVariantReferencePropertyDrawer : VariantReferencePropertyDrawer
        {
            private const float REF_SELECT_WIDTH = 85f;

            public int TriggerArgCount;
            public int CurrentTriggerArgIndex;

            public override void DrawValueField(Rect position, SerializedProperty property)
            {
                CopyValuesToHelper(property, _helper);

                //draw ref selection
                position = this.DrawRefModeSelectionDropDown(position, property, _helper);

                //draw value
                switch ((int)_helper._mode)
                {
                    case (int)VariantReference.RefMode.Value:
                        {
                            EditorGUI.BeginChangeCheck();
                            this.DrawValueFieldInValueMode(position, property, _helper);
                            if (EditorGUI.EndChangeCheck())
                            {
                                CopyValuesFromHelper(property, _helper);
                            }
                        }
                        break;
                    case (int)VariantReference.RefMode.Property:
                        this.DrawValueFieldInPropertyMode(position, property, _helper);
                        break;
                    case (int)VariantReference.RefMode.Eval:
                        this.DrawValueFieldInEvalMode(position, property, _helper);
                        break;
                    case (int)EventTriggerTarget.RefMode.TriggerArg:
                        if(this.TriggerArgCount > 1)
                        {
                            EditorGUI.BeginChangeCheck();
                            int argindex = EditorGUI.Popup(position, (int)_helper._w, Enumerable.Range(0, this.TriggerArgCount).Select(i => EditorHelper.TempContent(string.Format("Trigger Arg {0}", i))).ToArray());
                            if(EditorGUI.EndChangeCheck())
                            {
                                _helper.IntValue = argindex;
                                _helper._mode = (VariantReference.RefMode)((int)EventTriggerTarget.RefMode.TriggerArg);
                                CopyValuesFromHelper(property, _helper);
                            }
                        }
                        else
                        {
                            EditorGUI.LabelField(position, "Trigger Arg");
                        }
                        break;
                }
            }

            protected override Rect DrawRefModeSelectionDropDown(Rect position, SerializedProperty property, EditorVariantReference helper)
            {
                var r0 = new Rect(position.xMin, position.yMin, Mathf.Min(REF_SELECT_WIDTH, position.width), position.height);

                EditorGUI.BeginChangeCheck();
                var mode = (EventTriggerTarget.RefMode)EditorGUI.EnumPopup(r0, GUIContent.none, (EventTriggerTarget.RefMode)_helper._mode);
                if (EditorGUI.EndChangeCheck())
                {
                    _helper.PrepareForRefModeChange((VariantReference.RefMode)mode);
                    if (mode == EventTriggerTarget.RefMode.TriggerArg && this.TriggerArgCount > 1)
                    {
                        _helper._w = Mathf.Clamp(this.CurrentTriggerArgIndex, 0, Mathf.Max(0, this.TriggerArgCount - 1));
                    }
                    CopyValuesFromHelper(property, helper);
                }

                return new Rect(r0.xMax, r0.yMin, position.width - r0.width, r0.height);
            }

        }

    }

}
