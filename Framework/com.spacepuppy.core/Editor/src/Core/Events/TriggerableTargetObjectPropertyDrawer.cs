using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core.Events
{

    [CustomPropertyDrawer(typeof(TriggerableTargetObject), true)]
    public class TriggerableTargetObjectPropertyDrawer : PropertyDrawer
    {

        private enum PrettyResolveByCommand
        {
            Nothing = SearchBy.Nothing,
            WithTag = SearchBy.Tag,
            WithName = SearchBy.Name,
            WithType = SearchBy.Type
        }

        protected const float LEN_TARGETSOURCE = 60f;
        protected const float LEN_FINDCOMMAND = 140f;
        protected const float LEN_RESOLVEBYCOMMAND = 80f;

        public const string PROP_CONFIGURED = "_configured";
        public const string PROP_TARGET = "_target";
        public const string PROP_FIND = "_find";
        public const string PROP_RESOLVEBY = "_resolveBy";
        public const string PROP_QUERY = "_queryString";

        public enum TargetSource
        {
            Arg = 0,
            Self = 1,
            Root = 2,
            Config = 3
        }

        #region Fields

        private System.Type _targetType;
        public bool ManuallyConfigured;
        public bool DefaultFromSelf;
        public bool AlwaysExpanded;

        private SelectableComponentPropertyDrawer _objectDrawer = new SelectableComponentPropertyDrawer()
        {
            AllowNonComponents = true,
            AllowProxy = true
        };
        private bool _defaultSet;

        #endregion

        #region CONSTRUCTOR

        public TriggerableTargetObjectPropertyDrawer()
        {

        }

        public TriggerableTargetObjectPropertyDrawer(System.Type targetType, bool searchChildren, bool defaultFromSelf = false, bool alwaysExpanded = false)
        {
            this.ManuallyConfigured = true;
            this.TargetType = targetType;
            this.SearchChildren = searchChildren;
            this.DefaultFromSelf = defaultFromSelf;
            this.AlwaysExpanded = alwaysExpanded;
        }

        #endregion

        #region Properties

        public System.Type TargetType
        {
            get { return _targetType; }
            set
            {
                _targetType = value;
                _objectDrawer.RestrictionType = value ?? typeof(UnityEngine.Object); //typeof(Component);
            }
        }

        public bool SearchChildren
        {
            get { return _objectDrawer.SearchChildren; }
            set { _objectDrawer.SearchChildren = value; }
        }

        public IComponentChoiceSelector ChoiceSelector
        {
            get { return _objectDrawer.ChoiceSelector; }
            set { _objectDrawer.ChoiceSelector = value; }
        }

        #endregion

        #region Methods

        protected virtual void Init(SerializedProperty property)
        {
            if (this.ManuallyConfigured) return;
            if (this.fieldInfo == null) return;

            var attrib = this.fieldInfo.GetCustomAttributes(typeof(TriggerableTargetObject.ConfigAttribute), false).FirstOrDefault() as TriggerableTargetObject.ConfigAttribute;
            if(attrib != null)
            {
                this.TargetType = attrib.TargetType;
                this.SearchChildren = attrib.SearchChildren;
                this.DefaultFromSelf = attrib.DefaultFromSelf;
                this.AlwaysExpanded = attrib.AlwaysExpanded;
            }
            else
            {
                this.TargetType = null;
                this.SearchChildren = false;
                this.DefaultFromSelf = false;
                this.AlwaysExpanded = false;
            }
        }





        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            this.Init(property);

            if (this.AlwaysExpanded || property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight * 2f;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            this.Init(property);


            EditorGUI.BeginProperty(position, label, property);

            //################################
            //FIRST LINE
            var configProp = property.FindPropertyRelative(PROP_CONFIGURED);
            var targetProp = property.FindPropertyRelative(PROP_TARGET);
            var findProp = property.FindPropertyRelative(PROP_FIND);

            var prefixRect = new Rect(position.xMin, position.yMin, Mathf.Max(0f, EditorGUIUtility.labelWidth - LEN_TARGETSOURCE), EditorGUIUtility.singleLineHeight);
            var rect = new Rect(prefixRect.xMax, position.yMin, position.width - prefixRect.width, EditorGUIUtility.singleLineHeight);
            if (!this.AlwaysExpanded)
            {
                property.isExpanded = EditorGUI.Foldout(prefixRect, property.isExpanded, label);
            }
            else
            {
                EditorGUI.LabelField(prefixRect, label);
            }

            rect = this.DrawTargetSource(rect, property, label);
            rect = this.DrawTarget(rect, property, label);


            //################################
            //SECOND LINE
            if (this.AlwaysExpanded || property.isExpanded)
            {
                var indent = Mathf.Max(0f, EditorGUIUtility.labelWidth - LEN_FINDCOMMAND);
                rect = new Rect(position.xMin + indent, position.yMin + EditorGUIUtility.singleLineHeight, Mathf.Max(0f, position.width - indent), EditorGUIUtility.singleLineHeight);

                var w0 = Mathf.Min(EditorGUIUtility.labelWidth, LEN_FINDCOMMAND);
                var w1 = Mathf.Min((rect.width - w0) * 0.4f, LEN_RESOLVEBYCOMMAND);
                var w2 = rect.width - w0 - w1;
                var r0 = new Rect(rect.xMin, rect.yMin, w0, rect.height);
                var r1 = new Rect(r0.xMax, rect.yMin, w1, rect.height);
                var r2 = new Rect(r1.xMax, rect.yMin, w2, rect.height);

                var resolveProp = property.FindPropertyRelative(PROP_RESOLVEBY);
                var queryProp = property.FindPropertyRelative(PROP_QUERY);

                var e0 = findProp.GetEnumValue<TriggerableTargetObject.FindCommand>();
                EditorGUI.BeginChangeCheck();
                e0 = (TriggerableTargetObject.FindCommand)EditorGUI.EnumPopup(r0, e0);
                if (EditorGUI.EndChangeCheck())
                {
                    findProp.SetEnumValue(e0);
                }
                switch (e0)
                {
                    case TriggerableTargetObject.FindCommand.FindInScene:
                    case TriggerableTargetObject.FindCommand.FindEntityInScene:
                        configProp.boolValue = false;
                        targetProp.objectReferenceValue = null;
                        break;
                }

                var e1 = resolveProp.GetEnumValue<PrettyResolveByCommand>();
                EditorGUI.BeginChangeCheck();
                e1 = (PrettyResolveByCommand)EditorGUI.EnumPopup(r1, e1);
                if (EditorGUI.EndChangeCheck())
                    resolveProp.SetEnumValue(e1);

                switch (e1)
                {
                    case PrettyResolveByCommand.Nothing:
                        if (e0 != TriggerableTargetObject.FindCommand.Direct || targetProp.objectReferenceValue == null)
                        {
                            var cache = SPGUI.Disable();
                            EditorGUI.TextField(r2, string.Empty);
                            queryProp.stringValue = string.Empty;
                            cache.Reset();
                        }
                        else
                        {
                            var members = GetMembersFromTarget(targetProp.objectReferenceValue);
                            var names = members.Select(m => m.Name).Prepend(string.Empty).ToArray();
                            var guicontents = names.Select(o => string.IsNullOrEmpty(o) ? EditorHelper.TempContent("--Direct--") : EditorHelper.TempContent(o)).ToArray();

                            int index = names.IndexOf(queryProp.stringValue);
                            EditorGUI.BeginChangeCheck();
                            index = EditorGUI.Popup(r2, index, guicontents);
                            if (EditorGUI.EndChangeCheck())
                            {
                                queryProp.stringValue = index > 0 ? names[index] : string.Empty;
                            }
                        }
                        break;
                    case PrettyResolveByCommand.WithTag:
                        {
                            queryProp.stringValue = EditorGUI.TagField(r2, queryProp.stringValue);
                        }
                        break;
                    case PrettyResolveByCommand.WithName:
                        {
                            queryProp.stringValue = EditorGUI.TextField(r2, queryProp.stringValue);
                        }
                        break;
                    case PrettyResolveByCommand.WithType:
                        {
                            var tp = TypeUtil.FindType(queryProp.stringValue);
                            if (!TypeUtil.IsType(tp, typeof(UnityEngine.Object))) tp = null;
                            tp = SPEditorGUI.TypeDropDown(r2, GUIContent.none, tp, typeof(UnityEngine.Object));
                            queryProp.stringValue = (tp != null) ? tp.FullName : null;
                        }
                        break;
                }
            }

            EditorGUI.EndProperty();
        }

        protected virtual Rect DrawTargetSource(Rect position, SerializedProperty property, GUIContent label)
        {
            var configProp = property.FindPropertyRelative(PROP_CONFIGURED);
            var targetProp = property.FindPropertyRelative(PROP_TARGET);
            var findProp = property.FindPropertyRelative(PROP_FIND);

            var r0 = new Rect(position.xMin, position.yMin, LEN_TARGETSOURCE, position.height);
            var e = (configProp.boolValue) ? TargetSource.Config : TargetSource.Arg;
            EditorGUI.BeginChangeCheck();
            e = (TargetSource)EditorGUI.EnumPopup(r0, e);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateTargetFromSource(targetProp, e);
                configProp.boolValue = (e != TargetSource.Arg);
                if (e != TargetSource.Arg) findProp.SetEnumValue(TriggerableTargetObject.FindCommand.Direct);
            }
            else if (e == TargetSource.Config && !_defaultSet && targetProp.objectReferenceValue == null)
            {
                UpdateTargetFromSource(targetProp, e);
                _defaultSet = true;
            }
            else
            {
                _defaultSet = true;
            }

            return new Rect(r0.xMax, position.yMin, position.width - r0.width, position.height);
        }

        protected virtual Rect DrawTarget(Rect position, SerializedProperty property, GUIContent label)
        {
            var configProp = property.FindPropertyRelative(PROP_CONFIGURED);
            var targetProp = property.FindPropertyRelative(PROP_TARGET);
            var findProp = property.FindPropertyRelative(PROP_FIND);

            var r1 = new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight);
            if (!configProp.boolValue)
            {
                var e0 = findProp.GetEnumValue<TriggerableTargetObject.FindCommand>();
                switch (e0)
                {
                    case TriggerableTargetObject.FindCommand.Direct:
                        EditorGUI.LabelField(r1, "Target determined by activating trigger.");
                        break;
                    case TriggerableTargetObject.FindCommand.FindParent:
                    case TriggerableTargetObject.FindCommand.FindInChildren:
                    case TriggerableTargetObject.FindCommand.FindInEntity:
                        EditorGUI.LabelField(r1, e0.ToString() + " of activating trigger arg.");
                        break;
                    case TriggerableTargetObject.FindCommand.FindInScene:
                    case TriggerableTargetObject.FindCommand.FindEntityInScene:
                    default:
                        configProp.boolValue = false;
                        targetProp.objectReferenceValue = null;
                        EditorGUI.LabelField(r1, e0.ToString());
                        break;
                }

                targetProp.objectReferenceValue = null;
            }
            else
            {
                _objectDrawer.OnGUI(r1, targetProp, GUIContent.none);
            }

            return new Rect(r1.xMax, r1.yMin, 0f, r1.height);
        }

        #endregion


        #region Utils

        protected void UpdateTargetFromSource(SerializedProperty property, TargetSource esrc)
        {
            switch(esrc)
            {
                case TargetSource.Arg:
                    {
                        property.objectReferenceValue = null;
                    }
                    break;
                case TargetSource.Self:
                    {
                        UnityEngine.Object obj = property.serializedObject.targetObject;
                        if (this.TargetType != null)
                            obj = ObjUtil.GetAsFromSource(this.TargetType, obj) as UnityEngine.Object;
                        property.objectReferenceValue = obj;
                    }
                    break;
                case TargetSource.Root:
                    {
                        UnityEngine.Object obj = property.serializedObject.targetObject;
                        var go = GameObjectUtil.GetGameObjectFromSource(obj);
                        if (go != null)
                            obj = go.FindRoot();

                        if (this.TargetType != null)
                            obj = ObjUtil.GetAsFromSource(this.TargetType, obj) as UnityEngine.Object;
                        property.objectReferenceValue = obj;
                    }
                    break;
                case TargetSource.Config:
                    {
                        if (this.DefaultFromSelf && property.objectReferenceValue == null)
                        {
                            UnityEngine.Object obj = property.serializedObject.targetObject;
                            if (this.TargetType != null)
                            {
                                obj = ObjUtil.GetAsFromSource(this.TargetType, obj) as UnityEngine.Object;
                            }
                            property.objectReferenceValue = obj;
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Static Utils

        static IEnumerable<MemberInfo> GetMembersFromTarget(object o)
        {
            const System.Reflection.MemberTypes MASK = System.Reflection.MemberTypes.Field | System.Reflection.MemberTypes.Property | System.Reflection.MemberTypes.Method;
            const DynamicMemberAccess ACCESS = DynamicMemberAccess.Read;

            var e = o is IDynamic ? DynamicUtil.GetMembers(o, false, MASK) : DynamicUtil.GetMembersFromType((o.IsProxy_ParamsRespecting() ? (o as IProxy).GetTargetType() : o.GetType()), false, MASK);
            return e.Where(m =>
            {
                if (m.IsObsolete()) return false;
                if (!DynamicUtil.GetMemberAccessLevel(m).HasFlagT(ACCESS)) return false;

                switch (m)
                {
                    case FieldInfo fi:
                        return true;
                    case PropertyInfo pi:
                        return pi.GetIndexParameters().Length == 0;
                    case MethodInfo mi:
                        return !mi.IsSpecialName && !mi.IsGenericMethod && mi.GetParameters().Length == 0;
                    default:
                        return false;
                }
            });
            //return o is IDynamic ? DynamicUtil.GetEasilySerializedMembers(o) : DynamicUtil.GetEasilySerializedMembersFromType((o.IsProxy_ParamsRespecting() ? (o as IProxy).GetTargetType() : o.GetType()));
        }

        public static void ResetTriggerableTargetObjectTarget(SerializedProperty prop)
        {
            if (prop == null) return;

            try
            {
                prop.FindPropertyRelative(PROP_CONFIGURED).boolValue = true;
                prop.FindPropertyRelative(PROP_TARGET).objectReferenceValue = null;
                prop.FindPropertyRelative(PROP_FIND).SetEnumValue(TriggerableTargetObject.FindCommand.Direct);
                prop.FindPropertyRelative(PROP_RESOLVEBY).SetEnumValue(SearchBy.Nothing);
                prop.FindPropertyRelative(PROP_QUERY).stringValue = string.Empty;
            }
            catch
            {

            }
        }

        public static System.Type GetTargetType(SerializedProperty prop)
        {
            var field = EditorHelper.GetFieldOfProperty(prop);
            var attrib = field != null ? field.GetCustomAttributes(typeof(TriggerableTargetObject.ConfigAttribute), false).FirstOrDefault() as TriggerableTargetObject.ConfigAttribute : null;
            if (attrib != null && attrib.TargetType != null) return attrib.TargetType;

            var targ = EditorHelper.GetTargetObjectOfProperty(prop) as TriggerableTargetObject;
            return targ != null ? targ.GetTargetType() : typeof(UnityEngine.Object);
        }

        public static object GetTarget(SerializedProperty prop, System.Type tp, object triggerArg = null)
        {
            var targ = EditorHelper.GetTargetObjectOfProperty(prop) as TriggerableTargetObject;
            return targ != null ? targ.GetTarget(tp, triggerArg) : null;
        }

        #endregion

    }

}
