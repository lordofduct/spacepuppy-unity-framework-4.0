using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Spawn;
using com.spacepuppyeditor.Core;

namespace com.spacepuppyeditor.Spawn
{

    [CustomPropertyDrawer(typeof(SpawnablePrefabEntry))]
    public class SpawnablePrefabEntryPropertyDrawer : PropertyDrawer
    {

        internal bool DrawWeight { get; set; }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var prop_weight = property.FindPropertyRelative(SpawnablePrefabEntry.PROP_WEIGHT);
            var prop_prefab = property.FindPropertyRelative(SpawnablePrefabEntry.PROP_PREFAB);

            if (this.DrawWeight)
            {
                position = SPEditorGUI.SafePrefixLabel(position, label);
                var r0 = new Rect(position.xMin, position.yMin, Mathf.Min(position.width * 0.25f, 60f), EditorGUIUtility.singleLineHeight);
                var r1 = new Rect(r0.xMax + 1, position.yMin, position.width - r0.width - 1, EditorGUIUtility.singleLineHeight);

                SPEditorGUI.PropertyField(r0, prop_weight, GUIContent.none);
                SPEditorGUI.PropertyField(r1, prop_prefab, GUIContent.none);
            }
            else
            {
                SPEditorGUI.PropertyField(position, prop_prefab, label);
            }
        }

    }

    [CustomPropertyDrawer(typeof(SpawnablePrefabEntryCollectionAttribute))]
    public class SpawnablePrefabEntryCollectionPropertyDrawer : WeightedValueCollectionPropertyDrawer
    {

    }

}
