using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    public class BasicCharacterController : PlayerController {
        public CharacterChannels character { get; private set; }

        public float pitchMin = -80;
        public float pitchMax = 80;

        private Vector3 angles;

        public override void Initialize() {
            base.Initialize();

            character = channels as CharacterChannels;
        }
        
        public void Axis_Horizontal(float value) {
            Vector3 right = viewTarget ? viewTarget.transform.right : transform.right;
            right.y = 0;
            right = right.normalized;

            character.movement += right * value;
        }

        public void Axis_Vertical(float value) {
            Vector3 fwd = viewTarget ? viewTarget.transform.forward : transform.forward;
            fwd.y = 0;
            fwd = fwd.normalized;

            character.movement += fwd * value;
        }

        public void ButtonDown_Jump() {
            character.jump = true;
        }

        public void BUttonUp_Jump() {
            character.jump = false;
        }

        public void Axis_MouseX(float value) {
            angles.y += value;

            character.rotation = Quaternion.Euler(angles);
        }

        public void Axis_MouseY(float value) {
            angles.x -= value;

            if (angles.x < pitchMin) {
                angles.x = pitchMin;
            } else if (angles.x > pitchMax) {
                angles.x = pitchMax;
            }

            character.rotation = Quaternion.Euler(angles);
        }
    }
}