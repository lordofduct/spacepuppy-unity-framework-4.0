using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

using com.spacepuppyeditor.Internal;
using System.Reflection;

namespace com.spacepuppyeditor.Events
{

    [CustomPropertyDrawer(typeof(BaseSPEvent), true)]
    public class SPEventPropertyDrawer : PropertyDrawer
    {

        private const float ARG_MARGIN = 2f;
        private const float BOTTOM_MARGIN = 4f; //margin at the bottom of the drawer
        private const float MARGIN = 2.0f;
        private const float BTN_ACTIVATE_HEIGHT = 24f;

        public const string PROP_TARGETS = "_targets";
        private const string PROP_WEIGHT = "_weight";

        #region Fields

        private GUIContent _currentLabel;
        private CachedReorderableList _targetList;
        private EventTriggerTargetPropertyDrawer _triggerTargetDrawer = new EventTriggerTargetPropertyDrawer();

        private bool __drawWeight;
        private float _totalWeight = 0f;

        private bool __alwaysExpanded;

        private string __argdesc;

        private System.Reflection.ParameterInfo[] _spdelegateParameters;

        #endregion

        #region CONSTRUCTOR

        private void Init(SerializedProperty prop, GUIContent label)
        {
            _currentLabel = label;

            _targetList = CachedReorderableList.GetListDrawer(prop.FindPropertyRelative(PROP_TARGETS), _targetList_DrawHeader, _targetList_DrawElement, _targetList_OnAdd);

            _triggerTargetDrawer.DrawWeight = this.DrawWeight;

            var sptp = this.fieldInfo?.FieldType;
            if(TypeUtil.IsType(sptp, typeof(BaseSPDelegate<>)))
            {
                int argcount = 1;
                while (sptp != null)
                {
                    if (sptp.IsGenericType && sptp.GetGenericTypeDefinition() == typeof(BaseSPDelegate<>))
                    {
                        var dtp = sptp.GetGenericArguments().FirstOrDefault();
                        if (TypeUtil.IsType(dtp, typeof(System.Delegate)))
                        {
                            _spdelegateParameters = dtp.GetMethod("Invoke")?.GetParameters();
                            argcount = _spdelegateParameters?.Length ?? 1;
                        }
                        break;
                    }
                    sptp = sptp.BaseType;
                }
                _triggerTargetDrawer.TriggerArgCount = argcount;
            }
            else
            {
                _triggerTargetDrawer.TriggerArgCount = 1;
            }
        }

        #endregion

        #region Properties

        public bool DrawWeight
        {
            get { return this.fieldInfo?.GetCustomAttribute<SPEvent.ConfigAttribute>()?.Weighted ?? __drawWeight; }
            set
            {
                __drawWeight = value;
            }
        }

        public bool AlwaysExpanded
        {
            get { return this.fieldInfo?.GetCustomAttribute<SPEvent.ConfigAttribute>()?.AlwaysExpanded ?? __alwaysExpanded; }
            set
            {
                __alwaysExpanded = value;
            }
        }

        public string ArgumentDescription
        {
            get { return this.fieldInfo?.GetCustomAttribute<SPEvent.ConfigAttribute>()?.ArgDescription ?? __argdesc; }
            set { __argdesc = value; }
        }

        public System.Action<Rect, SerializedProperty, int> OnDrawCustomizedEntryLabel
        {
            get;
            set;
        }

        public bool DoNotDrawParensOnLabel { get; set; }

        #endregion

        #region Draw Methods

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h;
            if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;

            this.Init(property, label);

            if (this.AlwaysExpanded || property.isExpanded)
            {
                h = MARGIN * 2f;
                h += this.GetTargetsHeight(property, label);

                if (Application.isPlaying)
                {
                    h += BTN_ACTIVATE_HEIGHT;
                }
            }
            else
            {
                h = EditorGUIUtility.singleLineHeight;
            }

            return h + BOTTOM_MARGIN;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

            if (!this.DoNotDrawParensOnLabel) label.text += " ( )";

            this.Init(property, label);

