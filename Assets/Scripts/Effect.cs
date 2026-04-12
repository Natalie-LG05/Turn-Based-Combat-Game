using UnityEngine;

public class Effect
{
    public System.Action<CharacterInstance, StatusEffectInstance> OnApply { get; set; }
    public System.Action<CharacterInstance, StatusEffectInstance> OnExpire { get; set; }
    public System.Action<CharacterInstance, StatusEffectInstance> OnRemove { get; set; }

    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData, CharacterInstance> OnBeforeAttackDamage { get; set; }
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData, CharacterInstance> OnAfterAttackDamage { get; set; }

    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData> OnBeforeDamage { get; set; }
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData> OnAfterDamage { get; set; }

    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData> OnAfterHPChanged { get; set; }

    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData, StatusEffectInstance> OnStatusGained { get; set; }
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData, StatusEffectInstance> OnStatusRemoved { get; set; }
    
    public System.Action<CharacterInstance, AbilityData> OnBattleStart { get; set; }
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData> OnRoundStart { get; set; }
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData> OnRoundEnd { get; set; }

    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData> OnTurnStart { get; set; }
    public System.Action<CharacterInstance, StatusEffectInstance, AbilityData> OnTurnEnd { get; set; }
}
