using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BattleState { Setup, ActionSelection, MoveSelection, TargetSelection, PartyScreen, ReplaceDeadPartyMember, EnemyTurn, PerformMove, RoundEnd, Busy }

/// <summary>
/// Manages battle, containing the main gameplay loop during combat, handling much of the logic.
/// </summary>
public class BattleManager : MonoBehaviour
{
    private int wave;
    private int round;

    public static BattleManager Instance { get; private set; }
    private BattleState state;
    /// <summary>Gets the character who's turn it currently is.</summary>
    public CharacterInstance CurrentCharacter { get { return turnQueue.Peek(); } }

    private List<PartyMemberInstance> party;

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
    private PartyMemberSelectionUI partyMemberSelectionUI;

    [SerializeField] private GameObject dialogueBoxGO;
    [SerializeField] private GameObject actionSelectionButtons;
    [SerializeField] private GameObject moveSelectionUIGO;
    [SerializeField] private GameObject moveInfoUIGO;
    [SerializeField] private GameObject partyMemberSelectionUIGO;

    private MoveData selectedMove;
    private bool partyMemberDiedThisTurn;

    // Unity game object methods
    private void Awake()
    {
        party = new List<PartyMemberInstance>();

        playerTeam = new List<PartyMemberInstance>();
        enemyTeam = new List<EnemyInstance>();
        characters = new List<CharacterInstance>();
        turnQueue = new Queue<CharacterInstance>();

        dialogueBox = dialogueBoxGO.GetComponent<BattleDialogueBox>();
        moveSelectionUI = moveSelectionUIGO.GetComponent<MoveSelectionUI>();
        partyMemberSelectionUI = partyMemberSelectionUIGO.GetComponent<PartyMemberSelectionUI>();

        dialogueQueue = new Queue<string>();

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
                // pressing escape cancels the move selection that was made and returns to the move selection screen
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    ShowMoveSelectionScreen();
                    state = BattleState.MoveSelection;
                }
                break;
            case BattleState.MoveSelection or BattleState.PartyScreen:
                // pressing escape is the same as clicking the back button
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    BackButtonClicked();
                    state = BattleState.ActionSelection;
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

