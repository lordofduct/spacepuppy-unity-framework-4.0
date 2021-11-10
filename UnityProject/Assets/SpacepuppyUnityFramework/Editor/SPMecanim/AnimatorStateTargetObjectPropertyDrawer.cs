using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Mecanim;
using com.spacepuppy.Utils;
using com.spacepuppyeditor;

using com.spacepuppyeditor.Internal;
using FindCommand = com.spacepuppy.Events.TriggerableTargetObject.FindCommand;

namespace com.spacepuppyeditor.Mecanim
{

    [CustomPropertyDrawer(typeof(AnimatorStateTargetObject), true)]
    public class AnimatorStateTargetObjectPropertyDrawer : com.spacepuppyeditor.Core.Events.TriggerableTargetObjectPropertyDrawer
    {

        public enum AnimatorTargetSource
        {
            Bridge = TargetSource.Arg,
            Config = TargetSource.Config
        }

        #region CONSTRUCTOR

        public AnimatorStateTargetObjectPropertyDrawer() : base()
        {
            this.AlwaysExpanded = true;
        }

        public AnimatorStateTargetObjectPropertyDrawer(System.Type targetType, bool alwaysExpanded = true) : base(targetType, false, false, alwaysExpanded)
        {

        }

        #endregion

        protected override void Init(SerializedProperty property)
        {
            if (this.ManuallyConfigured) return;
            if (this.fieldInfo == null) return;

            var attrib = this.fieldInfo.GetCustomAttributes(typeof(AnimatorStateTargetObject.ConfigAttribute), false).FirstOrDefault() as AnimatorStateTargetObject.ConfigAttribute;
            if (attrib != null)
            {
                this.TargetType = attrib.TargetType;
                this.SearchChildren = false;
                this.DefaultFromSelf = false;
                this.AlwaysExpanded = attrib.AlwaysExpanded;
            }
            else
            {
                this.TargetType = null;
                this.SearchChildren = false;
                this.DefaultFromSelf = false;
                this.AlwaysExpanded = true;
            }
        }

        protected override Rect DrawTargetSource(Rect position, SerializedProperty property, GUIContent label)
        {
            var configProp = property.FindPropertyRelative(PROP_CONFIGURED);
            var targetProp = property.FindPropertyRelative(PROP_TARGET);
            var findProp = property.FindPropertyRelative(PROP_FIND);

            var r0 = new Rect(position.xMin, position.yMin, LEN_TARGETSOURCE, position.height);
            var e = (configProp.boolValue) ? AnimatorTargetSource.Config : AnimatorTargetSource.Bridge;
            EditorGUI.BeginChangeCheck();
            e = (AnimatorTargetSource)EditorGUI.EnumPopup(r0, e);
            if (EditorGUI.EndChangeCheck())
            {
                if (e == AnimatorTargetSource.Bridge) targetProp.objectReferenceValue = null;
                configProp.boolValue = (e != AnimatorTargetSource.Bridge);
                if (e != AnimatorTargetSource.Bridge) findProp.SetEnumValue(FindCommand.Direct);
            }
            else if (e == AnimatorTargetSource.Bridge && targetProp.objectReferenceValue != null)
            {
                configProp.objectReferenceValue = null;
            }

            return new Rect(r0.xMax, position.yMin, position.width - r0.width, position.height);
        }

        protected override Rect DrawTarget(Rect position, SerializedProperty property, GUIContent label)
        {
            var configProp = property.FindPropertyRelative(PROP_CONFIGURED);

            if (!configProp.boolValue)
            {
                var r1 = new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight);
                var targetProp = property.FindPropertyRelative(PROP_TARGET);
                var findProp = property.FindPropertyRelative(PROP_FIND);

                var e0 = findProp.GetEnumValue<FindCommand>();
                switch (e0)
                {
                    case FindCommand.Direct:
                        EditorGUI.LabelField(r1, "Target is animator.");
                        break;
                    case FindCommand.FindParent:
                    case FindCommand.FindInChildren:
                    case FindCommand.FindInEntity:
                        EditorGUI.LabelField(r1, e0.ToString() + " of activating animator.");
                        break;
                    case FindCommand.FindInScene:
                    case FindCommand.FindEntityInScene:
                    default:
                        configProp.boolValue = false;
                        targetProp.objectReferenceValue = null;
                        EditorGUI.LabelField(r1, e0.ToString());
                        break;
                }

                targetProp.objectReferenceValue = null;
                return new Rect(r1.xMax, r1.yMin, 0f, r1.height);
            }
            else
            {
                return base.DrawTarget(position, property, label);
            }
        }

    }

}
