using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(RequireTagAttribute))]
    public class RequireTagHeaderDrawer : ComponentHeaderDrawer
    {

        public override float GetHeight(SerializedObject serializedObject)
        {
            var attrib = this.Attribute as RequireTagAttribute;
            if (attrib == null || attrib.Tags == null || attrib.Tags.Length == 0 || attrib.HideInfoBox) return 0f;

            GUIStyle style = GUI.skin.GetStyle("HelpBox");
            return Mathf.Max(40f, style.CalcHeight(EditorHelper.TempContent(GetHeaderText(attrib)), EditorGUIUtility.currentViewWidth));
        }

        public override void OnGUI(Rect position, SerializedObject serializedObject)
        {
            var attrib = this.Attribute as RequireTagAttribute;
            if (attrib == null || attrib.Tags == null || attrib.Tags.Length == 0 || !(serializedObject.targetObject is Component)) return;

            foreach (var c in serializedObject.targetObjects)
            {
                var go = (c as Component)?.gameObject;
                if (go)
                {
                    var missingTags = attrib.Tags.Where(s => !string.IsNullOrEmpty(s)).Where(s => !go.HasTag(s));
                    if (missingTags.Any())
                    {
                        foreach (var s in missingTags)
                        {
                            go.AddTag(s);
                        }
                        EditorHelper.CommitDirectChanges(go, false);
                    }
                }
            }

            if (!attrib.HideInfoBox)
            {
                EditorGUI.HelpBox(position, GetHeaderText(attrib), MessageType.Info);
            }
        }

        static string GetHeaderText(RequireTagAttribute attrib)
        {
            var stags = attrib?.Tags?.Where(s => !string.IsNullOrEmpty(s)) ?? Enumerable.Empty<string>();
            if (stags.Count() > 1)
            {
                return "This component requires the tags: '" + string.Join("', '", stags) + "'.";
            }
            else
            {
                return "This component requires the tag: '" + stags.FirstOrDefault() + "'.";
            }

        }

    }
}
