using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A tooltip that displays the status effects that a character has and information about each. It is a scrollable list of all the status effects after the 4th.
/// </summary>
public class EffectListTooltip : Tooltip
{
    [SerializeField] private GameObject effectInfoUIPrefab;

    private List<StatusEffectInstance> effects;

    [SerializeField] private Transform effectUIContainer;

    private List<GameObject> effectInfoUIs;

    protected override void Awake()
    {
        base.Awake();
        effectInfoUIs = new List<GameObject>();
    }

    /// <summary>
    /// Set the data for the tooltip to show based on the object that triggered the tooltip.
    /// <br/>This tooltip is shown when a +x effect icon is hovered,
    /// <br/>so set the data based on the list of status effects attached to that icon.
    /// </summary>
    /// <param name="sourceObject">The game object that triggered the tooltip.</param>
    public override void SetTooltipData(GameObject sourceObject)
    {
        base.SetTooltipData(sourceObject);
        effects = sourceObject.GetComponent<StatusEffectListIcon>().Effects;
        
        // clear any existing data
        foreach (GameObject obj in effectInfoUIs)
            Destroy(obj);
        effectInfoUIs.Clear();

        SetEffects(effects);
    }

    private void SetEffects(List<StatusEffectInstance> effects)
    {
        foreach (StatusEffectInstance effect in effects)
            NewEffect(effect);
    }

    /// <summary>
    /// Add a new entry to the list, displaying info about the provided effect.
    /// </summary>
    /// <param name="effect">The status effect to display info about.</param>
    private void NewEffect(StatusEffectInstance effect)
    {
        GameObject effectInfo = Instantiate(effectInfoUIPrefab, effectUIContainer);
        // prevent Unity screenspace canvas bug that was spawning the object at extreme z positions
        effectInfo.transform.localPosition = new Vector3(effectInfo.transform.localPosition.x, effectInfo.transform.localPosition.y, 0);
        effectInfoUIs.Add(effectInfo);
        effectInfo.GetComponent<EffectInfoUI>().Effect = effect;
    }
}
