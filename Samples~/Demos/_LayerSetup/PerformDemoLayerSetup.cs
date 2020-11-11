#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Hairibar.Ragdoll.Demo
{
    //Inherits from ScriptableObject as a hack so that we can get the path to this file.
    internal sealed class PerformDemoLayerSetup : ScriptableObject
    {
        const string TAG_MANAGER_PRESET_NAME = "PRE_TagManager_RagdollDemo.preset";
        const string PHYSICS_MANAGER_PRESET_NAME = "PRE_PhysicsManager_RagdollDemo.preset";

        [MenuItem("Tools/Hairibar.Ragdoll/Perform Demo Layer Setup", isValidateFunction: false, priority = 10000)]
        public static void PerformSetup()
        {
            bool shouldContinue = EditorUtility.DisplayDialog("Hairibar.Ragdoll: Perform Demo Layer Setup?",
                "This operation will override the current tag, layer and physics project settings, setting the values required for the demos. Are you sure you want to continue?",
                "Continue", "Cancel");

            if (!shouldContinue) return;

            if (!TryFindPreset(TAG_MANAGER_PRESET_NAME, out Preset tagManagerPreset) || !TryFindPreset(PHYSICS_MANAGER_PRESET_NAME, out Preset physicsManagerPreset))
            {
                UnityEngine.Debug.LogError("Could not perform demo layer setup. At least one preset is missing.");
                return;
            }

            Object tagManager = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset");
            tagManagerPreset.ApplyTo(tagManager);

            Object physicsManager = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/DynamicsManager.asset");
            physicsManagerPreset.ApplyTo(physicsManager);
        }

        [MenuItem("Tools/Hairibar.Ragdoll/Perform Demo Layer Setup", isValidateFunction: true)]
        public static bool PresetsAreAvailable()
        {
            return TryFindPreset(TAG_MANAGER_PRESET_NAME, out _) && TryFindPreset(PHYSICS_MANAGER_PRESET_NAME, out _);
        }


        static string GetPresetDirectory()
        {
            var dummyInstance = PerformDemoLayerSetup.CreateInstance<PerformDemoLayerSetup>();
            MonoScript script = MonoScript.FromScriptableObject(dummyInstance);
            DestroyImmediate(dummyInstance);

            string scriptPath = AssetDatabase.GetAssetPath(script);
            return Path.GetDirectoryName(scriptPath);
        }

        static bool TryFindPreset(string filename, out Preset preset)
        {
            preset = AssetDatabase.LoadAssetAtPath<Preset>(Path.Combine(GetPresetDirectory(), filename));
            return preset;
        }
    }
}
#endif