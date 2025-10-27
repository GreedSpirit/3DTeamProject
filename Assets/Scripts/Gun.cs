using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera; // 메인 카메라
    [SerializeField] private AudioClip _audio; // 효과음

    [SerializeField] private int _maxAmmo = 120; // 최대 탄약 수
    [SerializeField] private int _currentAmmo = 30; // 현재 탄약 수
    [SerializeField] private const int _clipSize = 30; // 탄창 크기

    //[SerializeField] private float _damage; // 총 데미지
    [SerializeField] private float _shootRate = 5f; // 연사 속도
    private float _shootTimer = 0f; // 탄 발사 쿨다운 타이머

    [SerializeField] private float _reloadTime = 3.0f; // 재장전 시간
    private bool isReloading = false; // 현재 장전 중인지 확인

    private RaycastHit hitInfo;

    void Update()
    {
        if (_shootTimer > 0)
        {
            _shootTimer -= Time.deltaTime;
        }

        if (!isReloading)
        {
            // 마우스 좌클릭을 눌렀다면 Shoot
            if (Input.GetMouseButton(0) && _shootTimer <= 0f && _currentAmmo > 0)
            {
                // 쿨다운 설정
                _shootTimer = 1f / _shootRate;
                Shoot();
            }

            // R키를 눌렀다면 Reload
            if (Input.GetKeyDown(KeyCode.R))
            {
                StartCoroutine(ReloadCoroutine());
            }
        }

        // 장전 중이 아닌데, 현재 탄약이 0이고, 남은 탄약이 있다면
        if (!isReloading && _currentAmmo <= 0 && _maxAmmo > 0)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    IEnumerator ReloadCoroutine()
    {
        isReloading = true;

        // 탄창이 꽉 찼을 경우
        if (_currentAmmo == _clipSize)
        {
            Debug.Log("이미 탄창이 꽉 찼습니다.");
            isReloading = false;
            yield break;
        }
        // 남은 탄약이 없을 경우
        if (_maxAmmo <= 0)
        {
            Debug.Log("남은 탄약이 없습니다.");
            isReloading = false;
            yield break;
        }
        Debug.Log("장전 시작");
        yield return new WaitForSeconds(_reloadTime);
        Reload();
    }

    void Shoot()
    {
        if (!isReloading && _currentAmmo <= 0 && _maxAmmo <= 0)
        {
            Debug.Log("남은 탄약이 없습니다.");
        }
        else
        {
            Vector3 origin = _mainCamera.transform.position; // 시작점
            Vector3 direction = _mainCamera.transform.forward; // 방향

            if (Physics.Raycast(origin, direction, out RaycastHit hitInfo))
            {
                Hit(hitInfo);
            }

            _currentAmmo--;
            Debug.Log($"{_currentAmmo} / {_maxAmmo}");
        }
    }

    void Hit(RaycastHit hitInfo)
    {
        // 맞은 오브젝트의 Tag가 Enemy일 경우
        if (hitInfo.collider.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Enemy Hit");
        }
    }

    void Reload()
    {
        // 필요한 탄약 계산
        int needAmmo = _clipSize - _currentAmmo;

        // 남은 탄약이 부족할 경우 가져올 실제 탄약 수 제한
        // needAmmo = 10, _maxAmmo = 5일 경우 5만큼만 장전한다.
        int ReloadAmmo = Mathf.Min(needAmmo, _maxAmmo);

        // 탄약 장전 적용
        _maxAmmo -= ReloadAmmo;
        _currentAmmo += ReloadAmmo;

        Debug.Log("장전 끝");
        Debug.Log($"{_currentAmmo} / {_maxAmmo}");

        isReloading = false;
    }
}