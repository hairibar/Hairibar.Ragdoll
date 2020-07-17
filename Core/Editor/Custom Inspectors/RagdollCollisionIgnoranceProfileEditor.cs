using System.Collections.Generic;
using Hairibar.EngineExtensions.Editor;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Hairibar.Ragdoll.Editor
{
    [CustomEditor(typeof(RagdollCollisionProfile))]
    internal class RagdollCollisionIgnoranceProfileEditor : RagdollProfileEditor
    {
        const float HORIZONTAL_PAIR_SPACING = 2;

        #region Private State
        ReorderableList bonePairsList;
        ReorderableList disabledList;

        HashSet<RagdollCollisionProfile.BonePair> duplicatePairValidationSet;
        HashSet<string> duplicateDisabledBonesValidationSet;
        #endregion

        protected override bool RequiresDefinition => true;

        #region Inspector
        protected override void DrawInspector()
        {
            DrawDefinitionField();

            if (HasValidDefinition)
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                bonePairsList.DoLayoutList();
                ValidateNoSameBonePairs();
                ValidateNoDuplicatePairs();
                ShowConnectedBonesAlwaysIgnoredHelpBox();
                EditorGUILayout.Space();

                disabledList.DoLayoutList();
                ValidateNoDuplicateDisabledBones();
            }
        }

        void ShowConnectedBonesAlwaysIgnoredHelpBox()
        {
            NaughtyEditorGUI.HelpBox_Layout("Bones that are connected to each other already ignore collisions between them.", MessageType.Info);
        }
        #endregion

        #region Operations
        protected override void OnDefinitionChanged()
        {
            ClearLists();
        }

        void ClearLists()
        {
            bonePairsList.serializedProperty.ClearArray();
            disabledList.serializedProperty.ClearArray();
        }
        #endregion

        #region Validation
        void ValidateNoSameBonePairs()
        {
            bool hasInvalidPairs = false;
            foreach (SerializedProperty pair in bonePairsList.serializedProperty)
            {
                if (PairIsInvalid(pair))
                {
                    hasInvalidPairs = true;
                    break;
                }
            }

            if (hasInvalidPairs)
            {
                NaughtyEditorGUI.HelpBox_Layout("Pairs can't have the same bone on both sides.", MessageType.Warning);
                SetIsNotValid();

                if (GUILayout.Button("Fix")) Fix();
            }


            bool PairIsInvalid(SerializedProperty pair)
            {
                return pair.FindPropertyRelative("boneA.name").stringValue == pair.FindPropertyRelative("boneB.name").stringValue;
            }

            void Fix()
            {
                for (int i = 0; i < bonePairsList.serializedProperty.arraySize; i++)
                {
                    if (PairIsInvalid(bonePairsList.serializedProperty.GetArrayElementAtIndex(i)))
                    {
                        bonePairsList.serializedProperty.DeleteArrayElementAtIndex(i);
                        i--;
                    }
                }
            }
        }

        void ValidateNoDuplicatePairs()
        {
            bool hasDuplicates = false;
            duplicatePairValidationSet.Clear();

            foreach (SerializedProperty pair in bonePairsList.serializedProperty)
            {
                if (!duplicatePairValidationSet.Add(CreateBonePairFromPairProperty(pair)))
                {
                    hasDuplicates = true;
                    break;
                }
            }

            if (hasDuplicates)
            {
                NaughtyEditorGUI.HelpBox_Layout("There are duplicate pairs.", MessageType.Warning);
                SetIsNotValid();

                if (GUILayout.Button("Fix")) Fix();
            }


            void Fix()
            {
                duplicatePairValidationSet.Clear();

                for (int i = 0; i < bonePairsList.serializedProperty.arraySize; i++)
                {
                    SerializedProperty pair = bonePairsList.serializedProperty.GetArrayElementAtIndex(i);

                    if (!duplicatePairValidationSet.Add(CreateBonePairFromPairProperty(pair)))
                    {
                        bonePairsList.serializedProperty.DeleteArrayElementAtIndex(i);
                        i--;
                    }
                }
            }
        }

        RagdollCollisionProfile.BonePair CreateBonePairFromPairProperty(SerializedProperty property)
        {
            return new RagdollCollisionProfile.BonePair
            {
                boneA = property.FindPropertyRelative("boneA.name").stringValue,
                boneB = property.FindPropertyRelative("boneB.name").stringValue
            };
        }

        void ValidateNoDuplicateDisabledBones()
        {
            bool hasDuplicates = false;
            duplicateDisabledBonesValidationSet.Clear();

            foreach (SerializedProperty bone in disabledList.serializedProperty)
            {
                if (!duplicateDisabledBonesValidationSet.Add(bone.FindPropertyRelative("name").stringValue))
                {
                    hasDuplicates = true;
                    break;
                }
            }

            if (hasDuplicates)
            {
                NaughtyEditorGUI.HelpBox_Layout("There are duplicated bones.", MessageType.Warning);
                SetIsNotValid();

                if (GUILayout.Button("Fix")) Fix();
            }

            void Fix()
            {
                duplicateDisabledBonesValidationSet.Clear();

                for (int i = 0; i < disabledList.serializedProperty.arraySize; i++)
                {
                    SerializedProperty bone = disabledList.serializedProperty.GetArrayElementAtIndex(i);

                    if (!duplicateDisabledBonesValidationSet.Add(bone.FindPropertyRelative("name").stringValue))
                    {
                        disabledList.serializedProperty.DeleteArrayElementAtIndex(i);
                        i--;
                    }
                }
            }
        }
        #endregion

        #region List Callbacks
        void DrawIgnoredPairElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect leftRect = new Rect(rect.position, new Vector2(rect.width / 2 - HORIZONTAL_PAIR_SPACING, rect.height));
            Rect rightRect = new Rect(rect.x + rect.width / 2 + HORIZONTAL_PAIR_SPACING, rect.y, leftRect.width, leftRect.height);

            SerializedProperty boneA = bonePairsList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("boneA.name");
            SerializedProperty boneB = bonePairsList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("boneB.name");

            BoneNamePopupDrawer.Draw(leftRect, boneA, Definition, GUIContent.none);
            BoneNamePopupDrawer.Draw(rightRect, boneB, Definition, GUIContent.none);

            if (!BoneNamePopupDrawer.IsValidValue(boneA.stringValue, Definition) || !BoneNamePopupDrawer.IsValidValue(boneB.stringValue, Definition))
            {
                SetIsNotValid();
            }
        }

        float GetIgnoredPairsElementHeight(int index)
        {
            return EditorGUIUtility.singleLineHeight * 1.5f;
        }

        void DrawDisabledBonesElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty property = disabledList.serializedProperty.GetArrayElementAtIndex(index);

            BoneNamePopupDrawer.Draw(rect, property.FindPropertyRelative("name"), Definition, GUIContent.none);
        }

        float GetDisabledBonesElementHeight(int index)
        {
            return EditorGUIUtility.singleLineHeight * 1.5f;
        }
        #endregion

        #region Drag & Drop
        protected override Object GetDragAndDropTarget(Event e)
        {
            return SceneDragAndDrop.GetAssignTarget<RagdollCollisionIgnorer>(e);
        }

        protected override void AssignDragAndDrop(SerializedObject ignorer)
        {
            Object profile = target;
            ignorer.FindProperty("_profile").objectReferenceValue = profile;
            ReflectionUtility.GetProperty(ignorer.targetObject, "CollisionIgnoranceProfile").SetValue(ignorer.targetObject, target);
        }
        #endregion

        protected override void Initialize()
        {
            bonePairsList = ReorderableListUtility.Create(serializedObject.FindProperty("bonePairs"), false, true, true, true, "Ignored Collision Pairs");
            bonePairsList.drawElementCallback = DrawIgnoredPairElement;
            bonePairsList.elementHeightCallback = GetIgnoredPairsElementHeight;

            disabledList = ReorderableListUtility.Create(serializedObject.FindProperty("disabled"), false, true, true, true, "Disabled Collision Bones");
            disabledList.drawElementCallback = DrawDisabledBonesElement;
            disabledList.elementHeightCallback = GetDisabledBonesElementHeight;

            duplicatePairValidationSet = new HashSet<RagdollCollisionProfile.BonePair>();
            duplicateDisabledBonesValidationSet = new HashSet<string>();
        }
    }
}
