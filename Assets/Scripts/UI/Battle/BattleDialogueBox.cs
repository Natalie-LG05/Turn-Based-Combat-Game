using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles showing dialogue in the dialogue box. Dialogue can either be instantly set, or smoothly typed out.
/// </summary>
public class BattleDialogueBox : MonoBehaviour
{
    [SerializeField] private int charsPerSec;

    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float dialogueDuration;

    public void SetDialogue(string dialogue)
    {
        dialogueText.text = dialogue;
    }

    /// <summary>
    /// Type the dialogue instead of just instantly setting it.
    /// </summary>
    /// <param name="dialogue">The dialogue to show.</param>
    public IEnumerator TypeDialogue(string dialogue)
    {
        dialogueText.text = "";
        foreach (char c in dialogue.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(1f / charsPerSec);
        }

        // give the user time to read the dialogue
        yield return new WaitForSeconds(dialogueDuration);
    }
}
