using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;

namespace com.spacepuppy.Quality
{

    public class QualitySettingsPropertyNameAttribute : SPPropertyAttribute
    {

        public bool allowCustom;
        public QualitySettingsPropertyNameAttribute() { }
        public QualitySettingsPropertyNameAttribute(bool allowCustom)
        {
            this.allowCustom = allowCustom;
        }


    }

}
