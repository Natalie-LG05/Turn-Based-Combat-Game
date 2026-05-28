using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A class declaring and storing instances of the Effect class for each status effect and ability that requires custom logic.
/// </summary>
public class EffectsDB
{
    public static Dictionary<string, Effect> StatusEffects { get; private set; } = new Dictionary<string, Effect>()
    {
        // Move statuses

        // Ability statuses

        // Encounter modifier statuses
        {
            "fast_crabs",
            new Effect()
            {
                OnApply = (CharacterInstance character, StatusEffectInstance statusEffect) =>
                {
                    if (character.CharacterData.Categories.Contains(CharacterCategory.Crab))
                        character.AddModifier(Stat.Speed, 1.5f, statusEffect.StatusEffectData.Id);
                },
                OnRemove = (CharacterInstance character, StatusEffectInstance statusEffect) =>
                {
                    character.RemoveModifier(Stat.Speed, statusEffect.StatusEffectData.Id);
                }
            }
        },
    };

    public static Dictionary<string, Effect> AbilityEffects { get; private set; } = new Dictionary<string, Effect>()
    {
        {
            "hard_shell",
            new Effect()
            {
                OnAfterHPChanged = (CharacterInstance character, StatusEffectInstance statusEffect, AbilityData ability) =>
                {
                    character.RemoveStatusEffect(ability.StatusEffect.Id, false);
                    
                    float healthPercent = ((float)character.CurrentHP / character.MaxHP) * 100;
                    float power = Mathf.Floor((100 - healthPercent)) / 2f;
                    if (power > 0)
                        character.ApplyStatusEffect(new StatusEffectInstance(ability.StatusEffect, 99, power, character, character), false);
                }
            }
        },
        {
            "evasive",
            new Effect()
            {
                OnStatusGained = (CharacterInstance character, StatusEffectInstance statusEffect, AbilityData ability, StatusEffectInstance triggeringStatusEffect) =>
                {
                    // remove the triggering status effect (if it increases evasion) and replace it with the same effect with double the multiplier on the evasion up effect
                    if (triggeringStatusEffect.StatusEffectData.Categories.Contains(StatusEffectCategory.EvasionUp))
                    {
                        character.RemoveStatusEffect(triggeringStatusEffect, false);

                        StatusEffectInstance newEffect = new StatusEffectInstance(triggeringStatusEffect.StatusEffectData,
                            triggeringStatusEffect.Duration, triggeringStatusEffect.Power, triggeringStatusEffect.SourceCharacter,
                            triggeringStatusEffect.Character);
                        StatusEffectStatModifier modifier = newEffect.StatIncreases.Where(modifier => modifier.Stat == Stat.Evasion).FirstOrDefault();
                        modifier.Multiplier *= 2;

                        character.ApplyStatusEffect(newEffect, false);
                    }
                }
            }
        },
    };

    public static Dictionary<string, Effect> MoveEffects = new Dictionary<string, Effect>()
    {
        {
            "example_move",
            new Effect()
            {
                MoveOnUse = (CharacterInstance user, MoveData move, CharacterInstance target) =>
                {
                    
                }
            }
        },
    };

    public static Dictionary<string, Effect> ItemEffects = new Dictionary<string, Effect>()
    {
        {
            "example_item",
            new Effect()
            {
                ItemOnUse = (CharacterInstance user, ItemData item, List<CharacterInstance> targets) =>
                {
                    
                }
            }
        },
    };
}