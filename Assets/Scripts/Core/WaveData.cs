using System.Collections.Generic;
using UnityEngine;

namespace ArenaBreak.Core
{
    [CreateAssetMenu(fileName = "WaveData", menuName = "ArenaBreak/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [SerializeField] private GameObject _enemyPrefab;
        // 여러 종류를 섞어 스폰할 때 채운다. 비어 있으면 _enemyPrefab 단독 사용
        [SerializeField] private List<GameObject> _enemyPrefabs = new List<GameObject>();
        [SerializeField] private int _spawnCount = 5;
        [SerializeField] private float _spawnInterval = 1f;

        public int SpawnCount => _spawnCount;
        public float SpawnInterval => _spawnInterval;

        // 스폰마다 호출. 리스트가 채워져 있으면 무작위 선택, 아니면 단일 프리팹
        public GameObject GetRandomPrefab()
        {
            if (_enemyPrefabs != null && _enemyPrefabs.Count > 0)
                return _enemyPrefabs[Random.Range(0, _enemyPrefabs.Count)];

            if (_enemyPrefab == null)
                Debug.LogError($"{nameof(WaveData)} '{name}': 프리팹이 할당되지 않았습니다.");

            return _enemyPrefab;
        }
    }
}
