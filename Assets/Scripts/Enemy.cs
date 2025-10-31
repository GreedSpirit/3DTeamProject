using UnityEngine;
using System.Collections;

// 몬스터의 현재 상태를 정의하는 ENUM 
public enum EnemyState
{
    Idle,        // 대기 또는 순찰 상태 (현재 코드는 순찰 로직 미구현)
    //Patrol,      // 순찰 상태
    Chase,       // 플레이어 추적 상태
    Attack,      // 공격 상태
    Dead,        // 사망 상태
    Stun         // 경직 상태
}
public class Enemy : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] private bool _isEpicMonster = false; // 이 몬스터가 에픽 몬스터인가?
    [SerializeField] private int _waveNumber = 0; // 이 몬스터가 속한 웨이브 번호 (1, 2, 3...)

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
    [SerializeField] private float hitboxActiveTime = 1f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDelayTime = 1f; //공격 애니메이션 시작 후 히트박스 활성화까지의 딜레이
    [SerializeField] private float stunDuration = 0.5f; // 경직 시간 추가
    // [SerializeField] private int experienceValue = 50; // 경험치 보상(구현시)

    [Header("공격 판정")]
    [SerializeField] private MonsterHitbox attackHitbox;  //몬스터의 공격 히트박스

    //상태 변수
    private EnemyState _currentState = EnemyState.Idle; // 몬스터의 현재 AI 상태
    private Transform _targetPlayer; // 플레이어 Transform
    private PlayerController _playerController; // PlayerController 컴포넌트 참조 변수
    private bool _isPerformingAttack = false; // 공격 중인지 체크
    private Coroutine _currentAttackCoroutine; // 현재 공격 코루틴을 저장할 변수
    private Coroutine _currentStunCoroutine; // 경직 코루틴 변수 추가

    public EnemyState CurrentState => _currentState;

    void Awake()
    {
        // 몬스터 시작 시 현재 체력을 최대 체력으로 초기화
        CurrentHealth = MaxHealth;
        if (rb == null) rb = GetComponent<Rigidbody>(); // 안정성을 위한 조건부 설정

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _targetPlayer = playerObj.transform;
            // PlayerController 컴포넌트를 찾아서 저장
            _playerController = playerObj.GetComponent<PlayerController>();
        }
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

        if (!_isEpicMonster)
        {

            OnHit(hitPoint);

            if (CurrentHealth <= 0)
            {
                Die();
            }
            else
            {
                // 경직 로직 실행: 피격 시 항상 Stun 상태로 전환
                // 기존 경직 코루틴 중지 및 리셋
                if (_currentStunCoroutine != null)
                {
                    StopCoroutine(_currentStunCoroutine);
                    _currentStunCoroutine = null;
                }

                SetState(EnemyState.Stun);

                // 경직 시간 코루틴 시작
                _currentStunCoroutine = StartCoroutine(StunRoutine());

                // Idle 상태에서 피격당했다면, Stun이 끝난 후 Chase 상태로 가게 StunRoutine에 맡김
            }
        }
        else // 보스 몬스터인 경우
        {
            // 보스는 피격 모션, 경직 로직을 모두 건너뜀.
            if (CurrentHealth <= 0)
            {
                Die(); // 사망 시에만 Die() 호출
            }
        }
    }

    private void UpdateAI()
    {
        // Stun 상태일 때 AI 로직을 무시하고 멈춤
        if (_currentState == EnemyState.Stun) return; 

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
                    MoveToTarget();
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

    private void MoveToTarget()
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

        // Stun 상태 종료 로직 (다른 상태로 전환될 때 Stun 코루틴 중지)
        if (_currentState == EnemyState.Stun && _currentStunCoroutine != null)
        {
            StopCoroutine(_currentStunCoroutine);
            _currentStunCoroutine = null;
        }

            if (_currentState == EnemyState.Attack && _currentAttackCoroutine != null)
        {
            StopCoroutine(_currentAttackCoroutine);
            _currentAttackCoroutine = null; // 참조 해제
            if (attackHitbox != null)
            {
                attackHitbox.DisableHitbox();
            }
            _isPerformingAttack = false;
        }
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
            case EnemyState.Stun: // 경직 상태 진입 시 이동 정지
                rb.velocity = Vector3.zero;
                animator.SetBool("run", false);
                break;

        }
    }

    private void PerformAttack()
    {
        _isPerformingAttack = true;
        animator.SetTrigger("attack2");
        _currentAttackCoroutine = StartCoroutine(AttackRoutine());
    }
    // 몬스터 공격 로직을 처리하는 코루틴
    private IEnumerator AttackRoutine()
    {
        // 공격 애니메이션이 시작된 후, 히트박스를 켤 때까지 딜레이
        // 이 시간은 직접 애니메이션을 보고 몬스터의 팔이 플레이어에게 닿기 직전으로 설정
        yield return new WaitForSeconds(attackDelayTime);

        // 공격 판정 활성화
        if (attackHitbox != null)
        {
            attackHitbox.EnableHitbox();
        }

        // 공격 판정 유지 시간
        yield return new WaitForSeconds(hitboxActiveTime);

        // 공격 판정 비활성화
        if (attackHitbox != null)
        {
            attackHitbox.DisableHitbox();
        }

        // 다음 공격까지의 쿨다운 대기
        float remainingCooldown = Mathf.Max(0f, attackCooldown - attackDelayTime - hitboxActiveTime);
        yield return new WaitForSeconds(remainingCooldown);

        // 공격 플래그 리셋 (다음 공격 가능 상태)
        _isPerformingAttack = false;
        _currentAttackCoroutine = null; // 코루틴 참조 해제
    }
    
    private void OnHit(Vector3 hitPoint)
    {
        // (피격 효과 로직) 
        animator.SetTrigger("take_damage");
    }
    private IEnumerator StunRoutine()
    {

        yield return new WaitForSeconds(stunDuration);

        // 경직 시간 종료 후 추적 상태로 복귀
        SetState(EnemyState.Chase);
    }
    public void Die()
    {
       
        SetState(EnemyState.Dead);
        rb.velocity = Vector3.zero;
        rb.isKinematic = true; // 사망 후 물리 효과 중지
        mainCollider.enabled = false;
        animator.SetTrigger("death");
        // 보상 로직: 에픽 몬스터 처치 시 보상 지급
        if (_isEpicMonster && _playerController != null && _waveNumber > 0)
        {
            // PlayerController의 보상 지급 함수 호출
            _playerController.GrantWaveReward(_waveNumber);
        }
        Destroy(gameObject, 5f);
    }
}
