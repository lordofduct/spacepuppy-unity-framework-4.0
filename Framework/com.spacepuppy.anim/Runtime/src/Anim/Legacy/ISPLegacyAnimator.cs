using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace com.spacepuppy.Anim.Legacy
{

    public interface ISPLegacyAnimator : ISPAnimator
    {

        SPLegacyAnimController Controller { get; }
        bool IsInitialized { get; }

        void Configure(SPLegacyAnimController controller);

    }
}
