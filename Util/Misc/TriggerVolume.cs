using System;
using UnityEngine;
using UnityEngine.Events;

namespace SBR {
    public class TriggerVolume : MonoBehaviour {
        [MultiEnum]
        public Tag tagFilter = Tag.Player;
        public bool hideOnPlay = true;

        public event Action<GameObject> TriggerEntered;
        public event Action<GameObject> TriggerExited;

        [SerializeField]
        private Events events;

        [Serializable]
        public struct Events {
            public UnityEvent OnTriggerEnter;
            public UnityEvent OnTriggerExit;
        }

        private void Start() {
            if (hideOnPlay) {
                GetComponent<MeshRenderer>().enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag(tagFilter)) {
                events.OnTriggerEnter?.Invoke();
                TriggerEntered?.Invoke(other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.CompareTag(tagFilter)) {
                events.OnTriggerExit?.Invoke();
                TriggerExited?.Invoke(other.gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag(tagFilter)) {
                events.OnTriggerEnter?.Invoke();
                TriggerEntered?.Invoke(other.gameObject);
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag(tagFilter)) {
                events.OnTriggerExit?.Invoke();
                TriggerExited?.Invoke(other.gameObject);
            }
        }
    }

}