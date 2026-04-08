using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BattleState { Setup, ActionSelection, MoveSelection, PartyScreen, EnemyTurn, PerformMove, Busy }

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

    public void MoveButtonClicked()
    {
        if (state == BattleState.ActionSelection)
        {
            StartCoroutine(PlayerMove());
        }
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
        {
            characterInstances.Add(partyMember);
            partyMember.IsPlayerTeam = true;
        }
        playerTeamUI.SetTeam(characterInstances);

        // Set character UI for enemy team based on first wave of current encounter
        foreach (EnemyInstance enemy in encounterManager.CurrentEncounter.Waves[wave - 1].Enemies)
        {
            enemy.Init();  // enemies need to be initialized somewhere, here is the place that makes sense to do so
            enemy.IsPlayerTeam = false;
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

        // Apply status effects of encounter modifiers
        foreach (EncounterModifierData modifier in encounterManager.CurrentEncounter.Modifiers)
        {
            foreach (StatusEffectData effect in modifier.Effects)
            {
                foreach (CharacterInstance character in characters)
                    character.AddStatusEffect(new StatusEffectInstance(effect, 99, 0));
            }
        }
        
        StartRound();
    }

    private void StartRound()
    {
        DetermineTurnOrder();
        turnOrderUI.SetTurnOrder(turnQueue);
        
        StartNextTurn();
    }

    private void EndRound()
    {
        SetRound(round + 1);
        StartRound();
    }

    private void StartNextTurn()
    {
        // Check if there are more characters in the turn queue
        if (turnQueue.Count > 0)
        {
            CharacterInstance currentCharacter = turnQueue.Peek();

            // highlight the character who's turn it is
            foreach (CharacterInstance character in characters)
                character.CharacterUI.UpdateCurrentTurn(character == currentCharacter);

            if (currentCharacter.IsPlayerTeam)
            {
                state = BattleState.ActionSelection;
                dialogueBox.SetDialogue($"It is {currentCharacter.CharacterData.Name}'s turn, choose an action.");
            } else
            {
                StartCoroutine(EnemyTurn(currentCharacter));
            }
        } else
        {
            foreach (CharacterInstance character in characters)
                character.CharacterUI.UpdateCurrentTurn(false);
            EndRound();
        }
    }

    private IEnumerator PlayerMove()
    {
        state = BattleState.Busy;
        CharacterInstance currentCharacter = turnQueue.Peek();
        yield return UseMove(currentCharacter, enemyTeam[Random.Range(0, enemyTeam.Count)], currentCharacter.Moveset[0]);
        EndTurn();
    }

    private IEnumerator EnemyTurn(CharacterInstance currentCharacter)
    {
        state = BattleState.EnemyTurn;
        yield return UseMove(currentCharacter, playerTeam[Random.Range(0, playerTeam.Count)], currentCharacter.Moveset[0]);
        EndTurn();
    }

    private IEnumerator UseMove(CharacterInstance user, CharacterInstance target, MoveData move)
    {
        BattleState prevState = state;
        state = BattleState.PerformMove;

        yield return dialogueBox.TypeDialogue($"{user.CharacterData.Name} used {move.Name}.");

        List<CharacterInstance> targets = new List<CharacterInstance>();

        foreach (MoveDamageEffect effect in move.DamageEffects)
        {
            targets.Add(target);
            foreach (CharacterInstance character in targets)
            {
                yield return character.TakeAttackDamage(user, move, effect);
            }
        }

        foreach (MoveStatusEffect effect in move.StatusEffects)
        {
            // apply statuses
        }

        state = prevState;
    }

    private void EndTurn()
    {
        if (CheckDeadCharacters()) 
        {
            Debug.Log(playerTeam.Count <= 0 ? "Enemies Win!" : "Player Wins!");
        }
        else
        {
            turnQueue.Dequeue();
            turnOrderUI.NextTurn();
            StartNextTurn();
        }
    }

    // Helper methods

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

    private bool CheckDeadCharacters()
    {
        List<CharacterInstance> deadCharacters = new List<CharacterInstance>();
        foreach (CharacterInstance character in characters)
        {
            if (character.CurrentHP <= 0)
                deadCharacters.Add(character);
        }

        foreach (CharacterInstance character in deadCharacters)
            KillCharacter(character);

        if (playerTeam.Count <= 0)
            return true;
        else if (enemyTeam.Count <= 0)
            return true;
        else return false;
    }

    private void KillCharacter(CharacterInstance character)
    {
        characters.Remove(character);
        turnQueue = new Queue<CharacterInstance>(turnQueue.Where(chr => chr != character)); // remove character from the turn queue
        turnOrderUI.SetTurnOrder(turnQueue);

        if (character.IsPlayerTeam)
        {
            playerTeam.RemoveAll(chr => chr.uniqueCharacterId == character.uniqueCharacterId);
            playerTeamUI.RemoveCharacter(character);
        }
        else
        {
            enemyTeam.RemoveAll(chr => chr.uniqueCharacterId == character.uniqueCharacterId);
            enemyTeamUI.RemoveCharacter(character);
        }
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

    private void SetRound(int round)
    {
        this.round = round;
        encounterInfoUI.SetRound(this.round);
    }
}
