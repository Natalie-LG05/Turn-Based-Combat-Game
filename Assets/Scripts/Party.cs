using UnityEngine;

public class Party : MonoBehaviour
{
    private PartyMemberData[] partyMembers;

    private void Awake()
    {
        partyMembers = new PartyMemberData[4];
    }
}
