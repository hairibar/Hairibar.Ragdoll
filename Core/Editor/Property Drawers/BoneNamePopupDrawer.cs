using System.Collections;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace Hairibar.Ragdoll.Editor
{
    public class BoneNamePopupDrawer : PropertyDrawer
    {
        public static void Draw_Layout(SerializedProperty property, RagdollDefinition definition)
        {
            Draw_Layout(property, property.GetLabelContent(), definition);
        }

        public static void Draw_Layout(SerializedProperty property, GUIContent guiContent, RagdollDefinition definition)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            GUIContent label = EditorGUI.BeginProperty(rect, guiContent, property);

            property.stringValue = Draw(rect, property.stringValue, label, definition);
            EditorGUI.EndProperty();
        }

        public static string Draw_Layout(string currentValue, GUIContent guiContent, RagdollDefinition definition)
        {
            return Draw(EditorGUILayout.GetControlRect(), currentValue, guiContent, definition);
        }


        public static void Draw(Rect rect, SerializedProperty property, RagdollDefinition definition)
        {
            Draw(rect, property, definition, property.GetLabelContent());
        }

        public static void Draw(Rect rect, SerializedProperty property, RagdollDefinition definition, GUIContent guiContent)
        {
            GUIContent label = EditorGUI.BeginProperty(rect, guiContent, property);
            property.stringValue = Draw(rect, property.stringValue, label, definition);
            EditorGUI.EndProperty();
        }

        public static string Draw(Rect rect, string currentValue, GUIContent guiContent, RagdollDefinition definition)
        {
            string[] options = GetOptionsFromDefinition(definition);
            int selectedIndex = System.Array.IndexOf(options, currentValue);

            rect.height = EditorGUIUtility.singleLineHeight;

            int newSelection = EditorGUI.Popup(rect, guiContent.text, selectedIndex, options);

            if (newSelection == -1)
            {
                ShowBoneNotFoundError(currentValue, definition.name);
                return currentValue;
            }
            else
            {
                return options[newSelection];
            }
        }


        public static bool IsValidValue(string value, RagdollDefinition ragdollDefinition)
        {
            foreach (BoneName bone in ragdollDefinition.Bones)
            {
                if (value == bone) return true;
            }

            return false;
        }


        static string[] GetOptionsFromDefinition(RagdollDefinition definition)
        {
            IEnumerable boneNames = definition.Bones;
            string[] names = new string[definition.BoneCount];

            int i = 0;
            foreach (BoneName name in boneNames)
            {
                names[i] = name;
                i++;
            }

            return names;
        }

        static void ShowBoneNotFoundError(string boneName, string definitionName)
        {
            NaughtyEditorGUI.HelpBox_Layout($"Bone \"{boneName}\" was not found in definition {definitionName}.", MessageType.Warning);
        }
    }
}
