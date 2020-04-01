using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace com.spacepuppyeditor
{

    public class SpacepuppySettings
    {
        //TODO - implement with the actual prefs savings. For now it's just static/consts

        /*
         * EDITOR SETTINGS
         */

        public static bool UseSPEditorAsDefaultEditor
        {
            get { return false; }
        }

        public static bool UseAdvancedAnimationInspector
        {
            get { return false; }
        }

        public static bool UseHierarchDrawer
        {
            get { return true; }
        }

        public static bool UseHierarchyAlternateContextMenu
        {
            get { return true; }
        }

        public static bool SignalValidateReceiver
        {
            get { return false; }
        }
    }

}