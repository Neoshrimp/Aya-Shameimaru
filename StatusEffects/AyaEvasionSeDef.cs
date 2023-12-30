﻿using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using static AyaShameimaru.BepinexPlugin;
using UnityEngine;
using LBoL.Core;
using LBoL.Base;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;
using HarmonyLib;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace AyaShameimaru.StatusEffects
{
    public sealed class AyaEvasionSeDef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(AyaEvasionSe);
        }

        public override LocalizationOption LoadLocalization()
        {
            var gl = new GlobalLocalization(directorySource);
            gl.DiscoverAndLoadLocFiles(this);
            return gl;
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("Resources.AyaEvasionSe.png", embeddedSource);
        }
        public override StatusEffectConfig MakeConfig()
        {
            var statusEffectConfig = new StatusEffectConfig(
                Index: sequenceTable.Next(typeof(StatusEffectConfig)),
                Id: "",
                Order: 1,
                Type: StatusEffectType.Positive,
                IsVerbose: true,
                IsStackable: true,
                StackActionTriggerLevel: null,
                HasLevel: true,
                LevelStackType: StackType.Add,
                HasDuration: false,
                DurationStackType: StackType.Add,
                DurationDecreaseTiming: DurationDecreaseTiming.Custom,
                HasCount: false,
                CountStackType: StackType.Keep,
                LimitStackType: StackType.Keep,
                ShowPlusByLimit: false,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { "Graze" },
                VFX: "Graze",
                VFXloop: "Default",
                SFX: "Graze"
            );
            return statusEffectConfig;
        }


        [EntityLogic(typeof(AyaEvasionSeDef))]
        public sealed class AyaEvasionSe : StatusEffect
        {
            [HarmonyPatch(typeof(Unit), nameof(Unit.MeasureDamage))]
            class Unit_MeasureDamage_Patch
            {
                static bool Prefix(Unit __instance, ref DamageInfo info, ref DamageInfo __result)
                {
                    if (__instance.HasStatusEffect<AyaEvasionSe>() && info.DamageType == DamageType.Attack && info.Damage.Round(MidpointRounding.AwayFromZero) > 0f && !info.IsAccuracy)
                    {
                        __result = new DamageInfo(0f, info.DamageType, true, info.IsAccuracy).BlockBy(__instance.Block).ShieldBy(__instance.Shield);
                        return false;
                    }
                    return true;
                }
            }
            [HarmonyPatch(typeof(Unit), nameof(Unit.TakeDamage))]
            class Unit_TakeDamage_Patch
            {
                static void Postfix(Unit __instance, ref DamageInfo info)
                {
                    if (info.DamageType == DamageType.Attack && __instance.HasStatusEffect<AyaEvasionSe>())
                    {
                        if (info.IsGrazed)
                        {
                            __instance.GetStatusEffect<AyaEvasionSe>().Activate();
                        }
                        else if (info.IsAccuracy)
                        {
                            __instance.GetStatusEffect<AyaEvasionSe>().BeenAccurate();
                        }
                    }
                }
            }
            [HarmonyPatch(typeof(Graze), nameof(Graze.Activate))]
            class Graze_Activate_Patch
            {
                static bool Prefix(StatusEffect __instance)
                {
                    if (__instance.Owner.HasStatusEffect<AyaEvasionSe>())
                    {
                        return false;
                    }
                    return true;
                }
            }
            [HarmonyPatch(typeof(Graze), nameof(Graze.BeenAccurate))]
            class Graze_BeenAccurate_Patch
            {
                static bool Prefix(StatusEffect __instance)
                {
                    if (__instance.Owner.HasStatusEffect<AyaEvasionSe>())
                    {
                        return false;
                    }
                    return true;
                }
            }
            protected override void OnAdded(Unit unit)
            {
                HandleOwnerEvent(Owner.TurnStarted, new GameEventHandler<UnitEventArgs>(OnOwnerTurnStarted));
            }
            private void OnOwnerTurnStarted(UnitEventArgs args)
            {
                if (Owner.HasStatusEffect(typeof(AyaAccelerationSeDef.AyaAccelerationSe)))
                {
                    return;
                }
                if (IsAutoDecreasing)
                {
                    React(new RemoveStatusEffectAction(this, true));
                    return;
                }
                else
                {
                    IsAutoDecreasing = true;
                }
            }
            public void Activate()
            {
                Level--;
                if (Owner.HasStatusEffect(typeof(AyaAccelerationSeDef.AyaAccelerationSe)))
                {
                    Owner.GetStatusEffect(typeof(AyaAccelerationSeDef.AyaAccelerationSe)).Level--;
                    if (Owner.GetStatusEffect(typeof(AyaAccelerationSeDef.AyaAccelerationSe)).Level == 0)
                    {
                        Owner.TryRemoveStatusEffect(Owner.GetStatusEffect(typeof(AyaAccelerationSeDef.AyaAccelerationSe)));
                    }
                }
                if (Level > 0)
                {
                    NotifyActivating();
                    return;
                }
                React(new RemoveStatusEffectAction(this, true));
            }
            public void BeenAccurate()
            {
                Level--;
                if (Owner.HasStatusEffect(typeof(AyaAccelerationSeDef.AyaAccelerationSe)))
                {
                    Owner.GetStatusEffect(typeof(AyaAccelerationSeDef.AyaAccelerationSe)).Level--;
                    if (Owner.GetStatusEffect(typeof(AyaAccelerationSeDef.AyaAccelerationSe)).Level == 0)
                    {
                        Owner.TryRemoveStatusEffect(Owner.GetStatusEffect(typeof(AyaAccelerationSeDef.AyaAccelerationSe)));
                    }
                }
                if (Level == 0)
                {
                    React(new RemoveStatusEffectAction(this, true));
                }
            }
            public override string UnitEffectName
            {
                get
                {
                    return "GrazeLoop";
                }
            }
        }
    }
}