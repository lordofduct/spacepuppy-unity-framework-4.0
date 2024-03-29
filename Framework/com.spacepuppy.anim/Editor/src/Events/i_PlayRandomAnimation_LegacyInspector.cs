﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Anim;
using com.spacepuppy.Anim.Events;
using com.spacepuppy.Anim.Legacy;
using com.spacepuppy.Utils;

using com.spacepuppyeditor.Core;
using com.spacepuppyeditor.Core.Events;

namespace com.spacepuppyeditor.Anim.Events
{

    [CustomEditor(typeof(i_PlayRandomAnimation_Legacy), true)]
    public class i_PlayRandomAnimation_LegacyInspector : SPEditor
    {

        public const string PROP_TARGETANIMATOR = "_targetAnimator";
        public const string PROP_CLIPS = "_clips";

        public const string PROP_CLIP_WEIGHT = "_weight";
        public const string PROP_CLIP_MODE = "_mode";
        public const string PROP_CLIP_ID = "_id";
        public const string PROP_CLIP_CLIP = "_clip";
        public const string PROP_CLIP_SETTINGSMASK= "SettingsMask";
        public const string PROP_CLIP_SETTINGS = "Settings";
        public const string PROP_CLIP_QUEUEMODE = "QueueMode";
        public const string PROP_CLIP_PLAYMODE = "PlayMode";
        public const string PROP_CLIP_CROSSFADEDUR = "CrossFadeDur";

        private TriggerableTargetObjectPropertyDrawer _targetDrawer = new TriggerableTargetObjectPropertyDrawer()
        {
            ManuallyConfigured = true,
            SearchChildren = false,
            ChoiceSelector = new com.spacepuppyeditor.Core.MultiTypeComponentChoiceSelector()
            {
                AllowedTypes = new System.Type[] { typeof(Animation), typeof(ISPAnimationSource), typeof(ISPAnimator) }
            }
        };
        private WeightedValueCollectionPropertyDrawer _clipsDrawer;

        protected override void OnEnable()
        {
            base.OnEnable();

            _clipsDrawer = new PlayRandomAnimationWeightedPropertyDrawer()
            {
                ManuallyConfigured = true,
                WeightPropertyName = PlayAnimInfoPropertyDrawer.PROP_WEIGHT,
                DrawElementAtBottom = true,
                Draggable = true,
                ElementLabelFormatString = "Clip {0:00}",
                InternalDrawer = new PlayAnimInfoPropertyDrawer()
                {
                    DrawFlat = true
                }
            };
            _clipsDrawer.ElementAdded += OnElementAdded;
        }

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(EditorHelper.PROP_ORDER);
            this.DrawPropertyField(EditorHelper.PROP_ACTIVATEON);

            this.DrawTargetAnimatorProperty();

            var clipsProp = this.serializedObject.FindProperty(PROP_CLIPS);
            var clipsLabel = EditorHelper.TempContent(clipsProp.displayName);
            var h = _clipsDrawer.GetPropertyHeight(clipsProp, clipsLabel);
            var r = EditorGUILayout.GetControlRect(true, h);
            _clipsDrawer.OnGUI(r, clipsProp, clipsLabel);

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, EditorHelper.PROP_ORDER, EditorHelper.PROP_ACTIVATEON, PROP_TARGETANIMATOR, PROP_CLIPS);

