using UnityEngine;
using System.Linq;

using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    [System.Serializable()]
    public class EventTriggerTarget
    {

        #region Fields

        [SerializeField()]
        private float _weight = 1f;
        
        [SerializeField()]
        private UnityEngine.Object _triggerable;
        
        [SerializeField()]
        private SpecialVariantReference[] _triggerableArgs;
        
        [SerializeField()]
        private TriggerActivationType _activationType;
        
        [SerializeField()]
        private string _methodName;
        
        #endregion

        #region Properties

        /// <summary>
        /// A value that can represent the probability weight of the TriggerTarget. This is used by Trigger when configured for probability.
        /// </summary>
        public float Weight
        {
            get { return _weight; }
            set { _weight = value; }
        }

        //public GameObject Target { get { return (this._triggerable != null) ? _triggerable.gameObject : null; } }
        public UnityEngine.Object Target { get { return _triggerable; } }

        public TriggerActivationType ActivationType { get { return this._activationType; } }
        
        #endregion

        #region Configure Methods

        public void Clear()
        {
            this._triggerable = null;
            this._triggerableArgs = null;
            this._activationType = TriggerActivationType.TriggerAllOnTarget;
            this._methodName = null;
        }

        public void ConfigureTriggerAll(GameObject targ, object arg = null)
        {
            if (targ == null) throw new System.ArgumentNullException("targ");
            this._triggerable = targ.transform;
            if (arg == null)
            {
                this._triggerableArgs = null;
            }
            else
            {
                this._triggerableArgs = new [] { new SpecialVariantReference(arg) };
            }
            this._activationType = TriggerActivationType.TriggerAllOnTarget;
            this._methodName = null;
        }

        public void ConfigureTriggerAll(ITriggerable mechanism, object arg = null)
        {
            if (mechanism == null) throw new System.ArgumentNullException("mechanism");
            if (GameObjectUtil.IsGameObjectSource(mechanism))
                _triggerable = GameObjectUtil.GetGameObjectFromSource(mechanism).transform;
            else
                _triggerable = mechanism as UnityEngine.Object;
            if (arg == null || _triggerable == null)
            {
                this._triggerableArgs = null;
            }
            else
            {
                this._triggerableArgs = new [] { new SpecialVariantReference(arg) };
            }
            this._activationType = TriggerActivationType.TriggerAllOnTarget;
            this._methodName = null;
        }

        public void ConfigureTriggerAll(UnityEngine.Object targ, object arg = null)
        {
            if (targ == null) throw new System.ArgumentNullException("targ");
            if (GameObjectUtil.IsGameObjectSource(targ))
            {
                this.ConfigureTriggerAll(GameObjectUtil.GetGameObjectFromSource(targ));
                return;
            }
            else if (!EventTriggerTarget.IsValidTriggerTarget(targ, TriggerActivationType.TriggerAllOnTarget))
            {
                throw new System.ArgumentException("Must be a game object source of some sort.", "targ");
            }

            this._triggerable = targ;
            if (arg == null)
            {
                this._triggerableArgs = null;
            }
            else
            {
                this._triggerableArgs = new [] { new SpecialVariantReference(arg) };
            }
            this._activationType = TriggerActivationType.TriggerAllOnTarget;
            this._methodName = null;
        }

        public void ConfigureTriggerTarget(ITriggerable mechanism, object arg = null)
        {
            if (mechanism == null) throw new System.ArgumentNullException("mechanism");

            this._triggerable = mechanism as UnityEngine.Object;
            if (arg == null || _triggerable == null)
            {
                this._triggerableArgs = null;
            }
            else
            {
                this._triggerableArgs = new [] { new SpecialVariantReference(arg) };
            }
            this._activationType = TriggerActivationType.TriggerSelectedTarget;
            this._methodName = null;
        }

        public void ConfigureSendMessage(GameObject targ, string message, object arg = null)
        {
            if (targ == null) throw new System.ArgumentNullException("targ");
            this._triggerable = targ.transform;
            if (arg == null)
            {
                this._triggerableArgs = null;
            }
            else
            {
                this._triggerableArgs = new [] { new SpecialVariantReference(arg) };
            }
            this._methodName = message;
            this._activationType = TriggerActivationType.SendMessage;
        }

        public void ConfigureCallMethod(UnityEngine.Object targ, string methodName, params object[] args)
        {
            if (targ == null) throw new System.ArgumentNullException("targ");
            this._triggerable = targ;
            if (args == null || args.Length == 0)
            {
                this._triggerableArgs = null;
            }
            else
            {
                this._triggerableArgs = (from a in args select new SpecialVariantReference(a)).ToArray();
            }
            this._methodName = methodName;
            this._activationType = TriggerActivationType.CallMethodOnSelectedTarget;
        }

        public object CalculateTarget(object arg)
        {
            return IProxyExtensions.ReduceIfProxy(_triggerable, arg);
        }

        #endregion

        #region Trigger Methods

        public void Trigger(object sender, object incomingArg)
        {
            if (this._triggerable == null) return;

            try
            {
                //imp
                switch (this._activationType)
                {
                    case TriggerActivationType.TriggerAllOnTarget:
                        {
                            EventTriggerEvaluator.Current.TriggerAllOnTarget(_triggerable, incomingArg, sender, GetOutgoingArg(incomingArg));
                        }
                        break;
                    case TriggerActivationType.TriggerSelectedTarget:
                        {
                            EventTriggerEvaluator.Current.TriggerSelectedTarget(_triggerable, incomingArg, sender, GetOutgoingArg(incomingArg));
                        }
                        break;
                    case TriggerActivationType.SendMessage:
                        {
                            EventTriggerEvaluator.Current.SendMessageToTarget(_triggerable, incomingArg, _methodName, GetOutgoingArg(incomingArg));
                        }
                        break;
                    case TriggerActivationType.CallMethodOnSelectedTarget:
                        {
                            object[] args = null;
                            try
                            {
                                if (_triggerableArgs != null && _triggerableArgs.Length > 0)
                                {
                                    args = ArrayUtil.TryGetTemp<object>(_triggerableArgs.Length);
                                    for (int i = 0; i < args.Length; i++)
                                    {
                                        if (_triggerableArgs[i] != null)
                                        {
                                            if ((int)_triggerableArgs[i].Mode == (int)EventTriggerTarget.RefMode.TriggerArg)
                                            {
                                                args[i] = _triggerableArgs[i].IntValue == 0 ? incomingArg : null;
                                            }
                                            else
                                            {
                                                args[i] = _triggerableArgs[i].Value;
                                            }
                                        }
                                    }
                                }

                                EventTriggerEvaluator.Current.CallMethodOnSelectedTarget(_triggerable, incomingArg, _methodName, args);
                            }
                            finally
                            {
                                ArrayUtil.ReleaseTemp(args);
                            }
                        }
                        break;
                    case TriggerActivationType.EnableTarget:
                        {
                            //switch statement is faster than parse
                            switch (_methodName)
                            {
                                case "Enable":
                                case "enable":
                                    EventTriggerEvaluator.Current.EnableTarget(_triggerable, incomingArg, EnableMode.Enable);
                                    break;
                                case "Disable":
                                case "disable":
                                    EventTriggerEvaluator.Current.EnableTarget(_triggerable, incomingArg, EnableMode.Disable);
                                    break;
                                case "Toggle":
                                case "toggle":
                                    EventTriggerEvaluator.Current.EnableTarget(_triggerable, incomingArg, EnableMode.Toggle);
                                    break;
                                default:
                                    EventTriggerEvaluator.Current.EnableTarget(_triggerable, incomingArg, ConvertUtil.ToEnum<EnableMode>(_methodName));
                                    break;
                            }
                        }
                        break;
                    case TriggerActivationType.DestroyTarget:
                        {
                            EventTriggerEvaluator.Current.DestroyTarget(_triggerable, incomingArg);
                        }
                        break;
                }
            }
            catch(System.Exception ex)
            {
                Debug.LogException(ex, sender as UnityEngine.Object);
            }
        }

        internal bool IsMultiArgMethod()
        {
            if (_triggerable == null) return false;
            if (_activationType != TriggerActivationType.CallMethodOnSelectedTarget) return false;

            for(int i = 0; i < _triggerableArgs.Length; i++)
            {
                if (_triggerableArgs[i] != null && (int)_triggerableArgs[i].Mode == (int)EventTriggerTarget.RefMode.TriggerArg) return true;
            }
            return false;
        }

        internal bool TryTriggerAsMultiArgMethod(object sender, params object[] incomingArgs)
        {
            if (_triggerable == null) return false;
            if (_activationType != TriggerActivationType.CallMethodOnSelectedTarget) return false;

            object[] args = null;
            try
            {
                if (_triggerableArgs != null && _triggerableArgs.Length > 0)
                {
                    args = ArrayUtil.TryGetTemp<object>(_triggerableArgs.Length);
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (_triggerableArgs[i] != null)
                        {
                            if ((int)_triggerableArgs[i].Mode == (int)EventTriggerTarget.RefMode.TriggerArg)
                            {
                                int index = _triggerableArgs[i].ArgIndex;
                                args[i] = (incomingArgs != null && incomingArgs.InBounds(index)) ? incomingArgs[index] : null;
                            }
                            else
                            {
                                args[i] = _triggerableArgs[i].Value;
                            }
                        }
                    }
                }

                EventTriggerEvaluator.Current.CallMethodOnSelectedTarget(_triggerable, incomingArgs.FirstOrDefault(), _methodName, args);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex, sender as UnityEngine.Object);
            }
            finally
            {
                ArrayUtil.ReleaseTemp(args);
            }
            return false;
        }

        private object GetOutgoingArg(object incomingArg)
        {
            if (_triggerableArgs != null && _triggerableArgs.Length > 0)
            {
                if ((int)_triggerableArgs[0].Mode == (int)EventTriggerTarget.RefMode.TriggerArg)
                {
                    return incomingArg;
                }
                else
                {
                    return _triggerableArgs[0].Value;
                }
            }
            else
            {
                return incomingArg;
            }
        }

        #endregion


        #region Static Utils

        public static bool IsValidTriggerTarget(UnityEngine.Object obj, TriggerActivationType act)
        {
            if (obj == null) return true;

            switch (act)
            {
                case TriggerActivationType.TriggerAllOnTarget:
                case TriggerActivationType.TriggerSelectedTarget:
                    return (GameObjectUtil.IsGameObjectSource(obj) || obj is ITriggerable || obj is IProxy);
                case TriggerActivationType.SendMessage:
                    return GameObjectUtil.IsGameObjectSource(obj) || obj is IProxy;
                case TriggerActivationType.CallMethodOnSelectedTarget:
                    return true;
                case TriggerActivationType.EnableTarget:
                case TriggerActivationType.DestroyTarget:
                    return GameObjectUtil.IsGameObjectSource(obj) || obj is IProxy;
            }

            return false;
        }

        #endregion

        #region Special Types

        public enum RefMode : byte
        {
            Value = VariantReference.RefMode.Value,
            Property = VariantReference.RefMode.Property,
            Eval = VariantReference.RefMode.Eval,
            TriggerArg = 3,
        }

        [System.Serializable]
        private class SpecialVariantReference : VariantReference
        {

            public SpecialVariantReference(object arg) : base(arg) { }

            public new EventTriggerTarget.RefMode Mode
            {
                get => (EventTriggerTarget.RefMode)base.Mode;
            }

            public int ArgIndex
            {
                get => (int)_w;
            }

        }

        #endregion

    }

}
