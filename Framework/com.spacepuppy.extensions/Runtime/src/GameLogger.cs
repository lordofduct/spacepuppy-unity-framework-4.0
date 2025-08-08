#if (UNITY_EDITOR || DEBUG || UNITY_DEBUG) && !DISABLE_GAMELOGGER
#define GAMELOGGER_ENABLED
#endif

using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

namespace com.spacepuppy
{
    public static class GameLogger
    {

        [Conditional("GAMELOGGER_ENABLED")]
        public static void Log(object message)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.Log(message);
#endif
        }

        [Conditional("GAMELOGGER_ENABLED")]
        public static void Log(object message, UnityEngine.Object context)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.Log(message, context);
#endif
        }

        [Conditional("GAMELOGGER_ENABLED")]
        public static void LogWarning(object message)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.LogWarning(message);
#endif
        }

        [Conditional("GAMELOGGER_ENABLED")]
        public static void LogWarning(object message, UnityEngine.Object context)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.LogWarning(message, context);
#endif
        }

        [Conditional("GAMELOGGER_ENABLED")]
        public static void LogError(object message)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.LogError(message);
#endif
        }

        [Conditional("GAMELOGGER_ENABLED")]
        public static void LogError(object message, UnityEngine.Object context)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.LogError(message, context);
#endif
        }

        [Conditional("GAMELOGGER_ENABLED")]
        public static void LogException(System.Exception exception)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.LogException(exception);
#endif
        }

        [Conditional("GAMELOGGER_ENABLED")]
        public static void LogException(System.Exception exception, UnityEngine.Object context)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.LogException(exception, context);
#endif
        }

        [Conditional("GAMELOGGER_ENABLED")]
        public static void Assert(bool assertion)
        {
#if GAMELOGGER_ENABLED
            UnityEngine.Debug.Assert(assertion);
#endif
        }

    }
}
