using SBR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    public class DamageModifierPoint : MonoBehaviour, IDamageable {
        public float modifier = 1;

        private IParentDamageable parent;

        private void Awake() {
            parent = GetComponentInParent<IParentDamageable>();
        }

        public float Damage(Damage dmg) {
            dmg.amount *= modifier;
            return parent.Damage(dmg);
        }
    }

}