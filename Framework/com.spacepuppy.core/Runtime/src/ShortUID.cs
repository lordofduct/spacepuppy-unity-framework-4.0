using System;

namespace com.spacepuppy
{

    /// <summary>
    /// A serializable semi-unique id. It is not universely unique, but will be system unique at the least, and should be team unique confidently.
    /// It consists of the current ticks whose upper 3 bytes are xor'd by a large random number.
    /// </summary>
    [System.Serializable]
    public struct ShortUid : IEquatable<ShortUid>
    {

        public static readonly ShortUid Zero = default;

        #region Fields

        //has to be stored with uint's
        //unity has a bug at the time of writing this where long doesn't serialize in prefabs correctly
        //there is a fix in unity beta 2017.1, but we are unsure as to when the full release will be out
        //so stuck with this hack fix to maintain backwards compatibility
        [UnityEngine.SerializeField]
        private uint _low;
        [UnityEngine.SerializeField]
        private uint _high;

        #endregion

        #region CONSTRUCTOR

        public ShortUid(long value)
        {
            _low = (uint)(value & uint.MaxValue);
            _high = (uint)((ulong)value >> 32);
        }

        public ShortUid(ulong value)
        {
            _low = (uint)(value & uint.MaxValue);
            _high = (uint)(value >> 32);
        }

        public ShortUid(uint high, uint low)
        {
            _low = low;
            _high = high;
        }

        private static long _seed = System.DateTime.UtcNow.Ticks;
        public static ShortUid NewId()
        {
            _seed = 6364136223846793005L * _seed + 1442695040888963407L; //MMIX Knuth LCG
            return new ShortUid(System.DateTime.UtcNow.Ticks ^ (_seed << 40));
        }

        #endregion

        #region Properties

        public ulong Value
        {
            get
            {
                return ((ulong)_high << 32) | (ulong)_low;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the base64 encoded guid as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Value.ToString("X16");
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a
        /// specified Object represent the same type and value.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is ShortUid)
            {
                var uid = (ShortUid)obj;
                return uid._high == _high && uid._low == _low;
            }
            return false;
        }

        public bool Equals(ShortUid uid)
        {
            return uid._high == _high && uid._low == _low;
        }

        /// <summary>
        /// Returns the HashCode for underlying Guid.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int)(_high ^ _low);
        }

