using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy
{
    public class PlayerPrefs_SPExt : PlayerPrefs
    {

        public static bool GetBool(string key, bool defaultValue = false)
        {
            if (!PlayerPrefs.HasKey(key)) return defaultValue;
            return PlayerPrefs.GetInt(key) != 0;
        }

        public static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        /// <summary>
        /// Returns a datetime (based on a stored string) for a given key, or returns a default value if it doesn't exist or can't be parsed to a date.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static System.DateTime? GetDateTime(string key, System.DateTime? defaultValue = null)
        {
            var s = PlayerPrefs.GetString(key, null);
            if (string.IsNullOrEmpty(s)) return defaultValue;

            long l;
            if (long.TryParse(s, out l)) return new System.DateTime(l);

            System.DateTime dt;
            if (System.DateTime.TryParse(s, out dt)) return dt;

            return defaultValue;
        }

        public static void SetDateTime(string key, System.DateTime? value)
        {
            PlayerPrefs.SetString(key, value != null ? value.Value.Ticks.ToString() : null);
        }

        public static System.DateTime? GetDateTimeShort(string key, System.DateTime? defaultValue = null)
        {
            if (!PlayerPrefs.HasKey(key)) return defaultValue;
            return SPConstants.UnixEpoch.AddSeconds((uint)PlayerPrefs.GetInt(key));
        }

        public static System.DateTime? GetDateTimeShort(string key, System.DateTime epoch, System.DateTime? defaultValue = null)
        {
            if (!PlayerPrefs.HasKey(key)) return defaultValue;
            return epoch.AddSeconds((uint)PlayerPrefs.GetInt(key));
        }

        public static void SetDateTimeShort(string key, System.DateTime value)
        {
            double seconds = (value - SPConstants.UnixEpoch).TotalSeconds;
            if (seconds < 0 || seconds > uint.MaxValue) throw new System.ArgumentException("Can only store short datetimes that are greater than the UnixEpoch and less than UnixEpoch + 136 years.");

            PlayerPrefs.SetInt(key, (int)((uint)seconds));
        }

        public static void SetDateTimeShort(string key, System.DateTime epoch, System.DateTime value)
        {
            double seconds = (value - epoch).TotalSeconds;
            if (seconds < 0 || seconds > uint.MaxValue) throw new System.ArgumentException("Can only store short datetimes that are greater than the 'epoch' and less than 'epoch' + 136 years.");

            PlayerPrefs.SetInt(key, (int)((uint)seconds));
        }
    }
}
