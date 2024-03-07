using System;
using System.Collections.Generic;

namespace com.spacepuppy.Collections
{

    public interface ITempCollection<T> : ICollection<T>, IDisposable
    {

    }

    /// <summary>
    /// This is intended for a short lived collection that needs to be memory efficient and fast. 
    /// Call the static 'GetCollection' method to get a cached collection for use. 
    /// When you're done with the collection you call Release to make it available for reuse again later. 
    /// Do NOT use it again after calling Release.
    /// 
    /// Due to the design of this, it should only ever be used in a single threaded manner. Primarily intended 
    /// for the main Unity thread. 
    /// 
    /// If you're in a separate thread, it's best to cache your own list local to there, and don't even bother with 
    /// this.
    /// </summary>
    public static class TempCollection
    {

        #region Static Interface

        /// <summary>
        /// Returns the any available collection for use generically. 
        /// The collection could be a HashSet, List, or any temp implementation. 
        /// This is intended to reduce the need for creating a new collection 
        /// unnecessarily.
        /// </summary>
        /// <returns></returns>
        public static ITempCollection<T> GetCollection<T>()
        {
            return GetList<T>();
        }

        /// <summary>
        /// Returns the any available collection for use generically. 
        /// The collection could be a HashSet, List, or any temp implementation. 
        /// This is intended to reduce the need for creating a new collection 
        /// unnecessarily.
        /// </summary>
        /// <returns></returns>
        public static ITempCollection<T> GetCollection<T>(IEnumerable<T> e)
        {
            return GetList<T>(e);
        }



        public static TempList<T> GetList<T>()
        {
            return TempList<T>.GetList();
        }

        public static TempList<T> GetList<T>(IEnumerable<T> e)
        {
            return TempList<T>.GetList(e);
        }

        public static TempList<T> GetList<T>(int count)
        {
            return TempList<T>.GetList(count);
        }

        public static TempHashSet<T> GetSet<T>()
        {
            return TempHashSet<T>.GetSet();
        }

        public static TempHashSet<T> GetSet<T>(IEqualityComparer<T> comparer)
        {
            return TempHashSet<T>.GetSet(comparer);
        }

        public static TempHashSet<T> GetSet<T>(IEnumerable<T> e)
        {
            return TempHashSet<T>.GetSet(e);
        }

        public static TempHashSet<T> GetSet<T>(IEnumerable<T> e, IEqualityComparer<T> comparer)
        {
            return TempHashSet<T>.GetSet(e, comparer);
        }

        public static TempDictionary<TKey, TValue> GetDict<TKey, TValue>()
        {
            return TempDictionary<TKey, TValue>.GetDict();
        }

        public static TempDictionary<TKey, TValue> GetDict<TKey, TValue>(IEqualityComparer<TKey> comparer)
        {
            return TempDictionary<TKey, TValue>.GetDict(comparer);
        }

        public static TempDictionary<TKey, TValue> GetDict<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            return TempDictionary<TKey, TValue>.GetDict(dict);
        }

        public static TempDictionary<TKey, TValue> GetDict<TKey, TValue>(IDictionary<TKey, TValue> dict, IEqualityComparer<TKey> comparer)
        {
            return TempDictionary<TKey, TValue>.GetDict(dict, comparer);
        }

        public static TempQueue<T> GetQueue<T>()
        {
            return TempQueue<T>.GetQueue();
        }

        public static TempQueue<T> GetQueue<T>(IEnumerable<T> e)
        {
            return TempQueue<T>.GetQueue(e);
        }

        public static TempCallbackCollection<T> GetCallbackCollection<T>(Action<T> addCallback, Action<T> removeCallback = null)
        {
            return TempCallbackCollection<T>.GetCallbackCollection(addCallback, removeCallback);
        }

        public static void ValidateAndAdd<T>(ref TempList<T> lst, T value)
        {
            if (lst == null) lst = GetList<T>();
            lst.Add(value);
        }

        public static void ValidateAndAdd<T>(ref TempHashSet<T> lst, T value)
        {
            if (lst == null) lst = GetSet<T>();
            lst.Add(value);
        }

        public static void ValidateAndAdd<T>(ref TempQueue<T> lst, T value)
        {
            if (lst == null) lst = GetQueue<T>();
            lst.Enqueue(value);
        }

        public static void ValidateAndAdd<T>(ref ITempCollection<T> lst, T value)
        {
            if (lst == null) lst = GetCollection<T>();
            lst.Add(value);
        }

        #endregion

    }
}
