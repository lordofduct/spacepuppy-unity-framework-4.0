using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using com.spacepuppy.UI.Events;
using UnityEngine.UIElements;

namespace com.spacepuppyeditor.UI.Events
{

    [CustomEditor(typeof(i_SelectFirstAvailableUIElement)), CanEditMultipleObjects]
    public class i_SelectFirstAvailableUIElementInspector : SPEditor
    {

        #region Methods

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(EditorHelper.PROP_ORDER);
            this.DrawPropertyField(EditorHelper.PROP_ACTIVATEON);

            var propmode = this.serializedObject.FindProperty(i_SelectFirstAvailableUIElement.PROP_MODE);
            var propoptions = this.serializedObject.FindProperty(i_SelectFirstAvailableUIElement.PROP_OPTIONS);

            SPEditorGUILayout.PropertyField(propmode);

            if (!propmode.hasMultipleDifferentValues)
            {
                switch (propmode.GetEnumValue<i_SelectFirstAvailableUIElement.Modes>())
                {
                    case i_SelectFirstAvailableUIElement.Modes.FirstChild:
                        if (propoptions.hasMultipleDifferentValues || (propoptions.arraySize == 1 && propoptions.GetArrayElementAtIndex(0).hasMultipleDifferentValues))
                        {
                            EditorGUILayout.LabelField("Container", "-");
                        }
                        else
                        {
                            propoptions.arraySize = 1;
                            EditorGUILayout.ObjectField(propoptions.GetArrayElementAtIndex(0), EditorHelper.TempContent("Container", "Iterate the children of this container to find the first UIElement."));
                        }
                        break;
                    case i_SelectFirstAvailableUIElement.Modes.FirstInList:
                    default:
                        SPEditorGUILayout.PropertyField(propoptions);
                        break;
                }
            }
            else
            {
                EditorGUILayout.LabelField("Options", "Multi-Object editing is not supported.");
            }

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, EditorHelper.PROP_ORDER, EditorHelper.PROP_ACTIVATEON, i_SelectFirstAvailableUIElement.PROP_MODE, i_SelectFirstAvailableUIElement.PROP_OPTIONS);

            this.serializedObject.ApplyModifiedProperties();
        }

        #endregion

    }

}
