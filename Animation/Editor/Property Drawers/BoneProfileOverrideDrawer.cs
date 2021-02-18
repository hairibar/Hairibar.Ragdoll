using Hairibar.NaughtyExtensions.Editor;
using Hairibar.Ragdoll.Editor;
using UnityEditor;
using UnityEngine;

namespace Hairibar.Ragdoll.Animation.Editor
{
    [CustomPropertyDrawer(typeof(RagdollAnimationProfile.BoneProfileOverride))]
    internal class BoneProfileOverrideDrawer : PropertyDrawer
    {
        static RagdollDefinition currentDefinition;


        public static void SetCurrentRagdollDefinition(RagdollDefinition definition)
        {
            currentDefinition = definition;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!currentDefinition)
            {
                throw new System.InvalidOperationException("BoneProfileOverrideDrawer.currentDefinition must be set before drawing an override.");
            }

            Rect controlRect = new Rect(position.position, new Vector2(position.width, EditorGUIUtility.singleLineHeight));

            BoneNamePopupDrawer.Draw(controlRect, property.FindPropertyRelative("bone.name"), currentDefinition, GUIContent.none);
            AdvanceOneLine();

            NonLinearSliderDrawer.Draw(controlRect, property.FindPropertyRelative("alpha"), 0, 1, QuadraticSliderDrawer.GetQuadraticFunction(2));
            AdvanceOneLine();

            EditorGUI.Slider(controlRect, property.FindPropertyRelative("dampingRatio"), 0, 1);


            void AdvanceOneLine()
            {
                controlRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 2;
        }
    }
}
