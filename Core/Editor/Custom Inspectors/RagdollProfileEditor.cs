using UnityEditor;
using UnityEngine;

namespace Hairibar.Ragdoll.Editor
{
    public abstract class RagdollProfileEditor : UnityEditor.Editor
    {
        #region References
        protected SerializedProperty definitionProperty;
        SerializedProperty isValidProperty;
        #endregion

        #region Definition
        protected void DrawDefinitionField()
        {
            EditorGUI.BeginChangeCheck();
            RagdollProfileEditorUtility.DrawDefinitionField_Layout(definitionProperty, RequiresDefinition);

            if (EditorGUI.EndChangeCheck()) OnDefinitionChanged();
        }

        protected RagdollDefinition Definition => definitionProperty.objectReferenceValue as RagdollDefinition;

        protected bool HasDefinition => RagdollProfileEditorUtility.HasDefinition(definitionProperty);

        protected bool HasValidDefinition => RagdollProfileEditorUtility.IsValid(definitionProperty);

        protected abstract bool RequiresDefinition { get; }

        protected virtual void OnDefinitionChanged()
        {

        }
        #endregion

        #region Validity Flag
        protected void SetIsNotValid()
        {
            isValidProperty.boolValue = false;
        }

        protected bool IsValid => isValidProperty.boolValue;

        void SetIsValid()
        {
            isValidProperty.boolValue = true;
        }
        #endregion

        #region Inspector
        public sealed override void OnInspectorGUI()
        {
            serializedObject.Update();
            SetIsValid();

            DrawInspector();

            if (RequiresDefinition && !HasValidDefinition) SetIsNotValid();

            serializedObject.ApplyModifiedProperties();

            AfterAppliedProperties();
        }

        protected abstract void DrawInspector();

        protected virtual void AfterAppliedProperties() { }
        #endregion

        #region Drag & Drop
#pragma warning disable IDE0060 // Remove unused parameter. Unity requires SceneView to be there even if we don't use it.
        internal void OnSceneDrag(SceneView sceneView)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            Event e = Event.current;
            Object assignTarget = GetDragAndDropTarget(e);

            switch (e.type)
            {
                case EventType.DragUpdated:
                    if (assignTarget) DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    else DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

                    e.Use();
                    break;
                case EventType.DragPerform:
                    if (assignTarget)
                    {
                        SerializedObject so = new SerializedObject(assignTarget);
                        AssignDragAndDrop(so);
                        so.ApplyModifiedProperties();
                    }

                    e.Use();
                    break;
            }
        }

        protected abstract Object GetDragAndDropTarget(Event e);

        protected abstract void AssignDragAndDrop(SerializedObject assignTarget);
        #endregion

        #region Initialization
        void OnEnable()
        {
            definitionProperty = serializedObject.FindProperty("definition");
            isValidProperty = serializedObject.FindProperty("_isValid");

            Initialize();
        }

        protected abstract void Initialize();
        #endregion
    }
}
