using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState { Ready, Playing, Rest, Clear, GameOver }
    public GameState currentState = GameState.Ready;

    // 버티기 시간
    public float surviveTime = 20f;
    public float timer = 0f;

    //  웨이브 변수
    public int currentWave = 1;   // 현재 웨이브 번호
    public int maxWave = 4;       // 최대 웨이브 수
    public float waveTime = 120f; // 한 웨이브 시간

    // 정비 시간
    public float restTime = 30f;

    // UI
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI stateText;

    [SerializeField] private GameObject StatePanel;
    [SerializeField] private InteractSpawn interactSpawn;

    // 싱글톤
    public static GameManager Instance;

    // 적 스포너
    public EnemySpawner spawner;

    // 현재 적 리스트
    public List<GameObject> enemies = new List<GameObject>();

    // 조건 변수
    private GameObject curWaveTargetEnemy; // 현재 웨이브의 목표
    private bool isTargetSet = false; // 목표가 스폰되었는지 확인

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
        StartCoroutine(StagePanelCoroutine("Press Space to Start"));
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
                int remainTime = (int)(waveTime - timer);
                if (remainTime <= 0) remainTime = 0;
                timerText.text = remainTime / 60 + " : " + remainTime % 60;

                enemies.RemoveAll(item => item == null);

                // 몬스터가 스폰되었고 몬스터가죽었는지 확인
                if (isTargetSet && curWaveTargetEnemy == null)
                {
                    isTargetSet = false;
                    EpicMobClear(currentWave);
                }

                // 시간이 다 되었을 때
                if (timer >= waveTime)
                {
                    OnWaveTimeUp(currentWave); // 시간 초과 처리
                }
                break;

            // 정비 상태
            case GameState.Rest:
                timer += Time.deltaTime;
                int remainRestTime = (int)(restTime - timer);
                if (remainRestTime <= 0) remainTime = 0;
                timerText.text = remainRestTime / 60 + " : " + remainRestTime % 60;
                if (timer >= restTime)
                {
                    // 정비 시간이 끝나면 다음 웨이브 시작
                    currentWave++;
                    StartCombatWave();
                }
                break;

            case GameState.Clear:
            case GameState.GameOver:
                if (Input.GetKeyDown(KeyCode.R))
                    Restart();
                break;
        }
    }

    // 목표 Enemy 설정
    public void SetEnemy(GameObject enemy)
    {
        curWaveTargetEnemy = enemy;
        isTargetSet = true; // 목표 설정 기록
        Debug.Log($"{enemy.name}목표 등록");
    }

    // 에픽 몬스터 잡았을 때
    void EpicMobClear(int wave)
    {
        Debug.Log($"에픽 몬스터 처치 성공!");
        curWaveTargetEnemy = null;

        if (wave == 1)
        {
            StartRestWave(); // 1웨이브 즉시 종료
        }
        else if (wave == 2)
        {
            StartRestWave(); // 2웨이브 즉시 종료
        }
        else if (wave == 3)
        {
            StartRestWave(); // 3웨이브 즉시 종료
        }
        else if (wave == 4) // 보스(4웨이브)를 잡았을 때
        {
            ClearGame(); // 보스 잡으면 즉시 게임 클리어
            
        }
    }

    void OnWaveTimeUp(int wave)
    {
        // 웨이브 4 인데 시간이 다 되었다면
        if (wave == 4)
        {
            // 보스가 아직 살아있다면
            if (curWaveTargetEnemy != null)
            {
                Debug.Log("시간 초과 게임 오버");
                EndGame();
            }
        }
        else // 1~3 웨이브인데 시간이 다 되었다면
        {
            // 다음 정비 시간으로 이동
            StartRestWave();
        }
    }

    public void StartGame()
    {
        Debug.Log("게임 시작");
        timer = 0f;
        currentWave = 1;

        curWaveTargetEnemy = null;
        isTargetSet = false;

        StartCombatWave(); // 첫 전투 웨이브 시작
    }

    void StartCombatWave()
    {
        Debug.Log($"웨이브 {currentWave} 시작");
        interactSpawn.DisActiveAll();
        currentState = GameState.Playing;
        timer = 0f; // 전투 타이머 초기화
        curWaveTargetEnemy = null; // 새 웨이브 목표 초기화
        isTargetSet = false;

        stateText.text = "Survive!";
        stageText.text = $"Stage : {currentWave}";

        if (spawner != null)
            spawner.StartWave(currentWave); // 스포너에 웨이브 시작 알림
    }

    void ClearAllEnemies()
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] != null)
            {
                Destroy(enemies[i]);
            }
        }
        enemies.Clear();
    }

    // 정비 웨이브 시작 함수
    void StartRestWave()
    {
        // 중복 실행 방지
        if (currentState != GameState.Playing) return;

        Debug.Log("정비 웨이브 시작");
        currentState = GameState.Rest;
        timer = 0f; // 정비 타이머 초기화

        ClearAllEnemies();
        interactSpawn.CreateInteractObjects(3, 3, 3); // interactobject 생성

        StartCoroutine(StagePanelCoroutine("정비 시간"));

        if (spawner != null)
            spawner.StopSpawn(); // 정비 시간 동안 스폰 중지
    }

    public void ClearGame()
    {
        Debug.Log("게임 클리어");
        currentState = GameState.Clear;
        stateText.text = "Victory!\nPress R to Restart";

        ClearAllEnemies();

        Invoke("Debug.Log()", 3f);

        SceneManager.LoadScene("GameClearUI");

        if (spawner != null)
            spawner.StopSpawn();
    }

    public void EndGame()
    {
        Debug.Log("게임 오버");
        currentState = GameState.GameOver;
        ClearAllEnemies();

        if (spawner != null)
            spawner.StopSpawn();

        SceneManager.LoadScene("GameEndUI");
    }


    public void Restart()
    {
        Debug.Log("게임 재시작");
        ClearAllEnemies();
        timer = 0f;
        currentWave = 1;
        currentState = GameState.Ready;
        stateText.text = "Press Space to Start";
        stageText.text = "Stage : 1";
        isTargetSet = false;
    }

    public void EpicOutput(string comment)
    {
        StartCoroutine(StagePanelCoroutine(comment));
    }

    public IEnumerator StagePanelCoroutine(string comment)
    {
        StatePanel.SetActive(true);
        StatePanel.GetComponentInChildren<TextMeshProUGUI>().text = comment;
        yield return new WaitForSeconds(2f);
        StatePanel.SetActive(false);
    }
}