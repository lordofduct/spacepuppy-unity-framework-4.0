using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;

namespace com.spacepuppy.Events
{
    public class t_OnVideoPlayerCompleted : SPComponent, IObservableTrigger, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField, DefaultFromSelf]
        private VideoPlayer _videoPlayer;

        [SerializeField, SPEvent.Config("player (VideoPlayer)")]
        private SPEvent _onComplete = new SPEvent("OnComplete");

        [System.NonSerialized]
        private VideoPlayer.EventHandler _evhook_Looped;
        private VideoPlayer.EventHandler VideoPlayerLoopedCallback => _evhook_Looped ?? (_evhook_Looped = _videoPlayer_loopPointReached);


        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            if (_videoPlayer)
            {
                _videoPlayer.loopPointReached -= this.VideoPlayerLoopedCallback;
                _videoPlayer.loopPointReached += this.VideoPlayerLoopedCallback;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_videoPlayer)
            {
                _videoPlayer.loopPointReached -= this.VideoPlayerLoopedCallback;
            }
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
                    _videoPlayer.loopPointReached -= this.VideoPlayerLoopedCallback;
                }
                _videoPlayer = value;
                if (this.isActiveAndEnabled)
                {
                    _videoPlayer.loopPointReached -= this.VideoPlayerLoopedCallback;
                    _videoPlayer.loopPointReached += this.VideoPlayerLoopedCallback;
                }
            }
        }

        public SPEvent OnComplete => _onComplete;

        #endregion

        #region Methods

        private void _videoPlayer_loopPointReached(VideoPlayer source)
        {
            if (source != _videoPlayer || !this.isActiveAndEnabled)
            {
                source.loopPointReached -= this.VideoPlayerLoopedCallback;
                return;
            }

            _onComplete.ActivateTrigger(this, source);
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents() => new[] { _onComplete }; 

        #endregion

    }
}
