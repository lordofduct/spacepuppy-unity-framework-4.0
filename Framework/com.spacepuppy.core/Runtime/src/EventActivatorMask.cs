#pragma warning disable 0649 // variable declared but not used.
using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public interface IEventActivatorMask
    {

        bool Intersects(UnityEngine.Object obj);

    }

    [System.Serializable]
    public sealed class EventActivatorMaskRef : Project.SerializableInterfaceRef<IEventActivatorMask>
    {

    }

    [CreateAssetMenu(fileName = "EventActivatorMask", menuName = "Spacepuppy/EventActivatorMask")]
    public class EventActivatorMask : ScriptableObject, IEventActivatorMask
    {

        #region Fields

        [SerializeField]
        private bool _inverse;

        [SerializeReference]
        [ReorderableArray(DrawElementAtBottom = true, AlwaysExpanded = true, ElementLabelFormatString = "Filter {0:00}")]
        [SerializeRefPicker(typeof(IMode), AlwaysExpanded = true)]
        private IMode[] _filters;

        #endregion

        #region Properties

        public bool Inverse
        {
            get => _inverse;
            set => _inverse = value;
        }

        public IMode[] Filters
        {
            get => _filters;
            set => _filters = value;
        }

        #endregion

        #region IEventActivatorMask Interface

        public bool Intersects(Object obj)
        {
            if (_filters == null || _filters.Length == 0) return false;

            if (_inverse)
            {
                for (int i = 0; i < _filters.Length; i++)
                {
                    if (_filters[i]?.Intersects(obj) ?? true) return false;
                }
                return true;
            }
            else
            {
                for (int i = 0; i < _filters.Length; i++)
                {
                    if (!(_filters[i]?.Intersects(obj) ?? true)) return false;
                }
                return true;
            }
        }

        #endregion

        #region Special Types

        public interface IMode : IEventActivatorMask { }

        [System.Serializable]
        public class ByLayerMask : IMode
        {
            public bool testRoot;
            public LayerMask layerMask = -1;

            public bool Intersects(Object obj)
            {
                var go = GameObjectUtil.GetGameObjectFromSource(obj);
                return go && layerMask.Intersects(testRoot ? go.FindRoot() : go);
            }
        }

        [System.Serializable]
        public class ByTag : IMode
        {
            public bool testRoot;
            [ReorderableArray]
            [TagSelector()]
            public string[] tags;

            public bool Intersects(Object obj)
            {
                var go = GameObjectUtil.GetGameObjectFromSource(obj);
                if (!go) return false;
                if (testRoot) go = go.FindRoot();
                return tags?.Length > 0 && go.HasTag(tags);
            }
        }

        [System.Serializable]
        public class ByEvalStatement : IMode
        {
            public bool testRoot;
            public string evalStatement;

            public bool Intersects(Object obj)
            {
                var go = GameObjectUtil.GetGameObjectFromSource(obj);
                if (!go) return false;
                if (testRoot) go = go.FindRoot();
                return com.spacepuppy.Dynamic.Evaluator.EvalBool(evalStatement, go);
            }
        }

        #endregion

    }

}
