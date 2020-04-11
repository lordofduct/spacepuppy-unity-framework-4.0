using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Waypoints;
using com.spacepuppy.Utils;

using com.spacepuppyeditor.Internal;

namespace com.spacepuppyeditor.Waypoints
{

    [CustomEditor(typeof(WaypointPathComponent))]
    public class WaypointPathComponentInspector : SPEditor
    {

        #region Fields

        private const float SEG_LENGTH = 0.2f;

        private const string PROP_CONTROLPOINTS = "_controlPoints";
        private const string PROP_NODE_OWNER = "_owner";
        private const float ARG_BTN_WIDTH = 18f;

        private WaypointPathComponent _targ;

        private SerializedProperty _nodesProp;
        private SPReorderableList _nodeList;
        private GUIContent _argBtnLabel = new GUIContent("||", "Toggle allowing to manually configure which Transform is this waypoint.");

        private List<WaypointPathComponent.TransformControlPoint> _lastNodeCache = new List<WaypointPathComponent.TransformControlPoint>();

        #endregion

        #region CONSTRUCTOR

        protected override void OnEnable()
        {
            base.OnEnable();

            _targ = this.target as WaypointPathComponent;

            _nodesProp = this.serializedObject.FindProperty(PROP_CONTROLPOINTS);
            _nodeList = new SPReorderableList(this.serializedObject, _nodesProp);
            _nodeList.elementHeight = EditorGUIUtility.singleLineHeight;
            _nodeList.drawHeaderCallback = _nodeList_DrawHeader;
            _nodeList.drawElementCallback = _nodeList_DrawElement;
            _nodeList.onAddCallback = _nodeList_OnAdded;

            _lastNodeCache.Clear();
            _lastNodeCache.AddRange(this.GetCurrentNodes());
        }

        #endregion

        #region Methods

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            this.DrawDefaultInspectorExcept(PROP_CONTROLPOINTS);

            //clean nodes
            for (int i = 0; i < _nodesProp.arraySize; i++)
            {
                if (_nodesProp.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    _nodesProp.DeleteArrayElementAtIndex(i);
                    i--;
                }
            }

            var currentNodes = this.GetCurrentNodes().ToArray();
            if (_nodesProp.arraySize != _lastNodeCache.Count || !_lastNodeCache.SequenceEqual(currentNodes))
            {
                //delete any nodes that are no longer in the collection
                for (int i = 0; i < _lastNodeCache.Count; i++)
                {
                    if (_lastNodeCache[i] != null && !currentNodes.Contains(_lastNodeCache[i]) && _lastNodeCache[i].transform.parent == _targ.transform)
                    {
                        ObjUtil.SmartDestroy(_lastNodeCache[i].gameObject);
                    }
                }

                //update cache
                _lastNodeCache.Clear();
                _lastNodeCache.AddRange(currentNodes);

                //update names
                var rx = new Regex(@"^Node\d+$");
                for (int i = 0; i < _lastNodeCache.Count; i++)
                {
                    var node = _lastNodeCache[i];
                    var nm = "Node" + i.ToString("000");
                    if (node != null && node.transform.parent == _targ.transform && node.name != nm && rx.IsMatch(node.name))
                    {
                        node.name = nm;
                    }
                }
            }


