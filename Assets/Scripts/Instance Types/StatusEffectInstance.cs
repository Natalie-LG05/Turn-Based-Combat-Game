using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents an instance of a status effect, which is based off of a StatusEffectData.
/// </summary>
public class StatusEffectInstance
{
    private StatusEffectData _statusEffectData;
    private int _duration;
    private float _power;

    private List<StatusEffectStatModifier> _statIncreases;
    private List<StatusEffectStatModifier> _statDecreases;

    private CharacterInstance _sourceCharacter;
    private CharacterInstance _character;

    private bool wasAppliedThisTurn;

    /// <summary>Gets the status effect data this status effect instance is based off of.</summary>
    public StatusEffectData StatusEffectData { get => _statusEffectData; }
    public int Duration { get => _duration; }
    /// <summary>Gets the power of this status effect instance, which is also the base percentage increase of increases applied by this effect.</summary>
    public float Power { get => _power; }

    public List<StatusEffectStatModifier> StatIncreases { get => _statIncreases; }
    public List<StatusEffectStatModifier> StatDecreases { get => _statDecreases; }

    /// <summary>Gets the source character of this status effect instance, which is the character that applied the status effect.</summary>
    public CharacterInstance SourceCharacter { get => _sourceCharacter; }
    /// <summary>Gets the character that this status effect instance is attached to.</summary>
    public CharacterInstance Character { get => _character; }

    /// <summary>Gets the effects of this status effect, found by its id.</summary>
    public Effect Effects { get
        {
            return EffectsDB.StatusEffects[StatusEffectData.Id];
        } 
    }

    /// <summary>
    /// Initialize and create a new status effect instance.
    /// </summary>
    /// <param name="data">The data to base this effect of of.</param>
    /// <param name="duration">The duration of the effect.</param>
    /// <param name="power">The power of the effect.</param>
    /// <param name="sourceCharacter">The character that was the source of this effect.</param>
    /// <param name="targetCharacter">The character this effect is attached to.</param>
    public StatusEffectInstance(StatusEffectData data, int duration, float power, CharacterInstance sourceCharacter, CharacterInstance targetCharacter)
    {
        _statusEffectData = data;
        _duration = duration;
        _power = power;

        _statIncreases = data.StatIncreases.Select(increase => increase.DeepCopy()).ToList();
        _statDecreases = data.StatDecreases.Select(decrease => decrease.DeepCopy()).ToList();

        _sourceCharacter = sourceCharacter;
        _character = targetCharacter;
    }

    /// <summary>
    /// Signal to this status effect that its character's turn has started, and thus that it was not applied this turn.
    /// </summary>
    public void TurnStart()
    {
        wasAppliedThisTurn = false;
    }

    /// <summary>
    /// Signal to this status effect that its character's turn has ended, decreasing it's duration unless it is permanent or was applied this turn.
    /// </summary>
    /// <returns>Whether or not this effect's duration was reduced to zero.</returns>
    public bool TurnEnd()
    {
        if (_statusEffectData.IsPermanent || wasAppliedThisTurn) return false;
        
        _duration -= 1;
        if (_duration <= 0)
            return true;
        return false;
    }

    /// <summary>
    /// Signal to this status effect that is was just applied to a character, applying any stat modifiers of it to that character.
    /// </summary>
    public void OnApply()
    {
        foreach (StatusEffectStatModifier statIncrease in StatIncreases)
            Character.AddModifier(statIncrease.Stat, 1 + ((statIncrease.IsStrengthFixed ? statIncrease.FixedStrength : Power * statIncrease.Multiplier) / 100f), StatusEffectData.Id);

        foreach (StatusEffectStatModifier statDecrease in StatDecreases)
            Character.AddModifier(statDecrease.Stat, 1 - (GetDebuffPower(statDecrease.IsStrengthFixed ? statDecrease.FixedStrength : Power * statDecrease.Multiplier) / 100f), StatusEffectData.Id);

        wasAppliedThisTurn = true;
    }

    /// <summary>
    /// Signal to this status effect that it was just removed from its character, removing any stat modifiers of it from that character.
    /// </summary>
    public void OnRemove()
    {
        foreach (StatusEffectStatModifier statIncrease in StatIncreases)
            Character.RemoveModifier(statIncrease.Stat, StatusEffectData.Id);

        foreach (StatusEffectStatModifier statDecrease in StatDecreases)
            Character.RemoveModifier(statDecrease.Stat, StatusEffectData.Id);
    }

    /// <summary>
    /// Get the strength of a stat modifier of this status effect instance.
    /// </summary>
    /// <param name="modifier">The modifier who's current strength to get.</param>
    /// <param name="isIncrease">Whether or not the modifier being checked in an incraese or a decraese.</param>
    /// <returns>The power of the modifier.</returns>
    public float GetModifierStrength(StatusEffectStatModifier modifier, bool isIncrease)
    {
        if (isIncrease) return modifier.IsStrengthFixed ? modifier.FixedStrength : Power * modifier.Multiplier;
        else return GetDebuffPower(modifier.IsStrengthFixed ? modifier.FixedStrength : Power * modifier.Multiplier);
    }

    /// <summary>
    /// Convert the base power of a decrease effect into a percentage decrease.
    /// </summary>
    /// <param name="power">The base power of the decrease effect.</param>
    /// <returns>The percentage decrease that would result from a decrease effect of the provided power.</returns>
    public float GetDebuffPower(float power)
    {
        return 100f * (1f - (1f / (1f + (power / 100f))));
    }
}
