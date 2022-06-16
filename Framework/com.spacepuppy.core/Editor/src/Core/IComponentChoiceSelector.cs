using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

namespace com.spacepuppyeditor.Core
{
    public interface IComponentChoiceSelector
    {

        void BeforeGUI(SelectableComponentPropertyDrawer drawer, SerializedProperty property, System.Type restrictionType, bool allowProxy);

        Component[] GetComponents();
        GUIContent[] GetPopupEntries();
        int GetPopupIndexOfComponent(Component comp);
        Component GetComponentAtPopupIndex(int index);

        void GUIComplete(SerializedProperty property, int selectedIndex);

    }

    public class DefaultComponentChoiceSelector : IComponentChoiceSelector
    {

        public static readonly DefaultComponentChoiceSelector Default = new DefaultComponentChoiceSelector();

        private SelectableComponentPropertyDrawer _drawer;
        private SerializedProperty _property;
        private System.Type _restrictionType;
        private bool _allowProxy;
        private Component[] _components;

        public SelectableComponentPropertyDrawer Drawer { get { return _drawer; } }
        public SerializedProperty Property { get { return _property; } }
        public System.Type RestrictionType { get { return _restrictionType; } }
        public bool AllowProxy { get { return _allowProxy; } }
        public Component[] Components { get { return _components; } }



        protected virtual Component[] DoGetComponents()
        {
            return GetComponentsFromSerializedProperty(_property, _restrictionType, _drawer.ForceOnlySelf, _drawer.SearchChildren, _allowProxy);
        }

        protected virtual void OnBeforeGUI()
        {

        }

        protected virtual void OnGUIComplete(int selectedIndex)
        {

        }

        #region IComponentChoiceSelector Interface

        void IComponentChoiceSelector.BeforeGUI(SelectableComponentPropertyDrawer drawer, SerializedProperty property, System.Type restrictionType, bool allowProxy)
        {
            _drawer = drawer;
            _property = property;
            _restrictionType = restrictionType;
            _allowProxy = allowProxy;
            _components = this.DoGetComponents();
            this.OnBeforeGUI();
        }

        Component[] IComponentChoiceSelector.GetComponents()
        {
            return _components;
        }

        public virtual GUIContent[] GetPopupEntries()
        {
            //return (from c in components select new GUIContent(c.GetType().Name)).ToArray();
            using (var lst = com.spacepuppy.Collections.TempCollection.GetList<GUIContent>())
            {
                lst.Add(new GUIContent("Nothing..."));
                if (_drawer.SearchChildren)
                {
                    lst.AddRange(from s in DefaultComponentChoiceSelector.GetUniqueComponentNamesWithOwner(_components) select new GUIContent(s));
                }
                else
                {
                    lst.AddRange(from s in DefaultComponentChoiceSelector.GetUniqueComponentNames(_components) select new GUIContent(s));
                }
                return lst.ToArray();
            }
        }

        public virtual int GetPopupIndexOfComponent(Component comp)
        {
            if (_components == null) return -1;
            return _components.IndexOf(comp) + 1; //adjust for Nothing...
        }

        public virtual Component GetComponentAtPopupIndex(int index)
        {
            if (_components == null) return null;
            if (index == 0) return null;
            index--; //adjust for Nothing...
            if (index < 0 || index >= _components.Length) return null;
            return _components[index];
        }

        void IComponentChoiceSelector.GUIComplete(SerializedProperty property, int selectedIndex)
        {
            this.OnGUIComplete(selectedIndex);
            this.Reset();
        }

        public virtual void Reset()
        {
            _drawer = null;
            _property = null;
            _restrictionType = null;
            _allowProxy = false;
            _components = null;
        }

        #endregion



        public static GameObject GetGameObjectFromSource(SerializedProperty property, bool forceSelfOnly)
        {
            if (forceSelfOnly)
                return GameObjectUtil.GetGameObjectFromSource(property.serializedObject.targetObject);
            else
                return GameObjectUtil.GetGameObjectFromSource(property.objectReferenceValue);
        }

