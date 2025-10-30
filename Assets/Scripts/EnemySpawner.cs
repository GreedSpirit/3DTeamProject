using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("적 프리팹 (웨이브별 종류 증가)")]
    public List<GameObject> enemyPrefabs = new List<GameObject>(); // 일반 적들
    [Header("에픽 몬스터 프리팹")]
    public GameObject epicEnemyPrefab;
    [Header("보스 몬스터 (4웨이브용)")]
    public GameObject bossPrefab;

    [Header("스폰 위치들")]
    public Transform[] spawnPoints;

    [Header("스폰 설정")]
    public float spawnInterval = 3f;
    public int maxEnemies = 10;

    private float timer = 0f;
    private bool isSpawning = false;
    private bool epicSpawned = false;

    void Update()
    {
        // 스포너가 꺼져 있거나, 게임 상태가 Playing이 아닐 때 리턴
        if (!isSpawning || GameManager.Instance.currentState != GameManager.GameState.Playing)
            return;

        timer += Time.deltaTime;

        // 일반 적 스폰
        if (timer >= spawnInterval)
        {
            SpawnEnemy(GameManager.Instance.currentWave);
            timer = 0f;
        }

        // 1분 30초 이후 에픽 몬스터 등장
        if (!epicSpawned && GameManager.Instance.currentState == GameManager.GameState.Playing)
        {
            if (GameManager.Instance != null && GameManager.Instance.currentWave < GameManager.Instance.maxWave)
            {
                if (GameManager.Instance.waveTime - GameManager.Instance.timer <= 30f) // 남은 시간이 30초 이하일 때 등장
                {
                    SpawnEpicEnemy();
                    epicSpawned = true;
                }
            }
        }
    }

    /// <summary>
    /// 일반 적 생성
    /// </summary>
    void SpawnEnemy(int wave)
    {
        if (GameManager.Instance.enemies.Count >= maxEnemies)
            return;

        // 스폰 위치 랜덤
        int index = Random.Range(0, spawnPoints.Length);
        Transform point = spawnPoints[index];

        // 현재 웨이브에 맞게 적 종류 제한
        int enemyTypeCount = Mathf.Clamp(wave, 1, enemyPrefabs.Count);
        int enemyTypeIndex = Random.Range(0, enemyTypeCount);

        // 보스 웨이브(4)면 보스 소환
        if (wave == GameManager.Instance.maxWave && bossPrefab != null)
        {
            SpawnBoss();
            return;
        }

        GameObject prefab = enemyPrefabs[enemyTypeIndex];
        GameObject enemy = Instantiate(prefab, point.position, point.rotation);
        GameManager.Instance.enemies.Add(enemy);
    }

    /// <summary>
    /// 에픽 몬스터 소환
    /// </summary>
    public void SpawnEpicEnemy()
    {
        if (epicEnemyPrefab == null) return;

        int index = Random.Range(0, spawnPoints.Length);
        Transform point = spawnPoints[index];

        GameObject epic = Instantiate(epicEnemyPrefab, point.position, point.rotation);
        GameManager.Instance.enemies.Add(epic);

        Debug.Log(" 에픽 몬스터 등장!");
    }

    /// <summary>
    /// 보스 몬스터 소환
    /// </summary>
    public void SpawnBoss()
    {
        if (bossPrefab == null) return;

        int index = Random.Range(0, spawnPoints.Length);
        Transform point = spawnPoints[index];

        GameObject boss = Instantiate(bossPrefab, point.position, point.rotation);
        GameManager.Instance.enemies.Add(boss);

        Debug.Log(" 보스 트롤 등장!");
    }

    /// <summary>
    /// 스폰 시작
    /// </summary>
    public void StartSpawn()
    {
        isSpawning = true;
        timer = 0f;
        epicSpawned = false;
    }

    /// <summary>
    /// 스폰 중지
    /// </summary>
    public void StopSpawn()
    {
        isSpawning = false;
    }

    /// <summary>
    /// 웨이브 시작 시 호출 (GameManager에서 사용)
    /// </summary>
    public void StartWave(int wave)
    {
        Debug.Log($"웨이브 {wave} 시작: 적 스폰 시작!");
        StartSpawn();
        epicSpawned = false;
    }
}