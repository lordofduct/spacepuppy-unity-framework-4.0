#pragma warning disable 0649 // variable declared but not used.
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Dynamic;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    /// <summary>
    /// A serializable IProxy struct that will access a target's member/property for an object.
    /// </summary>
    [System.Serializable]
    public struct MemberProxy : IProxy
    {

        #region Fields

        [SerializeField()]
        [SelectableObject]
        private UnityEngine.Object _target;
        [SerializeField()]
        private string _memberName;

        #endregion

        #region Properties

        public UnityEngine.Object Target
        {
            get { return _target; }
            set { _target = value; }
        }

        public string MemberName
        {
            get { return _memberName; }
            set { _memberName = value; }
        }

        public object Value
        {
            get { return this.GetValue(); }
            set { this.SetValue(value); }
        }

        #endregion

        #region Methods

        public object GetValue()
        {
            if (_target == null) return null;

            var obj = ObjUtil.ReduceIfProxy(_target);
            if (obj == null)
                return null;
            else
                return DynamicUtil.GetValue(obj, _memberName);
        }

        public T GetValue<T>()
        {
            if (_target == null) return default(T);

            var obj = ObjUtil.ReduceIfProxy(_target);
            if (obj == null)
            {
                return default(T);
            }
            else
            {
                var result = DynamicUtil.GetValue(_target, _memberName);
                if (result is T)
                    return (T)result;
                else if (ConvertUtil.IsSupportedType(typeof(T)))
                    return ConvertUtil.ToPrim<T>(result);
                else
                    return default(T);
            }
        }

        public bool SetValue(object value)
        {
            if (_target == null) return false;

            var obj = ObjUtil.ReduceIfProxy(_target);
            if (obj == null)
                return false;
            else
                return DynamicUtil.SetValue(_target, _memberName, value);
        }

        #endregion

        #region IProxy Interface

        bool IProxy.QueriesTarget
        {
            get { return false; }
        }

        object IProxy.GetTarget()
        {
            return this.GetValue();
        }

        object IProxy.GetTarget(object arg)
        {
            return this.GetValue();
        }

        object IProxy.GetTargetAs(System.Type tp)
        {
            return ObjUtil.GetAsFromSource(tp, this.GetValue());
        }

        object IProxy.GetTargetAs(System.Type tp, object arg)
        {
            return ObjUtil.GetAsFromSource(tp, this.GetValue());
        }


        public System.Type GetTargetType()
        {
            if (_memberName == null) return typeof(object);

            if (_target is IProxy)
            {
                var tp = (_target as IProxy).GetTargetType();
                return DynamicUtil.GetReturnType(DynamicUtil.GetMemberFromType(tp, _memberName, false)) ?? typeof(object);
            }
            else
            {
                return DynamicUtil.GetReturnType(DynamicUtil.GetMember(ObjUtil.ReduceIfProxy(_target), _memberName, false)) ?? typeof(object);
            }
        }

        #endregion

        #region Config Attrib

        public class ConfigAttribute : System.Attribute
        {
            public DynamicMemberAccess MemberAccessLevel;

            public ConfigAttribute(DynamicMemberAccess memberAccessLevel)
            {
                this.MemberAccessLevel = memberAccessLevel;
            }

        }

        #endregion

    }

    [CreateAssetMenu(fileName = "MemberProxy", menuName = "Spacepuppy/Proxy/MemberProxy")]
    public class MemberProxyToken : ScriptableObject, IProxy
    {

        #region Fields

        [SerializeField]
        private MemberProxy _target;

        #endregion

        #region Properties

        public UnityEngine.Object Target
        {
            get { return _target.Target; }
            set { _target.Target = value; }
        }

        public string MemberName
        {
            get { return _target.MemberName; }
            set { _target.MemberName = value; }
        }

        public object Value
        {
            get { return _target.GetValue(); }
            set { _target.SetValue(value); }
        }

        #endregion

        #region Methods

        public object GetValue()
        {
            return _target.GetValue();
        }

        public T GetValue<T>()
        {
            return _target.GetValue<T>();
        }

        public bool SetValue(object value)
        {
            return _target.SetValue(value);
        }

        #endregion

        #region IProxy Interface

        bool IProxy.QueriesTarget
        {
            get { return false; }
        }

        object IProxy.GetTarget()
        {
            return _target.GetValue();
        }

        object IProxy.GetTarget(object arg)
        {
            return _target.GetValue();
        }

        object IProxy.GetTargetAs(System.Type tp)
        {
            return ObjUtil.GetAsFromSource(tp, _target.GetValue());
        }

        object IProxy.GetTargetAs(System.Type tp, object arg)
        {
            return ObjUtil.GetAsFromSource(tp, _target.GetValue());
        }

        System.Type IProxy.GetTargetType()
        {
            return _target.GetTargetType();
        }

        #endregion

    }

}