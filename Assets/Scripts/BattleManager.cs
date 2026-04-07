using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BattleState { Setup, ActionSelection, MoveSelection, PartyScreen, PerformMove, Busy }

public class BattleManager : MonoBehaviour
{
    private int wave;
    private int round;

    public static BattleManager Instance { get; private set; }
    private BattleState state;

    private List<PartyMemberInstance> playerTeam;
    private List<EnemyInstance> enemyTeam;
    private List<CharacterInstance> characters;

    private Queue<CharacterInstance> turnQueue;

    [SerializeField] private TeamUI playerTeamUI;
    [SerializeField] private TeamUI enemyTeamUI;

    [SerializeField] private BattleDialogueBox dialogueBox;
    [SerializeField] private EncounterInfoUI encounterInfoUI;
    [SerializeField] private TurnOrderUI turnOrderUI;

    [SerializeField] StatusEffectData testEffect;

    private EncounterManager encounterManager;
    private PartyManager partyManager;

    // Unity game object methods
    private void Awake()
    {
        playerTeam = new List<PartyMemberInstance>();
        enemyTeam = new List<EnemyInstance>();
        characters = new List<CharacterInstance>();
        turnQueue = new Queue<CharacterInstance>();

        Instance = this;
    }

    private void Start()
    {
        encounterManager = EncounterManager.Instance;
        partyManager = PartyManager.Instance;

        SetupBattle();
    }

    // Main logic methods
    private void SetupBattle()
    {
        state = BattleState.Setup;

        wave = 1;
        SetRound(1);

        // Set encounter info UI
        encounterInfoUI.SetEncounter(encounterManager.CurrentEncounter);

        // Set character UI for player team based on first two members of party
        AddActivePartyMember(partyManager.PartyMembers[0]);  // party should always have at least 1 member in it
        if (partyManager.PartyMembers.Count > 1)
            AddActivePartyMember(partyManager.PartyMembers[1]);

        List<CharacterInstance> characterInstances = new List<CharacterInstance>();
        foreach (PartyMemberInstance partyMember in playerTeam)
            characterInstances.Add(partyMember);
        playerTeamUI.SetTeam(characterInstances);

        // Set character UI for enemy team based on first wave of current encounter
        foreach (EnemyInstance enemy in encounterManager.CurrentEncounter.Waves[wave - 1].Enemies)
        {
            enemy.Init();  // enemies need to be initialized somewhere, here is the place that makes sense to do so
            AddEnemy(enemy);
        }

        characterInstances = new List<CharacterInstance>();
        foreach (EnemyInstance enemy in enemyTeam)
        {
            characterInstances.Add(enemy);
        }
        enemyTeamUI.SetTeam(characterInstances);

        foreach (CharacterInstance character in characters)
            character.BattleStart();

        StartRound();
    }

    private void StartRound()
    {
        DetermineTurnOrder();
        turnOrderUI.SetTurnOrder(turnQueue);
        
        StartNextTurn();
    }

    private void StartNextTurn()
    {
        turnQueue.Peek().CharacterUI.UpdateCurrentTurn(true); // highlight the character who's turn it is
    }

    // Helper methods
    private void SetRound(int round)
    {
        this.round = round;
        encounterInfoUI.SetRound(this.round);
    }

    private void AddActivePartyMember(PartyMemberInstance partyMember)
    {
        playerTeam.Add(partyMember);
        characters.Add(partyMember);
    }

    private void AddEnemy(EnemyInstance enemy)
    {
        enemyTeam.Add(enemy);
        characters.Add(enemy);
    }

    private void DetermineTurnOrder()
    {
        CharacterInstance[] characterInstances = new CharacterInstance[characters.Count];
        characters.CopyTo(characterInstances);
        // Sort the characters by their speed stats, handling ties randomly
        characterInstances = characterInstances.OrderByDescending(character => character.Speed).ThenByDescending(character => character.SpeedTieBreaker).ToArray();

        turnQueue.Clear();
        foreach (CharacterInstance character in characterInstances)
        {
            turnQueue.Enqueue(character);
        }
    }
}
