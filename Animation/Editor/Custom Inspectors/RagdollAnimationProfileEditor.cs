using System.Collections.Generic;
using System.Reflection;
using Hairibar.EngineExtensions.Editor;
using Hairibar.Ragdoll.Editor;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Hairibar.Ragdoll.Animation.Editor
{
    [CustomEditor(typeof(RagdollAnimationProfile)), CanEditMultipleObjects]
    public class RagdollAnimationProfileEditor : RagdollProfileEditor
    {
        const float MAX_STABLE_ALPHA_PLUS_DAMPING = 1.6f;

        #region Private State
        ReorderableList positionMatchingOverridesList;
        ReorderableList rotationMatchingOverridesList;
        MethodInfo rebuildBoneProfilesMethod;

        HashSet<string> bonesWithOverride;
        HashSet<string> bonesWithNonZeroPositionMatchingOverride;

        bool mustRebuildOverrides;
        #endregion

        protected override bool RequiresDefinition => false;

        #region Inspector Drawing
        protected override void DrawInspector()
        {
            mustRebuildOverrides = false;

            DrawDefinitionField();
            if (HasDefinition)
            {
                BoneProfileOverrideDrawer.SetCurrentRagdollDefinition(Definition);
            }
            else
            {
                NaughtyEditorGUI.HelpBox_Layout("You can assign a definition to be able to override individual bones.", MessageType.Info);
            }

            NaughtyEditorGUI.DrawHeader("Position Matching");
            NonLinearSliderDrawer.Draw_Layout(serializedObject.FindProperty("globalPositionAlpha"), 0, 1, NonLinearSliderDrawer.Function.Quadratic(2), 
                new GUIContent("Global Alpha", 
                "Alpha defines the stiffness with which the ragdoll matches the animation. " +
                "High values will instantly get to the target position, while low values will treat the target position more like a suggestion."));
            EditorGUILayout.Slider(serializedObject.FindProperty("globalPositionDampingRatio"), 0, 1, 
                new GUIContent("Global Position Damping Ratio", "A damping ratio of 1 will get to the target position perfectly, with no overshooting. " +
                "Lower values will overshoot the target position."));

            EditorGUI.BeginChangeCheck();
            ClampedFloatDrawer.Draw_Layout(serializedObject.FindProperty("globalMaxLinearAcceleration"), 0, Mathf.Infinity);
            if (EditorGUI.EndChangeCheck()) mustRebuildOverrides = true;

            EditorGUILayout.Space();

            if (HasValidDefinition)
            {
                DrawOverrideList(positionMatchingOverridesList);
            }
            WarnAgainstUnstablePositionMatchingParameters();


            NaughtyEditorGUI.DrawHeader("Rotation Matching");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("matchRootRotation"));
            NonLinearSliderDrawer.Draw_Layout(serializedObject.FindProperty("globalRotationAlpha"), 0, 1, NonLinearSliderDrawer.Function.Quadratic(2),
                new GUIContent("Global Rotation Alpha",
                "Alpha defines the stiffness with which the ragdoll matches the animation. " +
                "High values will instantly get to the target rotation, while low values will treat the target rotation more like a suggestion."));

            EditorGUILayout.Slider(serializedObject.FindProperty("globalRotationDampingRatio"), 0, 1,
                new GUIContent("Global Rotation Damping Ratio", "A damping ratio of 1 will get to the target rotation perfectly, with no overshooting. " +
                "Lower values will overshoot the target rotation."));

            EditorGUI.BeginChangeCheck();
            ClampedFloatDrawer.Draw_Layout(serializedObject.FindProperty("globalMaxAngularAcceleration"), 0, Mathf.Infinity);
            if (EditorGUI.EndChangeCheck()) mustRebuildOverrides = true;

            EditorGUILayout.Space();

            if (HasValidDefinition)
            {
                DrawOverrideList(rotationMatchingOverridesList);
            }
            WarnAgainstPositionWithoutRotation();

            BoneProfileOverrideDrawer.SetCurrentRagdollDefinition(null);
        }

        void DrawOverrideList(ReorderableList list)
        {
            EditorGUI.BeginChangeCheck();

            if (serializedObject.isEditingMultipleObjects)
            {
                NaughtyEditorGUI.HelpBox_Layout("Override editing is not supported when editing multiple profiles.", MessageType.Info, logToConsole: false);
            }
            else
            {
                list.DoLayoutList();
            }

            if (EditorGUI.EndChangeCheck()) mustRebuildOverrides = true;

            ValidateNoDuplicateOverrides(list);
        }
        #endregion

        #region Operations
        protected override void AfterAppliedProperties()
        {
            if (CanRebuildBones && mustRebuildOverrides) RebuildBoneProfiles();
        }

        bool CanRebuildBones => IsValid && (!HasDefinition || HasValidDefinition);

        protected override void OnDefinitionChanged()
        {
            ClearOverrides();
        }

        void RebuildBoneProfiles()
        {
            if (Application.isPlaying) rebuildBoneProfilesMethod.Invoke(target, null);
        }

        void ClearOverrides()
        {
            positionMatchingOverridesList.serializedProperty.ClearArray();
            rotationMatchingOverridesList.serializedProperty.ClearArray();
        }
        #endregion

        #region Validation
        static bool PositionMatchingParametersAreStable(float alpha, float dampingRatio)
        {
            return alpha + dampingRatio <= MAX_STABLE_ALPHA_PLUS_DAMPING;
        }

        void ValidateNoDuplicateOverrides(ReorderableList list)
        {
            bool isValid = true;
            bonesWithOverride.Clear();

            for (int i = 0; i < list.serializedProperty.arraySize; i++)
            {
                string bone = list.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("bone.name").stringValue;

                if (bonesWithOverride.Contains(bone))
                {
                    NaughtyEditorGUI.HelpBox_Layout($"Bone \"{bone}\" is duplicated.", MessageType.Warning, logToConsole: false);
                    if (GUILayout.Button("Fix duplicate bone"))
                    {
                        list.serializedProperty.DeleteArrayElementAtIndex(i);
                        i--;
                    }

                    isValid = false;
                }
                else
                {
                    bonesWithOverride.Add(bone);
                }
            }

            if (!isValid) SetIsNotValid();
        }

        void WarnAgainstUnstablePositionMatchingParameters()
        {
            bool hasUnstableSettings = false;

            if (!PositionMatchingParametersAreStable(serializedObject.FindProperty("globalPositionAlpha").floatValue, serializedObject.FindProperty("globalPositionDampingRatio").floatValue))
            {
                hasUnstableSettings = true;
            }

            for (int i = 0; i < positionMatchingOverridesList.serializedProperty.arraySize && !hasUnstableSettings; i++)
            {
                if (!PositionMatchingParametersAreStable(
                    positionMatchingOverridesList.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("alpha").floatValue,
                    positionMatchingOverridesList.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("dampingRatio").floatValue))
                {
                    hasUnstableSettings = true;
                }
            }

            if (hasUnstableSettings)
            {
                NaughtyEditorGUI.HelpBox_Layout($"Settings where (alpha + dampingRatio > {MAX_STABLE_ALPHA_PLUS_DAMPING}) are often unstable. Consider lowering the parameters."
                    , MessageType.Warning, logToConsole: false);

                if (GUILayout.Button("Fix unstable parameters"))
                {
                    FixUnstableParameters(serializedObject.FindProperty("globalPositionAlpha"), serializedObject.FindProperty("globalPositionDampingRatio"));

                    for (int i = 0; i < positionMatchingOverridesList.serializedProperty.arraySize; i++)
                    {
                        FixUnstableParameters(
                            positionMatchingOverridesList.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("alpha"),
                            positionMatchingOverridesList.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("dampingRatio")
                        );
                    }
                }
            }


            void FixUnstableParameters(SerializedProperty alpha, SerializedProperty dampingRatio)
            {
                if (!PositionMatchingParametersAreStable(alpha.floatValue, dampingRatio.floatValue))
                {
                    alpha.floatValue = MAX_STABLE_ALPHA_PLUS_DAMPING - dampingRatio.floatValue;
                }
            }
        }

        void WarnAgainstPositionWithoutRotation()
        {
            bool hasInvalidBones = false;

            if (serializedObject.FindProperty("globalPositionAlpha").floatValue > 0 && serializedObject.FindProperty("globalRotationAlpha").floatValue <= 0)
            {
                hasInvalidBones = true;
            }

            if (!hasInvalidBones)
            {
                bonesWithNonZeroPositionMatchingOverride.Clear();

                SerializedProperty posOverrides = serializedObject.FindProperty("positionMatchingOverrides");
                SerializedProperty rotOverrides = serializedObject.FindProperty("rotationMatchingOverrides");

                for (int i = 0; i < posOverrides.arraySize; i++)
                {
                    SerializedProperty posOverride = posOverrides.GetArrayElementAtIndex(i);
                    if (posOverride.FindPropertyRelative("alpha").floatValue > 0)
                    {
                        bonesWithNonZeroPositionMatchingOverride.Add(posOverride.FindPropertyRelative("bone.name").stringValue);
                    }
                }

                for (int i = 0; i < rotOverrides.arraySize; i++)
                {
                    SerializedProperty rotOverride = rotOverrides.GetArrayElementAtIndex(i);
                    bool boneHasPosOverride = bonesWithNonZeroPositionMatchingOverride.Contains(rotOverride.FindPropertyRelative("bone.name").stringValue);
                    if (boneHasPosOverride && rotOverride.FindPropertyRelative("alpha").floatValue <= 0)
                    {
                        hasInvalidBones = true;
                        break;
                    }
                }
            }

            if (hasInvalidBones)
            {
                NaughtyEditorGUI.HelpBox_Layout("Position Matching with no Rotation Matching can produce horrifying results. \n" +
                    "To avoid this, make sure that no bone has a Position Alpha over 0 and a Rotation Alpha of 0.", MessageType.Warning, logToConsole: false);
            }
        }
        #endregion

        #region Drag & Drop
        protected override Object GetDragAndDropTarget(Event e)
        {
            return SceneDragAndDrop.GetAssignTarget<RagdollAnimator>(e);
        }

        protected override void AssignDragAndDrop(SerializedObject assignTarget)
        {
            Object profile = target;
            assignTarget.FindProperty("profile").objectReferenceValue = profile;
        }
        #endregion

        protected override void Initialize()
        {
            rebuildBoneProfilesMethod = ReflectionUtility.GetMethod(target, "BuildOverridenBoneProfiles");

            positionMatchingOverridesList = ReorderableListUtility.Create(serializedObject.FindProperty("positionMatchingOverrides"),
                true, false, true, true, "Position Matching Overrides"
            );
            positionMatchingOverridesList.AddDefaultValueSetter((SerializedProperty newEntry) =>
            {
                newEntry.FindPropertyRelative("alpha").floatValue = serializedObject.FindProperty("globalPositionAlpha").floatValue;
                newEntry.FindPropertyRelative("dampingRatio").floatValue = serializedObject.FindProperty("globalPositionDampingRatio").floatValue;
            });

            rotationMatchingOverridesList = ReorderableListUtility.Create(serializedObject.FindProperty("rotationMatchingOverrides"),
                true, false, true, true, "Rotation Matching Overrides"
            );
            rotationMatchingOverridesList.AddDefaultValueSetter((SerializedProperty newEntry) =>
            {
                newEntry.FindPropertyRelative("alpha").floatValue = serializedObject.FindProperty("globalRotationAlpha").floatValue;
                newEntry.FindPropertyRelative("dampingRatio").floatValue = serializedObject.FindProperty("globalRotationDampingRatio").floatValue;
            });

            bonesWithNonZeroPositionMatchingOverride = new HashSet<string>();
            bonesWithOverride = new HashSet<string>();
        }
    }
}