        public static Component[] GetComponentsFromSerializedProperty(SerializedProperty property, System.Type restrictionType, bool forceSelfOnly, bool searchChildren, bool allowProxy)
        {
            var go = GetGameObjectFromSource(property, forceSelfOnly);
            if (go == null) return ArrayUtil.Empty<Component>();

            if (allowProxy || !ComponentUtil.IsAcceptableComponentType(restrictionType))
            {
                var e = searchChildren ? go.GetComponentsInChildren<Component>() : go.GetComponents<Component>();
                return e.Where(c => ObjUtil.IsType(c, restrictionType, allowProxy)).ToArray();
            }
            else
            {
                return searchChildren ? go.GetComponentsInChildren(restrictionType) : go.GetComponents(restrictionType);
            }
        }

        public static IEnumerable<string> GetUniqueComponentNames(Component[] components)
        {
            using (var uniqueCount = TempCollection.GetDict<System.Type, int>())
            {
                for (int i = 0; i < components.Length; i++)
                {
                    var tp = components[i].GetType();
                    int cnt;
                    if (uniqueCount.TryGetValue(tp, out cnt))
                        cnt++;
                    else
                        cnt = 1;
                    uniqueCount[tp] = cnt;

                    if (components[i] is IIdentifiableComponent ic && !string.IsNullOrEmpty(ic.Id))
                    {
                        yield return string.Format("{0} ({1} : {2})", components[i].gameObject.name, tp.Name, ic.Id);
                    }
                    else if (cnt > 1)
                    {
                        yield return string.Format("{0} ({1} {2})", components[i].gameObject.name, tp.Name, cnt);
                    }
                    else
                    {
                        yield return string.Format("{0} ({1})", components[i].gameObject.name, tp.Name);
                    }

                }
            }
        }

        public static IEnumerable<string> GetUniqueComponentNamesWithOwner(Component[] components)
        {
            using (var uniqueCount = TempCollection.GetDict<System.Type, int>())
            {
                for (int i = 0; i < components.Length; i++)
                {
                    //TODO - maybe come up with a better naming scheme for this
                    var tp = components[i].GetType();
                    int cnt;
                    if (uniqueCount.TryGetValue(tp, out cnt))
                        cnt++;
                    else
                        cnt = 1;
                    uniqueCount[tp] = cnt;

                    if (components[i] is IIdentifiableComponent ic && !string.IsNullOrEmpty(ic.Id))
                    {
                        yield return components[i].gameObject.name + "/" + tp.Name + " : " + ic.Id;
                    }
                    else if (cnt > 1)
                    {
                        yield return components[i].gameObject.name + "/" + tp.Name + " " + cnt.ToString();
                    }
                    else
                    {
                        yield return components[i].gameObject.name + "/" + tp.Name;
                    }
                }
            }
        }

    }

    public class MultiTypeComponentChoiceSelector : DefaultComponentChoiceSelector
    {

        public System.Type[] AllowedTypes;

        protected override Component[] DoGetComponents()
        {
            return GetComponentsFromSerializedProperty(this.Property, this.AllowedTypes, this.RestrictionType, this.Drawer.ForceOnlySelf, this.Drawer.SearchChildren, this.AllowProxy);
        }

        public static Component[] GetComponentsFromSerializedProperty(SerializedProperty property, System.Type[] allowedTypes, System.Type restrictionType, bool forceSelfOnly, bool searchChildren, bool allowProxy)
        {
            if (allowedTypes == null || allowedTypes.Length == 0) return ArrayUtil.Empty<Component>();

            var go = DefaultComponentChoiceSelector.GetGameObjectFromSource(property, forceSelfOnly);
            if (go == null) return ArrayUtil.Empty<Component>();

            using (var set = com.spacepuppy.Collections.TempCollection.GetSet<Component>())
            {
                if (searchChildren)
                {
                    foreach (var c in go.GetComponentsInChildren<Component>())
                    {
                        if (!ObjUtil.IsType(c, restrictionType, allowProxy)) continue;
                        foreach (var tp in allowedTypes)
                        {
                            if (ObjUtil.IsType(c, tp, allowProxy)) set.Add(c);
                        }
                    }
                }
                else
                {
                    foreach (var c in go.GetComponents<Component>())
                    {
                        if (!ObjUtil.IsType(c, restrictionType, allowProxy)) continue;
                        foreach (var tp in allowedTypes)
                        {
                            if (ObjUtil.IsType(c, tp, allowProxy)) set.Add(c);
                        }
                    }
                }

                return (from c in set orderby c.GetType().Name select c).ToArray();
            }
        }

    }

}
