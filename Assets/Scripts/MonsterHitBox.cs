using System.Collections.Generic;
using UnityEngine;

public class MonsterHitbox : MonoBehaviour
{
    [SerializeField] private int damageAmount = 10;

    // 물리 엔진의 오차로 인한 짧은 시간 내의 중복 피해를 방지
    private HashSet<Collider> damagedTargets = new HashSet<Collider>();

    private Collider hitCollider;

    void Awake()
    {
        hitCollider = GetComponent<Collider>();
        hitCollider.enabled = false;     // 몬스터가 공격하지 않을 때는 충돌 판정을 비활성화
    }

    // Enemy.cs에서 호출: 공격 판정 활성화
    public void EnableHitbox()
    {
        hitCollider.enabled = true;
        // 공격 판정이 켜질 때마다 피해 대상 목록을 초기화
        damagedTargets.Clear();
    }

    // Enemy.cs에서 호출: 공격 판정 비활성화
    public void DisableHitbox()
    {
        hitCollider.enabled = false;
        // 혹시 남아있을 수 있는 기록 정리
        damagedTargets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        
        // 이미 피해를 준 대상이라면, 추가적인 피해를 주지 않고 함수를 종료 (중복 피해 방지 체크)
        if (damagedTargets.Contains(other))
        {
            
            return;
        }

        // PlayerController 컴포넌트가 있는지 확인
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            
            // 피해 전달
            player.OnDamage(damageAmount);

            // 피해 전달 후, 이 공격(현재 판정이 활성화된 동안)에는 다시 피해를 주지 않도록 목록에 추가
            damagedTargets.Add(other);
        }
        
    }

}
