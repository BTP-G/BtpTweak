﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace BtpTweak.Tweaks.ItemTweaks {

    internal class BleedOnHitAndExplodeTweak : TweakBase<BleedOnHitAndExplodeTweak> {
        public const int BaseRadius = 16;
        public const int StackRadius = 8;
        public const int DamageCoefficient = 4;

        public override void SetEventHandlers() {
            RoR2Application.onLoad += Load;
            IL.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
        }

        public override void ClearEventHandlers() {
            RoR2Application.onLoad -= Load;
            IL.RoR2.GlobalEventManager.OnCharacterDeath -= GlobalEventManager_OnCharacterDeath;
        }

        private void Load() {
            var delayBlast = GlobalEventManager.CommonAssets.bleedOnHitAndExplodeBlastEffect.GetComponent<DelayBlast>();
            delayBlast.baseForce = 0f;
            delayBlast.bonusForce = Vector3.zero;
            delayBlast.damageColorIndex = DamageColorIndex.Item;
            delayBlast.damageType = DamageType.AOE | DamageType.BleedOnHit;
            delayBlast.falloffModel = BlastAttack.FalloffModel.SweetSpot;
            delayBlast.inflictor = null;
            delayBlast.maxTimer = 0.1f;
        }

        private void GlobalEventManager_OnCharacterDeath(ILContext il) {
            ILCursor ilcursor = new(il);
            if (ilcursor.TryGotoNext(MoveType.After,
                                     x => x.MatchLdsfld(typeof(RoR2Content.Items), "BleedOnHitAndExplode"),
                                     x => x.MatchCallvirt<Inventory>("GetItemCount"))) {
                ilcursor.Emit(OpCodes.Ldarg_1);
                ilcursor.Emit(OpCodes.Ldloc_2);
                ilcursor.EmitDelegate((int itemCount, DamageReport damageReport, CharacterBody victimBody) => {
                    if (itemCount > 0 && (victimBody.HasBuff(RoR2Content.Buffs.Bleeding.buffIndex) || victimBody.HasBuff(RoR2Content.Buffs.SuperBleed.buffIndex))) {
                        Util.PlaySound("Play_bleedOnCritAndExplode_explode", victimBody.gameObject);
                        GameObject bleedExplode = Object.Instantiate(GlobalEventManager.CommonAssets.bleedOnHitAndExplodeBlastEffect, victimBody.corePosition, Quaternion.identity);
                        bleedExplode.GetComponent<TeamFilter>().teamIndex = damageReport.attackerTeamIndex;
                        DelayBlast delayBlast = bleedExplode.GetComponent<DelayBlast>();
                        delayBlast.attacker = damageReport.attacker;
                        delayBlast.baseDamage = Util.OnKillProcDamage(damageReport.attackerBody.damage, DamageCoefficient * itemCount);
                        delayBlast.crit = damageReport.attackerBody.RollCrit();
                        delayBlast.position = damageReport.damageInfo.position;
                        delayBlast.procCoefficient = damageReport.damageInfo.procCoefficient;
                        delayBlast.radius = BaseRadius + StackRadius * (itemCount - 1) + 1.6f * victimBody.bestFitRadius;
                        NetworkServer.Spawn(bleedExplode);
                    }
                });
                ilcursor.Emit(OpCodes.Ldc_I4_0);
            } else {
                Main.Logger.LogError("BleedOnHitAndExplode :: Hook Failed!");
            }
        }
    }
}