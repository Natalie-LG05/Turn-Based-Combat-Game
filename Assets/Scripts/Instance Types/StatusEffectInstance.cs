using UnityEngine;

public class StatusEffectInstance : MonoBehaviour
{
    private StatusEffectData _data;
    private int _duration;

    public StatusEffectData Data { get => _data; }
    public int Duration { get => _duration; }

    public Effect Effects { get
        {
            return EffectsDB.StatusEffects[Data.Id];
        } 
    }
}