            bool alwaysExpanded = this.AlwaysExpanded;
            //const float WIDTH_FOLDOUT = 5f;
            //if(!alwaysExpanded) property.isExpanded = EditorGUI.Foldout(new Rect(position.xMin, position.yMin, WIDTH_FOLDOUT, EditorGUIUtility.singleLineHeight), property.isExpanded, GUIContent.none);
            if (!alwaysExpanded) property.isExpanded = EditorGUI.Foldout(new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, GUIContent.none, true);

            if (alwaysExpanded || property.isExpanded)
            {
                if (this.DrawWeight) this.CalculateTotalWeight();

                if (!alwaysExpanded) GUI.Box(position, GUIContent.none);

                position = new Rect(position.xMin + MARGIN, position.yMin + MARGIN, position.width - MARGIN * 2f, position.height - MARGIN * 2f);
                EditorGUI.BeginProperty(position, label, property);

                position = this.DrawTargets(position, property);

                EditorGUI.EndProperty();

                if (Application.isPlaying && !property.serializedObject.isEditingMultipleObjects)
                {
                    var w = position.width * 0.6f;
                    var pad = (position.width - w) / 2f;
                    var rect = new Rect(position.xMin + pad, position.yMax + -BTN_ACTIVATE_HEIGHT + 2f, w, 20f);
                    if (GUI.Button(rect, "Activate Trigger"))
                    {
                        var targ = EditorHelper.GetTargetObjectOfProperty(property) as SPEvent;
                        if (targ != null) targ.ActivateTrigger(property.serializedObject.targetObject, null);
                    }
                }
            }
            else
            {
                EditorGUI.BeginProperty(position, label, property);

                ReorderableListHelper.DrawRetractedHeader(position, label, this.GetTempArgDescriptionLabel());

                EditorGUI.EndProperty();
            }

        }


        private void CalculateTotalWeight()
        {
            _totalWeight = 0f;
            for (int i = 0; i < _targetList.serializedProperty.arraySize; i++)
            {
                _totalWeight += _targetList.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(PROP_WEIGHT).floatValue;
            }
        }

        protected virtual float GetTargetsHeight(SerializedProperty property, GUIContent label)
        {
            var h = _targetList.GetHeight();
            if (_targetList.count > 0 && _targetList.index >= 0)
            {
                h += ARG_MARGIN;
                var element = _targetList.serializedProperty.GetArrayElementAtIndex(_targetList.index);
                h += _triggerTargetDrawer.GetPropertyHeight(element, GUIContent.none);
            }
            else
            {
                h += ARG_MARGIN;
                h += EditorGUIUtility.singleLineHeight;
            }
            return h;
        }

        protected virtual Rect DrawTargets(Rect position, SerializedProperty property)
        {
            position = this.DrawList(position, property);
            position = this.DrawAdvancedTargetSettings(position, property);
            return position;
        }

        protected Rect DrawList(Rect position, SerializedProperty property)
        {
            var listRect = new Rect(position.xMin, position.yMin, position.width, _targetList.GetHeight());

            EditorGUI.BeginChangeCheck();
            _targetList.DoList(listRect);
            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();
            if (_targetList.index >= _targetList.count) _targetList.index = -1;

            var ev = Event.current;
            if (ev != null)
            {
                switch (ev.type)
                {
                    case EventType.DragUpdated:
                    case EventType.DragPerform:
                        {
                            if (listRect.Contains(ev.mousePosition))
                            {
                                var refs = DragAndDrop.objectReferences;
                                DragAndDrop.visualMode = refs.Length > 0 ? DragAndDropVisualMode.Link : DragAndDropVisualMode.Rejected;

                                if (ev.type == EventType.DragPerform && refs.Length > 0)
                                {
                                    ev.Use();
                                    AddObjectsToTrigger(property, refs);
                                }
                            }
                        }
                        break;
                }
            }

            return new Rect(position.xMin, listRect.yMax, position.width, position.height - listRect.height);
        }

