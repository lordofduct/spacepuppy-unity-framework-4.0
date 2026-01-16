using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween
{

    /// <remarks>
    /// At this time sequences do not support 'reverse'. 
    /// 
    /// If/When we upgrade this we need to go into the TweenHash.Create method and make sure it sets it up this sequence correctly.
    /// </remarks>
    public class TweenSequence : Tweener
    {

        #region Fields

        private TweenSequenceCollection _sequence;
        private int _currentIndex = -1;
        private float _currentStartT;
        private float _currentEndT;

        #endregion

        #region CONSTRUCTOR

        public TweenSequence(object id)
        {
            this.Id = id;
            _sequence = new TweenSequenceCollection(this);
        }

        #endregion

        #region Properties

        public override object Id { get; set; }

        public TweenSequenceCollection Tweens { get { return _sequence; } }

        #endregion

        #region Tweener Interface

        public override void Play(float playHeadPosition)
        {
            base.Play(playHeadPosition);
        }

        protected internal override bool GetTargetIsDestroyed()
        {
            for (int i = 0; i < _sequence.Count; i++)
            {
                if (_sequence[i].GetTargetIsDestroyed()) return true;
            }

            return false;
        }

        protected internal override float GetPlayHeadLength()
        {
            float dur = 0f;
            var e = _sequence.GetEnumerator();
            while (e.MoveNext())
            {
                dur += e.Current.TotalTime + e.Current.Delay;
            }
            return dur;
        }

        protected internal override void DoUpdate(float dt, float t)
        {
            if (_sequence.Count == 0 || _currentIndex >= _sequence.Count)
            {
                return;
            }

            float totalTime = 0f;
            float subt = 0f;
            Tweener current;
            if (_currentIndex < 0)
            {
                _currentStartT = 0f;
                for (int i = 0; i < _sequence.Count; i++)
                {
                    current = _sequence[i];
                    totalTime = current.TotalTime + current.Delay;
                    if (t < _currentStartT + totalTime)
                    {
                        _currentIndex = i;
                        _currentEndT = _currentStartT + totalTime;
                        subt = t - _currentStartT;
                        if (subt > current.Delay)
                        {
                            current.DoUpdate(dt, t - (_currentStartT + current.Delay));
                        }
                        break;
                    }
                    else
                    {
                        _currentStartT += totalTime;
                        _currentEndT = _currentStartT;
                    }
                }
            }
            else
            {
                current = _sequence[_currentIndex];
                if (t >= _currentEndT)
                {
                    float dt0 = System.Math.Max(0f, _currentEndT - (t - dt));
                    float dt1 = System.Math.Max(0f, t - _currentEndT);
                    current.DoUpdate(dt0, current.TotalTime);

                    _currentIndex++;
                    _currentStartT = _currentEndT;
                    if (_currentIndex < _sequence.Count)
                    {
                        current = _sequence[_currentIndex];
                        _currentEndT = _currentStartT + current.TotalTime + current.Delay;
                        subt = t - _currentStartT;
                        if (subt > current.Delay)
                        {
                            current.DoUpdate(dt1, t - (_currentStartT + current.Delay));
                        }
                    }
                    else
                    {
                        current = null;
                        _currentEndT = _currentStartT;
                    }
                }
                else
                {
                    subt = t - _currentStartT;
                    if (subt > current.Delay)
                    {
                        current.DoUpdate(dt, t - (_currentStartT + current.Delay));
                    }
                }
            }
        }

        #endregion

        #region Special Types

        public class TweenSequenceCollection : IList<Tweener>
        {
            private TweenSequence _owner;
            private List<Tweener> _lst = new List<Tweener>();

            internal TweenSequenceCollection(TweenSequence owner)
            {
                _owner = owner;
            }

            #region IList Interface

            public int IndexOf(Tweener item)
            {
                return _lst.IndexOf(item);
            }

            public void Insert(int index, Tweener item)
            {
                if (item == null) throw new System.ArgumentNullException("item");


                _lst.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                if (_owner.IsPlaying) throw new System.InvalidOperationException("Cannot modify TweenSequence while it is playing.");

                _lst.RemoveAt(index);
            }

            public Tweener this[int index]
            {
                get
                {
                    return _lst[index];
                }
                set
                {
                    if (_lst[index] == value) return;

                    if (_owner.IsPlaying) throw new System.InvalidOperationException("Cannot modify TweenSequence while it is playing.");
                    _lst[index] = value;
                }
            }

            public void Add(Tweener item)
            {
                if (_owner.IsPlaying) throw new System.InvalidOperationException("Cannot modify TweenSequence while it is playing.");

                _lst.Add(item);
            }

            public void Clear()
            {
                if (_owner.IsPlaying) throw new System.InvalidOperationException("Cannot modify TweenSequence while it is playing.");

                _lst.Clear();
            }

            public bool Contains(Tweener item)
            {
                return _lst.Contains(item);
            }

            public void CopyTo(Tweener[] array, int arrayIndex)
            {
                _lst.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return _lst.Count; }
            }

            public bool IsReadOnly
            {
                get { return _owner.IsPlaying; }
            }

            public bool Remove(Tweener item)
            {
                if (_owner.IsPlaying) throw new System.InvalidOperationException("Cannot modify TweenSequence while it is playing.");

                return _lst.Remove(item);
            }

            #endregion

            #region IEnumerable Interface

            public Enumerator GetEnumerator()
            {
                return new Enumerator(this);
            }

            IEnumerator<Tweener> IEnumerable<Tweener>.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion

            #region Special Types

            public struct Enumerator : IEnumerator<Tweener>
            {

                #region Fields

                private List<Tweener>.Enumerator _e;

                #endregion

                #region CONSTRUCTOR

                public Enumerator(TweenSequenceCollection coll)
                {
                    if (coll == null) throw new System.ArgumentNullException("coll");
                    _e = coll._lst.GetEnumerator();
                }

                #endregion

                #region IEnumerator Interface

                public Tweener Current
                {
                    get
                    {
                        return _e.Current;
                    }
                }

                object IEnumerator.Current
                {
                    get
                    {
                        return _e.Current;
                    }
                }

                public void Dispose()
                {
                    _e.Dispose();
                }

                public bool MoveNext()
                {
                    return _e.MoveNext();
                }

                void IEnumerator.Reset()
                {
                    (_e as IEnumerator).Reset();
                }

                #endregion

            }

            #endregion

        }

        #endregion

    }

    /// <remarks>
    /// At this time sequences do not support 'reverse'. 
    /// 
    /// If/When we upgrade this we need to go into the TweenHash.Create method and make sure it sets it up this sequence correctly.
    /// </remarks>
    public class FollowOnTweenSequence : Tweener
    {

        #region Fields

        private Deque<TweenHash> _sequence = new();
        private Tweener _current;
        private float _abandonedDurations;

        #endregion

        #region CONSTRUCTOR

        public FollowOnTweenSequence(object id)
        {
            this.Id = id;
        }

        #endregion

        #region Properties

        public override object Id { get; set; }

        #endregion

        #region Methods

        public void Prepend(TweenHash hash)
        {
            if (hash == null) throw new System.ArgumentNullException(nameof(hash));
            _sequence.Unshift(hash);
        }

        public void Append(TweenHash hash)
        {
            if (hash == null) throw new System.ArgumentNullException(nameof(hash));
            _sequence.Push(hash);
        }

        #endregion

        #region Tweener Interface

        protected internal override bool GetTargetIsDestroyed()
        {
            if (_current != null) return _current.GetTargetIsDestroyed();
            if (_sequence.Count == 0) return false;

            return _sequence.PeekShift().Target.IsNullOrDestroyed();
        }

        protected internal override float GetPlayHeadLength()
        {
            float dur = _abandonedDurations;
            if (_current != null) dur += _current.GetPlayHeadLength();

            var e = _sequence.GetEnumerator();
            while (e.MoveNext())
            {
                dur += e.Current.EstimateDuration();
            }
            return dur;
        }

        protected internal override void DoUpdate(float dt, float t)
        {
            if (_current == null)
            {
                if (_sequence.Count == 0) return;

                _current = _sequence.Shift().Create();
            }
            else if (_current.IsComplete || _current.IsDead)
            {
                float f = _current.GetPlayHeadLength();
                if (float.IsFinite(f)) _abandonedDurations += f;

                dt += t - _abandonedDurations;
                if (_sequence.Count == 0)
                {
                    _current = null;
                    return;
                }

                _current = _sequence.Shift().Create();
            }

            if (_current != null)
            {
                _current.Scrub(dt);
            }
        }

        #endregion

    }

}
