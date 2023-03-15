using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(SerializableGuid))]
    public class SerializableGuidPropertyDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h;
            if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;

            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);
            EditorHelper.SuppressIndentLevel();

            try
            {
                var attrib = this.fieldInfo.GetCustomAttribute<SerializableGuid.ConfigAttribute>();
                bool resetOnZero = attrib == null || !attrib.AllowZero;
                bool linkToAsset = attrib != null && attrib.LinkToAsset;
                bool linkToGlobalObjectId = attrib != null && attrib.LinkToGlobalObjectId;

                System.Guid guid = FromSerializedProperty(property);
                System.Guid newguid;
                if (TryGetLinkedGuid(property.serializedObject.targetObject, out newguid, linkToAsset, linkToGlobalObjectId))
                {
                    if (newguid != guid)
                    {
                        guid = newguid;
                        ToSerializedProperty(property, guid);
                    }

                    EditorGUI.SelectableLabel(position, guid.ToString("D"), EditorStyles.textField);
                    EditorGUI.EndProperty();
                    return;
                }

                //if we made it here we want to draw default
                float w = Mathf.Min(position.width, 60f);
                var r2 = new Rect(position.xMax - w, position.yMin, w, position.height);
                var r1 = new Rect(position.xMin, position.yMin, Mathf.Max(position.width - w, 0f), position.height);

                EditorGUI.SelectableLabel(r1, guid.ToString("D"), EditorStyles.textField);

                if (GUI.Button(r2, "New Id") || (resetOnZero && guid == System.Guid.Empty))
                {
                    guid = System.Guid.NewGuid();
                    ToSerializedProperty(property, guid);
                }
                EditorGUI.EndProperty();
            }
            finally
            {
                EditorHelper.ResumeIndentLevel();
            }
        }

        public static bool TryGetLinkedGuid(UnityEngine.Object obj, out System.Guid guid, bool linkToAsset, bool linkToGlobalObjectId)
        {
            GlobalObjectId gid;
            if (linkToAsset && EditorHelper.TryGetNearestAssetGlobalObjectId(obj, out gid) && gid.assetGUID != default)
            {
                guid = gid.assetGUID.ToGuid();
                return true;
            }

            if (linkToGlobalObjectId)
            {
                gid = GlobalObjectId.GetGlobalObjectIdSlow(obj);
                if (gid.targetObjectId != 0UL)
                {
                    guid = (new SerializableGuid(gid.targetObjectId, gid.targetPrefabId)).ToGuid();
                    return true;
                }
            }

            guid = default;
            return false;
        }

        public static System.Guid FromSerializedProperty(SerializedProperty prop)
        {
            return new System.Guid(prop.FindPropertyRelative("a").intValue, (short)prop.FindPropertyRelative("b").intValue, (short)prop.FindPropertyRelative("c").intValue,
                                   (byte)prop.FindPropertyRelative("d").intValue, (byte)prop.FindPropertyRelative("e").intValue, (byte)prop.FindPropertyRelative("f").intValue,
                                   (byte)prop.FindPropertyRelative("g").intValue, (byte)prop.FindPropertyRelative("h").intValue, (byte)prop.FindPropertyRelative("i").intValue,
                                   (byte)prop.FindPropertyRelative("j").intValue, (byte)prop.FindPropertyRelative("k").intValue);
        }

        public static void ToSerializedProperty(SerializedProperty prop, System.Guid guid)
        {
            var arr = guid.ToByteArray();
            prop.FindPropertyRelative("a").intValue = System.BitConverter.ToInt32(arr, 0);
            prop.FindPropertyRelative("b").intValue = System.BitConverter.ToInt16(arr, 4);
            prop.FindPropertyRelative("c").intValue = System.BitConverter.ToInt16(arr, 6);
            prop.FindPropertyRelative("d").intValue = arr[8];
            prop.FindPropertyRelative("e").intValue = arr[9];
            prop.FindPropertyRelative("f").intValue = arr[10];
            prop.FindPropertyRelative("g").intValue = arr[11];
            prop.FindPropertyRelative("h").intValue = arr[12];
            prop.FindPropertyRelative("i").intValue = arr[13];
            prop.FindPropertyRelative("j").intValue = arr[14];
            prop.FindPropertyRelative("k").intValue = arr[15];
        }

        private class SerializableGuidPostProcessor : AssetPostprocessor
        {

            private static readonly HashSet<System.Reflection.FieldInfo> _knownSOFields = new HashSet<System.Reflection.FieldInfo>();
            private static readonly HashSet<System.Reflection.FieldInfo> _knownGOFields = new HashSet<System.Reflection.FieldInfo>();

            static SerializableGuidPostProcessor()
            {
                var guidtp = typeof(SerializableGuid);
                var sotp = typeof(ScriptableObject);
                var ctp = typeof(MonoBehaviour);
                foreach (var tp in TypeUtil.GetTypes(t => TypeUtil.IsType(t, sotp) || TypeUtil.IsType(t, ctp)))
                {
                    foreach (var field in tp.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                    {
                        if (field.FieldType != guidtp) continue;

                        var attrib = field.GetCustomAttribute<SerializableGuid.ConfigAttribute>();
                        if (attrib != null && (attrib.LinkToAsset || attrib.LinkToGlobalObjectId))
                        {
                            if (TypeUtil.IsType(field.DeclaringType, sotp))
                            {
                                _knownSOFields.Add(field);
                            }
                            else if (TypeUtil.IsType(field.DeclaringType, ctp))
                            {
                                _knownGOFields.Add(field);
                            }
                        }
                    }
                }
            }

            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                if (_knownGOFields.Count == 0 && _knownSOFields.Count == 0) return;

                var sotp = typeof(ScriptableObject);
                var gotp = typeof(GameObject);
                if (importedAssets != null && importedAssets.Length > 0)
                {
                    foreach (var path in importedAssets)
                    {
                        var atp = AssetDatabase.GetMainAssetTypeAtPath(path);
                        if (_knownSOFields.Count > 0 && TypeUtil.IsType(atp, sotp))
                        {
                            HandleScriptableObject(path, atp);
                        }
                        else if (_knownGOFields.Count > 0 && TypeUtil.IsType(atp, gotp))
                        {
                            HandleGameObject(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                        }
                    }
                }
            }

            static void HandleGameObject(GameObject go)
            {
                if (go == null) return;

                bool edited = false;
                foreach (var field in _knownGOFields)
                {
                    var c = go.GetComponent(field.DeclaringType);
                    if (c != null)
                    {
                        var attrib = field.GetCustomAttribute<SerializableGuid.ConfigAttribute>();
                        if (attrib == null) continue;

                        try
                        {
                            var guid = (SerializableGuid)field.GetValue(c);
                            System.Guid newguid;
                            if (SerializableGuidPropertyDrawer.TryGetLinkedGuid(c, out newguid, attrib.LinkToAsset, attrib.LinkToGlobalObjectId))
                            {
                                if (newguid != guid)
                                {
                                    field.SetValue(c, (SerializableGuid)newguid);
                                    edited = true;
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }

                if (edited)
                {
                    PrefabUtility.SavePrefabAsset(go);
                }
            }

            static void HandleScriptableObject(string path, System.Type atp)
            {
                ScriptableObject so = null;

                foreach (var field in _knownSOFields)
                {
                    if (TypeUtil.IsType(atp, field.DeclaringType))
                    {
                        var attrib = field.GetCustomAttribute<SerializableGuid.ConfigAttribute>();
                        if (attrib == null) continue;

                        if (object.ReferenceEquals(so, null)) so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                        try
                        {
                            var guid = (SerializableGuid)field.GetValue(so);
                            System.Guid newguid;
                            if (SerializableGuidPropertyDrawer.TryGetLinkedGuid(so, out newguid, attrib.LinkToAsset, attrib.LinkToGlobalObjectId))
                            {
                                if (newguid != guid)
                                {
                                    field.SetValue(so, (SerializableGuid)newguid);
                                    EditorHelper.CommitDirectChanges(so, false);
                                    AssetDatabase.SaveAssetIfDirty(so);
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            }



        }

    }
}
