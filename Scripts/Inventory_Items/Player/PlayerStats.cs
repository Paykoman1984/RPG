using UnityEngine;
using TMPro;

namespace PoEClone2D
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("Base Stats")]
        public int level = 1;
        public int health = 100;
        public int maxHealth = 100;
        public int strength = 10;
        public int dexterity = 10;
        public int intelligence = 10;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI statsText;

        void Update()
        {
            if (statsText != null)
            {
                statsText.text = $"Lvl: {level}\n" +
                               $"HP: {health}/{maxHealth}\n" +
                               $"Str: {strength}\n" +
                               $"Dex: {dexterity}\n" +
                               $"Int: {intelligence}";
            }
        }

        public void Heal(int amount)
        {
            health = Mathf.Min(maxHealth, health + amount);
            Debug.Log($"Healed {amount} HP");
        }
    }
}