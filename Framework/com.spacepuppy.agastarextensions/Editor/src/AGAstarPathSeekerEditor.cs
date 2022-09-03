using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy.Pathfinding;
using Pathfinding;

namespace com.spacepuppyeditor.Pathfinding
{

    [CustomEditor(typeof(AGAstarPathSeeker))]
    [CanEditMultipleObjects]
    public class AGAstarPathSeekerEditor : SeekerEditor
    {
        
    }
    
}