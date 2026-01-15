using UnityEngine;
using System.Collections;

using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

namespace com.spacepuppy
{

    /// <summary>
    /// A composite yield instruction that waits for multiple instructions to all complete before continuing.
    /// </summary>
    public class WaitForAllComplete : RadicalYieldInstruction
    {

        private Deque<object> _instructions;
        private RadicalCoroutine _routine;

        public WaitForAllComplete(params object[] instructions)
        {
            _instructions = new Deque<object>(instructions);
        }

        public void Add(object instruction)
        {
            _instructions.Push(instruction);
        }

        protected override void SetSignal()
        {
            base.SetSignal();

            if(_routine != null)
            {
                RadicalCoroutine.Release(ref _routine);
            }
        }

        protected override bool TestIfComplete(out object yieldObject)
        {
            GameLoop.AssertMainThread();

            yieldObject = null;
            if (_instructions == null || _instructions.Count == 0)
            {
                this.SetSignal();
                return true;
            }

            if(_routine != null)
            {
                if(_routine.Finished)
                {
                    RadicalCoroutine.Release(ref _routine);
                    this.SetSignal();
                    return true;
                }
            }
            else
            {
                object current;
                for (int i = 0; i < _instructions.Count; i++)
                {
                    current = _instructions[i];
                    if (current == null)
                    {
                        _instructions.RemoveAt(i);
                        i--;
                    }
                    else if(current is IRadicalYieldInstruction ryi && ryi.IsComplete)
                    {
                        _instructions.RemoveAt(i);
                        i--;
                    }
                }

                if (_instructions.Count == 0)
                {
                    this.SetSignal();
                    return true;
                }

                _routine = GameLoop.Hook.StartRadicalCoroutine(this.WaitForAll());
                return false;
            }

            return true;
        }

        private IEnumerator WaitForAll()
        {
            object current;
            while (_instructions != null && _instructions.Count > 0 && !this.SafeIsComplete)
            {
                for (int i = 0; i < _instructions.Count; i++)
                {
                    current = _instructions[i];
                    if (current == null)
                    {
                        _instructions.RemoveAt(i);
                        i--;
                    }
                    else if (current is IRadicalYieldInstruction ryi && ryi.IsComplete)
                    {
                        _instructions.RemoveAt(i);
                        i--;
                    }
                }

                if(_instructions.Count > 0)
                {
                    yield return _instructions.Shift();
                }
            }

            if (!this.SafeIsComplete) this.SetSignal();
        }
    }

    /// <summary>
    /// A composite yield instruction that waits for any one of multiple instruction to complete before continuing.
    /// </summary>
    public class WaitForAnyComplete : RadicalYieldInstruction
    {

        private Deque<PairInfo> _instructions = new Deque<PairInfo>();
        private RadicalCoroutine _routine;
        private object _signaledInstruction;

        public WaitForAnyComplete(params object[] instructions)
        {
            foreach(var obj in instructions)
            {
                this.Add(obj);
            }
        }

        /// <summary>
        /// The instruction that caused this WaitForAny to signal complete.
        /// </summary>
        public object SignaledInstruction
        {
            get { return _signaledInstruction; }
        }

        public void Add(object instruction)
        {
            _instructions.Push(new PairInfo()
            {
                Instruction = instruction,
                Handler = null
            });
        }

        protected override void SetSignal()
        {
            base.SetSignal();

            for(int i = 0; i < _instructions.Count; i++)
            {
                if(_instructions[i].Handler != null)
                {
                    _instructions[i].Handler.Cancel();
                    RadicalCoroutine.Release(_instructions[i].Handler);
                }
            }
            _instructions.Clear();
        }

        protected override bool TestIfComplete(out object yieldObject)
        {
            GameLoop.AssertMainThread();

            yieldObject = null;
            if (_instructions == null || _instructions.Count == 0)
            {
                this.SetSignal();
                return true;
            }

            if (_routine != null)
            {
                if (_routine.Finished)
                {
                    RadicalCoroutine.Release(ref _routine);
                    this.SetSignal();
                    return true;
                }
            }
            else
            {
                _signaledInstruction = null;

                PairInfo current;
                for (int i = 0; i < _instructions.Count; i++)
                {
                    current = _instructions[i];
                    if (current.Instruction == null)
                    {
                        _signaledInstruction = null;
                        _instructions.RemoveAt(i);
                        i--;
                        this.SetSignal();
                        return true;
                    }
                    else if (current.Instruction is IRadicalYieldInstruction ryi && ryi.IsComplete)
                    {
                        _signaledInstruction = current.Instruction;
                        _instructions.RemoveAt(i);
                        i--;
                        this.SetSignal();
                        return true;
                    }
                    else if(current.Handler == null)
                    {
                        current.Handler = GameLoop.Hook.StartRadicalCoroutine(WaitForInstruction(current.Instruction));
                        _instructions[i] = current;
                    }
                }

                if (_instructions.Count == 0)
                {
                    _signaledInstruction = null;
                    this.SetSignal();
                    return true;
                }

                return false;
            }

            return true;
        }

        private IEnumerator WaitForInstruction(object inst)
        {
            yield return inst;
            if(!this.SafeIsComplete)
            {
                _signaledInstruction = inst;
                this.SetSignal();
            }
        }

        private struct PairInfo
        {
            public object Instruction;
            public RadicalCoroutine Handler;
        }

    }

}