        /// <summary>
        /// Places the ShortUid as the leading 64-bits of a guid, the rest as zeros.
        /// </summary>
        /// <returns></returns>
        public System.Guid ToGuid()
        {
            short mid = (short)(_high & ushort.MaxValue);
            short high = (short)(_high >> 16);
            return new System.Guid((int)_low, mid, high, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        /// <summary>
        /// Places the ShortUid as the leading 64-bits of a guid, the rest is the suffix.
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public System.Guid ToGuid(long suffix)
        {
            short mid = (short)(_high & ushort.MaxValue);
            short high = (short)(_high >> 16);
            return new System.Guid((int)_low, mid, high,
                                   (byte)(suffix >> 56),
                                   (byte)(suffix >> 48),
                                   (byte)(suffix >> 40),
                                   (byte)(suffix >> 32),
                                   (byte)(suffix >> 24),
                                   (byte)(suffix >> 16),
                                   (byte)(suffix >> 8),
                                   (byte)(suffix));
        }

        /// <summary>
        /// Places the ShortUid as the leading 64-bits of a guid, the rest is the suffix.
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public System.Guid ToGuid(byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            short mid = (short)(_high & ushort.MaxValue);
            short high = (short)(_high >> 16);
            return new System.Guid((int)_low, mid, high, d, e, f, g, h, i, j, k);
        }

        /// <summary>
        /// Places the ShortUid as the leading 64-bits of a guid, the rest is the first 8 characters of the suffix string as ascii codes. 
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public System.Guid ToGuid(string suffix)
        {
            short mid = (short)(_high & ushort.MaxValue);
            short high = (short)(_high >> 16);
            int len = suffix?.Length ?? 0;
            return new System.Guid((int)_low, mid, high,
                                   (len > 0) ? (byte)suffix[0] : (byte)0,
                                   (len > 1) ? (byte)suffix[1] : (byte)0,
                                   (len > 2) ? (byte)suffix[2] : (byte)0,
                                   (len > 3) ? (byte)suffix[3] : (byte)0,
                                   (len > 4) ? (byte)suffix[4] : (byte)0,
                                   (len > 5) ? (byte)suffix[5] : (byte)0,
                                   (len > 6) ? (byte)suffix[6] : (byte)0,
                                   (len > 7) ? (byte)suffix[7] : (byte)0);
        }

        #endregion

        #region Conversion

        public static ShortUid ToShortUid(System.Guid uid)
        {
            var arr = uid.ToByteArray();
            ulong low = (ulong)((int)(arr[3] << 24) | (int)(arr[2] << 16) | (int)(arr[1] << 8) | (int)arr[0]);
            ulong high = (ulong)((int)(arr[7] << 24) | (int)(arr[6] << 16) | (int)(arr[5] << 8) | (int)arr[4]) << 32;
            return new ShortUid(high | low);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if both ShortUids have the same underlying
        /// Guid value.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(ShortUid x, ShortUid y)
        {
            return x._high == y._high && x._low == y._low;
        }

        /// <summary>
        /// Determines if both ShortUids do not have the
        /// same underlying Guid value.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(ShortUid x, ShortUid y)
        {
            return x._high != y._high || x._low != y._low;
        }

        /// <summary>
        /// Implicitly converts the ShortUid to it's string equivilent
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static implicit operator string(ShortUid uid)
        {
            return uid.ToString();
        }

        public static implicit operator long(ShortUid uid)
        {
            return (long)uid.Value;
        }

        public static implicit operator ulong(ShortUid uid)
        {
            return uid.Value;
        }

        #endregion

        #region Special Types

        public class ConfigAttribute : Attribute
        {
            public bool ReadOnly;
            public bool AllowZero;
            public bool LinkToGlobalId;
        }

        #endregion

    }

    /// <summary>
    /// Similar to ShortUid in that it can store the same numeric value, but can also be customized to be a unique string instead. 
    /// ShortUid can be implicitly converted to TokenId.
    /// </summary>
    [System.Serializable]
    public struct TokenId
    {

        public static readonly TokenId Empty = new TokenId();

        #region Fields

        [UnityEngine.SerializeField]
        private uint _low;
        [UnityEngine.SerializeField]
        private uint _high;
        [UnityEngine.SerializeField]
        private string _id;

        #endregion

        #region CONSTRUCTOR

        public TokenId(long value)
        {
            _low = (uint)(value & uint.MaxValue);
            _high = (uint)((ulong)value >> 32);
            _id = null;
        }

        public TokenId(ulong value)
        {
            _low = (uint)(value & uint.MaxValue);
            _high = (uint)(value >> 32);
            _id = null;
        }

        public TokenId(uint high, uint low)
        {
            _low = low;
            _high = high;
            _id = null;
        }

        public TokenId(string value)
        {
            _low = 0;
            _high = 0;
            _id = value;
        }

        public TokenId(ShortUid value) : this(value.Value)
        {
        }

        public static TokenId NewId()
        {
            return new TokenId(ShortUid.NewId());
        }

        #endregion

        #region Properties

        public bool HasValue
        {
            get { return !string.IsNullOrEmpty(_id) || _low != 0 || _high != 0; }
        }

        public ulong LongValue
        {
            get
            {
                return ((ulong)_high << 32) | (ulong)_low;
            }
        }

        public bool IsLong
        {
            get
            {
                return string.IsNullOrEmpty(_id);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the id as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.IsLong)
                return this.LongValue.ToString("X16");
            else
                return _id ?? string.Empty;
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a
        /// specified Object represent the same type and value.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is TokenId)
            {
                return this.Equals((TokenId)obj);
            }
            else if (obj is ShortUid)
            {
                return this.IsLong && ((ShortUid)obj).Value == this.LongValue;
            }
            return false;
        }

        public bool Equals(TokenId id)
        {
            if (this.IsLong)
            {
                return id.IsLong && this._high == id._high && this._low == id._low;
            }
            else
            {
                return !id.IsLong && this._id == id._id;
            }
        }

        public bool Equals(ShortUid uid)
        {
            return this.IsLong && this.LongValue == uid.Value;
        }

        /// <summary>
        /// Returns the HashCode for underlying id.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (this.IsLong)
                return (int)(_high ^ _low);
            else
                return _id.GetHashCode();
        }

        public System.Guid ToGuid()
        {
            if (string.IsNullOrEmpty(_id))
            {
                short mid = (short)(_high & ushort.MaxValue);
                short high = (short)(_high >> 16);
                return new System.Guid((int)_low, mid, high, 0, 0, 0, 0, 0, 0, 0, 0);
            }

            lock (_buffer)
            {
                System.Array.Clear(_buffer, 0, 16);
                GetBytes(_buffer);
                return new System.Guid(_buffer);
            }
        }

        /// <summary>
        /// Places the ShortUid as the leading 64-bits of a guid, the rest is the suffix.
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public System.Guid ToGuid(long suffix)
        {
            if (string.IsNullOrEmpty(_id)) return (new ShortUid(_high, _low)).ToGuid(suffix);

            lock (_buffer)
            {
                System.Array.Clear(_buffer, 0, 16);
                GetBytes(_buffer);
                _buffer[8] ^= (byte)(suffix >> 56);
                _buffer[9] ^= (byte)(suffix >> 48);
                _buffer[10] ^= (byte)(suffix >> 40);
                _buffer[11] ^= (byte)(suffix >> 32);
                _buffer[12] ^= (byte)(suffix >> 24);
                _buffer[13] ^= (byte)(suffix >> 16);
                _buffer[14] ^= (byte)(suffix >> 8);
                _buffer[15] ^= (byte)(suffix);
                return new System.Guid(_buffer);
            }
        }

        /// <summary>
        /// Places the ShortUid as the leading 64-bits of a guid, the rest is the suffix.
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public System.Guid ToGuid(byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            if (string.IsNullOrEmpty(_id)) return (new ShortUid(_high, _low)).ToGuid(d, e, f, g, h, i, j, k);

            lock (_buffer)
            {
                System.Array.Clear(_buffer, 0, 16);
                GetBytes(_buffer);
                _buffer[8] ^= d;
                _buffer[9] ^= e;
                _buffer[10] ^= f;
                _buffer[11] ^= g;
                _buffer[12] ^= h;
                _buffer[13] ^= i;
                _buffer[14] ^= j;
                _buffer[15] ^= k;
                return new System.Guid(_buffer);
            }
        }

        public System.Guid ToGuid(string suffix)
        {
            if (string.IsNullOrEmpty(_id)) return (new ShortUid(_high, _low)).ToGuid(suffix);

            lock (_buffer)
            {
                System.Array.Clear(_buffer, 0, 16);
                GetBytes(_buffer);

                int len = suffix?.Length ?? 0;
                if (len < 8) len = 8;
                for (int i = 0; i < len; i++)
                {
                    _buffer[i + 8] ^= (byte)suffix[i];
                }

                return new System.Guid(_buffer);
            }
        }

        private static byte[] _buffer = new byte[16];
        private void GetBytes(byte[] buffer)
        {
            if (string.IsNullOrEmpty(_id))
            {
                //this mimics the same format as ShortUid.ToGuid
                buffer[3] = (byte)(_low >> 24);
                buffer[2] = (byte)(_low >> 16);
                buffer[1] = (byte)(_low >> 8);
                buffer[0] = (byte)(_low);
                buffer[7] = (byte)(_high >> 24);
                buffer[6] = (byte)(_high >> 16);
                buffer[5] = (byte)(_high >> 8);
                buffer[4] = (byte)(_high);
            }
            else
            {
                for (int i = 0; i < _id.Length; i++)
                {
                    buffer[i % 16] ^= (byte)_id[i];
                }
            }
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if both TokenId have the same underlying
        /// id value.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(TokenId x, TokenId y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Determines if both TokenId do not have the
        /// same underlying id value.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(TokenId x, TokenId y)
        {
            return !x.Equals(y);
        }

        /// <summary>
        /// Implicitly converts the TokenId to it's string equivilent
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static implicit operator string(TokenId uid)
        {
            return uid.ToString();
        }

        /// <summary>
        /// Implicitly converts from a ShortUid to a TokenId
        /// </summary>
        /// <param name="uid"></param>
        public static implicit operator TokenId(ShortUid uid)
        {
            return new TokenId(uid.Value);
        }

        #endregion

        #region Special Types

        public class ConfigAttribute : ShortUid.ConfigAttribute { }

        #endregion

    }

}
