using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public interface ISignalAwakeHandler<T>
    {
        void OnComponentAwake(T component);
    }

    [MSignalAwake]
    public interface IMSignalAwake<T> : IMixin
    {

    }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class MSignalAwakeAttribute : MixinConstructorAttribute
    {

        public override void OnConstructed(IMixin obj, System.Type mixinType)
        {
            if (mixinType == null) return;
            if (!TypeUtil.IsType(mixinType, typeof(ISignalAwakeHandler<>))) return;

            System.Type receiverType = null;
            System.Reflection.MethodInfo method = null;
            try
            {
                var componentType = mixinType.GetGenericArguments().FirstOrDefault();
                if (componentType == null) return;

                receiverType = typeof(ISignalAwakeHandler<>).MakeGenericType(componentType);
                if (receiverType == null) return;

                method = receiverType.GetMethod(nameof(ISignalAwakeHandler<object>.OnComponentAwake));
                if (method == null) return;
            }
            catch (System.Exception) { }

            var go = GameObjectUtil.GetGameObjectFromSource(obj, true);
            if (go != null)
            {
                var args = new object[] { obj };
                go.Signal(receiverType, (c) => method.Invoke(c, args), true);
            }
        }
    }

    [MSignalAwakeUpwards]
    public interface IMSignalAwakeUpwards<T> : IMixin
    {

    }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class MSignalAwakeUpwardsAttribute : MixinConstructorAttribute
    {

        public override void OnConstructed(IMixin obj, System.Type mixinType)
        {
            if (mixinType == null) return;
            if (!TypeUtil.IsType(mixinType, typeof(IMSignalAwakeUpwards<>))) return;

            System.Type receiverType = null;
            System.Reflection.MethodInfo method = null;
            try
            {
                var componentType = mixinType.GetGenericArguments().FirstOrDefault();
                if (componentType == null) return;

                receiverType = typeof(ISignalAwakeHandler<>).MakeGenericType(componentType);
                if (receiverType == null) return;

                method = receiverType.GetMethod(nameof(ISignalAwakeHandler<object>.OnComponentAwake));
                if (method == null) return;
            }
            catch (System.Exception) { }

            var go = GameObjectUtil.GetGameObjectFromSource(obj, true);
            if (go != null)
            {
                var args = new object[] { obj };
                go.SignalUpwards(receiverType, (c) => method.Invoke(c, args), true);
            }
        }
    }

    [MSignalAwakeUpwards]
    public interface IMBroadcastAwake<T> : IMixin
    {

    }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class MBroadcastAwakeAttribute : MixinConstructorAttribute
    {

        public override void OnConstructed(IMixin obj, System.Type mixinType)
        {
            if (mixinType == null) return;
            if (!TypeUtil.IsType(mixinType, typeof(IMBroadcastAwake<>))) return;

            System.Type receiverType = null;
            System.Reflection.MethodInfo method = null;
            try
            {
                var componentType = mixinType.GetGenericArguments().FirstOrDefault();
                if (componentType == null) return;

                receiverType = typeof(ISignalAwakeHandler<>).MakeGenericType(componentType);
                if (receiverType == null) return;

                method = receiverType.GetMethod(nameof(ISignalAwakeHandler<object>.OnComponentAwake));
                if (method == null) return;
            }
            catch (System.Exception) { }

            var go = GameObjectUtil.GetGameObjectFromSource(obj, true);
            if (go != null)
            {
                var args = new object[] { obj };
                go.Broadcast(receiverType, (c) => method.Invoke(c, args), true);
            }
        }
    }


    public interface ISignalEnabledHandler<T>
    {
        void OnComponentEnabled(T component);
        void OnComponentDisabled(T component);
    }

    [MSignalEnabled]
    public interface IMSignalEnabled<T> : IMixin, IEventfulComponent
    {

    }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class MSignalEnabledAttribute : MixinConstructorAttribute
    {

        public override void OnConstructed(IMixin obj, System.Type mixinType)
        {
            if (mixinType == null) return;
            if (!TypeUtil.IsType(mixinType, typeof(IMSignalEnabled<>))) return;

            var c = obj as IEventfulComponent;
            if (c == null) return;

            System.Type receiverType = null;
            System.Reflection.MethodInfo enabledmethod = null;
            System.Reflection.MethodInfo disabledmethod = null;
            try
            {
                var componentType = mixinType.GetGenericArguments().FirstOrDefault();
                if (componentType == null) return;

                receiverType = typeof(ISignalEnabledHandler<>).MakeGenericType(componentType);
                if (receiverType == null) return;

                enabledmethod = receiverType.GetMethod(nameof(ISignalEnabledHandler<object>.OnComponentEnabled));
                disabledmethod = receiverType.GetMethod(nameof(ISignalEnabledHandler<object>.OnComponentDisabled));
                if (enabledmethod == null || disabledmethod == null) return;
            }
            catch (System.Exception) { }

            var args = new object[] { obj };
            var enabledfunctor = new System.Action<Component>((o) => enabledmethod.Invoke(o, args));
            var disabledfunctor = new System.Action<Component>((o) => disabledmethod.Invoke(o, args));
            c.OnEnabled += (s, e) =>
            {
                c.gameObject.Signal(receiverType, enabledfunctor, true);
            };
            c.OnDisabled += (s, e) =>
            {
                c.gameObject.Signal(receiverType, disabledfunctor, true);
            };
        }
    }

    [MSignalEnabledUpwards]
    public interface IMSignalEnabledUpwards<T> : IMixin, IEventfulComponent
    {

    }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class MSignalEnabledUpwardsAttribute : MixinConstructorAttribute
    {

        public override void OnConstructed(IMixin obj, System.Type mixinType)
        {
            if (mixinType == null) return;
            if (!TypeUtil.IsType(mixinType, typeof(IMSignalEnabledUpwards<>))) return;

            var c = obj as IEventfulComponent;
            if (c == null) return;

            System.Type receiverType = null;
            System.Reflection.MethodInfo enabledmethod = null;
            System.Reflection.MethodInfo disabledmethod = null;
            try
            {
                var componentType = mixinType.GetGenericArguments().FirstOrDefault();
                if (componentType == null) return;

                receiverType = typeof(ISignalEnabledHandler<>).MakeGenericType(componentType);
                if (receiverType == null) return;

                enabledmethod = receiverType.GetMethod(nameof(ISignalEnabledHandler<object>.OnComponentEnabled));
                disabledmethod = receiverType.GetMethod(nameof(ISignalEnabledHandler<object>.OnComponentDisabled));
                if (enabledmethod == null || disabledmethod == null) return;
            }
            catch (System.Exception) { }

            var args = new object[] { obj };
            var enabledfunctor = new System.Action<Component>((o) => enabledmethod.Invoke(o, args));
            var disabledfunctor = new System.Action<Component>((o) => disabledmethod.Invoke(o, args));
            c.OnEnabled += (s, e) =>
            {
                c.gameObject.SignalUpwards(receiverType, enabledfunctor, true);
            };
            c.OnDisabled += (s, e) =>
            {
                c.gameObject.SignalUpwards(receiverType, disabledfunctor, true);
            };
        }
    }

    [MBroadcastEnabled]
    public interface IMBroadcastEnabled<T> : IMixin, IEventfulComponent
    {

    }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class MBroadcastEnabledAttribute : MixinConstructorAttribute
    {

        public override void OnConstructed(IMixin obj, System.Type mixinType)
        {
            if (mixinType == null) return;
            if (!TypeUtil.IsType(mixinType, typeof(IMBroadcastEnabled<>))) return;

            var c = obj as IEventfulComponent;
            if (c == null) return;

            System.Type receiverType = null;
            System.Reflection.MethodInfo enabledmethod = null;
            System.Reflection.MethodInfo disabledmethod = null;
            try
            {
                var componentType = mixinType.GetGenericArguments().FirstOrDefault();
                if (componentType == null) return;

                receiverType = typeof(ISignalEnabledHandler<>).MakeGenericType(componentType);
                if (receiverType == null) return;

                enabledmethod = receiverType.GetMethod(nameof(ISignalEnabledHandler<object>.OnComponentEnabled));
                disabledmethod = receiverType.GetMethod(nameof(ISignalEnabledHandler<object>.OnComponentDisabled));
                if (enabledmethod == null || disabledmethod == null) return;
            }
            catch (System.Exception) { }

            var args = new object[] { obj };
            var enabledfunctor = new System.Action<Component>((o) => enabledmethod.Invoke(o, args));
            var disabledfunctor = new System.Action<Component>((o) => disabledmethod.Invoke(o, args));
            c.OnEnabled += (s, e) =>
            {
                c.gameObject.Broadcast(receiverType, enabledfunctor, true);
            };
            c.OnDisabled += (s, e) =>
            {
                c.gameObject.Broadcast(receiverType, disabledfunctor, true);
            };
        }
    }



}
