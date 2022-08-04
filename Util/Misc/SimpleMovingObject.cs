using UnityEngine;

namespace SBR {
    public class SimpleMovingObject : MonoBehaviour {
        [SerializeField] private Vector3 _velocity = Vector3.zero;
        [SerializeField] private Vector3 _acceleration = Vector3.zero;

        public Vector3 Velocity {
            get => _velocity;
            set => _velocity = value;
        }

        public Vector3 Acceleration {
            get => _acceleration;
            set => _acceleration = value;
        }

        private void Update() {
            Vector3 dv = _acceleration * Time.deltaTime;

            transform.position += (_velocity + dv * 0.5f) * Time.deltaTime;
            _velocity += dv;
        }
    }

}