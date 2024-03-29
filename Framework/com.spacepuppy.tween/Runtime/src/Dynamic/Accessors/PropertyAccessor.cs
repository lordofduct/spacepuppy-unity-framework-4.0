﻿//
// FastDynamicMemberAccessor.cs
//
// Author: James Nies
// Licensed under The Code Project Open License (CPOL): http://www.codeproject.com/info/cpol10.aspx

using com.spacepuppy.Utils;
using System;
using System.Reflection;

#if ENABLE_MONO && NET_4_6
using System.Reflection.Emit;
#endif


namespace com.spacepuppy.Dynamic.Accessors
{

    /// <summary>
    /// The PropertyAccessor class provides fast dynamic access
    /// to a property of a specified target class.
    /// </summary>
    internal class PropertyAccessor : MemberAccessor
    {

        protected readonly PropertyInfo _propInfo;

        /// <summary>
        /// Creates a new property accessor.
        /// </summary>
        internal PropertyAccessor(PropertyInfo info)
            : base(info)
        {
            _propInfo = info;
        }

        internal override string MemberName {  get { return _propInfo.Name; } }

        /// <summary>
        /// The Type of the Property being accessed.
        /// </summary>
        internal override Type MemberType
        {
            get
            {
                return _propInfo.PropertyType;
            }
        }

        /// <summary>
        /// Whether or not the Property supports read access.
        /// </summary>
        internal override bool CanRead
        {
            get
            {
                return _propInfo.CanRead;
            }
        }

        /// <summary>
        /// Whether or not the Property supports write access.
        /// </summary>
        internal override bool CanWrite
        {
            get
            {
                return _propInfo.CanWrite;
            }
        }

#if ENABLE_MONO && NET_4_6 && ENABLE_SPTWEEN_EMIT

        protected override void _EmitSetter(TypeBuilder myType)
        {
            //
            // Define a method for the set operation.
            //
            Type[] setParamTypes = new[] { typeof(object), typeof(object) };
            Type setReturnType = null;
            MethodBuilder setMethod =
                myType.DefineMethod("Set",
                                    MethodAttributes.Public | MethodAttributes.Virtual,
                                    setReturnType,
                                    setParamTypes);

            //
            // From the method, get an ILGenerator. This is used to
            // emit the IL that we want.
            //
            ILGenerator setIL = setMethod.GetILGenerator();
            //
            // Emit the IL.
            //

            MethodInfo targetSetMethod = _targetType.GetMethod("set_" + _fieldName);
            if (targetSetMethod != null)
            {
                Type paramType = targetSetMethod.GetParameters()[0].ParameterType;

                setIL.DeclareLocal(paramType);
                setIL.Emit(OpCodes.Ldarg_1); //Load the first argument
                //(target object)

                setIL.Emit(OpCodes.Castclass, _targetType); //Cast to the source type

                setIL.Emit(OpCodes.Ldarg_2); //Load the second argument
                //(value object)

                if (paramType.IsValueType)
                {
                    setIL.Emit(OpCodes.Unbox, paramType); //Unbox it
                    if (s_TypeHash[paramType] != null) //and load
                    {
                        OpCode load = (OpCode)s_TypeHash[paramType];
                        setIL.Emit(load);
                    }
                    else
                    {
                        setIL.Emit(OpCodes.Ldobj, paramType);
                    }
                }
                else
                {
                    setIL.Emit(OpCodes.Castclass, paramType); //Cast class
                }

                setIL.EmitCall(OpCodes.Callvirt,
                               targetSetMethod, null); //Set the property value
            }
            else
            {
                setIL.ThrowException(typeof(MissingMethodException));
            }

            setIL.Emit(OpCodes.Ret);
        }

        protected override void _EmitGetter(TypeBuilder myType)
        {
            //
            // Define a method for the get operation.
            //
            Type[] getParamTypes = new[] { typeof(object) };
            Type getReturnType = typeof(object);
            MethodBuilder getMethod =
                myType.DefineMethod("Get",
                                    MethodAttributes.Public | MethodAttributes.Virtual,
                                    getReturnType,
                                    getParamTypes);

            //
            // From the method, get an ILGenerator. This is used to
            // emit the IL that we want.
            //
            ILGenerator getIL = getMethod.GetILGenerator();


            //
            // Emit the IL.
            //
            MethodInfo targetGetMethod = _targetType.GetMethod("get_" + _fieldName);

            if (targetGetMethod != null)
            {
                getIL.DeclareLocal(typeof(object));
                getIL.Emit(OpCodes.Ldarg_1); //Load the first argument
                //(target object)

                getIL.Emit(OpCodes.Castclass, _targetType); //Cast to the source type

                getIL.EmitCall(OpCodes.Call, targetGetMethod, null); //Get the property value

                if (targetGetMethod.ReturnType.IsValueType)
                {
                    getIL.Emit(OpCodes.Box, targetGetMethod.ReturnType); //Box if necessary
                }
                getIL.Emit(OpCodes.Stloc_0); //Store it

                getIL.Emit(OpCodes.Ldloc_0);
            }
            else
            {
                getIL.ThrowException(typeof(MissingMethodException));
            }

            getIL.Emit(OpCodes.Ret);
        }

#else

        public override object Get(object target)
        {
            return _propInfo.GetValue(target);
        }

        public override void Set(object target, object value)
        {
            _propInfo.SetValue(target, value);
        }

#endif

    }

    internal class PropertyAccessor<TargetType, MemberType> : PropertyAccessor, IMemberAccessor<MemberType>
    {

        private Action<TargetType, MemberType> _setter;
        private Func<TargetType, MemberType> _getter;

        public PropertyAccessor(PropertyInfo info) : base(info)
        {
            if (!TypeUtil.IsType(typeof(TargetType), info.DeclaringType)) throw new ArgumentException("TargetType must match the declaring type of the PropertyInfo", nameof(info));
            if (!TypeUtil.IsType(typeof(MemberType), info.PropertyType)) throw new ArgumentException("MemberType must match the proeprty type of the PropertyInfo", nameof(info));

            if (info.CanWrite) _setter = (Action<TargetType, MemberType>)Delegate.CreateDelegate(typeof(Action<TargetType, MemberType>), _propInfo.GetSetMethod());
            if (info.CanRead) _getter = (Func<TargetType, MemberType>)Delegate.CreateDelegate(typeof(Func<TargetType, MemberType>), _propInfo.GetGetMethod());
        }

        public void Set(object target, MemberType value)
        {
            if (target is TargetType targ)
            {
                _setter?.Invoke(targ, value);
            }
        }

        public new MemberType Get(object target)
        {
            if(target is TargetType targ)
            {
                return _getter != null ? _getter(targ) : default(MemberType);
            }
            else
            {
                return default(MemberType);
            }
        }
    }

}