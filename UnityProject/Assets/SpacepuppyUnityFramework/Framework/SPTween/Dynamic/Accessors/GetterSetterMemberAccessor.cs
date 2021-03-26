using System;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Dynamic.Accessors
{
    public delegate TProp MemberGetter<T, TProp>(T targ);
    public delegate void MemberSetter<T, TProp>(T targ, TProp value);

    public class GetterSetterMemberAccessor<T, TProp> : IMemberAccessor<TProp> where T : class
    {

        #region Fields

        private MemberGetter<T, TProp> _getter;
        private MemberSetter<T, TProp> _setter;

        #endregion

        #region Constructor

        public GetterSetterMemberAccessor(MemberGetter<T, TProp> getter, MemberSetter<T, TProp> setter)
        {
            if (getter == null) throw new System.ArgumentNullException(nameof(getter));
            if (setter == null) throw new System.ArgumentNullException(nameof(setter));
            _getter = getter;
            _setter = setter;
        }

        #endregion

        #region IMemberAccessor Interface

        public virtual string GetMemberName()
        {
            return typeof(TProp).Name + " Getter/Setter";
        }

        public Type GetMemberType()
        {
            return typeof(TProp);
        }

        public TProp Get(object target)
        {
            return _getter(target as T);
        }

        public void Set(object target, TProp value)
        {
            _setter(target as T, value);
        }

        protected virtual void BoxedSet(object target, object value)
        {
            this.Set(target, ConvertUtil.Coerce<TProp>(value));
        }

        void IMemberAccessor.Set(object target, object value)
        {
            this.BoxedSet(target, value);
        }

        object IMemberAccessor.Get(object target)
        {
            return this.Get(target);
        }

        #endregion

    }

}
