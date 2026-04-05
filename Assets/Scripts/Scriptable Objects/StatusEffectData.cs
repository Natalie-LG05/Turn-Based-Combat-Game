using UnityEngine;

[CreateAssetMenu(fileName = "NewStatusEffectData", menuName = "ScriptableObjects/Status Effect Data", order = -1000)]
public class StatusEffectData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private string _id;
    [SerializeField, TextArea] private string _description;
    [SerializeField] private bool _isPermanent;

    public string Name { get => _name; }
    public string Id { get => _id; }
    public string Description { get => _description; }
    public bool IsPermanent { get => _isPermanent; }
}
