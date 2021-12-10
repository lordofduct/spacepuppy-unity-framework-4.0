using UnityEditor;

using com.spacepuppy.Mecanim;

namespace com.spacepuppyeditor.Mecanim
{

    [CustomEditor(typeof(BridgedStateMachineBehaviour), true)]
    [CanEditMultipleObjects()]
    public class BridgeStateMachineBehaviourEditor : SPStateMachineBehaviourEditor
    {
    }

    [CustomEditor(typeof(BridgedStateMachineBehaviour<>), true)]
    [CanEditMultipleObjects()]
    class BridgeStateMachineBehaviourTEditor : SPStateMachineBehaviourEditor
    {
    }

}
