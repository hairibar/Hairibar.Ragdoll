using System.Collections.Generic;
using DuoVia.FuzzyStrings;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace Hairibar.Ragdoll.Editor
{
    [CustomEditor(typeof(RagdollDefinitionBindings))]
    internal class RagdollDefinitionBindingsEditor : UnityEditor.Editor
    {
        const float AUTOCOMPLETE_CONFIDENCE_THRESHOLD = 0.5f;

        #region Private State
        SerializedProperty definition;
        SerializedProperty bindingsDictionary;
        SerializedProperty keys;
        SerializedProperty values;

        bool hasValidDefinition;

        SortedSet<int> noDuplicateJointValidatingSet;
        #endregion


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            RagdollProfileEditorUtility.DrawDefinitionField_Layout(definition, true);

            hasValidDefinition = RagdollProfileEditorUtility.IsValid(definition);
            if (hasValidDefinition)
            {
                RagdollProfileEditorUtility.RefreshBoneDictionaryKeys(bindingsDictionary, definition);

                NaughtyEditorGUI.BeginBoxGroup_Layout("Bindings");

                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                DrawBindingFields();
                DrawAutoCompleteButton();
                EditorGUI.EndDisabledGroup();

                NaughtyEditorGUI.EndBoxGroup_Layout();

                ValidateNoDuplicateJoints();
                ValidateNoUnassignedBones();
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawBindingFields()
        {
            values.arraySize = keys.arraySize;

            for (int i = 0; i < keys.arraySize; i++)
            {
                GUIContent guiContent = new GUIContent(keys.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue);

                EditorGUILayout.ObjectField(values.GetArrayElementAtIndex(i), guiContent);
            }
        }

        #region Autocomplete
        void DrawAutoCompleteButton()
        {
            if (Application.isPlaying) return;

            if (HasEmptyFields())
            {
                if (GUILayout.Button("Attempt Autocomplete"))
                {
                    TryAutocomplete();
                }
            }
        }

        bool HasEmptyFields()
        {
            for (int i = 0; i < values.arraySize; i++)
            {
                if (!values.GetArrayElementAtIndex(i).objectReferenceValue)
                {
                    return true;
                }
            }

            return false;
        }

        void TryAutocomplete()
        {
            ConfigurableJoint[] joints = (target as RagdollDefinitionBindings).GetComponentsInChildren<ConfigurableJoint>();

            for (int i = 0; i < keys.arraySize; i++)
            {
                if (!values.GetArrayElementAtIndex(i).objectReferenceValue)
                {
                    TryAutocompleteField(i, joints);
                }
            }
        }

        void TryAutocompleteField(int index, ConfigurableJoint[] joints)
        {
            string boneName = keys.GetArrayElementAtIndex(index).FindPropertyRelative("name").stringValue;
            SerializedProperty valueProperty = values.GetArrayElementAtIndex(index);
            ConfigurableJoint bestMatch = FindBestMatch(boneName, joints);

            valueProperty.objectReferenceValue = bestMatch;
        }

        static ConfigurableJoint FindBestMatch(string boneName, ConfigurableJoint[] joints)
        {
            ConfigurableJoint bestMatch = null;
            double bestScore = double.NegativeInfinity;

            string jointNamePrefix = FindCommonPrefix(joints);
            string standardBoneName = StandardizeBoneName(boneName, jointNamePrefix);

            foreach (ConfigurableJoint joint in joints)
            {
                double score = standardBoneName.FuzzyMatch(StandardizeBoneName(joint.name, jointNamePrefix));
                if (score > bestScore)
                {
                    bestMatch = joint;
                    bestScore = score;
                }
            }

            if (bestScore > AUTOCOMPLETE_CONFIDENCE_THRESHOLD) return bestMatch;
            else return null;
        }

        static string FindCommonPrefix(ConfigurableJoint[] joints)
        {
            string ret = "";
            int idx = 0;

            while (true)
            {
                char thisLetter = '\0';
                foreach (var joint in joints)
                {
                    string word = joint.name;
                    if (idx == word.Length)
                    {
                        // if we reached the end of a word then we are done
                        return ret;
                    }
                    if (thisLetter == '\0')
                    {
                        // if this is the first word then note the letter we are looking for
                        thisLetter = word[idx];
                    }
                    if (thisLetter != word[idx])
                    {
                        return ret;
                    }
                }

                // if we haven't said we are done then this position passed
                ret += thisLetter;
                idx++;
            }
        }

        static string StandardizeBoneName(string boneName, string commonPrefix)
        {
            boneName = boneName.Replace(commonPrefix, "");

            string lowerName = boneName.ToLower();

            int rightFullPrefixIndex = lowerName.IndexOf("right");
            if (rightFullPrefixIndex > -1)
            {
                boneName = boneName.Remove(rightFullPrefixIndex, 5);
                boneName += "_R";
            }

            int leftFullPrefixIndex = lowerName.IndexOf("left");
            if (leftFullPrefixIndex > -1)
            {
                boneName = boneName.Remove(leftFullPrefixIndex, 4);
                boneName += "_L";
            }

            int rightShortPrefixIndex = boneName.IndexOf("R_");
            if (rightShortPrefixIndex > -1)
            {
                boneName = boneName.Remove(rightShortPrefixIndex, 2);
                boneName += "_R";
            }

            int leftShortPrefixIndex = boneName.IndexOf("L_");
            if (leftShortPrefixIndex > -1)
            {
                boneName = boneName.Remove(leftShortPrefixIndex, 2);
                boneName += "_L";
            }

            return boneName;
        }
        #endregion

        #region Validators
        void ValidateNoDuplicateJoints()
        {
            noDuplicateJointValidatingSet.Clear();

            for (int i = 0; i < values.arraySize; i++)
            {
                Object reference = values.GetArrayElementAtIndex(i).objectReferenceValue;

                if (!reference) continue;

                bool alreadyPresent = !noDuplicateJointValidatingSet.Add(reference.GetInstanceID());

                if (alreadyPresent)
                {
                    NaughtyEditorGUI.HelpBox_Layout("The same Joint is bound to multiple bones. This is not allowed.", MessageType.Error);
                    return;
                }
            }
        }

        void ValidateNoUnassignedBones()
        {
            for (int i = 0; i < values.arraySize; i++)
            {
                if (values.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    NaughtyEditorGUI.HelpBox_Layout("All bones must be bound to a ConfigurableJoint.", MessageType.Error);
                    return;
                }
            }
        }
        #endregion


        void OnEnable()
        {
            bindingsDictionary = serializedObject.FindProperty("bindings");

            definition = serializedObject.FindProperty("_definition");
            keys = bindingsDictionary.FindPropertyRelative("keys");
            values = bindingsDictionary.FindPropertyRelative("values");

            noDuplicateJointValidatingSet = new SortedSet<int>();
        }
    }
}
