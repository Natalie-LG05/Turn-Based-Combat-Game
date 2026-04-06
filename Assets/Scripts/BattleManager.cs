using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Setup, ActionSelection, MoveSelection, PartyScreen, PerformMove, Busy }

public class BattleManager : MonoBehaviour
{
    private int wave;
    private int round;

    private List<PartyMemberInstance> playerTeam;
    private List<EnemyInstance> enemyTeam;

    [SerializeField] private TeamUI playerTeamUI;
    [SerializeField] private TeamUI enemyTeamUI;

    private EncounterManager encounterManager;
    private PartyManager partyManager;

    [SerializeField] private BattleDialogueBox dialogueBox;
    [SerializeField] private EncounterInfoUI encounterInfoUI;

    private void Awake()
    {
        encounterManager = GameObject.Find("Encounter Manager").GetComponent<EncounterManager>();
        partyManager = GameObject.Find("Party Manager").GetComponent<PartyManager>();

        playerTeam = new List<PartyMemberInstance>();
        enemyTeam = new List<EnemyInstance>();
    }

    private void Start()
    {
        SetupBattle();
    }

    private void Update()
    {
        
    }

    private void SetupBattle()
    {
        wave = 1;
        SetRound(1);

        // Set encounter info UI
        encounterInfoUI.SetEncounter(encounterManager.CurrentEncounter);

        // Set character UI for player team based on first two members of party
        playerTeam.Add(partyManager.PartyMembers[0]);
        if (partyManager.PartyMembers.Count > 1)
            playerTeam.Add(partyManager.PartyMembers[1]);

        List<CharacterInstance> characters = new List<CharacterInstance>();
        foreach (PartyMemberInstance partyMember in playerTeam)
            characters.Add(partyMember);
        playerTeamUI.SetTeam(characters);

        // Set character UI for enemy team based on first wave of current encounter
        foreach (EnemyInstance enemy in encounterManager.CurrentEncounter.Waves[wave - 1].Enemies)
        {
            enemy.Init();
            enemyTeam.Add(enemy);
        }

        characters = new List<CharacterInstance>();
        foreach (EnemyInstance enemy in enemyTeam)
        {
            characters.Add(enemy);
        }
        enemyTeamUI.SetTeam(characters);
    }

    private void SetRound(int round)
    {
        this.round = round;
        encounterInfoUI.SetRound(this.round);
    }
}
