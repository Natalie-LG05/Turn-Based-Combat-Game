using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public enum BattleState { Setup, ActionSelection, MoveSelection, TargetSelection, PartyScreen, EnemyTurn, PerformMove, Busy }

public class BattleManager : MonoBehaviour
{
    private int wave;
    private int round;

    public static BattleManager Instance { get; private set; }
    private BattleState state;
    public CharacterInstance CurrentCharacter { get { return turnQueue.Peek(); } }

    private List<PartyMemberInstance> playerTeam;
    private List<EnemyInstance> enemyTeam;
    private List<CharacterInstance> characters;

    private Queue<CharacterInstance> turnQueue;

    [SerializeField] private TeamUI playerTeamUI;
    [SerializeField] private TeamUI enemyTeamUI;

    [SerializeField] private EncounterInfoUI encounterInfoUI;
    [SerializeField] private TurnOrderUI turnOrderUI;

    private EncounterManager encounterManager;
    private PartyManager partyManager;

    private BattleDialogueBox dialogueBox;
    private Queue<string> dialogueQueue;
    private MoveSelectionUI moveSelectionUI;

    [SerializeField] private GameObject dialogueBoxGO;
    [SerializeField] private GameObject actionSelectionButtons;
    [SerializeField] private GameObject moveSelectionUIGO;
    [SerializeField] private GameObject moveInfoUIGO;
    [SerializeField] private GameObject PartyMemberSelectionUIGO;

    private MoveData selectedMove;

    // Unity game object methods
    private void Awake()
    {
        playerTeam = new List<PartyMemberInstance>();
        enemyTeam = new List<EnemyInstance>();
        characters = new List<CharacterInstance>();
        turnQueue = new Queue<CharacterInstance>();

        dialogueBox = dialogueBoxGO.GetComponent<BattleDialogueBox>();
        dialogueQueue = new Queue<string>();
        moveSelectionUI = moveSelectionUIGO.GetComponent<MoveSelectionUI>();

        Instance = this;
    }

    private void Start()
    {
        encounterManager = EncounterManager.Instance;
        partyManager = PartyManager.Instance;

        SetupBattle();
    }

