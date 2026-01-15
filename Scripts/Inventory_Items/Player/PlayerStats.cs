using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
        [SerializeField] private Slider healthBar;

        // Events for health changes
        public System.Action<int> OnHealthChanged;
        public System.Action<int> OnMaxHealthChanged;

        void Start()
        {
            // Initialize health
            health = maxHealth;
            UpdateUI();

            // Try to find health bar if not set
            if (healthBar == null)
            {
                healthBar = GameObject.Find("HealthBar")?.GetComponent<Slider>();
                if (healthBar != null)
                {
                    healthBar.maxValue = maxHealth;
                    healthBar.value = health;
                }
            }
        }

        void Update()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            // Update text UI
            if (statsText != null)
            {
                statsText.text = $"Lvl: {level}\n" +
                               $"HP: {health}/{maxHealth}\n" +
                               $"Str: {strength}\n" +
                               $"Dex: {dexterity}\n" +
                               $"Int: {intelligence}";
            }

            // Update health bar
            if (healthBar != null)
            {
                healthBar.maxValue = maxHealth;
                healthBar.value = health;
            }
            else
            {
                // Try to find it again if it was null
                healthBar = GameObject.Find("HealthBar")?.GetComponent<Slider>();
            }
        }

        public void Heal(int amount)
        {
            int oldHealth = health;
            health = Mathf.Min(maxHealth, health + amount);
            int healed = health - oldHealth;

            Debug.Log($"<color=green>PLAYER HEALED: {healed} HP (now {health}/{maxHealth})</color>");
            OnHealthChanged?.Invoke(health);

            UpdateUI();
        }

        public void TakeDamage(int amount)
        {
            int oldHealth = health;
            health = Mathf.Max(0, health - amount);
            int damageTaken = oldHealth - health;

            Debug.Log($"<color=red>PLAYER DAMAGE: {damageTaken} damage (now {health}/{maxHealth})</color>");
            OnHealthChanged?.Invoke(health);

            if (health <= 0)
            {
                Die();
            }

            UpdateUI();
        }

        public void SetMaxHealth(int newMax)
        {
            maxHealth = newMax;
            health = Mathf.Min(health, maxHealth);
            OnMaxHealthChanged?.Invoke(maxHealth);
            OnHealthChanged?.Invoke(health);

            UpdateUI();
        }

        private void Die()
        {
            Debug.Log("Player died!");
            // Add death logic here
        }

        // For debugging
        [ContextMenu("Take 20 Damage")]
        public void DebugTakeDamage()
        {
            TakeDamage(20);
        }

        [ContextMenu("Heal 30 HP")]
        public void DebugHeal()
        {
            Heal(30);
        }

        // Method to add stats (for equipment)
        public void AddStats(int str, int dex, int intelligence, int healthBonus)
        {
            strength += str;
            dexterity += dex;
            this.intelligence += intelligence;

            if (healthBonus != 0)
            {
                SetMaxHealth(maxHealth + healthBonus);
            }

            UpdateUI();
            Debug.Log($"Stats updated: Str={strength}, Dex={dexterity}, Int={this.intelligence}, MaxHP={maxHealth}");
        }
    }
}