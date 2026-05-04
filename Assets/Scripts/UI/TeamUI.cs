using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A UI object that holds the character UIs for a team. A team should only contain up to five characters.
/// </summary>
public class TeamUI : MonoBehaviour
{
    [SerializeField] private GameObject characterUIPrefab;

    private List<GameObject> characterUIs;

    [SerializeField] private Transform characterUIContainer;

    private void Awake()
    {
        characterUIs = new List<GameObject>();
    }

    /// <summary>
    /// Set the characters to show on this team.
    /// </summary>
    /// <param name="characters">A list of the characters on this team.</param>
    public void SetTeam(List<CharacterInstance> characters)
    {
        // Delete old characters (if any)
        foreach (GameObject characterUI in characterUIs)
            Destroy(characterUI);
        characterUIs.Clear();

        foreach (CharacterInstance character in characters)
        {
            NewCharacter(character);
        }
    }

    public void RemoveCharacter(CharacterInstance character)
    {
        GameObject charUI = character.CharacterUI.gameObject;
        characterUIs.Remove(charUI);
        Destroy(charUI);
    }

    public void AddCharacter(CharacterInstance character)
    {
        NewCharacter(character);
    }

    /// <summary>
    /// Create a new character UI and set its data.
    /// </summary>
    /// <param name="character">The character to add and show info about.</param>
    private void NewCharacter(CharacterInstance character)
    {
        GameObject characterUI = Instantiate(characterUIPrefab, characterUIContainer);
        characterUIs.Add(characterUI);
        characterUI.GetComponent<CharacterUI>().SetCharacter(character);
    }
}
