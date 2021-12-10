using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween
{

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class CustomTweenMemberAccessorAttribute : System.Attribute
    {

        public static IEnumerable<CustomTweenMemberAccessorAttribute> FindCustomTweenMemberAccessorTypes()
        {
            foreach (var tp in TypeUtil.GetTypesAssignableFrom(typeof(ITweenMemberAccessor)))
            {
                foreach(var attrib in tp.GetCustomAttributes<CustomTweenMemberAccessorAttribute>())
                {
                    attrib.DeclaringType = tp;
                    yield return attrib;
                }
            }
        }

        #region Fields

        private System.Type _targetType;
        private System.Type _memberType;
        private string _propName;
        public int priority;

        #endregion

        #region Constructor

        public CustomTweenMemberAccessorAttribute(System.Type targetType, System.Type memberType, string propName)
        {
            _targetType = targetType;
            _memberType = memberType;
            _propName = propName;
        }

        #endregion

        #region Properties

        public System.Type HandledTargetType { get { return _targetType; } }

        /// <summary>
        /// The type of the member being handled. Same as what would be returned by ITweenMemberAccessor.GetMemberType.
        /// </summary>
        public System.Type MemberType { get { return _memberType; } }

        public string HandledPropName { get { return _propName; } }

        /// <summary>
        /// Set when calling FindCustomTweenMemberAccessorTypes
        /// </summary>
        internal System.Type DeclaringType { get; private set; }

        #endregion

    }
}
