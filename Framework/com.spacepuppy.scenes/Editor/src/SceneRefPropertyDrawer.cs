using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Scenes;
using com.spacepuppy.Utils;
using com.spacepuppyeditor.Core;

namespace com.spacepuppyeditor.Scenes
{

    [CustomPropertyDrawer(typeof(SceneRef))]
    public class SceneRefPropertyDrawer : PropertyDrawer
    {
        public const float OBJFIELD_DOT_WIDTH = 18f;

        public const string PROP_SCENENAME = "_sceneName";
        public const string PROP_GUID = "_guid";

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
            var totalPos = position;
            position = EditorGUI.PrefixLabel(position, label);
            EditorHelper.SuppressIndentLevel();

            try
            {
                var nameProp = property.FindPropertyRelative(PROP_SCENENAME);
                var guidProp = property.FindPropertyRelative(PROP_GUID);
                var guid = SerializableGuidPropertyDrawer.FromSerializedProperty(guidProp);

                //try to find SceneAsset
                SceneAsset scene = null;
                string apath = null;
                if (guid != System.Guid.Empty)
                {
                    apath = AssetDatabase.GUIDToAssetPath(guid.ToString("N"));
                }
                else if (!string.IsNullOrEmpty(nameProp.stringValue))
                {
                    var sguid = AssetDatabase.FindAssets($"{nameProp.stringValue} t:Scene").FirstOrDefault();
                    apath = !string.IsNullOrEmpty(sguid) ? AssetDatabase.GUIDToAssetPath(sguid) : null;
                }
                if (!string.IsNullOrEmpty(apath)) scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(apath);

                const float TOGGLE_WIDTH = 30f;
                Rect rObjField = new Rect(position.xMin, position.yMin, Mathf.Max(position.width - TOGGLE_WIDTH, 0f), EditorGUIUtility.singleLineHeight);
                if (scene != null)
                {
                    EditorGUI.BeginChangeCheck();
                    scene = EditorGUI.ObjectField(rObjField, GUIContent.none, scene, typeof(SceneAsset), false) as SceneAsset;
                    if (EditorGUI.EndChangeCheck() || nameProp.stringValue != scene?.name)
                    {
                        SetProps(nameProp, guidProp, scene);
                    }
                }
                else
                {
                    var rText = new Rect(rObjField.xMin, rObjField.yMin, Mathf.Max(rObjField.width - OBJFIELD_DOT_WIDTH, 0f), rObjField.height);
                    var rDot = new Rect(rText.xMax, rObjField.yMin, Mathf.Min(rObjField.width - rText.width, OBJFIELD_DOT_WIDTH), rObjField.height);
                    EditorGUI.BeginChangeCheck();
                    scene = EditorGUI.ObjectField(rDot, GUIContent.none, scene, typeof(SceneAsset), false) as SceneAsset;
                    if (scene != null)
                    {
                        SetProps(nameProp, guidProp, scene);
                        return;
                    }
                    nameProp.stringValue = EditorGUI.TextField(rText, nameProp.stringValue);


                    var ev = Event.current;
                    switch (ev.type)
                    {
                        case EventType.DragUpdated:
                        case EventType.DragPerform:
                            if (totalPos.Contains(ev.mousePosition))
                            {
                                scene = DragAndDrop.objectReferences.FirstOrDefault((o) => o is SceneAsset) as SceneAsset;
                                DragAndDrop.visualMode = scene != null ? DragAndDropVisualMode.Link : DragAndDropVisualMode.Rejected;

                                if (scene != null && ev.type == EventType.DragPerform)
                                {
                                    SetProps(nameProp, guidProp, scene);
                                }
                            }
                            break;
                    }
                }

                var rBtn = new Rect(rObjField.xMax, position.yMin, Mathf.Min(TOGGLE_WIDTH, position.width - rObjField.width), EditorGUIUtility.singleLineHeight);
                if (GUI.Button(rBtn, "X"))
                {
                    SetProps(nameProp, guidProp, null);
                }

                EditorGUI.EndProperty();
            }
            finally
            {
                EditorHelper.ResumeIndentLevel();
            }

        }

        private void SetProps(SerializedProperty nameProp, SerializedProperty guidProp, SceneAsset scene)
        {
            string sguid;
            nameProp.stringValue = (scene != null) ? scene.name : string.Empty;
            if (scene != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier<SceneAsset>(scene, out sguid, out _))
            {
                SerializableGuidPropertyDrawer.ToSerializedProperty(guidProp, System.Guid.Parse(sguid));
            }
            else
            {
                SerializableGuidPropertyDrawer.ToSerializedProperty(guidProp, System.Guid.Empty);
            }
        }

    }

}
