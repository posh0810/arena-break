using ArenaBreak.Core;
using UnityEngine;
using UnityEngine.AI;

namespace ArenaBreak.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Health))]
    public class EnemyAI : MonoBehaviour
    {
        private enum EnemyState
        {
            Idle,
            Chase,
            Attack,
            Dead
        }

        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Range")]
        [SerializeField] private float _chaseRange = 60f;
        [SerializeField] private float _attackRange = 2f;
        // 공격 사거리 경계에서 Attack과 Chase가 프레임마다 번갈아 바뀌는 것을 막는다
        [SerializeField] private float _attackExitMargin = 0.5f;

        [Header("Attack")]
        [SerializeField] private float _attackInterval = 2f;
        [SerializeField] private float _attackDamage = 10f;

        private NavMeshAgent _agent;
        private Health _health;
        private IDamageable _targetDamageable;

        private EnemyState _state = EnemyState.Idle;
        private float _nextAttackTime;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _health = GetComponent<Health>();
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
            SetState(EnemyState.Idle);
        }

        private void Update()
        {
            if (_state == EnemyState.Dead)
            {
                return;
            }

            if (_target == null)
            {
                SetState(EnemyState.Idle);
                return;
            }

            float distance = Vector3.Distance(transform.position, _target.position);

            switch (_state)
            {
                case EnemyState.Idle:
                    if (distance <= _chaseRange)
                    {
                        SetState(EnemyState.Chase);
                    }
                    break;

                case EnemyState.Chase:
                    if (distance > _chaseRange)
                    {
                        SetState(EnemyState.Idle);
                    }
                    else if (distance <= _attackRange)
                    {
                        SetState(EnemyState.Attack);
                    }
                    else if (_agent.isOnNavMesh)
                    {
                        _agent.SetDestination(_target.position);
                    }
                    break;

                case EnemyState.Attack:
                    if (distance > _attackRange + _attackExitMargin)
                    {
                        SetState(EnemyState.Chase);
                    }
                    else if (Time.time >= _nextAttackTime)
                    {
                        PerformAttack();
                    }
                    break;
            }
        }

        private void SetState(EnemyState next)
        {
            if (_state == next)
            {
                return;
            }

            _state = next;

            switch (next)
            {
                case EnemyState.Chase:
                    SetAgentStopped(false);
                    break;

                case EnemyState.Idle:
                case EnemyState.Attack:
                case EnemyState.Dead:
                    SetAgentStopped(true);
                    break;
            }
        }

        // 스폰 직후 NavMesh에 올라가기 전에는 isStopped 접근이 에러를 낸다
        private void SetAgentStopped(bool stopped)
        {
            if (_agent.isOnNavMesh)
            {
                _agent.isStopped = stopped;
            }
        }

        private void PerformAttack()
        {
            _nextAttackTime = Time.time + _attackInterval;

            _targetDamageable?.TakeDamage(_attackDamage);
            Debug.Log($"{name} 공격 - 데미지 {_attackDamage}");
        }

        private void OnDied()
        {
            SetState(EnemyState.Dead);
            // 오브젝트 파괴는 WaveSpawner 풀이 처리한다
        }

        // Update에서 GetComponent를 부르지 않도록 대상이 정해질 때 한 번만 찾는다
        private void CacheTargetDamageable()
        {
            _targetDamageable = _target != null ? _target.GetComponentInParent<IDamageable>() : null;
        }
    }
}
