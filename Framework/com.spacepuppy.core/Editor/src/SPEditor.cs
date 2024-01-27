using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;

using com.spacepuppyeditor.Internal;

namespace com.spacepuppyeditor
{

#if !DISABLE_GLOBAL_SPEDITOR
    [CustomEditor(typeof(MonoBehaviour), true)]
#else
    [CustomEditor(typeof(SPComponent), true)]
#endif
    [CanEditMultipleObjects()]
    public class SPEditor : Editor
    {

        #region Fields

        private List<GUIDrawer> _headerDrawers;
        private SPEditorAddonDrawer[] _addons;

        private List<ShownPropertyInfo> _shownFields;
        private ConstantlyRepaintEditorAttribute _constantlyRepaint;

        private bool _runtimeValuesFoldoutOpen = false;

        #endregion

        #region CONSTRUCTOR

        static SPEditor()
        {
            Editor.finishedDefaultHeaderGUI += (e) => {
                if (e.serializedObject.targetObject is ScriptableObject)
                {
                    var width = Mathf.Max(0f, Screen.width - 100f);
                    var r = new Rect(45f, 28f, Mathf.Min(width, 60f), 18f);

                    if (GUI.Button(r, "Select"))
                    {
                        foreach (var obj in e.serializedObject.targetObjects) EditorGUIUtility.PingObject(obj);
                    }
                }
            };
        }

        protected virtual void OnEnable()
        {
            var tp = this.target.GetType();
            var fields = tp.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
            foreach (var f in fields)
            {
                var attribs = f.GetCustomAttributes(typeof(ShowNonSerializedPropertyAttribute), false) as ShowNonSerializedPropertyAttribute[];
                if (attribs != null && attribs.Length > 0)
                {
                    if (_shownFields == null) _shownFields = new List<ShownPropertyInfo>();
                    var attrib = attribs[0];
                    _shownFields.Add(new ShownPropertyInfo()
                    {
                        Attrib = attrib,
                        MemberInfo = f,
                        Label = (attrib.Readonly || DynamicUtil.GetMemberAccessLevel(f) == DynamicMemberAccess.Read) ? new GUIContent((attrib.Label ?? f.Name) + " (readonly)", attrib.Tooltip) : new GUIContent(attrib.Label ?? f.Name, attrib.Tooltip)
                    });
                }
            }

            _constantlyRepaint = tp.GetCustomAttributes(typeof(ConstantlyRepaintEditorAttribute), false).FirstOrDefault() as ConstantlyRepaintEditorAttribute;
        }

        protected virtual void OnDisable()
        {
            if (_addons?.Length > 0)
            {
                for (int i = 0; i < _addons.Length; i++)
                {
                    _addons[i].OnDisable();
                }
            }
            _addons = null;
        }

        #endregion

        #region GUI Methods

