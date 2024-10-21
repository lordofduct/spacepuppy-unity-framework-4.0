using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

using com.spacepuppy;
using com.spacepuppy.Utils;
using System.Linq;

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
                System.Guid guid = FromSerializedProperty(property);
                if (Application.isPlaying)
                {
                    DrawGuidField(position, guid, attrib);
                    EditorGUI.EndProperty();
                    return;
                }

                System.Guid newguid;
                if (EditorHelper.TryGetLinkedGuid(property.serializedObject.targetObject, out newguid, attrib?.mode ?? LinkedGuidMode.None))
                {
                    if (newguid != guid)
                    {
                        guid = newguid;
                        ToSerializedProperty(property, guid);
                    }

                    DrawGuidField(position, guid, attrib);
                    EditorGUI.EndProperty();
                    return;
                }
                else if (attrib?.ObjectRefField ?? false)
                {
                    newguid = DrawGuidField(position, guid, attrib);
                    if (newguid != guid)
                    {
                        ToSerializedProperty(property, newguid);
                    }
                    EditorGUI.EndProperty();
                    return;
                }

                //if we made it here we want to draw default
                float w = Mathf.Min(position.width, 60f);
                var r2 = new Rect(position.xMax - w, position.yMin, w, position.height);
                var r1 = new Rect(position.xMin, position.yMin, Mathf.Max(position.width - w, 0f), position.height);

                DrawGuidField(r1, guid, null);

                bool resetOnZero = attrib == null || !attrib.AllowZero;
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

        private System.Guid DrawGuidField(Rect position, System.Guid guid, SerializableGuid.ConfigAttribute attrib)
        {
            if (attrib?.ObjectRefField ?? false)
            {
                var r0 = new Rect(position.xMin, position.yMin, 30f, position.height);
                var r1 = new Rect(position.xMin + r0.width + 1, position.yMin, position.width - r0.width - 1, position.height);
                EditorGUI.LabelField(r0, "Ref:");

                var path = AssetDatabase.GUIDToAssetPath(guid.ToString("N"));
                var obj = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (obj || guid == System.Guid.Empty)
                {
                    var newobj = EditorGUI.ObjectField(r1, obj, typeof(UnityEngine.Object), false);
                    if (!newobj)
                    {
                        return System.Guid.Empty;
                    }
                    else if (newobj != obj && EditorHelper.TryGetLinkedGuid(newobj, out System.Guid newguid, LinkedGuidMode.Asset))
                    {
                        return newguid;
                    }
                    else
                    {
                        return guid;
                    }
                }
                else
                {
                    //couldn't find asset
                    if (SPEditorGUI.XButton(ref r1))
                    {
                        EditorGUI.SelectableLabel(r1, "Missing: " + guid.ToString("D"), EditorStyles.textField);
                        return System.Guid.Empty;
                    }

                    EditorGUI.SelectableLabel(r1, "Missing: " + guid.ToString("D"), EditorStyles.textField);
                    var ev = Event.current;
                    switch (ev.type)
                    {
                        case EventType.DragUpdated:
                        case EventType.DragPerform:
                            if (position.Contains(ev.mousePosition))
                            {
                                DragAndDrop.visualMode = DragAndDrop.objectReferences.Count() > 0 ? DragAndDropVisualMode.Link : DragAndDropVisualMode.Rejected;
                                if (DragAndDrop.objectReferences.Count() > 0 && ev.type == EventType.DragPerform)
                                {
                                    var newobj = DragAndDrop.objectReferences.FirstOrDefault();
                                    if (newobj && EditorHelper.TryGetLinkedGuid(newobj, out System.Guid newguid, LinkedGuidMode.Asset))
                                    {
                                        return newguid;
                                    }
                                }
                            }
                            break;
                    }

                    return guid;
                }
            }
            else
            {
                EditorGUI.SelectableLabel(position, guid.ToString("D"), EditorStyles.textField);
                return guid;
            }
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
                        if (attrib != null && attrib.mode != LinkedGuidMode.None)
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
                            if (EditorHelper.TryGetLinkedGuid(c, out newguid, attrib.mode))
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
                            if (EditorHelper.TryGetLinkedGuid(so, out newguid, attrib.mode))
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
