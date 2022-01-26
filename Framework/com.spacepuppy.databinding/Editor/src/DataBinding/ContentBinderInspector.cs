using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;
using com.spacepuppyeditor;
using com.spacepuppy.DataBinding;

namespace com.spacepuppyeditor.DataBinding
{

    [CustomEditor(typeof(ContentBinder), true)]
    public class ContentBinderInspector : SPEditor
    {

        protected override void OnSPInspectorGUI()
        {
            ContentBinder target;
            DataBindingContext context;
            ISourceBindingProtocol protocol;
            if (this.serializedObject.isEditingMultipleObjects || 
                (target = this.serializedObject.targetObject as ContentBinder) == null ||
                (context = target.GetComponent<DataBindingContext>()) == null ||
                (protocol = context.BindingProtocol) == null)
            {
                this.DrawDefaultInspector();
                return;
            }

            var keys = protocol.GetDefinedKeys();
            if(keys == null || !keys.Any())
            {
                this.DrawDefaultInspector();
                return;
            }

            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);

            var keyprop = this.serializedObject.FindProperty(ContentBinder.PROP_KEY);
            
            GUIContent[] names = keys.Select(o => EditorHelper.TempContent(o)).ToArray();
            int index = names.IndexOf(o => string.Equals(o.text, keyprop.stringValue));
            EditorGUI.BeginChangeCheck();
            index = EditorGUILayout.Popup(EditorHelper.TempContent(keyprop.displayName, keyprop.tooltip), index, names);
            if (EditorGUI.EndChangeCheck())
            {
                keyprop.stringValue = index >= 0 ? names[index].text : string.Empty;
            }

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, ContentBinder.PROP_KEY);
            
            this.serializedObject.ApplyModifiedProperties();
        }

    }
}
