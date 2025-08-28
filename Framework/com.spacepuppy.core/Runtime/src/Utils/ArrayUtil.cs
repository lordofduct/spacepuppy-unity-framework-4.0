using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;

namespace com.spacepuppy.Utils
{

    public static class ArrayUtil
    {


#if NET35

        public static IEnumerable<T> Append<T>(this IEnumerable<T> lst, T obj)
        {
            //foreach (var o in lst)
            //{
            //    yield return o;
            //}
            var e = new LightEnumerator<T>(lst);
            while (e.MoveNext())
            {
                yield return e.Current;
            }
            yield return obj;
        }

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> lst, T obj)
        {
            yield return obj;
            //foreach (var o in lst)
            //{
            //    yield return o;
            //}
            var e = new LightEnumerator<T>(lst);
            while (e.MoveNext())
            {
                yield return e.Current;
            }
        }

#endif


        #region General Methods

        public static bool IsEmpty(this IEnumerable lst)
        {
            if (lst is ICollection coll)
            {
                return coll.Count == 0;
            }
            else
            {
                return !lst.GetEnumerator().MoveNext();
            }
        }

        public static int Count<T, TArg>(this IEnumerable<T> e, TArg arg, System.Func<T, TArg, bool> predicate)
        {
            if (predicate == null) throw new System.ArgumentNullException(nameof(predicate));

            int cnt = 0;
            foreach (var o in e)
            {
                if (predicate(o, arg)) cnt++;
            }
            return cnt;
        }

        public static T LastOrDefault<T>(this IEnumerable<T> e, T defaultvalue)
        {
            if (e is IList<T> ilst)
            {
                int cnt = ilst.Count;
                return cnt > 0 ? ilst[cnt - 1] : defaultvalue;
            }
            else if (e is IReadOnlyList<T> rlst)
            {
                int cnt = rlst.Count;
                return cnt > 0 ? rlst[cnt - 1] : defaultvalue;
            }
            else
            {
                var en = e.GetEnumerator();
                T result = defaultvalue;
                while (en.MoveNext())
                {
                    result = en.Current;
                }
                return result;
            }
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> e, T defaultvalue)
        {
            if (e is IList<T> ilst)
            {
                int cnt = ilst.Count;
                return cnt > 0 ? ilst[0] : defaultvalue;
            }
            else if (e is IReadOnlyList<T> rlst)
            {
                int cnt = rlst.Count;
                return cnt > 0 ? rlst[0] : defaultvalue;
            }
            else
            {
                var en = e.GetEnumerator();
                T result = defaultvalue;
                if (en.MoveNext())
                {
                    result = en.Current;
                }
                return result;
            }
        }

        /// <summary>
        /// Get how deep into the enumerable the first instance of the object is.
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int Depth(this IEnumerable lst, object obj)
        {
            int i = 0;
            foreach (var o in lst)
            {
                if (object.Equals(o, obj)) return i;
                i++;
            }
            return -1;
        }

        /// <summary>
        /// Get how deep into the enumerable the first instance of the value is.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lst"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Depth<T>(this IEnumerable<T> lst, T value)
        {
            int i = 0;
            foreach (var v in lst)
            {
                if (EqualityComparer<T>.Default.Equals(v, value)) return i;
                i++;
            }
            return -1;
        }

        public static bool Compare<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            var e1 = first.GetEnumerator();
            var e2 = second.GetEnumerator();

            while (true)
            {
                var b1 = e1.MoveNext();
                var b2 = e2.MoveNext();
                if (!b1 && !b2) break; //reached end of list

                if (b1 && b2)
                {
                    if (!EqualityComparer<T>.Default.Equals(e1.Current, e2.Current)) return false;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Each enumerable contains the same elements, not necessarily in the same order, or of the same count. Just the same elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool SimilarTo<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            return first.Except(second).Count() + second.Except(first).Count() == 0;
        }

        public static bool ContainsAny<T>(this IEnumerable<T> lst, IEnumerable<T> objs)
        {
            if (lst is ISet<T> set)
            {
                foreach (var o in objs)
                {
                    if (set.Contains(o)) return true;
                }
                return false;
            }
            else
            {
                return lst.Intersect(objs).Any();
            }
        }

        public static bool Contains<T>(this T[,] arr, T value)
        {
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    if (EqualityComparer<T>.Default.Equals(arr[i, j], value)) return true;
                }
            }