            _nodeList.DoLayoutList();


            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorHelper.TempContent("Scan", "Scans all child Transforms and makes a path out of them in the order they were found. All strengths will be set to 0.5f. Any objects named 'Visual' will be ignored."), GUILayout.MaxWidth(75f)))
            {
                var go = GameObjectUtil.GetGameObjectFromSource(this.serializedObject.targetObject);
                if (go != null)
                {
                    var arr = go.transform.OfType<Transform>().Where(t => t.name != "Visual").ToArray();
                    _nodesProp.arraySize = arr.Length;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        var c = arr[i].AddOrGetComponent<WaypointPathComponent.TransformControlPoint>();
                        c.Strength = 0.5f;
                        WaypointPathComponent.EditorHelper.SetParent(c, target as WaypointPathComponent);
                        Undo.RegisterCreatedObjectUndo(go, "Associate Node With Waypoint Path");
                        _nodesProp.GetArrayElementAtIndex(i).objectReferenceValue = c;
                    }
                }
            }
            GUILayout.EndHorizontal();

            this.serializedObject.ApplyModifiedProperties();
        }

        private IEnumerable<WaypointPathComponent.TransformControlPoint> GetCurrentNodes()
        {
            for (int i = 0; i < _nodesProp.arraySize; i++)
            {
                yield return _nodesProp.GetArrayElementAtIndex(i).objectReferenceValue as WaypointPathComponent.TransformControlPoint;
            }
        }

        #endregion

        #region NodeList Handlers

        private void _nodeList_DrawHeader(Rect area)
        {
            EditorGUI.LabelField(area, "Waypoints");
        }

        private void _nodeList_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            if (area.width < ARG_BTN_WIDTH)
            {
                return;
            }

            var elementProp = _nodesProp.GetArrayElementAtIndex(index);
            var btnRect = new Rect(area.xMax - ARG_BTN_WIDTH, area.yMin, ARG_BTN_WIDTH, area.height);
            var propRect = new Rect(area.xMin, area.yMin, area.width - ARG_BTN_WIDTH, area.height);

            if (elementProp.objectReferenceValue == null || elementProp.isExpanded)
            {
                var owner = this.serializedObject.targetObject as WaypointPathComponent;
                var obj = EditorGUI.ObjectField(propRect, EditorHelper.TempContent("Node"), elementProp.objectReferenceValue, typeof(WaypointPathComponent.TransformControlPoint), true) as WaypointPathComponent.TransformControlPoint;
                if (obj != null)
                {
                    if (obj.transform.parent != ObjUtil.GetAsFromSource<Transform>(this.serializedObject.targetObject))
                    {
                        obj = null;
                    }
                    else if (obj.Owner != owner)
                    {
                        Undo.RecordObject(obj, "Set Waypoint Path Node Owner");
                        WaypointPathComponent.EditorHelper.SetParent(obj, owner);
                    }
                }
                elementProp.objectReferenceValue = obj;
            }
            else
            {
                var t = elementProp.objectReferenceValue as WaypointPathComponent.TransformControlPoint;
                EditorGUI.LabelField(propRect, EditorHelper.TempContent(t.name), EditorHelper.TempContent("{ strength: " + t.Strength.ToString("0.00") + " }"));

                if (ReorderableListHelper.IsClickingArea(area))
                {
                    EditorGUIUtility.PingObject(t);
                }
            }

            if (GUI.Button(btnRect, _argBtnLabel))
            {
                elementProp.isExpanded = !elementProp.isExpanded;
            }

            if (GUI.enabled) ReorderableListHelper.DrawDraggableElementDeleteContextMenu(_nodeList, area, index, isActive, isFocused);
        }

        private void _nodeList_OnAdded(ReorderableList lst)
        {
            if (Application.isPlaying) return;

            lst.serializedProperty.arraySize++;
            lst.index = lst.serializedProperty.arraySize - 1;

            var elementProp = lst.serializedProperty.GetArrayElementAtIndex(lst.index);
            var lastNode = (lst.index > 1) ? lst.serializedProperty.GetArrayElementAtIndex(lst.index - 1).objectReferenceValue as WaypointPathComponent.TransformControlPoint : null;

            var go = new GameObject("Node" + lst.index.ToString("000"));
            IconHelper.SetIconForObject(go, IconHelper.Icon.DiamondPurple);
            go.transform.parent = _targ.transform;
            if (lastNode != null)
            {
                go.transform.position = lastNode.transform.position;
                go.transform.rotation = lastNode.transform.rotation;
                go.transform.localScale = lastNode.transform.localScale;
            }
            else
            {
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.LookRotation(Vector3.forward);
                go.transform.localScale = Vector3.one * 0.5f;
            }
            var obj = go.AddOrGetComponent<WaypointPathComponent.TransformControlPoint>();
            WaypointPathComponent.EditorHelper.SetParent(obj, this.serializedObject.targetObject as WaypointPathComponent);
            Undo.RegisterCreatedObjectUndo(go, "Create Node For Waypoint Path");
            elementProp.objectReferenceValue = obj;
        }

        #endregion


        #region Gizmos

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        private static void OnDrawGizmos(WaypointPathComponent c, GizmoType gizmoType)
        {
            if (gizmoType.HasFlag(GizmoType.NotInSelectionHierarchy) && !c.transform.IsParentOf(Selection.activeTransform)) return;

            var cam = SceneView.lastActiveSceneView.camera;
            if (cam == null) return;

            var path = WaypointPathComponent.GetPath(c, false);
            if (path == null || path.Count == 0) return;
            var matrix = (c.started && c.TransformRelativeTo != null) ? Matrix4x4.TRS(c.TransformRelativeTo.position, c.TransformRelativeTo.rotation, Vector3.one) : Matrix4x4.identity;

            Gizmos.color = Color.red;

            float arclength = path.GetArcLength();
            float seglength = Mathf.Max(SEG_LENGTH, arclength / Mathf.Min(path.Count * 8, 5000));
            Vector3? lastPnt = null;
            using (var pnts = TempCollection.GetCallbackCollection<Vector3>((p) =>
            {
                var p0 = lastPnt ?? Vector3.zero;
                lastPnt = p;
                if (lastPnt == null || (!PointVisibleInCam(cam, p0) && !PointVisibleInCam(cam, p))) return;

                Gizmos.DrawLine(matrix.MultiplyPoint3x4(p0), matrix.MultiplyPoint3x4(p));
            }))
            {
                path.GetDetailedPositions(pnts, seglength);
            }

            IControlPoint pnt;

            pnt = path.ControlPoint(0);
            if (PointVisibleInCam(cam, pnt.Position))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(matrix.MultiplyPoint3x4(pnt.Position), Vector3.one * 0.5f);
            }

            if (path.Count > 1)
            {
                Gizmos.color = Color.yellow;
                for (int i = 1; i < path.Count - 1; i++)
                {
                    pnt = path.ControlPoint(i);
                    if (PointVisibleInCam(cam, pnt.Position))
                    {
                        Gizmos.DrawWireSphere(matrix.MultiplyPoint3x4(pnt.Position), 0.25f);
                    }
                }

                Gizmos.color = Color.red;
                pnt = path.ControlPoint(path.Count - 1);
                if (PointVisibleInCam(cam, pnt.Position))
                {
                    Gizmos.DrawWireCube(matrix.MultiplyPoint3x4(pnt.Position), Vector3.one * 0.5f);
                }
            }
        }

        #endregion

        #region Utils

        private static bool PointVisibleInCam(Camera cam, Vector3 pnt)
        {
            var cp = cam.WorldToScreenPoint(pnt);
            return (cp.x >= 0f && cp.x <= cam.pixelWidth && cp.y >= 0f && cp.y <= cam.pixelHeight);
        }

        #endregion

    }

    [CustomEditor(typeof(WaypointPathComponent.TransformControlPoint))]
    public class WaypointPathComponent_TransformWaypointInspector : SPEditor
    {
        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.LabelField("WAYPOINT NODE", EditorStyles.boldLabel);

            GUI.enabled = false;
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("_owner"));
            GUI.enabled = true;

            this.serializedObject.ApplyModifiedProperties();



            var trans = ObjUtil.GetAsFromSource<Transform>(this.serializedObject.targetObject);
            float scale = ObjUtil.GetAsFromSource<Transform>(this.serializedObject.targetObject).localScale.z;
            EditorGUI.BeginChangeCheck();
            scale = EditorGUILayout.Slider("Strength", scale, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(trans, "Change Waypoint Path Control Point Strength");
                trans.localScale = Vector3.one * scale;
            }


            var targ = this.serializedObject.targetObject as WaypointPathComponent.TransformControlPoint;
            if (targ == null || targ.Owner == null || Application.isPlaying) return;

            if (GUILayout.Button("Add Node After This"))
            {
                var go = new GameObject("Node" + targ.Owner.Count.ToString("000"));
                go.transform.parent = targ.Owner.transform;
                go.transform.position = targ.transform.position;
                go.transform.rotation = targ.transform.rotation;
                go.transform.localScale = targ.transform.localScale;
                var newwaypoint = go.AddComponent<WaypointPathComponent.TransformControlPoint>();
                WaypointPathComponent.EditorHelper.InsertAfter(targ.Owner, newwaypoint, targ);
                Undo.RegisterCreatedObjectUndo(go, "Create Node For Waypoint Path");
                Undo.RecordObject(targ.Owner, "Add Node To Waypoint Path");
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

                Selection.activeObject = go;
            }
        }
    }

}
