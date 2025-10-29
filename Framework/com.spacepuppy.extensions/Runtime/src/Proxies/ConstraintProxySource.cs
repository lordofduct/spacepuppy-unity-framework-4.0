using UnityEngine;
using UnityEngine.Animations;
using System.Collections.Generic;

using com.spacepuppy.Project;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    [DefaultExecutionOrder(32000), RequireComponent(typeof(IConstraint))]
    public sealed class ConstraintProxySource : SPMonoBehaviour
    {

        #region Fields

        [SerializeField, ForceFromSelf]
        private InterfaceRef<IConstraint> _constraint = new();

        [SerializeField, ReorderableArray(HideElementLabel = true)]
        private ConstraintSourceTranslator[] _sources;

        #endregion

        #region CONSTRUCTOR

        private void Start()
        {
            var c = _constraint.Value;
            if (_sources?.Length > 0 && !c.IsNullOrDestroyed())
            {
                foreach (var s in _sources)
                {
                    var t = s.target.ReduceIfProxyAs(typeof(Transform)) as Transform;
                    if (t)
                    {
                        c.AddSource(new ConstraintSource()
                        {
                            sourceTransform = t,
                            weight = s.weight
                        });
                    }
                }
            }
        }

        #endregion

        #region Properties

        public IConstraint Constraint
        {
            get => _constraint.Value;
            set => _constraint.Value = value;
        }

        public ConstraintSourceTranslator[] Sources
        {
            get => _sources;
            set => _sources = value;
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public struct ConstraintSourceTranslator
        {

            [SerializeField, TypeRestriction(typeof(Transform), AllowProxy = true)]
            private UnityEngine.Object _target;
            public UnityEngine.Object target
            {
                get => _target;
                set => _target = value;
            }
            public float weight;
        }

        #endregion

    }

}
