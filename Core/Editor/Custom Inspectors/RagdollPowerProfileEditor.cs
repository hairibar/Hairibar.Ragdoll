using Hairibar.EngineExtensions.Editor;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace Hairibar.Ragdoll.Editor
{
    [CustomEditor(typeof(RagdollPowerProfile))]
    internal class RagdollPowerProfileEditor : RagdollProfileEditor
    {
        const float POPUP_MARGIN = 2;

        static PowerSetting GetPowerSettingFromSerializedProperty(SerializedProperty property)
        {
            return (PowerSetting) System.Enum.Parse(typeof(PowerSetting), property.enumNames[property.enumValueIndex], true);
        }

        #region Private State
        SerializedProperty settingsDictionary;
        SerializedProperty settingsKeys;
        SerializedProperty settingsValues;

        PowerSetting setAllValue;
        #endregion

        protected override bool RequiresDefinition => true;

        #region Drawing
        protected override void DrawInspector()
        {
            DrawDefinitionField();

            if (HasValidDefinition)
            {
                RefreshDictionary();

                DrawPowerProfile();

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                DrawSetAllButton();
            }
            else
            {
                SetIsNotValid();
            }
        }

        void DrawPowerProfile()
        {
            NaughtyEditorGUI.BeginBoxGroup_Layout("Power Settings");

            for (int i = 0; i < settingsKeys.arraySize; i++)
            {
                DrawPowerSetting(settingsKeys.GetArrayElementAtIndex(i), settingsValues.GetArrayElementAtIndex(i));
            }

            NaughtyEditorGUI.EndBoxGroup_Layout();
        }

        void DrawPowerSetting(SerializedProperty keyProperty, SerializedProperty valueProperty)
        {
            GUIContent label = new GUIContent(keyProperty.FindPropertyRelative("name").stringValue);
            Rect controlRect = EditorGUILayout.GetControlRect(true);

            PowerSetting currentSetting = GetPowerSettingFromSerializedProperty(valueProperty);
            DrawPowerSettingColorRect(controlRect, currentSetting);

            EditorGUI.PropertyField(controlRect, valueProperty, label, false);
        }

        void DrawPowerSettingColorRect(Rect controlRect, PowerSetting powerSetting)
        {
            float size = EditorGUIUtility.singleLineHeight - POPUP_MARGIN;
            Rect colorRect = new Rect(controlRect.x + EditorGUIUtility.labelWidth - size, controlRect.y + POPUP_MARGIN / 2, size, size);

            EditorGUI.DrawRect(colorRect, powerSetting.GetVisualizationColor());
        }

        void DrawSetAllButton()
        {
            Rect controlRect = EditorGUILayout.GetControlRect();

            Rect buttonRect = new Rect(controlRect.x, controlRect.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(buttonRect, "Set all to:"))
            {
                SetAllTo(setAllValue);
            }

            Rect popupRect = new Rect(buttonRect.xMax + POPUP_MARGIN, buttonRect.y, controlRect.width - buttonRect.width, buttonRect.height);
            setAllValue = (PowerSetting) EditorGUI.EnumPopup(popupRect, GUIContent.none, setAllValue);
        }
        #endregion

        #region Dictionary Operations
        void RefreshDictionary()
        {
            RagdollProfileEditorUtility.RefreshBoneDictionaryKeys(settingsDictionary, definitionProperty,
                    (SerializedProperty newEntry) => newEntry.enumValueIndex = (int) PowerSetting.Powered);
        }

        void SetAllTo(PowerSetting powerSetting)
        {
            for (int i = 0; i < settingsValues.arraySize; i++)
            {
                settingsValues.GetArrayElementAtIndex(i).enumValueIndex = (int) powerSetting;
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
            Object powerProfile = target;
            ragdollSettings.FindProperty("_powerProfile").objectReferenceValue = powerProfile;
            ReflectionUtility.GetProperty(ragdollSettings.targetObject, "PowerProfile").SetValue(ragdollSettings.targetObject, powerProfile);
        }
        #endregion

        protected override void Initialize()
        {
            settingsDictionary = serializedObject.FindProperty("settings");
            settingsKeys = settingsDictionary.FindPropertyRelative("keys");
            settingsValues = settingsDictionary.FindPropertyRelative("values");
        }
    }
}
