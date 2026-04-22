using System;
using UnityEngine;

/// <summary>
/// Shared health component for any damageable entity (Player, Goblin, etc.).
/// Other systems (UI, SFX, AI) subscribe to events instead of polling CurrentHP.
/// </summary>
public class Health : MonoBehaviour
{
    [SerializeField] private int maxHP = 100;
    [SerializeField] private float invulnerabilityTime = 0.4f; // i-frames prevent multi-hit from the same swing

    public int MaxHP => maxHP;
    public int CurrentHP { get; private set; }
    public bool IsDead => CurrentHP <= 0;

    // (current, max) — max sent so UI can rebuild if maxHP changes at runtime (buffs, level-up)
    public event Action<int, int> OnHealthChanged;
    public event Action OnDamaged;
    public event Action OnDied;

    private float lastHitTime = -999f;

    private void Awake()
    {
        CurrentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;
        if (Time.time - lastHitTime < invulnerabilityTime) return;
        lastHitTime = Time.time;

        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
        OnDamaged?.Invoke();

        if (IsDead) OnDied?.Invoke();
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }

    // Force-fires the event after Start so late-subscribed UIs sync their initial value.
    public void ForceNotify() => OnHealthChanged?.Invoke(CurrentHP, maxHP);
}
