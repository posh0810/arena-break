using UnityEngine;

namespace ArenaBreak.Core
{
    [CreateAssetMenu(fileName = "WaveData", menuName = "ArenaBreak/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private int _spawnCount = 5;
        [SerializeField] private float _spawnInterval = 1f;

        public GameObject EnemyPrefab => _enemyPrefab;
        public int SpawnCount => _spawnCount;
        public float SpawnInterval => _spawnInterval;
    }
}
