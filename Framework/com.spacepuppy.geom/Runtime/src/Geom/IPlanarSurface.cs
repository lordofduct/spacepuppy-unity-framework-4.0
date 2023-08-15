using com.spacepuppy.Project;
using UnityEngine;

namespace com.spacepuppy.Geom
{

    /// <summary>
    /// Interface for describing a 2d surface on which 3d information can be projected.
    /// 
    /// IPlanarSurfaces are useful for things like a 2.5d games where your motion is along a 2d cylindrical surface traveling through 3-space.
    /// </summary>
    public interface IPlanarSurface
    {

        /// <summary>
        /// Returns the surface normal at the 2D surface location.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        Vector3 GetSurfaceNormal(Vector2 location);

        /// <summary>
        /// Return the surface normal at the position on the curve closest to location.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        /// <remarks>When implementing the location isn't necessarily going to be on the surface so it should be treated as the point resolved from ClampToSurface.</remarks>
        Vector3 GetSurfaceNormal(Vector3 location);

        /// <summary>
        /// Converts a 3d vector to closest 2d vector on 2D surface
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        Vector2 ProjectVectorTo2D(Vector3 location, Vector3 v);

        /// <summary>
        /// Converts a 2d vector from the gameplay surface to the 3d world
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        Vector3 ProjectVectorTo3D(Vector3 location, Vector2 v);

        /// <summary>
        /// Converts a 3d position to closest 2d position on the 2d gameplay surface
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        Vector2 ProjectPosition2D(Vector3 v);

        /// <summary>
        /// Converts a 2d position from the gameplay surface to the 3d world
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        Vector3 ProjectPosition3D(Vector2 v);

        /// <summary>
        /// Returns a position on the closest point on the planar surface.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        Vector3 ClampToSurface(Vector3 v);

    }

    [System.Serializable]
    public class PlanarSurfaceRef : SerializableInterfaceRef<IPlanarSurface> { }

    public static class PlanarSurfaceExtensions
    {

        public static Vector3 MirrorPosition(this IPlanarSurface surface, Vector3 pos)
        {
            var cpos = surface.ClampToSurface(pos);
            var pn = surface.GetSurfaceNormal(cpos);
            var pd = Vector3.Dot(cpos, pn);

            //var d = Vector3.Dot(pn, (pos - pn * pd));
            var d = Vector3.Dot(pn, pos) - pd; //this way is just fewer calculations, both work
            return pos - pn * d * 2f;
        }

        public static Vector3 MirrorDirection(this IPlanarSurface surface, Vector3 pos, Vector3 v)
        {
            var pn = surface.GetSurfaceNormal(pos);
            var pd = Vector3.Dot(v, pn);
            return v - pn * pd * 2f;
        }

        public static Quaternion GetMirrorLookRotation(this IPlanarSurface surface, Vector3 pos, Vector3 forw, Vector3 up)
        {
            forw = surface.MirrorDirection(pos, forw);
            up = surface.MirrorDirection(pos, up);
            return Quaternion.LookRotation(forw, up);
        }

        public static Vector3 FlattenDirection(this IPlanarSurface surface, Vector3 pos, Vector3 dir)
        {
            var n = surface.GetSurfaceNormal(pos);
            return dir - (n * Vector3.Dot(dir, n));
        }

    }

}
