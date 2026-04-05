using System.Collections.Generic;
using UnityEngine;

public class EffectsDB
{
    public static Dictionary<string, Effect> StatusEffects { get; private set; } = new Dictionary<string, Effect>()
    {
        {
            "speed_down",
            new Effect()
            {
                OnApply = (CharacterInstance sourceCharacter, CharacterInstance targetCharacter, float power) =>
                {
                    // Add a speed down modifier to character that gets this effect
                }
            } 
        },
        {
            "evasion_up",
            new Effect()
            {
                OnApply = (CharacterInstance sourceCharacter, CharacterInstance targetCharacter, float power) =>
                {
                    // Add an evasion up modifier to character that gets this effect
                }
            }
        },
        {
            "defense_up",
            new Effect()
            {
                OnApply = (CharacterInstance sourceCharacter, CharacterInstance targetCharacter, float power) =>
                {
                    // Add a defense up modifier to character that gets this effect
                }
            }
        },
        {
            "hard_shell",
            new Effect()
            {
                OnApply = (CharacterInstance sourceCharacter, CharacterInstance targetCharacter, float power) =>
                {
                    // Add a defense up modifier to character that gets this effect
                }
            }
        },
        {
            "evasive",
            new Effect()
            {
                // Double evasion gained from evastion up effects
            }
        },
        {
            "fast_crabs",
            new Effect()
            {
                // Increase Speed by 15% if this character is a crab
            }
        },
    };

    public static Dictionary<string, Effect> AbilityEffects { get; private set; } = new Dictionary<string, Effect>()
    {
        {
            "hard_shell",
            new Effect()
            {
                OnTakeDamage = (CharacterInstance sourceCharacter) =>
                {
                    // Add hard_shell status effect with power based on user HP
                }
            }
        },
        {
            "evasive",
            new Effect()
            {
                // On initialization add the evasive status effect
            }
        },
    };
}
