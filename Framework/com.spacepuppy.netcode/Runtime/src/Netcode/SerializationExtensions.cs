using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Mathematics;

namespace com.spacepuppy.Netcode
{

    public static class SerializationExtensions
    {

        private static byte[] BUFFER_GUID = new byte[16];


        public static void RegisterRuntimeSerialization_Guid()
        {
            UserNetworkVariableSerialization<System.Guid>.ReadValue = ReadValueSafe;
            UserNetworkVariableSerialization<System.Guid>.WriteValue = WriteValueSafe;
            UserNetworkVariableSerialization<System.Guid>.DuplicateValue = DuplicateValueSafe;
            UserNetworkVariableSerialization<SerializableGuid>.ReadValue = ReadValueSafe;
            UserNetworkVariableSerialization<SerializableGuid>.WriteValue = WriteValueSafe;
            UserNetworkVariableSerialization<SerializableGuid>.DuplicateValue = DuplicateValueSafe;
        }

        public static void ReadValueSafe(this FastBufferReader reader, out System.Guid guid)
        {
            if (!reader.TryBeginRead(16))
            {
                throw new System.OverflowException("Not enough space in the buffer");
            }
            reader.ReadBytes(ref BUFFER_GUID, 16);
            guid = new System.Guid(BUFFER_GUID);
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in System.Guid guid)
        {
            if (!writer.TryBeginWrite(16))
            {
                throw new System.OverflowException("Not enough space in the buffer");
            }
            writer.WriteBytes(guid.ToByteArray());
        }

        public static void DuplicateValueSafe(in System.Guid value, ref System.Guid duplicatedValue)
        {
            duplicatedValue = value;
        }

        public static void SerializeValue<TReaderWriter>(this BufferSerializer<TReaderWriter> serializer, ref System.Guid guid) where TReaderWriter : IReaderWriter
        {
            SerializableGuid sguid = serializer.IsReader ? default(SerializableGuid) : (SerializableGuid)guid;
            serializer.SerializeValue(ref sguid.a);
            serializer.SerializeValue(ref sguid.b);
            serializer.SerializeValue(ref sguid.c);
            serializer.SerializeValue(ref sguid.d);
            serializer.SerializeValue(ref sguid.e);
            serializer.SerializeValue(ref sguid.f);
            serializer.SerializeValue(ref sguid.g);
            serializer.SerializeValue(ref sguid.h);
            serializer.SerializeValue(ref sguid.i);
            serializer.SerializeValue(ref sguid.j);
            serializer.SerializeValue(ref sguid.k);
            guid = sguid;
        }



        public static void ReadValueSafe(this FastBufferReader reader, out SerializableGuid guid)
        {
            if (!reader.TryBeginRead(16))
            {
                throw new System.OverflowException("Not enough space in the buffer");
            }
            reader.ReadBytes(ref BUFFER_GUID, 16);
            guid = new System.Guid(BUFFER_GUID);
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in SerializableGuid guid)
        {
            if (!writer.TryBeginWrite(16))
            {
                throw new System.OverflowException("Not enough space in the buffer");
            }
            writer.WriteBytes(guid.ToByteArray());
        }

        public static void DuplicateValueSafe(in SerializableGuid value, ref SerializableGuid duplicatedValue)
        {
            duplicatedValue = value;
        }

        public static void SerializeValue<TReaderWriter>(this BufferSerializer<TReaderWriter> serializer, ref SerializableGuid sguid) where TReaderWriter : IReaderWriter
        {
            if (serializer.IsReader)
            {
                sguid = default;
            }
            serializer.SerializeValue(ref sguid.a);
            serializer.SerializeValue(ref sguid.b);
            serializer.SerializeValue(ref sguid.c);
            serializer.SerializeValue(ref sguid.d);
            serializer.SerializeValue(ref sguid.e);
            serializer.SerializeValue(ref sguid.f);
            serializer.SerializeValue(ref sguid.g);
            serializer.SerializeValue(ref sguid.h);
            serializer.SerializeValue(ref sguid.i);
            serializer.SerializeValue(ref sguid.j);
            serializer.SerializeValue(ref sguid.k);
        }


