using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

using com.spacepuppy.Utils;

using System.Reflection;
using System.Runtime.CompilerServices;

namespace com.spacepuppy.Netcode
{

    //public interface IPolymorphicNetworkSerializable : INetworkSerializable
    //{

    //}

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class PolymorphicNetworkSerializableAttribute : System.Attribute
    {
        /// <summary>
        /// Set this to override default typeId. This id must be unique.
        /// This is useful if you change the type name and you need backwards compatibility. 
        /// The default hash is the Type.FullName.
        /// </summary>
        public string typeId;
    }

    [System.Serializable]
    public struct NetworkRef<T> : INetworkSerializable where T : class, INetworkSerializable
    {
        public T instance;

        public NetworkRef(T instance)
        {
            this.instance = instance;
        }

        void INetworkSerializable.NetworkSerialize<TBuffer>(BufferSerializer<TBuffer> serializer)
        {
            ulong tp_hash;
            if (serializer.IsReader)
            {
                tp_hash = 0;
                serializer.SerializeValue(ref tp_hash);

                try
                {
                    var tp = PolymorphicNetworkSerializableHelper.UnHash(tp_hash);
                    if (!TypeUtil.IsType(tp, typeof(T))) tp = null;
                    var obj = tp != null ? (T)System.Activator.CreateInstance(tp) : null;
                    obj?.NetworkSerialize(serializer);
                    instance = obj;
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    instance = null;
                }
            }
            else
            {
                tp_hash = instance != null ? PolymorphicNetworkSerializableHelper.Hash(instance.GetType()) : 0;
                serializer.SerializeValue(ref tp_hash);
                instance?.NetworkSerialize(serializer);
            }
        }

        public static implicit operator NetworkRef<T>(T instance)
        {
            return new NetworkRef<T>() { instance = instance };
        }

    }

    static class PolymorphicNetworkSerializableHelper
    {

        private static Dictionary<ulong, System.Type> _hashToType = new();
        private static Dictionary<System.Type, ulong> _typeToHash = new();

        static PolymorphicNetworkSerializableHelper()
        {
            _hashToType.Clear();
            _typeToHash.Clear();
            foreach (var tp in TypeUtil.GetTypesAssignableFrom(typeof(INetworkSerializable)))
            {
                var attrib = tp.GetCustomAttribute<PolymorphicNetworkSerializableAttribute>();
                if (attrib != null)
                {
                    ulong hash = xxHash64.ComputeHash(string.IsNullOrEmpty(attrib.typeId) ? tp.FullName : attrib.typeId);
                    _hashToType[hash] = tp;
                    _typeToHash[tp] = hash;
                }
            }
        }

        public static ulong Hash(System.Type tp)
        {
            if (tp != null && _typeToHash.TryGetValue(tp, out ulong hash))
            {
                return hash;
            }
            else
            {
                return 0;
            }
        }

