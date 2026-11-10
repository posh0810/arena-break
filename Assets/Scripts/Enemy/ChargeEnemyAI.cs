using ArenaBreak.Core;
using UnityEngine;
using UnityEngine.AI;

namespace ArenaBreak.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Health))]
    public class ChargeEnemyAI : MonoBehaviour
    {
        private enum State { Idle, Chase, Charge, Cooldown, Dead }

        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Range")]
        [SerializeField] private float _chaseRange = 60f;
        [SerializeField] private float _chargeRange = 10f;
        [SerializeField] private float _attackRange = 2f;
        [SerializeField] private float _attackExitMargin = 0.5f;

        [Header("Movement")]
        [SerializeField] private float _normalSpeed = 2f;
        [SerializeField] private float _chargeSpeed = 12f;

        [Header("Charge")]
        [SerializeField] private float _chargeDuration = 2f;
        [SerializeField] private float _cooldownDuration = 5f;

        [Header("Attack")]
        [SerializeField] private float _attackInterval = 2f;
        [SerializeField] private float _attackDamage = 15f;

        private NavMeshAgent _agent;
        private Health _health;
        private IDamageable _targetDamageable;

        private State _state = State.Idle;
        private float _chargeEndTime;
        private float _cooldownEndTime;
        private float _nextAttackTime;
        private float _chaseRangeSq;
        private float _chargeRangeSq;
        private float _attackRangeSq;
        private float _attackExitRangeSq;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _health = GetComponent<Health>();
            _chaseRangeSq     = _chaseRange * _chaseRange;
            _chargeRangeSq    = _chargeRange * _chargeRange;
            _attackRangeSq    = _attackRange * _attackRange;
            _attackExitRangeSq = (_attackRange + _attackExitMargin) * (_attackRange + _attackExitMargin);
            CacheTargetDamageable();
        }

        private void OnEnable()
        {
            _health.Died += OnDied;
        }

        private void OnDisable()
        {
            _health.Died -= OnDied;
        }

        // 프리팹은 씬 오브젝트를 참조할 수 없어서 스폰한 쪽이 플레이어를 주입한다
        public void SetTarget(Transform target)
        {
            _target = target;
            CacheTargetDamageable();
        }

        // 풀에서 꺼낸 뒤 호출. 상태와 타겟을 초기화한다
        public void Reinitialize(Transform target)
        {
            _target = target;
            CacheTargetDamageable();
            _nextAttackTime = 0f;
            _chargeEndTime = 0f;
            _cooldownEndTime = 0f;
            SetState(State.Idle);
        }

        private void Update()
        {
            if (_state == State.Dead) return;

            if (_target == null)
            {
                SetState(State.Idle);
                return;
            }

            float sqDist = (transform.position - _target.position).sqrMagnitude;

            switch (_state)
            {
                case State.Idle:
                    if (sqDist <= _chaseRangeSq) SetState(State.Chase);
                    break;

                case State.Chase:
                    if (sqDist > _chaseRangeSq) { SetState(State.Idle); break; }
                    if (sqDist <= _chargeRangeSq) { SetState(State.Charge); break; }
                    if (_agent.isOnNavMesh) _agent.SetDestination(_target.position);
                    if (sqDist <= _attackRangeSq) TryAttack();
                    break;

                case State.Charge:
                    if (Time.time >= _chargeEndTime) { SetState(State.Cooldown); break; }
                    if (_agent.isOnNavMesh) _agent.SetDestination(_target.position);
                    if (sqDist <= _attackRangeSq) TryAttack();
                    break;

                case State.Cooldown:
                    if (Time.time >= _cooldownEndTime) SetState(State.Chase);
                    break;
            }
        }

        private void SetState(State next)
        {
            if (_state == next) return;
            _state = next;

            switch (next)
            {
                case State.Chase:
                    _agent.speed = _normalSpeed;
                    SetAgentStopped(false);
                    break;

                case State.Charge:
                    _agent.speed = _chargeSpeed;
                    _chargeEndTime = Time.time + _chargeDuration;
                    SetAgentStopped(false);
                    break;

                case State.Cooldown:
                    _cooldownEndTime = Time.time + _cooldownDuration;
                    SetAgentStopped(true);
                    break;

                case State.Idle:
                case State.Dead:
                    SetAgentStopped(true);
                    break;
            }
        }

        private void TryAttack()
        {
            if (Time.time < _nextAttackTime) return;
            _nextAttackTime = Time.time + _attackInterval;
            _targetDamageable?.TakeDamage(_attackDamage);
        }

        private void OnDied()
        {
            SetState(State.Dead);
            // 오브젝트 파괴는 WaveSpawner 풀이 처리한다
        }

        // Update에서 GetComponent를 부르지 않도록 대상이 정해질 때 한 번만 찾는다
        private void CacheTargetDamageable()
        {
            _targetDamageable = _target != null ? _target.GetComponentInParent<IDamageable>() : null;
        }

        private void SetAgentStopped(bool stopped)
        {
            if (_agent.isOnNavMesh) _agent.isStopped = stopped;
        }
    }
}
