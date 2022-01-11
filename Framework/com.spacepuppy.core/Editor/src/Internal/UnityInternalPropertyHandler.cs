using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Internal
{
    internal class UnityInternalPropertyHandler : IPropertyHandler
    {

        #region Fields

        private TypeAccessWrapper _internalPropertyHandler;

        private System.Func<SerializedProperty, GUIContent, bool, float> _imp_GetHeight;
        private System.Func<Rect, SerializedProperty, GUIContent, bool, bool> _imp_OnGUI;
        private System.Func<SerializedProperty, GUIContent, bool, GUILayoutOption[], bool> _imp_OnGUILayout;

        private System.Action<SerializedProperty, PropertyAttribute, System.Reflection.FieldInfo, System.Type> _imp_HandleAttribute;

        #endregion

        #region CONSTRUCTOR

        public UnityInternalPropertyHandler(object internalPropertyhandler)
        {
            var klass = InternalTypeUtil.UnityEditorAssembly.GetType("UnityEditor.PropertyHandler");
            if (!klass.IsInstanceOfType(internalPropertyhandler)) throw new System.ArgumentException("Must be an instance of the internal UnityEditor.PropertyHandler type.", nameof(internalPropertyhandler));
            _internalPropertyHandler = new TypeAccessWrapper(klass, internalPropertyhandler, true);
        }

        public UnityInternalPropertyHandler()
        {
            var klass = InternalTypeUtil.UnityEditorAssembly.GetType("UnityEditor.PropertyHandler");
            var obj = System.Activator.CreateInstance(klass);
            _internalPropertyHandler = new TypeAccessWrapper(klass, obj, true);
        }

        #endregion

        #region Properties

        protected PropertyDrawer InternalDrawer
        {
            get
            {
                return _internalPropertyHandler.GetProperty("m_PropertyDrawer") as PropertyDrawer;
            }
            set
            {
                _internalPropertyHandler.SetProperty("m_PropertyDrawer", value);
            }
        }

        protected List<DecoratorDrawer> DecoratorDrawers
        {
            get
            {
                return _internalPropertyHandler.GetProperty("m_DecoratorDrawers") as List<DecoratorDrawer>;
            }
            set
            {
                _internalPropertyHandler.SetProperty("m_DecoratorDrawers", value);
            }
        }

        protected bool isCurrentlyNested
        {
            get { return (bool)_internalPropertyHandler.GetProperty("isCurrentlyNested"); }
        }

        protected string tooltip
        {
            get { return _internalPropertyHandler.GetProperty("tooltip") as string; }
        }

        #endregion

        #region Methods

        protected virtual void HandleAttribute(SerializedProperty property, PropertyAttribute attribute, System.Reflection.FieldInfo field, System.Type propertyType)
        {
            if (_imp_HandleAttribute == null) _imp_HandleAttribute = _internalPropertyHandler.GetMethod("HandleAttribute", typeof(System.Action<SerializedProperty, PropertyAttribute, System.Reflection.FieldInfo, System.Type>)) as System.Action<SerializedProperty, PropertyAttribute, System.Reflection.FieldInfo, System.Type>;
            _imp_HandleAttribute(property, attribute, field, propertyType);
        }

        #endregion

        #region IPropertyHandler Interface

        //TODO - SP4.0 - Unity has added support for their own reorderablearray which conflicts with SP's. This logic here works around that for now, BUT we probably should just gut that and go to their implementation and enhance as we need. Don't really have the time for that right now though.

        public virtual float GetHeight(UnityEditor.SerializedProperty property, UnityEngine.GUIContent label, bool includeChildren)
        {
            if (this.InternalDrawer is IArrayHandlingPropertyDrawer)
            {
                float num1 = 0.0f;
                if (this.DecoratorDrawers != null && !this.isCurrentlyNested)
                {
                    foreach (DecoratorDrawer decoratorDrawer in this.DecoratorDrawers)
                    {
                        num1 += decoratorDrawer.GetHeight();
                    }
                }
                float num2;
                if (this.InternalDrawer != null)
                {
                    num2 = num1 + this.InternalDrawer.GetPropertyHeight(property.Copy(), label ?? EditorHelper.TempContent(property.displayName, this.tooltip));
                }
                else if (!includeChildren)
                {
                    num2 = num1 + EditorGUIUtility.singleLineHeight;
                }
                else
                {
                    property = property.Copy();
                    num2 = num1 + EditorGUIUtility.singleLineHeight;
                    bool enterChildren = property.isExpanded && property.hasVisibleChildren; // EditorGUI.HasVisibleChildFields(property, false);
                    GUIContent label1 = EditorHelper.TempContent(property.displayName, this.tooltip);
                    if (enterChildren)
                    {
                        SerializedProperty endProperty = property.GetEndProperty();
                        while (property.NextVisible(enterChildren) && !SerializedProperty.EqualContents(property, endProperty))
                        {
                            float num3 = num2 + ScriptAttributeUtility.GetHandler(property).GetHeight(property, label1, true);
                            enterChildren = false;
                            num2 = num3 + 2f;
                        }
                    }
                }
                return num2;
            }
            else
            {
                if (_imp_GetHeight == null) _imp_GetHeight = _internalPropertyHandler.GetMethod("GetHeight", typeof(System.Func<SerializedProperty, GUIContent, bool, float>)) as System.Func<SerializedProperty, GUIContent, bool, float>;
                return _imp_GetHeight(property, label, includeChildren);
            }
        }

        public virtual bool OnGUI(UnityEngine.Rect position, UnityEditor.SerializedProperty property, UnityEngine.GUIContent label, bool includeChildren)
        {
            if (_imp_OnGUI == null) _imp_OnGUI = _internalPropertyHandler.GetMethod("OnGUI", typeof(System.Func<Rect, SerializedProperty, GUIContent, bool, bool>)) as System.Func<Rect, SerializedProperty, GUIContent, bool, bool>;
            return _imp_OnGUI(position, property, label, includeChildren);
        }

        public virtual bool OnGUILayout(UnityEditor.SerializedProperty property, UnityEngine.GUIContent label, bool includeChildren, UnityEngine.GUILayoutOption[] options)
        {
            if (this.InternalDrawer is IArrayHandlingPropertyDrawer)
            {
                var rect = EditorGUILayout.GetControlRect(LabelHasContent(label), this.GetHeight(property, label, includeChildren), options);
                return this.OnGUI(rect, property, label, includeChildren);
            }
            else
            {
                if (_imp_OnGUILayout == null) _imp_OnGUILayout = _internalPropertyHandler.GetMethod("OnGUILayout", typeof(System.Func<SerializedProperty, GUIContent, bool, GUILayoutOption[], bool>)) as System.Func<SerializedProperty, GUIContent, bool, GUILayoutOption[], bool>;
                return _imp_OnGUILayout(property, label, includeChildren, options);
            }
        }

        public virtual void OnValidate(SerializedProperty property)
        {
            //TODO
        }

        #endregion
        static bool LabelHasContent(GUIContent label)
        {
            if (label == null)
                return true;
            return !string.IsNullOrEmpty(label.text) || (UnityEngine.Object)label.image != null;
        }
    }
}
