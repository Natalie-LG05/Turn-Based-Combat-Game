using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// A component that makes a scroll rect scroll whenever the mouse wheel is scrolled, even if it isn't being hovered over.
/// </summary>
public class Scrollable : MonoBehaviour
{
    [SerializeField] private float scrollSpeed;
    [SerializeField] private ScrollRect scrollRect;

    private void Update()
    {
        // Scroll the attached scroll view based on scroll wheel input.
        float scrollInputY = Mouse.current.scroll.ReadValue().y;
        if (scrollInputY != 0)
            scrollRect.verticalNormalizedPosition += scrollInputY * scrollSpeed;
    }
}