        public static void RegisterRuntimeSerialization_Nullable<TValue>() where TValue : unmanaged, System.IComparable, System.IConvertible, System.IComparable<TValue>, System.IEquatable<TValue>
        {
            UserNetworkVariableSerialization<TValue?>.ReadValue = ReadNullableValueSafe<TValue>;
            UserNetworkVariableSerialization<TValue?>.WriteValue = WriteNullableValueSafe<TValue>;
            UserNetworkVariableSerialization<TValue?>.DuplicateValue = DuplicateValueSafe<TValue>;
        }

        private static void ReadNullableValueSafe<TValue>(this FastBufferReader reader, out TValue? value) where TValue : unmanaged, System.IComparable, System.IConvertible, System.IComparable<TValue>, System.IEquatable<TValue>
        {
            bool hasValue;
            TValue v;
            reader.ReadValueSafe(out hasValue);
            reader.ReadValueSafe(out v);
            value = hasValue ? v : null;
        }

        private static void WriteNullableValueSafe<TValue>(this FastBufferWriter writer, in TValue? value) where TValue : unmanaged, System.IComparable, System.IConvertible, System.IComparable<TValue>, System.IEquatable<TValue>
        {
            writer.WriteValueSafe(value.HasValue);
            writer.WriteValueSafe(value.HasValue ? value.Value : default);
        }

        public static void DuplicateValueSafe<TValue>(in TValue? value, ref TValue? duplicatedValue) where TValue : unmanaged, System.IComparable, System.IConvertible, System.IComparable<TValue>, System.IEquatable<TValue>
        {
            duplicatedValue = value;
        }

        public static void SerializeValue<TReaderWriter, TValue>(this BufferSerializer<TReaderWriter> serializer, ref TValue? value) where TReaderWriter : IReaderWriter where TValue : unmanaged, System.IComparable, System.IConvertible, System.IComparable<TValue>, System.IEquatable<TValue>
        {
            bool hasValue;
            TValue v;
            if (serializer.IsReader)
            {
                hasValue = default;
                v = default;
                serializer.SerializeValue(ref hasValue);
                serializer.SerializeValue(ref v);
                value = hasValue ? v : null;
            }
            else
            {
                hasValue = value.HasValue;
                v = value.HasValue ? value.Value : default;
                serializer.SerializeValue(ref hasValue);
                serializer.SerializeValue(ref v);
            }
        }



        public static void SerializeValue<TReaderWriter>(this BufferSerializer<TReaderWriter> serializer, ref float2 value) where TReaderWriter : IReaderWriter
        {
            Vector2 v;
            if (serializer.IsReader)
            {
                v = default;
                serializer.SerializeValue(ref v);
                value = v;
            }
            else
            {
                v = value;
                serializer.SerializeValue(ref v);
            }
        }

        public static void SerializeValue<TReaderWriter>(this BufferSerializer<TReaderWriter> serializer, ref float3 value) where TReaderWriter : IReaderWriter
        {
            Vector3 v;
            if (serializer.IsReader)
            {
                v = default;
                serializer.SerializeValue(ref v);
                value = v;
            }
            else
            {
                v = value;
                serializer.SerializeValue(ref v);
            }
        }

        public static void SerializeValue<TReaderWriter>(this BufferSerializer<TReaderWriter> serializer, ref float4 value) where TReaderWriter : IReaderWriter
        {
            Vector4 v;
            if (serializer.IsReader)
            {
                v = default;
                serializer.SerializeValue(ref v);
                value = v;
            }
            else
            {
                v = value;
                serializer.SerializeValue(ref v);
            }
        }

    }

}
