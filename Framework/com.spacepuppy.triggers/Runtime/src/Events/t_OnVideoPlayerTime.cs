using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{
    public class t_OnVideoPlayerTime : SPComponent, IObservableTrigger, IUpdateable, IMStartOrEnableReceiver
    {

        const double START_OFFSET = 0.00001d;

        #region Fields

        [SerializeField, DefaultFromSelf]
        private VideoPlayer _videoPlayer;

        [SerializeField, Tooltip("Use negative values to represent time before the end of the video.")]
        private float _time;

        [SerializeField, SPEvent.Config("player (VideoPlayer)")]
        private SPEvent _onTrigger = new SPEvent("OnTrigger");

        [System.NonSerialized]
        private double _lastCheckedTime;
        [System.NonSerialized]
        private VideoPlayer.EventHandler _evhook_Start;
        private VideoPlayer.EventHandler VideoPlayerStartedCallback => _evhook_Start ?? (_evhook_Start = _videoPlayer_started);

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            this.Initialize();
        }

        protected override void OnDisable()
        {
            this.Deinitialize();
            base.OnDisable();
        }

        #endregion

        #region Properties

        public VideoPlayer VideoPlayer
        {
            get => _videoPlayer;
            set
            {
                if (_videoPlayer == value) return;

                if (_videoPlayer && this.isActiveAndEnabled)
                {
                    this.Deinitialize();
                }
                _videoPlayer = value;
                if (this.isActiveAndEnabled)
                {
                    this.Initialize();
                }
            }
        }

        public SPEvent OnTrigger => _onTrigger;

        #endregion

        #region Methods

        void Initialize()
        {
            if (_videoPlayer)
            {
                _lastCheckedTime = _videoPlayer.time - START_OFFSET;
                _videoPlayer.started -= this.VideoPlayerStartedCallback;
                _videoPlayer.started += this.VideoPlayerStartedCallback;
                if (_videoPlayer.isPlaying)
                {
                    GameLoop.UpdatePump.Add(this);
                }
            }
            else
            {
                this.Deinitialize();
            }
        }

        void Deinitialize()
        {
            _lastCheckedTime = default;
            GameLoop.UpdatePump.Remove(this);
            if (_videoPlayer)
            {
                _videoPlayer.started -= this.VideoPlayerStartedCallback;
            }
        }

        private void _videoPlayer_started(VideoPlayer source)
        {
            if (source != _videoPlayer || !this.isActiveAndEnabled)
            {
                source.started -= this.VideoPlayerStartedCallback;
                return;
            }

            if (_videoPlayer)
            {
                _lastCheckedTime = _videoPlayer.time - START_OFFSET;
                GameLoop.UpdatePump.Add(this);
            }
        }

        void IUpdateable.Update()
        {
            if (!_videoPlayer)
            {
                this.Deinitialize();
                return;
            }

            double t = _videoPlayer.time;
            double flag = _time >= 0d ? _time : _videoPlayer.length + _time;
            if (t >= flag && _lastCheckedTime < flag)
            {
                _onTrigger.ActivateTrigger(this, _videoPlayer);
            }
            _lastCheckedTime = t;
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents() => new[] { _onTrigger };

        #endregion

    }
}
