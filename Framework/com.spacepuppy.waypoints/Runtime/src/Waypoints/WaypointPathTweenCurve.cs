using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Dynamic;
using com.spacepuppy.Tween;
using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

namespace com.spacepuppy.Waypoints
{

    /// <summary>
    /// Used to translate a target Vector3 property of any object with name 'propName' along a IWaypointPath.
    /// </summary>
    public class WaypointPathTweenCurve : MemberCurve<Vector3>
    {

        #region Fields

        private IWaypointPath _path;
        private float _redact;

        #endregion

        #region CONSTRUCTOR

        protected internal WaypointPathTweenCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor)
            : base(accessor)
        {

        }

        public WaypointPathTweenCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, IWaypointPath path)
            : base(accessor, null, dur)
        {
            _path = path;
        }

        public WaypointPathTweenCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, IWaypointPath path)
            : base(accessor, ease, dur)
        {
            _path = path;
        }

        #endregion

        #region Properties

        public IWaypointPath Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public float ArcLengthRedaction
        {
            get { return _redact; }
            set { _redact = value; }
        }

        #endregion

        #region MemberCurve Interface

        protected override void Configure(Ease ease, float dur, Vector3 start, Vector3 end, int option = 0)
        {
            //do nothing
        }

        protected override void ConfigureAsRedirectTo(Ease ease, float totalDur, Vector3 current, Vector3 start, Vector3 end, int option = 0)
        {
            //do nothing
        }

        protected override Vector3 GetValueAt(float dt, float t)
        {
            if (_path == null) return Vector3.zero;

            t = this.Ease(t, 0f, 1f, this.Duration);
            if (_redact != 0f)
            {
                float rt = _redact / _path.GetArcLength();
                t -= rt;
            }
            return _path.GetPositionAt(t);
        }

        #endregion

    }


    /// <summary>
    /// Used to update a target across a WaypointPathComponent. This will update the Transform position and rotation, as well as attempt any modifier 
    /// property updates based on modifiers attached to the waypoint.
    /// </summary>
    public class AdvancedWaypointPathTweenCurve : TweenCurve
    {
        
        public enum TranslationOptions
        {
            None = 0,
            Position = 1,
            LocalPosition = 2
        }

        public enum RotationOptions
        {
            None = 0,
            Rotation = 1,
            LocalRotation = 2,
            Heading = 3,
            LocalHeading = 4
        }

        #region Fields

        private Ease _ease;
        private float _dur;
        private float _delay;

        private WaypointPathComponent _path;

        private TranslationOptions _updateTranslation;
        private RotationOptions _updateRotation;

        private IStateModifier[][] _modifiers;

        #endregion

        #region CONSTRUCTOR

        public AdvancedWaypointPathTweenCurve(float dur, WaypointPathComponent path)
            : base()
        {
            _ease = EaseMethods.Linear;
            _dur = dur;
            _delay = 0f;
            _path = path;
        }

        public AdvancedWaypointPathTweenCurve(Ease ease, float dur, WaypointPathComponent path)
            : base()
        {
            _ease = ease ?? EaseMethods.Linear;
            _dur = dur;
            _path = path;
            _delay = 0f;
        }

        public AdvancedWaypointPathTweenCurve(float dur, float delay, WaypointPathComponent path)
            : base()
        {
            _ease = EaseMethods.Linear;
            _dur = dur;
            _path = path;
            _delay = delay;
        }

        public AdvancedWaypointPathTweenCurve(Ease ease, float dur, float delay, WaypointPathComponent path)
            : base()
        {
            _ease = ease ?? EaseMethods.Linear;
            _dur = dur;
            _path = path;
            _delay = delay;
        }

        #endregion

        #region Properties

        public float Duration
        {
            get { return _dur; }
            set { _dur = value; }
        }

        public float Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        public Ease Ease
        {
            get { return _ease; }
            set { _ease = value ?? EaseMethods.Linear; }
        }

        public WaypointPathComponent Path
        {
            get { return _path; }
        }


        public TranslationOptions UpdateTranslation
        {
            get { return _updateTranslation; }
            set { _updateTranslation = value; }
        }

        public RotationOptions UpdateRotation
        {
            get { return _updateRotation; }
            set { _updateRotation = value; }
        }

        public bool IgnoreModifiers
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public void SetToTarget(object targ, float t)
        {
            this.LerpToTarget(targ, t, 1f);
        }

        public void LerpToTarget(object targ, float t, float lerpT)
        {
            if (_dur == 0f)
                t = 1f;
            else
                t = t / _dur;

            if (_updateTranslation > TranslationOptions.None || _updateRotation >= RotationOptions.Heading)
            {
                var trans = GameObjectUtil.GetTransformFromSource(targ);
                if (trans != null)
                {
                    if (_updateRotation == RotationOptions.Heading)
                    {
                        var wp = _path.Path.GetWaypointAt(t);
                        this.SetPosition(trans, wp.Position);
                        this.SetRotation(trans, Quaternion.LookRotation(wp.Heading), lerpT);
                    }
                    else if (_updateTranslation > TranslationOptions.None)
                    {
                        this.SetPosition(trans, _path.Path.GetPositionAt(t), lerpT);
                    }
                }
            }

            if (_modifiers == null) this.SyncModifiers();
            if (_modifiers.Length > 0 || _updateRotation == RotationOptions.Rotation || _updateRotation == RotationOptions.LocalRotation)
            {
                var data = _path.Path.GetRelativePositionData(t);

                var cnt = _path.Path.Count;
                int i = data.Index;
                int j = (_path.Path.IsClosed) ? (i + 1) % cnt : i + 1;

                if (_updateRotation == RotationOptions.Rotation || _updateRotation == RotationOptions.LocalRotation)
                {
                    var trans = GameObjectUtil.GetTransformFromSource(targ);
                    if (trans != null)
                    {
                        var a = (i >= 0 && i < cnt) ? GameObjectUtil.GetTransformFromSource(_path.Path.ControlPoint(i)) : null;
                        var b = (j >= 0 && j < cnt) ? GameObjectUtil.GetTransformFromSource(_path.Path.ControlPoint(j)) : null;

                        if (a != null)
                        {
                            bool useRelative = _path.TransformRelativeTo != null;
                            var r = (useRelative) ? a.GetRelativeRotation(_path.TransformRelativeTo) : a.rotation;
                            if (b != null)
                            {
                                var rb = (useRelative) ? b.GetRelativeRotation(_path.TransformRelativeTo) : b.rotation;
                                r = Quaternion.LerpUnclamped(r, rb, data.TPrime);
                            }
                            this.SetRotation(trans, r, lerpT);
                        }
                    }
                }

                if (_modifiers.Length > 0)
                {
                    var ma = (i >= 0 && i < _modifiers.Length) ? _modifiers[i] : null;
                    var mb = (j >= 0 && j < _modifiers.Length) ? _modifiers[j] : null;

                    if(ma != null && mb != null)
                    {
                        foreach(var m in ma)
                        {
                            m.CopyTo(targ);
                        }
                        foreach (var m in mb)
                        {
                            m.LerpTo(targ, data.TPrime);
                        }
                    }
                    else if(ma != null)
                    {
                        foreach (var m in ma)
                        {
                            m.CopyTo(targ);
                        }
                    }
                    else if(mb != null)
                    {
                        foreach (var m in mb)
                        {
                            m.CopyTo(targ);
                        }
                    }
                }
            }
        }

        private void SetPosition(Transform trans, Vector3 pos, float lerpT = 1f)
        {
            if (lerpT != 1f)
            {
                switch (_updateTranslation)
                {
                    case TranslationOptions.Position:
                        trans.position = Vector3.LerpUnclamped(trans.position, pos, lerpT);
                        break;
                    case TranslationOptions.LocalPosition:
                        trans.localPosition = Vector3.LerpUnclamped(trans.localPosition, pos, lerpT);
                        break;
                }
            }
            else
            {
                switch (_updateTranslation)
                {
                    case TranslationOptions.Position:
                        trans.position = pos;
                        break;
                    case TranslationOptions.LocalPosition:
                        trans.localPosition = pos;
                        break;
                }
            }
        }

        private void SetRotation(Transform trans, Quaternion rot, float lerpT = 1f)
        {
            if (lerpT != 1f)
            {
                switch (_updateRotation)
                {
                    case RotationOptions.Rotation:
                    case RotationOptions.Heading:
                        trans.rotation = Quaternion.LerpUnclamped(trans.rotation, rot, lerpT);
                        break;
                    case RotationOptions.LocalRotation:
                    case RotationOptions.LocalHeading:
                        trans.localRotation = Quaternion.LerpUnclamped(trans.localRotation, rot, lerpT);
                        break;
                }
            }
            else
            {
                switch (_updateRotation)
                {
                    case RotationOptions.Rotation:
                    case RotationOptions.Heading:
                        trans.rotation = rot;
                        break;
                    case RotationOptions.LocalRotation:
                    case RotationOptions.LocalHeading:
                        trans.localRotation = rot;
                        break;
                }
            }
        }

        private void SyncModifiers()
        {
            var path = _path.Path;
            int cnt = path.Count;

            _modifiers = null;
            for(int i = 0; i < cnt; i++)
            {
                var p = GameObjectUtil.GetGameObjectFromSource(path.ControlPoint(i));
                if (p)
                {
                    using (var lst = TempCollection.GetList<IStateModifier>())
                    {
                        p.GetComponents<IStateModifier>(lst);
                        if(lst.Count > 0)
                        {
                            if (_modifiers == null) _modifiers = new IStateModifier[cnt][];
                            _modifiers[i] = lst.ToArray();
                        }
                    }
                }
            }

            if (_modifiers == null) _modifiers = ArrayUtil.Empty<IStateModifier[]>();
        }

        #endregion

        #region TweenCurve Interface

        public override float TotalTime
        {
            get
            {
                return _dur + _delay;
            }
        }

        public override void Update(object targ, float dt, float t)
        {
            if (_path == null || targ == null) return;

            t -= _delay;
            t = this.Ease(t, 0f, 1f, this.Duration) * this.Duration;

            this.SetToTarget(targ, t);
        }

        #endregion

    }

}
