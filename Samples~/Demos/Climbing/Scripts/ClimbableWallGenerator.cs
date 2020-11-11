using UnityEngine;
using NaughtyAttributes;

namespace Hairibar.Ragdoll.Demo.Climbing
{
    public class ClimbableWallGenerator : MonoBehaviour
    {
        public Transform grabbablePrefab;
        public float width;
        public float height;
        public float radius;

        private Transform childrenHolder;

        [Button("Regenerate")]
        private void Generate()
        {
            CleanUp();

            Transform grabbable;
            PoissonDiscSampler sampler = new PoissonDiscSampler(width, height, radius);
            foreach (Vector2 sample in sampler.Samples())
            {
                grabbable = Instantiate(grabbablePrefab, childrenHolder);
                grabbable.localPosition = sample;
            }
        }

        private void CleanUp()
        {
            foreach (Transform child in childrenHolder)
            {
                Destroy(child.gameObject);
            }
        }

        private void Awake()
        {
            childrenHolder = new GameObject().GetComponent<Transform>();
            childrenHolder.SetParent(GetComponent<Transform>());
            childrenHolder.localPosition = Vector3.zero;
            childrenHolder.localRotation = Quaternion.identity;
            childrenHolder.localScale = Vector3.one;

            Generate();
        }
    }
}

