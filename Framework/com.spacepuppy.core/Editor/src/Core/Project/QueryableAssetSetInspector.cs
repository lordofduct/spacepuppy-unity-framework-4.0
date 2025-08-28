using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Project;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core.Project
{

    [CustomEditor(typeof(QueryableAssetSet), true)]
    public class QueryableAssetSetInspector : SPEditor
    {

        #region Fields

        private TypeReferencePropertyDrawer _typeRefDrawer = new TypeReferencePropertyDrawer();
        private ReorderableArrayPropertyDrawer _reorderableArrayDrawer = new ReorderableArrayPropertyDrawer();
        private bool _supportNestedAssetSet;
        private System.Type _restrictedType;

        #endregion

        protected TypeReferencePropertyDrawer TypeRefDrawer => _typeRefDrawer;

        protected ReorderableArrayPropertyDrawer AssetArrayDrawer => _reorderableArrayDrawer;

        protected bool SupportNestedAssetSet => _supportNestedAssetSet;

        protected System.Type RestrictedType => _restrictedType;

        protected override void OnEnable()
        {
            base.OnEnable();

            _supportNestedAssetSet = this.serializedObject.FindProperty(QueryableAssetSet.PROP_SUPPORTNESTEDGROUPS).boolValue;

            _typeRefDrawer.DefaultType = typeof(UnityEngine.Object);
            _typeRefDrawer.TypeEnumerator = TypeUtil.GetTypes(tp =>
            {
                return tp.IsInterface || (TypeUtil.IsType(tp, typeof(UnityEngine.Object))) || (_supportNestedAssetSet && TypeUtil.IsType(tp, typeof(IAssetSet)));
            });

            _reorderableArrayDrawer.AllowDragAndDrop = true;
            _reorderableArrayDrawer.AllowDragAndDropSceneObjects = false;
            _reorderableArrayDrawer.DragDropElementType = typeof(UnityEngine.Object);
            _reorderableArrayDrawer.DragDropElementFilter = (o) =>
            {
                var obj = ObjUtil.GetAsFromSource(_restrictedType, o) as UnityEngine.Object;
                if (obj == this.serializedObject.targetObject) return null;

                if (obj != null) return obj;

                if (_supportNestedAssetSet)
                {
                    return ObjUtil.GetAsFromSource<IAssetSet>(o) as UnityEngine.Object;
                }

                return null;
            };
            _reorderableArrayDrawer.FormatElementLabel = (p, i, _, _) =>
            {
                var objref = p.objectReferenceValue;
                if (objref != null)
                {
                    //var sguid = ObjUtil.GetAsFromSource<IAssetGuidIdentifiable>(objref)?.AssetId.ToString();
                    //if (string.IsNullOrEmpty(sguid)) sguid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(objref));

                    string sguid = string.Empty;
                    if (this.serializedObject.isEditingMultipleObjects)
                    {
                        sguid = ObjUtil.GetAsFromSource<IAssetGuidIdentifiable>(objref)?.AssetId.ToString();
                        if (sguid == null) sguid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(objref));
                    }
                    else if ((this.serializedObject.targetObject as QueryableAssetSet)?.TrySlowLookupGuid(objref, out System.Guid guid) ?? false)
                    {
                        sguid = guid.ToString("n");
                    }
                    else
                    {
                        sguid = ObjUtil.GetAsFromSource<IAssetGuidIdentifiable>(objref)?.AssetId.ToString();
                        if (sguid == null) sguid = "(asset is not guid identifiable at runtime)";
                    }

                    //if (EditorGUIUtility.currentViewWidth < 375f)
                    //{
                    //    sguid = sguid.Substring(0, 11) + "...";
                    //}
                    //else if (EditorGUIUtility.currentViewWidth < 640f)
                    //{
                    //    int cnt = Mathf.Clamp(Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 390) / 14) + 11, 0, sguid.Length);
                    //    sguid = sguid.Substring(0, cnt) + "...";
                    //}

                    return $"{i:00} - {sguid}";
                }
                else
                {
                    return $"{i:00} - None";
                }
            };
            _reorderableArrayDrawer.InternalDrawer = new ReorderableArrayInternalDrawer(this);
        }

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawDefaultInspectorExcept(QueryableAssetSet.PROP_ASSETTYPE, QueryableAssetSet.PROP_ASSETS);

            this.DrawAssetTypeProperty();

            this.DrawAssetsProperty();

            this.serializedObject.ApplyModifiedProperties();

            if (!this.serializedObject.isEditingMultipleObjects && !Application.isPlaying)
            {
                EditorGUILayout.Space(20f);
                this.DrawQuickAddUtils();
            }
        }

        protected virtual void DrawAssetTypeProperty()
        {
            bool canEditAssetType = !this.serializedObject.isEditingMultipleObjects && ((this.serializedObject.targetObject as QueryableAssetSet)?.CanEditAssetType ?? false);
            bool cache = GUI.enabled;
            if (!canEditAssetType) GUI.enabled = false;

            var prop_assettype = this.serializedObject.FindProperty(QueryableAssetSet.PROP_ASSETTYPE);
            var prop_assets = this.serializedObject.FindProperty(QueryableAssetSet.PROP_ASSETS);
            _restrictedType = TypeReferencePropertyDrawer.GetTypeFromTypeReference(prop_assettype) ?? typeof(UnityEngine.Object);

            EditorGUI.BeginChangeCheck();
            _typeRefDrawer.OnGUILayout(prop_assettype);
            if (EditorGUI.EndChangeCheck() && prop_assets.arraySize > 0)
            {
                _restrictedType = TypeReferencePropertyDrawer.GetTypeFromTypeReference(prop_assettype) ?? typeof(UnityEngine.Object);
                using (var lst = TempCollection.GetList<UnityEngine.Object>())
                {
                    for (int i = 0; i < prop_assets.arraySize; i++)
                    {
                        var obj = ObjUtil.GetAsFromSource(_restrictedType, prop_assets.GetArrayElementAtIndex(i).objectReferenceValue) as UnityEngine.Object;
                        if (obj) lst.Add(obj);
                    }

                    prop_assets.arraySize = lst.Count;
                    for (int i = 0; i < prop_assets.arraySize; i++)
                    {
                        prop_assets.GetArrayElementAtIndex(i).objectReferenceValue = lst[i];
                    }
                }
            }

            GUI.enabled = cache;
        }

        protected virtual void DrawAssetsProperty()
        {
            _supportNestedAssetSet = this.serializedObject.FindProperty(QueryableAssetSet.PROP_SUPPORTNESTEDGROUPS).boolValue;
            _reorderableArrayDrawer.DragDropElementType = _restrictedType;
            _reorderableArrayDrawer.OnGUILayout(this.serializedObject.FindProperty(QueryableAssetSet.PROP_ASSETS));
        }

        protected virtual void DrawQuickAddUtils()
        {

            if (GUILayout.Button(EditorHelper.TempContent("Scan Project", "Scans the entire project and adds the matching assets to this AssetSet.")))
            {
                UpdateContentsByScanProject(this.target as QueryableAssetSet, _restrictedType);
            }
            EditorGUILayout.Space(2f);
            if (GUILayout.Button(EditorHelper.TempContent("Scan Local Folder", "Scans the folder this asset is in, and sub folders, and adds the matching assets to this AssetSet.")))
            {
                UpdateContentsByScanLocalFolder(this.target as QueryableAssetSet, _restrictedType);
            }
        }

        static string GetBestSearchStringForType(System.Type tp)
        {
            if (tp == typeof(GameObject) || TypeUtil.IsType(tp, typeof(Component)))
                return $"t:GameObject";
            if (TypeUtil.IsType(tp, typeof(ScriptableObject)))
                return $"t:{tp.Name}";
            if (TypeUtil.IsType(tp, typeof(Texture)))
                return $"t:texture";
            if (TypeUtil.IsType(tp, typeof(Sprite)))
                return $"t:sprite";

            return "a:assets";
        }


        #region Special Types

        private class ReorderableArrayInternalDrawer : PropertyDrawer
        {

            private QueryableAssetSetInspector _owner;

            public ReorderableArrayInternalDrawer(QueryableAssetSetInspector owner)
            {
                _owner = owner;
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                if (_owner._supportNestedAssetSet || _owner._restrictedType.IsInterface)
                {
                    EditorGUI.BeginChangeCheck();
                    var obj = SPEditorGUI.AdvancedObjectField(position,
                        label,
                        property.objectReferenceValue,
                        typeof(UnityEngine.Object),
                        false,
                        true,
                        (ref UnityEngine.Object o) => _owner._reorderableArrayDrawer.DragDropElementFilter(o) != null);
                    if (EditorGUI.EndChangeCheck())
                    {
                        obj = _owner._reorderableArrayDrawer.DragDropElementFilter(obj);
                        //if (obj) property.objectReferenceValue = obj;
                        property.objectReferenceValue = obj;
                    }
                }
                else
                {
                    if (position.height > EditorGUIUtility.singleLineHeight) position = new Rect(position.xMin, position.yMin + (position.height - EditorGUIUtility.singleLineHeight) / 2, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.ObjectField(position, property, _owner._restrictedType, label);
                }
            }

        }

        #endregion

        #region Static Helpers

        public static void UpdateContentsByScanProject(QueryableAssetSet assetset, System.Type restrictedType)
        {
            if (!assetset) return;

            if (restrictedType == null) restrictedType = assetset.AssetType;

            var assets = AssetDatabase.FindAssets(GetBestSearchStringForType(restrictedType))
                                    .Select(s => ObjUtil.GetAsFromSource(restrictedType, AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), typeof(UnityEngine.Object))) as UnityEngine.Object)
                                    .Where(o => o != null && o != assetset);
            Undo.RecordObject(assetset, "QueryableAssetSet - Scan Project");
            assetset.ResetAssets(assets);
            EditorHelper.CommitDirectChanges(assetset, true);
        }

        public static void UpdateContentsByScanLocalFolder(QueryableAssetSet assetset, System.Type restrictedType)
        {
            if (!assetset) return;

            if (restrictedType == null) restrictedType = assetset.AssetType;

            var path = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(assetset));
            var assetguids = AssetDatabase.FindAssets(GetBestSearchStringForType(restrictedType), new string[] { path });
            IEnumerable<UnityEngine.Object> assets;
            if (TypeUtil.IsType(restrictedType, typeof(UnityEngine.Object)))
            {
                assets = assetguids.Select(s => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), restrictedType));
            }
            else
            {
                assets = assetguids.Select(s => ObjUtil.GetAsFromSource(restrictedType, AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), typeof(UnityEngine.Object))) as UnityEngine.Object);
            }
            Undo.RecordObject(assetset, "QueryableAssetSet - Scan Local Folder");
            assetset.ResetAssets(assets.Where(o => o != null && o != assetset));
            EditorHelper.CommitDirectChanges(assetset, true);
        }

        #endregion

    }

}
