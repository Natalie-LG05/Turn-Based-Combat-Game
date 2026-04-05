using UnityEngine;

public class Effect
{
    public System.Action<CharacterInstance, CharacterInstance, float> OnApply { get; set; }
    public System.Action<CharacterInstance> OnTakeDamage { get; set; }
}
