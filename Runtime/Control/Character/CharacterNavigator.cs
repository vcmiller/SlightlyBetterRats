﻿// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
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
        [Tooltip("Transform to follow constantly.")]
        public Transform followTarget;

        /// <summary>
        /// Radius in which to consider the target reached when checking if arrived.
        /// </summary>
        [Tooltip("Radius in which to consider the target reached when checking if arrived.")]
        public float acceptance = 1;

        /// <summary>
        /// NavMeshAgent component reference.
        /// </summary>
        public NavMeshAgent agent { get; private set; }

        /// <summary>
        /// Desired movement speed. Will be used to multiply Channels.movement value.
        /// </summary>
        public float desiredSpeed { get; set; }
        
        /// <summary>
        /// Whether the character has reached its destination.
        /// </summary>
        public virtual bool arrived {
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

            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.autoTraverseOffMeshLink = false;
        }

        protected override void DoInput() {
            base.DoInput();

            agent.nextPosition = transform.position;

            if (followTarget) {
                agent.destination = followTarget.position;
            }

            if (agent.hasPath) {
                Channels.Movement = Vector3.ClampMagnitude(agent.desiredVelocity, 1) * desiredSpeed;

                if (agent.isOnOffMeshLink && agent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeJumpAcross) {
                    Channels.Jump = true;
                } 
            }
        }

        /// <summary>
        /// Move to the specified destination then stop.
        /// </summary>
        /// <param name="destination">Destination to move to.</param>
        public virtual void MoveTo(Vector3 destination) {
            agent.destination = destination;
            followTarget = null;
        }

        /// <summary>
        /// Continuously move towards the target Transform, never stopping even if it is reached.
        /// </summary>
        /// <param name="destination">The Transform to follow.</param>
        public virtual void MoveTo(Transform destination) {
            agent.destination = destination.position;
            followTarget = destination;
        }

        /// <summary>
        /// Stop moving and clear target transform if set.
        /// </summary>
        public virtual void Stop() {
            agent.destination = agent.transform.position;
            followTarget = null;
        }
    }
}
