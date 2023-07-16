using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Mecanim
{

    [System.Serializable]
    public class AnimatorStateTargetObject : com.spacepuppy.Events.TriggerableTargetObject
    {

        #region CONSTRUCTOR

        public AnimatorStateTargetObject()
        {
            this.Configure(FindCommand.FindInChildren, SearchBy.Name, string.Empty);
        }

        public AnimatorStateTargetObject(UnityEngine.Object target)
        {
            this.Configure(target);
        }

        public AnimatorStateTargetObject(UnityEngine.Object target, string sprop)
        {
            this.Configure(target, sprop);
        }

        public AnimatorStateTargetObject(UnityEngine.Object target, FindCommand find, SearchBy resolveBy = SearchBy.Nothing, string resolveQuery = null)
        {
            this.Configure(target, find, resolveBy, resolveQuery);
        }

        public AnimatorStateTargetObject(FindCommand find, SearchBy resolveBy, string resolveQuery)
        {
            this.Configure(find, resolveBy, resolveQuery);
        }

        #endregion

        #region Special Types

        public new class ConfigAttribute : System.Attribute
        {

            public System.Type TargetType;
            public bool AlwaysExpanded = true;

            public ConfigAttribute(System.Type targetType)
            {
                if (targetType == null ||
                    (!TypeUtil.IsType(targetType, typeof(UnityEngine.Object)) && !targetType.IsInterface))
                                    throw new TypeArgumentMismatchException(targetType, typeof(UnityEngine.Object), nameof(targetType));

                this.TargetType = targetType;
            }

        }

        #endregion

    }

}
