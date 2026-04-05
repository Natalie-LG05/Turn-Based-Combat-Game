using UnityEngine;

[CreateAssetMenu(fileName = "NewAbilityData", menuName = "ScriptableObjects/Ability Data", order = -1000)]
public class AbilityData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private string _id;
    [SerializeField, TextArea] private string _description;

    public Effect Effects
    {
        get
        {
            return EffectsDB.AbilityEffects[_id];
        }
    }
}