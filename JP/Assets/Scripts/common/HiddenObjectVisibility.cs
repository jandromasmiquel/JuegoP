using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HiddenObjectVisibility : MonoBehaviour
{
    [SerializeField] private FieldOfView fieldOfView;
    [SerializeField] private bool checkBoundsCorners = true;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (fieldOfView == null)
        {
            spriteRenderer.enabled = false;
            return;
        }

        spriteRenderer.enabled = IsVisibleFromFieldOfView();
    }

    private bool IsVisibleFromFieldOfView()
    {
        Bounds bounds = spriteRenderer.bounds;

        if (fieldOfView.CanSeePoint(bounds.center))
        {
            return true;
        }

        if (!checkBoundsCorners)
        {
            return false;
        }

        return fieldOfView.CanSeePoint(new Vector2(bounds.min.x, bounds.min.y))
            || fieldOfView.CanSeePoint(new Vector2(bounds.min.x, bounds.max.y))
            || fieldOfView.CanSeePoint(new Vector2(bounds.max.x, bounds.min.y))
            || fieldOfView.CanSeePoint(new Vector2(bounds.max.x, bounds.max.y));
    }
}
