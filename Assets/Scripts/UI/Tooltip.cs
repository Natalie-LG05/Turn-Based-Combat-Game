using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A base class for tooltips. Tooltips are UI objects that display information about something. Tooltips follow the mouse, changing their pivot based on which horizontal and vertical halves of the screen they are located in. 
/// </summary>
public class Tooltip : MonoBehaviour
{
    private RectTransform rectTransform;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        GoToMousePosition();
    }

    /// <summary>
    /// Set the data for the tooltip to show based on the object that triggered the tooltip.
    /// <br/>Currently does nothing, must be overridden by subclasses
    /// </summary>
    /// <param name="sourceObject">The game object that triggered the tooltip.</param>
    public virtual void SetTooltipData(GameObject sourceObject)
    {
        
    }

    /// <summary>
    /// Place the tooltip at the mouse's position and set its pivot based on which halves of the screen it is in.
    /// </summary>
    protected void GoToMousePosition()
    {
        // Set the tooltip's position to the mouse position
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Change the pivot based on which horizontal and vertical halves of the screen the mouse is on
        float pivotX = mousePosition.x > Screen.width / 2 ? 1 : 0;
        float pivotY = mousePosition.y > Screen.height / 2 ? 1 : 0;
        rectTransform.pivot = new Vector2(pivotX, pivotY);

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        transform.position = new Vector3(worldPosition.x, worldPosition.y, 0);  // make sure the z value is 0
    }
}
