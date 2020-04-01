using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy
{
    /// <summary>
    /// An entry point into the gameloop.
    /// </summary>
    /// <remarks>
    /// Currently in 4.0 we're diverging from the classic 'GameLoop' of earlier SP versions. We may refactor this later.
    /// </remarks>
    public class GameLoop : SPComponent
    {

        #region Static Interface

        private static GameLoop _instance;
        private static GameLoop GetInstance()
        {
            if(_instance == null)
            {
                var go = new GameObject("SP.GameLoop");
                _instance = go.AddComponent<GameLoop>();
            }
            return _instance;
        }
        

        public static CoroutineToken InvokeGuaranteed(System.Action callback, float delay, ITimeSupplier timeSupplier)
        {
            if (callback == null) return CoroutineToken.Empty;

            var loop = GetInstance();
            return new CoroutineToken(loop, loop.StartCoroutine(loop.InvokeRoutine(callback, delay, timeSupplier)));
        }

        #endregion

        #region Methods
        public static void QuitApplication()
        {
            //if (_quitState == QuitState.None)
            //{
            //    _quitState = QuitState.BeforeQuit;
            //    if (BeforeApplicationQuit != null) BeforeApplicationQuit(_instance, System.EventArgs.Empty);

            //    if (_quitState == QuitState.BeforeQuit)
            //    {
            //        //wasn't cancelled, or force quit
            //        if (UnityEngine.Application.isEditor)
            //        {
            //            try
            //            {
            //                var tp = com.spacepuppy.Utils.TypeUtil.FindType("UnityEditor.EditorApplication");
            //                tp.GetProperty("isPlaying").SetValue(null, false, null);
            //            }
            //            catch
            //            {
            //                UnityEngine.Debug.Log("Failed to stop play in editor.");
            //            }
            //        }
            //        else
            //        {
            //            UnityEngine.Application.Quit();
            //        }

            //    }
            //}

            //wasn't cancelled, or force quit
            if (UnityEngine.Application.isEditor)
            {
                try
                {
                    var tp = com.spacepuppy.Utils.TypeUtil.FindType("UnityEditor.EditorApplication");
                    tp.GetProperty("isPlaying").SetValue(null, false, null);
                }
                catch
                {
                    UnityEngine.Debug.Log("Failed to stop play in editor.");
                }
            }
            else
            {
                UnityEngine.Application.Quit();
            }
        }
        private System.Collections.IEnumerator InvokeRoutine(System.Action callback, float delay, ITimeSupplier timeSupplier)
        {
            if (timeSupplier == null) timeSupplier = SPTime.Normal;

            while(delay > 0f)
            {
                yield return null;
                delay -= timeSupplier.Delta;
            }
            callback();
        }

        #endregion

    }

}
