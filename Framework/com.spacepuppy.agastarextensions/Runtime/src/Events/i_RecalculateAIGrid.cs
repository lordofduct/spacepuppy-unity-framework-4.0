using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

using Pathfinding;
using com.spacepuppy.Pathfinding;

namespace com.spacepuppy.Events
{

    /// <summary>
    /// Bounds related recalc modes are AABB's because AronGranberg A* only accepts AABB's for recalc regions.
    /// </summary>
    public class i_RecalculateAIGrid : AutoTriggerable
    {

        public enum RecalculateMode
        {
            AllAsync = -1,
            All = 0,
            [System.ComponentModel.Description("Local Scale AABB")]
            LocalScaleAABB = 1, //rotation is ignored
            BoundsOfCollider = 2,
            [System.ComponentModel.Description("Lossy Scale AABB")]
            LossyScaleAABB, //rotation is ignored
        }

        #region Fields

        [SerializeField]
        private RecalculateMode _mode;
        [SerializeField]
        private UnityEngine.Object _objectRef;

        [SerializeField]
        private float _delay;

        #endregion

        #region Properties

        public RecalculateMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public Collider Collider
        {
            get { return _objectRef as Collider; }
            set { _objectRef = value; }
        }

        #endregion

        #region Methods

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            switch (_mode)
            {
                case RecalculateMode.AllAsync:
                    {
                        if (_delay > 0)
                        {
                            this.InvokeGuaranteed(() => AstarPath.active?.BeginScanAsync(), _delay);
                        }
                        else
                        {
                            AstarPath.active?.BeginScanAsync();
                        }
                        return true;
                    }
                case RecalculateMode.All:
                    {
                        if (_delay > 0f)
                        {
                            this.InvokeGuaranteed(() => AstarPath.active?.TryScan(), _delay);
                        }
                        else
                        {
                            AstarPath.active?.TryScan();
                        }
                        return true;
                    }
                case RecalculateMode.LocalScaleAABB:
                    {
                        var bounds = new Bounds(this.transform.position, this.transform.localScale);
                        var guo = new GraphUpdateObject(bounds)
                        {
                            updatePhysics = true
                        };
                        if (_delay > 0f)
                        {
                            this.InvokeGuaranteed(() => AstarPath.active?.UpdateGraphs(guo), _delay);
                        }
                        else
                        {
                            AstarPath.active?.UpdateGraphs(guo);
                        }
                    }
                    break;
                case RecalculateMode.BoundsOfCollider:
                    {
                        if (_objectRef == null || !(_objectRef is Collider)) return false;
                        var bounds = (_objectRef as Collider).bounds;
                        var guo = new GraphUpdateObject(bounds)
                        {
                            updatePhysics = true
                        };
                        if (_delay > 0f)
                        {
                            this.InvokeGuaranteed(() => AstarPath.active?.UpdateGraphs(guo), _delay);
                        }
                        else
                        {
                            AstarPath.active?.UpdateGraphs(guo);
                        }
                    }
                    break;
                case RecalculateMode.LossyScaleAABB:
                    {
                        var bounds = new Bounds(this.transform.position, this.transform.lossyScale);
                        var guo = new GraphUpdateObject(bounds)
                        {
                            updatePhysics = true
                        };
                        if (_delay > 0f)
                        {
                            this.InvokeGuaranteed(() => AstarPath.active?.UpdateGraphs(guo), _delay);
                        }
                        else
                        {
                            AstarPath.active?.UpdateGraphs(guo);
                        }
                    }
                    break;
            }

            return false;
        }

        #endregion

    }

}