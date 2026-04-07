using UnityEngine;

public class StatusEffectInstance
{
    private StatusEffectData _data;
    private int _duration;
    private float _power;

    public StatusEffectData Data { get => _data; }
    public int Duration { get => _duration; }

    public Effect Effects { get
        {
            return EffectsDB.StatusEffects[Data.Id];
        } 
    }

    public StatusEffectInstance(StatusEffectData data, int duration, float power)
    {
        _data = data;
        _duration = duration;
        _power = power;
    }
}
