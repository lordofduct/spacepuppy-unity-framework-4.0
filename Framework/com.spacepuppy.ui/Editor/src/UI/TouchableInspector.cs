using UnityEngine;
using UnityEditor;

using com.spacepuppy.UI;

namespace com.spacepuppyeditor.UI
{
    [CustomEditor(typeof(Touchable))]
    public class Touchable_Editor : Editor
    {
        public override void OnInspectorGUI() { }
    }
}