        public sealed override void OnInspectorGUI()
        {
            if (!(this.target is SPComponent) && (!SpacepuppySettings.UseSPEditorAsDefaultEditor || (this.target?.GetType().Assembly.FullName.Contains("UnityEngine.") ?? false)))
            {
                base.OnInspectorGUI();
                return;
            }

            this.OnBeforeSPInspectorGUI();

            EditorGUI.BeginChangeCheck();

            //draw header infobox if needed
            this.DrawDefaultInspectorHeader();
            this.OnSPInspectorGUI();
            this.DrawDefaultInspectorFooters();

            if (EditorGUI.EndChangeCheck())
            {
                //do call onValidate
                PropertyHandlerValidationUtility.OnInspectorGUIComplete(this.serializedObject, true);
                this.OnValidate();

                /*
                 * TODO - IValidateReceiver
                 * 
                if(SpacepuppySettings.SignalValidateReceiver)
                {
                    foreach (var obj in this.serializedObject.targetObjects)
                    {
                        var iterator = serializedObject.GetIterator();
                        while (iterator.Next(true))
                        {
                            if (iterator.propertyType == SerializedPropertyType.ObjectReference) continue;

                            var validated = EditorHelper.GetTargetObjectOfProperty(iterator, obj) as IValidateReceiver;
                            validated?.OnValidate();
                        }
                    }
                }
                 */
            }
            else
            {
                PropertyHandlerValidationUtility.OnInspectorGUIComplete(this.serializedObject, false);
            }

            if (_shownFields != null && _shownFields.Any(o => o.Attrib.ShowOutsideRuntimeValuesFoldout && (o.Attrib.ShowAtEditorTime || UnityEngine.Application.isPlaying)))
            {
                foreach (var info in _shownFields.Where(o => o.Attrib.ShowOutsideRuntimeValuesFoldout && (o.Attrib.ShowAtEditorTime || UnityEngine.Application.isPlaying)))
                {
                    switch (DynamicUtil.GetMemberAccessLevel(info.MemberInfo))
                    {
                        case DynamicMemberAccess.Read:
                            {
                                var cache = SPGUI.DisableIf(info.Attrib.Readonly);
                                var value = DynamicUtil.GetValue(this.target, info.MemberInfo);
                                SPEditorGUILayout.DefaultPropertyField(info.Label, value, DynamicUtil.GetReturnType(info.MemberInfo));
                                cache.Reset();
                            }
                            break;
                        case DynamicMemberAccess.ReadWrite:
                            {
                                var cache = SPGUI.DisableIf(info.Attrib.Readonly);

                                var value = DynamicUtil.GetValue(this.target, info.MemberInfo);
                                EditorGUI.BeginChangeCheck();
                                value = SPEditorGUILayout.DefaultPropertyField(info.Label, value, DynamicUtil.GetReturnType(info.MemberInfo));
                                if (EditorGUI.EndChangeCheck() && !info.Attrib.Readonly)
                                {
                                    DynamicUtil.SetValue(this.target, info.MemberInfo, value);
                                }

                                cache.Reset();
                            }
                            break;
                        default:
                            EditorGUILayout.LabelField(info.Label, EditorHelper.TempContent("* Unreadable Member *"));
                            break;
                    }
                }
            }

            if (_shownFields != null && _shownFields.Any(o => !o.Attrib.ShowOutsideRuntimeValuesFoldout && (o.Attrib.ShowAtEditorTime || UnityEngine.Application.isPlaying)))
            {
                EditorGUILayout.BeginVertical("box");
                var style = new GUIStyle(EditorStyles.boldLabel);
                style.alignment = TextAnchor.MiddleCenter;

                var r = EditorGUILayout.GetControlRect();
                GUI.Label(r, "Runtime Values", style);
                _runtimeValuesFoldoutOpen = EditorGUI.Foldout(r, _runtimeValuesFoldoutOpen, GUIContent.none, true);
                if (_runtimeValuesFoldoutOpen)
                {
                    foreach (var info in _shownFields.Where(o => !o.Attrib.ShowOutsideRuntimeValuesFoldout && (o.Attrib.ShowAtEditorTime || UnityEngine.Application.isPlaying)))
                    {
                        switch (DynamicUtil.GetMemberAccessLevel(info.MemberInfo))
                        {
                            case DynamicMemberAccess.Read:
                                {
                                    var cache = SPGUI.DisableIf(info.Attrib.Readonly);
                                    var value = DynamicUtil.GetValue(this.target, info.MemberInfo);
                                    SPEditorGUILayout.DefaultPropertyField(info.Label, value, DynamicUtil.GetReturnType(info.MemberInfo));
                                    cache.Reset();
                                }
                                break;
                            case DynamicMemberAccess.ReadWrite:
                                {
                                    var cache = SPGUI.DisableIf(info.Attrib.Readonly);

                                    var value = DynamicUtil.GetValue(this.target, info.MemberInfo);
                                    EditorGUI.BeginChangeCheck();
                                    value = SPEditorGUILayout.DefaultPropertyField(info.Label, value, DynamicUtil.GetReturnType(info.MemberInfo));
                                    if (EditorGUI.EndChangeCheck() && !info.Attrib.Readonly)
                                    {
                                        DynamicUtil.SetValue(this.target, info.MemberInfo, value);
                                    }

                                    cache.Reset();
                                }
                                break;
                            default:
                                EditorGUILayout.LabelField(info.Label, EditorHelper.TempContent("* Unreadable Member *"));
                                break;
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }

            this.OnAfterSPInspectorGUI();
        }

        protected virtual void OnBeforeSPInspectorGUI()
        {

        }

        protected virtual void OnSPInspectorGUI()
        {
            if (this.serializedObject.isEditingMultipleObjects)
            {
                var tp = this.GetType();
                if (tp != typeof(SPEditor) && tp.GetCustomAttribute<CanEditMultipleObjects>(false) == null)
                {
                    this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
                    EditorGUILayout.LabelField("Multi-object editing not supported.");
                    return;
                }

                if (tp == typeof(SPEditor))
                {
                    //var dtp = ScriptAttributeUtility.GetDrawerTypeForType(this.serializedObject.targetObject.GetType());
                    //Debug.Log(dtp?.Name ?? "NULL");

                    var editor = CreateEditor(this.serializedObject.targetObject);
                    if (editor != null)
                    {
                        tp = editor.GetType();
                        if (tp != typeof(SPEditor) && tp.GetCustomAttribute<CanEditMultipleObjects>(false) == null)
                        {
                            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
                            EditorGUILayout.LabelField("Multi-object editing not supported.");
                            return;
                        }
                    }
                }
            }

            this.DrawDefaultInspector();
        }

        protected virtual void OnAfterSPInspectorGUI()
        {

        }

        protected virtual void OnValidate()
        {

        }

        private void DrawDefaultInspectorHeader()
        {
            //var attribs = this.serializedObject.targetObject.GetType().GetCustomAttributes(typeof(InfoboxAttribute), false);
            //InfoboxAttribute infoboxAttrib = (attribs.Length > 0) ? attribs[0] as InfoboxAttribute : null;
            //if (infoboxAttrib != null)
            //{
            //    var position = EditorGUILayout.GetControlRect(false, com.spacepuppyeditor.Decorators.InfoboxDecorator.GetHeight(infoboxAttrib));
            //    com.spacepuppyeditor.Decorators.InfoboxDecorator.OnGUI(position, infoboxAttrib);
            //}

            if (_headerDrawers == null)
            {
                _headerDrawers = new List<GUIDrawer>();
                if (serializedObject.targetObject != null)
                {
                    var componentType = serializedObject.targetObject.GetType();
                    if (TypeUtil.IsType(componentType, typeof(Component), typeof(ScriptableObject)))
                    {
                        var attribs = (from o in componentType.GetCustomAttributes(typeof(ComponentHeaderAttribute), true)
                                       let a = o as ComponentHeaderAttribute
                                       where a != null
                                       orderby a.order
                                       select a).ToArray();
                        foreach (var attrib in attribs)
                        {
                            var dtp = ScriptAttributeUtility.GetDrawerTypeForType(attrib.GetType());
                            if (dtp != null)
                            {
                                if (TypeUtil.IsType(dtp, typeof(DecoratorDrawer)))
                                {
                                    var decorator = System.Activator.CreateInstance(dtp) as DecoratorDrawer;
                                    DynamicUtil.SetValue(decorator, "m_Attribute", attrib);
                                    _headerDrawers.Add(decorator);
                                }
                                else if (TypeUtil.IsType(dtp, typeof(ComponentHeaderDrawer)))
                                {
                                    var drawer = System.Activator.CreateInstance(dtp) as ComponentHeaderDrawer;
                                    drawer.Init(attrib, componentType);
                                    _headerDrawers.Add(drawer);
                                }
                            }
                        }

                        /*
                         * Unity now supports doing this itself.
                         * 
                        var obsoleteAttrib = componentType.GetCustomAttributes(typeof(System.ObsoleteAttribute), false).FirstOrDefault() as System.ObsoleteAttribute;
                        if (obsoleteAttrib != null)
                        {
                            _headerDrawers.Add(new ObsoleteHeaderDrawer("This script is considered deprecated:\n\t" + obsoleteAttrib.Message));
                        }
                         */

                        _addons = SPEditorAddonDrawer.GetDrawers(this, this.serializedObject);
                    }
                }
            }

            for (int i = 0; i < _headerDrawers.Count; i++)
            {
                var drawer = _headerDrawers[i];
                if (drawer is DecoratorDrawer)
                {
                    var decorator = drawer as DecoratorDrawer;
                    var h = decorator.GetHeight();
                    Rect position = EditorGUILayout.GetControlRect(false, h);
                    decorator.OnGUI(position);
                }
                else if (drawer is ComponentHeaderDrawer)
                {
                    var compDrawer = drawer as ComponentHeaderDrawer;
                    var h = compDrawer.GetHeight(this.serializedObject);
                    Rect position = EditorGUILayout.GetControlRect(false, h);
                    compDrawer.OnGUI(position, this.serializedObject);
                }
            }

            if (_addons != null)
            {
                foreach (var d in _addons)
                {
                    if (!d.IsFooter) d.OnInspectorGUI();
                }
            }
        }

        private void DrawDefaultInspectorFooters()
        {
            if (_addons != null)
            {
                foreach (var d in _addons)
                {
                    if (d.IsFooter) d.OnInspectorGUI();
                }
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return base.RequiresConstantRepaint() ||
                   (_shownFields != null && _shownFields.Any(o => Application.isPlaying || o.Attrib.ShowAtEditorTime)) ||
                   (_constantlyRepaint != null && (Application.isPlaying || !_constantlyRepaint.RuntimeOnly)) ||
                   (_addons != null && _addons.Length > 0 && _addons.Any((o) => o.RequiresConstantRepaint()));
        }

        #endregion

        #region Draw Methods

        /// <summary>
        /// Draw the inspector as it would have been if not an SPEditor.
        /// </summary>
        public void DrawDefaultStandardInspector()
        {
            base.DrawDefaultInspector();
        }

        public new bool DrawDefaultInspector()
        {
            //draw properties
            this.serializedObject.UpdateIfRequiredOrScript();
            var result = SPEditor.DrawDefaultInspectorExcept(this.serializedObject);
            this.serializedObject.ApplyModifiedProperties();

            return result;
        }

        public void DrawDefaultInspectorExcept(params string[] propsNotToDraw)
        {
            DrawDefaultInspectorExcept(this.serializedObject, propsNotToDraw);
        }

        public bool DrawPropertyField(string prop)
        {
            return SPEditorGUILayout.PropertyField(this.serializedObject, prop);
        }

        public bool DrawPropertyField(string prop, bool includeChildren)
        {
            return SPEditorGUILayout.PropertyField(this.serializedObject, prop, includeChildren);
        }

        public bool DrawPropertyField(string prop, string label, bool includeChildren)
        {
            return SPEditorGUILayout.PropertyField(this.serializedObject, prop, label, includeChildren);
        }

        public bool DrawPropertyField(string prop, GUIContent label, bool includeChildren)
        {
            return SPEditorGUILayout.PropertyField(this.serializedObject, prop, label, includeChildren);
        }

        #endregion

        #region Static Interface

        public static bool DrawDefaultInspectorExcept(SerializedObject serializedObject, params string[] propsNotToDraw)
        {
            if (serializedObject == null) throw new System.ArgumentNullException(nameof(serializedObject));

            EditorGUI.BeginChangeCheck();
            SerializedProperty iterator = serializedObject.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                if (propsNotToDraw == null || !propsNotToDraw.Contains(iterator.propertyPath))
                {
                    //using (new EditorGUI.DisabledScope(EditorHelper.PROP_SCRIPT == iterator.propertyPath))
                    {
                        //EditorGUILayout.PropertyField(iterator, true);
                        SPEditorGUILayout.PropertyField(iterator, true);
                    }
                }
            }
            return EditorGUI.EndChangeCheck();
        }

        #endregion



        #region Special Types

        private class ShownPropertyInfo
        {

            public ShowNonSerializedPropertyAttribute Attrib;
            public System.Reflection.MemberInfo MemberInfo;
            public GUIContent Label;

        }

        private class ObsoleteHeaderDrawer : DecoratorDrawer
        {

            private string _message;

            public ObsoleteHeaderDrawer(string msg)
            {
                _message = msg;
            }

            public override float GetHeight()
            {
                return EditorStyles.helpBox.CalcSize(EditorHelper.TempContent(_message)).y;
            }

            public override void OnGUI(Rect position)
            {
                EditorGUI.HelpBox(position, _message, MessageType.Warning);
            }

        }

        #endregion

    }

    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects()]
    public class SPScriptableObjectEditor : SPEditor
    {

    }

    [CustomEditor(typeof(StateMachineBehaviour), true)]
    [CanEditMultipleObjects()]
    public class SPStateMachineBehaviourEditor : SPEditor
    {

    }

}
