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
        
        Tweener Create(bool preserve = false);

    }

    public static class ITweenHashExtensions
    {

        public static T UseUpdate<T>(this T hash) where T : ITweenHash { hash.Use(UpdateSequence.Update); return hash; }
        public static T UseFixedUpdate<T>(this T hash) where T : ITweenHash { hash.Use(UpdateSequence.FixedUpdate); return hash; }
        public static T UseLateUpdate<T>(this T hash) where T : ITweenHash { hash.Use(UpdateSequence.LateUpdate); return hash; }

        public static T UseNormalTime<T>(this T hash) where T : ITweenHash { hash.Use(SPTime.Normal); return hash; }
        public static T UseRealTime<T>(this T hash) where T : ITweenHash { hash.Use(SPTime.Real); return hash; }
        public static T UseSmoothTime<T>(this T hash) where T : ITweenHash { hash.Use(SPTime.Smooth); return hash; }

        public static T PlayOnce<T>(this T hash) where T : ITweenHash { hash.Wrap(TweenWrapMode.Once); return hash; }
        public static T Loop<T>(this T hash, int count = -1) where T : ITweenHash { hash.Wrap(TweenWrapMode.Loop, count); return hash; }
        public static T PingPong<T>(this T hash, int count = -1) where T : ITweenHash { hash.Wrap(TweenWrapMode.PingPong, count); return hash; }

        public static T OnStep<T>(this T hash, System.Action<Tweener> d) where T : ITweenHash { if (d != null) hash.OnStep((s, e) => d(s as Tweener)); return hash; }
        public static T OnWrap<T>(this T hash, System.Action<Tweener> d) where T : ITweenHash { if (d != null) hash.OnWrap((s, e) => d(s as Tweener)); return hash; }
        public static T OnFinish<T>(this T hash, System.Action<Tweener> d) where T : ITweenHash { if (d != null) hash.OnFinish((s, e) => d(s as Tweener)); return hash; }
        public static T OnStopped<T>(this T hash, System.Action<Tweener> d) where T : ITweenHash { if (d != null) hash.OnStopped((s, e) => d(s as Tweener)); return hash; }

        public static T Reverse<T>(this T hash) where T : ITweenHash { hash.Reverse(true); return hash; }

        public static T Ease<T>(this T hash, EaseStyle ease) where T : ITweenHash { hash.Ease(EaseMethods.GetEase(ease)); return hash; }

        /// <summary>
        /// Play the ITweenHash disposing the hash in the process.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static Tweener Play(this ITweenHash hash)
        {
            var tween = hash.Create(false);
            tween.Play();
            return tween;
        }

        /// <summary>
        /// Play the ITweenHash disposing the hash in the process.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static Tweener Play(this ITweenHash hash, float playHeadPos)
        {
            var tween = hash.Create(false);
            tween.Play(playHeadPos);
            return tween;
        }

        /// <summary>
        /// Play the ITweenHash disposing the hash in the process.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static Tweener Play(this ITweenHash hash, bool autoKill, object autoKillToken = null)
        {
            var tween = hash.Create(false);
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
        /// Play the ITweenHash disposing the hash in the process.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static Tweener Play(this ITweenHash hash, float playHeadPos, bool autoKill, object autoKillToken = null)
        {
            var tween = hash.Create(false);
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

        /// <summary>
        /// Play the ITweenHash, preserving the hash in the process (Dispose is not called).
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static Tweener PlayPreserved(this ITweenHash hash)
        {
            var tween = hash.Create(true);
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
            var tween = hash.Create(true);
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
            var tween = hash.Create(true);
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
            var tween = hash.Create(true);
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
