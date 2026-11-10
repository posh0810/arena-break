using System;
using System.Collections;
using ArenaBreak.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ArenaBreak.Core
{
    public enum GameState { Ready, Playing, GameOver, Cleared }

    public class GameManager : MonoBehaviour
    {
        [SerializeField] private float _readyDuration = 3f;

        [Header("Scene References")]
        [SerializeField] private Health _playerHealth;
        [SerializeField] private WaveSpawner _waveSpawner;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private WeaponSystem _weaponSystem;

        private InputAction _restartAction;
        private InputAction _quitAction;

        private GameState _state;

        // HUDController 등이 구독한다
        public event Action<GameState> StateChanged;

        private void Awake()
        {
            // InputSystem_Actions 에셋에 없는 액션을 코드에서 직접 만든다
            _restartAction = new InputAction("Restart", InputActionType.Button, "<Keyboard>/r");
            _quitAction    = new InputAction("Quit",    InputActionType.Button, "<Keyboard>/escape");
        }

        private void OnEnable()
        {
            _restartAction.performed += OnRestartPerformed;
            _quitAction.performed   += OnQuitPerformed;
            _restartAction.Enable();
            _quitAction.Enable();
        }

        private void OnDisable()
        {
            _restartAction.performed -= OnRestartPerformed;
            _quitAction.performed   -= OnQuitPerformed;
            _restartAction.Disable();
            _quitAction.Disable();

            if (_playerHealth != null)  _playerHealth.Died         -= OnPlayerDied;
            if (_waveSpawner  != null)  _waveSpawner.AllWavesCleared -= OnAllWavesCleared;
        }

        private void OnDestroy()
        {
            _restartAction?.Dispose();
            _quitAction?.Dispose();
        }

        private void Start()
        {
            if (_playerHealth == null || _waveSpawner == null ||
                _playerController == null || _weaponSystem == null)
            {
                Debug.LogError($"{nameof(GameManager)}: 필드가 모두 할당돼야 합니다.", this);
                return;
            }

            _playerHealth.Died          += OnPlayerDied;
            _waveSpawner.AllWavesCleared += OnAllWavesCleared;

            ChangeState(GameState.Ready);
            StartCoroutine(ReadyToPlaying());
        }

        private IEnumerator ReadyToPlaying()
        {
            yield return new WaitForSeconds(_readyDuration);
            ChangeState(GameState.Playing);
        }

        private void ChangeState(GameState next)
        {
            _state = next;
            StateChanged?.Invoke(_state);

            switch (_state)
            {
                case GameState.GameOver:
                case GameState.Cleared:
                    _playerController.enabled = false;
                    _weaponSystem.enabled     = false;
                    // PlayerController.OnDisable이 커서를 복원한 뒤 덮어쓴다
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible   = true;
                    Time.timeScale   = 0f;
                    break;
            }
        }

        private void OnPlayerDied()
        {
            if (_state != GameState.Playing) return;
            ChangeState(GameState.GameOver);
        }

        private void OnAllWavesCleared()
        {
            if (_state != GameState.Playing) return;
            ChangeState(GameState.Cleared);
        }

        private void OnRestartPerformed(InputAction.CallbackContext ctx)
        {
            if (_state != GameState.GameOver && _state != GameState.Cleared) return;
            _playerController.enabled = true;
            _weaponSystem.enabled     = true;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnQuitPerformed(InputAction.CallbackContext ctx)
        {
            Application.Quit();
        }
    }
}
