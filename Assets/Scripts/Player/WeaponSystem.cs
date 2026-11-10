using System;
using ArenaBreak.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaBreak.Player
{
    public class WeaponSystem : MonoBehaviour
    {
        private const string PlayerMapName = "Player";
        private const string AttackActionName = "Attack";
        private const string ReloadActionName = "Reload";
        private const string ReloadBinding = "<Keyboard>/r";
        private const float ViewportCenter = 0.5f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [Header("Input")]
        [SerializeField] private InputActionAsset _inputActions;

        [Header("Aim")]
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _hitMask = ~0;

        [Header("Fire")]
        [SerializeField] private float _fireInterval = 0.15f;
        [SerializeField] private float _range = 50f;
        [SerializeField] private float _damage = 10f;

        [Header("Ammo")]
        [SerializeField] private int _magazineSize = 12;
        [SerializeField] private float _reloadDuration = 1.5f;

        [Header("Hit Particle")]
        [SerializeField] private ParticleSystem _hitParticlePrefab;

        [Header("Hit Marker (Debug)")]
        [SerializeField] private Color _hitMarkerColor = Color.yellow;
        [SerializeField] private float _hitMarkerSize = 0.15f;
        [SerializeField] private float _hitMarkerLifetime = 0.5f;

        private InputAction _attackAction;
        private InputAction _reloadAction;
        private MaterialPropertyBlock _markerBlock;

        private int _currentAmmo;
        private float _nextFireTime;
        private float _reloadEndTime;
        private bool _isReloading;

        public event Action<int, int> AmmoChanged;

        private void Awake()
        {
            _currentAmmo = _magazineSize;
            _markerBlock = new MaterialPropertyBlock();

            // 에셋에 R 액션이 없어서 코드에서 직접 만든다
            _reloadAction = new InputAction(ReloadActionName, InputActionType.Button, ReloadBinding);
        }

        private void OnEnable()
        {
            if (!TryBindActions())
            {
                enabled = false;
                return;
            }

            _attackAction.Enable();
            _reloadAction.Enable();
            _reloadAction.performed += OnReloadPerformed;
        }

        private void OnDisable()
        {
            if (_reloadAction != null)
            {
                _reloadAction.performed -= OnReloadPerformed;
                _reloadAction.Disable();
            }

            if (_attackAction != null)
            {
                _attackAction.Disable();
            }

            // 재장전 도중에 꺼지면 타이머가 멈춘 채로 남아 다시 켜도 발사가 막힌다
            _isReloading = false;
        }

        private void OnDestroy()
        {
            _reloadAction?.Dispose();
        }

        private void Update()
        {
            if (_isReloading)
            {
                if (Time.time >= _reloadEndTime)
                {
                    FinishReload();
                }
                return;
            }

            if (_attackAction.IsPressed() && _currentAmmo > 0 && Time.time >= _nextFireTime)
            {
                Fire();
            }
        }

        private bool TryBindActions()
        {
            if (_inputActions == null)
            {
                Debug.LogError($"{nameof(WeaponSystem)}: InputActionAsset이 할당되지 않았습니다.", this);
                return false;
            }

            if (_camera == null)
            {
                Debug.LogError($"{nameof(WeaponSystem)}: Camera가 할당되지 않았습니다.", this);
                return false;
            }

            InputActionMap playerMap = _inputActions.FindActionMap(PlayerMapName);
            if (playerMap == null)
            {
                Debug.LogError($"{nameof(WeaponSystem)}: '{PlayerMapName}' 액션 맵을 찾을 수 없습니다.", this);
                return false;
            }

            _attackAction = playerMap.FindAction(AttackActionName);
            if (_attackAction == null)
            {
                Debug.LogError($"{nameof(WeaponSystem)}: '{PlayerMapName}' 맵에 {AttackActionName} 액션이 없습니다.", this);
                return false;
            }

            return true;
        }

        private void Fire()
        {
            _currentAmmo--;
            _nextFireTime = Time.time + _fireInterval;

            Ray ray = _camera.ViewportPointToRay(new Vector3(ViewportCenter, ViewportCenter, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, _range, _hitMask, QueryTriggerInteraction.Ignore))
            {
                SpawnHitMarker(hit.point);
                SpawnHitParticle(hit.point, hit.normal);

                // 콜라이더가 자식에 붙은 적도 맞히도록 부모까지 탐색한다
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(_damage);
                }

            }

            AmmoChanged?.Invoke(_currentAmmo, _magazineSize);
        }

        private void OnReloadPerformed(InputAction.CallbackContext context)
        {
            if (_isReloading || _currentAmmo >= _magazineSize)
            {
                return;
            }

            _isReloading = true;
            _reloadEndTime = Time.time + _reloadDuration;
        }

        private void FinishReload()
        {
            _isReloading = false;
            _currentAmmo = _magazineSize;
            AmmoChanged?.Invoke(_currentAmmo, _magazineSize);
        }

        // 발사마다 생성/파괴되므로 풀링 대상
        private void SpawnHitParticle(Vector3 position, Vector3 normal)
        {
            if (_hitParticlePrefab == null) return;

            // 반구 shape가 로컬 Y 방향으로 방출되므로, Y를 법선 방향으로 세운다
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
            Instantiate(_hitParticlePrefab, position, rotation);
        }

        // 발사마다 생성/파괴되므로 풀링 대상
        private void SpawnHitMarker(Vector3 position)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "HitMarker";
            marker.transform.position = position;
            marker.transform.localScale = Vector3.one * _hitMarkerSize;

            // 콜라이더가 남아 있으면 다음 Raycast가 마커에 맞는다
            Destroy(marker.GetComponent<Collider>());

            var markerRenderer = marker.GetComponent<Renderer>();
            // material 접근은 인스턴스를 복제해 누수되므로 PropertyBlock으로 색만 덮어쓴다
            markerRenderer.GetPropertyBlock(_markerBlock);
            _markerBlock.SetColor(BaseColorId, _hitMarkerColor);
            markerRenderer.SetPropertyBlock(_markerBlock);

            Destroy(marker, _hitMarkerLifetime);
        }
    }
}
