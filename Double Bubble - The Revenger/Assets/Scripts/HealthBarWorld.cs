using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World-space health bar that floats above an enemy.
/// The Canvas should be Render Mode = World Space; this script keeps it positioned
/// above the target and hides it on death / when full (configurable).
/// </summary>
public class HealthBarWorld : MonoBehaviour
{
    [SerializeField] private Health target;
    [SerializeField] private Slider slider;
    [SerializeField] private Transform follow;                   // usually the enemy's root transform
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private bool hideOnDeath  = true;
    [SerializeField] private Canvas canvasRoot;                  // the World Space Canvas to toggle visibility

    private void Start()
    {
        if (!target || !slider) { enabled = false; return; }

        slider.maxValue = target.MaxHP;
        slider.value    = target.CurrentHP;

        target.OnHealthChanged += HandleHealthChanged;
        if (hideOnDeath) target.OnDied += HandleDied;

        SetVisible(!(hideWhenFull && target.CurrentHP >= target.MaxHP));
    }

    private void LateUpdate()
    {
        // LateUpdate ensures the bar follows the enemy after its movement this frame (no 1-frame lag).
        if (follow) transform.position = follow.position + worldOffset;
    }

    private void HandleHealthChanged(int current, int max)
    {
        slider.maxValue = max;
        slider.value    = current;

        if (hideWhenFull) SetVisible(current < max);
        else              SetVisible(current > 0);
    }

    private void HandleDied() => SetVisible(false);

    private void SetVisible(bool visible)
    {
        if (canvasRoot) canvasRoot.enabled = visible;
        else            gameObject.SetActive(visible);
    }

    private void OnDestroy()
    {
        if (target == null) return;
        target.OnHealthChanged -= HandleHealthChanged;
        target.OnDied          -= HandleDied;
    }
}
