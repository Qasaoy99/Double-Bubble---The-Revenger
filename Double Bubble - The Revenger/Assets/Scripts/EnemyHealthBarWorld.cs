using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World-space health bar for enemies driven by EnemyController.
/// نسخة موازية لـ HealthBarWorld (اللي يشتغل مع Health component) — هذي تسمع أحداث EnemyController مباشرة.
/// </summary>
public class EnemyHealthBarWorld : MonoBehaviour
{
    [SerializeField] private EnemyController target;
    [SerializeField] private Slider slider;
    [SerializeField] private Transform follow;                      // عادةً transform العدو نفسه
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private bool hideOnDeath = true;
    [SerializeField] private Canvas canvasRoot;                     // الـ World Space Canvas عشان نشغّله/نطفيه

    private void Start()
    {
        if (!target || !slider) { enabled = false; return; }

        slider.maxValue = target.MaxHealth;
        slider.value    = target.CurrentHealth;

        target.OnHealthChanged += HandleHealthChanged;
        if (hideOnDeath) target.OnDied += HandleDied;

        SetVisible(!(hideWhenFull && target.CurrentHealth >= target.MaxHealth));
    }

    private void LateUpdate()
    {
        // LateUpdate يضمن إن الشريط يتبع العدو بعد حركته في نفس الفريم (بدون تأخير)
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
