using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Represents a party member on the home screen. 
/// </summary>
public class HomePartyMemberUI : MonoBehaviour, IPointerClickHandler
{
    private PartyMemberInstance _partyMember;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image image;

    private HomePartyUI homePartyUI;

    /// <summary>Gets or sets the party member for this to display info about. Setting automatically updates the UI.</summary>
    public PartyMemberInstance PartyMember
    {
        get { return _partyMember; }
        set
        {
            _partyMember = value;

            nameText.text = _partyMember.CharacterData.Name;
            levelText.text = $"Lvl {_partyMember.Level}";
            image.sprite = _partyMember.CharacterData.Sprite;
        }
    }

    private void Start()
    {
        homePartyUI = FindObjectsByType<HomePartyUI>(FindObjectsSortMode.None)[0];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (HomeManager.Instance.State == HomeState.SwappingPartyMembers)
            homePartyUI.PartyMemberUIClicked(gameObject);
        if (HomeManager.Instance.State == HomeState.TargetSelection)
            HomeManager.Instance.TargetOptionClicked(this);
    }

    public void SwapButtonClicked()
    {
        if (!HomeManager.Instance.IsBusy)
            homePartyUI.SwapButtonClicked(gameObject);
    }
}
