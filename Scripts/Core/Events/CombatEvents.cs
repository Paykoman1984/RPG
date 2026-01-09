// Assets/Scripts/Core/Events/CombatEvents.cs
using UnityEngine;

public static class CombatEvents
{
    // Damage Events
    public delegate void DamageEvent(DamageInfo damageInfo, GameObject target, float damageDealt);
    public static event DamageEvent OnDamageDealt;
    public static event DamageEvent OnDamageReceived;

    // Health Events
    public delegate void HealthEvent(GameObject entity, float current, float max);
    public static event HealthEvent OnHealthChanged;

    // Entity Death Event - SIMPLIFIED: Just GameObject parameter
    public delegate void EntityDeathEvent(GameObject entity);
    public static event EntityDeathEvent OnEntityDied;

    // Hit Validation Events
    public delegate void HitValidationEvent(DamageInfo damageInfo, GameObject target, bool wasEvaded, bool wasBlocked);
    public static event HitValidationEvent OnHitValidated;

    // Combat State
    public delegate void CombatStateEvent(GameObject entity, bool inCombat);
    public static event CombatStateEvent OnEnterCombat;
    public static event CombatStateEvent OnExitCombat;

    // Public Triggers
    public static void TriggerDamageDealt(DamageInfo info, GameObject target, float damage) =>
        OnDamageDealt?.Invoke(info, target, damage);

    public static void TriggerDamageReceived(DamageInfo info, GameObject target, float damage) =>
        OnDamageReceived?.Invoke(info, target, damage);

    public static void TriggerHealthChanged(GameObject entity, float current, float max) =>
        OnHealthChanged?.Invoke(entity, current, max);

    public static void TriggerEntityDied(GameObject entity) =>
        OnEntityDied?.Invoke(entity);

    public static void TriggerHitValidated(DamageInfo info, GameObject target, bool evaded, bool blocked) =>
        OnHitValidated?.Invoke(info, target, evaded, blocked);

    public static void TriggerEnterCombat(GameObject entity) =>
        OnEnterCombat?.Invoke(entity, true);

    public static void TriggerExitCombat(GameObject entity) =>
        OnExitCombat?.Invoke(entity, false);
}