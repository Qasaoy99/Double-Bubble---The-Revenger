using UnityEngine;

/// <summary>
/// Child trigger on an enemy that only damages during specific animation frames.
/// Animation Events call EnableHitbox/DisableHitbox so the "attack animation"
/// actually deals damage instead of just playing visually.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyMeleeHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask targetLayers;   // set to "Player" layer
    [SerializeField] private Health ownerHealth;       // optional: stops attacking after enemy dies

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        col.enabled = false; // starts disabled — only the animation turns it on during the strike window
    }

    private void OnEnable()
    {
        if (ownerHealth != null) ownerHealth.OnDied += HandleOwnerDied;
    }

    private void OnDisable()
    {
        if (ownerHealth != null) ownerHealth.OnDied -= HandleOwnerDied;
    }

    // Animation Event — place on the first frame of the strike window.
    public void EnableHitbox()
    {
        if (ownerHealth != null && ownerHealth.IsDead) return;
        col.enabled = true;
    }

    // Animation Event — place on the last frame of the strike window.
    public void DisableHitbox() => col.enabled = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Bitmask check: is "other"'s layer inside targetLayers?
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

        var hp = other.GetComponentInParent<Health>();
        if (hp != null) hp.TakeDamage(damage);
    }

    private void HandleOwnerDied() => col.enabled = false;
}
