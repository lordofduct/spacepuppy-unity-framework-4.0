using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppyeditor.Internal;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(InfoboxAttribute))]
    public class InfoboxDecorator : DecoratorDrawer
    {

        private const float MARGIN = 8f;
        private const float MARGIN_DBL = MARGIN * 2f;

        private static bool _showInfo;
        private static bool ShowInfo
        {
            get => _showInfo;
            set
            {
                if (_showInfo == value) return;
                _showInfo = value;
                EditorPrefs.SetBool("SPEditor.ShowInfoboxDecorator", value);
            }
        }
        static InfoboxDecorator()
        {
            _showInfo = EditorPrefs.GetBool("SPEditor.ShowInfoboxDecorator", false);
        }

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
            if (ShowInfo)
            {
                GUIStyle style = GUI.skin.GetStyle("HelpBox");
                return Mathf.Max(20f, style.CalcHeight(new GUIContent(attrib.Message), EditorGUIUtility.currentViewWidth - MARGIN_DBL)) + EditorGUIUtility.singleLineHeight;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        public static void OnGUI(Rect position, InfoboxAttribute attrib)
        {
            if (ShowInfo)
            {
                EditorGUI.HelpBox(new Rect(position.xMin + MARGIN, position.yMin, position.width - MARGIN_DBL, position.height), attrib.Message, (MessageType)attrib.MessageType);
            }
            else
            {
                EditorGUI.HelpBox(new Rect(position.xMin + MARGIN, position.yMin, position.width - MARGIN_DBL, position.height), "click to expand...", (MessageType)attrib.MessageType);
            }

            if (ReorderableListHelper.IsClickingArea(position))
            {
                ShowInfo = !ShowInfo;
            }
        }

    }
}
