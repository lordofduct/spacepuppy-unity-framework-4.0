using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using com.spacepuppy.Project;
using com.spacepuppy.Spawn;
using com.spacepuppy.Utils;
using com.spacepuppyeditor.Core.Project;
using com.spacepuppyeditor.Core;
using com.spacepuppyeditor.Windows;
using UnityEngine.UIElements;

namespace com.spacepuppyeditor
{

    [CustomEditor(typeof(SpawnableAssetSet))]
    public class SpawnableAssetSetInspector : QueryableAssetSetInspector
    {

        private float _totalWeight;

        protected override void OnEnable()
        {
            base.OnEnable();


            this.TypeRefDrawer.DefaultType = typeof(UnityEngine.Object);
            this.TypeRefDrawer.TypeEnumerator = TypeUtil.GetTypes(tp =>
            {
                return tp.IsInterface || TypeUtil.IsType(tp, typeof(UnityEngine.GameObject)) || TypeUtil.IsType(tp, typeof(UnityEngine.Component)) || TypeUtil.IsType(tp, typeof(ISpawnable)) || (this.SupportNestedAssetSet && TypeUtil.IsType(tp, typeof(IAssetSet)) && TypeUtil.IsType(tp, typeof(ISpawnable)));
            });

            this.AssetArrayDrawer.DragDropElementFilter = (o) =>
            {
                var obj = ObjUtil.GetAsFromSource(this.RestrictedType, o) as UnityEngine.Object;
                if (obj == this.serializedObject.targetObject) return null;

                if (obj is UnityEngine.GameObject || obj is UnityEngine.Component || obj is ISpawnable)
                {
                    return obj;
                }
                else if (this.SupportNestedAssetSet && obj is IAssetSet && obj is ISpawnable)
                {
                    return obj;
                }

                return null;
            };
            this.AssetArrayDrawer.InternalDrawer = new AssetWeightJoinedDrawer(this);
            this.AssetArrayDrawer.ElementAdded += AssetArrayDrawer_ElementAdded;
        }

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawDefaultInspectorExcept(QueryableAssetSet.PROP_ASSETTYPE, QueryableAssetSet.PROP_ASSETS, SpawnableAssetSet.PROP_WEIGHTS, SpawnableAssetSet.PROP_LOGIC);

            this.DrawAssetTypeProperty();

            _totalWeight = 0f;
            var prop_weights = this.serializedObject.FindProperty(SpawnableAssetSet.PROP_WEIGHTS);
            for (int i = 0; i < prop_weights.arraySize; i++)
            {
                _totalWeight += prop_weights.GetArrayElementAtIndex(i).floatValue;
            }
            this.DrawAssetsProperty();

            this.DrawPropertyField(SpawnableAssetSet.PROP_LOGIC);

            this.serializedObject.ApplyModifiedProperties();

            if (!this.serializedObject.isEditingMultipleObjects && !Application.isPlaying)
            {
                EditorGUILayout.Space(20f);
                this.DrawQuickAddUtils();
            }
        }

        private void AssetArrayDrawer_ElementAdded(object sender, System.EventArgs e)
        {
            var prop_assets = this.serializedObject.FindProperty(SpawnableAssetSet.PROP_ASSETS);
            var prop_weights = this.serializedObject.FindProperty(SpawnableAssetSet.PROP_WEIGHTS);

            int oldcnt = prop_weights.arraySize;
            prop_weights.arraySize = prop_assets.arraySize;
            for (int i = oldcnt; i < prop_weights.arraySize; i++)
            {
                prop_weights.GetArrayElementAtIndex(i).floatValue = 1f;
            }
        }

        private class AssetWeightJoinedDrawer : PropertyDrawer
        {

            private SpawnableAssetSetInspector _owner;

            public AssetWeightJoinedDrawer(SpawnableAssetSetInspector owner)
            {
                _owner = owner;
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                float h;
                if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;
                return EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
            {
                if (EditorHelper.AssertMultiObjectEditingNotSupported(area, property, label)) return;

                const float MARGIN = 1.0f;
                const float WEIGHT_FIELD_WIDTH = 60f;
                const float PERC_FIELD_WIDTH = 45f;
                const float FULLWEIGHT_WIDTH = WEIGHT_FIELD_WIDTH + PERC_FIELD_WIDTH;

                int elementIndex = _owner.AssetArrayDrawer.CurrentDrawingArrayElementIndex;
                var prop_weights = _owner.serializedObject.FindProperty(SpawnableAssetSet.PROP_WEIGHTS);
                if (prop_weights.arraySize <= elementIndex) prop_weights.arraySize = elementIndex + 1;
                var weightProp = prop_weights.GetArrayElementAtIndex(elementIndex);

                Rect valueRect;
                if (area.width > FULLWEIGHT_WIDTH)
                {
                    var top = area.yMin + MARGIN;
                    var labelRect = new Rect(area.xMin, top, EditorGUIUtility.labelWidth - FULLWEIGHT_WIDTH, EditorGUIUtility.singleLineHeight);
                    var weightRect = new Rect(area.xMin + EditorGUIUtility.labelWidth - FULLWEIGHT_WIDTH, top, WEIGHT_FIELD_WIDTH, EditorGUIUtility.singleLineHeight);
                    var percRect = new Rect(area.xMin + EditorGUIUtility.labelWidth - PERC_FIELD_WIDTH, top, PERC_FIELD_WIDTH, EditorGUIUtility.singleLineHeight);
                    valueRect = new Rect(area.xMin + EditorGUIUtility.labelWidth, top, area.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

                    float weight = weightProp.floatValue;

                    EditorGUI.LabelField(labelRect, label);
                    EditorHelper.SuppressIndentLevel();

                    weightProp.floatValue = EditorGUI.FloatField(weightRect, weight);
                    float p = (_owner._totalWeight > 0f) ? (100f * weight / _owner._totalWeight) : ((elementIndex == 0) ? 100f : 0f);
                    EditorGUI.LabelField(percRect, string.Format("{0:0.#}%", p));
                }
                else
                {
                    //Draw Triggerable - this is the simple case to make a clean designer set up for newbs
                    var top = area.yMin + MARGIN;
                    var labelRect = new Rect(area.xMin, top, area.width, EditorGUIUtility.singleLineHeight);

                    valueRect = EditorGUI.PrefixLabel(labelRect, label);
                    EditorHelper.SuppressIndentLevel();
                }

                property.objectReferenceValue = SPEditorGUI.AdvancedObjectField(valueRect,
                    GUIContent.none,
                    property.objectReferenceValue,
                    typeof(UnityEngine.Object),
                    false,
                    false,
                    (ref UnityEngine.Object o) => _owner.AssetArrayDrawer.DragDropElementFilter(o) != null
                );
            }

        }

    }

}
