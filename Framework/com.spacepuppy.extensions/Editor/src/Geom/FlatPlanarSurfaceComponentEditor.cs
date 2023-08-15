using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Geom;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Geom
{

    [CustomEditor(typeof(FlatPlanarSurfaceComponent))]
    public class FlatPlanarSurfaceComponentEditor : SPEditor
    {


        [DrawGizmo(GizmoType.Selected, drawnType = typeof(FlatPlanarSurfaceComponent))]
        static void DrawGizmos(FlatPlanarSurfaceComponent surface, GizmoType gizmoType)
        {
            try
            {
                Gizmos.color = Color.green.SetAlpha(0.3f);
                Gizmos.matrix = Matrix4x4.TRS(surface.transform.position, surface.transform.rotation, Vector3.one);

                Gizmos.DrawCube(Vector3.zero, new Vector3(10f, 10f, 0.01f));

                Gizmos.color = Color.red.SetAlpha(0.5f);
                Gizmos.DrawRay(Vector3.zero, Vector3.forward);
            }
            finally
            {
                Gizmos.matrix = Matrix4x4.identity;
            }
        }


    }

}
