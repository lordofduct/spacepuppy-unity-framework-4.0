using System;
using System.Collections;
using System.Collections.Generic;
using com.spacepuppy.Collections;

namespace com.spacepuppy.StateMachine
{

    public class StateStack<T> : IStateStack<T> where T : class
    {

        #region Fields

        private IEqualityComparer<T> _comparer;
        private Stack<T> _stack = new Stack<T>();
        private T _current;
        private Func<T> _getInitialState;

        private int _silentTransactionCounter = -1;
        private T _silentTransactionFrom;
        private T _silentTransactionTo;

        #endregion

        #region CONSTRUCTOR

        public StateStack(T initialState, bool allowEmptyStack = false, IEqualityComparer<T> comparer = default)
        {
            if(allowEmptyStack)
            {
                _current = initialState;
                _getInitialState = null;
                _stack.Push(initialState);
            }
            else
            {
                if (initialState == null) throw new ArgumentNullException(nameof(initialState));

                _current = initialState;
                _getInitialState = () => initialState;
            }
            this.Comparer = comparer;
        }

        public StateStack(Func<T> initialStateReceiver, IEqualityComparer<T> comparer = default)
        {
            if (initialStateReceiver == null) throw new ArgumentNullException(nameof(initialStateReceiver));

            _current = initialStateReceiver();
            if (object.ReferenceEquals(_current, null)) throw new InvalidOperationException("Initial state receiver entered a faulted state.");

            _getInitialState = initialStateReceiver;
            this.Comparer = comparer;
        }

        #endregion

        #region Properties

