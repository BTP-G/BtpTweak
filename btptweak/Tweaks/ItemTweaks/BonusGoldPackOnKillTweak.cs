﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace BtpTweak.Tweaks.ItemTweaks {

    internal class BonusGoldPackOnKillTweak : TweakBase<BonusGoldPackOnKillTweak> {
        public const float DropChance = 0.05f;
        public const int StackMoney = 5;

        public override void SetEventHandlers() {
            IL.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
        }

        public override void ClearEventHandlers() {
            IL.RoR2.GlobalEventManager.OnCharacterDeath -= GlobalEventManager_OnCharacterDeath;
        }

        private void GlobalEventManager_OnCharacterDeath(ILContext il) {
            ILCursor ilcursor = new(il);
            if (ilcursor.TryGotoNext(MoveType.After,
                                     x => x.MatchLdsfld(typeof(RoR2Content.Items).GetField("BonusGoldPackOnKill")),
                                     x => x.MatchCallvirt<Inventory>("GetItemCount"))) {
                ilcursor.Emit(OpCodes.Ldloc, 15);
                ilcursor.Emit(OpCodes.Ldloc, 18);
                ilcursor.Emit(OpCodes.Ldloc, 6);
                ilcursor.EmitDelegate((int itemCount, CharacterBody attacterBody, TeamIndex attacterTeamindex, Vector3 pos) => {
                    if (itemCount > 0 && Util.CheckRoll(DropChance * itemCount, attacterBody.master)) {
                        var bonusMoneyPack = Object.Instantiate(AssetReferences.bonusMoneyPack, pos, Random.rotation);
                        var gravitatePickup = bonusMoneyPack.GetComponentInChildren<GravitatePickup>();
                        if (gravitatePickup) {
                            gravitatePickup.gravitateTarget = attacterBody.coreTransform;
                            gravitatePickup.teamFilter.teamIndex = attacterTeamindex;
                        }
                        var moneyPickup = bonusMoneyPack.GetComponentInChildren<MoneyPickup>();
                        if (moneyPickup) {
                            moneyPickup.goldReward += Run.instance.GetDifficultyScaledCost(StackMoney * itemCount);
                        }
                        NetworkServer.Spawn(bonusMoneyPack);
                    }
                });
                ilcursor.Emit(OpCodes.Ldc_I4_0);
            } else {
                Main.Logger.LogError("BonusGoldPackOnKill Hook Failed!");
            }
        }
    }
}