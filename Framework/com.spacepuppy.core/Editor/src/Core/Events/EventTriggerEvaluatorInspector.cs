using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Events
{

    [InitializeOnLoad()]
    internal static class EventTriggerEvaluatorInspector
    {

        internal static System.EventHandler<OnTargetTriggerInEditorEventArgs> OnTargetTriggerInEditor;
        internal static EventHandlerRef<OnTargetTriggerInEditorEventArgs> OnTargetTriggerInEditor_ref() => EventHandlerRef<OnTargetTriggerInEditorEventArgs>.Create((l) => OnTargetTriggerInEditor += l, (l) => OnTargetTriggerInEditor -= l);

        #region Fields

        public const long COOLDOWN_TRIGGERED = System.TimeSpan.TicksPerSecond * 1;

        private static long _lastT;
        private static Dictionary<int, long> _triggered = new Dictionary<int, long>();
        private static HashSet<int> _cache = new HashSet<int>();

        #endregion

        #region Static Constructor

        static EventTriggerEvaluatorInspector()
        {
            EventTriggerEvaluator.SetCurrentEvaluator(SpecialEventTriggerEvaluator.Default);
            _lastT = System.DateTime.Now.Ticks;
            EditorApplication.update += Update;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
        }

        #endregion

        #region Methods

        private static void SignalTriggered(object obj, object incomingarg, TriggerActivationType activationType)
        {
            if (obj is IProxy p)
            {
                bool wastriggercall;
                switch (activationType)
                {
                    case TriggerActivationType.TriggerAllOnTarget:
                    case TriggerActivationType.TriggerSelectedTarget:
                        wastriggercall = true;
                        break;
                    default:
                        wastriggercall = false;
                        break;
                }

                if (!wastriggercall || !(obj is ITriggerable))
                {
                    obj = p.GetTarget_IgnoringParams(incomingarg);
                }
            }

            if (GameObjectUtil.IsGameObjectSource(obj))
            {
                var go = GameObjectUtil.GetGameObjectFromSource(obj);
                if (go != null)
                {
                    int id = go.GetInstanceID();
                    _triggered[id] = COOLDOWN_TRIGGERED;
                }
            }

            var d = OnTargetTriggerInEditor;
            if (d != null)
            {
                d.Invoke(null, new OnTargetTriggerInEditorEventArgs()
                {
                    target = obj,
                    incomingarg = incomingarg,
                    activationType = activationType,
                });
            }
        }





        private static void Update()
        {
            var t = System.DateTime.Now.Ticks;
            var dt = t - _lastT;
            _lastT = t;

            if (_triggered.Count > 0)
            {
                _cache.AddRange(_triggered.Keys);

                foreach (var id in _cache)
                {
                    long value = _triggered[id] - dt;
                    if (value <= 0)
                        _triggered.Remove(id);
                    else
                        _triggered[id] = value;
                }

                _cache.Clear();
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        private static void OnHierarchyItemGUI(int instanceID, Rect selectionRect)
        {
            long ticks;
            if (!_triggered.TryGetValue(instanceID, out ticks)) return;

            float t = (float)((double)ticks / (double)COOLDOWN_TRIGGERED);
            //t = com.spacepuppy.Tween.EaseMethods.ExpoEaseOut(1f - t, 1f, -1f, 1f);
            t = Mathf.Clamp01(t * t * t);
            var c = Color.blue;
            c.a = t * 0.2f;
            EditorGUI.DrawRect(selectionRect, c);
        }

        #endregion

        #region Special Types

        public class OnTargetTriggerInEditorEventArgs : System.EventArgs
        {
            public object target;
            public object incomingarg;
            public TriggerActivationType activationType;
        }

        /// <summary>
        /// Evaluator used while in the editor and triggering when the game is not in play.
        /// </summary>
        private class SpecialEventTriggerEvaluator : EventTriggerEvaluator.IEvaluator
        {

            public static readonly SpecialEventTriggerEvaluator Default = new SpecialEventTriggerEvaluator();

            static EventTriggerEvaluator.IEvaluator ResolvedCurrent => EventTriggerEvaluator.Current is SpecialEventTriggerEvaluator ? EventTriggerEvaluator.Default : EventTriggerEvaluator.Current;

            public void GetAllTriggersOnTarget(object target, object incomingarg, List<ITriggerable> outputColl)
            {
                if (Application.isPlaying)
                {
                    ResolvedCurrent.GetAllTriggersOnTarget(target, incomingarg, outputColl);
                }
                else
                {
                    var go = GameObjectUtil.GetGameObjectFromSource(target);
                    if (go != null)
                    {
                        go.GetComponents<ITriggerable>(outputColl);
                        outputColl.Sort(TriggerableOrderComparer.Default);
                    }
                    else if (target is ITriggerable)
                    {
                        outputColl.Add(target as ITriggerable);
                    }
                }
            }

            void EventTriggerEvaluator.IEvaluator.TriggerAllOnTarget(object target, object incomingarg, object sender, object arg)
            {
                SignalTriggered(target, incomingarg, TriggerActivationType.TriggerAllOnTarget);
                if (Application.isPlaying)
                {
                    ResolvedCurrent.TriggerAllOnTarget(target, incomingarg, sender, arg);
                }
                else
                {
                    EventTriggerEvaluator.Default.TriggerAllOnTargetUncached(target, incomingarg, sender, arg);
                }
            }

            void EventTriggerEvaluator.IEvaluator.TriggerSelectedTarget(object target, object incomingarg, object sender, object arg)
            {
                SignalTriggered(target, incomingarg, TriggerActivationType.TriggerSelectedTarget);
                ResolvedCurrent.TriggerSelectedTarget(target, incomingarg, sender, arg);
            }

            void EventTriggerEvaluator.IEvaluator.CallMethodOnSelectedTarget(object target, object incomingarg, string methodName, object[] args)
            {
                SignalTriggered(target, incomingarg, TriggerActivationType.CallMethodOnSelectedTarget);
                ResolvedCurrent.CallMethodOnSelectedTarget(target, incomingarg, methodName, args);
            }

            void EventTriggerEvaluator.IEvaluator.SendMessageToTarget(object target, object incomingarg, string message, object arg)
            {
                SignalTriggered(target, incomingarg, TriggerActivationType.SendMessage);
                ResolvedCurrent.SendMessageToTarget(target, incomingarg, message, arg);
            }

            void EventTriggerEvaluator.IEvaluator.EnableTarget(object target, object incomingarg, EnableMode mode)
            {
                SignalTriggered(target, incomingarg, TriggerActivationType.EnableTarget);
                ResolvedCurrent.EnableTarget(target, incomingarg, mode);
            }

            void EventTriggerEvaluator.IEvaluator.DestroyTarget(object target, object incomingarg)
            {
                SignalTriggered(target, incomingarg, TriggerActivationType.DestroyTarget);
                ResolvedCurrent.DestroyTarget(target, incomingarg);
            }

        }

        #endregion

    }

}
