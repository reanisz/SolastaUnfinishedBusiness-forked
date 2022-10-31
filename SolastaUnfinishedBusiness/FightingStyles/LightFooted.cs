﻿using System.Collections.Generic;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionFightingStyleChoices;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterSubclassDefinitions;

namespace SolastaUnfinishedBusiness.FightingStyles;

internal sealed class LightFooted : AbstractFightingStyle
{
    internal override FightingStyleDefinition FightingStyle { get; } = FightingStyleBuilder
        .Create("LightFooted")
        .SetGuiPresentation(Category.FightingStyle, RangerMarksman)
        .SetFeatures(
            FeatureDefinitionMovementAffinityBuilder
                .Create("MovementAffinityLightFootedAdd1")
                .SetGuiPresentationNoContent()
                .SetAdditiveModifierAdvancement(MovementAffinityAdvancement.None)
                .SetBaseSpeedAdditiveModifier(1)
                .SetSituationalContext(ExtraSituationalContext.MainWeaponIsFinesseOrRange)
                .AddToDB())
        .AddToDB();

    internal override List<FeatureDefinitionFightingStyleChoice> FightingStyleChoice => new()
    {
        FightingStyleChampionAdditional, FightingStyleFighter, FightingStyleRanger
    };
}
