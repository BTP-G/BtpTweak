﻿using BtpTweak.Tweaks.ItemTweaks;
using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace BtpTweak.OrbPools {

    [RequireComponent(typeof(HealthComponent))]
    internal class ThornsOrbPool : OrbPool<SimpleOrbInfo, LightningOrb> {
        private HealthComponent _healthComponent;
        protected override float OrbInterval => 0.1f;
        private HealthComponent HealthComponent => _healthComponent ??= GetComponent<HealthComponent>();

        public void AddOrb(in SimpleOrbInfo simpleOrbInfo, DamageReport damageReport) {
            int itemCount = HealthComponent.itemCounts.thorns;
            if (Pool.TryGetValue(simpleOrbInfo, out var lightningOrb)) {
                lightningOrb.damageValue += damageReport.damageDealt * itemCount;
                lightningOrb.damageType |= damageReport.damageInfo.damageType;
                if (!lightningOrb.target && damageReport.attackerTeamIndex != lightningOrb.teamIndex) {
                    lightningOrb.target = damageReport.attackerBody?.mainHurtBox;
                }
            } else {
                lightningOrb = new() {
                    attacker = simpleOrbInfo.attacker,
                    bouncesRemaining = 0,
                    damageCoefficientPerBounce = 1f,
                    damageColorIndex = DamageColorIndex.Item,
                    damageType = damageReport.damageInfo.damageType,
                    damageValue = damageReport.damageDealt * itemCount,
                    isCrit = simpleOrbInfo.isCrit,
                    lightningType = LightningOrb.LightningType.RazorWire,
                    procCoefficient = HealthComponent.itemCounts.invadingDoppelganger > 0 ? 0 : 0.5f,
                    range = ThornsTweak.Radius * itemCount,
                    teamIndex = HealthComponent.body.teamComponent.teamIndex,
                };
                if (damageReport.attackerTeamIndex != lightningOrb.teamIndex) {
                    lightningOrb.target = damageReport.attackerBody?.mainHurtBox;
                }
                Pool.Add(simpleOrbInfo, lightningOrb);
            }
        }

        protected override void ModifyOrb(ref LightningOrb orb) {
            orb.origin = orb.attacker.transform.position;
            orb.target ??= orb.PickNextTarget(orb.origin);
            orb.procChainMask.AddProc(ProcType.Thorns);
        }
    }
}