            return false;
        }

        public static void AddRange<T>(this ICollection<T> lst, IEnumerable<T> elements)
        {
            //foreach (var e in elements)
            //{
            //    lst.Add(e);
            //}
            var e = new LightEnumerator<T>(elements);
            while (e.MoveNext())
            {
                lst.Add(e.Current);
            }
        }

        public static bool AddRange<T>(this ISet<T> lst, IEnumerable<T> elements)
        {
            var e = new LightEnumerator<T>(elements);
            bool result = false;
            while (e.MoveNext())
            {
                if (lst.Add(e.Current)) result = true;
            }
            return result;
        }

        /// <summary>
        /// Gets the element in the collection just after 'element'. If 'element' is not in the collection the first element is returned. If the collection is empty default(T) is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lst"></param>
        /// <param name="element"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        public static T GetValueAfterOrDefault<T>(this IEnumerable<T> lst, T element, bool loop = false)
        {
            if (lst is IList<T> ilst)
            {
                if (ilst.Count == 0) return default(T);

                int i = ilst.IndexOf(element) + 1;
                if (loop) i = i % ilst.Count;
                else if (i >= ilst.Count) return default(T);
                return ilst[i];
            }
            else
            {
                var e = lst.GetEnumerator();
                if (!e.MoveNext()) return default(T);
                var first = e.Current;
                if (object.Equals(e.Current, element))
                {
                    if (e.MoveNext())
                    {
                        return e.Current;
                    }
                    else if (loop)
                    {
                        return first;
                    }
                    else
                    {
                        return default(T);
                    }
                }

                while (e.MoveNext())
                {
                    if (object.Equals(e.Current, element))
                    {
                        if (e.MoveNext())
                        {
                            return e.Current;
                        }
                        else if (loop)
                        {
                            return first;
                        }
                        else
                        {
                            return default(T);
                        }
                    }
                }
                return default(T);
            }
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> lst, T element)
        {
            if (lst == null) throw new System.ArgumentNullException("lst");
            foreach (var e in lst)
            {
                if (!object.Equals(e, element)) yield return e;
            }
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> lst, T element, IEqualityComparer<T> comparer)
        {
            if (lst == null) throw new System.ArgumentNullException("lst");
            if (comparer == null) throw new System.ArgumentNullException("comparer");
            foreach (var e in lst)
            {
                if (!comparer.Equals(e, element)) yield return e;
            }
        }

        public delegate bool SelectIfCallback<T, TResult>(T input, out TResult output);
        public static IEnumerable<TResult> SelectIf<T, TResult>(this IEnumerable<T> e, SelectIfCallback<T, TResult> callback)
        {
            if (callback == null) yield break;
            TResult result;
            foreach (var o in e)
            {
                if (callback(o, out result)) yield return result;
            }
        }

        public static T MinOrDefault<T>(this IEnumerable<T> e)
        {
            if (e?.Any() ?? false)
            {
                return e.Min();
            }
            else
            {
                return default(T);
            }
        }

        public static T MaxOrDefault<T>(this IEnumerable<T> e)
        {
            if (e?.Any() ?? false)
            {
                return e.Max();
            }
            else
            {
                return default(T);
            }
        }

        public static IEnumerable ForEach(this IEnumerable e, System.Action<object> callback)
        {
            foreach (var o in e)
            {
                callback?.Invoke(o);
            }
            return e;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> e, System.Action<T> callback)
        {
            foreach (var o in e)
            {
                callback?.Invoke(o);
            }
            return e;
        }

        public static int Unpack<T>(this IEnumerable<T> e, out T a)
        {
            var en = e.GetEnumerator();
            if (!en.MoveNext())
            {
                a = default; return 0;
            }

            a = en.Current;
            return 1;
        }

        public static int Unpack<T>(this IEnumerable<T> e, out T a, out T b)
        {
            var en = e.GetEnumerator();
            if (!en.MoveNext())
            {
                a = default; b = default; return 0;
            }

            a = en.Current;
            if (!en.MoveNext())
            {
                b = default; return 1;
            }

            b = en.Current;
            return 2;
        }

        public static int Unpack<T>(this IEnumerable<T> e, out T a, out T b, out T c)
        {
            var en = e.GetEnumerator();
            if (!en.MoveNext())
            {
                a = default; b = default; c = default; return 0;
            }

            a = en.Current;
            if (!en.MoveNext())
            {
                b = default; c = default; return 1;
            }

            b = en.Current;
            if (!en.MoveNext())
            {
                c = default; return 2;
            }

            c = en.Current;
            return 3;
        }

        public static int Unpack<T>(this IEnumerable<T> e, out T a, out T b, out T c, out T d)
        {
            var en = e.GetEnumerator();
            if (!en.MoveNext())
            {
                a = default; b = default; c = default; d = default; return 0;
            }

            a = en.Current;
            if (!en.MoveNext())
            {
                b = default; c = default; d = default; return 1;
            }

            b = en.Current;
            if (!en.MoveNext())
            {
                c = default; d = default; return 2;
            }

            c = en.Current;
            if (!en.MoveNext())
            {
                d = default; return 3;
            }

            d = en.Current;
            return 4;
        }

        #endregion

        #region Random Methods

        public static void Shuffle<T>(T[] arr, IRandom rng = null)
        {
            if (arr == null) throw new System.ArgumentNullException("arr");
            if (rng == null) rng = RandomUtil.Standard;

            int j;
            T temp;
            for (int i = 0; i < arr.Length - 1; i++)
            {
                j = rng.Next(i, arr.Length);
                temp = arr[j];
                arr[j] = arr[i];
                arr[i] = temp;
            }
        }

        public static void Shuffle<T>(T[,] arr, IRandom rng = null)
        {
            if (arr == null) throw new System.ArgumentNullException("arr");
            if (rng == null) rng = RandomUtil.Standard;

            int width = arr.GetLength(0);
            for (int i = 0; i < arr.Length - 1; i++)
            {
                int j = rng.Next(i, arr.Length);
                int ix = i % width;
                int iy = (int)(i / width);
                int jx = j % width;
                int jy = (int)(j / width);
                T temp = arr[jx, jy];
                arr[jx, jy] = arr[ix, iy];
                arr[ix, iy] = temp;
            }
        }

        public static void Shuffle(IList lst, IRandom rng = null)
        {
            if (lst == null) throw new System.ArgumentNullException("lst");
            if (rng == null) rng = RandomUtil.Standard;

            int j;
            object temp;
            int cnt = lst.Count;
            for (int i = 0; i < cnt - 1; i++)
            {
                j = rng.Next(i, cnt);
                temp = lst[j];
                lst[j] = lst[i];
                lst[i] = temp;
            }
        }

        public static void Shuffle<T>(IList<T> lst, IRandom rng = null)
        {
            if (lst == null) throw new System.ArgumentNullException("lst");
            if (rng == null) rng = RandomUtil.Standard;

            int j;
            T temp;
            int cnt = lst.Count;
            for (int i = 0; i < cnt - 1; i++)
            {
                j = rng.Next(i, cnt);
                temp = lst[j];
                lst[j] = lst[i];
                lst[i] = temp;
            }
        }

        public static IEnumerable<T> Shuffled<T>(this IEnumerable<T> coll, IRandom rng = null)
        {
            if (coll == null) throw new System.ArgumentNullException("coll");
            if (rng == null) rng = RandomUtil.Standard;

            //NOTE - as long as the calling code uses this as part of a foreach, or in a linq statement, the IDisposable will be disposed properly
            //otherwise this TempList will be lost to the heap and made available for GC when appropriate thus making the TempList pointless.
            //But by using a TempList we get the benefit in most situations and lose nothing in the case of GC compared to a standard 'List'.
            using (var buffer = TempCollection.GetList<T>(coll))
            {
                int j;
                for (int i = 0; i < buffer.Count; i++)
                {
                    j = rng.Next(i, buffer.Count);
                    yield return buffer[j];
                    buffer[j] = buffer[i];
                }
            }
        }

        public static T PickRandom<T>(this IEnumerable<T> e, IRandom rng = null)
        {
            if (rng == null) rng = RandomUtil.Standard;

            if (e is IList<T> ilst)
            {
                return ilst.Count > 0 ? ilst[rng.Range(ilst.Count)] : default;
            }
            else if (e is IReadOnlyList<T> rlst)
            {
                return rlst.Count > 0 ? rlst[rng.Range(rlst.Count)] : default;
            }
            else
            {
                using (var tlst = TempCollection.GetList<T>(e))
                {
                    return tlst.Count > 0 ? tlst[rng.Range(tlst.Count)] : default;
                }
            }
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> lst, int count, IRandom rng = null)
        {
            return lst.Shuffled(rng).Take(count);
        }

        public static T PickRandom<T>(this IEnumerable<T> lst, System.Func<T, float> weightPredicate, IRandom rng = null)
        {
            var arr = (lst is IList<T>) ? lst as IList<T> : lst.ToList();
            if (arr.Count == 0) return default(T);

            using (var weights = com.spacepuppy.Collections.TempCollection.GetList<float>())
            {
                int i;
                float w;
                double total = 0f;
                for (i = 0; i < arr.Count; i++)
                {
                    w = weightPredicate(arr[i]);
                    if (float.IsPositiveInfinity(w)) return arr[i];
                    else if (w >= 0f && !float.IsNaN(w)) total += w;
                    weights.Add(w);
                }

                if (rng == null) rng = RandomUtil.Standard;
                double r = rng.NextDouble();
                double s = 0f;

                for (i = 0; i < weights.Count; i++)
                {
                    w = weights[i];
                    if (float.IsNaN(w) || w <= 0f) continue;

                    s += w / total;
                    if (s > r)
                    {
                        return arr[i];
                    }
                }

                //should only get here if last element had a zero weight, and the r was large
                i = arr.Count - 1;
                while (i > 0 && weights[i] <= 0f) i--;
                return arr[i];
            }
        }

        public static T[] PickRandom<T>(this IEnumerable<T> lst, System.Func<T, float> weightPredicate, int count, IRandom rng = null)
        {
            if (count <= 0) return ArrayUtil.Empty<T>();

            using (var arr = TempCollection.GetList<T>(lst))
            {
                if (arr.Count < count) return arr.ToArray();

                var result = new T[count];
                int ri = 0;
                using (var weights = com.spacepuppy.Collections.TempCollection.GetList<float>())
                {
                    int i;
                    float w = 0f;
                    double total = 0f;
                    for (i = 0; i < arr.Count; i++)
                    {
                        w = weightPredicate(arr[i]);
                        if (float.IsPositiveInfinity(w))
                        {
                            result[ri] = arr[i];
                            ri++;
                            arr.RemoveAt(i);
                            i--;
                            if (ri >= result.Length) return result;
                        }
                        else
                        {
                            if (w >= 0f && !float.IsNaN(w)) total += w;
                            weights.Add(w);
                        }
                    }

                    if (rng == null) rng = RandomUtil.Standard;

                    while (ri < result.Length)
                    {
                        double r = rng.NextDouble();
                        double s = 0f;
                        int choice = -1;
                        for (i = 0; i < weights.Count; i++)
                        {
                            w = weights[i];
                            if (float.IsNaN(w) || w <= 0f) continue;

                            s += w / total;
                            if (s > r)
                            {
                                choice = i;
                                break;
                            }
                        }

                        if (choice < 0)
                        {
                            //should only get here if last element had a zero weight, and the r was large
                            choice = arr.Count - 1;
                            while (choice > 0 && weights[choice] <= 0f) choice--;
                        }
                        result[ri] = arr[choice];
                        ri++;
                        w = weights[choice];
                        arr.RemoveAt(choice);
                        weights.RemoveAt(choice);
                        if (!float.IsPositiveInfinity(w) && w >= 0f && !float.IsNaN(w)) total -= w;
                    }
                }

                return result;
            }
        }

        public static T RemoveRandom<T>(this ICollection<T> coll, IRandom rng = null)
        {
            if (coll is IList<T> lst)
            {
                if (lst.Count == 0) return default;
                if (rng == null) rng = RandomUtil.Standard;
                int index = rng.Range(lst.Count);
                var result = lst[index];
                lst.RemoveAt(index);
                return result;
            }
            else
            {
                var result = coll.Shuffled(rng).Take(1).FirstOrDefault();
                coll.Remove(result);
                return result;
            }
        }

        public static IEnumerable<T> PickRandomWithRepeat<T>(this IEnumerable<T> coll, int count, IRandom rng = null)
        {
            if (count == 0) yield break;
            if (count == 1)
            {
                yield return PickRandom<T>(coll, rng);
                yield break;
            }

            if (rng == null) rng = RandomUtil.Standard;

            if (coll is IList<T> lst)
            {
                for (int i = 0; i < count; i++)
                {
                    yield return lst[rng.Next(lst.Count)];
                }
            }
            else if (coll is IReadOnlyList<T> rolst)
            {
                for (int i = 0; i < count; i++)
                {
                    yield return rolst[rng.Next(rolst.Count)];
                }
            }
            else
            {
                //NOTE - see Shuffled for dispose explanation
                using (var buffer = TempCollection.GetList<T>(coll))
                {
                    for (int i = 0; i < count; i++)
                    {
                        yield return buffer[rng.Next(buffer.Count)];
                    }
                }
            }
        }

        #endregion

        #region Array Methods

        /// <summary>
        /// Returns a temp array of length len if small enough for a cached array. 
        /// Otherwise returns a new array of length len.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="len"></param>
        /// <returns></returns>
        public static T[] TryGetTemp<T>(int len)
        {
            return TempArray<T>.TryGetTemp(len);
        }

        public static T[] Empty<T>()
        {
            return TempArray<T>.Empty;
        }

        public static T[] Temp<T>(T value)
        {
            return TempArray<T>.Temp(value);
        }

        public static T[] Temp<T>(T value1, T value2)
        {
            return TempArray<T>.Temp(value1, value2);
        }

        public static T[] Temp<T>(T value1, T value2, T value3)
        {
            return TempArray<T>.Temp(value1, value2, value3);
        }

        public static T[] Temp<T>(T value1, T value2, T value3, T value4)
        {
            return TempArray<T>.Temp(value1, value2, value3, value4);
        }

        /// <summary>
        /// Attempts to create a small temp array if coll is small enough, otherwise generates a new array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coll"></param>
        /// <returns></returns>
        public static T[] Temp<T>(IEnumerable<T> coll)
        {
            if (coll is IList<T> l)
            {
                return Temp<T>(l);
            }
            else if (coll is IReadOnlyList<T> rl)
            {
                return Temp<T>(rl);
            }
            else
            {
                using (var lst = TempCollection.GetList<T>(coll))
                {
                    return Temp<T>((IList<T>)lst);
                }
            }
        }

        public static T[] Temp<T>(IList<T> coll)
        {
            switch (coll.Count)
            {
                case 0:
                    return ArrayUtil.Empty<T>();
                case 1:
                    return TempArray<T>.Temp(coll[0]);
                case 2:
                    return TempArray<T>.Temp(coll[0], coll[1]);
                case 3:
                    return TempArray<T>.Temp(coll[0], coll[1], coll[2]);
                case 4:
                    return TempArray<T>.Temp(coll[0], coll[1], coll[2], coll[3]);
                default:
                    return coll.ToArray();
            }
        }

        private static T[] Temp<T>(IReadOnlyList<T> coll)
        {
            switch (coll.Count)
            {
                case 0:
                    return ArrayUtil.Empty<T>();
                case 1:
                    return TempArray<T>.Temp(coll[0]);
                case 2:
                    return TempArray<T>.Temp(coll[0], coll[1]);
                case 3:
                    return TempArray<T>.Temp(coll[0], coll[1], coll[2]);
                case 4:
                    return TempArray<T>.Temp(coll[0], coll[1], coll[2], coll[3]);
                default:
                    return coll.ToArray();
            }
        }

        public static void ReleaseTemp<T>(T[] arr)
        {
            TempArray<T>.Release(arr);
        }






        public static int IndexOf<T>(this T[] lst, T obj)
        {
            return System.Array.IndexOf(lst, obj);
        }
        public static int IndexOf<T>(this IList<T> lst, System.Func<T, bool> predicate)
        {
            for (int i = 0; i < lst.Count; i++)
            {
                if (predicate(lst[i])) return i;
            }
            return -1;
        }
        public static int IndexOf<T, TArg>(this IList<T> lst, TArg arg, System.Func<T, TArg, bool> predicate)
        {
            for (int i = 0; i < lst.Count; i++)
            {
                if (predicate(lst[i], arg)) return i;
            }
            return -1;
        }

        public static bool InBounds(this System.Array arr, int index)
        {
            return index >= 0 && index <= arr.Length - 1;
        }

        public static void Clear(this System.Array arr)
        {
            if (arr == null) return;
            System.Array.Clear(arr, 0, arr.Length);
        }

        public static void Copy<T>(IEnumerable<T> source, System.Array destination, int index)
        {
            if (source is System.Collections.ICollection)
                (source as System.Collections.ICollection).CopyTo(destination, index);
            else
            {
                int i = 0;
                foreach (var el in source)
                {
                    destination.SetValue(el, i + index);
                    i++;
                }
            }
        }


        #endregion

        #region List Methods

        public static T Pop<T>(this List<T> lst)
        {
            if (lst.Count == 0) throw new System.ArgumentException("List is empty.", nameof(lst));

            var result = lst[lst.Count - 1];
            lst.RemoveAt(lst.Count - 1);
            return result;
        }

        public static T Shift<T>(this List<T> lst)
        {
            if (lst.Count == 0) throw new System.ArgumentException("List is empty.", nameof(lst));

            var result = lst[0];
            lst.RemoveAt(0);
            return result;
        }

        public static T AtIndexOrDefault<T>(this IList<T> lst, int index)
        {
            if (index >= 0 && index < lst?.Count)
            {
                return lst[index];
            }
            return default;
        }

        public static T AtIndexOrDefault<T>(this IReadOnlyList<T> lst, int index)
        {
            if (index >= 0 && index < lst?.Count)
            {
                return lst[index];
            }
            return default;
        }

        #endregion

        #region HashSet Methods

