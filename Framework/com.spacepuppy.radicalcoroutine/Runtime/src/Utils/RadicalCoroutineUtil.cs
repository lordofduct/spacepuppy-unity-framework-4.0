using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace com.spacepuppy.Utils
{

    public static class RadicalCoroutineUtil
    {

        private class YieldArgAdapter : CustomYieldInstruction
        {
            private IRadicalYieldInstruction _instruction;
            private object _current;
            public YieldArgAdapter(IRadicalYieldInstruction inst)
            {
                _instruction = inst;
            }

            public override bool keepWaiting => !_instruction.IsComplete;
        }
        public static System.Collections.IEnumerator ToStandardYieldArg(this IRadicalYieldInstruction inst)
        {
            if (inst is CustomYieldInstruction)
                return inst as CustomYieldInstruction;
            else
                return new YieldArgAdapter(inst);
        }

        #region RadicalCoroutine

        public static RadicalCoroutine StartRadicalCoroutine(this MonoBehaviour behaviour, System.Collections.IEnumerator routine, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.Default)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (routine == null) throw new System.ArgumentNullException("routine");

            var co = RadicalCoroutine.Create(routine);
            co.Start(behaviour, disableMode);
            return co;
        }

        public static RadicalCoroutine StartRadicalCoroutine(this MonoBehaviour behaviour, System.Collections.IEnumerable routine, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.Default)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (routine == null) throw new System.ArgumentNullException("routine");

            var co = RadicalCoroutine.Create(routine.GetEnumerator());
            co.Start(behaviour, disableMode);
            return co;
        }

        public static RadicalCoroutine StartRadicalCoroutine(this MonoBehaviour behaviour, System.Func<System.Collections.IEnumerator> method, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.Default)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (method == null) throw new System.ArgumentNullException("routine");

            var co = RadicalCoroutine.Create(method());
            co.Start(behaviour, disableMode);
            return co;
        }

        public static RadicalCoroutine StartRadicalCoroutine(this MonoBehaviour behaviour, System.Delegate method, object[] args = null, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.Default)
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

            var co = RadicalCoroutine.Create(e);
            co.Start(behaviour, disableMode);
            return co;
        }




        /// <summary>
        /// Creates a RadicalCoroutine that will be cleaned up on complete. This RadicalCoroutine will be immediately returned to the cache pool on finish, only a simple IRadicalYieldInstruction handle is returned for awaiting. 
        /// Do not store a reference to this instruction as it will be cleaned up after it finishes.
        /// </summary>
        /// <param name="behaviour"></param>
        /// <param name="routine"></param>
        /// <param name="disableMode"></param>
        /// <returns></returns>
        public static IRadicalYieldInstruction StartPooledRadicalCoroutine(this MonoBehaviour behaviour, System.Collections.IEnumerator routine, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.Default)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (routine == null) throw new System.ArgumentNullException("routine");

            var co = RadicalCoroutine.Create(routine);
            co.Start(behaviour, disableMode);
            return RadicalCoroutine.AutoRelease(ref co);
        }

        public static IRadicalYieldInstruction StartPooledRadicalCoroutine(this MonoBehaviour behaviour, System.Collections.IEnumerable routine, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.Default)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (routine == null) throw new System.ArgumentNullException("routine");

            var co = RadicalCoroutine.Create(routine.GetEnumerator());
            co.Start(behaviour, disableMode);
            return RadicalCoroutine.AutoRelease(ref co);
        }

        public static IRadicalYieldInstruction StartPooledRadicalCoroutine(this MonoBehaviour behaviour, System.Func<System.Collections.IEnumerator> method, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.Default)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (method == null) throw new System.ArgumentNullException("routine");

            var co = RadicalCoroutine.Create(method());
            co.Start(behaviour, disableMode);
            return RadicalCoroutine.AutoRelease(ref co);
        }

        public static IRadicalYieldInstruction StartPooledRadicalCoroutine(this MonoBehaviour behaviour, System.Delegate method, object[] args = null, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.Default)
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

            var co = RadicalCoroutine.Create(e);
            co.Start(behaviour, disableMode);
            return RadicalCoroutine.AutoRelease(ref co);
        }




        public static RadicalCoroutine StartRadicalCoroutineAsync(this MonoBehaviour behaviour, System.Collections.IEnumerator routine, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.Default)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (routine == null) throw new System.ArgumentNullException("routine");

            var co = RadicalCoroutine.Create(routine);
            co.StartAsync(behaviour, disableMode);
            return co;
        }

        public static RadicalCoroutine StartRadicalCoroutineAsync(this MonoBehaviour behaviour, System.Collections.IEnumerable routine, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.Default)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (routine == null) throw new System.ArgumentNullException("routine");

            var co = RadicalCoroutine.Create(routine.GetEnumerator());
            co.StartAsync(behaviour, disableMode);
            return co;
        }

        public static RadicalCoroutine StartRadicalCoroutineAsync(this MonoBehaviour behaviour, System.Func<System.Collections.IEnumerator> method, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.Default)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (method == null) throw new System.ArgumentNullException("routine");

            var co = RadicalCoroutine.Create(method());
            co.StartAsync(behaviour, disableMode);
            return co;
        }

        public static RadicalCoroutine StartRadicalCoroutineAsync(this MonoBehaviour behaviour, System.Delegate method, object[] args = null, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.Default)
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

            var co = RadicalCoroutine.Create(e);
            co.StartAsync(behaviour, disableMode);
            return co;
        }





        public static RadicalCoroutine StartAutoKillRadicalCoroutine(this MonoBehaviour behaviour, System.Collections.IEnumerator routine, object autoKillToken, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.Default)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (routine == null) throw new System.ArgumentNullException("routine");

            var co = RadicalCoroutine.Create(routine);
            co.StartAutoKill(behaviour, autoKillToken, disableMode);
            return co;
        }

        public static RadicalCoroutine StartAutoKillRadicalCoroutine(this MonoBehaviour behaviour, System.Collections.IEnumerable routine, object autoKillToken, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.Default)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (routine == null) throw new System.ArgumentNullException("routine");

            var co = RadicalCoroutine.Create(routine.GetEnumerator());
            co.StartAutoKill(behaviour, autoKillToken, disableMode);
            return co;
        }

        public static RadicalCoroutine StartAutoKillRadicalCoroutine(this MonoBehaviour behaviour, System.Func<System.Collections.IEnumerator> method, object autoKillToken, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.Default)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (method == null) throw new System.ArgumentNullException("routine");

            var co = RadicalCoroutine.Create(method());
            co.StartAutoKill(behaviour, autoKillToken, disableMode);
            return co;
        }



        public static RadicalCoroutine StartValidatedRadicalCoroutine(this MonoBehaviour behaviour, System.Collections.IEnumerator routine, System.Func<bool> validator, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.Default)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (routine == null) throw new System.ArgumentNullException("routine");
            if (validator == null) throw new System.ArgumentNullException("validator");

            var co = RadicalCoroutine.Create(ValidatedRoutine(routine, validator));
            co.Start(behaviour, disableMode);
            return co;
        }

        public static System.Collections.IEnumerator ValidatedRoutine(System.Collections.IEnumerator routine, System.Func<bool> validator)
        {
            if (routine == null) throw new System.ArgumentNullException("routine");
            if (validator == null) throw new System.ArgumentNullException("validator");

            while (validator() && routine.MoveNext())
            {
                yield return routine.Current;
            }
        }

        #endregion

        #region Invoke

        public static RadicalCoroutine Invoke(this MonoBehaviour behaviour, System.Action method, float delay, ITimeSupplier time = null, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.CancelOnDisable)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (method == null) throw new System.ArgumentNullException("method");

            return StartRadicalCoroutine(behaviour, RadicalInvokeRedirect(method, delay, -1f, time), disableMode);
        }

        public static RadicalCoroutine Invoke(this MonoBehaviour behaviour, System.Action method, SPTimePeriod period, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.CancelOnDisable)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (method == null) throw new System.ArgumentNullException("method");

            return StartRadicalCoroutine(behaviour, RadicalInvokeRedirect(method, period.Seconds, -1f, period.TimeSupplier), disableMode);
        }

        public static InvokeHandle InvokeGuaranteed(this MonoBehaviour behaviour, System.Action method, float delay, ITimeSupplier time = null)
        {
            if (method == null) throw new System.ArgumentNullException("method");
            //return StartRadicalCoroutine(GameLoop.Hook, RadicalInvokeRedirect(method, delay, -1f, time));

            return InvokeHandle.Begin(GameLoop.UpdatePump, method, delay, time);
        }

        public static RadicalCoroutine InvokeRepeating(this MonoBehaviour behaviour, System.Action method, float delay, float repeatRate, ITimeSupplier time = null, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.CancelOnDisable)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (method == null) throw new System.ArgumentNullException("method");

            return StartRadicalCoroutine(behaviour, RadicalInvokeRedirect(method, delay, repeatRate, time), disableMode);
        }

        internal static System.Collections.IEnumerator RadicalInvokeRedirect(System.Action method, float delay, float repeatRate = -1f, ITimeSupplier time = null)
        {
            if (delay < SPConstants.MIN_FRAME_DELTA)
                yield return null;
            else if (delay > 0f)
                yield return WaitForDuration.Seconds(delay, time);

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
                while (true)
                {
                    method();
                    yield return WaitForDuration.Seconds(repeatRate, time);
                }
            }
        }

        public static RadicalCoroutine InvokeAfterYield(this MonoBehaviour behaviour, System.Action method, object yieldInstruction, RadicalCoroutineDisableMode disableMode = RadicalCoroutineDisableMode.CancelOnDisable)
        {
            if (behaviour == null) throw new System.ArgumentNullException("behaviour");
            if (method == null) throw new System.ArgumentNullException("method");

            return StartRadicalCoroutine(behaviour, InvokeAfterYieldRedirect(method, yieldInstruction));
        }

        internal static System.Collections.IEnumerator InvokeAfterYieldRedirect(System.Action method, object yieldInstruction)
        {
            yield return yieldInstruction;
            method();
        }

        #endregion

    }

}
