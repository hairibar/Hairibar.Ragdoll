using Hairibar.EngineExtensions;
using UnityEngine;

namespace Hairibar.Ragdoll.Demo
{
    public class BallShooter : MonoBehaviour
    {
        const string LAYER_NAME = "Balls";

        public float radius = 0.2f;
        public float force = 10;
        public float mass = 100;

        private Rigidbody ball;
        private new Transform transform;
        private new Camera camera;

        private void Update()
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKey(KeyCode.Space))
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            if (!ball) CreateBall();

            ball.transform.localScale = new Vector3(radius, radius, radius);
            ball.mass = mass;

            ball.transform.position = transform.position;
            ball.velocity = camera.ScreenPointToRay(Input.mousePosition).direction.normalized * force;
        }

        private void CreateBall()
        {
            GameObject go = PrimitiveHelper.CreatePrimitiveGameObject(PrimitiveType.Sphere, true);
            ball = go.AddComponent<Rigidbody>();
            go.layer = LayerMask.NameToLayer(LAYER_NAME);
            ball.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            go.transform.SetParent(transform.parent);
        }

        private void Awake()
        {
            transform = GetComponent<Transform>();
            camera = GetComponent<Camera>();
        }
    }

}