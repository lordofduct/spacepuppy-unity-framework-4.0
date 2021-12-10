using UnityEngine;

namespace com.spacepuppy.Waypoints
{

    public interface IControlPoint
    {

        Vector3 Position { get; set; }
        Vector3 Heading { get; set; }

    }

    public interface IWeightedControlPoint : IControlPoint
    {
        float Strength { get; set; }
    }

}
