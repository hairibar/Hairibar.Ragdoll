using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

#pragma warning disable 649
namespace Hairibar.Ragdoll.Editor
{
    [EditorTool("Physics Drag")]
    internal class PhysicsDragTool : EditorTool
    {
        #region Constants
        const string PREF_LAYER_MASK_KEY = "Hairibar.Ragdoll.PhysicsDrag.LayerMask";

        const int PANEL_WIDTH = 100;
        const float GRAB_VISUALIZATION_SCALE = 0.3f;
        #endregion

        string FullLayerMaskPrefKey => $"{PlayerSettings.companyName}.{PlayerSettings.productName}.{PREF_LAYER_MASK_KEY}";

        #region Configuration
        public LayerMask layerMask;
        SerializedProperty layerMask_Prop;

        Rigidbody grabberRigidbody;
        FixedJoint fixedJoint;

        void OnEnable()
        {
            LayerMask savedMask = EditorPrefs.GetInt(FullLayerMaskPrefKey, 0);

            SerializedObject serializedObject = new SerializedObject(this);
            layerMask_Prop = serializedObject.FindProperty("layerMask");
            layerMask_Prop.intValue = savedMask;

            CreateConstraint();
        }

        void OnDisable()
        {
            DestroyConstraint();

            EditorPrefs.SetInt(FullLayerMaskPrefKey, layerMask_Prop.intValue);
        }

        void DrawConfiguration()
        {
            Handles.BeginGUI();
            GUILayout.BeginVertical(EditorStyles.textArea, GUILayout.MaxWidth(PANEL_WIDTH));

            GUILayout.Label("Layer Mask", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(layerMask_Prop, GUIContent.none);
            layerMask_Prop.serializedObject.ApplyModifiedProperties();

            GUILayout.EndVertical();
            Handles.EndGUI();
        }
        #endregion

        GrabInfo currentDrag;

        #region Tool Stuff
        [Shortcut("Physics Drag Tool", KeyCode.D)]
        public static void ActivateToolShortcut()
        {
            EditorTools.SetActiveTool(typeof(PhysicsDragTool));
        }

        public override GUIContent toolbarIcon => EditorGUIUtility.IconContent("ViewToolMove", "|PhysicsDrop Tool");

        public override void OnToolGUI(EditorWindow window)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            DrawConfiguration();

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);


            switch (e.type)
            {
                case EventType.MouseDown:
                    GrabBody(sceneView.camera, e);
                    e.Use();
                    break;
                case EventType.MouseDrag:
                    if (currentDrag)
                    {
                        DragBody(sceneView.camera, e);
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                case EventType.MouseLeaveWindow:
                    if (currentDrag)
                    {
                        LetGoOfBody();
                        e.Use();
                    }
                    break;
                case EventType.Repaint:
                    if (currentDrag) DrawGrabVisualization();
                    break;
            }
        }

        public override bool IsAvailable()
        {
            return Application.isPlaying;
        }
        #endregion

        #region Drag Operations
        void GrabBody(Camera camera, Event e)
        {
            //Do a raycast
            Ray ray = GetRay(camera, e);
            if (!Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, layerMask)) return;

            //Look for a rigidbody in whatever we hit
            Rigidbody hitBody = hit.collider.attachedRigidbody;
            if (!hitBody) return;

            //Create the drag plane
            Vector3 normal = camera.transform.forward;
            normal.y = 0;
            currentDrag.screenPlane = new Plane(normal, hit.point);

            //Remeber the drag
            currentDrag.body = hitBody;
            grabberRigidbody.position = hit.point;
        }

        void DragBody(Camera camera, Event e)
        {
            //Do a raycast against the screen plane
            Ray ray = GetRay(camera, e);
            currentDrag.screenPlane.Raycast(ray, out float distance);

            fixedJoint.connectedBody = currentDrag.body;

            //Move the body
            Vector3 newPosition = ray.origin + ray.direction * distance;
            grabberRigidbody.position = newPosition;
        }

        void LetGoOfBody()
        {
            fixedJoint.connectedBody = null;

            currentDrag = new GrabInfo();
        }

        Ray GetRay(Camera camera, Event e)
        {
            //SceneView coordinates have (0, 0) at top left, Camera.ScreenPointToRay expects it at bottom left.
            Vector2 mousePos = e.mousePosition;
            mousePos.y = camera.pixelHeight - mousePos.y;

            return camera.ScreenPointToRay(mousePos);
        }

        void DrawGrabVisualization()
        {
            float size = HandleUtility.GetHandleSize(grabberRigidbody.position) * GRAB_VISUALIZATION_SCALE;

            Quaternion rotation = Quaternion.LookRotation(currentDrag.screenPlane.normal);
            Handles.DrawSelectionFrame(0, grabberRigidbody.position, rotation, size, EventType.Repaint);
        }
        #endregion

        #region Constraint Lifetime
        void CreateConstraint()
        {
            GameObject go = new GameObject
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            grabberRigidbody = go.AddComponent<Rigidbody>();
            grabberRigidbody.isKinematic = true;

            fixedJoint = go.AddComponent<FixedJoint>();
            fixedJoint.autoConfigureConnectedAnchor = true;
        }

        void DestroyConstraint()
        {
            if (fixedJoint) DestroyImmediate(fixedJoint.gameObject);
            else if (grabberRigidbody) DestroyImmediate(grabberRigidbody.gameObject);
        }
        #endregion

        struct GrabInfo
        {
            public Rigidbody body;
            public Plane screenPlane;

            public static implicit operator bool(GrabInfo grabInfo)
            {
                return grabInfo.body;
            }
        }
    }
}