    // public methods for buttons to call
    /// <summary>
    /// When the move button is clicked during action selection, change to move selection and show the move selection screen.
    /// </summary>
    public void MoveButtonClicked()
    {
        if (state == BattleState.ActionSelection)
        {
            state = BattleState.MoveSelection;

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
        if (state == BattleState.MoveSelection)
        {
            // these target options don't rely on the selected target, so simply use the move
            // with the user as the target, otherwise the player needs to select a target
            if (move.Target is MoveTarget.User or MoveTarget.UserTeam or MoveTarget.EnemyTeam or MoveTarget.All or MoveTarget.AllOther)
                StartCoroutine(PlayerMove(move, CurrentCharacter));
            else
            {
                state = BattleState.TargetSelection;

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

    /// <summary>
    /// When the switch button is clicked during action selection, change to party member selection and show the party screen.
    /// </summary>
    public void SwitchButtonClicked()
    {
        if (state == BattleState.ActionSelection)
        {
            state = BattleState.PartyScreen;

            // show the party screen and update the UI
            ShowPartyScreen();
            partyMemberSelectionUI.SetParty(party, partyMemberDiedThisTurn);  // TODO: keep track of and show dead party members
        }
    }

    /// <summary>
    /// When a valid party member option is clicked during party member selection, 
    /// <br/>switch that character in, deactivating the current party member (if applicable).
    /// </summary>
    /// <param name="character"></param>
    public void PartyMemberOptionClicked(CharacterInstance character)
    {
        if (state == BattleState.PartyScreen)
        {
            StartCoroutine(PlayerSwitch(CurrentCharacter, character));
        } else if (state == BattleState.ReplaceDeadPartyMember)
        {
            ActivatePartyMember(character);
            ShowActionSelectionScreen();

            // signal that the dead party member has been handled and return to ending the round
            partyMemberDiedThisTurn = false;
            EndRound();
        }
    }

    /// <summary>
    /// When the back button is clicked during move selection or party member selection, change to action selection and show the action selection screen.
    /// </summary>
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
    /// <summary>
    /// Handle the logic for starting battle.
    /// </summary>
    private void SetupBattle()
    {
        state = BattleState.Setup;

        wave = 1;
        SetRound(1);
        partyMemberDiedThisTurn = false;

        // Set encounter info UI
        encounterInfoUI.SetEncounter(encounterManager.CurrentEncounter);

        // setup the player's team based on their party
        party = partyManager.PartyMembers;
        foreach (PartyMemberInstance partyMember in party)
            partyMember.IsPlayerTeam = true;

        // start with the first two party members active
        AddActivePartyMember(party[0]);  // party should always have at least 1 member in it
        if (party.Count > 1)
            AddActivePartyMember(party[1]);
        playerTeamUI.SetTeam(playerTeam.ConvertAll(parMem => (CharacterInstance)parMem));

        // spawn the first wave of enemies from the current encounter
        SpawnEnemyWave(wave);

        // signal to each character that the battle has started
        foreach (CharacterInstance character in characters)
            character.BattleStart();

        // Apply status effects of encounter modifiers to inactive party members
        foreach (EncounterModifierData modifier in encounterManager.CurrentEncounter.Modifiers)
        {
            foreach (StatusEffectData effect in modifier.Effects)
            {
                if (party.Count > 2) party[2].ApplyStatusEffect(new StatusEffectInstance(effect, 99, 0, null, party[2]), false);
                if (party.Count > 3) party[3].ApplyStatusEffect(new StatusEffectInstance(effect, 99, 0, null, party[3]), false);
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
            state = BattleState.ReplaceDeadPartyMember;

            if (party.Count > 1)
            {
                ShowPartyScreen();
                partyMemberSelectionUI.SetParty(party, partyMemberDiedThisTurn);
            } else
            {
                // there are no inactive party members left, so just go back to ending the round as normal
                partyMemberDiedThisTurn = false;
                EndRound();
            }
        } else
        {
            state = BattleState.RoundEnd;

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
                state = BattleState.ActionSelection;
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
            if (playerTeam.Count > 0 && encounterManager.CurrentEncounter.Waves.Count() > wave)
            {
                SpawnEnemyWave(++wave);
                StartRound();
            }
#if UNITY_EDITOR
            else
                Debug.Log(playerTeam.Count <= 0 ? "Enemies Win!" : "Player Wins!");
#endif
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
    /// Handle the logic for an enemy taking its turn.
    /// </summary>
    /// <param name="currentCharacter">The character who's turn it currently is.</param>
    private IEnumerator EnemyTurn(CharacterInstance currentCharacter)
    {
        state = BattleState.EnemyTurn;
        EnemyInstance enemy = enemyTeam.Where(enemy => enemy.UniqueCharacterId == currentCharacter.UniqueCharacterId).First();

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
        foreach (EnemyInstance enemy in encounterManager.CurrentEncounter.Waves[waveNum - 1].Enemies)
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
        BattleState prevState = state;
        state = BattleState.PerformMove;

        yield return dialogueBox.TypeDialogue($"{user.CharacterData.Name} (lvl {user.Level}) used {move.Name}.");

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

        state = prevState;
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
        if (move.Target is MoveTarget.User or MoveTarget.UserTeam or MoveTarget.EnemyTeam or MoveTarget.All or MoveTarget.AllOther)
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
            {
                return action;
            }
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
        int currentIndex = party.FindIndex(chr => chr.UniqueCharacterId == oldCharacter.UniqueCharacterId);
        int newIndex = party.FindIndex(chr => chr.UniqueCharacterId == newCharacter.UniqueCharacterId);
        (party[currentIndex], party[newIndex]) = (party[newIndex], party[currentIndex]);

        // Update playerTeam to reflect the switch
        currentIndex = playerTeam.FindIndex(chr => chr.UniqueCharacterId == oldCharacter.UniqueCharacterId);
        playerTeam[currentIndex] = party.Find(chr => chr.UniqueCharacterId == newCharacter.UniqueCharacterId);

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
        int partyIndex = party.FindIndex(chr => chr.UniqueCharacterId == character.UniqueCharacterId);
        playerTeam.Add(party[partyIndex]);

        // if the third party member was selected, swap it with the second member,
        // since the first two should always be the two that are active
        if (partyIndex > 1)
            (party[partyIndex], party[1]) = (party[1], party[partyIndex]);

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
        if (party.Exists(chr => chr.UniqueCharacterId == character.UniqueCharacterId))
            partyMemberDiedThisTurn = true;

        characters.Remove(character);

        // remove character from the turn queue
        turnQueue = new Queue<CharacterInstance>(turnQueue.Where(chr => chr != character));
        turnOrderUI.SetTurnOrder(turnQueue);

        if (character.IsPlayerTeam)
        {
            playerTeam.RemoveAll(chr => chr.UniqueCharacterId == character.UniqueCharacterId);
            party.RemoveAll(chr => chr.UniqueCharacterId == character.UniqueCharacterId);
            playerTeamUI.RemoveCharacter(character);
        }
        else
        {
            enemyTeam.RemoveAll(chr => chr.UniqueCharacterId == character.UniqueCharacterId);
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
        else if (state == BattleState.PartyScreen || state == BattleState.ReplaceDeadPartyMember)
        {
            dialogueBoxGO.SetActive(true);
            actionSelectionButtons.SetActive(true);
            partyMemberSelectionUIGO.SetActive(false);
        }
    }

    private void ShowMoveSelectionScreen()
    {
        dialogueBoxGO.SetActive(false);
        actionSelectionButtons.SetActive(false);
        moveSelectionUIGO.SetActive(true);
        moveInfoUIGO.SetActive(true);
    }

    private void ShowPartyScreen()
    {
        dialogueBoxGO.SetActive(false);
        actionSelectionButtons.SetActive(false);
        partyMemberSelectionUIGO.SetActive(true);
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
        foreach (EncounterModifierData modifier in encounterManager.CurrentEncounter.Modifiers)
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
        foreach (EncounterModifierData modifier in encounterManager.CurrentEncounter.Modifiers)
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
