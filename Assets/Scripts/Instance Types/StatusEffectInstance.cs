using UnityEngine;

public class StatusEffectInstance
{
    private StatusEffectData _statusEffectData;
    private int _duration;
    private float _power;

    public StatusEffectData StatusEffectData { get => _statusEffectData; }
    public int Duration { get => _duration; }

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
    }
}
