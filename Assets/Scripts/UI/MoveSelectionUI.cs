using System.Collections.Generic;
using UnityEngine;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject moveOptionUIPrefab;

    private CharacterInstance character;

    [SerializeField] private Transform moveOptionContainer;
    private List<GameObject> moveOptionUIs;

    [SerializeField] private MoveInfoUI _moveInfoUI;

    public MoveInfoUI MoveInfoUI_ { get => _moveInfoUI; }

    private void Awake()
    {
        moveOptionUIs = new List<GameObject>();
    }

    public void SetCharacter(CharacterInstance characterInstance)
    {
        character = characterInstance;
        ShowMoveOptions();
    }

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

    private void NewMoveOption(MoveData move)
    {
        GameObject moveOptionUI = Instantiate(moveOptionUIPrefab, moveOptionContainer);
        moveOptionUI.GetComponent<MoveOptionUI>().Move = move;
        moveOptionUIs.Add(moveOptionUI);
    }
}
