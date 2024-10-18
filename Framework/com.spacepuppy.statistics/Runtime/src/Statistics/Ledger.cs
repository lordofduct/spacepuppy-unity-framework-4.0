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
        public string MetaData;
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
        public void SignalOnChanged(StatId stat)
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

        public void SignalOnChanged_Multi()
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
        private Dictionary<string, IStatModifier> _modifiers = new();
        private IStatModifier _defaultModifier;

        #endregion

        #region CONSTRUCTOR

        public Ledger()
        {
            _defaultModifier = StandardStatModifier.Default;
        }

        public Ledger(IStatModifier defaultmodifier)
        {
            _defaultModifier = defaultmodifier ?? StandardStatModifier.Default;
        }

        #endregion

        #region Properties

        public IStatModifier DefaultStatModifier
        {
            get => _defaultModifier;
            set => _defaultModifier = value ?? StandardStatModifier.Default;
        }

        /// <summary>
        /// If locked, then can only adjust stats which have been defined.
        /// If unlocked, and new stat adjusted will automatically be added.
        /// </summary>
        public bool Locked
        {
            get;
            set;
        }

        public double this[string category]
        {
            get
            {
                return this.GetStatOrDefault(category);
            }
            set
            {
                this.SetStat(category, value);
            }
        }

        public double this[string category, string token]
        {
            get
            {
                return this.GetStatOrDefault(category, token);
            }
            set
            {
                this.SetStat(category, token, value);
            }
        }

        public double this[StatId stat]
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

        public bool Contains(string category) => _stats.ContainsKey(new StatId(category)) || _stats.Keys.Contains(new StatId(category), StatIdCategoryComparer.Default);
        public bool Contains(StatId stat) => _stats.ContainsKey(stat);

        public void DefineCategory(string category, double? defaultValue = null)
        {
            var key = new StatId(category);
            if (!_stats.ContainsKey(key))
            {
                _stats.Add(key, defaultValue);
                this.SignalOnChanged(new StatId(category, null));
            }
        }

        public void SetCategoryModifier(string category, IStatModifier modifier)
        {
            if (string.IsNullOrEmpty(category)) throw new System.ArgumentException("Category must not be null or empty.", nameof(category));

            if (modifier == null)
            {
                _modifiers.Remove(category);
            }
            else
            {
                _modifiers[category] = modifier;
            }
        }
        public IStatModifier GetCategoryModifier(string category)
        {
            if (string.IsNullOrEmpty(category)) throw new System.ArgumentException("Category must not be null or empty.", nameof(category));

            if (_modifiers.TryGetValue(category, out IStatModifier result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public bool TryGetStat(string category, out double? value) => this.GetOperator(category).TryGetStat(this, new StatId(category), out value);
        public bool TryGetStat(string category, string token, out double? value) => this.GetOperator(category).TryGetStat(this, new StatId(category, token), out value);
        public bool TryGetStat(StatId stat, out double? value) => this.GetOperator(stat.Category).TryGetStat(this, stat, out value);

        public double? GetStat(string category) => this.GetOperator(category).TryGetStat(this, new StatId(category, null), out double? result) ? result : null;
        public double? GetStat(string category, string token) => this.GetOperator(category).TryGetStat(this, new StatId(category, token), out double? result) ? result : null;
        public double? GetStat(StatId stat) => this.GetOperator(stat.Category).TryGetStat(this, stat, out double? result) ? result : null;

        public double GetStatOrDefault(string category) => this.GetOperator(category).TryGetStat(this, new StatId(category, null), out double? result) ? result.GetValueOrDefault() : 0d;
        public double GetStatOrDefault(string category, string token) => this.GetOperator(category).TryGetStat(this, new StatId(category, token), out double? result) ? result.GetValueOrDefault() : 0d;
        public double GetStatOrDefault(StatId stat) => this.GetOperator(stat.Category).TryGetStat(this, stat, out double? result) ? result.GetValueOrDefault() : 0d;

        public bool GetStatAsBool(string category) => this.GetOperator(category).TryGetStat(this, new StatId(category, null), out double? result) && result != null && result.Value != 0d;
        public bool GetStatAsBool(string category, string token) => this.GetOperator(category).TryGetStat(this, new StatId(category, token), out double? result) && result != null && result.Value != 0d;
        public bool GetStatAsBool(StatId stat) => this.GetOperator(stat.Category).TryGetStat(this, stat, out double? result) && result != null && result.Value != 0d;

        public int CountStatTokens(string category)
        {
            if (string.IsNullOrEmpty(category)) return 0;

            int cnt = 0;
            var e = _stats.GetEnumerator();
            while (e.MoveNext())
            {
                var token = e.Current.Key;
                if (e.Current.Value != null && token.Category == category && token.Token != null)
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
        public int CountStatTokens(string category, string tokenStartsWith)
        {
            if (string.IsNullOrEmpty(tokenStartsWith)) return CountStatTokens(category);
            if (string.IsNullOrEmpty(category)) return 0;

            int cnt = 0;
            var e = _stats.GetEnumerator();
            while (e.MoveNext())
            {
                var token = e.Current.Key;
                if (e.Current.Value != null && token.Category == category && token.Token != null)
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
        public bool SetStat(string category, double? value) => this.GetOperator(category).SetStat(this, new StatId(category, null), value);
        public bool SetStat(string category, string token, double? value, bool recalcParentStat = false) => this.GetOperator(category).SetStat(this, new StatId(category, token), value);
        public bool SetStat(StatId stat, double? value, bool recalcParentStat = false) => this.GetOperator(stat.Category).SetStat(this, stat, value);

        /// <summary>
        /// Should only be used for resetting stats, such as from a reload.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="value"></param>
        public bool SetStat(string category, bool value) => this.SetStat(new StatId(category, null), value ? 1d : 0d);

        /// <summary>
        /// Sets the stat to null and removes any tokens associated with it.
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public bool ClearStat(string category) => this.GetOperator(category).ClearStat(this, new StatId(category, null));
        public bool ClearStat(StatId stat) => this.GetOperator(stat.Category).ClearStat(this, stat);

        public bool ClearCategory(string category)
        {
            if (string.IsNullOrEmpty(category)) return false;

            var topkey = new StatId(category);
            bool success = false;
            if (_stats.ContainsKey(topkey))
            {
                _stats[topkey] = null;
                success = true;
            }
            using (var set = TempCollection.GetSet<StatId>())
            {
                foreach (var key in _stats.Keys)
                {
                    if (key.Category == topkey.Category && !string.IsNullOrEmpty(key.Token))
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
                    success = true;
                }
            }
            if (success) this.SignalOnChanged(topkey);
            return success;
        }

        /// <summary>
        /// Deletes the stat completely and removes any tokens associated with it. Similar to Clear, but instead of nulling the stat, it's removed completely.
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public bool DeleteStat(string category) => this.GetOperator(category).DeleteStat(this, new StatId(category, null));
        public bool DeleteStat(StatId stat) => this.GetOperator(stat.Category).DeleteStat(this, stat);

        public bool DeleteCategory(string category)
        {
            var topkey = new StatId(category);
            bool success = _stats.Remove(topkey);
            using (var set = TempCollection.GetSet<StatId>())
            {
                foreach (var key in _stats.Keys)
                {
                    if (key.Category == topkey.Category)
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
                    success = true;
                }
            }
            if (success) this.SignalOnChanged(topkey);
            return success;
        }


        /// <summary>
        /// Adjust a recorded stat by some amount.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="amount"></param>
        /// <param name="token">A token for breaking up the stat into parts, see the class descrition for more.</param>
        /// <returns></returns>
        public bool AdjustStat(string category, double amount, string token = null) => this.GetOperator(category).AdjustStat(this, new StatId(category, token), amount);
        public bool AdjustStat(StatId stat, double amount) => this.GetOperator(stat.Category).AdjustStat(this, stat, amount);

        public IEnumerable<string> GetCategoryNames()
        {
            return _stats.Keys.Select(o => o.Category).Distinct();
        }

        public IEnumerable<LedgerStatData> GetAllStatTokenEntries() => _stats.Select(o => o.Key.CreateData(o.Value));

        /// <summary>
        /// Returns all tokens for a given category
        /// </summary>
        /// <param name="statPrefix"></param>
        /// <returns></returns>
        public IEnumerable<LedgerStatData> GetStatTokenEntries(string category)
        {
            if (string.IsNullOrEmpty(category)) yield break;

            foreach (var pair in _stats)
            {
                if (pair.Key.Category == category)
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

        public void CopyCategory(Ledger ledger, string category)
        {
            if (string.IsNullOrEmpty(category)) return;

            this.ClearCategory(category);
            foreach (var pair in ledger.GetStatTokenEntries(category))
            {
                this.SetStat(pair.Stat, pair.Token, pair.Value);
            }
        }

        /// <summary>
        /// Resets the entire ledger to an empty state conserving locked entries.
        /// </summary>
        public void Reset()
        {
            if (this.Locked)
            {
                using (var set = TempCollection.GetSet<StatId>(_stats.Keys))
                {
                    foreach (var id in set)
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
                }
                this.SignalOnChanged_Multi();
            }
            else
            {
                _stats.Clear();
                this.SignalOnChanged_Multi();
            }
        }


        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private IStatModifier GetOperator(string stat)
        {
            return !string.IsNullOrEmpty(stat) && _modifiers.TryGetValue(stat, out IStatModifier result) ? result : _defaultModifier;
        }

        #endregion

        #region Raw Access

        public double? GetStatRaw(StatId stat)
        {
            if (_stats.TryGetValue(stat, out double? value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        public bool TryGetStatRaw(StatId stat, out double? value)
        {
            return _stats.TryGetValue(stat, out value);
        }

        /// <summary>
        /// This should only ever be called from within a IStatModifier for raw modification based on that modifiers rules. Use at your own risk.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetStatRaw(StatId stat, double? value)
        {
            if (string.IsNullOrEmpty(stat.Category)) return false;
            _stats[stat] = value;
            return true;
        }

        public bool RemoveStatRaw(StatId stat)
        {
            return _stats.Remove(stat);
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

        #region Stat Operators

        /// <summary>
        /// When implementing one must call 'SignalOnChanged' manually.
        /// </summary>
        public interface IStatModifier
        {
            bool AdjustStat(Ledger ledger, StatId stat, double amount);
            bool ClearStat(Ledger ledger, StatId stat);
            bool DeleteStat(Ledger ledger, StatId stat);
            bool SetStat(Ledger ledger, StatId stat, double? value);
            bool TryGetStat(Ledger ledger, StatId stat, out double? value);
        }

        public class StandardStatModifier : IStatModifier
        {

            public static readonly StandardStatModifier Default = new();

            public bool AdjustStat(Ledger ledger, StatId stat, double amount)
            {
                if (string.IsNullOrEmpty(stat.Category)) return false;

                var _stats = ledger._stats;

                var topkey = new StatId(stat.Category);
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
                else if (!ledger.Locked)
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
                    ledger.SignalOnChanged(stat);
                }
                return result;
            }

            public bool ClearStat(Ledger ledger, StatId stat)
            {
                var _stats = ledger._stats;

                if (string.IsNullOrEmpty(stat.Token))
                {
                    return ledger.ClearCategory(stat.Category);
                }
                else
                {
                    _stats.Remove(stat);
                    _stats[new StatId(stat.Category)] = this.SumCategorySubTokens(ledger, stat.Category);
                    ledger.SignalOnChanged(stat);
                    return true;
                }
            }

            public bool DeleteStat(Ledger ledger, StatId stat)
            {
                if (ledger.Locked) return this.ClearStat(ledger, stat);

                var _stats = ledger._stats;
                if (string.IsNullOrEmpty(stat.Token))
                {
                    return ledger.DeleteCategory(stat.Category);
                }
                else
                {
                    _stats.Remove(stat);
                    _stats[new StatId(stat.Category)] = this.SumCategorySubTokens(ledger, stat.Category);
                    ledger.SignalOnChanged(stat);
                    return true;
                }
            }

            public bool SetStat(Ledger ledger, StatId stat, double? value)
            {
                if (string.IsNullOrEmpty(stat.Category)) return false;

                var _stats = ledger._stats;
                if (string.IsNullOrEmpty(stat.Token))
                {
                    stat.Token = null;
                    if (ledger.Locked && !_stats.ContainsKey(stat)) return false;

                    _stats[stat] = value;
                    ledger.SignalOnChanged(stat);
                    return true;
                }
                else
                {
                    var topkey = new StatId(stat.Category);
                    if (ledger.Locked && !_stats.ContainsKey(topkey)) return false;

                    _stats[stat] = value;
                    _stats[topkey] = this.SumCategorySubTokens(ledger, stat.Category);
                    ledger.SignalOnChanged(stat);
                    return true;
                }
            }

            public bool TryGetStat(Ledger ledger, StatId stat, out double? value)
            {
                if (string.IsNullOrEmpty(stat.Token)) stat.Token = null;
                return ledger._stats.TryGetValue(stat, out value);
            }

            /// <summary>
            /// Sums all tokens in a category except the topkey. Usually used to set the topkey in the Standard modifier.
            /// </summary>
            /// <param name="category"></param>
            /// <returns></returns>
            private double? SumCategorySubTokens(Ledger ledger, string category)
            {
                double total = 0d;
                int cnt = 0;
                var e = ledger._stats.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current.Key.Category == category && !string.IsNullOrEmpty(e.Current.Key.Token) && e.Current.Value != null)
                    {
                        total += e.Current.Value.Value;
                        cnt++;
                    }
                }
                return cnt > 0 ? total : null;
            }
        }

        public class ReadWriteStatModifier : IStatModifier
        {

            public static readonly ReadWriteStatModifier Default = new();

            public bool AdjustStat(Ledger ledger, StatId stat, double amount)
            {
                if (string.IsNullOrEmpty(stat.Category)) return false;

                var current = ledger._stats.TryGetValue(stat, out double? v) ? v.GetValueOrDefault() : 0d;
                current += amount;
                ledger._stats[stat] = current;
                ledger.SignalOnChanged(stat);
                return true;
            }

            public bool ClearStat(Ledger ledger, StatId stat)
            {
                if (ledger._stats.TryGetValue(stat, out double? value) && value != null)
                {
                    ledger._stats[stat] = null;
                    ledger.SignalOnChanged(stat);
                    return true;
                }
                return false;
            }

            public bool DeleteStat(Ledger ledger, StatId stat)
            {
                if (ledger._stats.Remove(stat))
                {
                    ledger.SignalOnChanged(stat);
                    return true;
                }
                return false;
            }

            public bool SetStat(Ledger ledger, StatId stat, double? value)
            {
                if (string.IsNullOrEmpty(stat.Category)) return false;

                if (!ledger._stats.TryGetValue(stat, out double? exist) || exist != value)
                {
                    ledger._stats[stat] = value;
                    ledger.SignalOnChanged(stat);
                }
                return true;
            }

            public bool TryGetStat(Ledger ledger, StatId stat, out double? value)
            {
                return ledger._stats.TryGetValue(stat, out value);
            }
        }

        public class CumulativeAverageStatModifier : IStatModifier
        {

            const string META_COUNT = "Count";
            const string META_SUM = "Sum";

            public bool AdjustStat(Ledger ledger, StatId stat, double amount)
            {
                if (string.IsNullOrEmpty(stat.Category)) return false;

                var tk_token = stat.GetMeta(null);
                var tk_cnt = stat.GetMeta(META_COUNT);
                var tk_sum = stat.GetMeta(META_SUM);
                double cnt = ledger._stats.TryGetValue(tk_cnt, out double? d1) ? d1.GetValueOrDefault() : 0d;
                double sum = ledger._stats.TryGetValue(tk_sum, out double? d2) ? d2.GetValueOrDefault() : 0d;
                cnt += 1d;
                sum += amount;
                ledger._stats[tk_cnt] = cnt;
                ledger._stats[tk_sum] = sum;
                ledger._stats[tk_token] = sum / cnt;

                ledger.SignalOnChanged(tk_token);
                return true;
            }

            public bool ClearStat(Ledger ledger, StatId stat)
            {
                if (string.IsNullOrEmpty(stat.Category)) return false;

                var tk_token = stat.GetMeta(null);
                var tk_cnt = stat.GetMeta(META_COUNT);
                var tk_sum = stat.GetMeta(META_SUM);
                if (ledger.Contains(tk_token))
                {
                    ledger.SetStatRaw(tk_token, null);
                    ledger.RemoveStatRaw(tk_cnt);
                    ledger.RemoveStatRaw(tk_sum);
                    ledger.SignalOnChanged(tk_token);
                    return true;
                }

                return false;
            }

            public bool DeleteStat(Ledger ledger, StatId stat)
            {
                if (string.IsNullOrEmpty(stat.Category)) return false;

                var tk_token = stat.GetMeta(null);
                var tk_cnt = stat.GetMeta(META_COUNT);
                var tk_sum = stat.GetMeta(META_SUM);

                if (ledger.RemoveStatRaw(tk_token))
                {
                    ledger.RemoveStatRaw(tk_cnt);
                    ledger.RemoveStatRaw(tk_sum);
                    ledger.SignalOnChanged(tk_token);
                    return true;
                }

                return false;
            }

            public bool SetStat(Ledger ledger, StatId stat, double? value)
            {
                if (string.IsNullOrEmpty(stat.Category)) return false;

                var tk_token = stat.GetMeta(null);
                var tk_cnt = stat.GetMeta(META_COUNT);
                var tk_sum = stat.GetMeta(META_SUM);
                ledger.SetStatRaw(tk_token, value ?? 0d);
                ledger.SetStatRaw(tk_cnt, 1);
                ledger.SetStatRaw(tk_sum, value ?? 0d);
                ledger.SignalOnChanged(tk_token);
                return true;
            }

            public bool TryGetStat(Ledger ledger, StatId stat, out double? value)
            {
                //we can read the individual stats for CumulativeAverageStatModifier
                return ledger._stats.TryGetValue(stat, out value);
            }
        }

        public class ExponentialMovingAverageStatModifier : IStatModifier
        {

            private int _sampleLength;
            private double _alpha;

            public ExponentialMovingAverageStatModifier(int samplelength = 10)
            {
                this.SampleLength = samplelength;
            }

            public int SampleLength
            {
                get => _sampleLength;
                set
                {
                    _sampleLength = System.Math.Max(1, value);
                    _alpha = 2d / (_sampleLength + 1);
                }
            }

            public bool AdjustStat(Ledger ledger, StatId stat, double amount)
            {
                if (string.IsNullOrEmpty(stat.Category)) return false;

                double avg = ledger._stats.TryGetValue(stat, out double? d) ? d.GetValueOrDefault() : 0d;
                avg = amount * _alpha + avg * (1d - _alpha);
                ledger._stats[stat] = avg;
                ledger.SignalOnChanged(stat);
                return true;
            }

            public bool ClearStat(Ledger ledger, StatId stat)
            {
                if (ledger._stats.TryGetValue(stat, out double? value) && value != null)
                {
                    ledger._stats[stat] = null;
                    ledger.SignalOnChanged(stat);
                    return true;
                }
                return false;
            }

            public bool DeleteStat(Ledger ledger, StatId stat)
            {
                if (ledger._stats.Remove(stat))
                {
                    ledger.SignalOnChanged(stat);
                    return true;
                }
                return false;
            }

            public bool SetStat(Ledger ledger, StatId stat, double? value)
            {
                if (string.IsNullOrEmpty(stat.Category)) return false;

                if (!ledger._stats.TryGetValue(stat, out double? exist) || exist != value)
                {
                    ledger._stats[stat] = value;
                    ledger.SignalOnChanged(stat);
                }
                return true;
            }

            public bool TryGetStat(Ledger ledger, StatId stat, out double? value)
            {
                //we can read the individual stats for ExponentialMovingAverageStatModifier
                return ledger._stats.TryGetValue(stat, out value);
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
        public string Stat => _statid.Category;
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
