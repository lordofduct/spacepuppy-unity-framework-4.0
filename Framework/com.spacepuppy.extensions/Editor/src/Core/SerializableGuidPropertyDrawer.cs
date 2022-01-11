using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using System.Diagnostics.Eventing.Reader;
using com.spacepuppy.Collections;
using System.Reflection;
using System.Linq.Expressions;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(SerializableGuid))]
    public class SerializableGuidPropertyDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(property.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.LabelField(position, label, EditorHelper.TempContent("Not Supported in Multi-Edit Mode"));
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);
            EditorHelper.SuppressIndentLevel();

            try
            {
                var attrib = this.fieldInfo.GetCustomAttributes(typeof(SerializableGuid.ConfigAttribute), false).FirstOrDefault() as SerializableGuid.ConfigAttribute;
                bool resetOnZero = attrib == null || !attrib.AllowZero;
                bool linkToAsset = attrib != null && attrib.LinkToAsset;

                System.Guid guid = FromSerializedProperty(property);

                if (linkToAsset)
                {
                    string gid;
                    long lid;
                    System.Guid assetguid;
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(property.serializedObject.targetObject, out gid, out lid) && System.Guid.TryParse(gid, out assetguid))
                    {
                        if(assetguid != guid)
                        {
                            guid = assetguid;
                            ToSerializedProperty(property, guid);
                        }

                        EditorGUI.SelectableLabel(position, guid.ToString("D"), EditorStyles.textField);
                        EditorGUI.EndProperty();
                        return;
                    }
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

        private static System.Guid FromSerializedProperty(SerializedProperty prop)
        {
            return new System.Guid(prop.FindPropertyRelative("a").intValue, (short)prop.FindPropertyRelative("b").intValue, (short)prop.FindPropertyRelative("c").intValue,
                                   (byte)prop.FindPropertyRelative("d").intValue, (byte)prop.FindPropertyRelative("e").intValue, (byte)prop.FindPropertyRelative("f").intValue,
                                   (byte)prop.FindPropertyRelative("g").intValue, (byte)prop.FindPropertyRelative("h").intValue, (byte)prop.FindPropertyRelative("i").intValue,
                                   (byte)prop.FindPropertyRelative("j").intValue, (byte)prop.FindPropertyRelative("k").intValue);
        }

        private static void ToSerializedProperty(SerializedProperty prop, System.Guid guid)
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
                        if(attrib != null && attrib.LinkToAsset)
                        {
                            if(TypeUtil.IsType(field.DeclaringType, sotp))
                            {
                                _knownSOFields.Add(field);
                            }
                            else if(TypeUtil.IsType(field.DeclaringType, ctp))
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
                foreach(var field in _knownGOFields)
                {
                    var c = go.GetComponent(field.DeclaringType);
                    if(c != null)
                    {
                        try
                        {
                            var guid = (SerializableGuid)field.GetValue(c);

                            string gid;
                            long lid;
                            System.Guid assetguid;
                            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(c, out gid, out lid) && System.Guid.TryParse(gid, out assetguid))
                            {
                                if (assetguid != guid)
                                {
                                    field.SetValue(c, new SerializableGuid(assetguid));
                                    edited = true;
                                }
                            }
                        }
                        catch(System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }

                if(edited)
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
                        if (object.ReferenceEquals(so, null)) so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                        try
                        { 
                            var guid = (SerializableGuid)field.GetValue(so);

                            string gid;
                            long lid;
                            System.Guid assetguid;
                            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(so, out gid, out lid) && System.Guid.TryParse(gid, out assetguid))
                            {
                                if (assetguid != guid)
                                {
                                    field.SetValue(so, new SerializableGuid(assetguid));
                                    EditorUtility.SetDirty(so);
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
