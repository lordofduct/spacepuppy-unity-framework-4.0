using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;
using com.spacepuppyeditor;
using com.spacepuppy.DataBinding;
using com.spacepuppy.Collections;
using com.spacepuppyeditor.Core;

namespace com.spacepuppyeditor.DataBinding
{

    [CustomEditor(typeof(ColorBinder), editorForChildClasses:true)]
    public class ColorBinderInspector : SPEditor
    {

        public const string PROP_KEY = "_key";
        public const string PROP_TARGET = "_target";

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(PROP_KEY);

            var targprop = this.serializedObject.FindProperty(PROP_TARGET);
            var go = GameObjectUtil.GetGameObjectFromSource(targprop.objectReferenceValue);

            if (go != null)
            {
                var rect = EditorGUILayout.GetControlRect(true);
                rect = EditorGUI.PrefixLabel(rect, EditorHelper.TempContent("Target"));

                if(SPEditorGUI.XButton(ref rect))
                {
                    targprop.objectReferenceValue = null;
                    goto DrawEverythingElse;
                }
                SPEditorGUI.RefButton(ref rect, go);

                using (var comps = TempCollection.GetList<Component>())
                {
                    go.GetComponents<Component>(comps);
                    for (int i = 0; i < comps.Count; i++)
                    {
                        if (!ColorBinder.HasColorMember(comps[i]))
                        {
                            comps.RemoveAt(i);
                            i--;
                        }
                    }

                    if (comps.Count == 0)
                    {
                        targprop.objectReferenceValue = null;
                        DrawEmptyTargetField(targprop);
                    }
                    else
                    {
                        var labels = comps.Select((o, i) => EditorHelper.TempContent(string.Format("{0} : {1} [{2}]", o.name, o.GetType().Name, i))).ToArray();
                        int index = comps.IndexOf(targprop.objectReferenceValue as Component);

                        EditorGUI.BeginChangeCheck();

                        index = EditorGUI.Popup(rect, index, labels);
                        if (EditorGUI.EndChangeCheck())
                        {
                            targprop.objectReferenceValue = index >= 0 ? comps[index] : null;
                        }
                    }
                }
            }
            else
            {
                DrawEmptyTargetField(targprop);
            }

            DrawEverythingElse:
            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, PROP_KEY, PROP_TARGET);

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawEmptyTargetField(SerializedProperty targprop)
        {
            EditorGUI.BeginChangeCheck();
            var comp = EditorGUILayout.ObjectField("Target", targprop.objectReferenceValue, typeof(Component), true) as Component;
            if (EditorGUI.EndChangeCheck() && comp)
            {
                targprop.objectReferenceValue = comp.gameObject.GetComponents<Component>().FirstOrDefault(ColorBinder.HasColorMember);
            }
        }

    }

}
