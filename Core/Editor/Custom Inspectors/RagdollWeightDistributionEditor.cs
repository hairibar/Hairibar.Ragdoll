using Hairibar.EngineExtensions.Editor;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace Hairibar.Ragdoll.Editor
{
    [CustomEditor(typeof(RagdollWeightDistribution))]
    internal class RagdollWeightDistributionEditor : RagdollProfileEditor
    {
        #region Private State
        SerializedProperty factorsDictionary;
        SerializedProperty factorsKeys;
        SerializedProperty factorsValues;
        #endregion

        protected override bool RequiresDefinition => true;

        #region Drawing
        protected override void DrawInspector()
        {
            DrawDefinitionField();

            if (HasValidDefinition)
            {
                RagdollProfileEditorUtility.RefreshBoneDictionaryKeys(factorsDictionary, definitionProperty,
                    (SerializedProperty newValue) => newValue.floatValue = 1);

                DrawFactors();
            }
        }

        void DrawFactors()
        {
            NaughtyEditorGUI.BeginBoxGroup_Layout("Distribution");

            for (int i = 0; i < factorsKeys.arraySize; i++)
            {
                DrawFactor(factorsKeys.GetArrayElementAtIndex(i), factorsValues.GetArrayElementAtIndex(i));
            }

            NaughtyEditorGUI.EndBoxGroup_Layout();
        }

        void DrawFactor(SerializedProperty keyProperty, SerializedProperty valueProperty)
        {
            GUIContent label = new GUIContent(keyProperty.FindPropertyRelative("name").stringValue);
            EditorGUILayout.Slider(valueProperty, 0.1f, 1.5f, label);
        }
        #endregion

        #region Operations
        protected override void OnDefinitionChanged()
        {
            SetAllTo(1);
        }

        void SetAllTo(float value)
        {
            for (int i = 0; i < factorsValues.arraySize; i++)
            {
                factorsValues.GetArrayElementAtIndex(i).floatValue = value;
            }
        }
        #endregion

        #region Drag & Drop
        protected override Object GetDragAndDropTarget(Event e)
        {
            return SceneDragAndDrop.GetAssignTarget<RagdollSettings>(e);
        }

        protected override void AssignDragAndDrop(SerializedObject ragdollSettings)
        {
            Object weightDistribution = target;
            ragdollSettings.FindProperty("_weightDistribution").objectReferenceValue = weightDistribution;
            ReflectionUtility.GetProperty(ragdollSettings.targetObject, "WeightDistribution").SetValue(ragdollSettings.targetObject, target as RagdollWeightDistribution);
        }
        #endregion


        protected override void Initialize()
        {
            factorsDictionary = serializedObject.FindProperty("factors");
            factorsKeys = factorsDictionary.FindPropertyRelative("keys");
            factorsValues = factorsDictionary.FindPropertyRelative("values");
        }
    }
}
