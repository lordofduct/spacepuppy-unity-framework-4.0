﻿using UnityEngine;
using BitConverter = System.BitConverter;

namespace com.spacepuppy.Utils
{

    /// <summary>
    /// A utility class for RNG values, when creating new RNG objects negative seeds are treated as random seeds. 
    /// This is counter to how System.Random works which absolute values negative seeds. If you want this behaviour 
    /// you must manually absolute value the seed.
    /// </summary>
    public static class RandomUtil
    {

        const int MAX_SINGLE_NUMERATOR = 0xFFFFFF;
        const int MAX_SINGLE_DENOMINATOR = 0x1000000;
        const float MAX_SINGLE_RATIO = (float)MAX_SINGLE_NUMERATOR / (float)MAX_SINGLE_DENOMINATOR;
        const ulong MAX_DOUBLE_NUMERATOR = 0x1FFFFFFFFFFFFFu;
        const ulong MAX_DOUBLE_DENOMINATOR = 0x20000000000000u;
        const double MAX_DOUBLE_RATIO = (double)MAX_DOUBLE_NUMERATOR / (double)MAX_DOUBLE_DENOMINATOR; //this technically prints as 1, but it's actually the largest possible value < 1 represented as a float. It's binary representation is: 0x3FEFFFFFFFFFFFFF

        #region Static Fields

        private static UnityRNG _unityRNG = new UnityRNG();

        public static IRandom Standard { get { return _unityRNG; } }

        public static void ReseedStandard(int seed)
        {
            _unityRNG.SetSeed(seed);
        }

        public static IRandom CreateRNG(int seed = -1)
        {
            return new MicrosoftRNG(seed);
        }

        public static IRandom CreateRNG(long seed = -1)
        {
            return new MicrosoftRNG(seed);
        }

        /// <summary>
        /// Create an rng that is deterministic to that 'seed' across all platforms.
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static IRandom CreateDeterministicRNG(long seed = -1)
        {
            return new SimplePCG(seed);
        }

        public static IRandom SelfOrDefault(this IRandom rng)
        {
            return rng ?? Standard;
        }

        #endregion

        #region Static Properties

        public static float Angle(this IRandom rng)
        {
            return rng.Next() * 360f;
        }

        public static float Radian(this IRandom rng)
        {
            return rng.Next() * Mathf.PI * 2f;
        }

        /// <summary>
        /// Return 0 or 1. Numeric version of Bool.
        /// </summary>
        /// <returns></returns>
        public static int Pop(this IRandom rng)
        {
            return rng.Next(1000) % 2;
        }

        public static int Sign(this IRandom rng)
        {
            int n = rng.Next(1000) % 2;
            return n + n - 1;
        }

        /// <summary>
        /// Return a true randomly.
        /// </summary>
        /// <returns></returns>
        public static bool Bool(this IRandom rng)
        {
            return (rng.Next(1000) % 2 != 0);
        }

        public static bool Bool(this IRandom rng, float oddsOfTrue)
        {
            int i = rng.Next(100000);
            int m = (int)(oddsOfTrue * 100000);
            return i < m;
        }

        /// <summary>
        /// Return -1, 0, 1 randomly. This can be used for bizarre things like randomizing an array.
        /// </summary>
        /// <returns></returns>
        public static int Shift(this IRandom rng)
        {
            return (rng.Next(999) % 3) - 1;
        }

        public static UnityEngine.Vector3 OnUnitSphere(this IRandom rng)
        {
            //uniform, using angles
            var a = rng.Next() * Mathf.PI * 2f;
            var b = rng.Next() * Mathf.PI * 2f;
            var sa = Mathf.Sin(a);
            return new Vector3(sa * Mathf.Cos(b), sa * Mathf.Sin(b), Mathf.Cos(a));

            //non-uniform, needs to test for 0 vector
            /*
            var v = new UnityEngine.Vector3(Value, Value, Value);
            return (v == UnityEngine.Vector3.zero) ? UnityEngine.Vector3.right : v.normalized;
                */
        }

