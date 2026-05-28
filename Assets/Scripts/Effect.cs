using System.Collections.Generic;

/// <summary>
/// Represents custom effects of a status effect or an ability. Each instance can set the functionality of each of several functions, which will be called at the appropriate time.
/// </summary>
public class Effect
{
    /// <summary>
    /// Triggered when this status effect is applied (status effect only).
    /// <br/><br/>Arguments: 
    /// <br/>character: the character with this effect
    /// <br/>statusEffect: the status effect with this effect
    /// </summary>
    public System.Action<CharacterInstance, StatusEffectInstance> OnApply { get; set; }
    /// <summary>
    /// Triggered when this status effect expires (status effect only).
    /// <br/><br/>Arguments: 
    /// <br/>character: the character with this effect
    /// <br/>statusEffect: the status effect with this effect
    /// </summary>
    public System.Action<CharacterInstance, StatusEffectInstance> OnExpire { get; set; }
    /// <summary>
    /// Triggered when this status effect is removed (status effect only).
    /// <br/><br/>Arguments: 
    /// <br/>character: the character with this effect
    /// <br/>statusEffect: the status effect with this effect
    /// </summary>
    public System.Action<CharacterInstance, StatusEffectInstance> OnRemove { get; set; }

    /// <summary>
    /// Triggered before this character takes attack damage.
    /// <br/><br/>Arguments: 
    /// <br/>character: the character with this effect
    /// <br/>statusEffect: the status effect with this effect (if any)
    /// <br/>ability: the ability with this effect (if any)
    /// <br/>attacker: the character that attacked this character
    /// </summary>
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData, CharacterInstance> OnBeforeAttackDamage { get; set; }
    /// <summary>
    /// Triggered after this character takes attack damage.
    /// <br/><br/>Arguments: 
    /// <br/>character: the character with this effect
    /// <br/>statusEffect: the status effect with this effect (if any)
    /// <br/>ability: the ability with this effect (if any)
    /// <br/>attacker: the character that attacked this character
    /// </summary>
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData, CharacterInstance> OnAfterAttackDamage { get; set; }

    /// <summary>
    /// Triggered before this character takes any damage.
    /// <br/><br/>Arguments: 
    /// <br/>character: the character with this effect
    /// <br/>statusEffect: the status effect with this effect (if any)
    /// <br/>ability: the ability with this effect (if any)
    /// </summary>
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData> OnBeforeDamage { get; set; }
    /// <summary>
    /// Triggered after this character takes any damage.
    /// <br/><br/>Arguments: 
    /// <br/>character: the character with this effect
    /// <br/>statusEffect: the status effect with this effect (if any)
    /// <br/>ability: the ability with this effect (if any)
    /// </summary>
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData> OnAfterDamage { get; set; }

    /// <summary>
    /// Triggered whenever the character's health changes.
    /// <br/><br/>Arguments: 
    /// <br/>character: the character with this effect
    /// <br/>statusEffect: the status effect with this effect (if any)
    /// <br/>ability: the ability with this effect (if any)
    /// </summary>
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData> OnAfterHPChanged { get; set; }

    /// <summary>
    /// Triggered when this character gains a status effect.
    /// <br/><br/>Arguments: 
    /// <br/>character: the character with this effect
    /// <br/>statusEffect: the status effect with this effect (if any)
    /// <br/>ability: the ability with this effect (if any)
    /// <br/>triggeringStatusEffect: the status effect that triggered this effect
    /// </summary>
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData, StatusEffectInstance> OnStatusGained { get; set; }
    /// <summary>
    /// Triggered when this character loses a status effect.
    /// <br/><br/>Arguments: 
    /// <br/>character: the character with this effect
    /// <br/>statusEffect: the status effect with this effect (if any)
    /// <br/>ability: the ability with this effect (if any)
    /// <br/>triggeringStatusEffect: the status effect that triggered this effect
    /// </summary>
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData, StatusEffectInstance> OnStatusRemoved { get; set; }

    /// <summary>
    /// Triggered at the start of battle (ability only).
    /// <br/><br/>Arguments: 
    /// <br/>character: the character with this effect
    /// <br/>ability: the ability with this effect
    /// </summary>
    public System.Action<CharacterInstance, AbilityData> OnBattleStart { get; set; }
    /// <summary>
    /// Triggered at the start of each round.
    /// <br/><br/>Arguments: 
    /// <br/>character: the character with this effect
    /// <br/>statusEffect: the status effect with this effect (if any)
    /// <br/>ability: the ability with this effect (if any)
    /// </summary>
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData> OnRoundStart { get; set; }
    /// <summary>
    /// Triggered at the end of each round.
    /// <br/><br/>Arguments: 
    /// <br/>character: the character with this effect
    /// <br/>statusEffect: the status effect with this effect (if any)
    /// <br/>ability: the ability with this effect (if any)
    /// </summary>
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData> OnRoundEnd { get; set; }

    /// <summary>
    /// Triggered at the start of this character's turn.
    /// <br/><br/>Arguments: 
    /// <br/>character: the character with this effect
    /// <br/>statusEffect: the status effect with this effect (if any)
    /// <br/>ability: the ability with this effect (if any)
    /// </summary>
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData> OnTurnStart { get; set; }
    /// <summary>
    /// Triggered at the end of this character's turn.
    /// <br/><br/>Arguments: 
    /// <br/>character: the character with this effect
    /// <br/>statusEffect: the status effect with this effect (if any)
    /// <br/>ability: the ability with this effect (if any)
    /// </summary>
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData> OnTurnEnd { get; set; }

    /// <summary>
    /// Triggered when an item is used.
    /// <br/><br/>Arguments: 
    /// <br/>user: the character that used this move or item
    /// <br/>item: the item with this effect (if any)
    /// <br/>target: a list of the characters targetted by this effect
    /// </summary>
    public System.Action<CharacterInstance, ItemData, List<CharacterInstance>> ItemOnUse { get; set; }
    /// <summary>
    /// Triggered when a move is used.
    /// <br/><br/>Arguments: 
    /// <br/>user: the character that used this move or item
    /// <br/>move: the move with this effect (if any)
    /// <br/>target: the primary target of this effect
    /// </summary>
    public System.Action<CharacterInstance, MoveData, CharacterInstance> MoveOnUse { get; set; }
}
