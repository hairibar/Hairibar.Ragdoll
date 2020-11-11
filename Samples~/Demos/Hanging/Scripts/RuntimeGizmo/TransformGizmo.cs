using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuntimeGizmos
{
    //To be safe, if you are changing any transforms hierarchy, such as parenting an object to something,
    //you should call ClearTargets before doing so just to be sure nothing unexpected happens... as well as call UndoRedoManager.Clear()
    //For example, if you select an object that has children, move the children elsewhere, deselect the original object, then try to add those old children to the selection, I think it wont work.

    [RequireComponent(typeof(Camera))]
    public class TransformGizmo : MonoBehaviour
    {
        public Transform selectedObject;
        public float maxMovePerSecond = 5;

        TransformSpace space = TransformSpace.Global;
        TransformType transformType = TransformType.Move;
        TransformPivot pivot = TransformPivot.Pivot;
        CenterType centerType = CenterType.All;

        [Header("Gizmo visuals")]
        public Color xColor = new Color(1, 0, 0, 0.8f);
        public Color yColor = new Color(0, 1, 0, 0.8f);
        public Color zColor = new Color(0, 0, 1, 0.8f);
        public Color allColor = new Color(.7f, .7f, .7f, 0.8f);
        public Color selectedColor = new Color(1, 1, 0, 0.8f);
        public Color hoverColor = new Color(1, .75f, 0, 0.8f);
        public float planesOpacity = .5f;
        //public Color rectPivotColor = new Color(0, 0, 1, 0.8f);
        //public Color rectCornerColor = new Color(0, 0, 1, 0.8f);
        //public Color rectAnchorColor = new Color(.7f, .7f, .7f, 0.8f);
        //public Color rectLineColor = new Color(.7f, .7f, .7f, 0.8f);

        public float handleLength = .25f;
        public float handleWidth = .003f;
        public float planeSize = .035f;
        public float triangleSize = .03f;
        public float boxSize = .03f;
        public int circleDetail = 40;
        public float allMoveHandleLengthMultiplier = 1f;
        public float allRotateHandleLengthMultiplier = 1.4f;
        public float allScaleHandleLengthMultiplier = 1.6f;
        public float minSelectedDistanceCheck = .01f;
        public float moveSpeedMultiplier = 1f;
        public float scaleSpeedMultiplier = 1f;
        public float rotateSpeedMultiplier = 1f;
        public float allRotateSpeedMultiplier = 20f;

        public bool useFirstSelectedAsMain = true;


        //Mainly for if you want the pivot point to update correctly if selected objects are moving outside the transformgizmo.
        //Might be poor on performance if lots of objects are selected...
        public bool forceUpdatePivotPointOnChange = true;

        public bool manuallyHandleGizmo;

        public Action onCheckForSelectedAxis;
        public Action onDrawCustomGizmo;

        public Camera myCamera { get; private set; }

        public bool isTransforming { get; private set; }
        public float totalScaleAmount { get; private set; }
        public Quaternion totalRotationAmount { get; private set; }
        public Axis translatingAxis => nearAxis;
        public Axis translatingAxisPlane => planeAxis;
        public bool hasTranslatingAxisPlane => translatingAxisPlane != Axis.None && translatingAxisPlane != Axis.Any;
        public TransformType transformingType => translatingType;

        public Vector3 pivotPoint { get; private set; }
        Vector3 totalCenterPivotPoint;

        public Transform mainTargetRoot => (targetRootsOrdered.Count > 0) ? (useFirstSelectedAsMain) ? targetRootsOrdered[0] : targetRootsOrdered[targetRootsOrdered.Count - 1] : null;

        AxisInfo axisInfo;
        Axis nearAxis = Axis.None;
        Axis planeAxis = Axis.None;
        TransformType translatingType;

        AxisVectors handleLines = new AxisVectors();
        AxisVectors handlePlanes = new AxisVectors();
        AxisVectors handleTriangles = new AxisVectors();
        AxisVectors handleSquares = new AxisVectors();
        AxisVectors circlesLines = new AxisVectors();

        //We use a HashSet and a List for targetRoots so that we get fast lookup with the hashset while also keeping track of the order with the list.
        List<Transform> targetRootsOrdered = new List<Transform>();
        Dictionary<Transform, TargetInfo> targetRoots = new Dictionary<Transform, TargetInfo>();
        HashSet<Renderer> highlightedRenderers = new HashSet<Renderer>();
        HashSet<Transform> children = new HashSet<Transform>();

        List<Transform> childrenBuffer = new List<Transform>();
        List<Renderer> renderersBuffer = new List<Renderer>();
        List<Material> materialsBuffer = new List<Material>();

        WaitForEndOfFrame waitForEndOFFrame = new WaitForEndOfFrame();
        Coroutine forceUpdatePivotCoroutine;

        static Material lineMaterial;
        static Material outlineMaterial;

        void Awake()
        {
            myCamera = GetComponent<Camera>();
            SetMaterial();
        }

        void OnEnable()
        {
            forceUpdatePivotCoroutine = StartCoroutine(ForceUpdatePivotPointAtEndOfFrame());

            AddTarget(selectedObject);
        }

        void OnDisable()
        {
            ClearTargets(); //Just so things gets cleaned up, such as removing any materials we placed on objects.

            StopCoroutine(forceUpdatePivotCoroutine);
        }

        void OnDestroy()
        {
            ClearAllHighlightedRenderers();
        }

        void Update()
        {
            if (manuallyHandleGizmo)
            {
                onCheckForSelectedAxis?.Invoke();
            }
            else
            {
                SetNearAxis();
            }

            TransformSelected();
        }

        void LateUpdate()
        {
            if (mainTargetRoot == null) return;

            //We run this in lateupdate since coroutines run after update and we want our gizmos to have the updated target transform position after TransformSelected()
            SetAxisInfo();

            if (manuallyHandleGizmo)
            {
                onDrawCustomGizmo?.Invoke();
            }
            else
            {
                SetLines();
            }
        }

        void OnPostRender()
        {
            if (mainTargetRoot == null || manuallyHandleGizmo) return;

            lineMaterial.SetPass(0);

            Color xColor = (nearAxis == Axis.X) ? (isTransforming) ? selectedColor : hoverColor : this.xColor;
            Color yColor = (nearAxis == Axis.Y) ? (isTransforming) ? selectedColor : hoverColor : this.yColor;
            Color zColor = (nearAxis == Axis.Z) ? (isTransforming) ? selectedColor : hoverColor : this.zColor;
            Color allColor = (nearAxis == Axis.Any) ? (isTransforming) ? selectedColor : hoverColor : this.allColor;

            //Note: The order of drawing the axis decides what gets drawn over what.

            TransformType moveOrScaleType = (transformType == TransformType.Scale || (isTransforming && translatingType == TransformType.Scale)) ? TransformType.Scale : TransformType.Move;
            DrawQuads(handleLines.z, GetColor(moveOrScaleType, this.zColor, zColor, hasTranslatingAxisPlane));
            DrawQuads(handleLines.x, GetColor(moveOrScaleType, this.xColor, xColor, hasTranslatingAxisPlane));
            DrawQuads(handleLines.y, GetColor(moveOrScaleType, this.yColor, yColor, hasTranslatingAxisPlane));

            DrawTriangles(handleTriangles.x, GetColor(TransformType.Move, this.xColor, xColor, hasTranslatingAxisPlane));
            DrawTriangles(handleTriangles.y, GetColor(TransformType.Move, this.yColor, yColor, hasTranslatingAxisPlane));
            DrawTriangles(handleTriangles.z, GetColor(TransformType.Move, this.zColor, zColor, hasTranslatingAxisPlane));

            DrawQuads(handlePlanes.z, GetColor(TransformType.Move, this.zColor, zColor, planesOpacity, !hasTranslatingAxisPlane));
            DrawQuads(handlePlanes.x, GetColor(TransformType.Move, this.xColor, xColor, planesOpacity, !hasTranslatingAxisPlane));
            DrawQuads(handlePlanes.y, GetColor(TransformType.Move, this.yColor, yColor, planesOpacity, !hasTranslatingAxisPlane));

            DrawQuads(handleSquares.x, GetColor(TransformType.Scale, this.xColor, xColor));
            DrawQuads(handleSquares.y, GetColor(TransformType.Scale, this.yColor, yColor));
            DrawQuads(handleSquares.z, GetColor(TransformType.Scale, this.zColor, zColor));
            DrawQuads(handleSquares.all, GetColor(TransformType.Scale, this.allColor, allColor));

            DrawQuads(circlesLines.all, GetColor(TransformType.Rotate, this.allColor, allColor));
            DrawQuads(circlesLines.x, GetColor(TransformType.Rotate, this.xColor, xColor));
            DrawQuads(circlesLines.y, GetColor(TransformType.Rotate, this.yColor, yColor));
            DrawQuads(circlesLines.z, GetColor(TransformType.Rotate, this.zColor, zColor));
        }

        Color GetColor(TransformType type, Color normalColor, Color nearColor, bool forceUseNormal = false)
        {
            return GetColor(type, normalColor, nearColor, false, 1, forceUseNormal);
        }
        Color GetColor(TransformType type, Color normalColor, Color nearColor, float alpha, bool forceUseNormal = false)
        {
            return GetColor(type, normalColor, nearColor, true, alpha, forceUseNormal);
        }
        Color GetColor(TransformType type, Color normalColor, Color nearColor, bool setAlpha, float alpha, bool forceUseNormal = false)
        {
            Color color;
            if (!forceUseNormal && TranslatingTypeContains(type, false))
            {
                color = nearColor;
            }
            else
            {
                color = normalColor;
            }

            if (setAlpha)
            {
                color.a = alpha;
            }

            return color;
        }


        //We only support scaling in local space.
        public TransformSpace GetProperTransformSpace()
        {
            return transformType == TransformType.Scale ? TransformSpace.Local : space;
        }

        public bool TransformTypeContains(TransformType type)
        {
            return TransformTypeContains(transformType, type);
        }
        public bool TranslatingTypeContains(TransformType type, bool checkIsTransforming = true)
        {
            TransformType transType = !checkIsTransforming || isTransforming ? translatingType : transformType;
            return TransformTypeContains(transType, type);
        }
        public bool TransformTypeContains(TransformType mainType, TransformType type)
        {
            return ExtTransformType.TransformTypeContains(mainType, type, GetProperTransformSpace());
        }

        public float GetHandleLength(TransformType type, Axis axis = Axis.None, bool multiplyDistanceMultiplier = true)
        {
            float length = handleLength;
            if (transformType == TransformType.All)
            {
                if (type == TransformType.Move) length *= allMoveHandleLengthMultiplier;
                if (type == TransformType.Rotate) length *= allRotateHandleLengthMultiplier;
                if (type == TransformType.Scale) length *= allScaleHandleLengthMultiplier;
            }

            if (multiplyDistanceMultiplier) length *= GetDistanceMultiplier();

            if (type == TransformType.Scale && isTransforming && (translatingAxis == axis || translatingAxis == Axis.Any)) length += totalScaleAmount;

            return length;
        }

        void TransformSelected()
        {
            if (mainTargetRoot != null)
            {
                if (nearAxis != Axis.None && Input.GetMouseButtonDown(0))
                {
                    StartCoroutine(TransformSelected(translatingType));
                }
            }
        }

        IEnumerator TransformSelected(TransformType transType)
        {
            isTransforming = true;
            totalScaleAmount = 0;
            totalRotationAmount = Quaternion.identity;

            Vector3 originalPivot = pivotPoint;

            Vector3 axis = GetNearAxisDirection(out Vector3 otherAxis1, out Vector3 otherAxis2);
            Vector3 planeNormal = hasTranslatingAxisPlane ? axis : (transform.position - originalPivot).normalized;
            Vector3 projectedAxis = Vector3.ProjectOnPlane(axis, planeNormal).normalized;
            Vector3 previousMousePosition = Vector3.zero;

            Vector3 currentSnapMovementAmount = Vector3.zero;

            while (!Input.GetMouseButtonUp(0))
            {
                Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);
                Vector3 mousePosition = Geometry.LinePlaneIntersect(mouseRay.origin, mouseRay.direction, originalPivot, planeNormal);

                if (previousMousePosition != Vector3.zero && mousePosition != Vector3.zero)
                {
                    if (transType == TransformType.Move)
                    {
                        Vector3 movement;

                        if (hasTranslatingAxisPlane)
                        {
                            movement = mousePosition - previousMousePosition;
                        }
                        else
                        {
                            float moveAmount = ExtVector3.MagnitudeInDirection(mousePosition - previousMousePosition, projectedAxis) * moveSpeedMultiplier;
                            movement = axis * moveAmount;
                        }

                        Vector3 movementPerSecond = movement / Time.deltaTime;
                        movementPerSecond = movementPerSecond.normalized * Mathf.Min(movementPerSecond.magnitude, maxMovePerSecond);

                        movement = movementPerSecond * Time.deltaTime;
                        for (int i = 0; i < targetRootsOrdered.Count; i++)
                        {
                            Transform target = targetRootsOrdered[i];

                            target.Translate(movement, Space.World);
                        }

                        SetPivotPointOffset(movement);
                    }
                }


                previousMousePosition = mousePosition;

                yield return null;
            }


            totalRotationAmount = Quaternion.identity;
            totalScaleAmount = 0;
            isTransforming = false;
            SetTranslatingAxis(transformType, Axis.None);

            SetPivotPoint();
        }


        Vector3 GetNearAxisDirection(out Vector3 otherAxis1, out Vector3 otherAxis2)
        {
            otherAxis1 = otherAxis2 = Vector3.zero;

            if (nearAxis != Axis.None)
            {
                if (nearAxis == Axis.X)
                {
                    otherAxis1 = axisInfo.yDirection;
                    otherAxis2 = axisInfo.zDirection;
                    return axisInfo.xDirection;
                }
                if (nearAxis == Axis.Y)
                {
                    otherAxis1 = axisInfo.xDirection;
                    otherAxis2 = axisInfo.zDirection;
                    return axisInfo.yDirection;
                }
                if (nearAxis == Axis.Z)
                {
                    otherAxis1 = axisInfo.xDirection;
                    otherAxis2 = axisInfo.yDirection;
                    return axisInfo.zDirection;
                }
                if (nearAxis == Axis.Any)
                {
                    return Vector3.one;
                }
            }

            return Vector3.zero;
        }


        public void AddTarget(Transform target)
        {
            if (target != null)
            {
                AddTargetRoot(target);
                SetPivotPoint();
            }
        }

        public void ClearTargets()
        {
            ClearAllHighlightedRenderers();

            targetRoots.Clear();
            targetRootsOrdered.Clear();
            children.Clear();
        }

        void ClearAndAddTarget(Transform target)
        {
            ClearTargets();
            AddTarget(target);
        }

        #region Highlight Renderers
        void AddTargetHighlightedRenderers(Transform target)
        {
            if (target != null)
            {
                GetTargetRenderers(target, renderersBuffer);

                for (int i = 0; i < renderersBuffer.Count; i++)
                {
                    Renderer render = renderersBuffer[i];

                    if (!highlightedRenderers.Contains(render))
                    {
                        materialsBuffer.Clear();
                        materialsBuffer.AddRange(render.sharedMaterials);

                        if (!materialsBuffer.Contains(outlineMaterial))
                        {
                            materialsBuffer.Add(outlineMaterial);
                            render.materials = materialsBuffer.ToArray();
                        }

                        highlightedRenderers.Add(render);
                    }
                }

                materialsBuffer.Clear();
            }
        }

        void GetTargetRenderers(Transform target, List<Renderer> renderers)
        {
            renderers.Clear();
            if (target != null)
            {
                target.GetComponentsInChildren<Renderer>(true, renderers);
            }
        }

        void ClearAllHighlightedRenderers()
        {
            foreach (KeyValuePair<Transform, TargetInfo> target in targetRoots)
            {
                RemoveTargetHighlightedRenderers(target.Key);
            }

            //In case any are still left, such as if they changed parents or what not when they were highlighted.
            renderersBuffer.Clear();
            renderersBuffer.AddRange(highlightedRenderers);
            RemoveHighlightedRenderers(renderersBuffer);
        }

        void RemoveTargetHighlightedRenderers(Transform target)
        {
            GetTargetRenderers(target, renderersBuffer);

            RemoveHighlightedRenderers(renderersBuffer);
        }

        void RemoveHighlightedRenderers(List<Renderer> renderers)
        {
            for (int i = 0; i < renderersBuffer.Count; i++)
            {
                Renderer render = renderersBuffer[i];
                if (render != null)
                {
                    materialsBuffer.Clear();
                    materialsBuffer.AddRange(render.sharedMaterials);

                    if (materialsBuffer.Contains(outlineMaterial))
                    {
                        materialsBuffer.Remove(outlineMaterial);
                        render.materials = materialsBuffer.ToArray();
                    }
                }

                highlightedRenderers.Remove(render);
            }

            renderersBuffer.Clear();
        }
        #endregion

        void AddTargetRoot(Transform targetRoot)
        {
            targetRoots.Add(targetRoot, new TargetInfo());
            targetRootsOrdered.Add(targetRoot);

            AddAllChildren(targetRoot);
        }
        void RemoveTargetRoot(Transform targetRoot)
        {
            if (targetRoots.Remove(targetRoot))
            {
                targetRootsOrdered.Remove(targetRoot);

                RemoveAllChildren(targetRoot);
            }
        }

        void AddAllChildren(Transform target)
        {
            childrenBuffer.Clear();
            target.GetComponentsInChildren<Transform>(true, childrenBuffer);
            childrenBuffer.Remove(target);

            for (int i = 0; i < childrenBuffer.Count; i++)
            {
                Transform child = childrenBuffer[i];
                children.Add(child);
                RemoveTargetRoot(child); //We do this in case we selected child first and then the parent.
            }

            childrenBuffer.Clear();
        }
        void RemoveAllChildren(Transform target)
        {
            childrenBuffer.Clear();
            target.GetComponentsInChildren<Transform>(true, childrenBuffer);
            childrenBuffer.Remove(target);

            for (int i = 0; i < childrenBuffer.Count; i++)
            {
                children.Remove(childrenBuffer[i]);
            }

            childrenBuffer.Clear();
        }

        public void SetPivotPoint()
        {
            if (mainTargetRoot != null)
            {
                if (pivot == TransformPivot.Pivot)
                {
                    pivotPoint = mainTargetRoot.position;
                }
                else if (pivot == TransformPivot.Center)
                {
                    totalCenterPivotPoint = Vector3.zero;

                    Dictionary<Transform, TargetInfo>.Enumerator targetsEnumerator = targetRoots.GetEnumerator(); //We avoid foreach to avoid garbage.
                    while (targetsEnumerator.MoveNext())
                    {
                        Transform target = targetsEnumerator.Current.Key;
                        TargetInfo info = targetsEnumerator.Current.Value;
                        info.centerPivotPoint = target.GetCenter(centerType);

                        totalCenterPivotPoint += info.centerPivotPoint;
                    }

                    totalCenterPivotPoint /= targetRoots.Count;

                    if (centerType == CenterType.Solo)
                    {
                        pivotPoint = targetRoots[mainTargetRoot].centerPivotPoint;
                    }
                    else if (centerType == CenterType.All)
                    {
                        pivotPoint = totalCenterPivotPoint;
                    }
                }
            }
        }
        void SetPivotPointOffset(Vector3 offset)
        {
            pivotPoint += offset;
            totalCenterPivotPoint += offset;
        }


        IEnumerator ForceUpdatePivotPointAtEndOfFrame()
        {
            while (this.enabled)
            {
                ForceUpdatePivotPointOnChange();
                yield return waitForEndOFFrame;
            }
        }

        void ForceUpdatePivotPointOnChange()
        {
            if (forceUpdatePivotPointOnChange)
            {
                if (mainTargetRoot != null && !isTransforming)
                {
                    bool hasSet = false;
                    Dictionary<Transform, TargetInfo>.Enumerator targets = targetRoots.GetEnumerator();
                    while (targets.MoveNext())
                    {
                        if (!hasSet)
                        {
                            if (targets.Current.Value.previousPosition != Vector3.zero && targets.Current.Key.position != targets.Current.Value.previousPosition)
                            {
                                SetPivotPoint();
                                hasSet = true;
                            }
                        }

                        targets.Current.Value.previousPosition = targets.Current.Key.position;
                    }
                }
            }
        }

        public void SetTranslatingAxis(TransformType type, Axis axis, Axis planeAxis = Axis.None)
        {
            this.translatingType = type;
            this.nearAxis = axis;
            this.planeAxis = planeAxis;
        }

        public AxisInfo GetAxisInfo()
        {
            AxisInfo currentAxisInfo = axisInfo;

            if (isTransforming && GetProperTransformSpace() == TransformSpace.Global && translatingType == TransformType.Rotate)
            {
                currentAxisInfo.xDirection = totalRotationAmount * Vector3.right;
                currentAxisInfo.yDirection = totalRotationAmount * Vector3.up;
                currentAxisInfo.zDirection = totalRotationAmount * Vector3.forward;
            }

            return currentAxisInfo;
        }

        void SetNearAxis()
        {
            if (isTransforming) return;

            SetTranslatingAxis(transformType, Axis.None);

            if (mainTargetRoot == null) return;

            float distanceMultiplier = GetDistanceMultiplier();
            float handleMinSelectedDistanceCheck = (this.minSelectedDistanceCheck + handleWidth) * distanceMultiplier;

            if (nearAxis == Axis.None && (TransformTypeContains(TransformType.Move) || TransformTypeContains(TransformType.Scale)))
            {
                //Important to check scale lines before move lines since in TransformType.All the move planes would block the scales center scale all gizmo.
                if (nearAxis == Axis.None && TransformTypeContains(TransformType.Scale))
                {
                    float tipMinSelectedDistanceCheck = (this.minSelectedDistanceCheck + boxSize) * distanceMultiplier;
                    HandleNearestPlanes(TransformType.Scale, handleSquares, tipMinSelectedDistanceCheck);
                }

                if (nearAxis == Axis.None && TransformTypeContains(TransformType.Move))
                {
                    //Important to check the planes first before the handle tip since it makes selecting the planes easier.
                    float planeMinSelectedDistanceCheck = (this.minSelectedDistanceCheck + planeSize) * distanceMultiplier;
                    HandleNearestPlanes(TransformType.Move, handlePlanes, planeMinSelectedDistanceCheck);

                    if (nearAxis != Axis.None)
                    {
                        planeAxis = nearAxis;
                    }
                    else
                    {
                        float tipMinSelectedDistanceCheck = (this.minSelectedDistanceCheck + triangleSize) * distanceMultiplier;
                        HandleNearestLines(TransformType.Move, handleTriangles, tipMinSelectedDistanceCheck);
                    }
                }

                if (nearAxis == Axis.None)
                {
                    //Since Move and Scale share the same handle line, we give Move the priority.
                    TransformType transType = transformType == TransformType.All ? TransformType.Move : transformType;
                    HandleNearestLines(transType, handleLines, handleMinSelectedDistanceCheck);
                }
            }

            if (nearAxis == Axis.None && TransformTypeContains(TransformType.Rotate))
            {
                HandleNearestLines(TransformType.Rotate, circlesLines, handleMinSelectedDistanceCheck);
            }
        }

        void HandleNearestLines(TransformType type, AxisVectors axisVectors, float minSelectedDistanceCheck)
        {
            float xClosestDistance = ClosestDistanceFromMouseToLines(axisVectors.x);
            float yClosestDistance = ClosestDistanceFromMouseToLines(axisVectors.y);
            float zClosestDistance = ClosestDistanceFromMouseToLines(axisVectors.z);
            float allClosestDistance = ClosestDistanceFromMouseToLines(axisVectors.all);

            HandleNearest(type, xClosestDistance, yClosestDistance, zClosestDistance, allClosestDistance, minSelectedDistanceCheck);
        }

        void HandleNearestPlanes(TransformType type, AxisVectors axisVectors, float minSelectedDistanceCheck)
        {
            float xClosestDistance = ClosestDistanceFromMouseToPlanes(axisVectors.x);
            float yClosestDistance = ClosestDistanceFromMouseToPlanes(axisVectors.y);
            float zClosestDistance = ClosestDistanceFromMouseToPlanes(axisVectors.z);
            float allClosestDistance = ClosestDistanceFromMouseToPlanes(axisVectors.all);

            HandleNearest(type, xClosestDistance, yClosestDistance, zClosestDistance, allClosestDistance, minSelectedDistanceCheck);
        }

        void HandleNearest(TransformType type, float xClosestDistance, float yClosestDistance, float zClosestDistance, float allClosestDistance, float minSelectedDistanceCheck)
        {
            if (type == TransformType.Scale && allClosestDistance <= minSelectedDistanceCheck) SetTranslatingAxis(type, Axis.Any);
            else if (xClosestDistance <= minSelectedDistanceCheck && xClosestDistance <= yClosestDistance && xClosestDistance <= zClosestDistance) SetTranslatingAxis(type, Axis.X);
            else if (yClosestDistance <= minSelectedDistanceCheck && yClosestDistance <= xClosestDistance && yClosestDistance <= zClosestDistance) SetTranslatingAxis(type, Axis.Y);
            else if (zClosestDistance <= minSelectedDistanceCheck && zClosestDistance <= xClosestDistance && zClosestDistance <= yClosestDistance) SetTranslatingAxis(type, Axis.Z);
            else if (type == TransformType.Rotate && mainTargetRoot != null)
            {
                Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);
                Vector3 mousePlaneHit = Geometry.LinePlaneIntersect(mouseRay.origin, mouseRay.direction, pivotPoint, (transform.position - pivotPoint).normalized);
                if ((pivotPoint - mousePlaneHit).sqrMagnitude <= (GetHandleLength(TransformType.Rotate)).Squared()) SetTranslatingAxis(type, Axis.Any);
            }
        }

        float ClosestDistanceFromMouseToLines(List<Vector3> lines)
        {
            Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);

            float closestDistance = float.MaxValue;
            for (int i = 0; i + 1 < lines.Count; i++)
            {
                IntersectPoints points = Geometry.ClosestPointsOnSegmentToLine(lines[i], lines[i + 1], mouseRay.origin, mouseRay.direction);
                float distance = Vector3.Distance(points.first, points.second);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                }
            }
            return closestDistance;
        }

        float ClosestDistanceFromMouseToPlanes(List<Vector3> planePoints)
        {
            float closestDistance = float.MaxValue;

            if (planePoints.Count >= 4)
            {
                Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);

                for (int i = 0; i < planePoints.Count; i += 4)
                {
                    Plane plane = new Plane(planePoints[i], planePoints[i + 1], planePoints[i + 2]);

                    if (plane.Raycast(mouseRay, out float distanceToPlane))
                    {
                        Vector3 pointOnPlane = mouseRay.origin + (mouseRay.direction * distanceToPlane);
                        Vector3 planeCenter = (planePoints[0] + planePoints[1] + planePoints[2] + planePoints[3]) / 4f;

                        float distance = Vector3.Distance(planeCenter, pointOnPlane);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                        }
                    }
                }
            }

            return closestDistance;
        }

        //float DistanceFromMouseToPlane(List<Vector3> planeLines)
        //{
        //	if(planeLines.Count >= 4)
        //	{
        //		Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);
        //		Plane plane = new Plane(planeLines[0], planeLines[1], planeLines[2]);

        //		float distanceToPlane;
        //		if(plane.Raycast(mouseRay, out distanceToPlane))
        //		{
        //			Vector3 pointOnPlane = mouseRay.origin + (mouseRay.direction * distanceToPlane);
        //			Vector3 planeCenter = (planeLines[0] + planeLines[1] + planeLines[2] + planeLines[3]) / 4f;

        //			return Vector3.Distance(planeCenter, pointOnPlane);
        //		}
        //	}

        //	return float.MaxValue;
        //}

        void SetAxisInfo()
        {
            if (mainTargetRoot != null)
            {
                axisInfo.Set(mainTargetRoot, pivotPoint, GetProperTransformSpace());
            }
        }

        //This helps keep the size consistent no matter how far we are from it.
        public float GetDistanceMultiplier()
        {
            if (mainTargetRoot == null) return 0f;

            if (myCamera.orthographic) return Mathf.Max(.01f, myCamera.orthographicSize * 2f);
            return Mathf.Max(.01f, Mathf.Abs(ExtVector3.MagnitudeInDirection(pivotPoint - transform.position, myCamera.transform.forward)));
        }

        void SetLines()
        {
            SetHandleLines();
            SetHandlePlanes();
            SetHandleTriangles();
            SetHandleSquares();
            SetCircles(GetAxisInfo(), circlesLines);
        }

        void SetHandleLines()
        {
            handleLines.Clear();

            if (TranslatingTypeContains(TransformType.Move) || TranslatingTypeContains(TransformType.Scale))
            {
                float lineWidth = handleWidth * GetDistanceMultiplier();

                float xLineLength = 0;
                float yLineLength = 0;
                float zLineLength = 0;
                if (TranslatingTypeContains(TransformType.Move))
                {
                    xLineLength = yLineLength = zLineLength = GetHandleLength(TransformType.Move);
                }
                else if (TranslatingTypeContains(TransformType.Scale))
                {
                    xLineLength = GetHandleLength(TransformType.Scale, Axis.X);
                    yLineLength = GetHandleLength(TransformType.Scale, Axis.Y);
                    zLineLength = GetHandleLength(TransformType.Scale, Axis.Z);
                }

                AddQuads(pivotPoint, axisInfo.xDirection, axisInfo.yDirection, axisInfo.zDirection, xLineLength, lineWidth, handleLines.x);
                AddQuads(pivotPoint, axisInfo.yDirection, axisInfo.xDirection, axisInfo.zDirection, yLineLength, lineWidth, handleLines.y);
                AddQuads(pivotPoint, axisInfo.zDirection, axisInfo.xDirection, axisInfo.yDirection, zLineLength, lineWidth, handleLines.z);
            }
        }
        int AxisDirectionMultiplier(Vector3 direction, Vector3 otherDirection)
        {
            return ExtVector3.IsInDirection(direction, otherDirection) ? 1 : -1;
        }

        void SetHandlePlanes()
        {
            handlePlanes.Clear();

            if (TranslatingTypeContains(TransformType.Move))
            {
                Vector3 pivotToCamera = myCamera.transform.position - pivotPoint;
                float cameraXSign = Mathf.Sign(Vector3.Dot(axisInfo.xDirection, pivotToCamera));
                float cameraYSign = Mathf.Sign(Vector3.Dot(axisInfo.yDirection, pivotToCamera));
                float cameraZSign = Mathf.Sign(Vector3.Dot(axisInfo.zDirection, pivotToCamera));

                float planeSize = this.planeSize;
                if (transformType == TransformType.All) { planeSize *= allMoveHandleLengthMultiplier; }
                planeSize *= GetDistanceMultiplier();

                Vector3 xDirection = (axisInfo.xDirection * planeSize) * cameraXSign;
                Vector3 yDirection = (axisInfo.yDirection * planeSize) * cameraYSign;
                Vector3 zDirection = (axisInfo.zDirection * planeSize) * cameraZSign;

                Vector3 xPlaneCenter = pivotPoint + (yDirection + zDirection);
                Vector3 yPlaneCenter = pivotPoint + (xDirection + zDirection);
                Vector3 zPlaneCenter = pivotPoint + (xDirection + yDirection);

                AddQuad(xPlaneCenter, axisInfo.yDirection, axisInfo.zDirection, planeSize, handlePlanes.x);
                AddQuad(yPlaneCenter, axisInfo.xDirection, axisInfo.zDirection, planeSize, handlePlanes.y);
                AddQuad(zPlaneCenter, axisInfo.xDirection, axisInfo.yDirection, planeSize, handlePlanes.z);
            }
        }

        void SetHandleTriangles()
        {
            handleTriangles.Clear();

            if (TranslatingTypeContains(TransformType.Move))
            {
                float triangleLength = triangleSize * GetDistanceMultiplier();
                AddTriangles(axisInfo.GetXAxisEnd(GetHandleLength(TransformType.Move)), axisInfo.xDirection, axisInfo.yDirection, axisInfo.zDirection, triangleLength, handleTriangles.x);
                AddTriangles(axisInfo.GetYAxisEnd(GetHandleLength(TransformType.Move)), axisInfo.yDirection, axisInfo.xDirection, axisInfo.zDirection, triangleLength, handleTriangles.y);
                AddTriangles(axisInfo.GetZAxisEnd(GetHandleLength(TransformType.Move)), axisInfo.zDirection, axisInfo.yDirection, axisInfo.xDirection, triangleLength, handleTriangles.z);
            }
        }

        void AddTriangles(Vector3 axisEnd, Vector3 axisDirection, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float size, List<Vector3> resultsBuffer)
        {
            Vector3 endPoint = axisEnd + (axisDirection * (size * 2f));
            Square baseSquare = GetBaseSquare(axisEnd, axisOtherDirection1, axisOtherDirection2, size / 2f);

            resultsBuffer.Add(baseSquare.bottomLeft);
            resultsBuffer.Add(baseSquare.topLeft);
            resultsBuffer.Add(baseSquare.topRight);
            resultsBuffer.Add(baseSquare.topLeft);
            resultsBuffer.Add(baseSquare.bottomRight);
            resultsBuffer.Add(baseSquare.topRight);

            for (int i = 0; i < 4; i++)
            {
                resultsBuffer.Add(baseSquare[i]);
                resultsBuffer.Add(baseSquare[i + 1]);
                resultsBuffer.Add(endPoint);
            }
        }

        void SetHandleSquares()
        {
            handleSquares.Clear();

            if (TranslatingTypeContains(TransformType.Scale))
            {
                float boxSize = this.boxSize * GetDistanceMultiplier();
                AddSquares(axisInfo.GetXAxisEnd(GetHandleLength(TransformType.Scale, Axis.X)), axisInfo.xDirection, axisInfo.yDirection, axisInfo.zDirection, boxSize, handleSquares.x);
                AddSquares(axisInfo.GetYAxisEnd(GetHandleLength(TransformType.Scale, Axis.Y)), axisInfo.yDirection, axisInfo.xDirection, axisInfo.zDirection, boxSize, handleSquares.y);
                AddSquares(axisInfo.GetZAxisEnd(GetHandleLength(TransformType.Scale, Axis.Z)), axisInfo.zDirection, axisInfo.xDirection, axisInfo.yDirection, boxSize, handleSquares.z);
                AddSquares(pivotPoint - (axisInfo.xDirection * (boxSize * .5f)), axisInfo.xDirection, axisInfo.yDirection, axisInfo.zDirection, boxSize, handleSquares.all);
            }
        }

        void AddSquares(Vector3 axisStart, Vector3 axisDirection, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float size, List<Vector3> resultsBuffer)
        {
            AddQuads(axisStart, axisDirection, axisOtherDirection1, axisOtherDirection2, size, size * .5f, resultsBuffer);
        }
        void AddQuads(Vector3 axisStart, Vector3 axisDirection, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float length, float width, List<Vector3> resultsBuffer)
        {
            Vector3 axisEnd = axisStart + (axisDirection * length);
            AddQuads(axisStart, axisEnd, axisOtherDirection1, axisOtherDirection2, width, resultsBuffer);
        }
        void AddQuads(Vector3 axisStart, Vector3 axisEnd, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float width, List<Vector3> resultsBuffer)
        {
            Square baseRectangle = GetBaseSquare(axisStart, axisOtherDirection1, axisOtherDirection2, width);
            Square baseRectangleEnd = GetBaseSquare(axisEnd, axisOtherDirection1, axisOtherDirection2, width);

            resultsBuffer.Add(baseRectangle.bottomLeft);
            resultsBuffer.Add(baseRectangle.topLeft);
            resultsBuffer.Add(baseRectangle.topRight);
            resultsBuffer.Add(baseRectangle.bottomRight);

            resultsBuffer.Add(baseRectangleEnd.bottomLeft);
            resultsBuffer.Add(baseRectangleEnd.topLeft);
            resultsBuffer.Add(baseRectangleEnd.topRight);
            resultsBuffer.Add(baseRectangleEnd.bottomRight);

            for (int i = 0; i < 4; i++)
            {
                resultsBuffer.Add(baseRectangle[i]);
                resultsBuffer.Add(baseRectangleEnd[i]);
                resultsBuffer.Add(baseRectangleEnd[i + 1]);
                resultsBuffer.Add(baseRectangle[i + 1]);
            }
        }

        void AddQuad(Vector3 axisStart, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float width, List<Vector3> resultsBuffer)
        {
            Square baseRectangle = GetBaseSquare(axisStart, axisOtherDirection1, axisOtherDirection2, width);

            resultsBuffer.Add(baseRectangle.bottomLeft);
            resultsBuffer.Add(baseRectangle.topLeft);
            resultsBuffer.Add(baseRectangle.topRight);
            resultsBuffer.Add(baseRectangle.bottomRight);
        }

        Square GetBaseSquare(Vector3 axisEnd, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float size)
        {
            Square square;
            Vector3 offsetUp = ((axisOtherDirection1 * size) + (axisOtherDirection2 * size));
            Vector3 offsetDown = ((axisOtherDirection1 * size) - (axisOtherDirection2 * size));
            //These might not really be the proper directions, as in the bottomLeft might not really be at the bottom left...
            square.bottomLeft = axisEnd + offsetDown;
            square.topLeft = axisEnd + offsetUp;
            square.bottomRight = axisEnd - offsetUp;
            square.topRight = axisEnd - offsetDown;
            return square;
        }

        void SetCircles(AxisInfo axisInfo, AxisVectors axisVectors)
        {
            axisVectors.Clear();

            if (TranslatingTypeContains(TransformType.Rotate))
            {
                float circleLength = GetHandleLength(TransformType.Rotate);
                AddCircle(pivotPoint, axisInfo.xDirection, circleLength, axisVectors.x);
                AddCircle(pivotPoint, axisInfo.yDirection, circleLength, axisVectors.y);
                AddCircle(pivotPoint, axisInfo.zDirection, circleLength, axisVectors.z);
                AddCircle(pivotPoint, (pivotPoint - transform.position).normalized, circleLength, axisVectors.all, false);
            }
        }

        void AddCircle(Vector3 origin, Vector3 axisDirection, float size, List<Vector3> resultsBuffer, bool depthTest = true)
        {
            Vector3 up = axisDirection.normalized * size;
            Vector3 forward = Vector3.Slerp(up, -up, .5f);
            Vector3 right = Vector3.Cross(up, forward).normalized * size;

            Matrix4x4 matrix = new Matrix4x4();

            matrix[0] = right.x;
            matrix[1] = right.y;
            matrix[2] = right.z;

            matrix[4] = up.x;
            matrix[5] = up.y;
            matrix[6] = up.z;

            matrix[8] = forward.x;
            matrix[9] = forward.y;
            matrix[10] = forward.z;

            Vector3 lastPoint = origin + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
            Vector3 nextPoint = Vector3.zero;
            float multiplier = 360f / circleDetail;

            Plane plane = new Plane((transform.position - pivotPoint).normalized, pivotPoint);

            float circleHandleWidth = handleWidth * GetDistanceMultiplier();

            for (int i = 0; i < circleDetail + 1; i++)
            {
                nextPoint.x = Mathf.Cos((i * multiplier) * Mathf.Deg2Rad);
                nextPoint.z = Mathf.Sin((i * multiplier) * Mathf.Deg2Rad);
                nextPoint.y = 0;

                nextPoint = origin + matrix.MultiplyPoint3x4(nextPoint);

                if (!depthTest || plane.GetSide(lastPoint))
                {
                    Vector3 centerPoint = (lastPoint + nextPoint) * .5f;
                    Vector3 upDirection = (centerPoint - origin).normalized;
                    AddQuads(lastPoint, nextPoint, upDirection, axisDirection, circleHandleWidth, resultsBuffer);
                }

                lastPoint = nextPoint;
            }
        }

        void DrawLines(List<Vector3> lines, Color color)
        {
            if (lines.Count == 0) return;

            GL.Begin(GL.LINES);
            GL.Color(color);

            for (int i = 0; i < lines.Count; i += 2)
            {
                GL.Vertex(lines[i]);
                GL.Vertex(lines[i + 1]);
            }

            GL.End();
        }

        void DrawTriangles(List<Vector3> lines, Color color)
        {
            if (lines.Count == 0) return;

            GL.Begin(GL.TRIANGLES);
            GL.Color(color);

            for (int i = 0; i < lines.Count; i += 3)
            {
                GL.Vertex(lines[i]);
                GL.Vertex(lines[i + 1]);
                GL.Vertex(lines[i + 2]);
            }

            GL.End();
        }

        void DrawQuads(List<Vector3> lines, Color color)
        {
            if (lines.Count == 0) return;

            GL.Begin(GL.QUADS);
            GL.Color(color);

            for (int i = 0; i < lines.Count; i += 4)
            {
                GL.Vertex(lines[i]);
                GL.Vertex(lines[i + 1]);
                GL.Vertex(lines[i + 2]);
                GL.Vertex(lines[i + 3]);
            }

            GL.End();
        }

        void DrawFilledCircle(List<Vector3> lines, Color color)
        {
            if (lines.Count == 0) return;

            Vector3 center = Vector3.zero;
            for (int i = 0; i < lines.Count; i++)
            {
                center += lines[i];
            }
            center /= lines.Count;

            GL.Begin(GL.TRIANGLES);
            GL.Color(color);

            for (int i = 0; i + 1 < lines.Count; i++)
            {
                GL.Vertex(lines[i]);
                GL.Vertex(lines[i + 1]);
                GL.Vertex(center);
            }

            GL.End();
        }

        void SetMaterial()
        {
            if (lineMaterial == null)
            {
                lineMaterial = new Material(Shader.Find("Custom/Lines"));
                outlineMaterial = new Material(Shader.Find("Custom/Outline"));
            }
        }
    }
}
