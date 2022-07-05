using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
using Math = System.Math;

namespace com.spacepuppy
{
    public static class AudioUtils
    {

        public static float GetVolume(this IAudioManager manager, string groupname)
        {
            float result = 0f;
            manager?.GetVolume(groupname, out result);
            return result;
        }

        public static float GetVolume01(this AudioMixer mixer, string name)
        {
            float result;
            if (GetVolume01(mixer, name, out result))
            {
                return result;
            }
            else
            {
                return 0f;
            }
        }

        public static bool GetVolume01(this AudioMixer mixer, string name, out float result)
        {
            if (mixer.GetFloat(name, out result))
            {
                result = Mathf.Clamp01(Mathf.Pow(10f, result / 20f));
                if (result <= 0.0001f) result = 0f;
                return true;
            }
            else
            {
                result = 0f;
                return false;
            }
        }

        public static bool SetVolume01(this AudioMixer mixer, string name, float value)
        {
            if (value < 0.0001f)
            {
                value = -80f;
            }
            else
            {
                value = Mathf.Log10(Mathf.Clamp01(value)) * 20f;
            }

            return mixer.SetFloat(name, value);
        }

    }
}
