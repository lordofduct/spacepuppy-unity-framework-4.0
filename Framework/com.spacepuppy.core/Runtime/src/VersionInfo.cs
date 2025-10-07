using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace com.spacepuppy
{

    [System.Serializable]
    public struct VersionInfo : IComparable<VersionInfo>
    {

        #region Fields

        [SerializeField]
        public int Major;
        [SerializeField]
        public int Minor;
        [SerializeField]
        public int Patch;
        [SerializeField]
        public int Build;

        #endregion

        #region CONSTRUCTOR

        public VersionInfo(int major, int minor = 0, int patch = 0, int build = 0)
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
            this.Build = build;
        }

        #endregion

        #region Methods

        public int CompareTo(VersionInfo other)
        {
            if (Major < other.Major) return -1;
            if (Major > other.Major) return +1;
            if (Minor < other.Minor) return -1;
            if (Minor > other.Minor) return +1;
            if (Patch < other.Patch) return -1;
            if (Patch > other.Patch) return +1;
            if (Build < other.Build) return -1;
            if (Build > other.Build) return +1;
            return 0;
        }

        public override string ToString()
        {
            if (this.Build == 0)
                return string.Format("{0}.{1}.{2}", Major, Minor, Patch);
            else
                return string.Format("{0}.{1}.{2}.{3}", Major, Minor, Patch, Build);
        }

        public bool Equals(VersionInfo other)
        {
            return this.CompareTo(other) != 0;
        }

        public override bool Equals(object other)
        {
            if (other is VersionInfo)
                return this.CompareTo((VersionInfo)other) != 0;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Major.GetHashCode() ^ Minor.GetHashCode() ^ Patch.GetHashCode() ^ Build.GetHashCode();
        }

        #endregion

        #region Operators

        public static bool operator ==(VersionInfo a, VersionInfo b)
        {
            return a.CompareTo(b) == 0;
        }


        public static bool operator !=(VersionInfo a, VersionInfo b)
        {
            return a.CompareTo(b) != 0;
        }


        public static bool operator <=(VersionInfo a, VersionInfo b)
        {
            return a.CompareTo(b) <= 0;
        }


        public static bool operator >=(VersionInfo a, VersionInfo b)
        {
            return a.CompareTo(b) >= 0;
        }


        public static bool operator <(VersionInfo a, VersionInfo b)
        {
            return a.CompareTo(b) < 0;
        }


        public static bool operator >(VersionInfo a, VersionInfo b)
        {
            return a.CompareTo(b) > 0;
        }

        #endregion

        #region Static Accessors

        private static VersionInfo _unityVersion;
        private static VersionInfo _applicationVersion;
        public static VersionInfo GetUnityVersion()
        {
            if (_unityVersion.GetHashCode() == 0)
            {
                TryParse(Application.unityVersion, out _unityVersion);
            }
            return _unityVersion;
        }

        public static VersionInfo GetApplicationVersion()
        {
            if (_applicationVersion.GetHashCode() == 0)
            {
                TryParse(Application.version, out _applicationVersion);
            }
            return _applicationVersion;
        }

        public static VersionInfo Parse(string version)
        {
            if (TryParse(version, out VersionInfo result))
            {
                return result;
            }
            else
            {
                throw new System.FormatException("Version should be in format 0.#.#.#");
            }
        }
        public static bool TryParse(string version, out VersionInfo info)
        {
            var m = Regex.Match(version, @"^(?:v|ver)?(\d+)(?:\.(\d+))?(?:\.(\d+))?(?:\.(\d+))?$");
            if (m.Success && m.Groups[1].Success)
            {
                info = new VersionInfo()
                {
                    Major = Convert.ToInt32(m.Groups[1].Value),
                    Minor = m.Groups[2].Success ? Convert.ToInt32(m.Groups[2].Value) : 0,
                    Patch = m.Groups[3].Success ? Convert.ToInt32(m.Groups[3].Value) : 0,
                    Build = m.Groups[4].Success ? Convert.ToInt32(m.Groups[4].Value) : 0,
                };
                return true;
            }
            else
            {
                info = default;
                return false;
            }
        }

        #endregion

    }
}
