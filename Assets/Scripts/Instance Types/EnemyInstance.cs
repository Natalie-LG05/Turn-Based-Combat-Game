using UnityEngine;

[System.Serializable]
public class EnemyInstance : CharacterInstance
{
    [SerializeField] private EnemyData enemyData;

    public override void Init()
    {
        _characterData = enemyData;
        base.Init();
    }
}
