using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaBreak.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private const string PlayerMapName = "Player";
        private const string MoveActionName = "Move";
        private const string LookActionName = "Look";
        private const string JumpActionName = "Jump";

        [Header("Input")]
        [SerializeField] private InputActionAsset _inputActions;

        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _jumpHeight = 1.5f;
        [SerializeField] private float _gravity = -20f;
        // 접지 중에도 살짝 눌러줘야 CharacterController.isGrounded가 프레임마다 흔들리지 않는다
        [SerializeField] private float _groundedStickVelocity = -2f;

        [Header("Look")]
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private float _lookSensitivity = 0.1f;
        [SerializeField] private float _pitchLimit = 80f;
        // 커서가 잠기는 첫 프레임에 delta가 크게 튀는 현상을 막는다
        [SerializeField] private float _maxLookDeltaPerFrame = 50f;

        [Header("Cursor")]
        [SerializeField] private bool _lockCursor = true;

        private CharacterController _controller;
        private InputActionMap _playerMap;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;

        private float _pitch;
        private float _verticalVelocity;

        private bool _cursorCaptured;
        private CursorLockMode _previousLockMode;
        private bool _previousCursorVisible;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            if (!TryBindActions())
            {
                enabled = false;
                return;
            }

            _playerMap.Enable();

            if (_lockCursor)
            {
                _previousLockMode = Cursor.lockState;
                _previousCursorVisible = Cursor.visible;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                _cursorCaptured = true;
            }
        }

        private void OnDisable()
        {
            if (_playerMap != null)
            {
                _playerMap.Disable();
            }

            if (_cursorCaptured)
            {
                Cursor.lockState = _previousLockMode;
                Cursor.visible = _previousCursorVisible;
                _cursorCaptured = false;
            }
        }

        private void Update()
        {
            UpdateLook();
            UpdateMovement();
        }

        private bool TryBindActions()
        {
            if (_inputActions == null)
            {
                Debug.LogError($"{nameof(PlayerController)}: InputActionAsset이 할당되지 않았습니다.", this);
                return false;
            }

            _playerMap = _inputActions.FindActionMap(PlayerMapName);
            if (_playerMap == null)
            {
                Debug.LogError($"{nameof(PlayerController)}: '{PlayerMapName}' 액션 맵을 찾을 수 없습니다.", this);
                return false;
            }

            _moveAction = _playerMap.FindAction(MoveActionName);
            _lookAction = _playerMap.FindAction(LookActionName);
            _jumpAction = _playerMap.FindAction(JumpActionName);

            if (_moveAction == null || _lookAction == null || _jumpAction == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerController)}: '{PlayerMapName}' 맵에 {MoveActionName}/{LookActionName}/{JumpActionName} 액션이 모두 있어야 합니다.",
                    this);
                return false;
            }

            return true;
        }

        private void UpdateLook()
        {
            Vector2 look = Vector2.ClampMagnitude(_lookAction.ReadValue<Vector2>(), _maxLookDeltaPerFrame);

            transform.Rotate(Vector3.up, look.x * _lookSensitivity, Space.Self);

            _pitch = Mathf.Clamp(_pitch - look.y * _lookSensitivity, -_pitchLimit, _pitchLimit);
            _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void UpdateMovement()
        {
            Vector2 input = _moveAction.ReadValue<Vector2>();
            Vector3 horizontal = (transform.right * input.x + transform.forward * input.y);
            horizontal = Vector3.ClampMagnitude(horizontal, 1f) * _moveSpeed;

            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = _groundedStickVelocity;
            }

            if (_controller.isGrounded && _jumpAction.WasPressedThisFrame())
            {
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * 2f * Mathf.Abs(_gravity));
            }

            _verticalVelocity += _gravity * Time.deltaTime;

            Vector3 velocity = horizontal + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }
    }
}
