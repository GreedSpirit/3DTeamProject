using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // ===========================
    // 게임 상테 관련
    // ===========================
    // 게임을 초기상태로 시작
    public void StartGame()
    {
        Debug.Log("StartGame called");
    }

    // 게임 오버 처리
    public void EndGame()
    {
        Debug.Log("EndGame called");
    }

    // ===========================
    // 스테이지 관리
    // ===========================
    // 현재 스테이지
    public int Stage()
    {
        return 1; // 예시 반환값
    }

    // 전체 스테이지
    public int MaxStage()
    {
        return 2; // 예시 반환값
    }

    // ===========================
    // UI 연결
    // ===========================
    // UI에 갱신할 정보
    public Text score;

    // ===========================
    // 싱글톤 패턴용
    // ===========================
    // 다른 스크립트에서 게임매니저 접근
    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ===========================
    // 오브젝트 관리
    // ===========================
    // 적 생성기
    public EnemySpawner spawner;

    // 활성화된 적 목록
    public List<GameObject> enemies;

    // ===========================
    // 플레이어 관련
    // ===========================
    // 플레이어 오브젝트
    public Player player;
}
