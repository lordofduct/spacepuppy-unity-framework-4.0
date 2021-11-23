using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Mecanim;
using com.spacepuppy.Utils;
using com.spacepuppy.Collections;
using UnityEditorInternal;
using com.spacepuppy.Mecanim.Behaviours;
using Codice.Client.BaseCommands.Merge;

namespace com.spacepuppyeditor.Mecanim
{

    [CustomEditor(typeof(AnimatorOverrideSource), true)]
    public class AnimatorOverrideSourceInspector : SPEditor
    {

        public const string PROP_CONTROLLER = "_controller";
        public const string PROP_OVERRIDES = "_overrides";
        private const string PROP_KEY = "Key";
        private const string PROP_VALUE = "Value";

        private ReorderableList _lstDrawer;

        //temp stuff only exists during OnSPInspectorGUI
        private AnimationClip[] _knownKeys;
        private HashSet<AnimationClip> _usedClips = new HashSet<AnimationClip>();

        protected override void OnEnable()
        {
            base.OnEnable();

            _lstDrawer = new ReorderableList(this.serializedObject, this.serializedObject.FindProperty(PROP_OVERRIDES))
            {
                draggable = true,
                displayAdd = true,
                displayRemove = true,
                elementHeight = EditorGUIUtility.singleLineHeight,
                drawHeaderCallback = _lstDrawer_DrawHeader,
                drawElementCallback = _lstDrawer_DrawElement,
                onAddCallback = _lstDrawer_OnAdd,
            };
        }

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            var ctrlProp = this.serializedObject.FindProperty(PROP_CONTROLLER);
            ctrlProp.objectReferenceValue = SPEditorGUILayout.ObjectFieldX(ctrlProp.displayName, ctrlProp.objectReferenceValue, typeof(UnityEditor.Animations.AnimatorController), false);
            //SPEditorGUILayout.PropertyField(ctrlProp);

            if (ctrlProp.objectReferenceValue is RuntimeAnimatorController rac)
            {
                var overridesProp = this.serializedObject.FindProperty(PROP_OVERRIDES);

                _knownKeys = rac.animationClips.Distinct().ToArray();
                _usedClips.Clear();

                //first sanitize
                using (var knowntable = TempCollection.GetSet<AnimationClip>(_knownKeys))
                using (var lst = TempCollection.GetList<AnimatorOverrideSource.AnimationClipPair>())
                {
                    bool dirty = false;
                    for(int i = 0; i < overridesProp.arraySize; i++)
                    {
                        var el = overridesProp.GetArrayElementAtIndex(i);
                        var keyprop = el.FindPropertyRelative(PROP_KEY);
                        var keyclip = keyprop.objectReferenceValue as AnimationClip;
                        if(keyclip == null || _usedClips.Contains(keyclip) || !knowntable.Contains(keyclip))
                        {
                            dirty = true;
                        }
                        else
                        {
                            lst.Add(new AnimatorOverrideSource.AnimationClipPair()
                            {
                                Key = keyclip,
                                Value = el.FindPropertyRelative(PROP_VALUE).objectReferenceValue as AnimationClip,
                            });
                            _usedClips.Add(keyclip);
                        }
                    }
                    if(dirty)
                    {
                        overridesProp.arraySize = lst.Count;
                        for(int i = 0; i < lst.Count; i++)
                        {
                            var el = overridesProp.GetArrayElementAtIndex(i);
                            el.FindPropertyRelative(PROP_KEY).objectReferenceValue = lst[i].Key;
                            el.FindPropertyRelative(PROP_VALUE).objectReferenceValue = lst[i].Value;
                        }
                    }
                }

                //draw the inspector
                _lstDrawer.displayAdd = _knownKeys.Count(o => !_usedClips.Contains(o)) > 0;
                _lstDrawer.displayRemove = _usedClips.Count > 0;
                _lstDrawer.DoLayoutList();
            }

            _knownKeys = null;
            _usedClips.Clear();

            this.serializedObject.ApplyModifiedProperties();
        }

        #region List Drawer Methods

        private void _lstDrawer_DrawHeader(Rect area)
        {
            EditorGUI.LabelField(area, "Overrides");
        }

        private void _lstDrawer_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            var prop = _lstDrawer.serializedProperty.GetArrayElementAtIndex(index);
            var keyprop = prop.FindPropertyRelative(PROP_KEY);
            var valueprop = prop.FindPropertyRelative(PROP_VALUE);

            var r0 = new Rect(area.xMin, area.yMin, area.width * 0.5f, area.height);
            var r1 = new Rect(r0.xMax, area.yMin, area.width - r0.width, area.height);

            var keyclip = keyprop.objectReferenceValue as AnimationClip;
            if(keyclip != null)
            {
                var keys = _knownKeys.Where(o => o == keyclip || !_usedClips.Contains(o)).ToArray();
                var keylabels = keys.Select(o => EditorHelper.TempContent(o.name)).ToArray();
                int i = keys.IndexOf(keyclip);
                EditorGUI.BeginChangeCheck();
                i = EditorGUI.Popup(r0, i, keylabels);
                if(EditorGUI.EndChangeCheck())
                {
                    keyprop.objectReferenceValue = i >= 0 ? keys[i] : null;
                }
            }

            valueprop.objectReferenceValue = SPEditorGUI.ObjectFieldX(r1, valueprop.objectReferenceValue, typeof(AnimationClip), false);
        }

        private void _lstDrawer_OnAdd(ReorderableList lst)
        {
            var keyclip = _knownKeys.FirstOrDefault(o => !_usedClips.Contains(o));
            if (keyclip == null) return;

            lst.serializedProperty.arraySize++;
            var el = lst.serializedProperty.GetArrayElementAtIndex(lst.serializedProperty.arraySize - 1);
            el.FindPropertyRelative(PROP_KEY).objectReferenceValue = keyclip;
            el.FindPropertyRelative(PROP_VALUE).objectReferenceValue = null;
        }

        #endregion

    }

}
