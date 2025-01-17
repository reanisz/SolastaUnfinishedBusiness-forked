﻿using System.Linq;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomValidators;
using TA;
using static ActionDefinitions;

namespace SolastaUnfinishedBusiness.Api.GameExtensions;

public static class GameLocationCharacterExtensions
{
    internal static (RulesetAttackMode mode, ActionModifier modifier) GetFirstMeleeModeThatCanAttack(
        this GameLocationCharacter instance,
        GameLocationCharacter target,
        IGameLocationBattleService service = null)
    {
        service ??= ServiceRepository.GetService<IGameLocationBattleService>();

        foreach (var mode in instance.RulesetCharacter.AttackModes)
        {
            if (!ValidatorsWeapon.IsMelee(mode))
            {
                continue;
            }

            // Prepare attack evaluation params
            var attackParams = new BattleDefinitions.AttackEvaluationParams();
            var modifier = new ActionModifier();

            attackParams.FillForPhysicalReachAttack(instance, instance.LocationPosition, mode,
                target, target.LocationPosition, modifier);

            // Check if the attack is possible and collect the attack modifier inside the attackParams
            if (service.CanAttack(attackParams))
            {
                return (mode, modifier);
            }
        }

        return (null, null);
    }

    internal static (RulesetAttackMode mode, ActionModifier modifier) GetFirstRangedModeThatCanAttack(
        this GameLocationCharacter instance,
        GameLocationCharacter target,
        IGameLocationBattleService service = null)
    {
        service ??= ServiceRepository.GetService<IGameLocationBattleService>();

        foreach (var mode in instance.RulesetCharacter.AttackModes)
        {
            if (mode.Reach)
            {
                continue;
            }

            // Prepare attack evaluation params
            var attackParams = new BattleDefinitions.AttackEvaluationParams();
            var modifier = new ActionModifier();

            attackParams.FillForPhysicalRangeAttack(instance, instance.LocationPosition, mode,
                target, target.LocationPosition, modifier);

            // Check if the attack is possible and collect the attack modifier inside the attackParams
            if (service.CanAttack(attackParams))
            {
                return (mode, modifier);
            }
        }

        return (null, null);
    }

    internal static RulesetAttackMode GetFirstRangedModeThatCanBeReadied(this GameLocationCharacter instance)
    {
        return instance.RulesetCharacter.AttackModes
            .Where(mode => mode.ActionType == ActionType.Main)
            .FirstOrDefault(mode => mode.Ranged || mode.Thrown);
    }

    /**
     * Finds first attack mode that can attack target on positionBefore, but can't on positionAfter
     */
    internal static bool CanPerformOpportunityAttackOnCharacter(
        this GameLocationCharacter instance,
        GameLocationCharacter target,
        int3? positionBefore,
        int3? positionAfter,
        out RulesetAttackMode attackMode,
        out ActionModifier attackModifier,
        bool allowRange = false,
        IGameLocationBattleService service = null,
        bool accountAoOImmunity = false,
        IsWeaponValidHandler weaponValidator = null)
    {
        service ??= ServiceRepository.GetService<IGameLocationBattleService>();
        attackMode = null;
        attackModifier = null;

        if (accountAoOImmunity && !service.IsValidAttackerForOpportunityAttackOnCharacter(instance, target))
        {
            return false;
        }

        foreach (var mode in instance.RulesetCharacter.AttackModes)
        {
            if (mode.Ranged && !allowRange)
            {
                continue;
            }

            if (!(weaponValidator?.Invoke(mode, null, instance.RulesetCharacter) ?? true))
            {
                continue;
            }

            // Prepare attack evaluation params
            var paramsBefore = new BattleDefinitions.AttackEvaluationParams();

            if (mode.Ranged)
            {
                paramsBefore.FillForPhysicalRangeAttack(instance, instance.LocationPosition, mode,
                    target, positionBefore ?? target.LocationPosition, new ActionModifier());
            }
            else
            {
                paramsBefore.FillForPhysicalReachAttack(instance, instance.LocationPosition, mode,
                    target, positionBefore ?? target.LocationPosition, new ActionModifier());
            }

            // Check if the attack is possible and collect the attack modifier inside the attackParams
            if (!service.CanAttack(paramsBefore))
            {
                continue;
            }

            if (positionAfter != null)
            {
                var paramsAfter = new BattleDefinitions.AttackEvaluationParams();

                paramsAfter.FillForPhysicalReachAttack(instance, instance.LocationPosition, mode,
                    target, positionAfter.Value, new ActionModifier());

                // skip if attack is still possible after move - target hasn't left reach yet
                if (service.CanAttack(paramsAfter))
                {
                    continue;
                }
            }

            attackMode = mode;
            attackModifier = paramsBefore.attackModifier;

            return true;
        }

        return false;
    }

