using Hairibar.NaughtyExtensions.Editor;
using UnityEditor;

namespace Hairibar.Ragdoll.Editor
{
    [CustomEditor(typeof(RagdollCollisionIgnorer))]
    internal class RagdollCollisionIgnorerEditor : UnityEditor.Editor
    {
        SerializedProperty profileProperty;
        SerializedProperty bindingsDefinition;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UsePropertySetterDrawer.Draw_Layout(profileProperty);
            RagdollProfileEditorUtility.ValidateProfileField_Layout(profileProperty, bindingsDefinition.objectReferenceValue as RagdollDefinition, false);

            serializedObject.ApplyModifiedProperties();
        }


        void OnEnable()
        {
            profileProperty = serializedObject.FindProperty("_profile");
            bindingsDefinition = new SerializedObject((target as RagdollCollisionIgnorer).GetComponent<RagdollDefinitionBindings>()).FindProperty("_definition");
        }
    }
}
