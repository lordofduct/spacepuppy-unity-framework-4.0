using UnityEngine;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace com.spacepuppy
{

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct SerializableGuid
    {

        #region Fields

        [SerializeField]
        public int a;
        [SerializeField]
        public short b;
        [SerializeField]
        public short c;
        [SerializeField]
        public byte d;
        [SerializeField]
        public byte e;
        [SerializeField]
        public byte f;
        [SerializeField]
        public byte g;
        [SerializeField]
        public byte h;
        [SerializeField]
        public byte i;
        [SerializeField]
        public byte j;
        [SerializeField]
        public byte k;

        #endregion

        #region CONSTRUCTOR

        public SerializableGuid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.e = e;
            this.f = f;
            this.g = g;
            this.h = h;
            this.i = i;
            this.j = j;
            this.k = k;
        }

        public SerializableGuid(ulong high, ulong low)
        {
            a = (int)(high >> 32);
            b = (short)(high >> 16);
            c = (short)high;
            d = (byte)(low >> 56);
            e = (byte)(low >> 48);
            f = (byte)(low >> 40);
            g = (byte)(low >> 32);
            h = (byte)(low >> 24);
            i = (byte)(low >> 16);
            j = (byte)(low >> 8);
            k = (byte)low;
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ToByteArray() => this.ToGuid().ToByteArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => this.ToGuid().ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format) => this.ToGuid().ToString(format);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public System.Guid ToGuid() => new System.Guid(a, b, c, d, e, f, g, h, i, j, k);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(SerializableGuid guid) => guid.ToGuid().Equals(this.ToGuid());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(System.Guid guid) => guid.Equals(this.ToGuid());

        public override bool Equals(object obj)
        {
            if (obj is SerializableGuid sg)
            {
                return this.ToGuid() == sg.ToGuid();
            }
            else if (obj is System.Guid g)
            {
                return this.ToGuid() == g;
            }
            else
            {
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => this.ToGuid().GetHashCode();

        #endregion

        #region Static Utils

        public static readonly SerializableGuid Empty = default;

        public static SerializableGuid NewGuid()
        {
            return System.Guid.NewGuid();
        }

        public static SerializableGuid Parse(string input)
        {
            return System.Guid.Parse(input);
        }

        public static bool TryParse(string input, out SerializableGuid result)
        {
            System.Guid output;
            if (System.Guid.TryParse(input, out output))
            {
                result = output;
                return true;
            }
            else
            {
                result = default(SerializableGuid);
                return false;
            }
        }

        public static bool Coerce(object input, out SerializableGuid result)
        {
            System.Guid guid;
            if (CoerceGuid(input, out guid))
            {
                result = guid;
                return true;
            }
            else
            {
                result = default(SerializableGuid);
                return false;
            }
        }

        public static bool CoerceGuid(object input, out System.Guid result)
        {
            switch (input)
            {
                case System.Guid guid:
                    result = guid;
                    return true;
                case SerializableGuid sguid:
                    result = sguid.ToGuid();
                    return true;
                case string s:
                    return System.Guid.TryParse(s, out result);
                case object o:
                    return System.Guid.TryParse(o.ToString(), out result);
                default:
                    result = default(SerializableGuid);
                    return false;
            }
        }

        public static implicit operator System.Guid(SerializableGuid guid)
        {
            return guid.ToGuid();
        }

        public unsafe static implicit operator SerializableGuid(System.Guid guid)
        {
            return *(SerializableGuid*)&guid;
        }

        public static bool operator ==(SerializableGuid a, SerializableGuid b)
        {
            return a.ToGuid() == b.ToGuid();
        }

        public static bool operator ==(System.Guid a, SerializableGuid b)
        {
            return a == b.ToGuid();
        }

        public static bool operator ==(SerializableGuid a, System.Guid b)
        {
            return a.ToGuid() == b;
        }

        public static bool operator !=(SerializableGuid a, SerializableGuid b)
        {
            return a.ToGuid() != b.ToGuid();
        }

        public static bool operator !=(System.Guid a, SerializableGuid b)
        {
            return a != b.ToGuid();
        }

        public static bool operator !=(SerializableGuid a, System.Guid b)
        {
            return a.ToGuid() != b;
        }

        #endregion

        #region Special Types

        public class ConfigAttribute : System.Attribute
        {
            public bool AllowZero;
            /// <summary>
            /// Attempts to make the guid match the guid associated with the asset this is on. 
            /// Note this only works if it's on an asset that exists on disk (ScriptableObject, Prefab). 
            /// Also it means guids will match across all scripts that are on a prefab with this flagged. (since the prefab only has one guid)
            /// Really... this should be mainly done with ScriptableObjects only, Prefab's are just a 
            /// sort of bonus but with caveats.
            /// 
            /// New instances created via 'Instantiate' or 'CreateInstance' will not get anything. 
            /// This is editor time only for assets on disk! 
            /// </summary>
            public bool LinkToAsset;

            /// <summary>
            /// Attempts to make the guid match the targetObjectId & targetPrefabId of the GlobalObjectId from the object 
            /// for the upper and lower portions of the guid respectively. If LinkToAsset is true, that takes precedance to this. 
            /// </summary>
            public bool LinkToGlobalObjectId;

            /// <summary>
            /// The guid will be displayed as an object reference field showing the asset related to the guid. 
            /// Dragging an object onto said field will update its value unless any LinkTo* is true.
            /// </summary>
            public bool ObjectRefField;
        }

        #endregion

    }

}