            this.serializedObject.ApplyModifiedProperties();
        }


        private void DrawTargetAnimatorProperty()
        {
            var targWrapperProp = this.serializedObject.FindProperty(PROP_TARGETANIMATOR);
            var targProp = targWrapperProp.FindPropertyRelative(TriggerableTargetObjectPropertyDrawer.PROP_TARGET);

            _targetDrawer.ManuallyConfigured = true;

            var label = EditorHelper.TempContent(targWrapperProp.displayName);
            var rect = EditorGUILayout.GetControlRect(true, _targetDrawer.GetPropertyHeight(targWrapperProp, label));
            _targetDrawer.OnGUI(rect, targWrapperProp, label);


            var obj = targProp.objectReferenceValue;
            if (obj == null || i_PlayAnimation_Legacy.IsAcceptibleAnimator(obj))
                return;

            var go = GameObjectUtil.GetGameObjectFromSource(obj);

            ISPAnimationSource src;
            if (go.GetComponent<ISPAnimationSource>(out src))
            {
                targProp.objectReferenceValue = src as UnityEngine.Object;
                return;
            }

            Animation anim;
            if (go.GetComponent<Animation>(out anim))
            {
                targProp.objectReferenceValue = anim;
                return;
            }

            ISPAnimator animator;
            if (go.GetComponent<ISPAnimator>(out animator))
            {
                targProp.objectReferenceValue = animator as UnityEngine.Object;
                return;
            }
        }

        private void OnElementAdded(object sender, System.EventArgs e)
        {
            var drawer = sender as ReorderableArrayPropertyDrawer;
            if (drawer == null || drawer.CurrentReorderableList == null) return;

            var lst = drawer.CurrentReorderableList;
            var infoProp = lst.serializedProperty.GetArrayElementAtIndex(lst.index);
            var settingsProp = infoProp.FindPropertyRelative(PlayAnimInfoPropertyDrawer.PROP_SETTINGS);
            if(settingsProp != null)
            {
                settingsProp.FindPropertyRelative("weight").floatValue = 1f;
                settingsProp.FindPropertyRelative("speed").floatValue = 1f;
                settingsProp.FindPropertyRelative("layer").intValue = 0;
                settingsProp.FindPropertyRelative("wrapMode").intValue = (int)WrapMode.Default;
                settingsProp.FindPropertyRelative("blendMode").intValue = (int)AnimationBlendMode.Blend;
                var timeProp = settingsProp.FindPropertyRelative("timeSupplier");
                timeProp.FindPropertyRelative(SPTimePropertyDrawer.PROP_TIMESUPPLIERTYPE).intValue = (int)DeltaTimeType.Normal;
                timeProp.FindPropertyRelative(SPTimePropertyDrawer.PROP_TIMESUPPLIERNAME).stringValue = null;
            }
        }


        #region Special Types

        private class PlayRandomAnimationWeightedPropertyDrawer : WeightedValueCollectionPropertyDrawer
        {
            
            protected override void DrawElementValue(Rect area, SerializedProperty element, GUIContent label, int elementIndex)
            {
                var controller = element.serializedObject.FindProperty(PROP_TARGETANIMATOR).FindPropertyRelative(TriggerableTargetObjectPropertyDrawer.PROP_TARGET).objectReferenceValue;
                if (controller is Animation || controller is SPLegacyAnimController)
                {
                    var modeProp = element.FindPropertyRelative(PlayAnimInfoPropertyDrawer.PROP_MODE);
                    switch (modeProp.GetEnumValue<i_PlayAnimation_Legacy.PlayByMode>())
                    {
                        case i_PlayAnimation_Legacy.PlayByMode.PlayAnim:
                            {
                                var clipProp = element.FindPropertyRelative(PlayAnimInfoPropertyDrawer.PROP_CLIP);
                                var obj = EditorGUI.ObjectField(area, GUIContent.none, clipProp.objectReferenceValue, typeof(UnityEngine.Object), true);
                                if (obj == null || obj is AnimationClip || obj is IScriptableAnimationClip)
                                    clipProp.objectReferenceValue = obj;
                                else if (GameObjectUtil.IsGameObjectSource(obj))
                                    clipProp.objectReferenceValue = ObjUtil.GetAsFromSource<IScriptableAnimationClip>(obj) as UnityEngine.Object;
                            }
                            break;
                        default:
                            {
                                SPEditorGUI.PropertyField(area, element.FindPropertyRelative(PlayAnimInfoPropertyDrawer.PROP_ID), GUIContent.none);
                            }
                            break;
                    }
                }
                else if (controller is ISPAnimator)
                {
                    var propId = element.FindPropertyRelative(PlayAnimInfoPropertyDrawer.PROP_ID);
                    propId.stringValue = i_PlayAnimation_LegacyInspector.DrawSPAnimatorFunctionPopup(area, GUIContent.none, controller as ISPAnimator, propId.stringValue);
                }
                else if (controller is ISPAnimationSource)
                {
                    SPEditorGUI.PropertyField(area, element.FindPropertyRelative(PlayAnimInfoPropertyDrawer.PROP_ID), GUIContent.none);
                }
            }

        }
        
        #endregion

    }

}
