using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Scrollable : MonoBehaviour
{
    [SerializeField] private float scrollSpeed;
    [SerializeField] private ScrollRect scrollRect;

    private void Update()
    {
        float scrollInputY = Mouse.current.scroll.ReadValue().y;
        if (scrollInputY != 0)
        {
            scrollRect.verticalNormalizedPosition += scrollInputY * scrollSpeed;
        }
    }
}
