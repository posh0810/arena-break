using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaBreak.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class DashAbility : MonoBehaviour
    {
        private const string PlayerMapName = "Player";
        private const string DashActionName = "Sprint";

        [Header("Input")]
        [SerializeField] private InputActionAsset _inputActions;

        [Header("Dash")]
        [SerializeField] private float _dashDistance = 6f;
        [SerializeField] private float _dashDuration = 0.2f;
        [SerializeField] private float _cooldown = 0.5f;

        private CharacterController _controller;
        private InputAction _dashAction;

        private Coroutine _dashRoutine;
        private bool _isDashing;
        private float _nextDashTime;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            if (!TryBindAction())
            {
                enabled = false;
                return;
            }

            _dashAction.Enable();
            _dashAction.performed += OnDashPerformed;
        }

        private void OnDisable()
        {
            if (_dashAction != null)
            {
                _dashAction.performed -= OnDashPerformed;
                _dashAction.Disable();
            }

            // 대시 도중에 꺼지면 코루틴이 멈춰 _isDashing이 true로 남아 다시 켜도 대시가 막힌다
            if (_dashRoutine != null)
            {
                StopCoroutine(_dashRoutine);
                _dashRoutine = null;
            }
            _isDashing = false;
        }

        private bool TryBindAction()
        {
            if (_inputActions == null)
            {
                Debug.LogError($"{nameof(DashAbility)}: InputActionAsset이 할당되지 않았습니다.", this);
                return false;
            }

            InputActionMap playerMap = _inputActions.FindActionMap(PlayerMapName);
            if (playerMap == null)
            {
                Debug.LogError($"{nameof(DashAbility)}: '{PlayerMapName}' 액션 맵을 찾을 수 없습니다.", this);
                return false;
            }

            _dashAction = playerMap.FindAction(DashActionName);
            if (_dashAction == null)
            {
                Debug.LogError($"{nameof(DashAbility)}: '{PlayerMapName}' 맵에 {DashActionName} 액션이 없습니다.", this);
                return false;
            }

            return true;
        }

        private void OnDashPerformed(InputAction.CallbackContext context)
        {
            if (_isDashing || Time.time < _nextDashTime)
            {
                return;
            }

            _dashRoutine = StartCoroutine(DashRoutine());
        }

        // 수직 이동은 건드리지 않는다: 점프 중이면 PlayerController의 중력이 그대로 착지시킨다
        private IEnumerator DashRoutine()
        {
            _isDashing = true;

            Vector3 direction = transform.forward;
            direction.y = 0f;
            direction.Normalize();

            float speed = _dashDistance / _dashDuration;
            float remaining = _dashDuration;

            while (remaining > 0f)
            {
                // 마지막 프레임이 초과 이동하지 않도록 남은 시간으로 자른다
                float step = Mathf.Min(Time.deltaTime, remaining);
                _controller.Move(direction * speed * step);
                remaining -= step;
                yield return null;
            }

            _isDashing = false;
            _dashRoutine = null;
            _nextDashTime = Time.time + _cooldown;
        }
    }
}
