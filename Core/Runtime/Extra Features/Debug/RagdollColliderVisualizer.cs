using System.Collections.Generic;
using Hairibar.EngineExtensions;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hairibar.Ragdoll.Debug
{
    /// <summary>
    /// Draws the ragdoll's colliders as meshes. Not suitable for Release.
    /// </summary>
    [AddComponentMenu("Ragdoll/Ragdoll Collider Visualizer")]
    [ExecuteAlways, RequireComponent(typeof(RagdollDefinitionBindings), typeof(RagdollSettings))]
    public class RagdollColliderVisualizer : MonoBehaviour
    {
        static Material poweredMaterial;
        static Material kinematicMaterial;
        static Material unpoweredMaterial;

        #region Private State
        ColliderData[] colliderData;

        RagdollSettings settings;
        RagdollDefinitionBindings bindings;
        #endregion

        #region Visualization
        void Update()
        {
            if (colliderData == null || colliderData.Length == 0) return;
            if (!bindings.IsInitialized) return;

            DrawColliders();
        }

        void DrawColliders()
        {
            foreach (ColliderData data in colliderData)
            {
                Vector3 translation = Vector3.zero;
                Quaternion rotation = Quaternion.identity;
                Vector4 scale = new Vector4(1, 1, 1, 0);

                ApplyTransformations(data, ref translation, ref rotation, ref scale);
                ApplyWorldScale(ref scale, data.transform.lossyScale);

                Matrix4x4 matrix = Matrix4x4.identity;
                matrix = Matrix4x4.Translate(translation) * Matrix4x4.Rotate(rotation) * Matrix4x4.Scale(scale) * matrix;

                Material material = GetMaterial(data);

                Mesh mesh = PrimitiveHelper.GetPrimitiveMesh(data.primitive);
                Graphics.DrawMesh(mesh, matrix, material, 0, null);
            }
        }

        static void ApplyTransformations(ColliderData data, ref Vector3 translation, ref Quaternion rotation, ref Vector4 scale)
        {
            switch (data.primitive)
            {
                case PrimitiveType.Sphere:
                    ApplySphereTransformations(data.collider as SphereCollider, data.transform,
                        ref translation, ref rotation, ref scale);
                    break;
                case PrimitiveType.Capsule:
                    ApplyCapsuleTransformations(data.collider as CapsuleCollider, data.transform,
                        ref translation, ref rotation, ref scale);
                    break;
                case PrimitiveType.Cube:
                    ApplyCubeTransformations(data.collider as BoxCollider, data.transform,
                        ref translation, ref rotation, ref scale);
                    break;
                case PrimitiveType.Cylinder:
                case PrimitiveType.Plane:
                case PrimitiveType.Quad:
                    break;
            }
        }

        static void ApplyCapsuleTransformations(CapsuleCollider capsuleCollider, Transform transform,
            ref Vector3 translation, ref Quaternion rotation, ref Vector4 scale)
        {
            rotation = Quaternion.identity;

            if (capsuleCollider.direction == 0) rotation = transform.rotation * Quaternion.Euler(0, 0, 90);
            else if (capsuleCollider.direction == 1) rotation = transform.rotation * Quaternion.Euler(0, 180, 0);
            else if (capsuleCollider.direction == 2) rotation = transform.rotation * Quaternion.Euler(90, 0, 0);

            scale = new Vector4
            {
                y = capsuleCollider.height / 2,
                x = capsuleCollider.radius * 2,
                z = capsuleCollider.radius * 2
            };

            translation = transform.position + transform.TransformDirection(capsuleCollider.center);
        }

        static void ApplySphereTransformations(SphereCollider sphereCollider, Transform transform,
            ref Vector3 translation, ref Quaternion rotation, ref Vector4 scale)
        {
            rotation = transform.rotation;

            scale *= sphereCollider.radius * 2;

            translation = transform.position + transform.TransformDirection(sphereCollider.center);
        }

        static void ApplyCubeTransformations(BoxCollider boxCollider, Transform transform,
            ref Vector3 translation, ref Quaternion rotation, ref Vector4 scale)
        {
            rotation = transform.rotation;

            Vector3 size = boxCollider.size;
            scale.x = size.x;
            scale.y = size.y;
            scale.z = size.z;

            translation = transform.position + transform.TransformDirection(boxCollider.center);
        }

        static void ApplyWorldScale(ref Vector4 scale, Vector3 worldScale)
        {
            scale.x *= worldScale.x;
            scale.y *= worldScale.y;
            scale.z *= worldScale.z;
        }

        Material GetMaterial(ColliderData data)
        {
            PowerSetting powerSetting;
            try
            {
                powerSetting = settings.PowerProfile?.GetBoneSetting(data.boneName) ?? PowerSetting.Unpowered;
            }
            catch (InvalidRagdollProfileException)
            {
                powerSetting = PowerSetting.Unpowered;
            }

            switch (powerSetting)
            {
                case PowerSetting.Kinematic:
                    return kinematicMaterial;
                case PowerSetting.Powered:
                    return poweredMaterial;
                case PowerSetting.Unpowered:
                    return unpoweredMaterial;
                default:
                    return null;
            }
        }
        #endregion

        #region Initialization
        void OnEnable()
        {
            settings = GetComponent<RagdollSettings>();
            bindings = GetComponent<RagdollDefinitionBindings>();

            bindings.UnsubscribeFromOnBonesCreated(GatherColliders);
            bindings.SubscribeToOnBonesCreated(GatherColliders);

            EnsureMaterials();
        }

        static void EnsureMaterials()
        {
            if (!kinematicMaterial)
            {
                kinematicMaterial = CreateMaterial(PowerSetting.Kinematic.GetVisualizationColor());
            }
            if (!poweredMaterial)
            {
                poweredMaterial = CreateMaterial(PowerSetting.Powered.GetVisualizationColor());
            }
            if (!unpoweredMaterial)
            {
                unpoweredMaterial = CreateMaterial(PowerSetting.Unpowered.GetVisualizationColor());
            }


            Material CreateMaterial(Color color)
            {
                Shader defaultShader = GraphicsSettings.currentRenderPipeline ? GraphicsSettings.currentRenderPipeline.defaultShader : Shader.Find("Standard");

                return new Material(defaultShader)
                {
                    color = color
                };
            }
        }

        void GatherColliders()
        {
            List<Collider> foundColliders = new List<Collider>();

            foreach (Collider collider in GetComponentsInChildren<Collider>(true))
            {
                if (collider is CapsuleCollider || collider is SphereCollider || collider is BoxCollider)
                {
                    foundColliders.Add(collider);
                }
            }

            List<ColliderData> dataList = new List<ColliderData>();
            foreach (Collider collider in foundColliders)
            {
                ConfigurableJoint bindingJoint = collider.transform.GetComponentInParent<ConfigurableJoint>();
                bool isPartOfABone = bindings.TryGetBoundBoneName(bindingJoint, out BoneName boneName);
                if (!isPartOfABone) continue;

                ColliderData data = new ColliderData
                {
                    collider = collider,
                    transform = collider.transform,
                    boneName = boneName
                };

                System.Type type = collider.GetType();

                if (type == typeof(CapsuleCollider)) data.primitive = PrimitiveType.Capsule;
                else if (type == typeof(SphereCollider)) data.primitive = PrimitiveType.Sphere;
                else if (type == typeof(BoxCollider)) data.primitive = PrimitiveType.Cube;

                dataList.Add(data);
            }

            colliderData = dataList.ToArray();
        }
        #endregion


        struct ColliderData
        {
            public Collider collider;
            public PrimitiveType primitive;
            public Transform transform;
            public BoneName boneName;
        }
    }
}