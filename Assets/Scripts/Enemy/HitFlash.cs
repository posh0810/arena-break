using System.Collections;
using ArenaBreak.Core;
using UnityEngine;

namespace ArenaBreak.Enemy
{
    public class HitFlash : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private float _flashDuration = 0.05f;

        private Health _health;
        private Renderer _renderer;
        private MaterialPropertyBlock _block;
        private Color _originalColor;
        private Coroutine _flashCoroutine;

        private void Awake()
        {
            _health   = GetComponent<Health>();
            _renderer = GetComponent<Renderer>();
            _block    = new MaterialPropertyBlock();

            if (_renderer != null && _renderer.sharedMaterial != null)
                _originalColor = _renderer.sharedMaterial.GetColor(BaseColorId);
        }

        private void OnEnable()
        {
            if (_health != null)
                _health.HealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            if (_health != null)
                _health.HealthChanged -= OnHealthChanged;

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
                _flashCoroutine = null;
            }
            ApplyColor(_originalColor);
        }

        private void OnHealthChanged(float current, float max)
        {
            // 풀 복귀 후 Reinitialize(current==max) 또는 사망(current==0) 시 플래시 없음
            if (current >= max || current <= 0f) return;

            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashCoroutine());
        }

        private IEnumerator FlashCoroutine()
        {
            ApplyColor(Color.white);
            yield return new WaitForSeconds(_flashDuration);
            ApplyColor(_originalColor);
            _flashCoroutine = null;
        }

        private void ApplyColor(Color color)
        {
            if (_renderer == null) return;
            _renderer.GetPropertyBlock(_block);
            _block.SetColor(BaseColorId, color);
            _renderer.SetPropertyBlock(_block);
        }
    }
}
