using UnityEngine;
using UnityEngine.AI;

namespace SBR {
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class CharacterNavigator : StateMachine<CharacterChannels> {
        public Transform target;
        
        public NavMeshAgent agent { get; private set; }

        public float acceptance = 1;
        
        public bool arrived {
            get {
                if (agent.pathPending) {
                    return false;
                } else {
                    return !agent.hasPath || agent.remainingDistance <= acceptance;
                }
            }
        }

        protected virtual void OnEnable() {
            agent = GetComponent<NavMeshAgent>();
            
            if (!agent) {
                Debug.LogError("Character navigator requires NavMeshAgent component.");
            }

            agent.updatePosition = agent.updateRotation = agent.updateUpAxis = false;
        }

        public override void GetInput() {
            base.GetInput();

            agent.nextPosition = transform.position;

            if (target) {
                agent.destination = target.position;
            } else if (arrived) {
                Stop();
            }

            if (agent.hasPath) {
                channels.movement = agent.desiredVelocity;
                
                if (agent.isOnOffMeshLink && agent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeJumpAcross) {
                    channels.jump = true;
                } 
            }
        }

        public void MoveTo(Vector3 destination) {
            agent.destination = destination;
            target = null;
        }

        public void MoveTo(Transform destination) {
            agent.destination = destination.position;
            target = destination;
        }

        public void Stop() {
            agent.destination = agent.transform.position;
            target = null;
        }
    }
}
