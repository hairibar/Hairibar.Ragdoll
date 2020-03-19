using UnityEditor;
using UnityEngine;

namespace Hairibar.Ragdoll.Animation.Editor
{
    public abstract class SetRagdollProfileOnEnterEditor : UnityEditor.Editor
    {
        HideableProperty profile;

        protected abstract string TypeDisplayName { get; }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawProperty(profile);
            serializedObject.ApplyModifiedProperties();
        }

        void DrawProperty(HideableProperty property)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            property.show = EditorGUILayout.Toggle("Set " + TypeDisplayName, property.show);
            if (property.show)
            {
                EditorGUILayout.PropertyField(property.property, GUIContent.none);
            }
            else
            {
                property.property.objectReferenceValue = null;
            }

            EditorGUILayout.EndVertical();
        }

        void OnEnable()
        {
            profile = new HideableProperty(serializedObject.FindProperty("profile"));
        }
    }

    class HideableProperty
    {
        public SerializedProperty property;
        public bool show;

        public HideableProperty(SerializedProperty property)
        {
            this.property = property;
            show = this.property.objectReferenceValue != null;
        }
    }
}
