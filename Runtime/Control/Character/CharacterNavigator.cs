// The MIT License (MIT)
// 
// Copyright (c) 2026-present Val Miller
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using SBR.StateMachines;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace SBR {
    /// <summary>
    /// Base class for StateMachines that need to use the NavMesh for character movement.
    /// </summary>
    /// <typeparam name="T">The channels type.</typeparam>
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class CharacterNavigator<T> : StateMachine<T> where T : CharacterChannels, new() {
        /// <summary>
        /// Transform to follow constantly.
        /// </summary>
        [FormerlySerializedAs("followTarget"),Tooltip("Transform to follow constantly.")]
        public Transform _followTarget;

        /// <summary>
        /// Radius in which to consider the target reached when checking if arrived.
        /// </summary>
        [FormerlySerializedAs("acceptance"),Tooltip("Radius in which to consider the target reached when checking if arrived.")]
        public float _acceptance = 1;

        /// <summary>
        /// NavMeshAgent component reference.
        /// </summary>
        public NavMeshAgent Agent { get; private set; }

        /// <summary>
        /// Desired movement speed. Will be used to multiply Channels.movement value.
        /// </summary>
        public float DesiredSpeed { get; set; }
        
        /// <summary>
        /// Whether the character has reached its destination.
        /// </summary>
        public virtual bool Arrived {
            get {
                if (Agent.pathPending) {
                    return false;
                } else {
                    return !Agent.hasPath || Agent.remainingDistance <= _acceptance;
                }
            }
        }

        protected virtual void OnEnable() {
            Agent = GetComponent<NavMeshAgent>();
            
            if (!Agent) {
                Debug.LogError("Character navigator requires NavMeshAgent component.");
            }

            Agent.updatePosition = false;
            Agent.updateRotation = false;
            Agent.updateUpAxis = false;
            Agent.autoTraverseOffMeshLink = false;
        }

        protected override void DoInput() {
            base.DoInput();

            Agent.nextPosition = transform.position;

            if (_followTarget) {
                Agent.destination = _followTarget.position;
            }

            if (Agent.hasPath) {
                Channels.Movement = Vector3.ClampMagnitude(Agent.desiredVelocity, 1) * DesiredSpeed;

                if (Agent.isOnOffMeshLink && Agent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeJumpAcross) {
                    Channels.Jump = true;
                } 
            }
        }

        /// <summary>
        /// Move to the specified destination then stop.
        /// </summary>
        /// <param name="destination">Destination to move to.</param>
        public virtual void MoveTo(Vector3 destination) {
            Agent.destination = destination;
            _followTarget = null;
        }

        /// <summary>
        /// Continuously move towards the target Transform, never stopping even if it is reached.
        /// </summary>
        /// <param name="destination">The Transform to follow.</param>
        public virtual void MoveTo(Transform destination) {
            Agent.destination = destination.position;
            _followTarget = destination;
        }

        /// <summary>
        /// Stop moving and clear target transform if set.
        /// </summary>
        public virtual void Stop() {
            Agent.destination = Agent.transform.position;
            _followTarget = null;
        }
    }
}
