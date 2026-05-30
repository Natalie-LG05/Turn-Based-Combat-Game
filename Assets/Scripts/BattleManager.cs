using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public enum BattleState { Setup, ActionSelection, MoveSelection, MoveTargetSelection, SwitchingPartyMembers, ReplaceDeadPartyMember, ItemSelectionScreen, ItemTargetSelection, EnemyTurn, PerformMove, UseItem, RoundEnd, Busy, PlayerWin, PlayerLose }

/// <summary>
/// Manages battle, containing the main gameplay loop during combat and handling much of the logic.
/// </summary>
public class BattleManager : MonoBehaviour
{
    private int wave;
    private int round;

    public static BattleManager Instance { get; private set; }
    public BattleState State { get; set; }
    /// <summary>Gets the character who's turn it currently is.</summary>
    public CharacterInstance CurrentCharacter { get { return turnQueue.Peek(); } }

    private List<PartyMemberInstance> alivePartyMembers;
    private List<PartyMemberInstance> deadPartyMembers;

    private List<PartyMemberInstance> playerTeam;
    private List<EnemyInstance> enemyTeam;
    private List<CharacterInstance> characters;

    private Queue<CharacterInstance> turnQueue;
    [SerializeField] private TurnOrderUI turnOrderUI;

    [SerializeField] private BattleEncounterInfoUI encounterInfoUI;
    [SerializeField] private Image background;

    [SerializeField] private TeamUI playerTeamUI;
    [SerializeField] private TeamUI enemyTeamUI;

    private EncounterManager encounterManager;
    private PartyManager partyManager;
    private InventoryManager inventoryManager;

    [SerializeField] private GameObject dialogueBoxGO;
    private BattleDialogueBox dialogueBox;
    private Queue<string> dialogueQueue;

    [SerializeField] private GameObject actionSelectionButtons;

    [SerializeField] private GameObject moveSelectionUIGO;
    [SerializeField] private GameObject moveInfoUIGO;
    private MoveSelectionUI moveSelectionUI;

    [SerializeField] private GameObject partyMemberSelectionUIGO;
    private PartyMemberSelectionUI partyMemberSelectionUI;

    [SerializeField] private GameObject effectInfoTooltip;

    [SerializeField] private GameObject itemSelectionUIGO;
    [SerializeField] private GameObject itemInfoUIGO;
    private ItemSelectionUI itemSelectionUI;

    [SerializeField] private GameObject winScreenUIGO;
    private WinScreenUI winScreenUI;

    [SerializeField] private GameObject loseScreenUIGO;
    private LoseScreenUI loseScreenUI;
    [SerializeField] private ItemData shellsItemData;

    private MoveData selectedMove;
    private BattleItemData selectedItem;
    private bool partyMemberDiedThisTurn;

    // Unity game object methods
    private void Awake()
    {
        alivePartyMembers = new List<PartyMemberInstance>();
        deadPartyMembers = new List<PartyMemberInstance>();

        playerTeam = new List<PartyMemberInstance>();
        enemyTeam = new List<EnemyInstance>();
        characters = new List<CharacterInstance>();
        turnQueue = new Queue<CharacterInstance>();

        dialogueBox = dialogueBoxGO.GetComponent<BattleDialogueBox>();
        moveSelectionUI = moveSelectionUIGO.GetComponent<MoveSelectionUI>();
        partyMemberSelectionUI = partyMemberSelectionUIGO.GetComponent<PartyMemberSelectionUI>();
        itemSelectionUI = itemSelectionUIGO.GetComponent<ItemSelectionUI>();
        winScreenUI = winScreenUIGO.GetComponent<WinScreenUI>();
        loseScreenUI = loseScreenUIGO.GetComponent<LoseScreenUI>();

        dialogueQueue = new Queue<string>();

        Instance = this;
    }

    private void Start()
    {
        encounterManager = EncounterManager.Instance;
        partyManager = PartyManager.Instance;
        inventoryManager = InventoryManager.Instance;

        SetupBattle();
    }

