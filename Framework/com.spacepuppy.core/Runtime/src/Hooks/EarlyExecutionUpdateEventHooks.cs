using UnityEngine;

namespace com.spacepuppy.Hooks
{

    /// <summary>
    /// This class exists to be added to the Execution Order Manager at an extra early time. Some managers use this to get early update timing.
    /// </summary>
    /// <remarks>
    /// The project must flag this as early execution in Edit->Project Settings->Execution Order
    /// </remarks>
    [AddComponentMenu("SpacePuppy/Hooks/EarlyExecutionUpdateEventHooks")]
    [DefaultExecutionOrder(EarlyExecutionUpdateEventHooks.DEFAULT_EXECUTION_ORDER)]
    public sealed class EarlyExecutionUpdateEventHooks : UpdateEventHooks
    {
        public const int DEFAULT_EXECUTION_ORDER = -31900;


    }

}