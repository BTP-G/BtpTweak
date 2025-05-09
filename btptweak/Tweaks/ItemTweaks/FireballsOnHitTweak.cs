﻿using BtpTweak.Pools.ProjectilePools;
using BtpTweak.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Projectile;

namespace BtpTweak.Tweaks.ItemTweaks {

    internal class FireballsOnHitTweak : TweakBase<FireballsOnHitTweak>, IOnModLoadBehavior, IOnRoR2LoadedBehavior {
        public const float 半数 = 9;
        public const float DamageCoefficient = 0.6f;
        public const float Interval = 0.3f;

        void IOnModLoadBehavior.OnModLoad() {
            IL.RoR2.GlobalEventManager.ProcessHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        void IOnRoR2LoadedBehavior.OnRoR2Loaded() {
            var projectileController = AssetReferences.fireMeatBallProjectile.GetComponent<ProjectileController>();
            projectileController.procCoefficient = 1f;
            var projectileImpactExplosion = AssetReferences.fireMeatBallProjectile.GetComponent<ProjectileImpactExplosion>();
            projectileImpactExplosion.blastProcCoefficient = 0.33f;
            projectileImpactExplosion.blastRadius = 10;
        }

        private void GlobalEventManager_OnHitEnemy(ILContext il) {
            var ilcursor = new ILCursor(il);
            if (ilcursor.TryGotoNext(MoveType.After,
                                     x => x.MatchLdsfld(typeof(RoR2Content.Items).GetField("FireballsOnHit")),
                                     x => x.MatchCallvirt<Inventory>("GetItemCount"))) {
                ilcursor.Emit(OpCodes.Ldarg_1)
                        .Emit(OpCodes.Ldloc, 5)
                        .Emit(OpCodes.Ldloc_2)
                        .EmitDelegate((int itemCount, DamageInfo damageInfo, CharacterMaster attackerMaster, CharacterBody victimBody) => {
                            if (itemCount > 0
                            && !damageInfo.procChainMask.HasProc(ProcType.Meatball)
                            && Util.CheckRoll(BtpUtils.简单逼近(itemCount, 半数, 100f * damageInfo.procCoefficient), attackerMaster)) {
                                var simpleProjectileInfo = new ProjectilePoolKey {
                                    attacker = damageInfo.attacker,
                                    isCrit = damageInfo.crit,
                                    procChainMask = damageInfo.procChainMask,
                                    targetBody = victimBody,
                                };
                                FireFountain.RentPool(victimBody.gameObject).AddProjectile(simpleProjectileInfo,
                                                                                           Util.OnHitProcDamage(damageInfo.damage, 0, DamageCoefficient * itemCount));
                            }
                        });
                ilcursor.Emit(OpCodes.Ldc_I4_0);
            } else {
                Main.Logger.LogError("FireballsOnHit :: Hook Failed!");
            }
        }
    }
}