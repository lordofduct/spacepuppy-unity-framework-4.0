using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Waypoints
{

    public struct Waypoint : IControlPoint
    {
        public Vector3 Position;
        public Vector3 Heading;

        public Waypoint(Vector3 p, Vector3 h)
        {
            Position = p;
            Heading = h.normalized;
        }

        public Waypoint(IControlPoint waypoint)
        {
            this.Position = waypoint.Position;
            this.Heading = waypoint.Heading.normalized;
        }

        #region IControlPoint Interface

        Vector3 IControlPoint.Position
        {
            get
            {
                return this.Position;
            }
            set
            {
                this.Position = value;
            }
        }

        Vector3 IControlPoint.Heading
        {
            get
            {
                return this.Heading;
            }
            set
            {
                this.Heading = value.normalized;
            }
        }

        public bool IsInvalid { get { return this.Heading == Vector3.zero; } }

        #endregion

        #region Operator Interface

        public static bool Compare(IControlPoint a, IControlPoint b)
        {
            return VectorUtil.FuzzyEquals(a.Position, b.Position) && VectorUtil.FuzzyEquals(a.Heading, b.Heading);
        }

        public static bool Compare(Waypoint a, IControlPoint b)
        {
            return !a.IsInvalid && VectorUtil.FuzzyEquals(a.Position, b.Position) && VectorUtil.FuzzyEquals(a.Heading, b.Heading);
        }

        public static bool Compare(IControlPoint a, Waypoint b)
        {
            return !b.IsInvalid && VectorUtil.FuzzyEquals(a.Position, b.Position) && VectorUtil.FuzzyEquals(a.Heading, b.Heading);
        }

        public static bool Compare(Waypoint a, Waypoint b)
        {
            return (a.IsInvalid && b.IsInvalid) || (VectorUtil.FuzzyEquals(a.Position, b.Position) && VectorUtil.FuzzyEquals(a.Heading, b.Heading));
        }

        #endregion

        public static readonly Waypoint Invalid = new Waypoint(Vector3.zero, Vector3.zero);

    }

    public struct WeightedWaypoint : IWeightedControlPoint
    {
        public Vector3 Position;
        public Vector3 Heading;
        public float Strength;

        public WeightedWaypoint(Vector3 p, Vector3 h, float s)
        {
            Position = p;
            Heading = h.normalized;
            Strength = s;
        }

        public WeightedWaypoint(IWeightedControlPoint waypoint)
        {
            this.Position = waypoint.Position;
            this.Heading = waypoint.Heading.normalized;
            this.Strength = waypoint.Strength;
        }

        #region IWaypoint Interface

        Vector3 IControlPoint.Heading
        {
            get { return this.Heading; }
            set { this.Heading = value.normalized; }
        }

        Vector3 IControlPoint.Position
        {
            get
            {
                return this.Position;
            }
            set
            {
                this.Position = value;
            }
        }

        float IWeightedControlPoint.Strength
        {
            get
            {
                return this.Strength;
            }
            set
            {
                this.Strength = value;
            }
        }

        public bool IsInvalid { get { return this.Heading == Vector3.zero; } }

        #endregion

        #region Operator Interface

        public static bool Compare(IWeightedControlPoint a, IWeightedControlPoint b)
        {
            return VectorUtil.FuzzyEquals(a.Position, b.Position) && VectorUtil.FuzzyEquals(a.Heading, b.Heading) && MathUtil.FuzzyEqual(a.Strength, b.Strength);
        }

        public static bool Compare(WeightedWaypoint a, IWeightedControlPoint b)
        {
            return !a.IsInvalid && VectorUtil.FuzzyEquals(a.Position, b.Position) && MathUtil.FuzzyEqual(a.Strength, b.Strength);
        }

        public static bool Compare(IWeightedControlPoint a, WeightedWaypoint b)
        {
            return !b.IsInvalid && VectorUtil.FuzzyEquals(a.Position, b.Position) && VectorUtil.FuzzyEquals(a.Heading, b.Heading) && MathUtil.FuzzyEqual(a.Strength, b.Strength);
        }

        public static bool Compare(WeightedWaypoint a, WeightedWaypoint b)
        {
            return (a.IsInvalid && b.IsInvalid) || (VectorUtil.FuzzyEquals(a.Position, b.Position) && VectorUtil.FuzzyEquals(a.Heading, b.Heading) && MathUtil.FuzzyEqual(a.Strength, b.Strength));
        }

        #endregion

        public static readonly WeightedWaypoint Invalid = new WeightedWaypoint(Vector3.zero, Vector3.zero, 0f);

    }

}
