using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Collections;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(TagSelectorAttribute))]
    public class TagSelectorPropertyDrawer : PropertyDrawer
    {

        private bool _allowUntagged;
        public bool AllowUntagged
        {
            get => (this.attribute as TagSelectorAttribute)?.AllowUntagged ?? _allowUntagged;
            set => _allowUntagged = value;
        }
        private bool _allowBlank;
        public bool AllowBlank
        {
            get => (this.attribute as TagSelectorAttribute)?.AllowBlank ?? _allowBlank;
            set => _allowBlank = value;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

            if (property.propertyType == SerializedPropertyType.String)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.BeginProperty(position, label, property);

                if (this.AllowUntagged && !this.AllowBlank)
                {
                    property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
                }
                else
                {
                    var tag_enumerator = this.AllowUntagged ? UnityEditorInternal.InternalEditorUtility.tags.Select(s => new GUIContent(s)) : (from s in UnityEditorInternal.InternalEditorUtility.tags where s != SPConstants.TAG_UNTAGGED select new GUIContent(s));
                    if (this.AllowBlank) tag_enumerator = tag_enumerator.Prepend(new GUIContent("- No Selection - "));

                    var tags = tag_enumerator.ToArray();
                    var stag = property.stringValue;
                    int index = -1;
                    for (int i = 0; i < tags.Length; i++)
                    {
                        if (tags[i].text == stag)
                        {
                            index = i;
                            break;
                        }
                    }

                    EditorGUI.BeginChangeCheck();
                    index = EditorGUI.Popup(position, label, index, tags);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (this.AllowBlank && index == 0)
                        {
                            property.stringValue = string.Empty;
                        }
                        else if (index >= 0)
                        {
                            property.stringValue = tags[index].text;
                        }
                        else
                        {
                            property.stringValue = string.Empty;
                        }
                    }
                }

                EditorGUI.EndProperty();
                if (EditorGUI.EndChangeCheck()) property.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }

        }

    }

}
