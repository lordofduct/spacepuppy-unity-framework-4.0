using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Tween.Accessors;
using com.spacepuppy.Tween.Curves;
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic.Accessors;

namespace com.spacepuppy.Tween
{

    public abstract class TweenCurve
    {

        public static TweenCurve Null { get; } = new NullCurve();

        #region Fields

        private Tweener _tween;

        #endregion

        #region CONSTRUCTOR

        public TweenCurve()
        {

        }

        protected internal virtual void Init(Tweener twn)
        {
            if (_tween != null) throw new System.InvalidOperationException("Curve can only be registered with one Tweener at a time, and should not be doubly nested in any Curve collections.");
            _tween = twn;
        }

        protected virtual void DeInit()
        {
            _tween = null;
        }

        #endregion

        #region Properties

        public Tweener Tween { get { return _tween; } }

        #endregion

        #region Methods

        #endregion

        #region Curve Interface

        /// <summary>
        /// The duration of this curve from beginning to end, including any delays.
        /// </summary>
        public abstract float TotalTime { get; }

        /// <summary>
        /// Updates the targ in an appropriate manner, if the targ is of a type that can be updated by this curve.
        /// </summary>
        /// <param name="dt">The change in time since last update.</param>
        /// <param name="t">A value from 0 to TotalDuration representing the position the curve aught to be at.</param>
        public abstract void Update(object targ, float dt, float t);

        #endregion

        #region Special Types

        private class NullCurve : TweenCurve
        {

            protected internal override void Init(Tweener twn)
            {
                //don't init
            }

            public override float TotalTime
            {
                get { return 0f; }
            }

            public override void Update(object targ, float dt, float t)
            {
            }
        }

        #endregion

    }

}
