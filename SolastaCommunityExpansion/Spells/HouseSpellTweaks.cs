﻿using System.Linq;
using SolastaModApi.Extensions;
using static SolastaModApi.DatabaseHelper.ConditionDefinitions;
using static SolastaModApi.DatabaseHelper.SpellDefinitions;

namespace SolastaCommunityExpansion.Spells
{
    internal static class HouseSpellTweaks
    {
        public static void Register()
        {
            AddBleedingToRestoration();
            SpikeGrowthDoesNotAffectFlyingCreatures();
            SquareAreaOfEffectSpellsDoNotAffectFlyingCreatures();
        }

        private static void SpikeGrowthDoesNotAffectFlyingCreatures()
        {
            if (!Main.Settings.SpikeGrowthDoesNotAffectFlyingCreatures)
            {
                return;
            }

            var spikeGrowthEffect = SpikeGrowth.EffectDescription;
            spikeGrowthEffect.EffectForms
                .Where(ef => ef.FormType == EffectForm.EffectFormType.Topology)
                .ToList()
                .ForEach(ef => ef.TopologyForm.SetImpactsFlyingCharacters(false));

            spikeGrowthEffect.SetTargetType(RuleDefinitions.TargetType.Cylinder);
            spikeGrowthEffect.SetTargetParameter2(1);
        }


        internal static void SquareAreaOfEffectSpellsDoNotAffectFlyingCreatures()
        {
            // always applicable
            ClearTargetParameter2ForTargetTypeCube();

            // Spells with TargetType.Cube (tp, tp2)
            // BlackTentacles: (4, 2)
            // Entangle: (4, 1)
            // FaerieFire: (4, 2)
            // FlamingSphere: (3, 2) <- a flaming sphere is a cube?
            // Grease: (2, 2)
            // HypnoticPattern: (6, 2)
            // PetalStorm: (3, 2)
            // Slow: (8, 2)

            if (!Main.Settings.SquareAreaOfEffectSpellsDoNotAffectFlyingCreatures)
            {
                RestoreDefinition(BlackTentacles);
                RestoreDefinition(Entangle);
                RestoreDefinition(Grease);
                return;
            }

            SetHeight(BlackTentacles);
            SetHeight(Entangle);
            SetHeight(Grease);

            static void SetHeight(SpellDefinition sd, int height = 1)
            {
                var effect = sd.EffectDescription;

                effect.EffectForms
                    .Where(ef => ef.FormType == EffectForm.EffectFormType.Topology)
                    .ToList()
                    .ForEach(ef => ef.TopologyForm.SetImpactsFlyingCharacters(false));

                if (Main.Settings.EnableTargetTypeSquareCylinder)
                {
                    Main.Log($"Changing {sd.Name} to target type=Cube");
                    effect.SetTargetType(RuleDefinitions.TargetType.Cube);
                }
                else
                {
                    Main.Log($"Changing {sd.Name} to target type=CylinderWithDiameter");
                    effect.SetTargetType(RuleDefinitions.TargetType.CylinderWithDiameter);
                }

                effect.SetTargetParameter2(height);
            }

            static void RestoreDefinition(SpellDefinition sd)
            {
                var effect = sd.EffectDescription;

                // Topology forms have ImpactsFlyingCharacters = true as default
                effect.EffectForms
                    .Where(ef => ef.FormType == EffectForm.EffectFormType.Topology)
                    .ToList()
                    .ForEach(ef => ef.TopologyForm.SetImpactsFlyingCharacters(true));

                Main.Log($"Restoring {sd.Name} to target type=Cube");
                effect.SetTargetType(RuleDefinitions.TargetType.Cube);
            }

            static void ClearTargetParameter2ForTargetTypeCube()
            {
                foreach (var sd in DatabaseRepository
                    .GetDatabase<SpellDefinition>()
                    .Where(sd => sd.EffectDescription.TargetType == RuleDefinitions.TargetType.Cube))
                {
                    // TargetParameter2 is not used by TargetType.Cube but has random values assigned.
                    // We are going to use it to create a square cylinder with height so set to zero for all spells with TargetType.Cube.
                    sd.EffectDescription.SetTargetParameter2(0);
                }
            }

/*            static void LogAllSpellsWithTargetTypeCube()
            {
                Main.Log("-----------------------------------");
                foreach (var g in DatabaseRepository.GetDatabase<SpellDefinition>()
                    .GroupBy(sd => sd.EffectDescription.TargetType))
                {
                    Main.Log($"{g.Key} -----------------------------------");
                    foreach (var sd in g.OrderBy(s => s.Name))
                    {
                        var ef = sd.EffectDescription;
                        Main.Log($"{sd.Name}: ({ef.TargetParameter}, {ef.TargetParameter2})");
                    }
                }
                Main.Log("-----------------------------------");
            }*/
        }

        public static void AddBleedingToRestoration()
        {
            var cf = LesserRestoration.EffectDescription.GetFirstFormOfType(EffectForm.EffectFormType.Condition);

            if (cf != null)
            {
                if (Main.Settings.AddBleedingToLesserRestoration)
                {
                    cf.ConditionForm.ConditionsList.TryAdd(ConditionBleeding);
                }
                else
                {
                    cf.ConditionForm.ConditionsList.Remove(ConditionBleeding);
                }
            }
            else
            {
                Main.Error("Unable to find form of type Condition in LesserRestoration");
            }

            var cfg = GreaterRestoration.EffectDescription.GetFirstFormOfType(EffectForm.EffectFormType.Condition);

            if (cfg != null)
            {
                // NOTE: using the same setting as for Lesser Restoration for compatibility
                if (Main.Settings.AddBleedingToLesserRestoration)
                {
                    cfg.ConditionForm.ConditionsList.TryAdd(ConditionBleeding);
                }
                else
                {
                    cfg.ConditionForm.ConditionsList.Remove(ConditionBleeding);
                }
            }
            else
            {
                Main.Error("Unable to find form of type Condition in GreaterRestoration");
            }
        }
    }
}
