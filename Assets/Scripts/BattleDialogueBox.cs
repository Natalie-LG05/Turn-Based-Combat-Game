using System.Collections;
using TMPro;
using UnityEngine;

public class BattleDialogueBox : MonoBehaviour
{
    [SerializeField] private int charsPerSec;

    [SerializeField] TextMeshProUGUI dialogueText;

    public void SetDialogue(string text)
    {
        dialogueText.text = text;
    }

    public IEnumerator TypeDialogue(string text)
    {
        dialogueText.text = "";
        foreach (char c in text.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(1f / charsPerSec);
        }
    }
}
