using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Waypoints;
using com.spacepuppy.Waypoints.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Waypoints.Events
{

    [CustomEditor(typeof(i_MoveOnPath)), CanEditMultipleObjects]
    public class i_MoveOnPathInspector : SPEditor
    {

        private const string PROP_ADDALLMODIFIERS = "_addAllPossibleModifierTypes";
        private const string PROP_MODIFIERS = "_updateModifierTypes";

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            var iterator = serializedObject.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                switch(iterator.name)
                {
                    case PROP_MODIFIERS:
                        if(!this.serializedObject.FindProperty(PROP_ADDALLMODIFIERS).boolValue)
                        {
                            this.DrawModifierTypes(iterator);
                        }
                        break;
                    default:
                        SPEditorGUILayout.PropertyField(iterator, true);
                        break;
                }
            }

            this.serializedObject.ApplyModifiedProperties();
        }



        private void DrawModifierTypes(SerializedProperty prop)
        {
            //TODO - we want to include a drop down that only lists the IStateModifiers that actually exist on the nodes...

            SPEditorGUILayout.PropertyField(prop, true);
        }

    }
}
