﻿using RoR2;
using UnityEngine;

namespace BtpTweak.Tweaks {

    internal class ShrineCombatTweak : TweakBase<ShrineCombatTweak> {
        private float 战斗祭坛物品掉落数_;

        public override void SetEventHandlers() {
            ShrineCombatBehavior.onDefeatedServerGlobal += ShrineCombatBehavior_onDefeatedServerGlobal;
            Stage.onStageStartGlobal += Stage_onStageStartGlobal;
        }

        public override void ClearEventHandlers() {
            ShrineCombatBehavior.onDefeatedServerGlobal -= ShrineCombatBehavior_onDefeatedServerGlobal;
            Stage.onStageStartGlobal -= Stage_onStageStartGlobal;
        }

        public void Stage_onStageStartGlobal(Stage stage) {
            战斗祭坛物品掉落数_ = Mathf.Min((Run.instance.stageClearCount + 1) * 0.5f, 5) * Run.instance.participatingPlayerCount;
        }

        private void ShrineCombatBehavior_onDefeatedServerGlobal(ShrineCombatBehavior shrine) {
            if (战斗祭坛物品掉落数_ > 0) {
                PickupIndex pickupIndex = Run.instance.treasureRng.NextElementUniform(Run.instance.availableTier1DropList);
                Vector3 pos = shrine.transform.position + (6 * Vector3.up);
                Vector3 velocity = Quaternion.AngleAxis(Random.Range(0, 360f), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 2.5f);
                Quaternion rotation = Quaternion.AngleAxis(360f / 战斗祭坛物品掉落数_, Vector3.up);
                for (int i = 0; i < 战斗祭坛物品掉落数_; ++i) {
                    PickupDropletController.CreatePickupDroplet(pickupIndex, pos, velocity);
                    velocity = rotation * velocity;
                }
                战斗祭坛物品掉落数_ *= 0.5f;
            }
        }
    }
}