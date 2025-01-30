using System;
using System.Linq;
using System.Reflection;

namespace com.spacepuppy.Dynamic
{

    /// <summary>
    /// Facilitates creating a wrapper to access an object of an otherwise unknown type. This is useful for reflecting out an internal 
    /// class in an assembly you don't have direct access to.
    /// </summary>
    public class TypeAccessWrapper
    {

        private const BindingFlags PUBLIC_MEMBERS = BindingFlags.Instance | BindingFlags.Public;
        private const BindingFlags PUBLIC_STATIC_MEMBERS = BindingFlags.Static | BindingFlags.Public;

        #region Fields

        private object _wrappedObject;
        private Type _wrappedType;

        private bool _includeNonPublic = false;

        #endregion

        #region CONSTRUCTOR

        public TypeAccessWrapper(Type type, bool includeNonPublic = false)
        {
            if (type == null) throw new ArgumentNullException("type");

            _wrappedType = type;
            _includeNonPublic = includeNonPublic;
            this.WrappedObject = null;
        }

        public TypeAccessWrapper(Type type, object obj, bool includeNonPublic = false)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (obj != null && !type.IsInstanceOfType(obj)) throw new ArgumentException("Wrapped Object must be of type assignable to target type.");

            _wrappedType = type;
            _includeNonPublic = includeNonPublic;
            this.WrappedObject = obj;
        }

        #endregion

        #region Properties

        public Type WrappedType { get { return _wrappedType; } }

        public object WrappedObject
        {
            get { return _wrappedObject; }
            set
            {
                if (value != null && !_wrappedType.IsInstanceOfType(value)) throw new ArgumentException("Wrapped Object must be of type assignable to target type.");
                _wrappedObject = value;
            }
        }

        public bool IncludeNonPublic { get { return _includeNonPublic; } set { _includeNonPublic = value; } }

        #endregion

        #region Instance Acccess

        public MemberInfo[] GetMembers()
        {
            var binding = PUBLIC_MEMBERS;
            if (_includeNonPublic) binding |= BindingFlags.NonPublic;
            return _wrappedType.GetMembers(binding);
        }

        public MethodInfo[] GetMethods()
        {
            var binding = PUBLIC_MEMBERS;
            if (_includeNonPublic) binding |= BindingFlags.NonPublic;
            return _wrappedType.GetMethods(binding);
        }

        public string[] GetPropertyNames()
        {
            var binding = PUBLIC_MEMBERS;
            if (_includeNonPublic) binding |= BindingFlags.NonPublic;
            return (from p in _wrappedType.GetProperties(binding) select p.Name).Union(from f in _wrappedType.GetFields(binding) select f.Name).ToArray();
        }

        public Delegate GetMethod(string name, System.Type delegShape)
        {
            if (_wrappedObject == null) throw new InvalidOperationException("Can only access static members.");
            if (!delegShape.IsSubclassOf(typeof(Delegate))) throw new ArgumentException("Type must inherit from Delegate.", "delegShape");

            var binding = PUBLIC_MEMBERS;
            if (_includeNonPublic) binding |= BindingFlags.NonPublic;

            var invokeMeth = delegShape.GetMethod("Invoke");
            var paramTypes = (from p in invokeMeth.GetParameters() select p.ParameterType).ToArray();
            MethodInfo meth = null;
            try
            {
                meth = _wrappedType.GetMethod(name, binding, null, paramTypes, null);
            }
            catch
            {
                try
                {
                    meth = _wrappedType.GetMethod(name, binding);
                }
                catch
                {

                }
            }


            if (meth != null)
            {
                try
                {
                    return Delegate.CreateDelegate(delegShape, _wrappedObject, meth);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("A method matching the name and shape requested could not be found.", ex);
                }
            }
            else
            {
                throw new InvalidOperationException("A method matching the name and shape requested could not be found.");
            }

        }

        public T GetMethod<T>(string name) where T : System.Delegate
        {
            return GetMethod(name, typeof(T)) as T;
        }

        public Delegate GetUnboundMethod(string name, System.Type delegShape)
        {
            if (!delegShape.IsSubclassOf(typeof(Delegate))) throw new ArgumentException("Type must inherit from Delegate.", nameof(delegShape));

            var binding = PUBLIC_MEMBERS;
            if (_includeNonPublic) binding |= BindingFlags.NonPublic;

            var invokeMeth = delegShape.GetMethod("Invoke");
            var paramTypes = (from p in invokeMeth.GetParameters().Skip(1) select p.ParameterType).ToArray();
            MethodInfo meth = null;
            try
            {
                meth = _wrappedType.GetMethod(name, binding, null, paramTypes, null);
            }
            catch
            {
                try
                {
                    meth = _wrappedType.GetMethod(name, binding);
                }
                catch
                {

                }
            }

            if (meth != null)
            {
                try
                {
                    return meth.CreateDelegate(delegShape);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("A method matching the name and shape requested could not be found.", ex);
                }
            }
            else
            {
                throw new InvalidOperationException("A method matching the name and shape requested could not be found.");
            }
        }

