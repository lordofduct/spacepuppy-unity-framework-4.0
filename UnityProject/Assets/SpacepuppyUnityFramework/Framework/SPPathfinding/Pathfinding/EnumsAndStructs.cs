using System;

namespace com.spacepuppy.Pathfinding
{
    public enum PathCalculateStatus
    {
        Invalid = -1,
        NotStarted = 0,
        Calculating = 1,
        Partial = 2,
        Success = 3,
    }

}
