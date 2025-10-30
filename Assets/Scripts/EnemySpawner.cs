using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float spawnInterval = 3f;
    public int maxEnemies = 10;

    private float timer = 0f;
    private bool isSpawning = false;

    void Update()
    {
        if (!isSpawning || GameManager.Instance.currentState != GameManager.GameState.Playing)
            return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        if (GameManager.Instance.enemies.Count >= maxEnemies)
            return;

        int index = Random.Range(0, spawnPoints.Length);
        Transform point = spawnPoints[index];

        GameObject enemy = Instantiate(enemyPrefab, point.position, point.rotation);
        GameManager.Instance.enemies.Add(enemy);
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
    public void SpawnEnemies(int stage)
    {
        // 스테이지에 따라 생성할 적 수
        int spawnCount = 5 + stage * 2;

        for (int i = 0; i < spawnCount; i++)
        {
            int index = Random.Range(0, spawnPoints.Length);
            Transform point = spawnPoints[index];

            GameObject enemy = Instantiate(enemyPrefab, point.position, point.rotation);
            GameManager.Instance.enemies.Add(enemy);
        }
    }
}