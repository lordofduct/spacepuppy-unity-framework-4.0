using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace com.spacepuppy.Dynamic
{

    /// <summary>
    /// Represents an object that handles its own creation of a token of its state. 
    /// 
    /// A token should be serializable.
    /// 
    /// This contract will be respected by com.spacepuppy.Dynamics when trying to receive a StateToken of an object. 
    /// If the object implements this, the interface will be used, otherwise a <see cref="com.spacepuppy.Dynamic.StateToken"/> will be used.
    /// </summary>
    public interface ITokenizable
    {

        object CreateStateToken();
        void RestoreFromStateToken(object token);

    }

    public interface IToken
    {

        /// <summary>
        /// Copy the tokens state onto some target 'obj'.
        /// </summary>
        /// <param name="obj"></param>
        void CopyTo(object obj);

        /// <summary>
        /// Copies the the member's of obj with the same members as the IToken.
        /// </summary>
        /// <param name="obj"></param>
        void SyncFrom(object obj);

    }

    /// <summary>
    /// Implies that the object is a dynamic token for storing states. 
    /// The members of the token should only be fields/properties, and not methods.
    /// </summary>
    public interface IStateToken : IDynamic, IToken, ITokenizable
    {

    }

    public static class TokenUtil
    {
        /// <summary>
        /// Returns a state token with a shallow copy of all public properties/fields of 'obj'.
        /// The state token is possibly serializable but not guaranteed. 
        /// If 'obj' implements ITokenizable, then the object itself controls if the token is serializable.
        /// Otherwise a StateToken is returned which is serializable, but not all of the values of the copied members are serializable. That's up to them
        /// 
        /// Note - by serializable, this refers to .net serialization or any engine that supports the ISerialable interface. Not the unity serialization engine.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object CreateStateToken(object obj)
        {
            if (obj == null)
                return null;
            else if (obj is ITokenizable)
            {
                try
                {
                    return (obj as ITokenizable).CreateStateToken();
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                var token = StateToken.GetToken();
                token.CopyFrom(obj);
                return token;
            }
        }

        /// <summary>
        /// Restores the state of 'obj' based on 'token'. Token should be a state object that was returned by 'CreateStateToken'. 
        /// Respects ITokenizable interface on 'obj'.
        /// If the type of 'token' is mismatched, then this may likely fail.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        public static void RestoreFromStateToken(object obj, object token)
        {
            if (obj is ITokenizable)
            {
                try
                {
                    (obj as ITokenizable).RestoreFromStateToken(token);
                }
                catch
                {
                }
            }
            else
            {
                CopyState(obj, token);
            }
        }

        /// <summary>
        /// Like RestoreFromStateToken, but ignores ITokenizable interface.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        public static void CopyState(object obj, object source)
        {
            if (obj is IToken)
                (obj as IToken).SyncFrom(source);
            else if (source is IToken)
                (source as IToken).CopyTo(obj);
            else if (source != null)
            {
                foreach (var m in DynamicUtil.GetMembers(source, false, MemberTypes.Property | MemberTypes.Field))
                {
                    DynamicUtil.SetValue(obj, m.Name, DynamicUtil.GetValue(source, m));
                }
            }
        }

        /// <summary>
        /// Sync's obj and source's state for members that overlap.
        /// If obj is an IToken it respect's IToken.SyncFrom.
        /// If source is an IToken it respect's IToken.CopyTo.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        public static void SyncState(object obj, object source)
        {
            if (obj is IToken)
                (obj as IToken).SyncFrom(source);
            else if (source is IToken)
                (source as IToken).CopyTo(obj);
            else if (source != null)
            {
                foreach (var m in DynamicUtil.GetMembers(source, false, MemberTypes.Property | MemberTypes.Field))
                {
                    DynamicUtil.SetValue(obj, m.Name, DynamicUtil.GetValue(source, m));
                }
            }
        }
    }

}
