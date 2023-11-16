using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Dynamic;
using com.spacepuppy.Project;
using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

namespace com.spacepuppy.Events
{

    public sealed class EventTriggerEvaluator : EventTriggerEvaluator.IEvaluator
    {

        #region Singleton Interface

        private static EventTriggerEvaluator _default = new EventTriggerEvaluator();
        private static IEvaluator _evaluator;

        public static EventTriggerEvaluator Default => _default;

        public static IEvaluator Current => _evaluator;

        public static void SetCurrentEvaluator(IEvaluator ev)
        {
            _evaluator = ev ?? _default;
        }

        static EventTriggerEvaluator()
        {
            _evaluator = _default;
        }

        #endregion

        #region Methods

        private void ReduceTriggerTarget(ref object target, object incomingarg, out GameObject go)
        {
            go = null;
            if (target is IProxy p)
            {
                var prms = p.Params;
                if ((prms & ProxyParams.HandlesTriggerDirectly) != 0)
                {
                    go = GameObjectUtil.GetGameObjectFromSource(target);
                }
                else if ((prms & ProxyParams.PrioritizeAsTargetFirst) != 0)
                {
                    go = GameObjectUtil.GetGameObjectFromSource(target);
                    if (go == null)
                    {
                        target = p.GetTarget_IgnoringParams(incomingarg);
                        go = GameObjectUtil.GetGameObjectFromSource(target);
                    }
                }
                else
                {
                    target = p.GetTarget_IgnoringParams(incomingarg);
                    go = GameObjectUtil.GetGameObjectFromSource(target);
                }
            }
            else
            {
                go = GameObjectUtil.GetGameObjectFromSource(target);
            }
        }

        private ITriggerable[] GetCache(GameObject go)
        {
            //we don't trigger inactive GameObjects unless they are prefabs

            EventTriggerCache cache;
            if (go.activeInHierarchy)
            {
                cache = go.AddOrGetComponent<EventTriggerCache>();
                return cache.Targets ?? cache.RefreshCache();
            }
            else if (go.HasComponent<PrefabToken>())
            {
                cache = go.GetComponent<EventTriggerCache>();
                if (cache != null) return cache.Targets ?? cache.RefreshCache();

                return go.GetComponents<ITriggerable>();
            }
            else
            {
                return ArrayUtil.Empty<ITriggerable>();
            }
        }

        public void GetAllTriggersOnTarget(object target, object incomingarg, List<ITriggerable> outputColl)
        {
            GameObject go;
            ReduceTriggerTarget(ref target, incomingarg, out go);

            if (go != null)
            {
                outputColl.AddRange(this.GetCache(go));
            }
            else if (target is ITriggerable)
            {
                outputColl.Add(target as ITriggerable);
            }
        }

        public void TriggerAllOnTarget(object target, object incomingarg, object sender, object outgoingarg)
        {
            GameObject go;
            ReduceTriggerTarget(ref target, incomingarg, out go);

            if (go != null)
            {
                var arr = this.GetCache(go);

                foreach (var t in arr)
                {
                    if (t.CanTrigger)
                    {
                        t.Trigger(sender, outgoingarg);
                    }
                }
            }
            else if(target is ITriggerable t && t.CanTrigger)
            {
                t.Trigger(sender, outgoingarg);
            }
        }

        public void TriggerAllOnTargetUncached(object target, object incomingarg, object sender, object outgoingarg)
        {
            GameObject go;
            ReduceTriggerTarget(ref target, incomingarg, out go);

            if (go != null)
            {
                using (var lst = TempCollection.GetList<ITriggerable>())
                {
                    foreach (var t in lst)
                    {
                        if (t.CanTrigger)
                        {
                            t.Trigger(sender, outgoingarg);
                        }
                    }
                }
            }
            else if (target is ITriggerable t && t.CanTrigger)
            {
                t.Trigger(sender, outgoingarg);
            }
        }

