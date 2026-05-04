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
        // Standard stat ups
        {
            "attack_up",
            new Effect()
            {
                
            }
        },
        {
            "defense_up",
            new Effect()
            {
                
            }
        },
        {
            "evasion_up",
            new Effect()
            {
                
            }
        },
        // Standard stat downs
        {
            "speed_down",
            new Effect()
            {
                
            } 
        },
        // Move statuses
        {
            "whirlpool",
            new Effect()
            {
                
            }
        },
        // Ability statuses
        {
            "hard_shell",
            new Effect()
            {

            }
        },
        // Encounter modifier statuses
        {
            "fast_crabs",
            new Effect()
            {
                OnApply = (CharacterInstance character, StatusEffectInstance statusEffect) =>
                {
                    if (character.CharacterData.Categories.Contains(CharacterCategory.Crab))
                        character.AddModifier(Stat.Speed, 1.5f, statusEffect.StatusEffectData.Id);
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
                    // remove the triggering status effect (if it increases evasion) and replace it with the same effect with double the power
                    // TODO: Only increase the power of the evasion up part, not the entire effect (do after making modular status effects)
                    if (triggeringStatusEffect.StatusEffectData.Categories.Contains(StatusEffectCategory.EvasionUp))
                    {
                        character.RemoveStatusEffect(triggeringStatusEffect, false);

                        StatusEffectInstance newEffect = new StatusEffectInstance(triggeringStatusEffect.StatusEffectData,
                            triggeringStatusEffect.Duration, triggeringStatusEffect.Power, triggeringStatusEffect.SourceCharacter,
                            triggeringStatusEffect.Character);
                        StatusEffectStatModifier modifier = newEffect.StatIncreases.Where(modifier => modifier.Stat == Stat.Evasion).First();
                        modifier.Multiplier *= 2;

                        character.ApplyStatusEffect(newEffect, false);
                    }
                }
            }
        },
    };
}