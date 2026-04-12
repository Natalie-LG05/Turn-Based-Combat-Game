using UnityEngine;

public class StatusEffectInstance
{
    private StatusEffectData _statusEffectData;
    private int _duration;
    private float _power;
    private float _debuffPower;

    private CharacterInstance _sourceCharacter;
    private CharacterInstance _character;

    private bool wasAppliedThisTurn;

    public StatusEffectData StatusEffectData { get => _statusEffectData; }
    public int Duration { get => _duration; }
    public float BuffPower { get => _power; }
    public float DebuffPower { get => 100f * (1f - (1f / (1f + (_power / 100f)))); }

    public CharacterInstance SourceCharacter { get => _sourceCharacter; }
    public CharacterInstance Character { get => _character; }

    public Effect Effects { get
        {
            return EffectsDB.StatusEffects[StatusEffectData.Id];
        } 
    }

    public StatusEffectInstance(StatusEffectData data, int duration, float power, CharacterInstance sourceCharacter, CharacterInstance targetCharacter)
    {
        _statusEffectData = data;
        _duration = duration;
        _power = power;

        _sourceCharacter = sourceCharacter;
        _character = targetCharacter;

        wasAppliedThisTurn = true;
    }

    public void TurnStart()
    {
        wasAppliedThisTurn = false;
    }

    public bool TurnEnd()
    {
        if (_statusEffectData.IsPermanent || wasAppliedThisTurn) return false;
        
        _duration -= 1;
        if (_duration <= 0)
            return true;
        return false;
    }
}
