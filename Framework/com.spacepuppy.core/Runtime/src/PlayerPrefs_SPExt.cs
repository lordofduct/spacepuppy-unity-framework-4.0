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


        public static Resolution? GetResolution(string key, Resolution? defaultValue = null)
        {
            var s = PlayerPrefs.GetString(key, null);
            if (string.IsNullOrEmpty(s) || !s.Contains('|')) return defaultValue;

#if UNITY_2022_2_OR_NEWER
            var arr = s.Split('|');
            if (arr.Length < 3) return defaultValue;

            int w, h, rn, rd;
            if (!int.TryParse(arr[0], out w)) return defaultValue;
            if (!int.TryParse(arr[1], out h)) return defaultValue;
            if (!int.TryParse(arr[2], out rn)) return defaultValue;
            if (arr.Length > 3) //is 2022_2 or later
            {
                int.TryParse(arr[3], out rd);
            }
            else //is in 2021 or older format
            {
                rd = 1;
            }

            return new Resolution()
            {
                width = w,
                height = h,
                refreshRateRatio = new RefreshRate() { numerator = (uint)rn, denominator = (uint)rd }
            };
#else
            var arr = s.Split('|');
            if (arr.Length != 3) return defaultValue;

            int w, h, r;
            if (!int.TryParse(arr[0], out w)) return defaultValue;
            if (!int.TryParse(arr[1], out h)) return defaultValue;
            if (!int.TryParse(arr[2], out r)) return defaultValue;

            return new Resolution()
            {
                width = w,
                height = h,
                refreshRate = r
            };
#endif
        }

        public static void SetResolution(string key, Resolution? resolution)
        {
            if (resolution == null)
            {
                PlayerPrefs.SetString(key, null);
            }
            else
            {
#if UNITY_2022_2_OR_NEWER
                var res = resolution.Value;
                PlayerPrefs.SetString(key, $"{res.width}|{res.height}|{res.refreshRateRatio.numerator}|{res.refreshRateRatio.denominator}");
#else
                var res = resolution.Value;
                PlayerPrefs.SetString(key, $"{res.width}|{res.height}|{res.refreshRate}");
#endif
            }
        }

    }
}
