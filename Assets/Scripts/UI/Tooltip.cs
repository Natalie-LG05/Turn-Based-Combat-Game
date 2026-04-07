using UnityEngine;
using UnityEngine.InputSystem;

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

    public virtual void SetTooltipData(GameObject sourceObject)
    {
        
    }

    private void GoToMousePosition()
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
