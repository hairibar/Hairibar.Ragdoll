using Hairibar.NaughtyExtensions.Editor;
using UnityEditor;
using UnityEngine;

namespace Hairibar.Ragdoll.Editor
{
    [CustomEditor(typeof(RagdollSettings))]
    internal class RagdollSettingsEditor : UnityEditor.Editor
    {
        SerializedProperty bindingsDefinition;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            bindingsDefinition.serializedObject.Update();

            ExtraNaughtyEditorGUILayout.Header("Ragdoll Profile");
            DrawProfileField(serializedObject.FindProperty("_powerProfile"));
            DrawProfileField(serializedObject.FindProperty("_weightDistribution"));

            ExtraNaughtyEditorGUILayout.Header("Limit Parameters");
            DrawLimitProperties();

            ExtraNaughtyEditorGUILayout.Header("Joint Processing");
            DrawJointProcessingProperties();

            ExtraNaughtyEditorGUILayout.Header("Rigidbody Settings");
            DrawRigidbodySettings();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawProfileField(SerializedProperty property)
        {
            UsePropertySetterDrawer.Draw_Layout(property);
            RagdollProfileEditorUtility.ValidateProfileField_Layout(property, bindingsDefinition.objectReferenceValue as RagdollDefinition, true);
        }

        void DrawLimitProperties()
        {
            EditorGUILayout.Slider(serializedObject.FindProperty("limitBounciness"), 0, 1);

            EditorGUILayout.Slider(serializedObject.FindProperty("limitContactDistanceFactor"), 0, 1,
                new GUIContent("Limit Contact Distance Factor", "How far the joint can \"see\" the limit, expressed as a factor of the total distance of the limit." +
                "1 means everywhere. 0.5 means halfwat through the joint. 0 lets PhysX pick the distance. \n" +
                "Increase this value to decrease jittering, at the cost of performance.")
                );

            ClampedFloatDrawer.Draw_Layout(serializedObject.FindProperty("limitSpring"),
                new GUIContent("Limit Spring",
                "The strength of the springs that enforce the joint limits. The value for each specific Joint is scaled by the mass of the attached Rigidbody."),
                0, Mathf.Infinity);

            ClampedFloatDrawer.Draw_Layout(serializedObject.FindProperty("limitSpringDamping"),
                new GUIContent("Limit Spring Damping",
                "The damping of the springs that enforce the joint limits. The value for each specific Joint is scaled by the mass of the attached Rigidbody."),
                0, Mathf.Infinity);
        }

        void DrawJointProcessingProperties()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableJointPreProcessing"),
                new GUIContent("Enable Joint Preprocessing",
                "Preprocessing is useful for fixing strange behaviour resulting from frozen rotation axes."));

            SerializedProperty enableJointProjection = serializedObject.FindProperty("enableJointProjection");
            EditorGUILayout.PropertyField(enableJointProjection,
                new GUIContent("Enable Joint Projection",
                "Projection \"cheats\" by bringing joints back in place even when the constraints are violated. " +
                "It is not a physical process, so it is best not to use it unless constraint violation is an issue."));

            if (enableJointProjection.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("minJointProjectionDistance"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("minJointProjectionAngle"));
            }
        }

        void DrawRigidbodySettings()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useGravity"));
            ClampedFloatDrawer.Draw_Layout(serializedObject.FindProperty("totalMass"),
                new GUIContent("Total Mass", "The total mass of the Rigidbody. It will be distributed according to the assigned WeightDistribution."),
                0, Mathf.Infinity);
            ClampedFloatDrawer.Draw_Layout(serializedObject.FindProperty("drag"), 0, Mathf.Infinity);
            ClampedFloatDrawer.Draw_Layout(serializedObject.FindProperty("angularDrag"), 0, Mathf.Infinity);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("material"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("interpolation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("collisionDetectionMode"));

            EditorGUILayout.IntSlider(serializedObject.FindProperty("solverIterations"), 6, 40,
                new GUIContent("Joint Solver Iterations",
                "The higher the iterations, the more accurate the Joint behaviour will be, at the cost of lower performance."));
        }


        void OnEnable()
        {
            SerializedObject bindings = new SerializedObject((serializedObject.targetObject as RagdollSettings).GetComponent<RagdollDefinitionBindings>());
            bindingsDefinition = bindings.FindProperty("_definition");
        }
    }
}