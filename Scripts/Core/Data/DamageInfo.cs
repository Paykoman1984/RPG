// Assets/Scripts/Core/Data/DamageInfo.cs
using UnityEngine;

[System.Serializable]
public struct DamageInfo
{
    // Source information
    public GameObject source;
    public string sourceName;
    public bool isPlayer;

    // Damage values
    public float physicalDamage;
    public float fireDamage;
    public float coldDamage;
    public float lightningDamage;
    public float chaosDamage;

    // Combat mechanics
    public bool isCritical;
    public float criticalMultiplier;
    public Vector2 hitDirection;
    public Vector2 hitPoint;

    // PoE-specific flags
    public bool canEvade;
    public bool canBlock;
    public bool ignoreArmor;
    public bool alwaysHit;

    public float damage
    {
        get => GetTotalDamage();
        set => physicalDamage = value; // Set as physical damage for compatibility
    }

    // Constructor for simple damage
    public DamageInfo(GameObject source, float damage)
    {
        this.source = source;
        this.sourceName = source.name;
        this.isPlayer = source.CompareTag("Player");

        this.physicalDamage = damage;
        this.fireDamage = 0;
        this.coldDamage = 0;
        this.lightningDamage = 0;
        this.chaosDamage = 0;

        this.isCritical = false;
        this.criticalMultiplier = 1.5f;
        this.hitDirection = Vector2.zero;
        this.hitPoint = Vector2.zero;

        this.canEvade = true;
        this.canBlock = true;
        this.ignoreArmor = false;
        this.alwaysHit = false;
    }

    public float GetTotalDamage()
    {
        return physicalDamage + fireDamage + coldDamage + lightningDamage + chaosDamage;
    }
}