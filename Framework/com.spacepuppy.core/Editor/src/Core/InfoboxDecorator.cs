using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(InfoboxAttribute))]
    public class InfoboxDecorator : DecoratorDrawer
    {

        private const float MARGIN = 8f;
        private const float MARGIN_DBL = MARGIN * 2f;

        public override float GetHeight()
        {
            return InfoboxDecorator.GetHeight(this.attribute as InfoboxAttribute);
        }

        public override void OnGUI(Rect position)
        {
            InfoboxDecorator.OnGUI(position, this.attribute as InfoboxAttribute);
        }


        public static float GetHeight(InfoboxAttribute attrib)
        {
            GUIStyle style = GUI.skin.GetStyle("HelpBox");
            return Mathf.Max(40f, style.CalcHeight(new GUIContent(attrib.Message), EditorGUIUtility.currentViewWidth - MARGIN_DBL));
        }

        public static void OnGUI(Rect position, InfoboxAttribute attrib)
        {
            EditorGUI.HelpBox(new Rect(position.xMin + MARGIN, position.yMin, position.width - MARGIN_DBL, position.height), attrib.Message, (MessageType)attrib.MessageType);
        }

    }
}
