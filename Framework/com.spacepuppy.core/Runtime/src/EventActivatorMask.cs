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
    [InsertButton("Clear", "ClearMode", PrecedeProperty = true, SupportsMultiObjectEditing = true, RecordUndo = true, UndoLabel = "EventActivatorMask - Clear", Space = 20f)]
    public class EventActivatorMask : ScriptableObject, IEventActivatorMask
    {

        #region Fields

        [SerializeReference, SerializeRefPicker(typeof(IMode), AlwaysExpanded = true)]
        private IMode _mode;

        #endregion

        #region Properties

        public IMode Mode
        {
            get => _mode;
            set => _mode = value;
        }

        #endregion

        #region IEventActivatorMask Interface

        public bool Intersects(Object obj) => _mode?.Intersects(obj) ?? false;

        #endregion

#if UNITY_EDITOR
        private void ClearMode()
        {
            _mode = null;
        }
#endif

        #region Special Types

        public interface IMode : IEventActivatorMask { }

        [System.Serializable]
        public class Inverted : IMode
        {

            [SerializeReference, SerializeRefPicker(typeof(IMode), AlwaysExpanded = true)]
            public IMode mode;

            public bool Intersects(Object obj) => mode != null ? !mode.Intersects(obj) : true;

        }

        [System.Serializable]
        public class ManyFilters : IMode
        {

            [SerializeReference]
            [ReorderableArray(DrawElementAtBottom = true, AlwaysExpanded = true, ElementLabelFormatString = "Filter {0:00}")]
            [SerializeRefPicker(typeof(IMode), AlwaysExpanded = true)]
            public IMode[] filters;

            public bool Intersects(Object obj)
            {
                if (filters == null || filters.Length == 0) return false;

                for (int i = 0; i < filters.Length; i++)
                {
                    if (!(filters[i]?.Intersects(obj) ?? true)) return false;
                }
                return true;
            }

        }

        [System.Serializable]
        public class Nested : IMode
        {
            [SerializeField]
            private EventActivatorMaskRef _mask = new EventActivatorMaskRef();
            public IEventActivatorMask mask
            {
                get => _mask.Value;
                set => _mask.Value = value;
            }

            public bool Intersects(Object obj) => this.mask?.Intersects(obj) ?? false;

        }

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
