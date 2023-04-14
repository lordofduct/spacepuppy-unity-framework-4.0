using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{
    public sealed class t_PhaseEvaluatorStateMachine : t_PhaseEvaluator, IMStartOrEnableReceiver
    {

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            this.EvaluateTransitions(this.CurrentValue, this.CurrentValue);
        }

        protected override void EvaluateTransitions(float oldvalue, float newvalue)
        {
            using (var dict = TempCollection.GetDict<GameObject, bool>())
            {
                int cnt = this.Phases.Count;
                for (int i = 0; i < cnt; i++)
                {
                    var phase = this.Phases[i];
                    var go = GameObjectUtil.GetGameObjectFromSource(phase.Target, true);
                    if (go == null) continue;

                    if (!dict.ContainsKey(go)) dict.Add(go, false);

                    switch(phase.Direction)
                    {
                        case Direction.Up:
                            if(newvalue >= phase.Value)
                            {
                                dict[go] = true;
                            }
                            break;
                        case Direction.Down:
                            if(newvalue <= phase.Value)
                            {
                                dict[go] = true;
                            }
                            break;
                    }
                }

                //first disable, then enable, this way you can use the OnDisable and OnEnable of the states to perform actions predictably
                foreach (var pair in dict)
                {
                    if (!pair.Value) pair.Key.SetActive(false);
                }
                foreach(var pair in dict)
                {
                    if (pair.Value) pair.Key.SetActive(true);
                }
            }
        }

    }
}
