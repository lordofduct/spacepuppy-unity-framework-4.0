using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Statistics
{

    public struct LedgerStatData
    {
        public string Stat;
        public string Token;
        public double? Value;
    }

    /// <summary>
    /// Track an amount for each stat.
    /// 
    /// If the stat is something where it can be broken up into individual unique amounts, include a token to track this. 
    /// For example if you're counting the number of documents read, include a token that identifies each individual document. 
    /// Now the GetStat(stat) is the total amount of times a document was read, GetStat(stat,token) is the total times that 
    /// doc was read, and 'CountStatTokens(stat)' is the number of times each doc was read uniquely.
    /// </summary>
    [System.Serializable]
    public class Ledger : ISerializable, System.ICloneable
    {

        public event System.EventHandler<LedgerChangedEventArgs> Changed;
        private void OnChanged(StatId stat)
        {
            this.Dirty = true;

            var d = Changed;
            if (d != null)
            {
                using (var ev = LedgerChangedEventArgs.Create(stat))
                {
                    d(this, ev);
                }
            }
        }

        private void OnChanged_Multi()
        {
            this.Dirty = true;

            var d = Changed;
            if (d != null)
            {
                using (var ev = LedgerChangedEventArgs.CreateMultipleChanged())
                {
                    d(this, ev);
                }
            }
        }

        #region Fields

        private Dictionary<StatId, double?> _stats = new Dictionary<StatId, double?>(StatIdComparer.Default);

        #endregion

        #region CONSTRUCTOR

        public Ledger()
        {

        }

        #endregion

        #region Properties

        /// <summary>
        /// If locked, then can only adjust stats which have been defined.
        /// If unlocked, and new stat adjusted will automatically be added.
        /// </summary>
        public bool Locked
        {
            get;
            set;
        }

        public double this[string stat]
        {
            get
            {
                return this.GetStatOrDefault(stat);
            }
            set
            {
                this.SetStat(stat, value);
            }
        }

        public double this[string stat, string token]
        {
            get
            {
                return this.GetStatOrDefault(stat, token);
            }
            set
            {
                this.SetStat(stat, token, value);
            }
        }

        /// <summary>
        /// A stat has been modified since the last time this flag was reset to false.
        /// </summary>
        public bool Dirty
        {
            get;
            set;
        }

        /// <summary>
        /// When serializing should we write null entries.
        /// </summary>
        public bool WriteNullEntries
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public void DefineStat(string stat, double? defaultValue = null)
        {
            var key = new StatId(stat);
            if (!_stats.ContainsKey(key))
            {
                _stats.Add(key, defaultValue);
                this.OnChanged(new StatId(stat, null));
            }
        }

        public double? GetStat(string stat) => GetStat(new StatId(stat, null));
        public double? GetStat(string stat, string token) => GetStat(new StatId(stat, token));
        public double? GetStat(StatId stat)
        {
            double? value = 0d;
            _stats.TryGetValue(stat, out value);
            return value;
        }

        public double GetStatOrDefault(string stat) => GetStatOrDefault(new StatId(stat, null));
        public double GetStatOrDefault(string stat, string token) => GetStatOrDefault(new StatId(stat, token));
        public double GetStatOrDefault(StatId stat)
        {
            double? value = null;
            _stats.TryGetValue(stat, out value);
            return value.GetValueOrDefault();
        }

        public bool GetStatAsBool(string stat) => GetStatAsBool(new StatId(stat, null));
        public bool GetStatAsBool(string stat, string token) => GetStatAsBool(new StatId(stat, token));
        public bool GetStatAsBool(StatId stat)
        {
            double? value = null;
            return _stats.TryGetValue(stat, out value) && value != null && value.Value != 0d;
        }

        public int CountStatTokens(string stat)
        {
            if (string.IsNullOrEmpty(stat)) return 0;

            int cnt = 0;
            var e = _stats.GetEnumerator();
            while (e.MoveNext())
            {
                var token = e.Current.Key;
                if (e.Current.Value != null && token.Stat == stat && token.Token != null)
                {
                    cnt++;
                }
            }
            return cnt;
        }

        /// <summary>
        /// Special count that lets you count up stats with a token that starts with something special.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="tokenStartsWith"></param>
        /// <returns></returns>
        public int CountStatTokens(string stat, string tokenStartsWith)
        {
            if (string.IsNullOrEmpty(tokenStartsWith)) return CountStatTokens(stat);
            if (string.IsNullOrEmpty(stat)) return 0;

            int cnt = 0;
            var e = _stats.GetEnumerator();
            while (e.MoveNext())
            {
                var token = e.Current.Key;
                if (e.Current.Value != null && token.Stat == stat && token.Token != null)
                {
                    if (token.Token.StartsWith(tokenStartsWith)) cnt++;
                }
            }
            return cnt;
        }

        /// <summary>
        /// Should only be used for resetting stats, such as from a reload.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="value"></param>
        public bool SetStat(string stat, double? value)
        {
            if (string.IsNullOrEmpty(stat)) return false;

            var token = new StatId(stat);
            if (this.Locked && !_stats.ContainsKey(token)) return false;

            _stats[token] = value;
            this.OnChanged(new StatId(stat, null));
            return true;
        }

        /// <summary>
        /// Should only be used for resetting stats, such as from a reload.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="value"></param>
        public bool SetStat(string stat, string token, double? value, bool recalcParentStat = false) => SetStat(new StatId(stat, token), value, recalcParentStat);
        public bool SetStat(StatId stat, double? value, bool recalcParentStat = false)
        {
            if (string.IsNullOrEmpty(stat.Stat)) return false;
            if (string.IsNullOrEmpty(stat.Token)) return this.SetStat(stat.Stat, value);

            var topkey = new StatId(stat.Stat);
            if (this.Locked && !_stats.ContainsKey(topkey)) return false;

            if (recalcParentStat)
            {
                _stats[stat] = value;
                _stats[topkey] = SumStatTokens(stat.Stat);
            }
            else
            {
                if (!_stats.ContainsKey(topkey)) _stats[topkey] = null;
                _stats[stat] = value;
            }
            this.OnChanged(stat);
            return true;
        }

        /// <summary>
        /// Should only be used for resetting stats, such as from a reload.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="value"></param>
        public bool SetStat(string stat, bool value)
        {
            if (string.IsNullOrEmpty(stat)) return false;

            var key = new StatId(stat);
            if (this.Locked && !_stats.ContainsKey(key)) return false;

            if (value)
                _stats[key] = 1d;
            else
                _stats[key] = 0d;
            this.OnChanged(new StatId(stat, null));
            return true;
        }

        /// <summary>
        /// Sets the stat to null and removes any tokens associated with it.
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public bool ClearStat(string stat)
        {
            if (string.IsNullOrEmpty(stat)) return false;

            var topkey = new StatId(stat);
            if (!_stats.ContainsKey(topkey)) return false;

            _stats[topkey] = null;
            using (var set = TempCollection.GetSet<StatId>())
            {
                foreach (var key in _stats.Keys)
                {
                    if (key.Stat == topkey.Stat && !string.IsNullOrEmpty(key.Token))
                    {
                        set.Add(key);
                    }
                }

                if (set.Count > 0)
                {
                    foreach (var key in set)
                    {
                        _stats.Remove(key);
                    }
                }
            }
            this.OnChanged(new StatId(stat, null));
            return true;
        }

        /// <summary>
        /// Deletes the stat completely and removes any tokens associated with it. Similar to Clear, but instead of nulling the stat, it's removed completely.
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public bool DeleteStat(string stat)
        {
            if (string.IsNullOrEmpty(stat)) return false;

            var topkey = new StatId(stat);
            if (!_stats.ContainsKey(topkey)) return false;

            _stats.Remove(topkey);
            using (var set = TempCollection.GetSet<StatId>())
            {
                foreach (var key in _stats.Keys)
                {
                    if (key.Stat == topkey.Stat)
                    {
                        set.Add(key);
                    }
                }

                if (set.Count > 0)
                {
                    foreach (var key in set)
                    {
                        _stats.Remove(key);
                    }
                }
            }
            this.OnChanged(new StatId(stat, null));
            return true;
        }

        /// <summary>
        /// Adjust a recorded stat by some amount.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="amount"></param>
        /// <param name="token">A token for breaking up the stat into parts, see the class descrition for more.</param>
        /// <returns></returns>
        public bool AdjustStat(string stat, double amount, string token = null) => AdjustStat(new StatId(stat, token), amount);
        public bool AdjustStat(StatId stat, double amount)
        {
            if (string.IsNullOrEmpty(stat.Stat)) return false;

            var topkey = new StatId(stat.Stat);
            bool result = false;
            //standard stat adjustment
            double? current;
            if (_stats.TryGetValue(topkey, out current))
            {
                if (current != null)
                    _stats[topkey] = current + amount;
                else
                    _stats[topkey] = amount;
                result = true;
            }
            else if (!this.Locked)
            {
                _stats[topkey] = amount;
                result = true;
            }
            else
            {
                return false;
            }

            //adjust for token if any
            if (!string.IsNullOrEmpty(stat.Token))
            {
                //set join value
                if (_stats.TryGetValue(stat, out current) && current != null)
                    current += amount;
                else
                    current = amount;
                _stats[stat] = current;
            }

            if (result)
            {
                this.OnChanged(stat);
            }
            return result;
        }

        public IEnumerable<string> GetStatNames()
        {
            return _stats.Keys.Select(o => o.Stat).Distinct();
        }

        public IEnumerable<LedgerStatData> GetAllStatAndTokenEntries() => _stats.Select(o => o.Key.CreateData(o.Value));

        /// <summary>
        /// Returns all stats including token entries for a give stat.
        /// </summary>
        /// <param name="statPrefix"></param>
        /// <returns></returns>
        public IEnumerable<LedgerStatData> GetStatAndTokenEntries(string stat)
        {
            if (string.IsNullOrEmpty(stat)) yield break;

            foreach (var pair in _stats)
            {
                if (pair.Key.Stat == stat)
                {
                    yield return pair.Key.CreateData(pair.Value);
                }
            }
        }

        /// <summary>
        /// Copies state of another ledger onto itself.
        /// </summary>
        /// <param name="ledger"></param>
        public void Copy(Ledger ledger)
        {
            if (ledger == null) throw new System.ArgumentNullException("ledger");

            _stats.Clear();
            foreach (var pair in ledger._stats)
            {
                _stats.Add(pair.Key, pair.Value);
            }
        }

        public void CopyStat(Ledger ledger, string stat)
        {
            if (string.IsNullOrEmpty(stat)) return;

            this.ClearStat(stat);
            foreach (var pair in ledger.GetStatAndTokenEntries(stat))
            {
                this.SetStat(pair.Stat, pair.Token, pair.Value);
            }
        }

        public void Reset()
        {
            if (this.Locked)
            {
                var arr = _stats.Keys.ToArray();
                foreach (var id in arr)
                {
                    if (!string.IsNullOrEmpty(id.Token))
                    {
                        _stats.Remove(id);
                    }
                    else
                    {
                        _stats[id] = null;
                    }
                }
                this.OnChanged_Multi();
            }
            else
            {
                _stats.Clear();
                this.OnChanged_Multi();
            }
        }



        private double SumStatTokens(string stat)
        {
            double total = 0d;
            var e = _stats.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Key.Stat == stat && !string.IsNullOrEmpty(e.Current.Key.Token) && e.Current.Value != null)
                {
                    total += e.Current.Value.Value;
                }
            }
            return total;
        }

        #endregion

        #region ICloneable Interface

        public Ledger Clone()
        {
            var l = new Ledger();
            l.Copy(this);
            return l;
        }

        object System.ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion

        #region ISerializable Interface

        protected Ledger(SerializationInfo info, StreamingContext context)
        {
            _stats.Clear();
            foreach (var pair in info)
            {
                _stats[StatId.Parse(pair.Name)] = pair.Value != null ? (double?)ConvertUtil.ToDouble(pair.Value) : null;
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (var pair in _stats)
            {
                if (this.WriteNullEntries || pair.Value != null)
                {
                    info.AddValue(pair.Key.ToString(), pair.Value);
                }
            }
        }

        #endregion

    }

    public sealed class LedgerChangedEventArgs : System.EventArgs, System.IDisposable
    {

        #region CONSTRUCTOR

        private LedgerChangedEventArgs() { }

        #endregion

        #region Properties

        public bool MultipleChanged { get; private set; }

        private StatId _statid;
        public StatId StatId => _statid;
        public string Stat => _statid.Stat;
        public string Token => _statid.Token;

        #endregion

        #region Methods

        void System.IDisposable.Dispose()
        {
            Release(this);
        }

        #endregion

        #region Static Methods

        private static ObjectCachePool<LedgerChangedEventArgs> _pool = new ObjectCachePool<LedgerChangedEventArgs>(16, () => new LedgerChangedEventArgs());

        public static LedgerChangedEventArgs Create(StatId stat)
        {
            var ev = _pool.GetInstance();
            ev.MultipleChanged = false;
            ev._statid = stat;
            return ev;
        }

        public static LedgerChangedEventArgs Create(string stat, string token = null)
        {
            var ev = _pool.GetInstance();
            ev.MultipleChanged = false;
            ev._statid = new StatId(stat, token);
            return ev;
        }

        public static LedgerChangedEventArgs CreateMultipleChanged()
        {
            var ev = _pool.GetInstance();
            ev.MultipleChanged = true;
            ev._statid = default;
            return ev;
        }

        public static void Release(LedgerChangedEventArgs ev)
        {
            if (ev == null) return;

            ev.MultipleChanged = false;
            ev._statid = default;
            _pool.Release(ev);
        }

        #endregion

    }

}
