using System.Collections;
using ArenaBreak.Core;
using UnityEngine;

namespace ArenaBreak.Player
{
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] private Health _playerHealth;
        [SerializeField] private float _shakeDuration  = 0.1f;
        [SerializeField] private float _shakeMagnitude = 0.15f;

        private Vector3 _originalLocalPos;
        private Coroutine _shakeCoroutine;

        private void Awake()
        {
            _originalLocalPos = transform.localPosition;
        }

        private void OnEnable()
        {
            if (_playerHealth != null)
                _playerHealth.HealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
                _playerHealth.HealthChanged -= OnHealthChanged;

            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
            }
            transform.localPosition = _originalLocalPos;
        }

        private void OnHealthChanged(float current, float max)
        {
            // 체력 감소 시에만 흔든다 (Reinitialize나 힐은 무시)
            if (current >= max || current < 0f) return;

            if (_shakeCoroutine != null)
                StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(ShakeCoroutine());
        }

        private IEnumerator ShakeCoroutine()
        {
            float elapsed = 0f;
            while (elapsed < _shakeDuration)
            {
                // 화면 평면(X/Y)에서만 흔든다. Z는 카메라 전방이라 흔들면 어색하다
                Vector2 offset = Random.insideUnitCircle * _shakeMagnitude;
                transform.localPosition = _originalLocalPos + new Vector3(offset.x, offset.y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = _originalLocalPos;
            _shakeCoroutine = null;
        }
    }
}
