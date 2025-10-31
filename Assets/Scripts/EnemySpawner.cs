using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveData
{
    public string waveName; // 웨이브 이름
    public GameObject normalEnemyPrefab; // 해당 웨이브의 일반 몬스터
    public GameObject epicEnemyPrefab;    // 해당 웨이브의 에픽 몬스터
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
    public float spawnInterval = 3f;
    public int maxEnemies = 10;

    private float timer = 0f;
    private bool isSpawning = false;
    private bool epicSpawned = false;
    private int currentWaveForSpawner; // 스포너가 현재 웨이브를 기억

    void Update()
    {
        // 스포너가 꺼져 있거나, 게임 상태가 Playing이 아닐 때 리턴
        if (!isSpawning || GameManager.Instance.currentState != GameManager.GameState.Playing)
            return;

        timer += Time.deltaTime;

        // 모든 웨이브에서 일반몹 스폰 
        if (timer >= spawnInterval)
        {
            SpawnEnemy(currentWaveForSpawner);
            timer = 0f;
        }

        // 1~3 웨이브는 30초 남았을 때 에픽 등장
        // 4웨이브는 시작 시 보스 한 번만 등장
        if (!epicSpawned && GameManager.Instance.currentState == GameManager.GameState.Playing)
        {
            if (currentWaveForSpawner == GameManager.Instance.maxWave)
            {
                SpawnBoss();
                epicSpawned = true;
            }
            else if (GameManager.Instance.waveTime - GameManager.Instance.timer <= 30f)
            {
                SpawnEpicEnemy(currentWaveForSpawner);
                epicSpawned = true;
            }
        }
    }

    void SpawnEnemy(int wave)
    {
        if (GameManager.Instance.enemies.Count >= maxEnemies)
            return;

        int index = Random.Range(0, spawnPoints.Length);
        Transform point = spawnPoints[index];

        // 4웨이브에서도 3웨이브 데이터 사용
        WaveData data = waveList[Mathf.Min(wave - 1, waveList.Count - 1)];

        GameObject prefab = data.normalEnemyPrefab;
        GameObject enemy = Instantiate(prefab, point.position, point.rotation);
        GameManager.Instance.enemies.Add(enemy);
    }

    public void SpawnEpicEnemy(int wave)
    {
        WaveData data = waveList[wave - 1];

        int index = Random.Range(0, spawnPoints.Length);
        Transform point = spawnPoints[index];

        GameObject epic = Instantiate(data.epicEnemyPrefab, point.position, point.rotation);
        GameManager.Instance.enemies.Add(epic);

        GameManager.Instance.SetEnemy(epic);
        GameManager.Instance.EpicOutput("에픽 몬스터가 등장했습니다. 처치하고 보상을 획득하세요!");

        Debug.Log("에픽 몬스터 등장!");
    }

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
        epicSpawned = false;
    }
}