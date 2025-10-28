using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    private Camera _mainCamera; // 메인 카메라

    private Animator anim; // 애니메이션

    [SerializeField] private AudioClip _shootAudio; // 효과음
    [SerializeField] private AudioClip _reloadAudio;
    private AudioSource _audioSource; // 효과음 재생

    [SerializeField] private int _maxAmmo; // 최대 탄약 수
    [SerializeField] private int _currentAmmo; // 현재 탄약 수
    [SerializeField] private int _clipSize; // 탄창 크기

    //[SerializeField] private float _damage; // 총 데미지
    [SerializeField] private float _shootRate; // 연사 속도
    private float _shootTimer; // 탄 발사 쿨다운 타이머

    private const float _reloadTime = 3.0f; // 재장전 시간
    private bool isReloading = false; // 현재 장전 중인지 확인

    private RaycastHit hitInfo;

    void Start()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        anim = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

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

        anim.SetTrigger("Reload");

        if (_audioSource != null && _reloadAudio != null)
        {
            _audioSource.PlayOneShot(_reloadAudio);
        }

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
            anim.SetTrigger("Shoot");

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
            Debug.Log($"{_currentAmmo} / {_maxAmmo}");
        }
        anim.SetTrigger("Idle");
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
        anim.SetTrigger("Idle");
    }
}