namespace com.spacepuppy.Dynamic.Accessors
{

    /// <summary>
    /// Acts as a IMemberAccessor that accesses IDynamic objects.
    /// 
    /// NOTE - when updating DynamicUtil later to use IMemberAccessors for speed... we need to reverse this implementation.
    /// </summary>
    public class DynamicMemberAccessor : IMemberAccessor
    {

        #region Fields

        private string _memberName;
        private System.Type _memberType;

        #endregion

        #region CONSTRUCTOR

        public DynamicMemberAccessor(string memberName, System.Type memberType)
        {
            _memberName = memberName;
            _memberType = memberType ?? typeof(object);
        }

        #endregion


        public string GetMemberName() { return _memberName; }

        public System.Type GetMemberType() { return _memberType; }

        public object Get(object target)
        {
            return DynamicUtil.GetValue(target, _memberName);
        }

        public void Set(object target, object value)
        {
            DynamicUtil.SetValue(target, _memberName, value);
        }
    }
}
