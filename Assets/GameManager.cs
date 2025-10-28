using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // 현재 게임 상태를 나타내는 enum
    public enum GameState { Ready, Playing, StageClear, GameOver }

    // 현재 상태
    public GameState currentState = GameState.Ready;

    // 현재 스테이지 번호
    public int currentStage = 1;

    // 전체 스테이지 수
    public int maxStage = 2;

    // UI에 표시할 점수 텍스트
    public Text score;

    // 현재 스테이지 표시용 텍스트
    public Text stageText;

    // 게임 상태 표시용 텍스트
    public Text stateText;

    // 싱글톤 인스턴스
    public static GameManager Instance;

    // 적 생성 스크립트
    //public EnemySpawner spawner;

    // 현재 활성화된 적 리스트
    public List<GameObject> enemies = new List<GameObject>();

    // 플레이어 오브젝트
    //public Player player;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동 시 파괴되지 않음
        }
        else
        {
            Destroy(gameObject); // 이미 있으면 중복 제거
        }
    }

    void Start()
    {
        currentState = GameState.Ready;
        stateText.text = "Press Space to Start";
        stageText.text = "Stage : " + currentStage;
    }

    void Update()
    {
        switch (currentState)
        {
            // 게임 시작 대기 상태
            case GameState.Ready:
                if (Input.GetKeyDown(KeyCode.Space))
                    StartGame();
                break;

            // 실제 플레이 중
            case GameState.Playing:
                if (enemies.Count == 0)
                    StageClear();
                break;

            // 스테이지 클리어 상태
            case GameState.StageClear:
                if (Input.GetKeyDown(KeyCode.Space))
                    NextStage();
                break;

            // 게임 오버 상태
            case GameState.GameOver:
                if (Input.GetKeyDown(KeyCode.R))
                    Restart();
                break;
        }
    }

    // 게임을 시작할 때 호출
    public void StartGame()
    {
        Debug.Log("StartGame called");
        currentStage = 1;
        currentState = GameState.Playing;
        stateText.text = "Stage " + currentStage;
        stageText.text = "Stage : " + currentStage;

        //if (spawner != null)
            //spawner.SpawnEnemies(currentStage);
    }

    // 게임을 종료할 때 호출
    public void EndGame()
    {
        Debug.Log("EndGame called");
        currentState = GameState.GameOver;
        stateText.text = "Game Over\nPress R to Restart";
    }

    // 스테이지 클리어 시 호출
    public void StageClear()
    {
        Debug.Log("StageClear called");
        currentState = GameState.StageClear;
        stateText.text = "Stage Clear!\nPress Space for Next Stage";
    }

    // 다음 스테이지로 이동
    public void NextStage()
    {
        currentStage++;
        if (currentStage > maxStage)
        {
            EndGame();
            return;
        }

        Debug.Log("NextStage called");
        currentState = GameState.Playing;
        stageText.text = "Stage : " + currentStage;
        stateText.text = "Stage " + currentStage;

        enemies.Clear();
        //if (spawner != null)
            //spawner.SpawnEnemies(currentStage);
    }

    // 게임 재시작
    public void Restart()
    {
        Debug.Log("Restart called");
        currentStage = 1;
        enemies.Clear();
        currentState = GameState.Ready;
        stateText.text = "Press Space to Start";
        stageText.text = "Stage : " + currentStage;
    }

    // 현재 스테이지 번호 반환
    public int Stage()
    {
        return currentStage;
    }

    // 전체 스테이지 수 반환
    public int MaxStage()
    {
        return maxStage;
    }
}