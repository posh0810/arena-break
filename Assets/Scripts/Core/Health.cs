using System;
using UnityEngine;

namespace ArenaBreak.Core
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _maxHealth = 100f;
        // 0이면 무적 없음. 플레이어에만 값을 주고 적은 기본값 그대로 쓴다
        [SerializeField] private float _invincibilityDuration;

        private float _currentHealth;
        private float _invincibleUntil;

        public event Action<float, float> HealthChanged;
        public event Action Died;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsDead => _currentHealth <= 0f;

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f || Time.time < _invincibleUntil)
            {
                return;
            }

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            _invincibleUntil = Time.time + _invincibilityDuration;

            HealthChanged?.Invoke(_currentHealth, _maxHealth);

            // 사망 후 처리(파괴, 게임오버)는 구독하는 쪽이 정한다
            if (IsDead)
            {
                Died?.Invoke();
            }
        }
    }
}