        public IEqualityComparer<T> Comparer
        {
            get { return _comparer; }
            set { _comparer = value ?? EqualityComparer<T>.Default; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reset what the initial state is.
        /// </summary>
        /// <param name="state">The state to replace initial state with.</param>
        /// <param name="preserveStack">True if the override stack should stay in place.</param>
        public void Reset(T state, bool preserveStack = false)
        {
            if (_comparer.Equals(state, null)) throw new ArgumentNullException(nameof(state));

            _getInitialState = () => state;
            if (preserveStack) return;
            
            if(_stack.Count == 0 && !_comparer.Equals(_current, state))
            {
                var c = _current;
                _current = state;
                OnStateChanged(c, state);
            }
            else
            {
                var c = _current;
                _current = state;
                _stack.Clear();
                OnStateChanged(c, _current);
            }
        }

        /// <summary>
        /// Reset what the initial state is.
        /// </summary>
        /// <param name="initialStateReceiver">The state receiver callback called to get the initial state.</param>
        /// <param name="preserveStack">True if the override stack should stay in place.</param>
        public void Reset(Func<T> initialStateReceiver, bool preserveStack = false)
        {
            if (initialStateReceiver == null) throw new ArgumentNullException(nameof(initialStateReceiver));

            _getInitialState = initialStateReceiver;
            if (preserveStack) return;

            var next = _getInitialState();
            if (_comparer.Equals(next, null)) throw new InvalidOperationException("Initial state receiver entered a faulted state.");

            if (_stack.Count == 0 && !_comparer.Equals(_current, next))
            {
                var c = _current;
                _current = next;
                OnStateChanged(c, next);
            }
            else
            {
                var c = _current;
                _current = next;
                _stack.Clear();
                OnStateChanged(c, _current);
            }
        }

        /// <summary>
        /// Removes the top-most instance of a state in the override statck, ignoring the initial state.
        /// </summary>
        /// <param name="state"></param>
        public bool PopState(T state)
        {
            if (_comparer.Equals(state, null)) return false;
            if (_stack.Count == 0) return false;

            int index = -1;
            int cnt = 0;
            var e = _stack.GetEnumerator();
            while (e.MoveNext())
            {
                if (_comparer.Equals(e.Current, state)) index = cnt;
                cnt++;
            }
            if (index < 0) return false;

            using (var lst = TempCollection.GetList<T>())
            {
                while(_stack.Count > index)
                {
                    lst.Add(_stack.Pop());
                }
                for(int i = lst.Count - 1; i >= 0; i--)
                {
                    _stack.Push(lst[i]);
                }

                if (_comparer.Equals(_current, state))
                {
                    if (_getInitialState == null)
                    {
                        _current = _stack.Count > 0 ? _stack.Peek() : null;
                        OnStateChanged(state, _current);
                    }
                    else
                    {
                        var next = _stack.Count > 0 ? _stack.Peek() : _getInitialState();
                        if (_comparer.Equals(next, null)) throw new InvalidOperationException("Initial state receiver entered a faulted state.");
                        _current = next;
                        OnStateChanged(state, _current);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Searches the state stack for a state and purges it no matter its location. Unless its the initial state.
        /// </summary>
        /// <param name="state"></param>
        public bool PurgeState(T state)
        {
            if (_comparer.Equals(state, null)) return false;
            if (_stack.Count == 0) return false;

            using (var lst = TempCollection.GetList<T>())
            {
                bool found = false;
                var e = _stack.GetEnumerator();
                while(e.MoveNext())
                {
                    if(_comparer.Equals(e.Current, state))
                    {
                        found = true;
                    }
                    else
                    {
                        lst.Add(e.Current);
                    }
                }

                if(found)
                {
                    _stack.Clear();
                    for(int i = 0; i < lst.Count; i++)
                    {
                        _stack.Push(lst[i]);
                    }

                    if(_comparer.Equals(_current, state))
                    {
                        if(_getInitialState == null)
                        {
                            _current = _stack.Count > 0 ? _stack.Peek() : null;
                            OnStateChanged(state, _current);
                        }
                        else
                        {
                            var next = _stack.Count > 0 ? _stack.Peek() : _getInitialState();
                            if (_comparer.Equals(next, null)) throw new InvalidOperationException("Initial state receiver entered a faulted state.");
                            _current = next;
                            OnStateChanged(state, _current);
                        }
                    }
                }
                return found;
            }
        }

        public bool PurgeStates(IEnumerable<T> states)
        {
            if (states == null) return false;
            if (_stack.Count == 0) return false;

            using (var set = TempCollection.GetSet<T>(states, _comparer))
            {
                if (set.Count == 0) return false;

                using (var lst = TempCollection.GetList<T>())
                {
                    bool found = false;
                    var e = _stack.GetEnumerator();
                    while (e.MoveNext())
                    {
                        if (set.Contains(e.Current))
                        {
                            found = true;
                        }
                        else
                        {
                            lst.Add(e.Current);
                        }
                    }

                    if (found)
                    {
                        _stack.Clear();
                        for (int i = 0; i < lst.Count; i++)
                        {
                            _stack.Push(lst[i]);
                        }

                        if (set.Contains(_current))
                        {
                            var c = _current;
                            if (_getInitialState == null)
                            {
                                _current = _stack.Count > 0 ? _stack.Peek() : null;
                                OnStateChanged(c, _current);
                            }
                            else
                            {
                                var next = _stack.Count > 0 ? _stack.Peek() : _getInitialState();
                                if (_comparer.Equals(next, null)) throw new InvalidOperationException("Initial state receiver entered a faulted state.");
                                _current = next;
                                OnStateChanged(c, _current);
                            }
                        }
                    }
                    return found;
                }
            }
        }

        public void BeginSilentTransaction()
        {
            if (_silentTransactionCounter < 0) _silentTransactionCounter = 0;
        }

        public void EndSilentTransaction()
        {
            var silentTransactionCount = _silentTransactionCounter;
            var from = _silentTransactionFrom;
            var to = _silentTransactionTo;
            _silentTransactionCounter = -1;

            if (silentTransactionCount > 0)
            {
                StateChanged?.Invoke(this, new StateChangedEventArgs<T>(from, to));
            }
        }

        protected void OnStateChanged(T from, T to)
        {
            if(_silentTransactionCounter < 0)
            {
                StateChanged?.Invoke(this, new StateChangedEventArgs<T>(from, to));
            }
            else
            {
                if (_silentTransactionCounter == 0) _silentTransactionFrom = from;
                _silentTransactionTo = to;
                _silentTransactionCounter++;
            }
        }

        #endregion

        #region IStateStack Interface

        public event StateChangedEventHandler<T> StateChanged;

        public int Count => _getInitialState != null ? _stack.Count + 1 : _stack.Count;

        public T Current => _current;

        public bool Contains(T state)
        {
            if (_comparer.Equals(state, null)) return false;
            if (_comparer.Equals(_current, state)) return true;
            return _stack.Contains(state);
        }

        public void PushState(T state)
        {
            if (_comparer.Equals(state, null)) throw new ArgumentNullException(nameof(state));

            var c = _current;
            _current = state;
            _stack.Push(state);
            OnStateChanged(c, state);
        }

        public T PopState()
        {
            var c = _current;
            if(_stack.Count > 1)
            {
                _stack.Pop();
                _current = _stack.Peek();
                OnStateChanged(c, _current);
            }
            else
            {
                if (_getInitialState == null) //we allow empty stacks
                {
                    if (_stack.Count == 0) return _current;

                    _stack.Clear();
                    _current = null;
                    OnStateChanged(c, null);
                }
                else
                {
                    var next = _getInitialState();
                    if (_comparer.Equals(next, null)) throw new InvalidOperationException("Initial state receiver entered a faulted state.");
                    if (_stack.Count == 0 && _comparer.Equals(c, next)) return _current; //already at bottom, don't change

                    _stack.Clear();
                    _current = next;
                    OnStateChanged(c, next);
                }
            }

            return _current;
        }

        public void PopAllStates()
        {
            var c = _current;
            if(_getInitialState == null) //we allow empty stacks
            {
                if (_stack.Count == 0) return;

                _stack.Clear();
                _current = null;
                OnStateChanged(c, null);
            }
            else
            {
                var next = _getInitialState();
                if (_comparer.Equals(next, null)) throw new InvalidOperationException("Initial state receiver entered a faulted state.");
                if (_stack.Count == 0 && _comparer.Equals(c, next)) return; //already at bottom, don't change

                _stack.Clear();
                _current = next;
                OnStateChanged(c, next);
            }
        }

        public int Enumerate(ICollection<T> coll)
        {
            int cnt = 0;
            var e = _stack.GetEnumerator();
            while(e.MoveNext())
            {
                cnt++;
                coll.Add(e.Current);
            }
            return cnt;
        }

        public int Enumerate(Action<T> callback)
        {
            if(callback != null)
            {
                var e = _stack.GetEnumerator();
                while (e.MoveNext())
                {
                    callback(e.Current);
                }
            }
            return _stack.Count;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _stack.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _stack.GetEnumerator();
        }

        #endregion

    }

}