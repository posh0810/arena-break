using System;
using System.Collections;
using System.Collections.Generic;
using ArenaBreak.Enemy;
using UnityEngine;

namespace ArenaBreak.Core
{
    public class WaveSpawner : MonoBehaviour
    {
        [Header("Waves")]
        [SerializeField] private List<WaveData> _waves = new List<WaveData>();
        [SerializeField] private float _nextWaveDelay = 3f;

        [Header("Scene References")]
        [SerializeField] private Transform _spawnPointsRoot;
        [SerializeField] private Transform _player;

        [Header("Pool")]
        [SerializeField] private float _returnToPoolDelay = 1f;

        private readonly List<Transform> _spawnPoints = new List<Transform>();
        // 이벤트를 구독 해제하려면 구독할 때 쓴 델리게이트를 그대로 들고 있어야 한다
        private readonly Dictionary<Health, Action> _deathHandlers = new Dictionary<Health, Action>();
        private readonly Dictionary<GameObject, Queue<GameObject>> _pool = new Dictionary<GameObject, Queue<GameObject>>();

        private Transform _poolRoot;
        private WaitUntil _waitUntilAllDead;
        private int _aliveCount;
        private int _killCount;

        public event Action<int> WaveStarted;
        public event Action<int> WaveCleared;
        public event Action AllWavesCleared;
        public event Action<int> EnemyKilled;

        public int TotalWaves => _waves.Count;

        private void Awake()
        {
            _waitUntilAllDead = new WaitUntil(() => _aliveCount == 0);

            _poolRoot = new GameObject("_EnemyPool").transform;
            _poolRoot.SetParent(transform);

            if (_spawnPointsRoot == null)
            {
                Debug.LogError($"{nameof(WaveSpawner)}: SpawnPoints 루트가 할당되지 않았습니다.", this);
                return;
            }

            foreach (Transform child in _spawnPointsRoot)
            {
                _spawnPoints.Add(child);
            }
        }

        private void Start()
        {
            if (_spawnPoints.Count == 0)
            {
                Debug.LogError($"{nameof(WaveSpawner)}: 스폰 위치가 없습니다. SpawnPoints 아래에 자식 오브젝트가 필요합니다.", this);
                return;
            }

            if (_player == null)
            {
                Debug.LogError($"{nameof(WaveSpawner)}: Player가 할당되지 않았습니다.", this);
                return;
            }

            StartCoroutine(RunWaves());
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<Health, Action> pair in _deathHandlers)
            {
                // 적이 이미 파괴된 뒤라면 구독 해제할 대상이 없다
                if (pair.Key != null)
                {
                    pair.Key.Died -= pair.Value;
                }
            }

            _deathHandlers.Clear();
        }

        private IEnumerator RunWaves()
        {
            for (int i = 0; i < _waves.Count; i++)
            {
                int waveNumber = i + 1;
                WaveStarted?.Invoke(waveNumber);

                yield return SpawnWave(_waves[i]);

                // 스폰이 끝난 뒤에 기다려야 한다. 스폰 도중 잠깐 0이 되는 것을 클리어로 오인하지 않도록
                yield return _waitUntilAllDead;

                WaveCleared?.Invoke(waveNumber);

                if (i < _waves.Count - 1)
                {
                    yield return new WaitForSeconds(_nextWaveDelay);
                }
            }

            AllWavesCleared?.Invoke();
        }

        private IEnumerator SpawnWave(WaveData wave)
        {
            var interval = new WaitForSeconds(wave.SpawnInterval);

            for (int i = 0; i < wave.SpawnCount; i++)
            {
                SpawnEnemy(wave.GetRandomPrefab());

                if (i < wave.SpawnCount - 1)
                {
                    yield return interval;
                }
            }
        }

        private void SpawnEnemy(GameObject prefab)
        {
            Transform point = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Count)];

            GameObject enemy = GetFromPool(prefab);
            enemy.transform.SetPositionAndRotation(point.position, point.rotation);
            enemy.SetActive(true);

            if (!enemy.TryGetComponent(out Health health))
            {
                Debug.LogError($"{nameof(WaveSpawner)}: 적 프리팹 '{prefab.name}'에 Health가 없습니다.", this);
                ReturnToPool(prefab, enemy);
                return;
            }

            if (!TryReinitializeController(enemy, _player))
            {
                Debug.LogError($"{nameof(WaveSpawner)}: 적 프리팹 '{prefab.name}'에 EnemyAI 또는 ChargeEnemyAI가 없습니다.", this);
                ReturnToPool(prefab, enemy);
                return;
            }

            health.Reinitialize();

            _aliveCount++;
            Action handler = null;
            handler = () => OnEnemyDied(prefab, enemy, health, handler);
            _deathHandlers[health] = handler;
            health.Died += handler;
        }

        private bool TryReinitializeController(GameObject enemy, Transform player)
        {
            if (enemy.TryGetComponent(out EnemyAI enemyAI))
            {
                enemyAI.Reinitialize(player);
                return true;
            }

            if (enemy.TryGetComponent(out ArenaBreak.Enemy.ChargeEnemyAI chargeAI))
            {
                chargeAI.Reinitialize(player);
                return true;
            }

            return false;
        }

        private GameObject GetFromPool(GameObject prefab)
        {
            if (_pool.TryGetValue(prefab, out Queue<GameObject> queue) && queue.Count > 0)
            {
                return queue.Dequeue();
            }

            GameObject instance = Instantiate(prefab, _poolRoot);
            instance.SetActive(false);
            return instance;
        }

        private void ReturnToPool(GameObject prefab, GameObject enemy)
        {
            enemy.SetActive(false);
            enemy.transform.SetParent(_poolRoot);

            if (!_pool.TryGetValue(prefab, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>();
                _pool[prefab] = queue;
            }

            queue.Enqueue(enemy);
        }

        private void OnEnemyDied(GameObject prefab, GameObject enemy, Health health, Action handler)
        {
            if (_deathHandlers.ContainsKey(health))
            {
                health.Died -= handler;
                _deathHandlers.Remove(health);
            }

            _aliveCount--;
            _killCount++;
            EnemyKilled?.Invoke(_killCount);
            StartCoroutine(ReturnToPoolDelayed(prefab, enemy));
        }

        private IEnumerator ReturnToPoolDelayed(GameObject prefab, GameObject enemy)
        {
            yield return new WaitForSeconds(_returnToPoolDelay);
            ReturnToPool(prefab, enemy);
        }
    }
}
