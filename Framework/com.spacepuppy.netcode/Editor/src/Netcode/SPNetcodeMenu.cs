using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;

namespace com.spacepuppyeditor.Netcode
{

    public class SPNetcodeMenu
    {

        /// <remarks>
        /// This HACK is taken from this conversation on the unity forums:
        /// https://discussions.unity.com/t/scene-objects-assigned-same-globalobjectidhash-value/882913/12
        /// </remarks>
        [MenuItem("Spacepuppy/Netcode For GameObjects/Repair GlobalObjectIdHashes In Scene")]
        public static void FixGlobalObjectIdHashesInScene()
        {
            var networkObjects = Object.FindObjectsOfType<NetworkObject>(true);
            foreach (var networkObject in networkObjects)
            {
                if (!networkObject.gameObject.scene.isLoaded) continue;

                var serializedObject = new SerializedObject(networkObject);
                var hashField = serializedObject.FindProperty("GlobalObjectIdHash");

                //HACK: Reset the hash and apply it.
                //This implicitly marks the field as dirty, allowing it to be saved as an override.
#if UNITY_2022_2_OR_NEWER
                hashField.uintValue = 0;
#else
                hashField.intValue = 0;
#endif
                serializedObject.ApplyModifiedProperties();
                //Afterwards, OnValidate will kick in and return the hash to it's real value, which will be saved now.
            }
        }

    }

}
