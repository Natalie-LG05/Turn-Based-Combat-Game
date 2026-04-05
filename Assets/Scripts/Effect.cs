using UnityEngine;

public class Effect
{
    public System.Action<Character, Character, float> OnApply { get; set; }
    public System.Action<Character> OnTakeDamage { get; set; }
}
