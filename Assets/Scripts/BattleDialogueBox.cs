using System.Collections;
using TMPro;
using UnityEngine;

public class BattleDialogueBox : MonoBehaviour
{
    [SerializeField] private int charsPerSec;

    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float dialogueDuration;

    public void SetDialogue(string dialogue)
    {
        dialogueText.text = dialogue;
    }

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