#if !UNITY_2021_2_OR_NEWER
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> e)
        {
            return new HashSet<T>(e);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> e, IEqualityComparer<T> comparer)
        {
            return new HashSet<T>(e, comparer);
        }
#endif

        public static T Pop<T>(this HashSet<T> set)
        {
            if (set == null) throw new System.ArgumentNullException(nameof(set));

            var e = set.GetEnumerator();
            if (e.MoveNext())
            {
                set.Remove(e.Current);
                return e.Current;
            }

            throw new System.ArgumentException("HashSet must not be empty.");
        }

        public static T First<T>(this HashSet<T> set)
        {
            if (set == null) throw new System.ArgumentNullException(nameof(set));

            var e = set.GetEnumerator();
            if (e.MoveNext())
            {
                return e.Current;
            }

            throw new System.ArgumentException("HashSet must not be empty.");
        }

        public static T FirstOrDefault<T>(this HashSet<T> set)
        {
            if (set == null) throw new System.ArgumentNullException(nameof(set));

            var e = set.GetEnumerator();
            if (e.MoveNext())
            {
                return e.Current;
            }

            return default(T);
        }

        #endregion

        #region Special Types

        private class TempArray<T>
        {

            private static object _lock = new object();
            private static volatile T[] _empty;
            private static volatile T[] _oneArray;
            private static volatile T[] _twoArray;
            private static volatile T[] _threeArray;
            private static volatile T[] _fourArray;

            public static T[] Empty
            {
                get
                {
                    if (_empty == null) _empty = new T[0];
                    return _empty;
                }
            }

            public static T[] TryGetTemp(int len)
            {
                T[] result;
                lock (_lock)
                {
                    switch (len)
                    {
                        case 0:
                            result = Empty;
                            break;
                        case 1:
                            if (_oneArray != null)
                            {
                                result = _oneArray;
                                _oneArray = null;
                            }
                            else
                            {
                                result = new T[1];
                            }
                            break;
                        case 2:
                            if (_twoArray != null)
                            {
                                result = _twoArray;
                                _twoArray = null;
                            }
                            else
                            {
                                result = new T[2];
                            }
                            break;
                        case 3:
                            if (_threeArray != null)
                            {
                                result = _threeArray;
                                _threeArray = null;
                            }
                            else
                            {
                                result = new T[3];
                            }
                            break;
                        case 4:
                            if (_fourArray != null)
                            {
                                result = _fourArray;
                                _fourArray = null;
                            }
                            else
                            {
                                result = new T[4];
                            }
                            break;
                        default:
                            result = new T[len];
                            break;
                    }
                }
                return result;
            }

            public static T[] Temp(T value)
            {
                T[] arr;

                lock (_lock)
                {
                    if (_oneArray != null)
                    {
                        arr = _oneArray;
                        _oneArray = null;
                    }
                    else
                    {
                        arr = new T[1];
                    }
                }

                arr[0] = value;
                return arr;
            }

            public static T[] Temp(T value1, T value2)
            {
                T[] arr;

                lock (_lock)
                {
                    if (_twoArray != null)
                    {
                        arr = _twoArray;
                        _twoArray = null;
                    }
                    else
                    {
                        arr = new T[2];
                    }
                }

                arr[0] = value1;
                arr[1] = value2;
                return arr;
            }

            public static T[] Temp(T value1, T value2, T value3)
            {
                T[] arr;

                lock (_lock)
                {
                    if (_threeArray != null)
                    {
                        arr = _threeArray;
                        _threeArray = null;
                    }
                    else
                    {
                        arr = new T[3];
                    }
                }

                arr[0] = value1;
                arr[1] = value2;
                arr[2] = value3;
                return arr;
            }

            public static T[] Temp(T value1, T value2, T value3, T value4)
            {
                T[] arr;

                lock (_lock)
                {
                    if (_fourArray != null)
                    {
                        arr = _fourArray;
                        _fourArray = null;
                    }
                    else
                    {
                        arr = new T[4];
                    }
                }

                arr[0] = value1;
                arr[1] = value2;
                arr[2] = value3;
                arr[3] = value4;
                return arr;
            }


            public static void Release(T[] arr)
            {
                if (arr == null) return;
                System.Array.Clear(arr, 0, arr.Length);

                lock (_lock)
                {
                    switch (arr.Length)
                    {
                        case 1:
                            _oneArray = arr;
                            break;
                        case 2:
                            _twoArray = arr;
                            break;
                        case 3:
                            _threeArray = arr;
                            break;
                        case 4:
                            _fourArray = arr;
                            break;
                    }
                }
            }
        }

        #endregion

    }

}

