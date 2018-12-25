using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    public class Projectile2D : Projectile {
        protected override bool hitsTriggers => 
            triggerInteraction == QueryTriggerInteraction.Collide ||
            (triggerInteraction == QueryTriggerInteraction.UseGlobal && Physics2D.queriesHitTriggers);

        protected virtual void OnHitCollider2D(Collider2D col, Vector2 position) {
            if (hitsTriggers || !col.isTrigger) {
                OnHitObject(col.transform, position);
            }
        }

        protected override Vector3 gravityVector => gravity * Physics2D.gravity;
    }
}