        public object CallMethod(string name, System.Type delegShape, params object[] args)
        {
            var d = GetMethod(name, delegShape);
            return d.DynamicInvoke(args);
        }

        public object GetProperty(string name)
        {
            if (_wrappedObject == null) throw new InvalidOperationException("Can only access static members.");
            return GetProperty(name, _wrappedObject);
        }

        public object GetProperty(string name, object wrappedobject)
        {
            if (wrappedobject == null) throw new ArgumentNullException(nameof(wrappedobject));

            var binding = PUBLIC_MEMBERS;
            if (_includeNonPublic) binding |= BindingFlags.NonPublic;

            var prop = _wrappedType.GetProperty(name, binding, null, null, Type.EmptyTypes, null);
            if (prop != null)
            {
                return prop.GetValue(wrappedobject, null);
            }

            var field = _wrappedType.GetField(name, binding);
            if (field != null)
            {
                return field.GetValue(wrappedobject);
            }

            return null;
        }

        public void SetProperty(string name, object value)
        {
            if (_wrappedObject == null) throw new InvalidOperationException("Can only access static members.");
            SetProperty(name, _wrappedObject, value);
        }

        public void SetProperty(string name, object wrappedobject, object value)
        {
            if (wrappedobject == null) throw new ArgumentNullException(nameof(wrappedobject));

            var binding = PUBLIC_MEMBERS;
            if (_includeNonPublic) binding |= BindingFlags.NonPublic;

            var prop = _wrappedType.GetProperty(name, binding, null, null, Type.EmptyTypes, null);
            if (prop != null)
            {
                try
                {
                    prop.SetValue(wrappedobject, value, null);
                    return;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Mismatch when attempting to set property.", ex);
                }
            }

            var field = _wrappedType.GetField(name, binding);
            if (field != null)
            {
                try
                {
                    field.SetValue(wrappedobject, value);
                    return;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Mismatch when attempting to set property.", ex);
                }
            }
        }

        #endregion

        #region Static Access

        public MemberInfo[] GetStaticMembers()
        {
            var binding = PUBLIC_STATIC_MEMBERS;
            if (_includeNonPublic) binding |= BindingFlags.NonPublic;

            return _wrappedType.GetMembers(binding);
        }

        public MethodInfo[] GetStaticMethods()
        {
            var binding = PUBLIC_STATIC_MEMBERS;
            if (_includeNonPublic) binding |= BindingFlags.NonPublic;

            return _wrappedType.GetMethods(binding);
        }

        public string[] GetStaticPropertyNames()
        {
            var binding = PUBLIC_STATIC_MEMBERS;
            if (_includeNonPublic) binding |= BindingFlags.NonPublic;

            return (from p in _wrappedType.GetProperties(binding) select p.Name).Union(from f in _wrappedType.GetFields(binding) select f.Name).ToArray();
        }

        public bool TryGetStaticMethod(string name, System.Type delegShape, out Delegate del)
        {
            if (!delegShape.IsSubclassOf(typeof(Delegate))) throw new ArgumentException("Type must inherit from Delegate.", "delegShape");

            var binding = PUBLIC_STATIC_MEMBERS;
            if (_includeNonPublic) binding |= BindingFlags.NonPublic;

            var invokeMeth = delegShape.GetMethod("Invoke");
            var paramTypes = (from p in invokeMeth.GetParameters() select p.ParameterType).ToArray();
            var meth = _wrappedType.GetMethod(name, binding, null, paramTypes, null);

            if (meth != null)
            {
                try
                {
                    del = Delegate.CreateDelegate(delegShape, meth);
                    return true;
                }
                catch //(Exception ex)
                {
                    del = null;
                    return false;
                }
            }
            else
            {
                del = null;
                return false;
            }
        }
        public Delegate GetStaticMethod(string name, System.Type delegShape)
        {
            if (!delegShape.IsSubclassOf(typeof(Delegate))) throw new ArgumentException("Type must inherit from Delegate.", "delegShape");

            var binding = PUBLIC_STATIC_MEMBERS;
            if (_includeNonPublic) binding |= BindingFlags.NonPublic;

            var invokeMeth = delegShape.GetMethod("Invoke");
            var paramTypes = (from p in invokeMeth.GetParameters() select p.ParameterType).ToArray();
            var meth = _wrappedType.GetMethod(name, binding, null, paramTypes, null);

            if (meth != null)
            {
                try
                {
                    return Delegate.CreateDelegate(delegShape, meth);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("A method matching the name and shape requested could not be found.", ex);
                }
            }
            else
            {
                throw new InvalidOperationException("A method matching the name and shape requested could not be found.");
            }
        }

