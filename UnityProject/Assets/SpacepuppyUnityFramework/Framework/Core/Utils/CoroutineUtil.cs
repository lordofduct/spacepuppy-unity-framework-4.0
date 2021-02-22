using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Utils
{

    public static class CoroutineUtil
    {

        public static System.Collections.IEnumerator Wait(object instruction, System.Action<object> callback)
        {
            if (callback == null) throw new System.ArgumentNullException("callback");
            yield return instruction;
            callback(instruction);
        }

        #region StartCoroutine

        public static CoroutineToken StartCoroutine(this MonoBehaviour behaviour, System.Collections.IEnumerable enumerable)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            return new CoroutineToken(behaviour, behaviour.StartCoroutine(enumerable.GetEnumerator()));
        }

        public static CoroutineToken StartCoroutine(this MonoBehaviour behaviour, System.Func<System.Collections.IEnumerator> method)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (method == null) throw new System.ArgumentNullException("method");

            return new CoroutineToken(behaviour, behaviour.StartCoroutine(method()));
        }

        public static CoroutineToken StartCoroutine(this MonoBehaviour behaviour, System.Delegate method, params object[] args)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (method == null) throw new System.ArgumentNullException("method");

            System.Collections.IEnumerator e;
            if (com.spacepuppy.Utils.TypeUtil.IsType(method.Method.ReturnType, typeof(System.Collections.IEnumerable)))
            {
                e = (method.DynamicInvoke(args) as System.Collections.IEnumerable).GetEnumerator();
            }
            else if (com.spacepuppy.Utils.TypeUtil.IsType(method.Method.ReturnType, typeof(System.Collections.IEnumerator)))
            {
                e = (method.DynamicInvoke(args) as System.Collections.IEnumerator);
            }
            else
            {
                throw new System.ArgumentException("Delegate must have a return type of IEnumerable or IEnumerator.", "method");
            }

            return new CoroutineToken(behaviour, behaviour.StartCoroutine(e));
        }

        #endregion

        #region Invoke

        public static Coroutine InvokeLegacy(this MonoBehaviour behaviour, System.Action method, float delay)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (method == null) throw new System.ArgumentNullException("method");

            return behaviour.StartCoroutine(InvokeRedirect(method, delay));
        }

        private static System.Collections.IEnumerator InvokeRedirect(System.Action method, float delay, float repeatRate = -1f)
        {
            yield return new WaitForSeconds(delay);

            if (repeatRate < 0f)
            {
                method();
            }
            else if (repeatRate == 0f)
            {
                while (true)
                {
                    method();
                    yield return null;
                }
            }
            else
            {
                var w = new WaitForSeconds(repeatRate);
                while (true)
                {
                    method();
                    yield return w;
                }
            }
        }

        #endregion

    }

}