﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;

namespace BTP.RoR2Plugin.Tweaks.ItemTweaks {

    internal class ParentEggTweak : ModComponent, IModLoadMessageHandler {
        public const float HealFractionFromDamage = 0.01f;
        public const float HealAmount = 20f;

        void IModLoadMessageHandler.Handle() {
            IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(ILContext il) {
            var ilcursor = new ILCursor(il);
            if (ilcursor.TryGotoNext(MoveType.After,
                                     x => x.MatchLdarg(0),
                                     x => x.MatchLdflda<HealthComponent>("itemCounts"),
                                     x => x.MatchLdfld(typeof(HealthComponent.ItemCounts), "parentEgg"))) {
                ilcursor.Emit(OpCodes.Ldarg_0);
                ilcursor.Emit(OpCodes.Ldloc, 7);
                ilcursor.EmitDelegate((int itemCount, HealthComponent healthComponent, float damage) => {
                    healthComponent.Heal(itemCount * (damage * HealFractionFromDamage + HealAmount), default, true);
                });
                ilcursor.Emit(OpCodes.Ldc_I4_0);
            } else {
                LogExtensions.LogError("ParentEgg hook error");
            }
        }
    }
}