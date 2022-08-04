using UnityEngine;

namespace SBR {
    public abstract class HitFilter : MonoBehaviour {
        public abstract bool ShouldHitObject(Transform col, Vector3 position);
    }
}