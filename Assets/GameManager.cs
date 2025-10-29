using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum GameState { Ready, Playing, Clear, GameOver }
    public GameState currentState = GameState.Ready;

    // 버티기 시간 (초)
    public float surviveTime = 20f;
    private float timer = 0f;

    // UI
    public Text score;
    public Text stageText;
    public Text stateText;

    // 싱글톤
    public static GameManager Instance;

    // 적 스포너
    public EnemySpawner spawner;

    // 현재 적 리스트
    public List<GameObject> enemies = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentState = GameState.Ready;
        //stateText.text = "Press Space to Start";
        //stageText.text = "Stage : 1";
    }

    void Update()
    {
        switch (currentState)
        {
            case GameState.Ready:
                if (Input.GetKeyDown(KeyCode.Space))
                    StartGame();
                break;

            case GameState.Playing:
                timer += Time.deltaTime;
                if (timer >= surviveTime)
                    ClearStage();
                break;

            case GameState.Clear:
            case GameState.GameOver:
                if (Input.GetKeyDown(KeyCode.R))
                    Restart();
                break;
        }
    }

    public void StartGame()
    {
        Debug.Log("게임 시작");
        currentState = GameState.Playing;
        timer = 0f;
        stateText.text = "Survive!";
        stageText.text = "Stage : 1";

        if (spawner != null)
            spawner.StartSpawn();
    }

    public void EndGame()
    {
        Debug.Log("게임 오버");
        currentState = GameState.GameOver;
        stateText.text = "Game Over\nPress R to Restart";

        if (spawner != null)
            spawner.StopSpawn();
    }

    public void ClearStage()
    {
        Debug.Log("스테이지 클리어!");
        currentState = GameState.Clear;
        stateText.text = "Stage Clear!\nPress R to Restart";

        if (spawner != null)
            spawner.StopSpawn();
    }

    public void Restart()
    {
        Debug.Log("게임 재시작");
        enemies.Clear();
        timer = 0f;
        currentState = GameState.Ready;
        stateText.text = "Press Space to Start";
        stageText.text = "Stage : 1";
    }
    
}