﻿using BTP.RoR2Plugin.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;

namespace BTP.RoR2Plugin.Tweaks.ItemTweaks {

    internal class ExplodeOnDeathTweak : TweakBase<ExplodeOnDeathTweak>, IOnModLoadBehavior, IOnRoR2LoadedBehavior {
        public const int BaseRadius = 12;
        public const int StackRadius = 4;
        public const float 半数 = 3f;

        public static readonly GameObject explosionEffect = GameObjectPaths.WilloWispExplosion.Load<GameObject>();

        private static readonly BlastAttack _blastAttack = new() {
            crit = false,
            baseForce = 3000f,
            bonusForce = new Vector3(0f, 1500f, 0f),
            damageColorIndex = DamageColorIndex.Item,
            damageType = DamageType.AOE,
            falloffModel = BlastAttack.FalloffModel.Linear,
            procCoefficient = 0,
        };

        void IOnModLoadBehavior.OnModLoad() {
            IL.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
        }

        void IOnRoR2LoadedBehavior.OnRoR2Loaded() {
            var delayBlast = GlobalEventManager.CommonAssets.explodeOnDeathPrefab.GetComponent<DelayBlast>();
            delayBlast.baseForce = 3000f;
            delayBlast.bonusForce = Vector3.up * 1500f;
            delayBlast.damageColorIndex = DamageColorIndex.Item;
            delayBlast.damageType = DamageType.AOE;
            delayBlast.falloffModel = BlastAttack.FalloffModel.SweetSpot;
            delayBlast.procCoefficient = 0f;
            delayBlast.inflictor = null;
            delayBlast.maxTimer = 0.5f;
        }

        private void GlobalEventManager_OnCharacterDeath(ILContext il) {
            var ilcursor = new ILCursor(il);
            if (ilcursor.TryGotoNext(MoveType.After,
                                     x => x.MatchLdsfld(typeof(RoR2Content.Items).GetField("ExplodeOnDeath")),
                                     x => x.MatchCallvirt<Inventory>("GetItemCount"))) {
                ilcursor.Emit(OpCodes.Ldarg_1)
                        .Emit(OpCodes.Ldloc_2)
                        .EmitDelegate((int itemCount, DamageReport damageReport, CharacterBody victimBody) => {
                            if (itemCount > 0 && (victimBody.HasBuff(DLC1Content.Buffs.StrongerBurn.buffIndex) || victimBody.HasBuff(RoR2Content.Buffs.OnFire.buffIndex))) {
                                var baseDamage = 0f;
                                var totalDamage = 0f;
                                foreach (var dotStack in DotController.FindDotController(victimBody.gameObject).dotStackList) {
                                    if (dotStack.dotIndex == DotController.DotIndex.Burn || dotStack.dotIndex == DotController.DotIndex.StrongerBurn) {
                                        baseDamage += dotStack.damage;
                                        totalDamage += dotStack.damage * Mathf.Ceil(dotStack.timer / dotStack.dotDef.interval);
                                    }
                                }
                                var damageCoefficient = BtpUtils.简单逼近1(itemCount, 半数);
                                _blastAttack.attacker = damageReport.attacker;
                                _blastAttack.baseDamage = Util.OnKillProcDamage(baseDamage, damageCoefficient);
                                _blastAttack.position = damageReport.damageInfo.position;
                                _blastAttack.radius = BaseRadius + StackRadius * (itemCount - 1) + 1.2f * victimBody.bestFitRadius;
                                _blastAttack.teamIndex = damageReport.attackerTeamIndex;
                                var result = _blastAttack.Fire();
                                EffectManager.SpawnEffect(explosionEffect, new EffectData {
                                    origin = _blastAttack.position,
                                    scale = _blastAttack.radius,
                                }, true);
                                if (result.hitCount > 0) {
                                    var baseDotInfo = new InflictDotInfo {
                                        attackerObject = damageReport.attacker,
                                        damageMultiplier = damageCoefficient * itemCount,
                                        dotIndex = DotController.DotIndex.Burn,
                                        totalDamage = Util.OnKillProcDamage(totalDamage, damageCoefficient),
                                    };
                                    StrengthenBurnUtils.CheckDotForUpgrade(damageReport.attackerBody.inventory, ref baseDotInfo);
                                    foreach (var hitPoint in result.hitPoints) {
                                        var dotInfo = baseDotInfo;
                                        dotInfo.victimObject = hitPoint.hurtBox.healthComponent.gameObject;
                                        var reduceCoefficient = 1f - Mathf.Clamp01(Mathf.Sqrt(hitPoint.distanceSqr) / _blastAttack.radius);
                                        dotInfo.damageMultiplier *= reduceCoefficient;
                                        dotInfo.totalDamage *= reduceCoefficient;
                                        DotController.InflictDot(ref dotInfo);
                                    }
                                }
                            }
                        });
                ilcursor.Emit(OpCodes.Ldc_I4_0);
            } else {
                LogExtensions.LogError("ExplodeOnDeath :: Hook Failed!");
            }
        }
    }
}