    internal static bool CanAct(this GameLocationCharacter instance)
    {
        var character = instance.RulesetCharacter;

        return character is { IsDeadOrDyingOrUnconscious: false }
               && !character.HasConditionOfType(RuleDefinitions.ConditionProne)
               && !character.HasConditionOfType(RuleDefinitions.ConditionIncapacitated)
               && !character.HasConditionOfType(RuleDefinitions.ConditionStunned)
               && !character.HasConditionOfType(RuleDefinitions.ConditionParalyzed);
    }

    internal static bool CanReact(this GameLocationCharacter instance, bool ignoreReactionUses = false)
    {
        if (!instance.CanAct())
        {
            return false;
        }

        var actionService = ServiceRepository.GetService<IGameLocationActionService>();

        if (actionService == null)
        {
            return false;
        }

        var hasReactionInQueue = actionService.PendingReactionRequestGroups
            .SelectMany(x => x.Requests)
            .Any(x => x.Character == instance);

        if (hasReactionInQueue)
        {
            return false;
        }

        if (!ignoreReactionUses)
        {
            return instance.GetActionTypeStatus(ActionType.Reaction) == ActionStatus.Available;
        }

        var wasUsed = instance.currentActionRankByType[ActionType.Reaction] > 0;

        if (wasUsed)
        {
            instance.currentActionRankByType[ActionType.Reaction]--;
        }

        var canReact = instance.GetActionTypeStatus(ActionType.Reaction) == ActionStatus.Available;

        if (wasUsed)
        {
            instance.currentActionRankByType[ActionType.Reaction]++;
        }

        return canReact;
    }

    internal static bool OnceInMyTurnIsValid(this GameLocationCharacter instance, string key)
    {
        return instance.OncePerTurnIsValid(key) &&
               Gui.Battle != null && Gui.Battle.ActiveContender == instance;
    }

    internal static bool OncePerTurnIsValid(this GameLocationCharacter instance, string key)
    {
        return !instance.UsedSpecialFeatures.ContainsKey(key);
    }

#if false
    internal static int GetActionTypeRank(this GameLocationCharacter instance, ActionType type)
    {
        var ranks = instance.currentActionRankByType;
        return ranks.TryGetValue(type, out var value) ? value : 0;
    }
#endif

    internal static FeatureDefinition GetCurrentAdditionalActionFeature(this GameLocationCharacter instance,
        ActionType type)
    {
        if (!instance.currentActionRankByType.TryGetValue(type, out var rank))
        {
            return null;
        }

        var filters = instance.ActionPerformancesByType[type];
        return rank >= filters.Count ? null : PerformanceFilterExtraData.GetData(filters[rank])?.Feature;
    }

    internal static bool CanCastAnyInvocationOfActionId(this GameLocationCharacter instance,
        Id actionId,
        ActionScope scope,
        bool canCastSpells,
        bool canOnlyUseCantrips)
    {
        var character = instance.RulesetCharacter;

        if (character.Invocations.Empty())
        {
            return false;
        }

        ActionStatus? mainSpell = null;
        ActionStatus? bonusSpell = null;

        foreach (var invocation in character.Invocations)
        {
            var definition = invocation.InvocationDefinition;
            var isValid = definition
                .GetAllSubFeaturesOfType<IsInvocationValidHandler>()
                .All(v => v(character, definition));

            if (definition.HasSubFeatureOfType<HiddenInvocation>() || !isValid)
            {
                continue;
            }

            if (scope == ActionScope.Battle)
            {
                isValid = definition.GetActionId() == actionId;
            }
            else
            {
                isValid = definition.GetMainActionId() == actionId;
            }

            var grantedSpell = definition.GrantedSpell;
            if (isValid && grantedSpell != null)
            {
                if (!canCastSpells)
                {
                    isValid = false;
                }
                else if (canOnlyUseCantrips && grantedSpell.SpellLevel > 0)
                {
                    isValid = false;
                }
                else
                {
                    var spellActionId = grantedSpell.BattleActionId;

                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                    switch (spellActionId)
                    {
                        case Id.CastMain:
                            mainSpell ??= scope == ActionScope.Battle
                                ? instance.GetActionStatus(spellActionId, scope)
                                : ActionStatus.Available;
                            if (mainSpell != ActionStatus.Available)
                            {
                                isValid = false;
                            }

                            break;
                        case Id.CastBonus:
                            bonusSpell ??= scope == ActionScope.Battle
                                ? instance.GetActionStatus(spellActionId, scope)
                                : ActionStatus.Available;
                            if (bonusSpell != ActionStatus.Available)
                            {
                                isValid = false;
                            }

                            break;
                    }
                }
            }

            if (isValid)
            {
                return true;
            }
        }

        return false;
    }
}
