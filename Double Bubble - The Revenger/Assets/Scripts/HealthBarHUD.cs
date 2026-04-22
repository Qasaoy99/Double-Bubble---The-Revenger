using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Screen-space health bar for the player.
/// Subscribes to Health events instead of polling in Update — saves work when HP doesn't change.
/// </summary>
public class HealthBarHUD : MonoBehaviour
{
    [SerializeField] private Health target;
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;                 // optional — color tint based on % HP
    [SerializeField] private Gradient healthGradient;         // green → yellow → red, configured in inspector

    private void Start()
    {
        if (!target || !slider) { enabled = false; return; }

        slider.maxValue = target.MaxHP;
        slider.value    = target.CurrentHP;
        ApplyColor(target.CurrentHP, target.MaxHP);

        target.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDestroy()
    {
        // Prevents a dangling reference if the HUD is destroyed before the target.
        if (target != null) target.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int current, int max)
    {
        slider.maxValue = max;
        slider.value    = current;
        ApplyColor(current, max);
    }

    private void ApplyColor(int current, int max)
    {
        if (!fillImage || healthGradient == null || max <= 0) return;
        fillImage.color = healthGradient.Evaluate((float)current / max);
    }
}
