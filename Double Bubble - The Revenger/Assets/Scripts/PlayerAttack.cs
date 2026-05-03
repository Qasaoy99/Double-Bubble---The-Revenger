using UnityEngine;

/// <summary>
/// Button-driven melee attack. OverlapBox is cheaper than a trigger collider
/// because the hitbox only exists for a single frame and is layer-filtered.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode attackKey = KeyCode.J;

    [Header("Hitbox")]
    [SerializeField] private Transform attackPoint;                 // empty child placed in front of the player
    [SerializeField] private Vector2 hitboxSize = new Vector2(1.2f, 1f);
    [SerializeField] private LayerMask enemyLayers;                 // set to the "Enemy" layer in inspector
    [SerializeField] private int damage = 10;
    [SerializeField] private float cooldown = 0.45f;

    [Header("Animation (optional)")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackTrigger = "Attack";

    [Header("Facing (optional)")]
    [SerializeField] private SpriteRenderer spriteRenderer;         // used to mirror the hitbox when facing left

    private float nextAttackTime;

    private void Update()
    {
        if (Input.GetKeyDown(attackKey) && Time.time >= nextAttackTime)
        {
            PerformAttack();
            nextAttackTime = Time.time + cooldown;
        }
    }

    /// <summary>
    /// Public so an Animation Event can call it for frame-perfect damage instead of on button press.
    /// If called from an Animation Event, remove the PerformAttack() call inside Update and only trigger the animator.
    /// </summary>
    public void PerformAttack()
    {
        if (animator) animator.SetTrigger(attackTrigger);

        Vector2 origin = GetHitboxCenter();
        // OverlapBoxAll returns every collider inside the box on the given layers; we filter to Health holders.
        Collider2D[] hits = Physics2D.OverlapBoxAll(origin, hitboxSize, 0f, enemyLayers);
        for (int i = 0; i < hits.Length; i++)
        {
            var hp = hits[i].GetComponentInParent<Health>();
            if (hp != null) hp.TakeDamage(damage);
        }
    }

    private Vector2 GetHitboxCenter()
    {
        if (!attackPoint) return transform.position;

        // Mirror the local offset when sprite is flipped, so the hitbox stays in front of the player regardless of facing.
        if (spriteRenderer && spriteRenderer.flipX)
        {
            Vector3 local = attackPoint.localPosition;
            local.x = -local.x;
            return transform.TransformPoint(local);
        }
        return attackPoint.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 center = Application.isPlaying ? GetHitboxCenter()
                                               : (attackPoint ? (Vector2)attackPoint.position : (Vector2)transform.position);
        Gizmos.DrawWireCube(center, hitboxSize);
    }
}