        public static UnityEngine.Vector2 OnUnitCircle(this IRandom rng)
        {
            //uniform, using angles
            var a = rng.Next() * Mathf.PI * 2f;
            return new Vector2(Mathf.Sin(a), Mathf.Cos(a));
        }

        public static UnityEngine.Vector3 InsideUnitSphere(this IRandom rng)
        {
            return rng.OnUnitSphere() * rng.Next();
        }

        public static UnityEngine.Vector2 InsideUnitCircle(this IRandom rng)
        {
            return rng.OnUnitCircle() * rng.Next();
        }

        public static UnityEngine.Vector3 InsideUnitBox(this IRandom rng)
        {
            return new Vector3(rng.Next() - 0.5f, rng.Next() - 0.5f, rng.Next() - 0.5f);
        }

        public static UnityEngine.Vector3 InsideBox(this IRandom rng, Vector3 size)
        {
            return new Vector3(rng.Next() * size.x - size.x * 0.5f, rng.Next() * size.y - size.y * 0.5f, rng.Next() * size.z - size.z * 0.5f);
        }

        public static UnityEngine.Vector3 InsideBoxExtents(this IRandom rng, Vector3 extents)
        {
            return new Vector3(rng.Next() * extents.x * 2f - extents.x, rng.Next() * extents.y * 2f - extents.y, rng.Next() * extents.z * 2f - extents.z);
        }

        public static UnityEngine.Vector3 AroundAxis(this IRandom rng, Vector3 axis)
        {
            var a = rng.Angle();
            if (VectorUtil.NearSameAxis(axis, Vector3.forward))
            {
                return Quaternion.AngleAxis(a, axis) * VectorUtil.GetForwardTangent(Vector3.up, axis);
            }
            else
            {
                return Quaternion.AngleAxis(a, axis) * VectorUtil.GetForwardTangent(Vector3.forward, axis);
            }
        }

