using UnityEngine;

namespace com.spacepuppy
{

    /// <summary>
    /// Serializable representation of a time period and the time supplier that period should respect.
    /// </summary>
    [System.Serializable()]
    public struct SPTimePeriod
    {

        public static SPTimePeriod Zero { get { return new SPTimePeriod(0f); } }

        public const float SECONDS_IN_MINUTE = 60f;
        public const float SECONDS_IN_HOUR = 3600f;
        public const float SECONDS_IN_DAY = 86400f;
        public const float SECONDS_IN_WEEK = 604800;

        #region Fields

        [SerializeField()]
        private float _seconds;

        [SerializeField()]
        private DeltaTimeType _timeSupplierType;
        [SerializeField()]
        private string _timeSupplierName;

        #endregion

        #region CONSTRUCTOR

        public SPTimePeriod(float seconds)
        {
            _seconds = seconds;
            _timeSupplierType = DeltaTimeType.Normal;
            _timeSupplierName = SPTime.Normal.Id;
        }

        public SPTimePeriod(float seconds, SPTime time)
        {
            _seconds = seconds;
            _timeSupplierType = time.TimeSupplierType;
            _timeSupplierName = time.TimeSupplierName;
        }

        public SPTimePeriod(float seconds, ITimeSupplier supplier)
        {
            _seconds = seconds;
            _timeSupplierType = SPTime.GetDeltaType(supplier);
            _timeSupplierName = SPTime.GetValidatedId(supplier);
        }

        #endregion

        #region Properties

        public float Seconds
        {
            get { return _seconds; }
            set { _seconds = value; }
        }

        public DeltaTimeType TimeSupplierType
        {
            get { return _timeSupplierType; }
        }

        public string TimeSupplierName
        {
            get { return _timeSupplierName; }
        }

        public ITimeSupplier TimeSupplier
        {
            get
            {
                return SPTime.GetTime(_timeSupplierType, _timeSupplierName);
            }
            set
            {
                if (value == null) value = SPTime.Normal;
                _timeSupplierType = SPTime.GetDeltaType(value);
                _timeSupplierName = SPTime.GetValidatedId(value);
            }
        }

        public ITimeSupplier TimeSupplierOrDefault => this.TimeSupplier ?? SPTime.Normal;

        public bool IsCustom
        {
            get { return _timeSupplierType == DeltaTimeType.Custom; }
        }

        #endregion

        #region Methods

        public bool Elapsed(double startTime)
        {
            var time = this.TimeSupplier;
            if (time == null) return false;
            return (time.TotalPrecise - startTime) >= _seconds;
        }

        public float ElapsedTime(double startTime)
        {
            var time = this.TimeSupplier;
            if (time == null) return 0f;
            return (float)(time.TotalPrecise - startTime);
        }

        #endregion

        #region Special Types

        [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
        public class Config : SPTime.Config
        {
            public string DefaultUnits;

            public Config()
            {

            }
            public Config(string defaultUnits)
            {
                DefaultUnits = defaultUnits;
            }
        }

        #endregion

        #region Conversion

        public static implicit operator SPTime(SPTimePeriod period)
        {
            return new SPTime(period._timeSupplierType, period._timeSupplierName);
        }

        public static implicit operator SPTimePeriod(float seconds)
        {
            return new SPTimePeriod(seconds);
        }

        #endregion

    }

    [System.Serializable]
    public struct SPTimeSpan
    {

        public static SPTimeSpan Zero { get { return new SPTimeSpan(0f); } }

        public const float SECONDS_IN_MINUTE = 60f;
        public const float SECONDS_IN_HOUR = 3600f;
        public const float SECONDS_IN_DAY = 86400f;
        public const float SECONDS_IN_WEEK = 604800;

        #region Fields

        [SerializeField()]
        private float _seconds;

        #endregion

        #region CONSTRUCTOR

        public SPTimeSpan(float seconds)
        {
            _seconds = seconds;
        }

        #endregion

        #region Properties

        public float Seconds
        {
            get { return _seconds; }
            set { _seconds = value; }
        }

        #endregion

        #region Special Types

        [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
        public class Config : System.Attribute
        {
            public string DefaultUnits;

            public Config()
            {

            }
            public Config(string defaultUnits)
            {
                DefaultUnits = defaultUnits;
            }
        }

        #endregion

        #region Conversion

        public static implicit operator SPTimeSpan(float seconds)
        {
            return new SPTimeSpan(seconds);
        }

        public static explicit operator SPTimeSpan(System.TimeSpan span)
        {
            return new SPTimeSpan((float)span.TotalSeconds);
        }

        public static implicit operator System.TimeSpan(SPTimeSpan span)
        {
            return System.TimeSpan.FromSeconds(span.Seconds);
        }

        #endregion

    }

}
