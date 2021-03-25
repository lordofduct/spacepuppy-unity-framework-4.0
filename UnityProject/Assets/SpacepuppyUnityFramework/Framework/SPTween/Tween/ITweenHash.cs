namespace com.spacepuppy.Tween
{
    public interface ITweenHash : System.ICloneable, System.IDisposable
    {

        ITweenHash SetId(object id);
        ITweenHash Ease(Ease ease);
        ITweenHash Delay(float delay);
        ITweenHash Use(UpdateSequence type);
        ITweenHash Use(ITimeSupplier type);
        ITweenHash Wrap(TweenWrapMode wrap, int count = -1);
        ITweenHash Reverse(bool reverse);
        ITweenHash SpeedScale(float scale);
        ITweenHash OnStep(System.EventHandler d);
        ITweenHash OnWrap(System.EventHandler d);
        ITweenHash OnFinish(System.EventHandler d);
        ITweenHash OnStopped(System.EventHandler d);
        
        Tweener Create();

    }

    public static class ITweenHashExtensions
    {

        public static ITweenHash UseUpdate(this ITweenHash hash) { return hash.Use(UpdateSequence.Update); }
        public static ITweenHash UseFixedUpdate(this ITweenHash hash) { return hash.Use(UpdateSequence.FixedUpdate); }
        public static ITweenHash UseLateUpdate(this ITweenHash hash) { return hash.Use(UpdateSequence.LateUpdate); }

        public static ITweenHash UseNormalTime(this ITweenHash hash) { return hash.Use(SPTime.Normal); }
        public static ITweenHash UseRealTime(this ITweenHash hash) { return hash.Use(SPTime.Real); }
        public static ITweenHash UseSmoothTime(this ITweenHash hash) { return hash.Use(SPTime.Smooth); }

        public static ITweenHash PlayOnce(this ITweenHash hash) { return hash.Wrap(TweenWrapMode.Once); }
        public static ITweenHash Loop(this ITweenHash hash, int count = -1) { return hash.Wrap(TweenWrapMode.Loop, count); }
        public static ITweenHash PingPong(this ITweenHash hash, int count = -1) { return hash.Wrap(TweenWrapMode.PingPong, count); }

        public static ITweenHash OnStep(this ITweenHash hash, System.Action<Tweener> d) { return d == null ? hash : hash.OnStep((s, e) => d(s as Tweener)); }
        public static ITweenHash OnWrap(this ITweenHash hash, System.Action<Tweener> d) { return d == null ? hash : hash.OnWrap((s, e) => d(s as Tweener)); }
        public static ITweenHash OnFinish(this ITweenHash hash, System.Action<Tweener> d) { return d == null ? hash : hash.OnFinish((s, e) => d(s as Tweener)); }
        public static ITweenHash OnStopped(this ITweenHash hash, System.Action<Tweener> d) { return d == null ? hash : hash.OnStopped((s, e) => d(s as Tweener)); }

        public static ITweenHash Reverse(this ITweenHash hash) { return hash.Reverse(true); }

        /// <summary>
        /// Play the ITweenHash disposing the hash in the process.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static Tweener Play(this ITweenHash hash)
        {
            var tween = hash.Create();
            tween.Play();
            hash.Dispose();
            return tween;
        }

        /// <summary>
        /// Play the ITweenHash disposing the hash in the process.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static Tweener Play(this ITweenHash hash, float playHeadPos)
        {
            var tween = hash.Create();
            tween.Play(playHeadPos);
            hash.Dispose();
            return tween;
        }

        /// <summary>
        /// Play the ITweenHash disposing the hash in the process.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static Tweener Play(this ITweenHash hash, bool autoKill, object autoKillToken = null)
        {
            var tween = hash.Create();
            if (autoKill)
            {
                tween.AutoKillToken = autoKillToken;
                tween.Play();
                if (tween.IsPlaying) SPTween.AutoKill(tween);
            }
            else
            {
                tween.Play();
            }
            hash.Dispose();
            return tween;
        }

        /// <summary>
        /// Play the ITweenHash disposing the hash in the process.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static Tweener Play(this ITweenHash hash, float playHeadPos, bool autoKill, object autoKillToken = null)
        {
            var tween = hash.Create();
            if (autoKill)
            {
                tween.AutoKillToken = autoKillToken;
                tween.Play(playHeadPos);
                SPTween.AutoKill(tween);
            }
            else
            {
                tween.Play(playHeadPos);
            }
            hash.Dispose();
            return tween;
        }

        /// <summary>
        /// Play the ITweenHash, preserving the hash in the process (Dispose is not called).
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static Tweener PlayPreserved(this ITweenHash hash)
        {
            var tween = hash.Create();
            tween.Play();
            return tween;
        }

        /// <summary>
        /// Play the ITweenHash, preserving the hash in the process (Dispose is not called).
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static Tweener PlayPreserved(this ITweenHash hash, float playHeadPos)
        {
            var tween = hash.Create();
            tween.Play(playHeadPos);
            return tween;
        }

        /// <summary>
        /// Play the ITweenHash, preserving the hash in the process (Dispose is not called).
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static Tweener PlayPreserved(this ITweenHash hash, bool autoKill, object autoKillToken = null)
        {
            var tween = hash.Create();
            if (autoKill)
            {
                tween.AutoKillToken = autoKillToken;
                tween.Play();
                if (tween.IsPlaying) SPTween.AutoKill(tween);
            }
            else
            {
                tween.Play();
            }
            return tween;
        }

        /// <summary>
        /// Play the ITweenHash, preserving the hash in the process (Dispose is not called).
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static Tweener PlayPreserved(this ITweenHash hash, float playHeadPos, bool autoKill, object autoKillToken = null)
        {
            var tween = hash.Create();
            if (autoKill)
            {
                tween.AutoKillToken = autoKillToken;
                tween.Play(playHeadPos);
                SPTween.AutoKill(tween);
            }
            else
            {
                tween.Play(playHeadPos);
            }
            return tween;
        }

    }

}
