using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public interface IMixin
    {

    }

    public class AutoInitMixinAttribute : System.Attribute
    {

        public const string INIT_METHOD_NAME = "OnInitMixin";

        public virtual void Initialize(IMixin mixin, System.Type mixinType)
        {
            mixinType.GetMethod(INIT_METHOD_NAME).Invoke(mixin, null);
        }

    }

    /// <summary>
    /// Static class for initializing mixins.
    /// </summary>
    public static class MixinUtil
    {

        private static readonly System.Type _rootMixinType = typeof(IMixin);
        private static readonly List<System.Type> _knownMixinTypes = new();
        private static readonly Dictionary<System.Type, AutoInitMixinAttribute> _autoMixinTypeAttribTable = new();
        private static bool _useAutoTableForInitialization;

        static MixinUtil()
        {
            foreach (var tp in TypeUtil.GetTypes())
            {
                if (tp != _rootMixinType && tp.IsInterface && _rootMixinType.IsAssignableFrom(tp))
                {
                    _knownMixinTypes.Add(tp);
                    var attrib = tp.GetCustomAttribute<AutoInitMixinAttribute>(false);
                    if (attrib != null) _autoMixinTypeAttribTable[tp] = attrib;
                }
            }
            _useAutoTableForInitialization = _autoMixinTypeAttribTable.Count < 128;
        }

        public static IEnumerable<System.Type> GetKnownMixinTypes() => _knownMixinTypes;

        public static void InitializeMixins(IMixin target)
        {
            if (target == null) return;

            if (_useAutoTableForInitialization)
            {
                foreach (var pair in _autoMixinTypeAttribTable)
                {
                    if (pair.Key.IsInstanceOfType(target))
                    {
                        pair.Value.Initialize(target, pair.Key);
                    }
                }
            }
            else
            {
                foreach (var tp in target.GetType().GetInterfaces())
                {
                    if (_autoMixinTypeAttribTable.TryGetValue(tp, out AutoInitMixinAttribute attrib))
                    {
                        attrib.Initialize(target, tp);
                    }
                }
            }
        }

    }

    /// <summary>
    /// On start or on enable if and only if start already occurred. This adjusts the order of 'OnEnable' so that it can be used in conjunction with 'OnDisable' to wire up handlers cleanly. 
    /// OnEnable occurs BEFORE Start sometimes, and other components aren't ready yet. This remedies that.
    /// </summary>
    /// <remarks>
    /// In earlier versions of Spacepuppy Framework this was implemented directly on SPComponent. I've since moved it to here to match our new IMixin interface, and so that only those components 
    /// that need OnStartOrEnable actually have it implemented. No need for empty method calls on ALL components.
    /// </remarks>
    [AutoInitMixin]
    public interface IMStartOrEnableReceiver : IMixin, IEventfulComponent
    {
        sealed void OnInitMixin()
        {
            this.OnStarted += (s, e) =>
            {
                this.OnStartOrEnable();
            };
            this.OnEnabled += (s, e) =>
            {
                if (this.started)
                {
                    this.OnStartOrEnable();
                }
            };
        }

        void OnStartOrEnable();

    }

    /// <summary>
    /// Sometimes you want to run Start late, to allow Start to be called on all other scripts. Basically adding a final ordering for Start similar to LateUpdate.
    /// </summary>
    [AutoInitMixin]
    public interface IMLateStartReceiver : IMixin, IEventfulComponent
    {
        sealed void OnInitMixin()
        {
            this.OnStarted += (s, e) =>
            {
                switch (GameLoop.CurrentSequence)
                {
                    case UpdateSequence.None:
                        GameLoop.UpdateHandle.BeginInvoke(() =>
                        {
                            this.OnLateStart();
                        });
                        break;
                    case UpdateSequence.Update:
                        GameLoop.LateUpdateHandle.BeginInvoke(() =>
                        {
                            this.OnLateStart();
                        });
                        break;
                    case UpdateSequence.FixedUpdate:
                    case UpdateSequence.LateUpdate:
                        GameLoop.UpdateHandle.BeginInvoke(() =>
                        {
                            this.OnLateStart();
                        });
                        break;
                }
            };
        }

        void OnLateStart();
    }

    /// <summary>
    /// Sometimes you want to run StartOrEnable late, to allow Start to be called on all other scripts. Basically adding a final ordering point for Start similar to LateUpdate.
    /// </summary>
    [AutoInitMixin]
    public interface IMLateStartOrEnableReceiver : IMixin, IEventfulComponent
    {

        sealed void OnInitMixin()
        {
            var state = new MixinState()
            {
                target = this,
            };
            this.OnDisabled += (s, e) =>
            {
                GameLoop.LateUpdatePump.Remove(state);
            };
            this.OnEnabled += (s, e) =>
            {
                GameLoop.LateUpdatePump.Add(state);
            };
        }

        void OnLateStartOrEnable();

        class MixinState : IUpdateable
        {

            public IMLateStartOrEnableReceiver target;
            public void Update()
            {
                GameLoop.LateUpdatePump.Remove(this);
                if (target.enabled)
                {
                    target.OnLateStartOrEnable();
                }
            }

        }

    }

}
