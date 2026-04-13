using System.Collections.Generic;
using UnityEngine;

public class EffectsDB
{
    public static Dictionary<string, Effect> StatusEffects { get; private set; } = new Dictionary<string, Effect>()
    {
        // Standard stat ups
        {
            "attack_up",
            new Effect()
            {
                OnApply = (CharacterInstance character, StatusEffectInstance statusEffect) =>
                {
                    character.AddModifier(Stat.Attack, 1 + (statusEffect.BuffPower / 100f), statusEffect.StatusEffectData.Id);
                },
                OnRemove = (CharacterInstance character, StatusEffectInstance statusEffect) =>
                {
                    character.RemoveModifier(Stat.Attack, statusEffect.StatusEffectData.Id);
                }
            }
        },
        {
            "defense_up",
            new Effect()
            {
                OnApply = (CharacterInstance character, StatusEffectInstance statusEffect) =>
                {
                    character.AddModifier(Stat.Defense, 1 + (statusEffect.BuffPower / 100f), statusEffect.StatusEffectData.Id);
                },
                OnRemove = (CharacterInstance character, StatusEffectInstance statusEffect) =>
                {
                    character.RemoveModifier(Stat.Defense, statusEffect.StatusEffectData.Id);
                }
            }
        },
        {
            "evasion_up",
            new Effect()
            {
                OnApply = (CharacterInstance character, StatusEffectInstance statusEffect) =>
                {
                    character.AddModifier(Stat.Evasion, 1 + (statusEffect.BuffPower / 100f), statusEffect.StatusEffectData.Id);
                },
                OnRemove = (CharacterInstance character, StatusEffectInstance statusEffect) =>
                {
                    character.RemoveModifier(Stat.Evasion, statusEffect.StatusEffectData.Id);
                }
            }
        },
        // Standard stat downs
        {
            "speed_down",
            new Effect()
            {
                OnApply = (CharacterInstance character, StatusEffectInstance statusEffect) =>
                {
                    character.AddModifier(Stat.Speed, 1 - (statusEffect.DebuffPower / 100f), statusEffect.StatusEffectData.Id);
                },
                OnRemove = (CharacterInstance character, StatusEffectInstance statusEffect) =>
                {
                    character.RemoveModifier(Stat.Speed, statusEffect.StatusEffectData.Id);
                }
            } 
        },
        // Move statuses
        {
            "whirlpool",
            new Effect()
            {
                OnApply = (CharacterInstance character, StatusEffectInstance statusEffect) =>
                {
                    character.AddModifier(Stat.Speed, 1 - (statusEffect.DebuffPower / 100f), statusEffect.StatusEffectData.Id);
                },
                OnRemove = (CharacterInstance character, StatusEffectInstance statusEffect) =>
                {
                    character.RemoveModifier(Stat.Speed, statusEffect.StatusEffectData.Id);
                }
            }
        },
        // Ability statuses
        {
            "hard_shell",
            new Effect()
            {
                OnApply = (CharacterInstance character, StatusEffectInstance statusEffect) =>
                {
                    character.AddModifier(Stat.Defense, 1 + (statusEffect.BuffPower / 100f), statusEffect.StatusEffectData.Id);
                },
                OnRemove = (CharacterInstance character, StatusEffectInstance statusEffect) =>
                {
                    character.RemoveModifier(Stat.Defense, statusEffect.StatusEffectData.Id);
                }
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
                OnAfterHPChanged = (CharacterInstance character, StatusEffectInstance sourceEffect, AbilityData ability) =>
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
                OnStatusGained = (CharacterInstance character, StatusEffectInstance sourceEffect, AbilityData ability, StatusEffectInstance statusEffect) =>
                {
                    character.RemoveStatusEffect(statusEffect, false);
                    character.ApplyStatusEffect(new StatusEffectInstance(statusEffect.StatusEffectData,
                        statusEffect.Duration, statusEffect.BuffPower * 2, statusEffect.SourceCharacter,
                        statusEffect.Character), false);
                }
            }
        },
    };
}