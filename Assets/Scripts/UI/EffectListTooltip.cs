using System.Collections.Generic;
using UnityEngine;

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

    public override void SetTooltipData(GameObject sourceObject)
    {
        base.SetTooltipData(sourceObject);
        effects = sourceObject.GetComponent<StatusEffectListIcon>().Effects;
        
        foreach (GameObject obj in effectInfoUIs)
            Destroy(obj);
        effectInfoUIs.Clear();

        SetEffects(effects);
    }

    private void SetEffects(List<StatusEffectInstance> effects)
    {
        if (effects.Count > 0)
        {
            foreach (StatusEffectInstance effect in effects)
            {
                NewEffect(effect);
            }
        }
    }

    private void NewEffect(StatusEffectInstance effect)
    {
        GameObject effectInfo = Instantiate(effectInfoUIPrefab, effectUIContainer);
        effectInfo.transform.localPosition = new Vector3(effectInfo.transform.localPosition.x, effectInfo.transform.localPosition.y, 0);
        effectInfoUIs.Add(effectInfo);
        effectInfo.GetComponent<EffectInfoUI>().Effect = effect;
    }
}
