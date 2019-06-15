using UnityEngine;

namespace SBR {
    /// <summary>
    /// A projectile that uses a Collider2D to determine when it hits an object.
    /// Should have a kinematic Rigidbody2D in order to work properly.
    /// </summary>
    public class ColliderProjectile2D : Projectile2D {
        protected virtual void Update() {
            transform.position += velocity * Time.deltaTime;
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            var hit = collision.contacts[0];
            OnHitCollider2D(collision.collider, hit.point);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            OnHitCollider2D(other, transform.position);
        }
    }
}