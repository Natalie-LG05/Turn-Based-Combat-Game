using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A UI showing up to the next five characters in the current turn order. Each character is represented by a CharacterIcon.
/// </summary>
public class TurnOrderUI : MonoBehaviour
{
    [SerializeField] GameObject characterIconPrefab;

    private Queue<GameObject> characterIcons;

    [SerializeField] private Transform characterIconContainer;

    private void Awake()
    {
        characterIcons = new Queue<GameObject>();
    }
    
    /// <summary>
    /// Set the turn order and display it.
    /// </summary>
    /// <param name="turnQueue">The turn queue to display.</param>
    public void SetTurnOrder(Queue<CharacterInstance> turnQueue)
    {
        // remove old data
        foreach (GameObject icon in characterIcons)
            Destroy(icon);
        characterIcons.Clear();

        if (turnQueue.Count > 0)
        {
            for (int i = 0; i < turnQueue.Count; i++)
            {
                GameObject icon = Instantiate(characterIconPrefab, characterIconContainer);
                characterIcons.Enqueue(icon);
                icon.GetComponent<CharacterIcon>().Character = turnQueue.ElementAt(i);

                if (i > 4) icon.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Dequeue and destroy the icon of the character that just finished their turn.
    /// <br/>To be called when a character's turn ends. 
    /// </summary>
    public void NextTurn()
    {
        if (characterIcons.Count <= 0) return;

        // Destroy the first character icon since it's turn is over
        GameObject icon = characterIcons.Dequeue();
        icon.GetComponent<CharacterIcon>().DeselectCharacter();
        Destroy(icon);

        if (characterIcons.Count > 4)
        {
            // Enable the next character icon (now the 5th since one was just deleted) 
            characterIcons.ElementAt(4).SetActive(true);
        }
    }
}
