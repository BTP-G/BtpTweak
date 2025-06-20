﻿using BTP.RoR2Plugin.Utils;
using EntityStates.ImpBossMonster;
using RoR2;
using RoR2.Skills;

namespace BTP.RoR2Plugin.Tweaks.MonsterTweaks {

    internal class ImpBossTweak : ModComponent, IModLoadMessageHandler, IRoR2LoadedMessageHandler {

        void IModLoadMessageHandler.Handle() {
            On.EntityStates.ImpBossMonster.BlinkState.OnEnter += BlinkState_OnEnter;
        }

        void IRoR2LoadedMessageHandler.Handle() {
            SkillDefPaths.ImpBossBodyFireVoidspikes.Load<SkillDef>().baseMaxStock = 2;
            FireVoidspikes.projectileCount = 12;  // 6
            FireVoidspikes.projectileYawSpread /= 2f;
            FireVoidspikes.projectileSpeedPerProjectile = 0f;
            GroundPound.blastAttackRadius *= 1.25f;
            var body = GameObjectPaths.ImpBossBody4.LoadComponent<CharacterBody>();
            body.baseAcceleration = 30f;  // 15f
            body.baseMoveSpeed = 10f;  // 15f
            body.baseAttackSpeed = 1.5f;  // 1
            body.baseMaxHealth *= 1.5f;  // 1
            body.levelMaxHealth *= 1.5f;  // 1
        }

        private void BlinkState_OnEnter(On.EntityStates.ImpBossMonster.BlinkState.orig_OnEnter orig, BlinkState self) {
            orig(self);
            self.duration *= 0.6f;
            self.exitDuration *= 0.5f;
        }
    }
}