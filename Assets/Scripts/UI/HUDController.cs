using ArenaBreak.Core;
using ArenaBreak.Player;
using UnityEngine;
using UnityEngine.UI;

namespace ArenaBreak.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Health Bar")]
        [SerializeField] private RectTransform _healthFill;

        [Header("Ammo")]
        [SerializeField] private Text _ammoText;

        [Header("Wave / Kill")]
        [SerializeField] private Text _waveText;
        [SerializeField] private Text _killText;

        [Header("End Screen")]
        [SerializeField] private GameObject _endPanel;
        [SerializeField] private Text _endMessageText;

        [Header("Scene References")]
        [SerializeField] private Health _playerHealth;
        [SerializeField] private WeaponSystem _weaponSystem;
        [SerializeField] private WaveSpawner _waveSpawner;
        [SerializeField] private GameManager _gameManager;

        private void Start()
        {
            if (_healthFill == null || _ammoText == null || _waveText == null ||
                _killText == null || _endPanel == null || _endMessageText == null ||
                _playerHealth == null || _weaponSystem == null ||
                _waveSpawner == null || _gameManager == null)
            {
                Debug.LogError($"{nameof(HUDController)}: 필드가 모두 할당돼야 합니다.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_playerHealth != null) _playerHealth.HealthChanged += OnHealthChanged;
            if (_weaponSystem  != null) _weaponSystem.AmmoChanged   += OnAmmoChanged;
            if (_waveSpawner   != null)
            {
                _waveSpawner.WaveStarted += OnWaveStarted;
                _waveSpawner.EnemyKilled += OnEnemyKilled;
            }
            if (_gameManager != null) _gameManager.StateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            if (_playerHealth != null) _playerHealth.HealthChanged -= OnHealthChanged;
            if (_weaponSystem  != null) _weaponSystem.AmmoChanged   -= OnAmmoChanged;
            if (_waveSpawner   != null)
            {
                _waveSpawner.WaveStarted -= OnWaveStarted;
                _waveSpawner.EnemyKilled -= OnEnemyKilled;
            }
            if (_gameManager != null) _gameManager.StateChanged -= OnStateChanged;
        }

        private void OnHealthChanged(float current, float max)
        {
            float ratio = max > 0f ? current / max : 0f;

            // anchorMax.x를 비율로 조정해 채움 너비를 줄인다
            // fillAmount를 쓰지 않는 이유: Image 컴포넌트가 Simple/Filled 모드를 강제하고
            // 나중에 9-slice 배경으로 교체할 때 타입을 바꿔야 한다
            Vector2 anchorMax = _healthFill.anchorMax;
            anchorMax.x = ratio;
            _healthFill.anchorMax = anchorMax;

            // 앵커 기반 크기를 쓸 때 sizeDelta.x가 남아 있으면 오프셋이 생긴다
            Vector2 delta = _healthFill.sizeDelta;
            delta.x = 0f;
            _healthFill.sizeDelta = delta;
        }

        private void OnAmmoChanged(int current, int max)
        {
            _ammoText.text = current + " / " + max;
        }

        private void OnWaveStarted(int waveNumber)
        {
            _waveText.text = "Wave " + waveNumber;
        }

        private void OnEnemyKilled(int killCount)
        {
            _killText.text = "Kill " + killCount;
        }

        private void OnStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.GameOver:
                    _endPanel.SetActive(true);
                    _endMessageText.text = "Game Over\nR키로 재시작";
                    break;
                case GameState.Cleared:
                    _endPanel.SetActive(true);
                    _endMessageText.text = "Cleared!\nR키로 재시작";
                    break;
                default:
                    _endPanel.SetActive(false);
                    break;
            }
        }
    }
}
