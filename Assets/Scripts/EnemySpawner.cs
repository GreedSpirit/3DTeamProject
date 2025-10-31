using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveData
{
    public string waveName; // 웨이브 이름
    public GameObject normalEnemyPrefab; // 일반 몬스터
    public GameObject epicEnemyPrefab;   // 에픽 몬스터 (1~3웨이브 전용)
}

public class EnemySpawner : MonoBehaviour
{
    [Header("웨이브 1, 2, 3 데이터")]
    public List<WaveData> waveList = new List<WaveData>();

    [Header("보스 몬스터 (4웨이브용)")]
    public GameObject bossPrefab;

    [Header("스폰 위치")]
    public Transform[] spawnPoints;

    [Header("스폰 설정")]
    public float spawnInterval = 3f;   // 기본 스폰 주기
    public int maxEnemies = 15;

    private float timer = 0f;
    private bool isSpawning = false;
    private bool bossSpawned = false;
    private bool epicSpawned = false;
    private int currentWaveForSpawner;

    void Update()
    {
        if (!isSpawning || GameManager.Instance.currentState != GameManager.GameState.Playing)
            return;

        timer += Time.deltaTime;

        // 마지막 웨이브는 스폰 속도 2배 빠르게
        float currentInterval = (currentWaveForSpawner == GameManager.Instance.maxWave)
            ? spawnInterval * 0.5f
            : spawnInterval;

        // 일반 몬스터 스폰
        if (timer >= currentInterval)
        {
            SpawnEnemy(currentWaveForSpawner);
            timer = 0f;
        }

        // 웨이브별 특수 스폰 처리
        HandleSpecialSpawns();
    }

    void HandleSpecialSpawns()
    {
        // 1~3웨이브: 30초 남았을 때 에픽 1회 등장
        if (currentWaveForSpawner < GameManager.Instance.maxWave && !epicSpawned)
        {
            if (GameManager.Instance.waveTime - GameManager.Instance.timer <= 30f)
            {
                SpawnEpicEnemy(currentWaveForSpawner);
                epicSpawned = true;
            }
        }

        // 4웨이브: 시작 시 보스 1회 등장
        if (currentWaveForSpawner == GameManager.Instance.maxWave && !bossSpawned)
        {
            SpawnBoss();
            bossSpawned = true;
        }
    }

    /// <summary>
    /// 일반 적 스폰
    /// </summary>
    void SpawnEnemy(int wave)
    {
        if (GameManager.Instance.enemies.Count >= maxEnemies)
            return;

        int index = Random.Range(0, spawnPoints.Length);
        Transform point = spawnPoints[index];
        GameObject prefab;

        // 마지막 웨이브 → 모든 웨이브의 일반 몬스터 중 랜덤
        if (wave == GameManager.Instance.maxWave)
        {
            int randomWaveIndex = Random.Range(0, waveList.Count);
            prefab = waveList[randomWaveIndex].normalEnemyPrefab;
        }
        else
        {
            prefab = waveList[Mathf.Min(wave - 1, waveList.Count - 1)].normalEnemyPrefab;
        }

        if (prefab == null) return;

        GameObject enemy = Instantiate(prefab, point.position, point.rotation);
        GameManager.Instance.enemies.Add(enemy);
    }

    /// <summary>
    /// 에픽 몬스터 스폰 (1~3웨이브용)
    /// </summary>
    public void SpawnEpicEnemy(int wave)
    {
        WaveData data = waveList[wave - 1];
        if (data.epicEnemyPrefab == null) return;

        int index = Random.Range(0, spawnPoints.Length);
        Transform point = spawnPoints[index];

        GameObject epic = Instantiate(data.epicEnemyPrefab, point.position, point.rotation);
        GameManager.Instance.enemies.Add(epic);

        GameManager.Instance.SetEnemy(epic);
        GameManager.Instance.EpicOutput("에픽 몬스터가 등장했습니다. 처치하고 보상을 획득하세요!");

        Debug.Log("에픽 몬스터 등장!");
    }

    /// <summary>
    /// 보스 몬스터 스폰 (4웨이브 전용)
    /// </summary>
    public void SpawnBoss()
    {
        if (bossPrefab == null) return;

        int index = Random.Range(0, spawnPoints.Length);
        Transform point = spawnPoints[index];

        GameObject boss = Instantiate(bossPrefab, point.position, point.rotation);
        GameManager.Instance.enemies.Add(boss);

        GameManager.Instance.SetEnemy(boss);
        GameManager.Instance.EpicOutput("보스 트롤이 등장했습니다. 행운을 빕니다!");

        Debug.Log("보스 트롤 등장!");
    }

    public void StartSpawn()
    {
        isSpawning = true;
        timer = 0f;
    }

    public void StopSpawn()
    {
        isSpawning = false;
    }

    public void StartWave(int wave)
    {
        Debug.Log($"웨이브 {wave} 시작: 적 스폰 시작!");
        currentWaveForSpawner = wave;
        StartSpawn();
        bossSpawned = false;
        epicSpawned = false;
    }
}