    private void Update()
    {
        switch (state)
        {
            case BattleState.TargetSelection:
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    ShowMoveSelectionScreen();
                    state = BattleState.MoveSelection;
                }
                break;
            case BattleState.MoveSelection or BattleState.PartyScreen:
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    BackButtonClicked();
                    state = BattleState.ActionSelection;
                }
                break;
        }
    }

    // public methods for use in other scripts
    public void QueueMessage(string message)
    {
        dialogueQueue.Enqueue(message);
    }

    // public methods for buttons to call
    public void MoveButtonClicked()
    {
        if (state == BattleState.ActionSelection)
        {
            ShowMoveSelectionScreen();

            state = BattleState.MoveSelection;

            moveSelectionUI.SetCharacter(CurrentCharacter);
        }
    }

    public void MoveOptionClicked(MoveData move)
    {
        if (state == BattleState.MoveSelection )
        {
            if (move.Target is MoveTarget.User or MoveTarget.UserTeam or MoveTarget.EnemyTeam or MoveTarget.All or MoveTarget.AllOther)
                StartCoroutine(PlayerMove(move, CurrentCharacter));
            else
            {
                selectedMove = move;
                state = BattleState.TargetSelection;
                ShowActionSelectionScreen();
                dialogueBox.SetDialogue("Select a target character (AOE moves will target that entire team). Press escape to cancel.");
            }
        }
    }

    public void TargetOptionClicked(CharacterInstance target)
    {
        if (state == BattleState.TargetSelection)
        {
            List<CharacterInstance> validTargets = new List<CharacterInstance>();
            if (selectedMove.Target == MoveTarget.SingleOther)
                validTargets = characters.Where(chr => chr != CurrentCharacter).ToList();
            else if (selectedMove.Target == MoveTarget.SingleEnemy)
                validTargets = enemyTeam.ConvertAll(chr => (CharacterInstance)chr);
            else if (selectedMove.Target == MoveTarget.SingleAlly)
                validTargets = playerTeam.ConvertAll(chr => (CharacterInstance)chr);
            else if (selectedMove.Target == MoveTarget.SingleAny || selectedMove.Target == MoveTarget.AnyTeam || selectedMove.Target == MoveTarget.OppositeTeam)
                validTargets = characters;

            if (validTargets.Contains(target))
                StartCoroutine(PlayerMove(selectedMove, target));
        }
    }

    public void BackButtonClicked()
    {
        if (state == BattleState.MoveSelection || state == BattleState.PartyScreen)
        {
            ShowActionSelectionScreen();
            state = BattleState.ActionSelection;
            dialogueBox.SetDialogue($"It is {CurrentCharacter.CharacterData.Name}'s turn, choose an action.");
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
            // highlight the character who's turn it is
            foreach (CharacterInstance character in characters)
                character.CharacterUI.UpdateCurrentTurn(character == CurrentCharacter);

            CurrentCharacter.TurnStart();

            if (CurrentCharacter.IsPlayerTeam)
            {
                state = BattleState.ActionSelection;
                dialogueBox.SetDialogue($"It is {CurrentCharacter.CharacterData.Name}'s turn, choose an action.");
            } else
            {
                StartCoroutine(EnemyTurn(CurrentCharacter));
            }
        } else
        {
            foreach (CharacterInstance character in characters)
                character.CharacterUI.UpdateCurrentTurn(false);
            EndRound();
        }
    }

    private IEnumerator PlayerMove(MoveData move, CharacterInstance target)
    {
        ShowActionSelectionScreen();
        yield return UseMove(CurrentCharacter, target, move);
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

        yield return dialogueBox.TypeDialogue($"{user.CharacterData.Name} (lvl {user.Level}) used {move.Name}.");

        foreach (MoveDamageEffect effect in move.DamageEffects)
        {
            foreach (CharacterInstance character in GetEffectTargets(effect.Targets, CurrentCharacter, target))
            {
                bool hit = Random.Range(1, 101) <= effect.Accuracy;

                if (hit)
                {
                    for (int i = 0; i < effect.Hits; i++)
                        yield return character.TakeAttackDamage(user, move, effect);
                } else
                {
                    yield return dialogueBox.TypeDialogue($"{user.CharacterData.Name} (lvl {user.Level}) missed!");
                }
            }
        }

        foreach (MoveStatusEffect effect in move.StatusEffects)
        {
            foreach (CharacterInstance character in GetEffectTargets(effect.Targets, CurrentCharacter, target))
            {
                bool hit = Random.Range(1, 101) <= effect.Accuracy;

                if (hit)
                {
                    character.ApplyMoveStatusEffect(CurrentCharacter, move, effect);
                } else
                {
                    yield return dialogueBox.TypeDialogue($"{user.CharacterData.Name} (lvl {user.Level}) missed!");
                }
            }
        }
        yield return ShowQueuedDialogue();

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
            CurrentCharacter.TurnEnd();

            turnQueue.Dequeue();
            turnOrderUI.NextTurn();
            StartNextTurn();
        }
    }

    // Helper methods
    private IEnumerator ShowQueuedDialogue()
    {
        foreach (string msg in dialogueQueue)
            yield return dialogueBox.TypeDialogue(msg);
        dialogueQueue.Clear();
    }


    private List<CharacterInstance> GetEffectTargets(MoveTarget targetType, CharacterInstance user, CharacterInstance target)
    {
        List<CharacterInstance> targets = new List<CharacterInstance>();
        switch (targetType)
        {
            case MoveTarget.SingleOther or MoveTarget.SingleAlly or MoveTarget.SingleEnemy or MoveTarget.SingleAny:
                targets.Add(target);
                break;
            case MoveTarget.User:
                targets.Add(user);
                break;
            case MoveTarget.UserTeam:
                if (user.IsPlayerTeam)
                    targets = playerTeam.ConvertAll(chr => (CharacterInstance)chr);
                else
                    targets = enemyTeam.ConvertAll(chr => (CharacterInstance)chr);
                break;
            case MoveTarget.EnemyTeam:
                if (user.IsPlayerTeam)
                    targets = enemyTeam.ConvertAll(chr => (CharacterInstance)chr);
                else
                    targets = playerTeam.ConvertAll(chr => (CharacterInstance)chr);
                break;
            case MoveTarget.AnyTeam:
                if (target.IsPlayerTeam)
                    targets = playerTeam.ConvertAll(chr => (CharacterInstance)chr);
                else
                    targets = enemyTeam.ConvertAll(chr => (CharacterInstance)chr);
                break;
            case MoveTarget.OppositeTeam:
                if (target.IsPlayerTeam)
                    targets = enemyTeam.ConvertAll(chr => (CharacterInstance)chr);
                else
                    targets = playerTeam.ConvertAll(chr => (CharacterInstance)chr);
                break;
            case MoveTarget.AllOther:
                targets = characters.Where(chr => chr != user).ToList();
                break;
            case MoveTarget.All:
                targets = characters;
                break;
        }
        return targets;
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

    private void ShowActionSelectionScreen()
    {
        if (state == BattleState.MoveSelection || state == BattleState.TargetSelection)
        {
            moveSelectionUIGO.SetActive(false);
            moveInfoUIGO.SetActive(false);
            dialogueBoxGO.SetActive(true);
            actionSelectionButtons.SetActive(true);
        }
        else if (state == BattleState.PartyScreen)
        {

        }
    }

    private void ShowMoveSelectionScreen()
    {
        dialogueBoxGO.SetActive(false);
        actionSelectionButtons.SetActive(false);
        moveSelectionUIGO.SetActive(true);
        moveInfoUIGO.SetActive(true);
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
