using System;

namespace PoEClone2D.Items
{
    [Serializable]
    public class StatModifier
    {
        public string statName;
        public float value;

        // Optional: Add more properties for advanced stat systems
        public StatModifierType modifierType = StatModifierType.Flat;

        public enum StatModifierType
        {
            Flat,           // Add directly to base value
            Percentage,     // Percentage of base value
            More,           // Multiplicative (PoE-style "more" modifier)
            Increased       // Additive percentage (PoE-style "increased" modifier)
        }

        // Constructor for convenience
        public StatModifier(string name, float val, StatModifierType type = StatModifierType.Flat)
        {
            statName = name;
            value = val;
            modifierType = type;
        }

        public override string ToString()
        {
            string sign = value >= 0 ? "+" : "";
            string typeSymbol = modifierType == StatModifierType.Percentage ? "%" : "";
            return $"{sign}{value}{typeSymbol} {statName}";
        }
    }
}