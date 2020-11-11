using Hairibar.Ragdoll.Animation;
using Hairibar.Ragdoll.Debug;
using UnityEngine;
using UnityEngine.UI;

namespace Hairibar.Ragdoll.Demo
{
    public class DemoModeSwitcher : MonoBehaviour
    {
        Text gui;
        Mode currentMode;

        private void Start()
        {
            gui = GetComponent<Text>();
            EnterRagdollMode();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                ToggleMode();
            }
        }

        void ToggleMode()
        {
            switch (currentMode)
            {
                case Mode.Ragdoll:
                    EnterTargetMode();
                    break;
                case Mode.Target:
                    EnterBothMode();
                    break;
                case Mode.Both:
                    EnterRagdollMode();
                    break;
                default:
                    break;
            }
        }

        void EnterRagdollMode()
        {
            currentMode = Mode.Ragdoll;

            SetForceAnimatedPose(false);
            SetCollidersVisible(false);

            gui.text = "Ragdoll pose";
        }

        void EnterTargetMode()
        {
            currentMode = Mode.Target;

            SetForceAnimatedPose(true);
            SetCollidersVisible(false);

            gui.text = "Target pose";
        }

        void EnterBothMode()
        {
            currentMode = Mode.Both;

            SetForceAnimatedPose(true);
            SetCollidersVisible(true);

            gui.text = "Both";
        }

        void SetForceAnimatedPose(bool value)
        {
            foreach (RagdollAnimator animator in FindObjectsOfType<RagdollAnimator>())
            {
                animator.forceTargetPose = value;
            }
        }

        void SetCollidersVisible(bool value)
        {
            foreach (RagdollSettings settings in FindObjectsOfType<RagdollSettings>())
            {
                RagdollColliderVisualizer visualizer = settings.GetComponentInChildren<RagdollColliderVisualizer>(true);
                visualizer.enabled = value;
            }

        }

        enum Mode
        {
            Ragdoll,
            Target,
            Both
        }
    }
}
