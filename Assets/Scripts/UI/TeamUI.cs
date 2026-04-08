using System.Collections.Generic;
using UnityEngine;

public class TeamUI : MonoBehaviour
{
    [SerializeField] private GameObject characterUIPrefab;

    private List<GameObject> characterUIs;

    [SerializeField] private Transform characterUIContainer;

    private void Awake()
    {
        characterUIs = new List<GameObject>();
    }

    public void SetTeam(List<CharacterInstance> characters)
    {
        // Delete characters (if any)
        if (characterUIs.Count > 0)
        {
            characterUIs.RemoveRange(0, characterUIs.Count);
        }

        if (characters.Count > 0)
        {
            foreach (CharacterInstance character in characters)
            {
                GameObject characterUI = Instantiate(characterUIPrefab, characterUIContainer);
                characterUIs.Add(characterUI);
                characterUI.GetComponent<CharacterUI>().SetCharacter(character);
            }
        }
    }

    public void RemoveCharacter(CharacterInstance character)
    {
        GameObject charUI = character.CharacterUI.gameObject;
        characterUIs.Remove(charUI);
        Destroy(charUI);
    }
}
