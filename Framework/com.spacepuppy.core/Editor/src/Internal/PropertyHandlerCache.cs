using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic;

namespace com.spacepuppyeditor.Internal
{
    internal class PropertyHandlerCache
    {

        private static TypeAccessWrapper _accessWrapper;
        private static System.Func<SerializedProperty, int> _imp_getPropertyHash;

        static PropertyHandlerCache()
        {
            var klass = InternalTypeUtil.UnityEditorAssembly.GetType("UnityEditor.PropertyHandlerCache");
            _accessWrapper = new TypeAccessWrapper(klass, true);
        }

        public static int GetPropertyHash(SerializedProperty property)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));
            if (property.serializedObject.targetObject == null)
                return 0;

            //var spath = property.propertyPath;
            //int index = spath.IndexOf(".Array.data[");
            //int len = 0;
            //while (index >= 0)
            //{
            //    len = spath.IndexOf(']', index) - index;
            //    spath = spath.Remove(index, len);
            //    index = spath.IndexOf(".Array.data[");
            //}

            //int num = property.serializedObject.targetObject.GetInstanceID() ^ spath.GetHashCode();
            //if (property.propertyType == SerializedPropertyType.ObjectReference)
            //    num ^= property.objectReferenceInstanceIDValue;

            //return num;

            if(_imp_getPropertyHash == null)
            {
                _imp_getPropertyHash = _accessWrapper.GetStaticMethod<System.Func<SerializedProperty, int>>("GetPropertyHash");
            }
            return _imp_getPropertyHash?.Invoke(property) ?? 0;
        }

        /// <summary>
        /// Unlike GetPropertyHash, this will respect the index in an array. Useful if you need uniqueness over an array of elements.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static int GetIndexRespectingPropertyHash(SerializedProperty property)
        {
            if (property == null) throw new System.ArgumentNullException("property");
            if (property.serializedObject.targetObject == null)
                return 0;

            var spath = property.propertyPath;

            int num = property.serializedObject.targetObject.GetInstanceID() ^ spath.GetHashCode();
            if (property.propertyType == SerializedPropertyType.ObjectReference)
                num ^= property.objectReferenceInstanceIDValue;

            return num;
        }


        #region Instance Interface

        private Dictionary<int, IPropertyHandler> _table = new Dictionary<int, IPropertyHandler>();
        private Dictionary<int, object> _states = new Dictionary<int, object>();

        public IPropertyHandler GetHandler(SerializedProperty property)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));

            var hash = GetPropertyHash(property);
            IPropertyHandler result;
            if (_table.TryGetValue(hash, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public void SetHandler(SerializedProperty property, IPropertyHandler handler)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));

            var hash = GetPropertyHash(property);
            if (handler == null)
            {
                if (_table.ContainsKey(hash)) _table.Remove(hash);
            }
            else
            {
                _table[hash] = handler;
            }
        }

        public void Clear()
        {
            _table.Clear();
        }

        #endregion

    }
}
