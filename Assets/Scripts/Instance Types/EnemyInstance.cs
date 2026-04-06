using UnityEngine;

[System.Serializable]
public class EnemyInstance : CharacterInstance
{
    [SerializeField] private EnemyData enemyData;

    public override void Init()
    {
        characterData = enemyData;
        base.Init();
    }
}
