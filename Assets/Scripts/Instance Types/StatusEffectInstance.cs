using UnityEngine;

public class StatusEffectInstance
{
    private StatusEffectData _statusEffectData;
    private int _duration;
    private float _power;

    private bool wasAppliedThisTurn;

    public StatusEffectData StatusEffectData { get => _statusEffectData; }
    public int Duration { get => _duration; }
    public float Power { get => _power; }

    public Effect Effects { get
        {
            return EffectsDB.StatusEffects[StatusEffectData.Id];
        } 
    }

    public StatusEffectInstance(StatusEffectData data, int duration, float power)
    {
        _statusEffectData = data;
        _duration = duration;
        _power = power;

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
