using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(ProxyOrDirectField<>))]
    public class ProxyOrDirectFieldPropertyDrawer : PropertyDrawer
    {

        private TypeRestrictionPropertyDrawer _typeRestrictionPropertyDrawer = new TypeRestrictionPropertyDrawer()
        {
            FieldType = typeof(UnityEngine.Object),
            AllowProxy = true,
        };
        private System.Type[] _typeArray = new System.Type[] { typeof(object) };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            _typeRestrictionPropertyDrawer.InheritsFromTypes = property.GetTargetType().GetGenericArguments();
            return _typeRestrictionPropertyDrawer.GetPropertyHeight(property.FindPropertyRelative("_target"), label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _typeRestrictionPropertyDrawer.InheritsFromTypes = property.GetTargetType().GetGenericArguments();
            _typeRestrictionPropertyDrawer.OnGUI(position, property.FindPropertyRelative("_target"), label);
        }

    }

}
