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

        private readonly List<Transform> _spawnPoints = new List<Transform>();
        // 이벤트를 구독 해제하려면 구독할 때 쓴 델리게이트를 그대로 들고 있어야 한다
        private readonly Dictionary<Health, Action> _deathHandlers = new Dictionary<Health, Action>();

        private WaitUntil _waitUntilAllDead;
        private int _aliveCount;

        public event Action<int> WaveStarted;
        public event Action<int> WaveCleared;
        public event Action AllWavesCleared;

        public int TotalWaves => _waves.Count;

        private void Awake()
        {
            _waitUntilAllDead = new WaitUntil(() => _aliveCount == 0);

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
                SpawnEnemy(wave.EnemyPrefab);

                if (i < wave.SpawnCount - 1)
                {
                    yield return interval;
                }
            }
        }

        private void SpawnEnemy(GameObject prefab)
        {
            Transform point = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Count)];
            GameObject enemy = Instantiate(prefab, point.position, point.rotation);

            if (!enemy.TryGetComponent(out EnemyAI enemyAI) || !enemy.TryGetComponent(out Health health))
            {
                Debug.LogError($"{nameof(WaveSpawner)}: 적 프리팹 '{prefab.name}'에 EnemyAI 또는 Health가 없습니다.", this);
                Destroy(enemy);
                return;
            }

            enemyAI.SetTarget(_player);

            _aliveCount++;
            Action handler = () => OnEnemyDied(health);
            _deathHandlers[health] = handler;
            health.Died += handler;
        }

        private void OnEnemyDied(Health health)
        {
            if (_deathHandlers.TryGetValue(health, out Action handler))
            {
                health.Died -= handler;
                _deathHandlers.Remove(health);
            }

            _aliveCount--;
        }
    }
}
