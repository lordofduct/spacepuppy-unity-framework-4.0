using System;
using UnityEngine;

namespace com.spacepuppy
{

    /// <summary>
    /// General interface for custom numeric types like fixedpoint, discrete float, etc. 
    /// Numerics should usually be a struct type, but is not required.
    /// </summary>
    public interface INumeric : IConvertible
    {

        /// <summary>
        /// Returns the byte reprsentation of the numeric value.
        /// When implementing this methods, if you need to convert C# built-in numeric types make sure they're in big-endian. 
        /// Use the 'Numerics' static class as a helper tool to do this.
        /// </summary>
        /// <returns></returns>
        byte[] ToByteArray();
        /// <summary>
        /// Sets the numeric value based on some byte array.
        /// When implementing this methods, if you need to convert C# built-in numeric types make sure they're in big-endian. 
        /// Use the 'Numerics' static class as a helper tool to do this.
        /// </summary>
        /// <param name="arr"></param>
        void FromByteArray(byte[] arr);

        /// <summary>
        /// Get the type code the underlying data can losslessly be converted to for easy storing.
        /// </summary>
        /// <returns></returns>
        TypeCode GetUnderlyingTypeCode();

        /// <summary>
        /// Set value based on a long.
        /// </summary>
        /// <param name="value"></param>
        void FromLong(long value);

        /// <summary>
        /// Set value based on a double.
        /// </summary>
        /// <param name="value"></param>
        void FromDouble(double value);

    }

    public static class Numerics
    {

        public static INumeric CreateNumeric<T>(byte[] data) where T : INumeric
        {
            var value = System.Activator.CreateInstance<T>();
            if (value != null) value.FromByteArray(data);
            return value;
        }

        public static INumeric CreateNumeric<T>(long data) where T : INumeric
        {
            var value = System.Activator.CreateInstance<T>();
            if (value != null) value.FromLong(data);
            return value;
        }

        public static INumeric CreateNumeric<T>(double data) where T : INumeric
        {
            var value = System.Activator.CreateInstance<T>();
            if (value != null) value.FromDouble(data);
            return value;
        }

        public static INumeric CreateNumeric(System.Type tp, byte[] data)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");
            if (!typeof(INumeric).IsAssignableFrom(tp) && !tp.IsAbstract) throw new System.ArgumentException("Type must implement INumeric.");

            var value = System.Activator.CreateInstance(tp) as INumeric;
            if (value != null) value.FromByteArray(data);
            return value;
        }

        public static INumeric CreateNumeric(System.Type tp, long data)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");
            if (!typeof(INumeric).IsAssignableFrom(tp) && !tp.IsAbstract) throw new System.ArgumentException("Type must implement INumeric.");

            var value = System.Activator.CreateInstance(tp) as INumeric;
            if (value != null) value.FromLong(data);
            return value;
        }

        public static INumeric CreateNumeric(System.Type tp, double data)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");
            if (!typeof(INumeric).IsAssignableFrom(tp) && !tp.IsAbstract) throw new System.ArgumentException("Type must implement INumeric.");

            var value = System.Activator.CreateInstance(tp) as INumeric;
            if (value != null) value.FromDouble(data);
            return value;
        }


        #region Bit Helpers

        /// <summary>
        /// Returns a byte representation in big-endian order.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(float value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) System.Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Returns a byte representation in big-endian order.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(double value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) System.Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Returns a byte representation in big-endian order.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(Int16 value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) System.Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Returns a byte representation in big-endian order.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(Int32 value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) System.Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Returns a byte representation in big-endian order.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(Int64 value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) System.Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Returns a byte representation in big-endian order.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(UInt16 value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) System.Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Returns a byte representation in big-endian order.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(UInt32 value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) System.Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Returns a byte representation in big-endian order.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(UInt64 value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) System.Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Converts a byte array in big-endian form to a numeric value.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static float ToSingle(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                System.Array.Reverse(arr);
            return BitConverter.ToSingle(arr, 0);
        }

        /// <summary>
        /// Converts a byte array in big-endian form to a numeric value.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static double ToDouble(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                System.Array.Reverse(arr);
            return BitConverter.ToDouble(arr, 0);
        }

        /// <summary>
        /// Converts a byte array in big-endian form to a numeric value.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static Int16 ToInt16(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                System.Array.Reverse(arr);
            return BitConverter.ToInt16(arr, 0);
        }

        /// <summary>
        /// Converts a byte array in big-endian form to a numeric value.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static Int32 ToInt32(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                System.Array.Reverse(arr);
            return BitConverter.ToInt32(arr, 0);
        }

        /// <summary>
        /// Converts a byte array in big-endian form to a numeric value.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static Int64 ToInt64(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                System.Array.Reverse(arr);
            return BitConverter.ToInt64(arr, 0);
        }

        /// <summary>
        /// Converts a byte array in big-endian form to a numeric value.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static UInt16 ToUInt16(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                System.Array.Reverse(arr);
            return BitConverter.ToUInt16(arr, 0);
        }

        /// <summary>
        /// Converts a byte array in big-endian form to a numeric value.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static UInt32 ToUInt32(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                System.Array.Reverse(arr);
            return BitConverter.ToUInt32(arr, 0);
        }

        /// <summary>
        /// Converts a byte array in big-endian form to a numeric value.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static UInt64 ToUInt64(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                System.Array.Reverse(arr);
            return BitConverter.ToUInt64(arr, 0);
        }

        #endregion

    }

}
