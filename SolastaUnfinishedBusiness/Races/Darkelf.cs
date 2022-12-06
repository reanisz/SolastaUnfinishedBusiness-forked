﻿using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using TA;
using static FeatureDefinitionAttributeModifier;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterRaceDefinitions;

namespace SolastaUnfinishedBusiness.Races;

internal static class DarkelfSubraceBuilder
{
    internal static CharacterRaceDefinition SubraceDarkelf { get; } = BuildDarkelf();

    internal static readonly FeatureDefinitionCastSpell CastSpellDarkelfMagic = FeatureDefinitionCastSpellBuilder
        .Create("CastSpellDarkelfMagic")
        .SetGuiPresentation(Category.Feature)
        .SetSpellCastingOrigin(FeatureDefinitionCastSpell.CastingOrigin.Race)
        .SetSpellCastingAbility(AttributeDefinitions.Charisma)
        .SetSpellKnowledge(SpellKnowledge.Selection)
        .SetSpellReadyness(SpellReadyness.AllKnown)
        .SetSlotsRecharge(RechargeRate.LongRest)
        .SetSlotsPerLevel(SharedSpellsContext.RaceCastingSlots)
        .SetKnownCantrips(1, 1, FeatureDefinitionCastSpellBuilder.CasterProgression.Flat)
        .SetSpellList(SpellListDefinitionBuilder
            .Create("SpellListDarkelf")
            .SetGuiPresentationNoContent(true)
            .ClearSpells()
            .SetSpellsAtLevel(0, SpellDefinitions.DancingLights)
            .SetSpellsAtLevel(1, SpellDefinitions.FaerieFire)
            .SetSpellsAtLevel(2, SpellDefinitions.Darkness)
            .FinalizeSpells(true, -1)
            .AddToDB())
        .AddToDB();

    [NotNull]
    private static CharacterRaceDefinition BuildDarkelf()
    {
        var darkelfSpriteReference = Sprites.GetSprite("Darkelf", Resources.Darkelf, 1024, 512);

        var attributeModifierDarkelfCharismaAbilityScoreIncrease = FeatureDefinitionAttributeModifierBuilder
            .Create("AttributeModifierDarkelfCharismaAbilityScoreIncrease")
            .SetGuiPresentation(Category.Feature)
            .SetModifier(AttributeModifierOperation.Additive, AttributeDefinitions.Charisma, 1)
            .AddToDB();

        var lightAffinityDarkelfLightSensitivity = FeatureDefinitionLightAffinityBuilder
            .Create("LightAffinityDarkelfLightSensitivity")
            .SetGuiPresentation(CustomConditionsContext.LightSensitivity.Name, Category.Condition)
            .AddLightingEffectAndCondition(
                new FeatureDefinitionLightAffinity.LightingEffectAndCondition
                {
                    lightingState = LocationDefinitions.LightingState.Bright,
                    condition = CustomConditionsContext.LightSensitivity
                })
            .AddToDB();

        var proficiencyDarkelfWeaponTraining = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyDarkelfWeaponTraining")
            .SetGuiPresentation(Category.Feature)
            .SetProficiencies(
                ProficiencyType.Weapon,
                CustomWeaponsContext.HandXbowWeaponType.Name,
                WeaponTypeDefinitions.RapierType.Name,
                WeaponTypeDefinitions.ShortswordType.Name)
            .AddToDB();

        var darkelfRacePresentation = Elf.RacePresentation.DeepCopy();

        darkelfRacePresentation.femaleNameOptions = ElfHigh.RacePresentation.FemaleNameOptions;
        darkelfRacePresentation.maleNameOptions = ElfHigh.RacePresentation.MaleNameOptions;
        darkelfRacePresentation.preferedSkinColors = new RangedInt(48, 53);
        darkelfRacePresentation.preferedHairColors = new RangedInt(48, 53);

        darkelfRacePresentation.surNameOptions = new List<string>();

        for (var i = 1; i <= 5; i++)
        {
            darkelfRacePresentation.surNameOptions.Add($"Race/&DarkelfSurName{i}Title");
        }

        var raceDarkelf = CharacterRaceDefinitionBuilder
            .Create(ElfHigh, "RaceDarkelf")
            .SetGuiPresentation(Category.Race, darkelfSpriteReference)
            .SetRacePresentation(darkelfRacePresentation)
            .SetFeaturesAtLevel(1,
                FeatureDefinitionMoveModes.MoveModeMove6,
                FeatureDefinitionSenses.SenseSuperiorDarkvision,
                FeatureDefinitionFeatureSets.FeatureSetElfHighLanguages,
                attributeModifierDarkelfCharismaAbilityScoreIncrease,
                proficiencyDarkelfWeaponTraining,
                CastSpellDarkelfMagic,
                lightAffinityDarkelfLightSensitivity)
            .AddToDB();

        raceDarkelf.subRaces.Clear();
        Elf.SubRaces.Add(raceDarkelf);

        return raceDarkelf;
    }
}