        public static System.Type UnHash(ulong hash)
        {
            if (_hashToType.TryGetValue(hash, out System.Type tp))
            {
                return tp;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sourced under the MIT license from, pruned down to only what is needed:
        /// https://github.com/uranium62/xxHash
        /// </summary>
        static class xxHash64
        {

            /// <summary>
            /// Compute xxHash for the string 
            /// </summary>
            /// <param name="str">The source of data</param>
            /// <param name="seed">The seed number</param>
            /// <returns>hash</returns>
            public static unsafe ulong ComputeHash(string str, uint seed = 0)
            {
                Debug.Assert(str != null);

                fixed (char* c = str)
                {
                    byte* ptr = (byte*)c;
                    int length = str.Length * 2;

                    return UnsafeComputeHash(ptr, length, seed);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static unsafe ulong UnsafeComputeHash(byte* ptr, int length, ulong seed)
            {
                // Use inlined version
                // return XXH64(ptr, length, seed);

                return __inline__XXH64(ptr, length, seed);
            }


            private static readonly ulong XXH_PRIME64_1 = 11400714785074694791UL;
            private static readonly ulong XXH_PRIME64_2 = 14029467366897019727UL;
            private static readonly ulong XXH_PRIME64_3 = 1609587929392839161UL;
            private static readonly ulong XXH_PRIME64_4 = 9650029242287828579UL;
            private static readonly ulong XXH_PRIME64_5 = 2870177450012600261UL;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static ulong XXH_rotl64(ulong x, int r)
            {
                return (x << r) | (x >> (64 - r));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static unsafe ulong XXH64(byte* input, int len, ulong seed)
            {
                ulong h64;

                if (len >= 32)
                {
                    byte* end = input + len;
                    byte* limit = end - 31;

                    ulong v1 = seed + XXH_PRIME64_1 + XXH_PRIME64_2;
                    ulong v2 = seed + XXH_PRIME64_2;
                    ulong v3 = seed + 0;
                    ulong v4 = seed - XXH_PRIME64_1;

                    do
                    {
                        v1 = XXH64_round(v1, *(ulong*)input); input += 8;
                        v2 = XXH64_round(v2, *(ulong*)input); input += 8;
                        v3 = XXH64_round(v3, *(ulong*)input); input += 8;
                        v4 = XXH64_round(v4, *(ulong*)input); input += 8;
                    } while (input < limit);

                    h64 = XXH_rotl64(v1, 1) +
                          XXH_rotl64(v2, 7) +
                          XXH_rotl64(v3, 12) +
                          XXH_rotl64(v4, 18);

                    h64 = XXH64_mergeRound(h64, v1);
                    h64 = XXH64_mergeRound(h64, v2);
                    h64 = XXH64_mergeRound(h64, v3);
                    h64 = XXH64_mergeRound(h64, v4);
                }
                else
                {
                    h64 = seed + XXH_PRIME64_5;
                }

                h64 += (ulong)len;

                return XXH64_finalize(h64, input, len);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static ulong XXH64_round(ulong acc, ulong input)
            {
                acc += input * XXH_PRIME64_2;
                acc = XXH_rotl64(acc, 31);
                acc *= XXH_PRIME64_1;
                return acc;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static ulong XXH64_mergeRound(ulong acc, ulong val)
            {
                val = XXH64_round(0, val);
                acc ^= val;
                acc = acc * XXH_PRIME64_1 + XXH_PRIME64_4;
                return acc;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static ulong XXH64_avalanche(ulong hash)
            {
                hash ^= hash >> 33;
                hash *= XXH_PRIME64_2;
                hash ^= hash >> 29;
                hash *= XXH_PRIME64_3;
                hash ^= hash >> 32;
                return hash;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static unsafe ulong XXH64_finalize(ulong hash, byte* ptr, int len)
            {
                len &= 31;
                while (len >= 8)
                {
                    ulong k1 = XXH64_round(0, *(ulong*)ptr);
                    ptr += 8;
                    hash ^= k1;
                    hash = XXH_rotl64(hash, 27) * XXH_PRIME64_1 + XXH_PRIME64_4;
                    len -= 8;
                }
                if (len >= 4)
                {
                    hash ^= *(uint*)ptr * XXH_PRIME64_1;
                    ptr += 4;
                    hash = XXH_rotl64(hash, 23) * XXH_PRIME64_2 + XXH_PRIME64_3;
                    len -= 4;
                }
                while (len > 0)
                {
                    hash ^= (*ptr++) * XXH_PRIME64_5;
                    hash = XXH_rotl64(hash, 11) * XXH_PRIME64_1;
                    --len;
                }
                return XXH64_avalanche(hash);
            }




            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static unsafe ulong __inline__XXH64(byte* input, int len, ulong seed)
            {
                ulong h64;

                if (len >= 32)
                {
                    byte* end = input + len;
                    byte* limit = end - 31;

                    ulong v1 = seed + XXH_PRIME64_1 + XXH_PRIME64_2;
                    ulong v2 = seed + XXH_PRIME64_2;
                    ulong v3 = seed + 0;
                    ulong v4 = seed - XXH_PRIME64_1;

                    do
                    {
                        var reg1 = *((ulong*)(input + 0));
                        var reg2 = *((ulong*)(input + 8));
                        var reg3 = *((ulong*)(input + 16));
                        var reg4 = *((ulong*)(input + 24));

                        // XXH64_round
                        v1 += reg1 * XXH_PRIME64_2;
                        v1 = (v1 << 31) | (v1 >> (64 - 31));
                        v1 *= XXH_PRIME64_1;

                        // XXH64_round
                        v2 += reg2 * XXH_PRIME64_2;
                        v2 = (v2 << 31) | (v2 >> (64 - 31));
                        v2 *= XXH_PRIME64_1;

                        // XXH64_round
                        v3 += reg3 * XXH_PRIME64_2;
                        v3 = (v3 << 31) | (v3 >> (64 - 31));
                        v3 *= XXH_PRIME64_1;

                        // XXH64_round
                        v4 += reg4 * XXH_PRIME64_2;
                        v4 = (v4 << 31) | (v4 >> (64 - 31));
                        v4 *= XXH_PRIME64_1;
                        input += 32;
                    } while (input < limit);

                    h64 = ((v1 << 1) | (v1 >> (64 - 1))) +
                          ((v2 << 7) | (v2 >> (64 - 7))) +
                          ((v3 << 12) | (v3 >> (64 - 12))) +
                          ((v4 << 18) | (v4 >> (64 - 18)));

                    // XXH64_mergeRound
                    v1 *= XXH_PRIME64_2;
                    v1 = (v1 << 31) | (v1 >> (64 - 31));
                    v1 *= XXH_PRIME64_1;
                    h64 ^= v1;
                    h64 = h64 * XXH_PRIME64_1 + XXH_PRIME64_4;

                    // XXH64_mergeRound
                    v2 *= XXH_PRIME64_2;
                    v2 = (v2 << 31) | (v2 >> (64 - 31));
                    v2 *= XXH_PRIME64_1;
                    h64 ^= v2;
                    h64 = h64 * XXH_PRIME64_1 + XXH_PRIME64_4;

                    // XXH64_mergeRound
                    v3 *= XXH_PRIME64_2;
                    v3 = (v3 << 31) | (v3 >> (64 - 31));
                    v3 *= XXH_PRIME64_1;
                    h64 ^= v3;
                    h64 = h64 * XXH_PRIME64_1 + XXH_PRIME64_4;

                    // XXH64_mergeRound
                    v4 *= XXH_PRIME64_2;
                    v4 = (v4 << 31) | (v4 >> (64 - 31));
                    v4 *= XXH_PRIME64_1;
                    h64 ^= v4;
                    h64 = h64 * XXH_PRIME64_1 + XXH_PRIME64_4;
                }
                else
                {
                    h64 = seed + XXH_PRIME64_5;
                }

                h64 += (ulong)len;

                // XXH64_finalize
                len &= 31;
                while (len >= 8)
                {
                    ulong k1 = XXH64_round(0, *(ulong*)input);
                    input += 8;
                    h64 ^= k1;
                    h64 = XXH_rotl64(h64, 27) * XXH_PRIME64_1 + XXH_PRIME64_4;
                    len -= 8;
                }
                if (len >= 4)
                {
                    h64 ^= *(uint*)input * XXH_PRIME64_1;
                    input += 4;
                    h64 = XXH_rotl64(h64, 23) * XXH_PRIME64_2 + XXH_PRIME64_3;
                    len -= 4;
                }
                while (len > 0)
                {
                    h64 ^= (*input++) * XXH_PRIME64_5;
                    h64 = XXH_rotl64(h64, 11) * XXH_PRIME64_1;
                    --len;
                }

                // XXH64_avalanche
                h64 ^= h64 >> 33;
                h64 *= XXH_PRIME64_2;
                h64 ^= h64 >> 29;
                h64 *= XXH_PRIME64_3;
                h64 ^= h64 >> 32;

                return h64;
            }

        }

    }

}
