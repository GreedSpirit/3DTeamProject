using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    private Camera _mainCamera; // 메인 카메라

    private Animator anim; // 애니메이션

    private AudioSource _audioSource; // 효과음 재생
    [SerializeField] private AudioClip _shootAudio; // 발사 효과음
    [SerializeField] private AudioClip _reloadAudio; // 장전 효과음
    [SerializeField] private AudioClip _refillAmmo; // 탄약 보급 효과음

    [SerializeField] public int _maxAmmo; // 최대 탄약 수
    [SerializeField] public int _currentAmmo; // 현재 탄약 수
    [SerializeField] public int _clipSize; // 탄창 크기

    [SerializeField] private float _damage; // 총 데미지
    [SerializeField] private float _reloadTime; // 재장전 시간
    [SerializeField] private float _shootRate; // 연사 속도
    [SerializeField] private float _recoilForce = 20f; //반동
    [SerializeField] private int _aimingFov = 10; //조준정도

    [Header("Reward Stats")]
    // 공격력 및 탄창 증가량을 인스펙터에서 설정할 수 있도록 추가
    [SerializeField] private float _attackIncreaseAmount = 5f;
    [SerializeField] private int _magazineIncreaseAmount = 5;

    private float _shootTimer; // 탄 발사 쿨다운 타이머


    public bool isReloading = false; // 현재 장전 중인지 확인

    private Coroutine _reloadCoroutine; // 현재 실행 중인 재장전 코루틴
    private RaycastHit hitInfo;

    public List<IBulletConsumObserver> _healthObservers = new List<IBulletConsumObserver>();
    public void AddBulletObserver(IBulletConsumObserver observer) => _healthObservers.Add(observer);
    public void RemoveBulletObserver(IBulletConsumObserver observer) => _healthObservers.Remove(observer);



    private void Start()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        anim = GetComponent<Animator>();

        _audioSource = GetComponent<AudioSource>();

        NotifyBulletChanged();
    }

    private void Update()
    {
        if (_shootTimer > 0)
        {
            _shootTimer -= Time.deltaTime;
        }

    }

    public int GetFov()
    {
        return _aimingFov;
    }
    public float GetRecoilForec()
    {
        return _recoilForce;
    }

    public bool Shoot()
    {
        // 발사 불가능 조건 체크
        if (isReloading || _shootTimer > 0)
        {
            return false;
        }

        // 탄약 부족 체크
        if (_currentAmmo <= 0)
        {
            // 남은 예비 탄약이 없을 경우
            if (_maxAmmo >= 1)
            {
                StartCoroutine(ReloadCoroutine());
            }
            return false;
        }

        // 쿨다운 설정
        _shootTimer = 1f / _shootRate;

        // 애니메이션 설정
        anim.SetTrigger("Shoot");

        // 효과음 설정
        if (_audioSource != null && _shootAudio != null)
        {
            _audioSource.PlayOneShot(_shootAudio);
        }

        Vector3 origin = _mainCamera.transform.position; // 시작점
        Vector3 direction = _mainCamera.transform.forward; // 방향

        if (Physics.Raycast(origin, direction, out RaycastHit hitInfo))
        {
            Hit(hitInfo);
        }

        _currentAmmo--;
        NotifyBulletChanged();
        
        anim.SetTrigger("Idle");

        // 발사 성공 true 반환
        return true;
    }

    private void Hit(RaycastHit hitInfo)
    {
        // 맞은 오브젝트의 Tag가 Enemy일 경우
        if (hitInfo.collider.gameObject.CompareTag("Enemy"))
        {
            //Debug.Log("Enemy Hit");

            // 몬스터의 Enemy 컴포넌트를 가져옵니다.
            Enemy enemy = hitInfo.collider.GetComponent<Enemy>();

            if (enemy != null)
            {
                // 몬스터의 TakeDamage 함수를 호출하고,
                // 이 총의 데미지(_damage)와 충돌 지점(hitInfo.point)을 전달합니다.
                enemy.TakeDamage(_damage, hitInfo.point);
            }
        }
    }

    public void StartReload()
    {
        if (isReloading)
        {
            //Debug.Log("이미 장전 중입니다.");
            return;
        }

        // 탄창이 꽉 찼을 경우
        if (_currentAmmo == _clipSize)
        {
            //Debug.Log("이미 탄창이 꽉 찼습니다.");
            return;
        }

        // 남은 탄약이 없을 경우
        if (_maxAmmo <= 0)
        {
            //Debug.Log("남은 탄약이 없습니다.");
            return;
        }

        isReloading = true; // 플래그만 설정 (PlayerController가 코루틴을 실행할 것임)
    }

    public IEnumerator ReloadCoroutine()
    {
        isReloading = true;

        // 탄창이 꽉 찼을 경우
        if (_currentAmmo == _clipSize)
        {
            //Debug.Log("이미 탄창이 꽉 찼습니다.");
            isReloading = false;
            yield break;
        }
        // 남은 탄약이 없을 경우
        if (_maxAmmo <= 0)
        {
            //Debug.Log("남은 탄약이 없습니다.");
            isReloading = false;
            yield break;
        }

        //Debug.Log("장전 시작");

        anim.SetTrigger("Reload");

        if (_audioSource != null && _reloadAudio != null)
        {
            _audioSource.PlayOneShot(_reloadAudio);
        }

        yield return new WaitForSeconds(_reloadTime);

        Reload();

    }

    private void Reload()
    {
        // 필요한 탄약 계산
        int needAmmo = _clipSize - _currentAmmo;

        // 남은 탄약이 부족할 경우 가져올 실제 탄약 수 제한
        // needAmmo = 10, _maxAmmo = 5일 경우 5만큼만 장전한다.
        int ReloadAmmo = Mathf.Min(needAmmo, _maxAmmo);

        // 탄약 장전 적용
        _maxAmmo -= ReloadAmmo;
        _currentAmmo += ReloadAmmo;
        NotifyBulletChanged();

        //Debug.Log("장전 끝");
        //Debug.Log($"{_currentAmmo} / {_maxAmmo}");

        isReloading = false;
        anim.SetTrigger("Idle");
    }

    // 장전 강제 취소
    public void CancelReload()
    {
        if (isReloading)
        {
            //Debug.Log("장전 취소됨");
            // PlayerController가 전달한 코루틴을 중지
            

            isReloading = false; // 장전 상태 false
            anim.SetTrigger("Idle"); // 애니메이션 리셋
        }
    }

    // 무기 들기
    public void DrawWeapon()
    {
        CancelReload();
        NotifyBulletChanged();

        anim.SetTrigger("Draw");
    }

    public void RefillAmmo()
    {
        _maxAmmo += 30; // 임시로 30 추가
        _audioSource.PlayOneShot(_refillAmmo);
        NotifyBulletChanged();
    }

    private void NotifyBulletChanged()
    {
        foreach (IBulletConsumObserver observer in _healthObservers)
        {
            observer?.OnBulletChanged(_currentAmmo, _maxAmmo);
        }
    }

    // 웨이브 2 보상: 공격력 상승
    public void IncreaseDamage()
    {
        _damage += _attackIncreaseAmount;
    }

    // 웨이브 3 보상: 탄창 증가
    public void IncreaseClipSize()
    {
        int increaseAmount = _magazineIncreaseAmount;

        // 탄창 크기 증가
        _clipSize += increaseAmount;

        // 현재 탄창을 증가된 크기만큼 즉시 장전
        if (_currentAmmo < _clipSize)
        {
            int needAmmo = _clipSize - _currentAmmo;
            int loadAmmo = Mathf.Min(needAmmo, _maxAmmo);

            _maxAmmo -= loadAmmo;
            _currentAmmo += loadAmmo;
        }

        NotifyBulletChanged();
    }

}