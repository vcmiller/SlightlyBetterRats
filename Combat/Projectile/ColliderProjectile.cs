using UnityEngine;

namespace SBR {
    /// <summary>
    /// A projectile that uses a Collider to determine when it hits an object.
    /// Should have a Rigidbody with isKinematic set to true in order to work properly.
    /// </summary>
    public class ColliderProjectile : Projectile {
        protected virtual void Update() {
            transform.position += velocity * Time.deltaTime;
        }

        private void OnCollisionEnter(Collision collision) {
            var hit = collision.contacts[0];
            OnHitCollider(collision.collider, hit.point);
        }

        private void OnTriggerEnter(Collider other) {
            OnHitCollider(other, transform.position);
        }
    }
}