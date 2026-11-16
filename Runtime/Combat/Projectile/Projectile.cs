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

using System;
using Infohazard.Core;
using Infohazard.Core.Addressables;
using UnityEngine;
using UnityEngine.Serialization;

namespace SBR {
    public delegate void ProjectileHitCallback(GameObject hitObject, Vector3 position, float damageDealt);

    /// <summary>
    /// Base class for Projectiles.
    /// </summary>
    public class Projectile : MonoBehaviour, IHasCreator {
        /// <summary>
        /// Speed that the projectile is fired at.
        /// </summary>
        [FormerlySerializedAs("launchSpeed")]
        [Tooltip("Speed that the projectile is fired at.")]
        public float _launchSpeed;

        /// <summary>
        /// How much damage to do if the projectile hits an object with a Health component.
        /// </summary>
        [FormerlySerializedAs("damage")]
        [Tooltip("How much damage to do if the projectile hits an object with a Health component.")]
        public float _damage;

        /// <summary>
        /// How much force to apply if the projectile hits an object with a Rigidbody.
        /// </summary>
        [FormerlySerializedAs("impactForce")]
        [Tooltip("How much force to apply if the projectile hits an object with a Rigidbody.")]
        public float _impactForce;

        /// <summary>
        /// Whether the projectile should hit triggers.
        /// </summary>
        [FormerlySerializedAs("triggerInteraction")]
        [Tooltip("Whether the projectile should hit triggers.")]
        public QueryTriggerInteraction _triggerInteraction;

        /// <summary>
        /// Whether the projectile should be able to hit objects before it is fired.
        /// </summary>
        [FormerlySerializedAs("hitsIfNotFired")]
        [Tooltip("Whether the projectile should be able to hit objects before it is fired.")]
        public bool _hitsIfNotFired;

        /// <summary>
        /// How long the projectile should last after it hits, if destroyOnHit is true.
        /// </summary>
        [FormerlySerializedAs("linger")]
        [Tooltip("How long the projectile should last after it hits, if destroyOnHit is true.")]
        public float _linger;

        /// <summary>
        /// Multiplier for Physics.gravity or Physics2D.gravity.
        /// </summary>
        [FormerlySerializedAs("gravity")]
        [Tooltip("Multiplier for Physics.gravity or Physics2D.gravity.")]
        public float _gravity;

        /// <summary>
        /// Whether the projectile should destroy its GameObject on impact.
        /// </summary>
        [FormerlySerializedAs("destroyOnHit")]
        [Tooltip("Whether the projectile should destroy its GameObject on impact.")]
        public bool _destroyOnHit = true;

        [FormerlySerializedAs("destroyAfterTime")]
        public float _destroyAfterTime;

        [FormerlySerializedAs("playDestroyEffectsAfterTime")]
        public bool _playDestroyEffectsAfterTime;

        /// <summary>
        /// Whether the projectile should stop moving when it hits an object.
        /// </summary>
        [FormerlySerializedAs("stopOnHit")]
        [Tooltip("Whether the projectile should stop moving when it hits an object.")]
        public bool _stopOnHit = true;

        /// <summary>
        /// Prefab to spawn on impact, such as an explosion.
        /// </summary>
        [FormerlySerializedAs("impactPrefab")]
        [Tooltip("Prefab to spawn on impact, such as an explosion.")]
        public GameObject _impactPrefab;

        /// <summary>
        /// Whether to align the impact prefab to the hit object's normal.
        /// </summary>
        [FormerlySerializedAs("alignImpactToHit")]
        [Tooltip("Whether to align the impact prefab to the hit object's normal.")]
        public bool _alignImpactToHit = false;

        [FormerlySerializedAs("_impactPrefabAddressable")]
        [SerializeField]
        private AddressableSpawnRef _impactPrefabSpawnRef;

        /// <summary>
        /// Whether to parent the spawned impact object to the hit object.
        /// </summary>
        [FormerlySerializedAs("parentImpactObject")]
        [Tooltip("Whether to parent the spawned impact object to the hit object.")]
        public bool _parentImpactObject;

        /// <summary>
        /// Sound to play on impact.
        /// </summary>
        [FormerlySerializedAs("impactSound")]
        [Tooltip("Sound to play on impact.")]
        public AudioParameters _impactSound;

        /// <summary>
        /// Filters on hit objects.
        /// </summary>
        [FormerlySerializedAs("hitFilters")]
        [Tooltip("Filters on hit objects.")]
        public HitFilter[] _hitFilters;

        private Vector3 _velocity;
        private float _lifetime;
        private bool _hasLifetime;

        /// <summary>
        /// Current velocity of the projectile.
        /// </summary>
        public virtual Vector3 Velocity {
            get => _velocity;
            set => _velocity = value;
        }

        /// <summary>
        /// Whether the projectile has been fired.
        /// </summary>
        public bool Fired { get; protected set; }

        public float DamageMultiplier { get; set; }

        public AddressableSpawnRef ImpactPrefabSpawnRef {
            get => _impactPrefabSpawnRef;
            set {
                if (_impactPrefabSpawnRef == value) return;

                if (_impactPrefabSpawnRef?.IsValid == true) {
                    _impactPrefabSpawnRef.Release();
                }

                _impactPrefabSpawnRef = value;

                if (_impactPrefabSpawnRef?.IsValid == true) {
                    _impactPrefabSpawnRef.Retain();
                }
            }
        }

