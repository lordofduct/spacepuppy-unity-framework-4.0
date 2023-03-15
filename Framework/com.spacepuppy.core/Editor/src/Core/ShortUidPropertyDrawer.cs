using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.spacepuppyeditor.Internal;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(ShortUid))]
    public class ShortUidPropertyDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);
            EditorHelper.SuppressIndentLevel();

            try
            {
                var lowProp = property.FindPropertyRelative("_low");
                var highProp = property.FindPropertyRelative("_high");
                ulong value = ((ulong)lowProp.longValue & uint.MaxValue) | ((ulong)highProp.longValue << 32);

                var attrib = this.fieldInfo.GetCustomAttributes(typeof(ShortUid.ConfigAttribute), false).FirstOrDefault() as ShortUid.ConfigAttribute;
                bool resetOnZero = attrib == null || !attrib.AllowZero;
                bool readWrite = attrib == null || !attrib.ReadOnly;

                if (attrib?.LinkToGlobalId ?? false)
                {
                    var gid = GetGlobalIdHash(property.serializedObject.targetObject);
                    if (value != gid)
                    {
                        value = gid;
                        lowProp.longValue = (long)(value & uint.MaxValue);
                        highProp.longValue = (long)(value >> 32);
                    }
                    EditorGUI.SelectableLabel(position, string.Format("0x{0:X16}", value), EditorStyles.textField);
                }
                else
                {
                    float w = Mathf.Min(position.width, 60f);
                    var r2 = new Rect(position.xMax - w, position.yMin, w, position.height);
                    var r1 = new Rect(position.xMin, position.yMin, Mathf.Max(position.width - w, 0f), position.height);

                    if (readWrite)
                    {
                        //read-write
                        EditorGUI.BeginChangeCheck();
                        var sval = EditorGUI.DelayedTextField(r1, string.Format("0x{0:X16}", value));
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (sval != null && sval.StartsWith("0x"))
                            {
                                ulong.TryParse(sval.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out value);
                                lowProp.longValue = (long)(value & uint.MaxValue);
                                highProp.longValue = (long)(value >> 32);
                            }
                        }
                    }
                    else
                    {
                        //read-only
                        EditorGUI.SelectableLabel(r1, string.Format("0x{0:X16}", value), EditorStyles.textField);
                    }

                    if (GUI.Button(r2, "New Id") || (resetOnZero && value == 0))
                    {
                        value = (ulong)ShortUid.NewId().Value;
                        lowProp.longValue = (long)(value & uint.MaxValue);
                        highProp.longValue = (long)(value >> 32);
                    }
                }

                EditorGUI.EndProperty();
            }
            finally
            {
                EditorHelper.ResumeIndentLevel();
            }
        }

        public static ulong GetGlobalIdHash(UnityEngine.Object obj)
        {
            var hash = XXHash.Hash64(GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString());
            if ((hash >> 32) == 0) hash |= (1UL << 63); //ensure the high bits always have at least 1 bit flagged. This way shortuid's formed from the instanceId's could be used to distinguish globalhash's from instanceid's
            return hash;
        }

    }

    [CustomPropertyDrawer(typeof(TokenId))]
    public class TokenIdPropertyDrawer : PropertyDrawer
    {


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);
            EditorHelper.SuppressIndentLevel();

            try
            {
                var lowProp = property.FindPropertyRelative("_low");
                var highProp = property.FindPropertyRelative("_high");
                var idProp = property.FindPropertyRelative("_id");

                ulong lval = ((ulong)lowProp.longValue & uint.MaxValue) | ((ulong)highProp.longValue << 32);
                string sval = idProp.stringValue;

                var attrib = this.fieldInfo.GetCustomAttributes(typeof(TokenId.ConfigAttribute), false).FirstOrDefault() as TokenId.ConfigAttribute;
                bool resetOnZero = attrib == null || !attrib.AllowZero;
                bool readWrite = attrib == null || !attrib.ReadOnly;

                if (attrib?.LinkToGlobalId ?? false)
                {
                    var gid = ShortUidPropertyDrawer.GetGlobalIdHash(property.serializedObject.targetObject);
                    if (lval != gid)
                    {
                        lval = gid;
                        lowProp.longValue = (long)(lval & uint.MaxValue);
                        highProp.longValue = (long)(lval >> 32);
                        idProp.stringValue = string.Empty;
                    }
                    EditorGUI.SelectableLabel(position, string.Format("0x{0:X16}", lval), EditorStyles.textField);
                }
                else
                {
                    float w = Mathf.Min(position.width, 60f);
                    var r2 = new Rect(position.xMax - w, position.yMin, w, position.height);
                    var r1 = new Rect(position.xMin, position.yMin, Mathf.Max(position.width - w, 0f), position.height);

                    if (readWrite)
                    {
                        //read-write
                        EditorGUI.BeginChangeCheck();
                        if (lval == 0)
                            sval = EditorGUI.DelayedTextField(r1, sval);
                        else
                            sval = EditorGUI.DelayedTextField(r1, string.Format("0x{0:X16}", lval));

                        if (EditorGUI.EndChangeCheck())
                        {
                            if (sval != null && sval.StartsWith("0x"))
                            {
                                ulong.TryParse(sval.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out lval);
                                lowProp.longValue = (long)(lval & uint.MaxValue);
                                highProp.longValue = (long)(lval >> 32);
                                idProp.stringValue = string.Empty;
                            }
                            else
                            {
                                idProp.stringValue = sval;
                                lowProp.longValue = 0;
                                highProp.longValue = 0;
                            }
                        }
                    }
                    else
                    {
                        //read-only
                        if (lval == 0)
                            EditorGUI.SelectableLabel(r1, string.Format("0x{0:X16}", lval), EditorStyles.textField);
                        else
                            EditorGUI.SelectableLabel(r1, sval, EditorStyles.textField);
                    }

                    if (GUI.Button(r2, "New Id") || (resetOnZero && lval == 0 && string.IsNullOrEmpty(sval)))
                    {
                        ulong value = TokenId.NewId().LongValue;
                        lowProp.longValue = (long)(value & uint.MaxValue);
                        highProp.longValue = (long)(value >> 32);
                        idProp.stringValue = string.Empty;
                    }
                }

                EditorGUI.EndProperty();
            }
            finally
            {
                EditorHelper.ResumeIndentLevel();
            }
        }


    }



    internal class ShortUidPostProcessor : AssetPostprocessor
    {

        private static readonly HashSet<System.Reflection.FieldInfo> _knownSOFields = new HashSet<System.Reflection.FieldInfo>();
        private static readonly HashSet<System.Reflection.FieldInfo> _knownGOFields = new HashSet<System.Reflection.FieldInfo>();

        static ShortUidPostProcessor()
        {
            var sidtp = typeof(ShortUid);
            var tidtp = typeof(TokenId);
            var sotp = typeof(ScriptableObject);
            var ctp = typeof(MonoBehaviour);
            foreach (var tp in TypeUtil.GetTypes(t => TypeUtil.IsType(t, sotp) || TypeUtil.IsType(t, ctp)))
            {
                foreach (var field in tp.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                {
                    if (field.DeclaringType != tp) continue;
                    if (field.FieldType != sidtp && field.FieldType != tidtp) continue;

                    var attrib = field.GetCustomAttribute<ShortUid.ConfigAttribute>();
                    if (attrib != null && attrib.LinkToGlobalId)
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
                var arr = go.GetComponentsInChildren(field.DeclaringType);
                if (arr?.Length > 0)
                {
                    foreach (var c in arr)
                    {
                        try
                        {
                            if (field.FieldType == typeof(ShortUid))
                            {
                                var sid = (ShortUid)field.GetValue(c);
                                var gid = ShortUidPropertyDrawer.GetGlobalIdHash(c);
                                if (gid != sid.Value)
                                {
                                    field.SetValue(c, new ShortUid(gid));
                                    edited = true;
                                }
                            }
                            else
                            {
                                var sid = (TokenId)field.GetValue(c);
                                var gid = ShortUidPropertyDrawer.GetGlobalIdHash(c);
                                if (!sid.IsLong || gid != sid.LongValue)
                                {
                                    field.SetValue(c, new TokenId(gid));
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
                    if (object.ReferenceEquals(so, null)) so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                    try
                    {
                        if (field.FieldType == typeof(ShortUid))
                        {
                            var sid = (ShortUid)field.GetValue(so);
                            var gid = GlobalObjectId.GetGlobalObjectIdSlow(so);
                            if (gid.targetObjectId != sid.Value)
                            {
                                field.SetValue(so, new ShortUid(gid.targetObjectId));
                                EditorHelper.CommitDirectChanges(so, false);
                                AssetDatabase.SaveAssetIfDirty(so);
                            }
                        }
                        else
                        {
                            var sid = (TokenId)field.GetValue(so);
                            var gid = GlobalObjectId.GetGlobalObjectIdSlow(so);
                            if (!sid.IsLong || gid.targetObjectId != sid.LongValue)
                            {
                                field.SetValue(so, new TokenId(gid.targetObjectId));
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
