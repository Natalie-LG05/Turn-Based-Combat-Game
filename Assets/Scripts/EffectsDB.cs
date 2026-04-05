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
                OnApply = (Character sourceCharacter, Character targetCharacter, float power) =>
                {
                    // TODO: Add a speed down modifier to character that gets this effect
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
                OnTakeDamage = (Character sourceCharacter) =>
                {
                    // Add hard_shell status effect with power based on user HP
                }
            }
        }
    };
}
