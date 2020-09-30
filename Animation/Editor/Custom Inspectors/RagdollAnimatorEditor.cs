using Hairibar.EngineExtensions.Editor;
using Hairibar.Ragdoll.Editor;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

#pragma warning disable 649
namespace Hairibar.Ragdoll.Animation.Editor
{
    [CustomEditor(typeof(RagdollAnimator))]
    internal class RagdollAnimatorEditor : UnityEditor.Editor
    {
        const string EDITOR_STATE_DIRECTORY = "Temp/Packages/com.hairibar.ragdoll/EditorState/RagdollAnimator/";

        [SerializeField] bool hideMesh;

        bool isInitialized;

        SerializedProperty bindingDefinition;
        Animator animator;

        #region Global Shortcut
        [ClutchShortcut("Hairibar.Ragdoll.RagdollAnimator/ForceTargetPose", KeyCode.P, ShortcutModifiers.Action, displayName = "Force Target Pose")]
        public static void ForceAnimatedPose(ShortcutArguments args)
        {
            if (!Application.isPlaying) return;

            if (args.stage == ShortcutStage.Begin)
            {
                SetForceAnimatedPoseGlobally(true);
            }
            else if (args.stage == ShortcutStage.End)
            {
                SetForceAnimatedPoseGlobally(false);
            }


            void SetForceAnimatedPoseGlobally(bool value)
            {
                foreach (RagdollAnimator ragdollAnimator in FindObjectsOfType<RagdollAnimator>())
                {
                    ragdollAnimator.forceTargetPose = value;
                }
            }
        }
        #endregion

        public override void OnInspectorGUI()
        {
            bindingDefinition?.serializedObject.Update();

            DrawBindingsField();

            NaughtyEditorGUI.DrawHeader("Animation Parameters");
            DrawProfileField();
            ClampedFloatDrawer.Draw_Layout(serializedObject.FindProperty("_profileTransitionLength"),
                new GUIContent("Profile Transition Length",
                "When changing profile, a blend transition will be done. This is the length of that transition."),
                0, Mathf.Infinity);

            NaughtyEditorGUI.DrawHeader("Master controls");
            NonLinearSliderDrawer.Draw_Layout(serializedObject.FindProperty("_masterAlpha"), 0, 1, NonLinearSliderDrawer.Function.Quadratic(2),
                new GUIContent("Master Alpha", "The profile's alpha values will be multiplied by this amount. \n" +
                "Alpha defines the stiffness with which the ragdoll matches the animation. " +
                "High values will instantly get to the target pose, while low values will treat the target pose more like a suggestion."));

            EditorGUILayout.Slider(serializedObject.FindProperty("_masterDampingRatio"), 0, 1,
                new GUIContent("Master Damping Ratio", "The profile's damping ratio values will be multiplied by this amount. \n" +
                "A damping ratio of 1 will get to the target pose perfectly, with no overshooting. " +
                "Lower values will overshoot the target pose."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("forceTargetPose"));
            NaughtyEditorGUI.DrawHeader("Debug Features (Editor Only)");
            if (serializedObject.isEditingMultipleObjects)
            {
                NaughtyEditorGUI.HelpBox_Layout("Multiple object editing isn't supported for debug features.", MessageType.Info, logToConsole: false);
            }
            else
            {
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    DoHideMeshField();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        #region Field Drawers
        void DrawBindingsField()
        {
            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            EditorGUI.BeginChangeCheck();

            SerializedProperty bindings = serializedObject.FindProperty("_ragdollBindings");
            EditorGUILayout.PropertyField(bindings);

            bool bindingChanged = EditorGUI.EndChangeCheck();

            if (!bindings.objectReferenceValue)
            {
                bindingDefinition = null;
                NaughtyEditorGUI.HelpBox_Layout("A RagdollDefinitionBindings must be assigned to the RagdollAnimator.", MessageType.Error);
            }
            else if (bindingChanged || bindingDefinition == null)
            {
                bindingDefinition = new SerializedObject((bindings.objectReferenceValue as RagdollDefinitionBindings)).FindProperty("_definition");
            }

            EditorGUI.EndDisabledGroup();
        }

        void DrawProfileField()
        {
            SerializedProperty profile = serializedObject.FindProperty("currentProfile");
            UsePropertySetterDrawer.Draw_Layout(profile);
            RagdollProfileEditorUtility.ValidateProfileField_Layout(profile, bindingDefinition?.objectReferenceValue as RagdollDefinition, true);
        }


        void DoHideMeshField()
        {
            EditorGUI.BeginChangeCheck();
            hideMesh = EditorGUILayout.Toggle("Hide Mesh", hideMesh);
            if (EditorGUI.EndChangeCheck())
            {
                RefreshHideMesh();
            }

            RefreshAnimatorCulling();
        }
        #endregion

        #region Value Appliers
        void RefreshHideMesh()
        {
            foreach (Renderer renderer in (target as RagdollAnimator).GetComponentsInChildren<Renderer>(true))
            {
                renderer.forceRenderingOff = hideMesh;
            }
        }

        void RefreshAnimatorCulling()
        {
            if (hideMesh && Application.isPlaying && animator) animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }
        #endregion

        #region Lifetime
        void OnEnable()
        {
            if (isInitialized) return;

            animator = (target as RagdollAnimator).GetComponent<Animator>();

            EditorSerializationUtility.Deserialize(EDITOR_STATE_DIRECTORY, this, target);
            RefreshHideMesh();

            isInitialized = true;
        }

        void OnDisable()
        {
            if (!isInitialized) return;

            EditorSerializationUtility.Serialize(EDITOR_STATE_DIRECTORY, this, target);
            isInitialized = false;
        }
        #endregion
    }
}
