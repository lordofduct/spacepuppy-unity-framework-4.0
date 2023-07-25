using UnityEngine;

using com.spacepuppy.Events;
using com.spacepuppy.Tween;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween.Events
{

    public class i_ScrubTween : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        private VariantReference _scrubTime = new VariantReference(VariantReference.RefMode.Property);

        [SerializeField]
        [SelectableObject]
        private UnityEngine.Object _target;

        [SerializeReference]
        private GenericTweenData[] _data;

        [System.NonSerialized]
        private ObjectTweener _cachedTweener;

        #endregion

        #region Methods

        private ObjectTweener GetCachedTweener(object arg)
        {
            if (!_target) return null;

            var targ = _target.IsProxy_ParamsRespecting() ? (_target as IProxy).GetTarget_ParamsRespecting(arg) : _target;
            if (_cachedTweener != null && object.ReferenceEquals(_cachedTweener.Target, targ)) return _cachedTweener;

            var twn = SPTween.Tween(targ);
            for (int i = 0; i < _data.Length; i++)
            {
                if (_data[i] != null) _data[i].Apply(twn);
            }
            _cachedTweener = twn.Create() as ObjectTweener;
            return _cachedTweener;
        }

        #endregion

        #region ITriggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var twn = this.GetCachedTweener(arg);
            twn?.Reset();
            twn?.Scrub(_scrubTime.FloatValue);
            return true;
        }

        #endregion

        #region Special Types

        public interface ITweenData
        {
            void Apply(TweenHash hash);
        }

        [System.Serializable()]
        public class GenericTweenData : ITweenData
        {
            [SerializeField()]
            public string MemberName;
            [SerializeReference()]
            public EaseSelector Ease;
            [SerializeField()]
            public VariantReference ValueS;
            [SerializeField()]
            public VariantReference ValueE;
            [SerializeField()]
            [TimeUnitsSelector()]
            public float Duration;
            [SerializeField]
            public int Option;

            public void Apply(TweenHash hash)
            {
                hash.ByAnimMode(TweenHash.AnimMode.FromTo, this.MemberName, this.Ease?.GetEase(), this.Duration, this.ValueS.Value, this.ValueE.Value, this.Option);
            }

        }

        #endregion

    }

}
