using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public interface IAudioManager : IService
    {

        /// <summary>
        /// The configured volume of the game. This is the volume that if all audio sources were at max, this is what it would sound at.
        /// </summary>
        float MasterVolume { get; set; }
        /// <summary>
        /// Allows you to fade the MasterVolume with out actually modifying it.
        /// </summary>
        float FadeVolume { get; set; }

        /// <summary>
        /// sets the volume of a group linearly with a value from 0->1
        /// </summary>
        /// <param name="groupname"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetVolume(string label, float value);
        /// <summary>
        /// get the volume of a group linearly as a value from 0->1
        /// </summary>
        /// <param name="groupname"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool GetVolume(string label, out float value);

    }

}