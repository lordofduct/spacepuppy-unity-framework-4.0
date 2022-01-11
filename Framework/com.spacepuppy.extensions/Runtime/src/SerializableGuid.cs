using UnityEngine;

namespace com.spacepuppy
{

    [System.Serializable]
    public struct SerializableGuid
    {

        #region Fields

        [SerializeField]
        private int a;
        [SerializeField]
        private short b;
        [SerializeField]
        private short c;
        [SerializeField]
        private byte d;
        [SerializeField]
        private byte e;
        [SerializeField]
        private byte f;
        [SerializeField]
        private byte g;
        [SerializeField]
        private byte h;
        [SerializeField]
        private byte i;
        [SerializeField]
        private byte j;
        [SerializeField]
        private byte k;

#endregion

#region CONSTRUCTOR

        public SerializableGuid(System.Guid guid)
        {
            var arr = guid.ToByteArray();
            a = System.BitConverter.ToInt32(arr, 0);
            b = System.BitConverter.ToInt16(arr, 4);
            c = System.BitConverter.ToInt16(arr, 6);
            d = arr[8];
            e = arr[9];
            f = arr[10];
            g = arr[11];
            h = arr[12];
            i = arr[13];
            j = arr[14];
            k = arr[15];
        }

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

#endregion

#region Methods

        public override string ToString()
        {
            return this.ToGuid().ToString();
        }

        public string ToString(string format)
        {
            return this.ToGuid().ToString(format);
        }

        public System.Guid ToGuid()
        {
            return new System.Guid(a, b, c, d, e, f, g, h, i, j, k);
        }

        public override bool Equals(object obj)
        {
            if(obj is SerializableGuid sg)
            {
                return this.ToGuid() == sg.ToGuid();
            }
            else if(obj is System.Guid g)
            {
                return this.ToGuid() == g;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.ToGuid().GetHashCode();
        }

#endregion

#region Static Utils

        public static SerializableGuid NewGuid()
        {
            return new SerializableGuid(System.Guid.NewGuid());
        }

        public static implicit operator System.Guid(SerializableGuid guid)
        {
            return guid.ToGuid();
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
        }

#endregion

    }

}