        protected Rect DrawAdvancedTargetSettings(Rect position, SerializedProperty property)
        {
            position = new Rect(position.xMin, position.yMin + ARG_MARGIN, position.width, position.height - ARG_MARGIN);

            if (_targetList.count > 0 && _targetList.index >= 0)
            {
                var element = _targetList.serializedProperty.GetArrayElementAtIndex(_targetList.index);
                const float INDENT_MRG = 14f;
                var settingsRect = new Rect(position.xMin + INDENT_MRG, position.yMin, position.width - INDENT_MRG, _triggerTargetDrawer.GetPropertyHeight(element, GUIContent.none));
                _triggerTargetDrawer.OnGUI(settingsRect, element, GUIContent.none);

                position = new Rect(position.xMin, settingsRect.yMax, position.width, position.yMax - settingsRect.yMax);
            }
            else
            {
                EditorGUI.indentLevel++;
                var cache = SPGUI.Disable();
                EditorGUI.LabelField(position, GUIContent.none, new GUIContent("*Select Element*"));
                cache.Reset();
                position = new Rect(position.xMin, position.yMin + EditorGUIUtility.singleLineHeight, position.width, position.yMax - (position.yMin + EditorGUIUtility.singleLineHeight));
                EditorGUI.indentLevel--;
            }

            return position;
        }

        private GUIContent GetTempArgDescriptionLabel()
        {
            var str = this.ArgumentDescription;
            if (string.IsNullOrEmpty(str))
            {
                if(_spdelegateParameters != null && _spdelegateParameters.Length > 0)
                {
                    return EditorHelper.TempContent(string.Join(", ", _spdelegateParameters.Select(p => string.Format("{0} ({1})", p.Name, p.ParameterType.Name))));
                }
                else
                {
                    return GUIContent.none;
                }
            }

            return EditorHelper.TempContent(string.Format("arg: {0}", str));
        }

        #endregion

        #region ReorderableList Handlers

        private void _targetList_DrawHeader(Rect area)
        {
            EditorGUI.LabelField(area, _currentLabel, this.GetTempArgDescriptionLabel());
        }

        private void _targetList_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            var element = _targetList.serializedProperty.GetArrayElementAtIndex(index);

            var targProp = element.FindPropertyRelative(EventTriggerTargetPropertyDrawer.PROP_TRIGGERABLETARG);

            const float MARGIN = 1.0f;
            const float WEIGHT_FIELD_WIDTH = 60f;
            const float PERC_FIELD_WIDTH = 45f;
            const float FULLWEIGHT_WIDTH = WEIGHT_FIELD_WIDTH + PERC_FIELD_WIDTH;

            EditorGUI.BeginProperty(area, GUIContent.none, targProp);

