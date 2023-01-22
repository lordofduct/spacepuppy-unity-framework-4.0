using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy.Geom;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core.Geom
{

    [CustomPropertyDrawer(typeof(ExpectsCompoundTriggerAttribute))]
    public class ExpectsCompoundTriggerHeaderDrawer : ComponentHeaderDrawer
    {

        private const string MSG = "This component requires being in the message path of an ICompoundTrigger.";

        private const float MARGIN = 8f;
        private const float MARGIN_DBL = MARGIN * 2f;

        private bool RequiresDrawing(SerializedObject serializedObject)
        {
            if (serializedObject.isEditingMultipleObjects) return false;

            var go = GameObjectUtil.GetGameObjectFromSource(serializedObject.targetObject);
            return !(go && CompoundTrigger.FindCompoundTriggerWithTarget(go) != null);
        }

        public override float GetHeight(SerializedObject serializedObject)
        {
            if (!this.RequiresDrawing(serializedObject)) return 0f;

            GUIStyle style = GUI.skin.GetStyle("HelpBox");
            return Mathf.Max(20f, style.CalcHeight(new GUIContent(MSG), EditorGUIUtility.currentViewWidth - MARGIN_DBL)) + EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedObject serializedObject)
        {
            if (!this.RequiresDrawing(serializedObject)) return;

            EditorGUI.HelpBox(new Rect(position.xMin + MARGIN, position.yMin, position.width - MARGIN_DBL, position.height), MSG, MessageType.Warning);
        }

    }
}
