using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the UI for the move selection screen.
/// </summary>
public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject moveOptionUIPrefab;

    private CharacterInstance character;

    [SerializeField] private Transform moveOptionContainer;
    private List<GameObject> moveOptionUIs;

    [SerializeField] private MoveInfoUI _moveInfoUI;

    /// <summary>Gets the move info UI assigned to this object.</summary>
    public MoveInfoUI MoveInfoUI { get => _moveInfoUI; }

    private void Awake()
    {
        moveOptionUIs = new List<GameObject>();
    }

    /// <summary>
    /// Set the character who's moves to show, and display its moves.
    /// </summary>
    /// <param name="characterInstance">The character who's moves to show.</param>
    public void SetCharacter(CharacterInstance characterInstance)
    {
        character = characterInstance;
        ShowMoveOptions();
    }

    /// <summary>
    /// Display interactable move options based on the attached character's moveset.
    /// </summary>
    private void ShowMoveOptions()
    {
        // clear old data
        foreach (GameObject go in moveOptionUIs)
            Destroy(go);
        moveOptionUIs.Clear();

        foreach (MoveData move in character.Moveset)
            NewMoveOption(move);

        // select the first move by default
        _moveInfoUI.Move = moveOptionUIs[0].GetComponent<MoveOptionUI>().Move;
    }

    /// <summary>
    /// Add a new interactable move option to the list.
    /// </summary>
    /// <param name="move">The move to be attached to that move option.</param>
    private void NewMoveOption(MoveData move)
    {
        GameObject moveOptionUI = Instantiate(moveOptionUIPrefab, moveOptionContainer);
        moveOptionUI.GetComponent<MoveOptionUI>().Move = move;
        moveOptionUIs.Add(moveOptionUI);
    }
}
