// Assets/Scripts/Combat/StatSystem.cs
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityStats
{
    // Core Stats
    public float maxHealth = 100;
    public float health = 100;
    public float armor = 0;
    public float evasion = 0;
    public float blockChance = 0;

    // Resistances (0-75 default like PoE, can be increased)
    public float fireResistance = 0;
    public float coldResistance = 0;
    public float lightningResistance = 0;
    public float chaosResistance = 0;

    // Offensive
    public float physicalDamage = 10;
    public float attackSpeed = 1.0f;
    public float criticalChance = 5.0f;
    public float criticalMultiplier = 150.0f;
    public float accuracy = 100.0f;

    // Movement
    public float movementSpeed = 5.0f;

    // Buffs/Debuffs
    public List<StatusEffect> activeEffects = new List<StatusEffect>();

    public bool IsAlive => health > 0;

    public void TakeDamage(float damage)
    {
        health = Mathf.Max(0, health - damage);
    }

    public void Heal(float amount)
    {
        health = Mathf.Min(maxHealth, health + amount);
    }

    // Apply resistance to elemental damage
    public float ApplyResistance(float damage, DamageType type)
    {
        float resistance = 0;

        switch (type)
        {
            case DamageType.Fire: resistance = fireResistance; break;
            case DamageType.Cold: resistance = coldResistance; break;
            case DamageType.Lightning: resistance = lightningResistance; break;
            case DamageType.Chaos: resistance = chaosResistance; break;
        }

        // Cap at 75% like PoE (unless you want to allow overcapping)
        resistance = Mathf.Min(resistance, 75);

        return damage * (1 - resistance / 100f);
    }
}

public enum DamageType
{
    Physical,
    Fire,
    Cold,
    Lightning,
    Chaos
}

[System.Serializable]
public class StatusEffect
{
    public string effectId;
    public float duration;
    public float value;
    public StatusEffectType type;

    public StatusEffect(string id, float duration, float value, StatusEffectType type)
    {
        this.effectId = id;
        this.duration = duration;
        this.value = value;
        this.type = type;
    }
}

public enum StatusEffectType
{
    Buff,
    Debuff,
    Aura,
    Curse
}