        public void TriggerSelectedTarget(object target, object incomingarg, object sender, object outgoingarg)
        {
            ITriggerable trig = null;
            if(target is IProxy p)
            {
                var prms = p.Params;
                if((prms & ProxyParams.HandlesTriggerDirectly) != 0 && target is ITriggerable)
                {
                    trig = target as ITriggerable; 
                }
                else if((prms & ProxyParams.PrioritizeAsTargetFirst) != 0 && target is ITriggerable)
                {
                    trig = target as ITriggerable;
                }
                else
                {
                    trig = p.GetTarget_IgnoringParams(incomingarg) as ITriggerable;
                }
            }

            if (trig?.CanTrigger ?? false)
            {
                trig.Trigger(sender, outgoingarg);
            }
        }

        public void SendMessageToTarget(object target, object incomingarg, string message, object outgoingarg)
        {
            if (target is IProxy p) target = p.GetTarget(incomingarg);

            var go = GameObjectUtil.GetGameObjectFromSource(target, true);
            if (go != null && message != null)
            {
                go.SendMessage(message, outgoingarg, SendMessageOptions.DontRequireReceiver);
            }
        }

        public void CallMethodOnSelectedTarget(object target, object incomingarg, string methodName, object[] args)
        {
            if (target is IProxy p) target = p.GetTarget(incomingarg);

            if (methodName != null)
            {
                //CallMethod does not support using the passed in arg
                if (args != null && args.Length == 1)
                {
                    DynamicUtil.SetValue(target, methodName, args[0]);
                }
                else
                {
                    DynamicUtil.InvokeMethod(target, methodName, args);
                }
            }
        }

        public void EnableTarget(object target, object incomingarg, EnableMode mode)
        {
            if (target is IProxy p) target = p.GetTarget(incomingarg);

            if (target is Component c && IsEnableableComponent(c))
            {
                switch (mode)
                {
                    case EnableMode.Enable:
                        c.SetEnabled(true);
                        break;
                    case EnableMode.Disable:
                        c.SetEnabled(false);
                        break;
                    case EnableMode.Toggle:
                        c.SetEnabled(!c.IsEnabled());
                        break;
                }
            }
            else
            {
                var go = GameObjectUtil.GetGameObjectFromSource(target, true);
                if (go != null)
                {
                    switch (mode)
                    {
                        case EnableMode.Enable:
                            go.SetActive(true);
                            break;
                        case EnableMode.Disable:
                            go.SetActive(false);
                            break;
                        case EnableMode.Toggle:
                            go.SetActive(!go.activeSelf);
                            break;
                    }
                }
            }
        }

        public void DestroyTarget(object target, object incomingarg)
        {
            if (target is IProxy p) target = p.GetTarget(incomingarg);

            var go = GameObjectUtil.GetGameObjectFromSource(target);
            if (go != null)
            {
                ObjUtil.SmartDestroy(go);
            }
            else if (target is UnityEngine.Object)
            {
                ObjUtil.SmartDestroy(target as UnityEngine.Object);
            }
        }

        #endregion

        #region Special Types

        private class EventTriggerCache : MonoBehaviour
        {

            #region Fields

            private ITriggerable[] _targets;

            #endregion

            #region Properties

            public ITriggerable[] Targets
            {
                get { return _targets; }
            }

            #endregion

            #region Methods

            private void Awake()
            {
                this.RefreshCache();
            }

            public ITriggerable[] RefreshCache()
            {
                _targets = this.gameObject.GetComponents<ITriggerable>();
                if (_targets.Length > 1)
                    System.Array.Sort(_targets, TriggerableOrderComparer.Default);
                return _targets;
            }

            #endregion

        }

        public interface IEvaluator
        {

            void GetAllTriggersOnTarget(object target, object incomingarg, List<ITriggerable> outputColl);

            void TriggerAllOnTarget(object target, object incomingarg, object sender, object outgoingarg);
            void TriggerSelectedTarget(object target, object incomingarg, object sender, object outgoingarg);
            void SendMessageToTarget(object target, object incomingarg, string message, object outgoingarg);
            void CallMethodOnSelectedTarget(object target, object incomingarg, string methodName, object[] methodArgs);
            void EnableTarget(object target, object incomingarg, EnableMode mode);
            void DestroyTarget(object target, object incomingarg);

        }

        #endregion

        #region Utils

        public static bool IsEnableableComponent(object c)
        {
            return !(c is Transform) && (c is Behaviour || c is Collider);
        }

        #endregion

    }

}
