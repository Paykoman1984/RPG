using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PoEClone2D.Combat; // Add this line

namespace PoEClone2D.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable // Now IDamageable is recognized
    {
        [Header("Health Settings")]
        [SerializeField] private float _currentHealth = 100f;
        [SerializeField] private float _maxHealth = 100f;

        [Header("UI References")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI healthText;

        // Public properties for other scripts to access
        public float currentHealth => _currentHealth;
        public float maxHealth => _maxHealth;

        // Events
        public event System.Action<float> OnHealthChanged;
        public event System.Action OnDeath;

        private void Start()
        {
            InitializeHealth();
        }

        private void InitializeHealth()
        {
            _currentHealth = _maxHealth;
            UpdateHealthUI();
            Debug.Log("Player Health initialized: " + _currentHealth + "/" + _maxHealth);
        }

        public void TakeDamage(float damage)
        {
            if (damage <= 0) return;

            _currentHealth -= damage;
            _currentHealth = Mathf.Max(0, _currentHealth);

            Debug.Log("Player took " + damage + " damage! Health: " + _currentHealth + "/" + _maxHealth);

            UpdateHealthUI();
            OnHealthChanged?.Invoke(_currentHealth);

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0) return;

            float oldHealth = _currentHealth;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            float healed = _currentHealth - oldHealth;

            Debug.Log($"<color=green>PlayerHealth: Healed {healed} HP (now {_currentHealth}/{_maxHealth})</color>");

            UpdateHealthUI();
            OnHealthChanged?.Invoke(_currentHealth);
        }

        public void SetMaxHealth(float newMax)
        {
            _maxHealth = newMax;
            if (_currentHealth > _maxHealth)
            {
                _currentHealth = _maxHealth;
            }
            UpdateHealthUI();
        }

        // Required by IDamageable interface
        public void ApplyKnockback(Vector2 force)
        {
            // Optional: Implement knockback logic here
            // For now, just log it or do nothing
            Debug.Log($"Knockback applied: {force}");
        }

        private void UpdateHealthUI()
        {
            // Update health slider
            if (healthSlider != null)
            {
                healthSlider.maxValue = _maxHealth;
                healthSlider.value = _currentHealth;
            }

            // Update health text
            if (healthText != null)
            {
                healthText.text = $"{_currentHealth:F0}/{_maxHealth:F0}";
            }
        }

        private void Die()
        {
            Debug.Log("Player died!");
            OnDeath?.Invoke();
            // Add death logic here
        }

        // For testing
        [ContextMenu("Take 20 Damage")]
        private void DebugTakeDamage()
        {
            TakeDamage(20);
        }

        [ContextMenu("Heal 30 HP")]
        private void DebugHeal()
        {
            Heal(30);
        }
    }
}
