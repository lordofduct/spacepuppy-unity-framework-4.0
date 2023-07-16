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
    /// A serializable IProxy struct that will search the scene for an object by name/tag/type.
    /// </summary>
    [System.Serializable]
    public struct QueryProxy : IProxy
    {

        #region Fields

        [SerializeField()]
        [RespectsIProxy()]
        private UnityEngine.Object _target;
        [SerializeField()]
        private SearchBy _searchBy;
        [SerializeField()]
        private string _queryString;

        #endregion

        #region CONSTRUCTOR

        public QueryProxy(UnityEngine.Object target)
        {
            _target = target;
            _searchBy = SearchBy.Nothing;
            _queryString = null;
        }

        public QueryProxy(SearchBy searchBy)
        {
            _target = null;
            _searchBy = searchBy;
            _queryString = null;
        }

        public QueryProxy(SearchBy searchBy, string query)
        {
            _target = null;
            _searchBy = searchBy;
            _queryString = query;
        }

        #endregion

        #region Properties

        public UnityEngine.Object Target
        {
            get { return _target; }
            set { _target = value; }
        }

        public SearchBy SearchBy
        {
            get { return _searchBy; }
            set { _searchBy = value; }
        }

        public string SearchByQuery
        {
            get { return _queryString; }
            set { _queryString = value; }
        }

        #endregion

        #region Methods

        public object GetTarget()
        {
            if (_searchBy == SearchBy.Nothing)
            {
                return (_target is IProxy) ? _target.ReduceIfProxy() : _target;
            }
            else
            {
                return ObjUtil.Find(_searchBy, _queryString);
            }
        }

        public object[] GetTargets()
        {
            if (_searchBy == SearchBy.Nothing)
            {
                return new object[] { (_target is IProxy) ? _target.ReduceIfProxy() : _target };
            }
            else
            {
                return ObjUtil.FindAll(_searchBy, _queryString);
            }
        }

        public T GetTarget<T>() where T : class
        {
            if (_searchBy == SearchBy.Nothing)
            {
                return ObjUtil.GetAsFromSource<T>(_target, true);
            }
            else
            {
                return ObjUtil.Find<T>(_searchBy, _queryString);
            }
        }

        public T[] GetTargets<T>() where T : class
        {
            if (_searchBy == SearchBy.Nothing)
            {
                var targ = ObjUtil.GetAsFromSource<T>(_target, true);
                return targ != null ? new T[] { targ } : ArrayUtil.Empty<T>();
            }
            else
            {
                return ObjUtil.FindAll<T>(_searchBy, _queryString);
            }
        }

        #endregion

        #region IProxy Interface

        ProxyParams IProxy.Params => _searchBy > SearchBy.Nothing ? ProxyParams.QueriesTarget : ProxyParams.None;

        object IProxy.GetTargetInternal(System.Type expectedType, object arg)
        {
            return this.GetTarget();
        }

        public System.Type GetTargetType()
        {
            if (_target == null) return typeof(object);
            return (_target is IProxy) ? (_target as IProxy).GetTargetType() : _target.GetType();
        }

        #endregion

        #region Special Types

        public class ConfigAttribute : System.Attribute
        {

            public System.Type TargetType;
            public bool AllowProxy = true;

            public ConfigAttribute()
            {
                this.TargetType = typeof(GameObject);
            }

            public ConfigAttribute(System.Type targetType)
            {
                //if (targetType == null || 
                //    (!TypeUtil.IsType(targetType, typeof(UnityEngine.Object)) && !TypeUtil.IsType(targetType, typeof(IComponent)))) throw new TypeArgumentMismatchException(targetType, typeof(UnityEngine.Object), "targetType");
                if (targetType == null ||
                    (!TypeUtil.IsType(targetType, typeof(UnityEngine.Object)) && !targetType.IsInterface))
                    throw new TypeArgumentMismatchException(targetType, typeof(UnityEngine.Object), "targetType");

                this.TargetType = targetType;
            }

        }

        #endregion

    }

    [CreateAssetMenu(fileName = "QueryProxy", menuName = "Spacepuppy/Proxy/QueryProxy")]
    public class QueryProxyToken : ScriptableObject, IProxy
    {

        #region Fields

        [SerializeField]
        private TriggerableTargetObject _target = new TriggerableTargetObject(TriggerableTargetObject.FindCommand.FindInScene, SearchBy.Nothing, string.Empty);
        [SerializeField]
        [TypeReference.Config(typeof(Component), allowAbstractClasses = true, allowInterfaces = true)]
        private TypeReference _componentTypeOnTarget;

        [Space()]
        [SerializeField]
        [Tooltip("Cache the target when it's first retrieved. This is useful for speeding up any 'Find' commands if called repeatedly, but is hindered if the target is changing.")]
        private bool _cache;
        [System.NonSerialized]
        private UnityEngine.Object _object;

        #endregion

        #region IProxy Interface

        ProxyParams IProxy.Params => _target.ImplicityReducesEntireEntity ? ProxyParams.QueriesTarget : ProxyParams.None;

        object IProxy.GetTargetInternal(System.Type expectedType, object arg)
        {
            if (_cache)
            {
                if (_object != null) return _object;

                _object = _target.GetTarget(_componentTypeOnTarget.Type ?? typeof(UnityEngine.Object), arg) as UnityEngine.Object;
                return _object;
            }
            else
            {
                return _target.GetTarget(_componentTypeOnTarget.Type ?? typeof(UnityEngine.Object), arg) as UnityEngine.Object;
            }
        }

        public System.Type GetTargetType()
        {
            if (_componentTypeOnTarget.Type != null) return _componentTypeOnTarget.Type;
            return (_cache && _object != null) ? _object.GetType() : typeof(UnityEngine.Object);
        }

        #endregion

    }

}