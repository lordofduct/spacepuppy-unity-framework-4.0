using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Tween;
using com.spacepuppy.Tween.Events;

using com.spacepuppyeditor.Core.Events;

namespace com.spacepuppyeditor.Tween.Events
{

    [CustomEditor(typeof(i_TweenState))]
    public class i_TweenStateInspector : SPEditor
    {

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);

            this.DrawPropertyField(i_TweenState.PROP_TARGET);

            var propSourceAlt = this.serializedObject.FindProperty(i_TweenState.PROP_ANIMMODE);
            switch (propSourceAlt.GetEnumValue<TweenHash.AnimMode>())
            {
                case TweenHash.AnimMode.To:
                case TweenHash.AnimMode.From:
                case TweenHash.AnimMode.By:
                    this.DrawPropertyField(i_TweenState.PROP_SOURCE, "Values", false);
                    TriggerableTargetObjectPropertyDrawer.ResetTriggerableTargetObjectTarget(this.serializedObject.FindProperty(i_TweenState.PROP_SOURCEALT));
                    break;
                case TweenHash.AnimMode.FromTo:
                case TweenHash.AnimMode.RedirectTo:
                    this.DrawPropertyField(i_TweenState.PROP_SOURCE, "Start", false);
                    this.DrawPropertyField(i_TweenState.PROP_SOURCEALT, "End", false);
                    break;
            }

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, i_TweenState.PROP_TARGET, i_TweenState.PROP_SOURCE, i_TweenState.PROP_SOURCEALT);

            this.serializedObject.ApplyModifiedProperties();
        }

    }

}