        /// <summary>
        /// Invoked when the projectile collides with an object.
        /// </summary>
        public event ProjectileHitCallback HitObject;

        protected virtual bool HitsTriggers => _triggerInteraction == QueryTriggerInteraction.Collide ||
                    (_triggerInteraction == QueryTriggerInteraction.UseGlobal && Physics.queriesHitTriggers);

        protected virtual Vector3 GravityVector => _gravity * Physics.gravity;

        public GameObject Creator { get; set; }

        public object Method { get; set; }

        /// <summary>
        /// Fire the projectile in the direction it is currently facing.
        /// </summary>
        public virtual void Fire(GameObject creator, object method = null) {
            Fire(creator, transform.forward, false, method);
        }

        /// <summary>
        /// Fire the projectile in a given direction.
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="direction">Direction in which to fire.</param>
        /// <param name="align">Whether to orient the projectile to the given direction.</param>
        /// <param name="method">Method for dealing the damage, such as a weapon.</param>
        public virtual void Fire(GameObject creator, Vector3 direction, bool align = true, object method = null) {
            Creator = creator;
            Method = method;
            Velocity = direction.normalized * _launchSpeed;
            if (align) {
                transform.forward = direction;
            }
            Fired = true;
            _hasLifetime = _destroyAfterTime > 0;
            _lifetime = _destroyAfterTime;
        }

        protected virtual void OnSpawned() {
            Velocity = Vector3.zero;
            Fired = false;
        }

        protected virtual void OnDespawned() {
            HitObject = null;
            _hasLifetime = false;
        }

        private void Awake() {
            if (_impactPrefabSpawnRef?.IsValid == true) {
                _impactPrefabSpawnRef.Retain();
            }
        }

        private void OnDestroy() {
            if (_impactPrefabSpawnRef?.IsValid == true) {
                _impactPrefabSpawnRef.Release();
            }
        }

        protected virtual void Update() {
            if (!Fired || !_hasLifetime) return;
            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0) {
                DestroyFromTimeout();
            }
        }

        private void DestroyFromTimeout() {
            if (_stopOnHit) {
                Velocity = Vector3.zero;
            }

            Spawnable.Despawn(gameObject, _linger);
            Fired = false;

            if (!_playDestroyEffectsAfterTime) return;

            Transform t = transform;
            if (_impactSound) {
                _impactSound.PlayAtPoint(t.position);
            }

            if (_impactPrefab) {
                GameObject obj = Spawnable.Spawn(_impactPrefab, t.position, t.rotation, null, true, scene: gameObject.scene);
                if (obj.TryGetComponent(out IHasCreator ihc)) {
                    ihc.Creator = Creator;
                    ihc.Method = Method;
                }
            }

            if (_impactPrefabSpawnRef.IsValid) {
                GameObject obj = _impactPrefabSpawnRef.Spawn(SpawnParams.At(t, false, true));
                if (obj.TryGetComponent(out IHasCreator ihc)) {
                    ihc.Creator = Creator;
                    ihc.Method = Method;
                }
            }
        }

        protected virtual bool OnHitCollider(Collider col, Vector3 position, Vector3 normal) {
            if (ShouldHitObject(col.transform, position) && (HitsTriggers || !col.isTrigger)) {
                OnHitObject(col.transform, position, normal);
                return true;
            } else {
                return false;
            }
        }

        protected virtual bool ShouldHitObject(Transform col, Vector3 position) {
            if (!Fired && !_hitsIfNotFired) return false;

            foreach (HitFilter filter in _hitFilters) {
                if (filter && !filter.ShouldHitObject(col, position)) return false;
            }

            return true;
        }

        protected virtual void OnHitObject(Transform col, Vector3 position, Vector3 normal) {
            float damageDealt = col.Damage(new PointDamage(_damage * DamageMultiplier,
                                                           Creator ? Creator : gameObject,
                                                           position,
                                                           Velocity.normalized,
                                                           _impactForce,
                                                           col.gameObject,
                                                           Method));

            HitObject?.Invoke(col.gameObject, position, damageDealt);

            if (_stopOnHit) {
                Velocity = Vector3.zero;
            }

            if (_destroyOnHit) {
                Spawnable.Despawn(gameObject, _linger);
                Fired = false;
            }

            if (_impactSound) {
                _impactSound.PlayAtPoint(transform.position);
            }

            Quaternion impactRotation = _alignImpactToHit ? Quaternion.LookRotation(normal) : transform.rotation;
            if (_impactPrefab) {
                GameObject obj = Spawnable.Spawn(_impactPrefab, position, impactRotation, _parentImpactObject ? col : null,
                                                 true, scene:gameObject.scene);
                if (obj.TryGetComponent(out IHasCreator ihc)) {
                    ihc.Creator = Creator;
                    ihc.Method = Method;
                }
            }

            if (_impactPrefabSpawnRef.IsValid) {
                GameObject obj = _impactPrefabSpawnRef.Spawn(new SpawnParams {
                    Position = position,
                    Rotation = impactRotation,
                    Parent = _parentImpactObject ? col : null,
                    InWorldSpace = true,
                    Scene = gameObject.scene,
                });

                if (obj.TryGetComponent(out IHasCreator ihc)) {
                    ihc.Creator = Creator;
                    ihc.Method = Method;
                }
            }
        }

        [ContextMenu("Add Hit Filters")]
        private void AddHitFilters() {
            _hitFilters = GetComponentsInChildren<HitFilter>();
        }
    }
}
