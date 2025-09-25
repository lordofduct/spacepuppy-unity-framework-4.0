#if (UNITY_EDITOR || DEBUG || UNITY_DEBUG || ENABLE_GAMELOGGER) && !DISABLE_GAMELOGGER
#define GAMELOGGER_ENABLED
#endif

using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

namespace com.spacepuppy
{

    public static class GameLogger
    {

        [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("UNITY_DEBUG"), Conditional("ENABLE_GAMELOGGER")]
        public static void Log(object message)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.Log(message);
#endif
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("UNITY_DEBUG"), Conditional("ENABLE_GAMELOGGER")]
        public static void Log(object message, UnityEngine.Object context)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.Log(message, context);
#endif
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("UNITY_DEBUG"), Conditional("ENABLE_GAMELOGGER")]
        public static void LogWarning(object message)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.LogWarning(message);
#endif
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("UNITY_DEBUG"), Conditional("ENABLE_GAMELOGGER")]
        public static void LogWarning(object message, UnityEngine.Object context)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.LogWarning(message, context);
#endif
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("UNITY_DEBUG"), Conditional("ENABLE_GAMELOGGER")]
        public static void LogError(object message)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.LogError(message);
#endif
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("UNITY_DEBUG"), Conditional("ENABLE_GAMELOGGER")]
        public static void LogError(object message, UnityEngine.Object context)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.LogError(message, context);
#endif
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("UNITY_DEBUG"), Conditional("ENABLE_GAMELOGGER")]
        public static void LogException(System.Exception exception)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.LogException(exception);
#endif
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("UNITY_DEBUG"), Conditional("ENABLE_GAMELOGGER")]
        public static void LogException(System.Exception exception, UnityEngine.Object context)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.LogException(exception, context);
#endif
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("UNITY_DEBUG"), Conditional("ENABLE_GAMELOGGER")]
        public static void Assert(bool assertion)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.Assert(assertion);
#endif
        }



        /// <summary>
        /// Returns a context object for logging if and only if it's the first to call the method. If the object dies the next object 
        /// of that type to call will take over as context. This is ueful for logging for only one instance of a class that there are 
        /// multiple instances of.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="forceContext"></param>
        /// <returns></returns>
        public static LogContext<T> TryGetMonoContext<T>(this T context, bool forceContext = false) where T : UnityEngine.Object
        {
#if GAMELOGGER_ENABLED
            if (context != null && (forceContext || LogContext<T>.context == null || LogContext<T>.context == context))
            {
                LogContext<T>.context = context;
                return LogContext<T>.instance;
            }
#endif
            return null;
        }
        public class LogContext<T> where T : UnityEngine.Object
        {

            internal static T context;
            internal static readonly LogContext<T> instance = new();

            [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("UNITY_DEBUG"), Conditional("ENABLE_GAMELOGGER")]
            public void Log(object message)
            {
#if GAMELOGGER_ENABLED
                UnityEngine.Debug.Log(message);
#endif
            }

            [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("UNITY_DEBUG"), Conditional("ENABLE_GAMELOGGER")]
            public void Log(object message, UnityEngine.Object context)
            {
#if GAMELOGGER_ENABLED
                UnityEngine.Debug.Log(message, context);
#endif
            }

        }

    }
}
