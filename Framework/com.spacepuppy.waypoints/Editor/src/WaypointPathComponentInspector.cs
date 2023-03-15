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

    [CustomEditor(typeof(WaypointPathComponent), true)]
    public class WaypointPathComponentInspector : SPEditor
    {

        #region Fields

        private const float SEG_LENGTH = 0.2f;

        public const string PROP_PATHTYPE = "_pathType";
        public const string PROP_CLOSED = "_closed";
        public const string PROP_TRANSFORMRELATIVETO = "_transformRelativeTo";
        public const string PROP_CONTROLPOINTSANIMATE = "_controlPointsAnimate";
        public const string PROP_CONTROLPOINTS = "_controlPoints";
        private const float ARG_BTN_WIDTH = 18f;

        private WaypointPathComponent _targ;

        private SerializedProperty _nodesProp;
        private SPReorderableList _nodeList;
        private GUIContent _argBtnLabel = new GUIContent("||", "Toggle allowing to manually configure which Transform is this waypoint.");

        private List<TransformControlPoint> _lastNodeCache = new List<TransformControlPoint>();

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

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(PROP_PATHTYPE);
            this.DrawPropertyField(PROP_CLOSED);
            this.DrawPropertyField(PROP_TRANSFORMRELATIVETO);
            this.DrawPropertyField(PROP_CONTROLPOINTSANIMATE);

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
                        var c = _targ.InitializeTransformAsControlPoint(arr[i]);
                        c.Strength = 0.5f;
                        Undo.RegisterCreatedObjectUndo(go, "Associate Node With Waypoint Path");
                        _nodesProp.GetArrayElementAtIndex(i).objectReferenceValue = c;
                    }
                }
            }
            GUILayout.EndHorizontal();

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, PROP_PATHTYPE, PROP_CLOSED, PROP_TRANSFORMRELATIVETO, PROP_CONTROLPOINTSANIMATE, PROP_CONTROLPOINTS);

            this.serializedObject.ApplyModifiedProperties();
        }

        private IEnumerable<TransformControlPoint> GetCurrentNodes()
        {
            for (int i = 0; i < _nodesProp.arraySize; i++)
            {
                yield return _nodesProp.GetArrayElementAtIndex(i).objectReferenceValue as TransformControlPoint;
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
                var obj = EditorGUI.ObjectField(propRect, EditorHelper.TempContent("Node"), elementProp.objectReferenceValue, typeof(TransformControlPoint), true) as TransformControlPoint;
                if (obj != null)
                {
                    if (obj.transform.parent != ObjUtil.GetAsFromSource<Transform>(this.serializedObject.targetObject))
                    {
                        obj = null;
                    }
                    else if (obj.Owner != owner)
                    {
                        Undo.RecordObject(obj, "Set Waypoint Path Node Owner");
                        obj = _targ.InitializeTransformAsControlPoint(obj.transform);
                        EditorHelper.CommitDirectChanges(obj, true);
                    }
                }
                elementProp.objectReferenceValue = obj;
            }
            else
            {
                var t = elementProp.objectReferenceValue as TransformControlPoint;
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
            var lastNode = (lst.index > 1) ? lst.serializedProperty.GetArrayElementAtIndex(lst.index - 1).objectReferenceValue as TransformControlPoint : null;

            var go = new GameObject("Node" + lst.index.ToString("000"));
            IconHelper.SetIconForObject(go, IconHelper.Icon.DiamondPurple);
            go.transform.SetParent(_targ.transform, false);
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
            var obj = _targ.InitializeTransformAsControlPoint(go.transform);
            Undo.RegisterCreatedObjectUndo(go, "Create Node For Waypoint Path");
            elementProp.objectReferenceValue = obj;
        }

        #endregion


        #region Gizmos

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        private static void OnDrawGizmos(WaypointPathComponent c, GizmoType gizmoType)
        {
            if (gizmoType.HasFlag(GizmoType.NotInSelectionHierarchy) && !c.transform.IsParentOf(Selection.activeTransform)) return;

            DrawWaypointPath(c, gizmoType);
        }

        protected static void DrawWaypointPath(WaypointPathComponent c, GizmoType gizmoType)
        {
            var cam = SceneView.lastActiveSceneView.camera;
            if (cam == null) return;

            var path = WaypointPathComponent.GetPath(c, false);
            if (path == null || path.Count == 0) return;
            var matrix = (c.started && c.TransformRelativeTo != null) ? Matrix4x4.TRS(c.TransformRelativeTo.position, c.TransformRelativeTo.rotation, Vector3.one) : Matrix4x4.identity;

            Gizmos.color = Color.red;
            float seglength = Mathf.Max(SEG_LENGTH, path.GetArcLength() / 5000f);
            int cnt = 0;
            Vector3? lastPnt = null;
            using (var pnts = TempCollection.GetCallbackCollection<Vector3>((p) =>
            {
                if (c.TransformRelativeTo) p = c.TransformRelativeTo.TransformPoint(p);

                var p0 = lastPnt ?? Vector3.zero;
                var pt0 = matrix.MultiplyPoint3x4(p0);
                var pt1 = matrix.MultiplyPoint3x4(p);
                lastPnt = p;
                cnt++;
                if (cnt == 1 || lastPnt == null || (!PointVisibleInCam(cam, pt0) && !PointVisibleInCam(cam, pt1))) return;

                Gizmos.DrawLine(pt0, pt1);
            }))
            {
                path.GetDetailedPositions(pnts, seglength);
            }

            //draw control points
            for (int i = 0; i < path.Count; i++)
            {
                var p = path.ControlPoint(i).Position;
                if (c.TransformRelativeTo) p = c.TransformRelativeTo.TransformPoint(p);

                if (PointVisibleInCam(cam, p))
                {
                    if (i == 0)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireCube(matrix.MultiplyPoint3x4(p), Vector3.one * 0.5f);
                    }
                    else if (i == path.Count - 1)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireCube(matrix.MultiplyPoint3x4(p), Vector3.one * 0.5f);
                    }
                    else
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireSphere(matrix.MultiplyPoint3x4(p), 0.25f);
                    }
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

    [CustomEditor(typeof(TransformControlPoint), true)]
    [CanEditMultipleObjects()]
    public class TransformWaypointInspector : SPEditor
    {

        public const string PROP_OWNER = "_owner";

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);

            EditorGUILayout.LabelField("WAYPOINT NODE", EditorStyles.boldLabel);

            GUI.enabled = false;
            this.DrawPropertyField(PROP_OWNER);
            GUI.enabled = true;

            if (this.serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.LabelField("Strength", "Not supported in multi-edit mode.");
            }
            else
            {
                var trans = ObjUtil.GetAsFromSource<Transform>(this.serializedObject.targetObject);
                float scale = ObjUtil.GetAsFromSource<Transform>(this.serializedObject.targetObject).localScale.z;
                EditorGUI.BeginChangeCheck();
                scale = EditorGUILayout.Slider("Strength", scale, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(trans, "Change Waypoint Path Control Point Strength");
                    trans.localScale = Vector3.one * scale;
                    EditorHelper.CommitDirectChanges(trans, true);
                }
            }

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, PROP_OWNER);

            this.serializedObject.ApplyModifiedProperties();

            if (this.serializedObject.isEditingMultipleObjects) return;

            //draw button at bottom
            var targ = this.serializedObject.targetObject as TransformControlPoint;
            if (targ == null || targ.Owner == null || Application.isPlaying) return;

            if (GUILayout.Button("Add Node After This"))
            {
                var go = new GameObject("Node" + targ.Owner.Count.ToString("000"));
                go.transform.SetParent(targ.Owner.transform, false);
                go.transform.position = targ.transform.position;
                go.transform.rotation = targ.transform.rotation;
                go.transform.localScale = targ.transform.localScale;
                var newwaypoint = go.AddComponent<TransformControlPoint>();
                Undo.RegisterCreatedObjectUndo(go, "Create Node For Waypoint Path");
                Undo.RecordObjects(new UnityEngine.Object[] { targ.Owner, newwaypoint }, "Add Node To Waypoint Path");
                WaypointPathComponent.EditorHelper.InsertAfter(targ.Owner, newwaypoint, targ);
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

                Selection.activeObject = go;
            }

        }
    }

}
