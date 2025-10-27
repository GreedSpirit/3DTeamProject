using UnityEngine;

// 몬스터의 현재 상태를 정의하는 ENUM 
public enum EnemyState
{
    Idle,        // 대기 또는 순찰 상태 (현재 코드는 순찰 로직 미구현)
    //Patrol,      // 순찰 상태
    Chase,       // 플레이어 추적 상태
    Attack,      // 공격 상태
    Dead         // 사망 상태
}
public class Enemy : MonoBehaviour
{
    [Header("Component")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Collider mainCollider;
    //스텟 관련
    [Header("Stats")]
    [SerializeField] private float _maxHealth = 100f;
    public float MaxHealth => _maxHealth; // 읽기 전용 속성
    public float CurrentHealth { get; private set; } // 외부는 읽기, 내부는 쓰기 가능

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;  // 회전 속도
    [SerializeField] private float chaseRange = 10f;    //어그로범위
    [SerializeField] private float attackRange = 2f;    //사정거리
    [SerializeField] private float retreatFactor = 1.5f;  // 추적 포기 거리 배율 (1.5배)
    [SerializeField] private float attackDuration = 1.5f; // 공격 애니메이션 지속 시간 (Invoke용)
    // [SerializeField] private int experienceValue = 50; // 경험치 보상(구현시)

    //상태 변수
    private EnemyState _currentState = EnemyState.Idle; // 몬스터의 현재 AI 상태
    private Transform _targetPlayer; // 플레이어 Transform
    private bool _isPerformingAttack = false; // 공격 중인지 체크

    void Awake()
    {
        // 몬스터 시작 시 현재 체력을 최대 체력으로 초기화
        CurrentHealth = MaxHealth;
        if (rb == null) rb = GetComponent<Rigidbody>(); // 안정성을 위한 조건부 설정

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player"); //나중에 player 담당자와 태그/이름 맞춰야함
        if (playerObj != null) _targetPlayer = playerObj.transform; 
    }
    void FixedUpdate()
    {
        if (_currentState == EnemyState.Dead || _targetPlayer == null) return;

        UpdateAI();
    }
    public void TakeDamage(float amount, Vector3 hitPoint)
    {
        if (_currentState == EnemyState.Dead) return;

        //(대미지 계산, 체력 감소 로직)
        CurrentHealth -= amount;
        OnHit(hitPoint);

        if (CurrentHealth <= 0)
        {
            Die();
        }
        else if (_currentState == EnemyState.Idle) // 대기 상태에서 공격받을 시 추적 ( 어그로 )
        {
            SetState(EnemyState.Chase);
        }
    }

    private void UpdateAI()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, _targetPlayer.position);

        switch (_currentState)
        {
            case EnemyState.Idle:
                // 어그로 범위 내에 들어올 시 추적 상태
                if (distanceToPlayer <= chaseRange)
                {
                    SetState(EnemyState.Chase);
                }
                break;

            case EnemyState.Chase:
                RotateToTarget();
                //플레이어 추적 포기 거리.포기하면 대기 상태로 복귀
                if (distanceToPlayer > chaseRange * retreatFactor)
                {
                    SetState(EnemyState.Idle);
                }
                else if (distanceToPlayer <= attackRange) //공격 전환
                {
                    SetState(EnemyState.Attack);
                }
                else
                {
                    MoveToTarget(_targetPlayer.position);
                }
                break;

            case EnemyState.Attack:
                RotateToTarget();
                //공격 범위 벗어났을 때 다시 추적
                if (distanceToPlayer > attackRange)
                {
                    SetState(EnemyState.Chase);
                }
                else if (!_isPerformingAttack)
                {
                    rb.velocity = Vector3.zero; // 공격 시 이동 멈춤
                    PerformAttack();
                }
                break;
        }
    }

    private void MoveToTarget(Vector3 destination)
    {
        // Y축은 고정하고, XZ 평면에서의 방향 벡터만 계산
        Vector3 direction = (_targetPlayer.position - transform.position).normalized;
        direction.y = 0;

        // Rigidbody를 사용하여 속도 설정
        rb.velocity = direction * moveSpeed;
    }

    // 타겟 방향으로 몬스터를 회전시키는 함수
    private void RotateToTarget()
    {
        Vector3 directionToTarget = _targetPlayer.position - transform.position;
        directionToTarget.y = 0; // Y축 회전만 고려

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    // 상태 전환 및 공격/사망 로직 

    private void SetState(EnemyState newState)
    {
        // (상태 전환 로직) 
        if (_currentState == newState) return;
        _currentState = newState;

        switch (newState)
        {
            case EnemyState.Chase:
                // 추적 시작 시 이동 애니메이션 켜기
                animator.SetBool("run", true);
                break;
            case EnemyState.Attack:
                // 공격 시작 시 이동 정지 및 이동 애니메이션 끄기
                rb.velocity = Vector3.zero;
                animator.SetBool("run", false);
                break;
            case EnemyState.Idle:
                // 대기 상태 진입 시 이동 정지 및 애니메이션 끄기
                rb.velocity = Vector3.zero;
                animator.SetBool("run", false);
                break;
        }
    }

    private void PerformAttack()
    {
        // (공격 애니메이션 및 대미지 처리 로직)
        _isPerformingAttack = true;
        animator.SetTrigger("attack2");
        Invoke("ResetAttackFlag", attackDuration);
    }
    private void ResetAttackFlag()
    {
        _isPerformingAttack = false;
    }

    private void OnHit(Vector3 hitPoint)
    {
        // (피격 효과 로직) 
        animator.SetTrigger("take_damage");
    }

    public void Die()
    {
        // (사망 처리 로직) 
        SetState(EnemyState.Dead);
        rb.velocity = Vector3.zero;
        rb.isKinematic = true; // 사망 후 물리 효과 중지
        mainCollider.enabled = false;
        animator.SetTrigger("death");
        Destroy(gameObject, 5f);
    }
}
