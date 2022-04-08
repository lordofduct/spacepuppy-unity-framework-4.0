using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.DataBinding;

namespace com.spacepuppyeditor.DataBinding
{

    [CustomEditor(typeof(PropertyBinder))]
    public class PropertyBinderInspector : SPEditor
    {

        private const string PROP_TARGET = "_target";
        private const string PROP_TARGETPROPERTY = "_targetProperty";

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(ContentBinder.PROP_KEY);

            var targprop = this.serializedObject.FindProperty(PROP_TARGET);
            var nameprop = this.serializedObject.FindProperty(PROP_TARGETPROPERTY);

            SPEditorGUILayout.PropertyField(targprop);

            var props = DynamicUtil.GetMemberNames(targprop.objectReferenceValue, false, System.Reflection.MemberTypes.Field | System.Reflection.MemberTypes.Property).ToArray();
            nameprop.stringValue = SPEditorGUILayout.OptionPopupWithCustom("Target Property", nameprop.stringValue, props);

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, ContentBinder.PROP_KEY, PROP_TARGET, PROP_TARGETPROPERTY);

            this.serializedObject.ApplyModifiedProperties();
        }

    }

}