        public static UnityEngine.Quaternion Rotation(this IRandom rng)
        {
            return UnityEngine.Quaternion.AngleAxis(rng.Angle(), rng.OnUnitSphere());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Select between min and max, exclussive of max.
        /// </summary>
        /// <param name="rng"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static float Range(this IRandom rng, float max, float min = 0.0f)
        {
            return rng.Next() * (max - min) + min;
        }

        /// <summary>
        /// Select between min and max, exclussive of max.
        /// </summary>
        /// <param name="rng"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static int Range(this IRandom rng, int max, int min = 0)
        {
            return rng.Next(min, max);
        }

        /// <summary>
        /// Select between min and max, exclussive of max.
        /// </summary>
        /// <param name="rng"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static long Range(this IRandom rng, long max, long min = 0)
        {
            return (long)((max - min) * rng.NextDouble()) + min;
        }

        /// <summary>
        /// Select between min and max, exclussive of max.
        /// </summary>
        /// <param name="rng"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static float Range(this IRandom rng, com.spacepuppy.Geom.Interval interval)
        {
            return rng.Next() * (interval.Max - interval.Min) + interval.Min;
        }

        /// <summary>
        /// Select an weighted index from 0 to length of weights.
        /// </summary>
        /// <param name="rng"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static int Range(this IRandom rng, params float[] weights)
        {
            int i;
            float w;
            float total = 0f;
            for (i = 0; i < weights.Length; i++)
            {
                w = weights[i];
                if (float.IsPositiveInfinity(w)) return i;
                else if (w >= 0f && !float.IsNaN(w)) total += w;
            }

            if (rng == null) rng = RandomUtil.Standard;
            if (total == 0f) return rng.Next(weights.Length);

            float r = rng.Next();
            float s = 0f;

            for (i = 0; i < weights.Length; i++)
            {
                w = weights[i];
                if (float.IsNaN(w) || w <= 0f) continue;

                s += w / total;
                if (s > r)
                {
                    return i;
                }
            }

            //should only get here if last element had a zero weight, and the r was large
            i = weights.Length - 1;
            while (i > 0 && weights[i] <= 0f) i--;
            return i;
        }

        /// <summary>
        /// Select an weighted index from 0 to length of weights.
        /// </summary>
        /// <param name="rng"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static int Range(this IRandom rng, float[] weights, int startIndex, int count = -1)
        {
            int i;
            float w;
            float total = 0f;
            int last = count < 0 ? weights.Length : System.Math.Min(startIndex + count, weights.Length);
            for (i = startIndex; i < last; i++)
            {
                w = weights[i];
                if (float.IsPositiveInfinity(w)) return i;
                else if (w >= 0f && !float.IsNaN(w)) total += w;
            }

            if (rng == null) rng = RandomUtil.Standard;
            if (total == 0f) return rng.Next(weights.Length);

            float r = rng.Next();
            float s = 0f;

            for (i = startIndex; i < last; i++)
            {
                w = weights[i];
                if (float.IsNaN(w) || w <= 0f) continue;

                s += w / total;
                if (s > r)
                {
                    return i;
                }
            }

            //should only get here if last element had a zero weight, and the r was large
            i = last - 1;
            while (i > 0 && weights[i] <= 0f) i--;
            return i;
        }

        #endregion




        #region Special Types

        private class UnityRNG : IRandom
        {

            public void SetSeed(int seed)
            {
                Random.InitState(seed);
            }

            public float Next()
            {
                //return Random.value;
                //because unity's Random returns in range 0->1, which is dumb
                //why you might say? Well it means that the 1 is the least likely value to generate, so for generating indices you get uneven results
                return Random.value * MAX_SINGLE_RATIO;
            }

            public double NextDouble()
            {
                //return (double)Random.value;
                //because unity's Random returns in range 0->1, which is dumb
                //why you might say? Well it means that the 1 is the least likely value to generate, so for generating indices you get uneven results
                return (double)Random.value * MAX_DOUBLE_RATIO;
            }

            public int Next(int size)
            {
                return (int)((double)size * NextDouble());
            }


            public int Next(int low, int high)
            {
                return (int)(NextDouble() * (high - low)) + low;
            }
        }

        public class MicrosoftRNG : System.Random, IRandom
        {

            public MicrosoftRNG() : base()
            {

            }

            public MicrosoftRNG(int seed) : base(seed < 0 ? (System.DateTime.Now.Ticks.GetHashCode() & 0x7FFFFFFF) : seed)
            {

            }

            public MicrosoftRNG(long seed) : base((seed < 0 ? System.DateTime.Now.Ticks : seed).GetHashCode() & 0x7FFFFFF)
            {

            }


            float IRandom.Next()
            {
                return (float)this.Next(MAX_SINGLE_DENOMINATOR) / (float)MAX_SINGLE_DENOMINATOR;
            }

            double IRandom.NextDouble()
            {
                return this.NextDouble();
            }

            int IRandom.Next(int size)
            {
                return this.Next(size);
            }

            int IRandom.Next(int low, int high)
            {
                return this.Next(low, high);
            }
        }

        /// <summary>
        /// A simple deterministic rng using a linear congruential algorithm. 
        /// Not the best, but fast and effective for deterministic rng for games.
        /// 
        /// Various known parameter configurations are included as static factory methods for ease of creating known long-period generators.
        /// See the wiki article for a list of more known long period parameters: https://en.wikipedia.org/wiki/Linear_congruential_generator
        /// </summary>
        public class LinearCongruentialRNG : IRandom
        {

            #region Fields

            private ulong _mode;
            private ulong _mult;
            private ulong _incr;
            private ulong _seed;

            private System.Func<double> _getNext;
            private System.Func<ulong> _getNextUlong;

            #endregion

            #region CONSTRUCTOR

            public LinearCongruentialRNG(long seed, ulong increment, ulong mult, ulong mode)
            {
                this.Reset(seed, increment, mult, mode);
            }

            #endregion

            #region Methods

            public void Reset(long seed, ulong increment, ulong mult, ulong mode)
            {
                _mode = mode;
                _mult = System.Math.Max(1, System.Math.Min(mode - 1, mult));
                _incr = System.Math.Max(0, System.Math.Min(mode - 1, increment));
                if (seed < 0)
                {
                    seed = System.DateTime.Now.Ticks;
                }

                if (_mode == 0)
                {
                    //this counts as using 2^64 as the mode
                    _seed = (ulong)seed;
                    _getNext = () =>
                    {
                        _seed = _mult * _seed + _incr;
                        return (double)(_seed % MAX_DOUBLE_DENOMINATOR) / (double)MAX_DOUBLE_DENOMINATOR;
                    };
                    _getNextUlong = () => (_seed = _mult * _seed + _incr);
                }
                else if (_mode > MAX_DOUBLE_DENOMINATOR)
                {
                    //double doesn't have the sig range to handle these
                    _seed = (ulong)seed % _mode;
                    _getNext = () =>
                    {
                        _seed = (_mult * _seed + _incr) % _mode;
                        return (double)(_seed % MAX_DOUBLE_DENOMINATOR) / (double)MAX_DOUBLE_DENOMINATOR;
                    };
                    _getNextUlong = () => (_seed = (_mult * _seed + _incr) % _mode);
                }
                else
                {
                    //just do the maths
                    _seed = (ulong)seed % _mode;
                    _getNext = () => (double)(_seed = (_mult * _seed + _incr) % _mode) / (double)(_mode);
                    _getNextUlong = () => (_seed = (_mult * _seed + _incr) % _mode);
                }
            }

            #endregion

            #region IRandom Interface

            public double NextDouble()
            {
                return _getNext();
            }

            public float Next()
            {
                return Next(MAX_SINGLE_DENOMINATOR) / (float)MAX_SINGLE_DENOMINATOR;
            }

            public int Next(int size)
            {
                ulong l = _getNextUlong();
                if (size > 0)
                {
                    return (int)(l % (ulong)size);
                }
                else if (size < 0)
                {
                    return -(int)(l % (ulong)(-size));
                }
                else
                {
                    return 0;
                }
            }

            public int Next(int low, int high)
            {
                return Next(high - low) + low;
            }

            #endregion

            #region Static Factory

            public static LinearCongruentialRNG CreateMMIXKnuth(long seed = -1)
            {
                return new LinearCongruentialRNG(seed, 1442695040888963407, 6364136223846793005, 0);
            }

            public static LinearCongruentialRNG CreateAppleCarbonLib(int seed = -1)
            {
                return new LinearCongruentialRNG(seed, 0, 16807, 16807);
            }

            public static LinearCongruentialRNG CreateGLibc(int seed = -1)
            {
                return new LinearCongruentialRNG(seed, 12345, 1103515245, 2147483648);
            }

            public static LinearCongruentialRNG CreateVB6(int seed = -1)
            {
                return new LinearCongruentialRNG(seed, 12820163, 1140671485, 16777216);
            }

            #endregion

        }

        public class SimplePCG : IRandom
        {

            #region Fields

            private ulong _seed;
            private ulong _inc;

            #endregion

            #region CONSTRUCTOR

            public SimplePCG(long seed = -1, ulong inc = 1)
            {
                this.Reset(seed, inc);
            }

            #endregion

            #region Methods

            public void Reset(long seed, ulong inc = 1)
            {
                if (seed < 0)
                {
                    seed = System.DateTime.Now.Ticks;
                }
                _seed = 0;
                _inc = (inc << 1) | 1;
                this.GetNext();
                _seed += (ulong)seed;
                this.GetNext();
            }

            private uint GetNext()
            {
                ulong old = _seed;
                _seed = old * 6364136223846793005 + _inc;
                uint xor = (uint)(((old >> 18) ^ old) >> 27);
                int rot = (int)(old >> 59);
                return (xor >> rot) | (xor << (64 - rot));
            }

            #endregion

            #region IRandom Interface

            public double NextDouble()
            {
                return (double)this.GetNext() / (double)(0x100000000u);
            }

            public float Next()
            {
                return Next(MAX_SINGLE_DENOMINATOR) / (float)MAX_SINGLE_DENOMINATOR;
            }

            public int Next(int size)
            {
                return (int)(size * this.NextDouble());
            }

            public int Next(int low, int high)
            {
                return (int)((high - low) * this.NextDouble()) + low;
            }

            #endregion

        }

        #endregion

    }
}