    private void Update()
    {
        switch (State)
        {
            case BattleState.MoveTargetSelection:
                // pressing escape cancels the move selection that was made and returns to the move selection screen
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    ShowMoveSelectionScreen();
                    State = BattleState.MoveSelection;
                }
                break;
            case BattleState.ItemTargetSelection:
                // pressing escape cancels the item selection that was made and returns to the item selection screen
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    ShowItemSelectionScreen();
                    State = BattleState.ItemSelectionScreen;
                }
                break;
            case BattleState.MoveSelection or BattleState.SwitchingPartyMembers or BattleState.ItemSelectionScreen:
                // pressing escape is the same as clicking the back button
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    BackButtonClicked();
                    State = BattleState.ActionSelection;
                }
                break;
        }
    }

    // public methods for use in other scripts
    /// <summary>
    /// Add a message to the dialogue queue.
    /// </summary>
    /// <param name="message">The message/dialogue to queue.</param>
    public void QueueMessage(string message)
    {
        dialogueQueue.Enqueue(message);
    }

    /// <summary>
    /// Add a status effect message to the dialogue queue.
    /// </summary>
    /// <param name="targetCharacter">The character that gained the status effect.</param>
    /// <param name="status">The status effect that was gained.</param>
    public void QueueStatusMessage(CharacterInstance targetCharacter, StatusEffectInstance status)
    {
        string message = $"{targetCharacter.CharacterData.Name} (lvl {targetCharacter.Level}) ";
        message += status.StatusEffectData.Type == StatusEffectType.Debuff ? "was inflicted with" : "gained";
        message += $" {status.StatusEffectData.Name} for {status.Duration} turns.";
        QueueMessage(message);
    }

    // public methods for buttons to call
    /// <summary>
    /// When the move button is clicked during action selection, change to move selection and show the move selection screen.
    /// </summary>
    public void MoveButtonClicked()
    {
        if (State == BattleState.ActionSelection)
        {
            State = BattleState.MoveSelection;

            // show the move selection screen and update the UI
            ShowMoveSelectionScreen();
            moveSelectionUI.SetCharacter(CurrentCharacter);
        }
    }

    /// <summary>
    /// When a move option is clicked during move selection, 
    /// <br/>either use that move if it requires no target selection 
    /// <br/>or change to target selection and show the action selection screen and some dialogue.
    /// </summary>
    /// <param name="move">The move that was selected.</param>
    public void MoveOptionClicked(MoveData move)
    {
        if (State == BattleState.MoveSelection)
        {
            // these target options don't rely on the selected target, so simply use the move
            // with the user as the target, otherwise the player needs to select a target
            if (move.Target is BattleTargettingType.User or BattleTargettingType.UserTeam or BattleTargettingType.EnemyTeam or BattleTargettingType.All or BattleTargettingType.AllOther)
                StartCoroutine(PlayerMove(move, CurrentCharacter));
            else
            {
                State = BattleState.MoveTargetSelection;

                selectedMove = move;
                ShowActionSelectionScreen();
                dialogueBox.SetDialogue("Select a target character (AOE moves will target that entire team). Press escape to cancel.");
            }
        }
    }

    /// <summary>
    /// When a valid target option is clicked during target selection, use the selected move with that target.
    /// </summary>
    /// <param name="target"></param>
    public void TargetOptionClicked(CharacterInstance target)
    {
        if (State == BattleState.MoveTargetSelection)
        {
            List<CharacterInstance> validTargets = new List<CharacterInstance>();
            if (selectedMove.Target == BattleTargettingType.SingleOther)
                validTargets = characters.Where(chr => chr != CurrentCharacter).ToList();
            else if (selectedMove.Target == BattleTargettingType.SingleEnemy)
                validTargets = enemyTeam.ConvertAll(chr => (CharacterInstance)chr);
            else if (selectedMove.Target == BattleTargettingType.SingleAlly)
                validTargets = playerTeam.ConvertAll(chr => (CharacterInstance)chr);
            else if (selectedMove.Target == BattleTargettingType.SingleAny || selectedMove.Target == BattleTargettingType.AnyTeam || selectedMove.Target == BattleTargettingType.OppositeTeam)
                validTargets = characters;

            if (validTargets.Contains(target))
                StartCoroutine(PlayerMove(selectedMove, target));
        }
        else if (State == BattleState.ItemTargetSelection)
        {
            List<CharacterInstance> validTargets = new List<CharacterInstance>();
            if (selectedItem.Target == BattleTargettingType.SingleOther)
                validTargets = characters.Where(chr => chr != CurrentCharacter).ToList();
            else if (selectedItem.Target == BattleTargettingType.SingleEnemy)
                validTargets = enemyTeam.ConvertAll(chr => (CharacterInstance)chr);
            else if (selectedItem.Target == BattleTargettingType.SingleAlly)
                validTargets = playerTeam.ConvertAll(chr => (CharacterInstance)chr);
            else if (selectedItem.Target == BattleTargettingType.SingleAny || selectedMove.Target == BattleTargettingType.AnyTeam || selectedMove.Target == BattleTargettingType.OppositeTeam)
                validTargets = characters;

            if (validTargets.Contains(target))
                StartCoroutine(PlayerItem(selectedItem, target));
        }
    }

    /// <summary>
    /// When the switch button is clicked during action selection, change to party member selection and show the party screen.
    /// </summary>
    public void SwitchButtonClicked()
    {
        if (State == BattleState.ActionSelection)
        {
            State = BattleState.SwitchingPartyMembers;

            // show the party screen and update the UI
            ShowPartyScreen();
            partyMemberSelectionUI.SetParty(alivePartyMembers, deadPartyMembers, partyMemberDiedThisTurn, false);
        }
    }

    /// <summary>
    /// When a valid party member option is clicked during party member selection, 
    /// <br/>switch that character in, deactivating the current party member (if applicable).
    /// </summary>
    /// <param name="character"></param>
    public void PartyMemberOptionClicked(CharacterInstance character)
    {
        if (State == BattleState.SwitchingPartyMembers)
        {
            effectInfoTooltip.SetActive(false);  // here to fix a bug of the tooltip getting stuck on screen

            StartCoroutine(PlayerSwitch(CurrentCharacter, character));
        }
        else if (State == BattleState.ReplaceDeadPartyMember)
        {
            effectInfoTooltip.SetActive(false);  // here to fix a bug of the tooltip getting stuck on screen

            ActivatePartyMember(character);
            ShowActionSelectionScreen();

            // signal that the dead party member has been handled and return to ending the round
            partyMemberDiedThisTurn = false;
            EndRound();
        }
        else if (State == BattleState.ItemTargetSelection)
        {
            effectInfoTooltip.SetActive(false);  // here to fix a bug of the tooltip getting stuck on screen

            PartyMemberInstance partyMember = (PartyMemberInstance)character;

            // set valid targets to dead party members
            List<PartyMemberInstance> validTargets = deadPartyMembers;
            if (validTargets.Contains(partyMember))
                StartCoroutine(PlayerItem(selectedItem, partyMember));
        }
    }

    /// <summary>
    /// When the run button is clicked, determine if a lose penalty should be inflicted and then end combat, showing the lose screen.
    /// </summary>
    public void RunButtonClicked()
    {
        if (State == BattleState.ActionSelection)
        {
            bool inflictLosePenalty = encounterManager.CurrentEncounter.EncounterData.DifficultyLevel - partyManager.AveragePartyMemberLevel > -3;
            EndCombat(!inflictLosePenalty, true);
        }
    }

    public void ItemButtonClicked()
    {
        if (State == BattleState.ActionSelection)
        {
            StartCoroutine(EnterItemSelection());
        }
    }

    private IEnumerator EnterItemSelection()
    {
        // if the player has battle items, enter item selection
        if (inventoryManager.BattleItems.Count <= 0)
        {
            yield return dialogueBox.TypeDialogue("You do not have any battle items to use!");
            dialogueBox.SetDialogue($"It is {CurrentCharacter.CharacterData.Name}'s turn, choose an action.");
        } else
        {
            State = BattleState.ItemSelectionScreen;

            ShowItemSelectionScreen();
            itemSelectionUI.UpdateItemOptions();
        }
    }

    /// <summary>
    /// When an item option is clicked during item selection, 
    /// <br/>either use that item if it requires no target selection 
    /// <br/>or change to target selection and show the action selection screen and some dialogue. 
    /// </summary>
    /// <param name="item"></param>
    public void ItemOptionClicked(BattleItemData item)
    {
        if (State == BattleState.ItemSelectionScreen)
        {
            // these target options don't rely on the selected target, so simply use the item
            // with the user as the target, otherwise the player needs to select a target
            if (item.Target is BattleTargettingType.User or BattleTargettingType.UserTeam or BattleTargettingType.EnemyTeam or BattleTargettingType.All or BattleTargettingType.AllOther)
                StartCoroutine(PlayerItem(item, CurrentCharacter));
            else if (item.Target == BattleTargettingType.PartyMember)
            {
                State = BattleState.ItemTargetSelection;
                selectedItem = item;

                ShowPartyScreen();
                partyMemberSelectionUI.SetParty(alivePartyMembers, deadPartyMembers, partyMemberDiedThisTurn, true);
            }
            else
            {
                State = BattleState.ItemTargetSelection;
                selectedItem = item;

                ShowActionSelectionScreen();
                dialogueBox.SetDialogue("Select a target character (AOE items will target that entire team). Press escape to cancel.");
            }
        }
    }

    /// <summary>
    /// When the back button is clicked during move selection or party member selection, change to action selection and show the action selection screen.
    /// </summary>
    public void BackButtonClicked()
    {
        if (State == BattleState.MoveSelection || State == BattleState.SwitchingPartyMembers || State == BattleState.ItemSelectionScreen || State == BattleState.ItemTargetSelection)
        {
            ShowActionSelectionScreen();
            State = BattleState.ActionSelection;
            dialogueBox.SetDialogue($"It is {CurrentCharacter.CharacterData.Name}'s turn, choose an action.");
        }
    }

    /// <summary>
    /// When the return home button on the win or lose screen is clicked, return to the home scene.
    /// </summary>
    public void ReturnHomeButtomClicked()
    {
        if (State == BattleState.PlayerWin || State == BattleState.PlayerLose)
            SceneManager.LoadScene(1);
    }

    // Main logic methods
    /// <summary>
    /// Handle the logic for starting battle.
    /// </summary>
    private void SetupBattle()
    {
        State = BattleState.Setup;

        wave = 1;
        SetRound(1);
        partyMemberDiedThisTurn = false;

        // Set encounter info UI and background
        encounterInfoUI.SetEncounter(encounterManager.CurrentEncounter.EncounterData);
        background.sprite = encounterManager.CurrentEncounter.EncounterData.Area.Background;

        // setup the player's team based on their party
        alivePartyMembers = partyManager.Party.Select(partyMember => partyMember).ToList();
        foreach (PartyMemberInstance partyMember in alivePartyMembers)
            partyMember.IsPlayerTeam = true;

        // start with the first two party members active
        AddActivePartyMember(alivePartyMembers[0]);  // party should always have at least 1 member in it
        if (alivePartyMembers.Count > 1)
            AddActivePartyMember(alivePartyMembers[1]);
        playerTeamUI.SetTeam(playerTeam.ConvertAll(parMem => (CharacterInstance)parMem));

        // spawn the first wave of enemies from the current encounter
        SpawnEnemyWave(wave);

        // signal to each character that the battle has started
        foreach (CharacterInstance character in characters)
            character.BattleStart();

        // apply status effects of encounter modifiers to inactive party members
        foreach (EncounterModifierData modifier in encounterManager.CurrentEncounter.EncounterData.Modifiers)
        {
            foreach (StatusEffectData effect in modifier.Effects)
            {
                if (alivePartyMembers.Count > 2) alivePartyMembers[2].ApplyStatusEffect(new StatusEffectInstance(effect, 99, 0, null, alivePartyMembers[2]), false);
                if (alivePartyMembers.Count > 3) alivePartyMembers[3].ApplyStatusEffect(new StatusEffectInstance(effect, 99, 0, null, alivePartyMembers[3]), false);
            }
        }

        StartRound();
    }

    private void StartRound()
    {
        // get the turn order for this round and update the UI accordingly
        DetermineTurnOrder();
        turnOrderUI.SetTurnOrder(turnQueue);

        // signal to each character that a new round has started
        foreach (CharacterInstance character in characters)
            character.RoundStart();

        StartNextTurn();
    }

    /// <summary>
    /// Handle the logic for when a round ends.
    /// </summary>
    private void EndRound()
    {
        // if a party member died this round, show the party screen for the player to choose another (if there are any) to replace it
        if (partyMemberDiedThisTurn)
        {
            State = BattleState.ReplaceDeadPartyMember;

            if (alivePartyMembers.Count > 1)
            {
                ShowPartyScreen();
                partyMemberSelectionUI.SetParty(alivePartyMembers, deadPartyMembers, partyMemberDiedThisTurn, false);
            } else
            {
                // there are no inactive party members left, so just go back to ending the round as normal
                partyMemberDiedThisTurn = false;
                EndRound();
            }
        } else
        {
            State = BattleState.RoundEnd;

            // signal to each character that a round has ended
            foreach (CharacterInstance character in characters)
                character.RoundEnd();

            SetRound(round + 1);
            StartRound();
        }
    }

    /// <summary>
    /// Start the next character's turn (if any) and handle any applicabale start of turn logic.
    /// <br/>If there are no characters left in the turn queue, end the current round.
    /// </summary>
    private void StartNextTurn()
    {
        // Check if there are more characters in the turn queue, otherwise end the round
        if (turnQueue.Count > 0)
        {
            // highlight only the character who's turn it is
            foreach (CharacterInstance character in characters)
                character.CharacterUI.UpdateCurrentTurn(character == CurrentCharacter);

            CurrentCharacter.TurnStart();  // signal to the character that its turn has started

            // for party members enter action selection, for enemies use a move based on their algorithm
            if (CurrentCharacter.IsPlayerTeam)
            {
                State = BattleState.ActionSelection;
                dialogueBox.SetDialogue($"It is {CurrentCharacter.CharacterData.Name}'s turn, choose an action.");
            } else
            {
                StartCoroutine(EnemyTurn(CurrentCharacter));
            }
        } else
        {
            // make sure no characters are highlighted between rounds
            foreach (CharacterInstance character in characters)
                character.CharacterUI.UpdateCurrentTurn(false);
            EndRound();
        }
    }

    /// <summary>
    /// Handle the logic for when a character's turn ends.
    /// </summary>
    private void EndTurn()
    {
        // signal to the character that its turn has ended, unless it switched or died
        if (playerTeam.Contains(CurrentCharacter)) CurrentCharacter.TurnEnd();

        turnQueue.Dequeue();
        turnOrderUI.NextTurn();  // update the turn order UI

        // if all enemies have died, spawn the next wave, unless there are no waves left or all active party members or dead, then end combat
        if (CheckDeadCharacters())
        {
            if (playerTeam.Count > 0 && encounterManager.CurrentEncounter.EncounterData.Waves.Count() > wave)
            {
                SpawnEnemyWave(++wave);
                StartRound();
            }
            else
                EndCombat(playerTeam.Count > 0, false);
        }
        else
        {
            StartNextTurn();
        }
    }

    /// <summary>
    /// Handle the logic for when the player uses a move.
    /// </summary>
    /// <param name="move">The move to be used.</param>
    /// <param name="target">The primary target of the move.</param>
    private IEnumerator PlayerMove(MoveData move, CharacterInstance target)
    {
        // show the action selection screen so that dialogue is visible in the dialogue box and to avoid confusion
        ShowActionSelectionScreen();
        yield return UseMove(CurrentCharacter, target, move);
        EndTurn();
    }

    /// <summary>
    /// Handle the logic for when the player uses an item.
    /// </summary>
    /// <param name="item">The item to be used.</param>
    /// <param name="target">The primary target of the item.</param>
    /// <returns></returns>
    private IEnumerator PlayerItem(BattleItemData item, CharacterInstance target)
    {
        ShowActionSelectionScreen();
        State = BattleState.UseItem;

        // resolve the item's effects, displaying dialogue as this is done
        yield return dialogueBox.TypeDialogue($"{CurrentCharacter.CharacterData.Name} used {item.Name}.");

        item.Effects?.ItemOnUse?.Invoke(CurrentCharacter, item, new List<CharacterInstance>() { target });
        inventoryManager.RemoveItemPublic(item, 1);

        // perform all heal effects of the item
        foreach (BattleItemHealEffect healEffect in item.HealEffects)
        {
            foreach (CharacterInstance character in GetEffectTargets(healEffect.Targets, CurrentCharacter, target))
            {
                character.ApplyItemHeal(CurrentCharacter, item, healEffect);   
                if (character.CharacterUI != null) yield return UpdateHealthbarSmooth(character);
            }
        }

        // perform all status effects of the item
        foreach (BattleItemStatusEffect itemEffect in item.ItemStatusEffects)
        {
            foreach (CharacterInstance character in GetEffectTargets(itemEffect.Targets, CurrentCharacter, target))
            {
                foreach (StatusEffectData statusData in itemEffect.StatusEffects)
                {
                    StatusEffectInstance statusEffectInstance = new StatusEffectInstance(statusData, itemEffect.Duration, itemEffect.Power, CurrentCharacter, character);
                    character.ApplyStatusEffect(statusEffectInstance, true);
                    QueueStatusMessage(character, statusEffectInstance);
                }
            }
        }
        yield return ShowQueuedDialogue();

        // perform all revive effects of the move
        foreach (BattleItemReviveEffect reviveEffect in item.ReviveEffects)
        {
            PartyMemberInstance partyMember = (PartyMemberInstance)target;
            partyMember.ApplyItemHeal(CurrentCharacter, item, reviveEffect);
            deadPartyMembers.Remove(partyMember);
            alivePartyMembers.Add(partyMember);
        }

        if (item.UsesTurn) EndTurn();
        else State = BattleState.ActionSelection;
    }

    /// <summary>
    /// Handle the logic for when the player does the switch action.
    /// </summary>
    /// <param name="oldCharacter">The character being swapped out.</param>
    /// <param name="newCharacter">The character being swapped in.</param>
    private IEnumerator PlayerSwitch(CharacterInstance oldCharacter, CharacterInstance newCharacter)
    {
        Switch(oldCharacter, newCharacter);

        // show the action selection screen so that dialogue is visible in the dialogue box and to avoid confusion
        ShowActionSelectionScreen();
        yield return dialogueBox.TypeDialogue($"Switched {oldCharacter.CharacterData.Name} with {newCharacter.CharacterData.Name}");

        EndTurn();
    }

    /// <summary>
    /// Handles ending combat, taking into account if the player won or lost and if the player ran or not.
    /// </summary>
    /// <param name="didPlayerWin">Whether or not the player won.</param>
    /// <param name="ran">Whether or not the player ran from combat.</param>
    private void EndCombat(bool didPlayerWin, bool ran)
    {
        State = didPlayerWin ? BattleState.PlayerWin : BattleState.PlayerLose;

        foreach (PartyMemberInstance partyMember in partyManager.Party)
            partyMember.BattleEnd();

        if (State == BattleState.PlayerWin && !ran)
        {
            ShowWinScreen();
            
            // determine rewards and award them
            List<ItemEntry<ItemData>> rewards = encounterManager.CurrentEncounter.Rewards;
            foreach (ItemEntry<ItemData> entry in rewards)
                inventoryManager.AddItemPublic(entry.ItemData, entry.Amount);

            // update UI
            winScreenUI.SetRewardItems(rewards);

            encounterManager.CurrentEncounter.Clear();
        } else
        {
            ShowLoseScreen();
            int penaltyAmount = 0;

            if (!didPlayerWin)
            {
                // inflict lose penalty and apply it
                int difference = encounterManager.CurrentEncounter.EncounterData.DifficultyLevel - Mathf.FloorToInt(partyManager.AveragePartyMemberLevel);
                penaltyAmount = Mathf.Max(difference * 10, 5);
                inventoryManager.RemoveItemPublic(shellsItemData, penaltyAmount);
            }

            // update UI
            loseScreenUI.SetPenaltyAmount(penaltyAmount);
        }
    }

    /// <summary>
    /// Handle the logic for an enemy taking its turn.
    /// </summary>
    /// <param name="currentCharacter">The character who's turn it currently is.</param>
    private IEnumerator EnemyTurn(CharacterInstance currentCharacter)
    {
        State = BattleState.EnemyTurn;
        EnemyInstance enemy = enemyTeam.Where(enemy => enemy.UniqueCharacterId == currentCharacter.UniqueCharacterId).FirstOrDefault();

        // choose what action the enemy will use then perform that action
        EnemyAction chosenAction = DetermineEnemyAction(enemy);
        switch (chosenAction)
        {
            case EnemyAction.Attack:
                yield return EnemyAttack(enemy);
                break;
            case EnemyAction.Buff:
                yield return EnemyBuff(enemy);
                break;
            case EnemyAction.Debuff:
                yield return EnemyDebuff(enemy);
                break;
            case EnemyAction.Heal:
                yield return EnemyHeal(enemy);
                break;
        }

        EndTurn();
    }

    /// <summary>
    /// Handle logic for an enemy using an attack move.
    /// <br/>Enemies use their strongest attack on a random player character.
    /// </summary>
    /// <param name="enemy">The enemy performing the action.</param>
    private IEnumerator EnemyAttack(EnemyInstance enemy)
    {
        MoveData move = enemy.StrongestAttackMove();  // enemies attack with their strongest attack move
        CharacterInstance target = EnemyChooseTarget(enemy, EnemyAction.Attack, move);
        yield return UseMove(enemy, target, move);
    }

    /// <summary>
    /// Handle logic for an enemy using a buff move.
    /// <br/>Enemies give the strongest version of a buff to a random ally that doesn't already have that buff.
    /// </summary>
    /// <param name="enemy">The enemy performing the action.</param>
    private IEnumerator EnemyBuff(EnemyInstance enemy)
    {
        // only use buff moves on targets that don't have the buffs they give already
        List<MoveData> moveOptions = enemy.UsefulBuffMoves(enemyTeam);
        MoveData move = moveOptions[Random.Range(0, moveOptions.Count)];
        CharacterInstance target = EnemyChooseTarget(enemy, EnemyAction.Buff, move);

        // after selected a move and target, if there is a move with a stronger version of
        // one of that move's useful effects that can affect that target, use that move instead
        StatusEffectData status = enemy.RandomUsefulStatusEffect(target, move);
        move = enemy.StrongestStatusMove(target, status);

        yield return UseMove(enemy, target, move);
    }

    /// <summary>
    /// Handle logic for an enemy using a debuff move.
    /// <br/>Enemies give the strongest version of a debuff to a random player character that doesn't already have that debuff.
    /// </summary>
    /// <param name="enemy">The enemy performing the action.</param>
    private IEnumerator EnemyDebuff(EnemyInstance enemy)
    {
        // only use debuff moves on targets that don't have the debuffs they give already
        List<MoveData> moveOptions = enemy.UsefulDebuffMoves(playerTeam.ConvertAll(chr => (CharacterInstance)chr));
        MoveData move = moveOptions[Random.Range(0, moveOptions.Count)];
        CharacterInstance target = EnemyChooseTarget(enemy, EnemyAction.Debuff, move);

        // after selected a move and target, if there is a move with a stronger version of
        // one of that move's useful effects that can affect that target, use that move instead
        StatusEffectData status = enemy.RandomUsefulStatusEffect(target, move);
        move = enemy.StrongestStatusMove(target, status);

        yield return UseMove(enemy, target, move);
    }

    /// <summary>
    /// Handle logic for an enemy using a healing move.
    /// <br/>Enemies will heal allies that are under a certain health percentage.
    /// </summary>
    /// <param name="enemy">The enemy performing the action.</param>
    private IEnumerator EnemyHeal(EnemyInstance enemy)
    {
        // enemies use the strongest healing move they have that affects the ally they selected to heal
        List<MoveData> moveOptions = enemy.UsefulHealingMoves(enemyTeam);
        MoveData move = moveOptions[Random.Range(0, moveOptions.Count)];
        CharacterInstance target = EnemyChooseTarget(enemy, EnemyAction.Heal, move);

        yield return UseMove(enemy, target, move);
    }

    /// <summary>
    /// Spawn a specified wave of enemies from the current encounter.
    /// </summary>
    /// <param name="waveNum">The wave to spawn.</param>
    private void SpawnEnemyWave(int waveNum)
    {
        List<EnemyInstance> enemies = new List<EnemyInstance>();
        EncounterData encounter = encounterManager.CurrentEncounter.EncounterData;

        switch (encounter.Type)
        {
            case EncounterType.Normal or EncounterType.Dungeon:
                enemies = encounter.Waves[waveNum - 1].Enemies;
                break;
            case EncounterType.SingleWild:
                // spawn one random common enemy
                enemies.Add(new EnemyInstance(encounter.Area.RandomCommonEnemy, encounter.DifficultyLevel));
                break;
            case EncounterType.DoubleWild:
                // spawn two random common enemies
                for (int i = 0; i < 2; i++)
                    enemies.Add(new EnemyInstance(encounter.Area.RandomCommonEnemy, encounter.DifficultyLevel));
                break;
            case EncounterType.HardWild:
                // spawn one random common enemy
                enemies.Add(new EnemyInstance(encounter.Area.RandomRareEnemy, encounter.DifficultyLevel));
                break;
            case EncounterType.HardDoubleWild:
                // 1 random rare and 1 random common or rare (50% chance to be either or)
                enemies.Add(new EnemyInstance(encounter.Area.RandomRareEnemy, encounter.DifficultyLevel));

                EnemyData randomEnemyData = Random.Range(0, 2) == 0 ? encounter.Area.RandomCommonEnemy : encounter.Area.RandomRareEnemy;
                enemies.Add(new EnemyInstance(randomEnemyData, encounter.DifficultyLevel));
                break;
        }

        foreach (EnemyInstance enemy in enemies)
            AddEnemy(enemy);
        enemyTeamUI.SetTeam(enemyTeam.ConvertAll(enemy => (CharacterInstance)enemy));
    }

    /// <summary>
    /// Have a provided character perform a provided move with a provided character as the primary target.
    /// <br/>Depending on the move's target type, the primary target may or may not matter.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="target"></param>
    /// <param name="move"></param>
    /// <returns></returns>
    private IEnumerator UseMove(CharacterInstance user, CharacterInstance target, MoveData move)
    {
        BattleState prevState = State;
        State = BattleState.PerformMove;

        yield return dialogueBox.TypeDialogue($"{user.CharacterData.Name} (lvl {user.Level}) used {move.Name}.");

        move.Effects?.MoveOnUse?.Invoke(user, move, target);

        // perform all damage effects of the move
        foreach (MoveDamageEffect effect in move.DamageEffects)
        {
            foreach (CharacterInstance character in GetEffectTargets(effect.Targets, CurrentCharacter, target))
            {
                // if the effect hits, perform its effects, otherwise show dialogue saying that the character missed
                if (CheckIfMoveEffectHits(user, character, effect))
                {
                    for (int i = 0; i < effect.Hits; i++)
                    {
                        character.TakeAttackDamage(user, move, effect);
                        if (character.CharacterUI != null) yield return UpdateHealthbarSmooth(character);
                    }
                } else
                {
                    yield return dialogueBox.TypeDialogue($"{user.CharacterData.Name} (lvl {user.Level}) missed!");
                }
            }
        }

        // perform all status effects of the move
        foreach (MoveStatusEffect effect in move.MoveStatusEffects)
        {
            foreach (CharacterInstance character in GetEffectTargets(effect.Targets, CurrentCharacter, target))
            {
                // if the effect hits, perform its effects, otherwise show dialogue saying that the character missed
                if (CheckIfMoveEffectHits(user, character, effect))
                {
                    character.ApplyMoveStatusEffect(CurrentCharacter, move, effect);
                } else
                {
                    yield return dialogueBox.TypeDialogue($"{user.CharacterData.Name} (lvl {user.Level}) missed!");
                }
            }
        }
        yield return ShowQueuedDialogue();  // show any dialogue that was queued by status effects being applied

        // perform all heal effects of the move
        foreach (MoveHealEffect effect in move.HealEffects)
        {
            foreach (CharacterInstance character in GetEffectTargets(effect.Targets, CurrentCharacter, target))
            {
                // if the effect hits, perform its effects, otherwise show dialogue saying that the character missed
                if (CheckIfMoveEffectHits(user, character, effect))
                {
                    character.ApplyMoveHeal(user, move, effect);
                    if (character.CharacterUI != null) yield return UpdateHealthbarSmooth(character);
                } else
                {
                    yield return dialogueBox.TypeDialogue($"{user.CharacterData.Name} (lvl {user.Level}) missed!");
                }
            }
        }

        State = prevState;
    }

    // Helper methods
    /// <summary>
    /// Given an enemy, enemy action, and move, pick a random valid target.
    /// <br/>When attacking, chooses a random player character.
    /// <br/>When buffing, chooses a random ally that doesn't already have all the buffs that the move can apply.
    /// <br/>When debuffing, chooses a random player character that doesn't already have all the debuffs that the move can apply.
    /// <br/>When healing, chooses a random ally that is below a certain health threshold.
    /// </summary>
    /// <param name="enemy"></param>
    /// <param name="action"></param>
    /// <param name="move"></param>
    /// <returns>The chosen target.</returns>
    private CharacterInstance EnemyChooseTarget(EnemyInstance enemy, EnemyAction action, MoveData move)
    {
        // these target options don't rely on the selected target, so just use the user as the target
        if (move.Target is BattleTargettingType.User or BattleTargettingType.UserTeam or BattleTargettingType.EnemyTeam or BattleTargettingType.All or BattleTargettingType.AllOther)
            return enemy;

        switch (action)
        {
            case EnemyAction.Attack:
                return playerTeam[Random.Range(0, playerTeam.Count)];
            case EnemyAction.Buff:
                List<EnemyInstance> buffTargetOptions = enemy.AlliesNeedingBuff(enemyTeam, move);
                return buffTargetOptions[Random.Range(0, buffTargetOptions.Count)];
            case EnemyAction.Debuff:
                List<CharacterInstance> debuffTargetOptions = enemy.PlayerCharactersNeedingDebuff(playerTeam.ConvertAll(chr => (CharacterInstance)chr), move);
                return debuffTargetOptions[Random.Range(0, debuffTargetOptions.Count)];
            case EnemyAction.Heal:
                List<EnemyInstance> healTargetOptions = enemy.AlliesNeedingHealing(enemyTeam);
                return healTargetOptions[Random.Range(0, healTargetOptions.Count)];
        }
        return null;  // this should never run and is just here to prevent a compilation error
    }

    /// <summary>
    /// Randomly pick one of the valid enemy action based the weight (chance) of each action for the provided enemy.
    /// </summary>
    /// <param name="enemy">The enemy performing the action.</param>
    /// <returns>The selected action.</returns>
    private EnemyAction DetermineEnemyAction(EnemyInstance enemy)
    {
        // Determine useful valid actions for the enemy
        Dictionary<EnemyAction, int> actions = new Dictionary<EnemyAction, int>() { { EnemyAction.Attack, enemy.EnemyData.AttackChance } };
        if (enemy.UsefulBuffMoves(enemyTeam).Count > 0) actions.Add(EnemyAction.Buff, enemy.EnemyData.BuffChance);
        if (enemy.UsefulDebuffMoves(playerTeam.ConvertAll(chr => (CharacterInstance)chr)).Count > 0) actions.Add(EnemyAction.Debuff, enemy.EnemyData.DebuffChance);
        if (enemy.UsefulHealingMoves(enemyTeam).Count > 0) actions.Add(EnemyAction.Heal, enemy.EnemyData.HealChance);

        // Randomly choose one of the valid actions based on the weight of each
        int totalWeight = actions.Values.Sum();
        int randNum = Random.Range(0, totalWeight);
        foreach (EnemyAction action in actions.Keys)
        {
            if (randNum < actions[action])
                return action;
            randNum -= actions[action];
        }
        return EnemyAction.Attack;  // In case that somehow fails, just default to attacking
    }

    /// <summary>
    /// Check if a move effect hits based on the accuracy of the effect, the user's accuracy modifiers, and the target's evasion modifiers.
    /// </summary>
    /// <param name="user">The character that used the move.</param>
    /// <param name="target">A target of the move effect.</param>
    /// <param name="effect">The move effect being applied.</param>
    /// <returns></returns>
    private bool CheckIfMoveEffectHits(CharacterInstance user, CharacterInstance target, MoveEffect effect)
    {
        // return true in special cases where the move should be guarenteed to hit
        if (effect.AlwaysHits)
            return true;
        else if (effect.AlwaysHitsSelf && user.UniqueCharacterId == target.UniqueCharacterId)
            return true;

        // calculate chance of move hitting based on move's base acccuracy and any relevant accuracy and evasion modifiers
        List<float> userAccuracyModifierVals = user.StatModifiers[Stat.Accuracy].Select(mod => mod.Power).ToList();
        List<float> targetEvasionModifierVals = target.StatModifiers[Stat.Evasion].Select(mod => mod.Power).ToList();

        float userAccuracyMod = userAccuracyModifierVals.Aggregate(1f, (acc, next) => acc * next);
        float targetEvasionMod = targetEvasionModifierVals.Aggregate(1f, (acc, next) => acc * (next == 0 ? 0 : 1 / next));

        float hitChance = effect.Accuracy * userAccuracyMod * targetEvasionMod;

        // randomly determine if the move it based on the chance calculated
        int randNum = Random.Range(1, 101);
        bool hit = randNum <= hitChance;

#if UNITY_EDITOR
        Debug.Log($"{randNum} <= {hitChance}");
#endif

        return hit;
    }

    private IEnumerator UpdateHealthbarSmooth(CharacterInstance character)
    {
        yield return character.CharacterUI.UpdateHealthSmooth();
    }

    /// <summary>
    /// Switch an active party member with an inactive party member.
    /// </summary>
    /// <param name="oldCharacter">The party member to swap out.</param>
    /// <param name="newCharacter">The party member to swap in.</param>
    private void Switch(CharacterInstance oldCharacter, CharacterInstance newCharacter)
    {
        // Update party to reflect the switch
        int currentIndex = alivePartyMembers.FindIndex(chr => chr.UniqueCharacterId == oldCharacter.UniqueCharacterId);
        int newIndex = alivePartyMembers.FindIndex(chr => chr.UniqueCharacterId == newCharacter.UniqueCharacterId);
        (alivePartyMembers[currentIndex], alivePartyMembers[newIndex]) = (alivePartyMembers[newIndex], alivePartyMembers[currentIndex]);

        // Update playerTeam to reflect the switch
        currentIndex = playerTeam.FindIndex(chr => chr.UniqueCharacterId == oldCharacter.UniqueCharacterId);
        playerTeam[currentIndex] = alivePartyMembers.Find(chr => chr.UniqueCharacterId == newCharacter.UniqueCharacterId);

        // Update characters to reflect the switch
        characters[characters.FindIndex(chr => chr.UniqueCharacterId == oldCharacter.UniqueCharacterId)] = newCharacter;

        // Update the character UI to reflect the switch
        oldCharacter.CharacterUI = null;
        playerTeamUI.SetTeam(playerTeam.ConvertAll(chr => (CharacterInstance)chr));
    }

    /// <summary>
    /// Activate an inactive party member. Assumes that only one party member is already active.
    /// </summary>
    /// <param name="character">The party member to activate.</param>
    private void ActivatePartyMember(CharacterInstance character)
    {
        int partyIndex = alivePartyMembers.FindIndex(chr => chr.UniqueCharacterId == character.UniqueCharacterId);
        playerTeam.Add(alivePartyMembers[partyIndex]);

        // if the third or fourth party member was selected, swap it with the second member,
        // since the first two should always be the two that are active
        if (partyIndex > 1)
            (alivePartyMembers[partyIndex], alivePartyMembers[1]) = (alivePartyMembers[1], alivePartyMembers[partyIndex]);

        characters.Add(character);
        playerTeamUI.SetTeam(playerTeam.ConvertAll(chr => (CharacterInstance)chr));
    }
    
    private IEnumerator ShowQueuedDialogue()
    {
        foreach (string msg in dialogueQueue)
            yield return dialogueBox.TypeDialogue(msg);
        dialogueQueue.Clear();
    }


    /// <summary>
    /// Get the character's that will be targeted by a move effect based on the provided target type, the user of the move, and primary target.
    /// </summary>
    /// <param name="targetType">The target type of the effect being used.</param>
    /// <param name="user">The character using the move.</param>
    /// <param name="target">The primary target of that was already determined. Certain target types may not use this.</param>
    /// <returns>The characters that will be targeted.</returns>
    public List<CharacterInstance> GetEffectTargets(BattleTargettingType targetType, CharacterInstance user, CharacterInstance target)
    {
        List<CharacterInstance> targets = new List<CharacterInstance>();
        switch (targetType)
        {
            case BattleTargettingType.SingleOther or BattleTargettingType.SingleAlly or BattleTargettingType.SingleEnemy or BattleTargettingType.SingleAny:
                targets.Add(target);
                break;
            case BattleTargettingType.User:
                targets.Add(user);
                break;
            case BattleTargettingType.UserTeam:
                if (user.IsPlayerTeam)
                    targets = playerTeam.ConvertAll(chr => (CharacterInstance)chr);
                else
                    targets = enemyTeam.ConvertAll(chr => (CharacterInstance)chr);
                break;
            case BattleTargettingType.EnemyTeam:
                if (user.IsPlayerTeam)
                    targets = enemyTeam.ConvertAll(chr => (CharacterInstance)chr);
                else
                    targets = playerTeam.ConvertAll(chr => (CharacterInstance)chr);
                break;
            case BattleTargettingType.AnyTeam:
                if (target.IsPlayerTeam)
                    targets = playerTeam.ConvertAll(chr => (CharacterInstance)chr);
                else
                    targets = enemyTeam.ConvertAll(chr => (CharacterInstance)chr);
                break;
            case BattleTargettingType.OppositeTeam:
                if (target.IsPlayerTeam)
                    targets = enemyTeam.ConvertAll(chr => (CharacterInstance)chr);
                else
                    targets = playerTeam.ConvertAll(chr => (CharacterInstance)chr);
                break;
            case BattleTargettingType.AllOther:
                targets = characters.Where(chr => chr != user).ToList();
                break;
            case BattleTargettingType.All:
                targets = characters;
                break;
        }
        return targets;
    }
    
    /// <summary>
    /// Determine and set the turn order for this round based on the current speed stat of each character. Speed ties are handled randomly.
    /// </summary>
    private void DetermineTurnOrder()
    {
        CharacterInstance[] sortedCharacters = new CharacterInstance[characters.Count];
        characters.CopyTo(sortedCharacters);
        // Sort the characters by their speed stats, handling ties randomly
        sortedCharacters = sortedCharacters.OrderByDescending(character => character.Speed).
            ThenByDescending(character => character.SpeedTieBreaker).ToArray();

        turnQueue.Clear();
        foreach (CharacterInstance character in sortedCharacters)
            turnQueue.Enqueue(character);
    }

    /// <summary>
    /// Check if there are currently any dead characters (characters with <= 0 current HP),
    /// <br/>killing them if there are.
    /// </summary>
    /// <returns>Whether or not there are any dead characters.</returns>
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

        if (playerTeam.Count <= 0 || enemyTeam.Count <= 0)
            return true;
        else return false;
    }

    /// <summary>
    /// Kill a character, removing it from all applicable places.
    /// </summary>
    /// <param name="character">The character to kill.</param>
    private void KillCharacter(CharacterInstance character)
    {
        // check if the character is a party member
        if (alivePartyMembers.Exists(chr => chr.UniqueCharacterId == character.UniqueCharacterId))
            partyMemberDiedThisTurn = true;

        characters.Remove(character);

        // remove character from the turn queue
        turnQueue = new Queue<CharacterInstance>(turnQueue.Where(chr => chr != character));
        turnOrderUI.SetTurnOrder(turnQueue);

        if (character.IsPlayerTeam)
        {
            playerTeam.RemoveAll(chr => chr.UniqueCharacterId == character.UniqueCharacterId);
            playerTeamUI.RemoveCharacter(character);

            deadPartyMembers.Add(alivePartyMembers.Where(partyMember => partyMember.UniqueCharacterId == character.UniqueCharacterId).FirstOrDefault());
            alivePartyMembers.RemoveAll(chr => chr.UniqueCharacterId == character.UniqueCharacterId);
        }
        else
        {
            enemyTeam.RemoveAll(chr => chr.UniqueCharacterId == character.UniqueCharacterId);
            enemyTeamUI.RemoveCharacter(character);
        }
    }

    private void ShowActionSelectionScreen()
    {
        SetActionSelectionScreen(true);
        SetMoveSelectionScreen(false);
        SetPartyScreen(false);
        SetItemSelectionScreen(false);
    }

    private void ShowMoveSelectionScreen()
    {
        SetMoveSelectionScreen(true);
        SetActionSelectionScreen(false);
        SetPartyScreen(false);
        SetItemSelectionScreen(false);
    }

    private void ShowPartyScreen()
    {
        SetPartyScreen(true);
        SetActionSelectionScreen(false);
        SetMoveSelectionScreen(false);
        SetItemSelectionScreen(false);
    }

    private void ShowItemSelectionScreen()
    {
        SetItemSelectionScreen(true);
        SetActionSelectionScreen(false);
        SetMoveSelectionScreen(false);
        SetPartyScreen(false);
    }

    private void ShowWinScreen()
    {
        SetWinScreen(true);
        SetPartyScreen(false);
        SetActionSelectionScreen(false);
        SetMoveSelectionScreen(false);
    }

    private void ShowLoseScreen()
    {
        SetLoseScreen(true);
        SetPartyScreen(false);
        SetActionSelectionScreen(false);
        SetMoveSelectionScreen(false);
    }

    /// <summary>
    /// Set whether or not the action selection screen game objects are active.
    /// </summary>
    /// <param name="isActive">Whether or not the action selection screen game objects are active.</param>
    private void SetActionSelectionScreen(bool isActive)
    {
        dialogueBoxGO.SetActive(isActive);
        actionSelectionButtons.SetActive(isActive);
    }

    /// <summary>
    /// Set whether or not the move selection screen game objects are active.
    /// </summary>
    /// <param name="isActive">Whether or not the move selection screen game objects are active.</param>
    private void SetMoveSelectionScreen(bool isActive)
    {
        moveSelectionUIGO.SetActive(isActive);
        moveInfoUIGO.SetActive(isActive);
    }

    /// <summary>
    /// Set whether or not the party screen game objects are active.
    /// </summary>
    /// <param name="isActive">Whether or not the party screen game objects are active.</param>
    private void SetPartyScreen(bool isActive)
    {
        partyMemberSelectionUIGO.SetActive(isActive);
    }

    /// <summary>
    /// Set whether or not the item selection screen game objects are active.
    /// </summary>
    /// <param name="isActive">Whether or not the item selection screen game objects are active.</param>
    private void SetItemSelectionScreen(bool isActive)
    {
        itemSelectionUIGO.SetActive(isActive);
        itemInfoUIGO.SetActive(isActive);
    }

    /// <summary>
    /// Set whether or not the win screen game objects are active.
    /// </summary>
    /// <param name="isActive">Whether or not the win screen game objects are active.</param>
    private void SetWinScreen(bool isActive)
    {
        winScreenUIGO.SetActive(isActive);
    }

    /// <summary>
    /// Set whether or not the lose screen game objects are active.
    /// </summary>
    /// <param name="isActive">Whether or not the lose screen game objects are active.</param>
    private void SetLoseScreen(bool isActive)
    {
        loseScreenUIGO.SetActive(isActive);
    }

    /// <summary>
    /// Add an active party member and apply all encounter modifiers to it.
    /// </summary>
    /// <param name="partyMember">The party member to add.</param>
    private void AddActivePartyMember(PartyMemberInstance partyMember)
    {
        playerTeam.Add(partyMember);
        characters.Add(partyMember);

        // Apply status effects of encounter modifiers
        foreach (EncounterModifierData modifier in encounterManager.CurrentEncounter.EncounterData.Modifiers)
        {
            foreach (StatusEffectData effect in modifier.Effects)
                partyMember.ApplyStatusEffect(new StatusEffectInstance(effect, 99, 0, null, partyMember), false);
        }
    }

    /// <summary>
    /// Add an enemy, initialize it, and apply all encounter modifiers to it.
    /// </summary>
    /// <param name="enemy">The enemy to add.</param>
    private void AddEnemy(EnemyInstance enemy)
    {
        enemy.Init();  // enemies need to be initialized somewhere, here is the place that makes sense to do so
        enemy.IsPlayerTeam = false;

        enemyTeam.Add(enemy);
        characters.Add(enemy);

        // Apply status effects of encounter modifiers
        foreach (EncounterModifierData modifier in encounterManager.CurrentEncounter.EncounterData.Modifiers)
        {
            foreach (StatusEffectData effect in modifier.Effects)
                enemy.ApplyStatusEffect(new StatusEffectInstance(effect, 99, 0, null, enemy), false);
        }
    }

    /// <summary>
    /// Set the round to a provided value and update the UI to reflect the new value.
    /// </summary>
    /// <param name="round">The new round value.</param>
    private void SetRound(int round)
    {
        this.round = round;
        encounterInfoUI.SetRound(this.round);
    }
}
