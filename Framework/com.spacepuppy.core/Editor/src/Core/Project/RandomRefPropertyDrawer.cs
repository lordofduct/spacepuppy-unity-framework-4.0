using UnityEngine;
using UnityEditor;

namespace com.spacepuppyeditor.Core.Project
{

    [CustomPropertyDrawer(typeof(com.spacepuppy.RandomRef), true)]
    public class RandomRefPropertyDrawer : SerializableInterfaceRefPropertyDrawer
    {

        private const string STR_TOOLTIP = "Optional - leaving this blank will result in the Unity Random class to be used.";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (string.IsNullOrEmpty(label.tooltip))
            {
                label.tooltip = STR_TOOLTIP;
            }
            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (string.IsNullOrEmpty(label.tooltip))
            {
                label.tooltip = STR_TOOLTIP;
            }
            base.OnGUI(position, property, label);
        }

    }

}
