﻿using BtpTweak.Utils;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace BtpTweak.MissilePools {

    [RequireComponent(typeof(CharacterBody))]
    public class MissilePool : MonoBehaviour {
        private readonly Dictionary<MissileInfo, Missile> _pool = new();
        private CharacterBody _attackerBody;
        private float _missileTimer;

        public void AddMissile(in MissileInfo missileInfo, float damageValue) {
            if (_pool.TryGetValue(missileInfo, out var missile)) {
                missile.baseDamage += damageValue;
                missile.target ??= missileInfo.target;
            } else {
                _pool.Add(missileInfo, new() {
                    baseDamage = damageValue,
                    isCrit = missileInfo.isCrit,
                    missilePrefab = missileInfo.missilePrefab,
                    procChainMask = missileInfo.procChainMask,
                    target = missileInfo.target,
                });
            }
        }

        private void Awake() {
            if (enabled = NetworkServer.active) {
                _attackerBody = GetComponent<CharacterBody>();
            }
        }

        private void FireMissile(Missile missile) {
            MissileUtils.FireMissile(_attackerBody.corePosition, _attackerBody, missile.procChainMask, missile.target, missile.baseDamage, missile.isCrit, missile.missilePrefab, DamageColorIndex.Item, Vector3.up, 0f, true);
        }

        private void FixedUpdate() {
            if ((_missileTimer -= Time.fixedDeltaTime) < 0) {
                if (_pool.Count > 0) {
                    foreach (var kvp in _pool) {
                        FireMissile(kvp.Value);
                    }
                    _pool.Clear();
                    _missileTimer = ModConfig.导弹发射间隔.Value;
                }
            }
        }

        private void OnDestroy() => _pool.Clear();

        public struct MissileInfo {
            public GameObject missilePrefab;
            public GameObject target;
            public ProcChainMask procChainMask;
            public bool isCrit;
        }

        private class Missile {
            public GameObject missilePrefab;
            public GameObject target;
            public ProcChainMask procChainMask;
            public float baseDamage;
            public bool isCrit;
        }
    }
}