            Rect trigRect;
            if (this.DrawWeight && area.width > FULLWEIGHT_WIDTH)
            {
                var top = area.yMin + MARGIN;
                var labelRect = new Rect(area.xMin, top, EditorGUIUtility.labelWidth - FULLWEIGHT_WIDTH, EditorGUIUtility.singleLineHeight);
                var weightRect = new Rect(area.xMin + EditorGUIUtility.labelWidth - FULLWEIGHT_WIDTH, top, WEIGHT_FIELD_WIDTH, EditorGUIUtility.singleLineHeight);
                var percRect = new Rect(area.xMin + EditorGUIUtility.labelWidth - PERC_FIELD_WIDTH, top, PERC_FIELD_WIDTH, EditorGUIUtility.singleLineHeight);
                trigRect = new Rect(area.xMin + EditorGUIUtility.labelWidth, top, area.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

                var weightProp = element.FindPropertyRelative(PROP_WEIGHT);
                float weight = weightProp.floatValue;

                if (this.OnDrawCustomizedEntryLabel != null)
                    this.OnDrawCustomizedEntryLabel(labelRect, element, index);
                else
                    DrawDefaultListElementLabel(labelRect, element, index);
                weightProp.floatValue = EditorGUI.FloatField(weightRect, weight);
                float p = (_totalWeight > 0f) ? (100f * weight / _totalWeight) : ((index == 0) ? 100f : 0f);
                EditorGUI.LabelField(percRect, string.Format("{0:0.#}%", p));
            }
            else
            {
                //Draw Triggerable - this is the simple case to make a clean designer set up for newbs
                var top = area.yMin + MARGIN;
                var labelRect = new Rect(area.xMin, top, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                trigRect = new Rect(area.xMin + EditorGUIUtility.labelWidth, top, area.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

                if (this.OnDrawCustomizedEntryLabel != null)
                    this.OnDrawCustomizedEntryLabel(labelRect, element, index);
                else
                    DrawDefaultListElementLabel(labelRect, element, index);
            }

            //Draw Triggerable - this is the simple case to make a clean designer set up for newbs
            //EditorGUI.BeginChangeCheck();
            //var targObj = EventTriggerTargetPropertyDrawer.TargetObjectField(trigRect, GUIContent.none, targProp.objectReferenceValue);
            //if (EditorGUI.EndChangeCheck())
            //{
            //    var actInfo = EventTriggerTargetPropertyDrawer.GetTriggerActivationInfo(element);
            //    targProp.objectReferenceValue = EventTriggerTarget.IsValidTriggerTarget(targObj, actInfo.ActivationType) ? targObj : null;
            //}
            //EditorGUI.EndProperty();
            EventTriggerTargetPropertyDrawer.DrawCombinedObjectField(trigRect, element);

            ReorderableListHelper.DrawDraggableElementDeleteContextMenu(_targetList, area, index, isActive, isFocused);
        }

        private void _targetList_OnAdd(ReorderableList lst)
        {
            lst.serializedProperty.arraySize++;
            lst.index = lst.serializedProperty.arraySize - 1;

            lst.serializedProperty.serializedObject.ApplyModifiedProperties();

            var obj = EditorHelper.GetTargetObjectOfProperty(lst.serializedProperty.GetArrayElementAtIndex(lst.index)) as EventTriggerTarget;
            if (obj != null)
            {
                obj.Clear();
                obj.Weight = 1f;
                lst.serializedProperty.serializedObject.Update();
            }
        }

        #endregion

        #region Static Utils

        public static void DrawDefaultListElementLabel(Rect area, SerializedProperty property, int index)
        {
            var r0 = new Rect(area.xMin, area.yMin, Mathf.Min(25f, area.width), EditorGUIUtility.singleLineHeight);
            var r1 = new Rect(r0.xMax, area.yMin, Mathf.Max(0f, area.width - r0.width), EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(r0, index.ToString("00:"));
            EventTriggerTargetPropertyDrawer.DrawTriggerActivationTypeDropdown(r1, property, false);
        }

        /// <summary>
        /// Adds targets to a Trigger/SPEvent.
        /// 
        /// This method applies changes to the SerializedProperty. Only call if you expect this behaviour.
        /// </summary>
        /// <param name="triggerProperty"></param>
        /// <param name="objs"></param>
        public static void AddObjectsToTrigger(SerializedProperty triggerProperty, UnityEngine.Object[] objs)
        {
            if (triggerProperty == null) throw new System.ArgumentNullException("triggerProperty");
            if (triggerProperty.serializedObject.isEditingMultipleObjects) throw new System.ArgumentException("Can not use this method for multi-selected SerializedObjects.", "triggerProperty");

            try
            {
                triggerProperty.serializedObject.ApplyModifiedProperties();
                var trigger = EditorHelper.GetTargetObjectOfProperty(triggerProperty) as BaseSPEvent;
                if (trigger == null) return;

                Undo.RecordObjects(triggerProperty.serializedObject.targetObjects, "Add Trigger Targets");
                using (var set = TempCollection.GetSet<UnityEngine.Object>())
                {
                    for (int i = 0; i < trigger.Targets.Count; i++)
                    {
                        set.Add(trigger.Targets[i].Target);
                    }

                    foreach (var obj in objs)
                    {
                        if (set.Contains(obj)) continue;
                        set.Add(obj);

                        var targ = trigger.AddNew();
                        if (EventTriggerTarget.IsValidTriggerTarget(obj, TriggerActivationType.TriggerAllOnTarget))
                            targ.ConfigureTriggerAll(obj);
                        else
                            targ.ConfigureCallMethod(obj, "");
                        targ.Weight = 1f;
                    }
                }

                triggerProperty.CommitDirectChanges(true);
                triggerProperty.serializedObject.Update();
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        #endregion

    }

}
