#if SP_ADDRESSABLES
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Addressables;
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace com.spacepuppyeditor.Addressables
{

    [CustomPropertyDrawer(typeof(DynamicAssetReference), true)]
    public class DynamicAssetReferencePropertyDrawer : PropertyDrawer
    {

        const string PROP_DIRECTREF = "_directReference";
        private static readonly System.Type _internalDrawerKlass;
        private static readonly GUIContent[] _dropdownOptions = new GUIContent[] { new GUIContent("Direct"), new GUIContent("Addressable") };

        private bool _initialized;
        private PropertyDrawer _internalDrawer;
        private GUIStyle _rightAlignedLabel;
        private Dictionary<int, int> _dropdownSelectionByHash = new Dictionary<int, int>();

        static DynamicAssetReferencePropertyDrawer()
        {
            _internalDrawerKlass = TypeUtil.FindType("UnityEditor.AddressableAssets.GUI.AssetReferenceDrawer");
        }

        void Initialize()
        {
            _initialized = true;

            _rightAlignedLabel = new GUIStyle(GUI.skin.label);
            _rightAlignedLabel.alignment = TextAnchor.MiddleRight;

            _internalDrawer = _internalDrawerKlass != null ? System.Activator.CreateInstance(_internalDrawerKlass) as PropertyDrawer : null;
            if (_internalDrawer != null)
            {
                DynamicUtil.SetValueDirect(_internalDrawer, "m_FieldInfo", this.fieldInfo);
            }
            else
            {
                Debug.LogWarning("Could not create instance of AssetReferenceDrawer. This version of Spacepuppy expects an AssetReferenceDrawer to exist with a shape similar to that found in version 1.19.19 of Addressables.");
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!_initialized) this.Initialize();
            return _internalDrawer?.GetPropertyHeight(property, label) ?? EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.LabelField(position, label, EditorHelper.TempContent("Multi-Object editing is not supported."));
                return;
            }

            if (!_initialized) this.Initialize();

#if UNITY_2022_1_OR_NEWER
            var assetref = property.boxedValue as DynamicAssetReference;
#else
            var assetref = EditorHelper.GetTargetObjectOfProperty(property) as DynamicAssetReference;
#endif

            const float DROPDOWN_WIDTH = 90f;
            var rect_property = SPEditorGUI.SafePrefixLabel(position, label);
            Rect rect_dropdown;
            if (label.HasContent())
            {
                rect_dropdown = new Rect(rect_property.xMin - DROPDOWN_WIDTH - 1, rect_property.yMin, DROPDOWN_WIDTH, EditorGUIUtility.singleLineHeight);
            }
            else
            {
                rect_dropdown = new Rect(rect_property.xMin, rect_property.yMin, DROPDOWN_WIDTH, EditorGUIUtility.singleLineHeight);
                rect_property = new Rect(rect_dropdown.xMax + 1, rect_property.yMin, rect_property.width - DROPDOWN_WIDTH - 1, rect_property.height);
            }
            if (SPEditorGUI.XButton(ref rect_property))
            {
                if (assetref != null)
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "Modified DynamicAssetReference");
                    assetref.SetEditorAsset(null);
                    property.CommitDirectChanges(true);
                }
                else
                {
                    property.FindPropertyRelative(PROP_DIRECTREF).objectReferenceValue = null;
                }
            }

            int selection = this.GetDropdownSelection(property) ?? ((_internalDrawer == null || (assetref?.IsDirectAssetReference ?? false)) ? 0 : 1);
            switch (selection)
            {
                case 0:
                    {
                        EditorGUI.BeginChangeCheck();

                        var exptp = assetref?.ExpectedAssetType ?? typeof(UnityEngine.Object);
                        var obj = EditorGUI.ObjectField(rect_property, GUIContent.none, assetref?.DirectAssetReference, exptp, true);
                        if (EditorGUI.EndChangeCheck())
                        {
                            obj = ObjUtil.GetAsFromSource(exptp, obj) as UnityEngine.Object;

                            if (assetref != null)
                            {
                                Undo.RecordObject(property.serializedObject.targetObject, "Modified DynamicAssetReference");
                                assetref.SetDirectReference(obj);
                                property.CommitDirectChanges(true);
                            }
                            else
                            {
                                property.FindPropertyRelative(PROP_DIRECTREF).objectReferenceValue = obj;
                            }
                        }

                        if (EditorGUI.Popup(rect_dropdown, 0, _dropdownOptions) != 0)
                        {
                            selection = 1;
                            Undo.RecordObject(property.serializedObject.targetObject, "Modified DynamicAssetReference");
                            assetref.SetEditorAsset(obj);
                            property.CommitDirectChanges(true);
                        }
                    }
                    break;
                case 1:
                    {
                        _internalDrawer.OnGUI(rect_property, property, GUIContent.none);
                        if (EditorGUI.Popup(rect_dropdown, 1, _dropdownOptions) != 1)
                        {
                            selection = 0;
                            if (assetref != null)
                            {
                                Undo.RecordObject(property.serializedObject.targetObject, "Modified DynamicAssetReference");
                                assetref.SetDirectReference(assetref.editorAsset);
                                property.CommitDirectChanges(true);
                            }
                        }
                    }
                    break;
            }
            this.SetDropdownSelection(property, selection);
        }

        private int? GetDropdownSelection(SerializedProperty property)
        {
            int hash = com.spacepuppyeditor.Internal.PropertyHandlerCache.GetIndexRespectingPropertyHash(property);
            int result;
            if (_dropdownSelectionByHash.TryGetValue(hash, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        private void SetDropdownSelection(SerializedProperty property, int selection)
        {
            _dropdownSelectionByHash[com.spacepuppyeditor.Internal.PropertyHandlerCache.GetIndexRespectingPropertyHash(property)] = selection;
        }

        private void ClearDropdownSelection(SerializedProperty property, int selection)
        {
            _dropdownSelectionByHash.Remove(com.spacepuppyeditor.Internal.PropertyHandlerCache.GetIndexRespectingPropertyHash(property));
        }

    }

}
#endif