        public bool TryGetStaticMethod<T>(string name, out T del) where T : System.Delegate
        {
            if (TryGetStaticMethod(name, typeof(T), out System.Delegate d) && d is T)
            {
                del = d as T;
                return true;
            }
            else
            {
                del = null;
                return false;
            }
        }
        public T GetStaticMethod<T>(string name) where T : System.Delegate
        {
            return GetStaticMethod(name, typeof(T)) as T;
        }

        public object CallStaticMethod(string name, System.Type delegShape, params object[] args)
        {
            var d = GetStaticMethod(name, delegShape);
            return d.DynamicInvoke(args);
        }

        public object GetStaticProperty(string name)
        {
            var binding = PUBLIC_STATIC_MEMBERS;
            if (_includeNonPublic) binding |= BindingFlags.NonPublic;

            var prop = _wrappedType.GetProperty(name, binding, null, null, Type.EmptyTypes, null);
            if (prop != null)
            {
                return prop.GetValue(null, null);
            }

            var field = _wrappedType.GetField(name, binding);
            if (field != null)
            {
                return field.GetValue(null);
            }

            return null;
        }

        public void SetStaticProperty(string name, object value)
        {
            var binding = PUBLIC_STATIC_MEMBERS;
            if (_includeNonPublic) binding |= BindingFlags.NonPublic;

            var prop = _wrappedType.GetProperty(name, binding, null, null, Type.EmptyTypes, null);
            if (prop != null)
            {
                try
                {
                    prop.SetValue(null, value, null);
                    return;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Mismatch when attempting to set property.", ex);
                }
            }

            var field = _wrappedType.GetField(name, binding);
            if (field != null)
            {
                try
                {
                    field.SetValue(null, value);
                    return;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Mismatch when attempting to set property.", ex);
                }
            }
        }

        #endregion

        #region Unbound Method Accessor - an unbound method requires the instance object as the first parameter

#if ENABLE_MONO //THIS IS ONLY SUPPORTED IN MONO
        public Delegate GetUnboundAction(string name)
        {
            var dtp = typeof(Action<>).MakeGenericType(_wrappedType);
            return this.GetUnboundMethod(name, dtp);
        }

        public Delegate GetUnboundAction<T>(string name)
        {
            var dtp = typeof(Action<,>).MakeGenericType(_wrappedType, typeof(T));
            return this.GetUnboundMethod(name, dtp);
        }

        public Delegate GetUnboundAction<T1, T2>(string name)
        {
            var dtp = typeof(Action<,,>).MakeGenericType(_wrappedType, typeof(T1), typeof(T2));
            return this.GetUnboundMethod(name, dtp);
        }

        public Delegate GetUnboundAction<T1, T2, T3>(string name)
        {
            var dtp = typeof(Action<,,,>).MakeGenericType(_wrappedType, typeof(T1), typeof(T2), typeof(T3));
            return this.GetUnboundMethod(name, dtp);
        }

        public Delegate GetUnboundAction<T1, T2, T3, T4>(string name)
        {
            var dtp = typeof(Action<,,,,>).MakeGenericType(_wrappedType, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
            return this.GetUnboundMethod(name, dtp);
        }

        public Delegate GetUnboundFunc<TReturn>(string name)
        {
            var dtp = typeof(Func<,>).MakeGenericType(_wrappedType, typeof(TReturn));
            return this.GetUnboundMethod(name, dtp);
        }

        public Delegate GetUnboundFunc<T, TReturn>(string name)
        {
            var dtp = typeof(Func<,,>).MakeGenericType(_wrappedType, typeof(T), typeof(TReturn));
            return this.GetUnboundMethod(name, dtp);
        }

        public Delegate GetUnboundFunc<T1, T2, TReturn>(string name)
        {
            var dtp = typeof(Func<,,,>).MakeGenericType(_wrappedType, typeof(T1), typeof(T2), typeof(TReturn));
            return this.GetUnboundMethod(name, dtp);
        }

        public Delegate GetUnboundFunc<T1, T2, T3, TReturn>(string name)
        {
            var dtp = typeof(Func<,,,,>).MakeGenericType(_wrappedType, typeof(T1), typeof(T2), typeof(T3), typeof(TReturn));
            return this.GetUnboundMethod(name, dtp);
        }

        public Delegate GetUnboundFunc<T1, T2, T3, T4, TReturn>(string name)
        {
            var dtp = typeof(Func<,,,,,>).MakeGenericType(_wrappedType, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(TReturn));
            return this.GetUnboundMethod(name, dtp);
        }
#endif

        #endregion

    }

}
