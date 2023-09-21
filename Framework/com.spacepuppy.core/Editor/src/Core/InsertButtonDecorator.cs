using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    /// <summary>
    /// Inserts a button before or after a field.
    /// 
    /// Though this is technically a decorator, it uses a PropertyModifier to accomplish its task due to its special needs.
    /// </summary>
    [CustomPropertyDrawer(typeof(InsertButtonAttribute))]
    public class InsertButtonDecorator : PropertyModifier
    {

        protected internal override void OnBeforeGUI(SerializedProperty property, ref bool cancelDraw)
        {
            var attrib = this.attribute as InsertButtonAttribute;
            if (attrib.PrecedeProperty && (!attrib.RuntimeOnly || Application.isPlaying))
            {
                this.DrawButton(property, attrib);
            }
        }

        protected internal override void OnPostGUI(SerializedProperty property)
        {
            var attrib = this.attribute as InsertButtonAttribute;
            if (!attrib.PrecedeProperty && (!attrib.RuntimeOnly || Application.isPlaying))
            {
                this.DrawButton(property, attrib);
            }
        }


        private void DrawButton(SerializedProperty property, InsertButtonAttribute attrib)
        {
            if (!attrib.SupportsMultiObjectEditing && property.serializedObject.isEditingMultipleObjects) return;

            if(GUILayout.Button(attrib.Label))
            {
                foreach (var obj in EditorHelper.GetTargetObjectsWithProperty(property))
                {
                    DynamicUtil.InvokeMethod(obj, attrib.OnClick);
                }
            }
        }

    }

    [CustomAddonDrawer(typeof(InsertButtonAttribute), displayAsFooter = true, supportMultiObject = true)]
    public class InsertButtonAddonDrawer : SPEditorAddonDrawer
    {

        public override bool IsFooter
        {
            get
            {
                var attrib = this.Attribute as InsertButtonAttribute;
                if (attrib != null) return !attrib.PrecedeProperty;
                return base.IsFooter;
            }
            protected set
            {
                base.IsFooter = value;
            }
        }

        public override void OnInspectorGUI()
        {
            var attrib = this.Attribute as InsertButtonAttribute;
            if (attrib == null) return;
            if (attrib.RuntimeOnly && !Application.isPlaying) return;
            if (!attrib.SupportsMultiObjectEditing && this.SerializedObject.isEditingMultipleObjects) return;

            if (GUILayout.Button(attrib.Label))
            {
                foreach (var obj in this.SerializedObject.targetObjects)
                {
                    DynamicUtil.InvokeMethod(obj, attrib.OnClick);
                }
            }
        }

    }

}