using System.Collections.Generic;
using NaughtyAttributes.Editor;
using UnityEditor;

namespace Hairibar.Ragdoll
{
    public static class RagdollDefinitionValidator
    {
        public static bool Validate(RagdollDefinition definition, bool doGUI)
        {
            SerializedObject serializedObject = new SerializedObject(definition);
            SerializedProperty boneList = serializedObject.FindProperty("bones");
            SerializedProperty rootProperty = serializedObject.FindProperty("_root");

            return ValidateNotEmpty(boneList, doGUI) &&
                ValidateThereIsARoot(rootProperty, boneList, doGUI) &&
                ValidateNoEmptyNames(boneList, doGUI) &&
                ValidateNoDuplicateNames(boneList, doGUI);
        }

        static bool ValidateNotEmpty(SerializedProperty boneList, bool doGUI)
        {
            bool isEmpty = boneList.arraySize == 0;

            if (isEmpty)
            {
                if (doGUI) NaughtyEditorGUI.HelpBox_Layout("At least 1 bone is required.", MessageType.Error);
                return false;
            }
            else
            {
                return true;
            }
        }

        static bool ValidateThereIsARoot(SerializedProperty rootNameProperty, SerializedProperty boneList, bool doGUI)
        {
            string rootName = rootNameProperty.FindPropertyRelative("name").stringValue;

            bool rootExists = false;
            for (int i = 0; i < boneList.arraySize; i++)
            {
                string currentBone = boneList.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
                if (currentBone == rootName)
                {
                    rootExists = true;
                    break;
                }
            }

            if (!rootExists)
            {
                if (boneList.arraySize == 1)
                {
                    rootNameProperty.stringValue = boneList.GetArrayElementAtIndex(0).FindPropertyRelative("name").stringValue;
                }
                else
                {
                    if (doGUI) NaughtyEditorGUI.HelpBox_Layout("A bone must be selected as root bone.", MessageType.Error);
                    return false;
                }
            }

            return true;
        }

        static bool ValidateNoEmptyNames(SerializedProperty boneList, bool doGUI)
        {
            bool hasEmptyNames = false;
            for (int i = 0; i < boneList.arraySize; i++)
            {
                string currentBone = boneList.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
                if (string.IsNullOrEmpty(currentBone))
                {
                    hasEmptyNames = true;
                    break;
                }
            }

            if (hasEmptyNames)
            {
                if (doGUI) NaughtyEditorGUI.HelpBox_Layout("Empty bone names are not allowed.", MessageType.Error);
                return false;
            }
            else
            {
                return true;
            }
        }

        static bool ValidateNoDuplicateNames(SerializedProperty boneList, bool doGUI)
        {
            HashSet<string> duplicateSearchingSet = new HashSet<string>();

            bool hasDuplicates = false;
            for (int i = 0; i < boneList.arraySize; i++)
            {
                if (!duplicateSearchingSet.Add(boneList.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue))
                {
                    hasDuplicates = true;
                    break;
                }
            }

            if (hasDuplicates)
            {
                if (doGUI) NaughtyEditorGUI.HelpBox_Layout("There are duplicate bone names. This is not allowed.", MessageType.Error);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
