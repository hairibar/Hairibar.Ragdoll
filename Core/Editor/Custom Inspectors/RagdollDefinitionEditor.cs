using Hairibar.NaughtyExtensions.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Hairibar.Ragdoll.Editor
{
    [CustomEditor(typeof(RagdollDefinition))]
    internal class RagdollDefinitionEditor : UnityEditor.Editor
    {
        #region Dimensions
        const float ROOT_LABEL_WIDTH = 30;
        const float ROOT_CONTROL_RIGHT_MARGIN = 0;
        const float ROOT_CONTROL_WIDTH = 15;
        const float HORIZONTAL_SPACING = 5;
        #endregion

        ReorderableList boneList;
        SerializedProperty rootNameProperty;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            boneList.DoLayoutList();

            serializedObject.FindProperty("_isValid").boolValue = RagdollDefinitionValidator.Validate(target as RagdollDefinition, true);

            serializedObject.ApplyModifiedProperties();
        }

        #region Bone List Callbacks
        void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            float verticalEmptySpace = rect.height - EditorGUIUtility.singleLineHeight;
            Rect textRect = new Rect(rect.x, rect.y + verticalEmptySpace / 2, rect.width - ROOT_CONTROL_RIGHT_MARGIN - ROOT_LABEL_WIDTH - HORIZONTAL_SPACING, EditorGUIUtility.singleLineHeight);

            SerializedProperty nameProperty = boneList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("name");
            EditorGUI.BeginProperty(textRect, GUIContent.none, nameProperty);
            nameProperty.stringValue = EditorGUI.TextField(textRect, nameProperty.stringValue);
            EditorGUI.EndProperty();


            Rect rootControlRect = new Rect(rect.xMax - ROOT_CONTROL_RIGHT_MARGIN - ROOT_LABEL_WIDTH / 2 - ROOT_CONTROL_WIDTH / 2, textRect.y, ROOT_CONTROL_WIDTH, EditorGUIUtility.singleLineHeight);

            bool setRoot = EditorGUI.Toggle(rootControlRect, rootNameProperty.stringValue == nameProperty.stringValue, EditorStyles.radioButton);
            if (setRoot)
            {
                rootNameProperty.stringValue = nameProperty.stringValue;
            }
        }

        void DrawListHeader(Rect rect)
        {
            rect.x = rect.xMax - ROOT_CONTROL_RIGHT_MARGIN - ROOT_LABEL_WIDTH;
            EditorGUI.LabelField(rect, "Root", EditorStyles.boldLabel);
        }

        void OnBoneAdded(SerializedProperty newElement)
        {
            if (boneList.count == 1) rootNameProperty.stringValue = serializedObject.FindProperty("bones").GetArrayElementAtIndex(0).FindPropertyRelative("name").stringValue;
        }
        #endregion

        void OnEnable()
        {
            boneList = ReorderableListUtility.Create(serializedObject.FindProperty("bones"), false, true, true, true, "Bones");
            boneList.elementHeightCallback = GetElementHeight;
            boneList.drawElementCallback = DrawListElement;
            boneList.drawHeaderCallback += DrawListHeader;
            boneList.AddDefaultValueSetter(OnBoneAdded);

            rootNameProperty = serializedObject.FindProperty("_root").FindPropertyRelative("name");
        }

        float GetElementHeight(int index)
        {
            return EditorGUIUtility.singleLineHeight * 1.5f;
        }
    }
}

