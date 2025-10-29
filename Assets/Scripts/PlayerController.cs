using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 이동 WASD, 점프 Space, 달리기 Shift, 공격 좌클릭, 줌 우클릭, 총 교체 1,2,3,4키 or 휠 업/다운
    // 이벤트
    event Action OnDeath;
    //event Action<int> OnHealthChange;

    Gun _curGun; //현재 무기
    List<Gun> _weaponList; // 무기 목록
    int _gunIndex; // 현재 무기 인덱스

    private KeyCode[] _weaponKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };//무기 교체시 키보드 1 2 3 4 미리담아두기
    [Header("레이캐스트")]
    [SerializeField] private LayerMask _groundLayer;//지상 레이캐스트 체크대상 레이어
    [SerializeField] private float _groundCheckDistance = 1.1f;//지상 체크 레이캐스트 길이
    [SerializeField] private float _interactDistance = 2f;//상호작용 레이캐스트 길이

    [Header("기본 이동 제어")]
    [SerializeField] private float _moveSpeed = 10f;//이동 속도
    [SerializeField] private float _dashSpeed = 20f;//대쉬 속도
    [SerializeField] private float _JumpForce = 0.5f;//점프력

    [SerializeField] private float _tmpRecoil = 3.0f;//반동 테스트값

    [Header("카메라 제어")]
    [SerializeField] private float _mouseSensitivity = 2.5f;//마우스 민감도
    [SerializeField] private Camera _myCamera;
    [SerializeField] private float _cameraRotationLimit = 90;// 카메라 상하한계,
    [SerializeField] private float _baseFOV = 60; // 기본 시야 각
    [SerializeField] private float _zoomFOV = 30; // 줌 시야 각

    [Header("구르기")]
    [SerializeField] private float _rollSpeed = 25f;//구르기 속도
    [SerializeField] private float _rollTime = 0.5f;//구르기 시간
    [SerializeField] private float _rollSize = 0.7f;//구를때 크기 비율
    private Vector3 _originalScale;
    private bool _isRolling = false;

    private bool _isAlive = true;
    private bool _isWalking = true;
    private bool _isDashing = true;


    [Header("걷기 소리 간격")]
    [SerializeField] private float _moveSoundInterval = 0.5f;//구르기 속도
    [SerializeField] private float _dashSoundInterval = 0.2f;//구르기 속도
    private AudioSource[] audioSources;
    private float lastStepSoundTime = 0f;


    [Header("죽음")]
    [SerializeField]
    private float _deathAnimeMotionTime = 1.0f;
    private Quaternion _aliveRotation;
    private Quaternion _deathRotation;
    private float _elapsedTime = 0f;

    [SerializeField]
    public int maxHp// 최대체력
    {
        get; set;
    } = 100;
    private int _currentHp;//현재 체력
    private float _curCameraRotationX = 0;//현재 카메라각도
    private Rigidbody _myRigid;

    public List<IPlayerHealthObserver> _healthObservers = new List<IPlayerHealthObserver>();
    public void AddHealthObserver(IPlayerHealthObserver observer) => _healthObservers.Add(observer);
    public void RemoveHealthObserver(IPlayerHealthObserver observer) => _healthObservers.Remove(observer);

    void Start()
    {
        Cursor.visible = false;
        _myRigid = GetComponent<Rigidbody>();
        _originalScale = transform.localScale;

        // _weaponList 초기화
        _weaponList = new List<Gun>();
        // 컴포넌트를 찾는데 비활성화된 자식도 모두 찾기
        foreach (Gun gun in _myCamera.GetComponentsInChildren<Gun>(true))
        {
            _weaponList.Add(gun);
            gun.gameObject.SetActive(false); // 모든 무기를 비활성화
        }

        // 1번 무기를 기본 무기로 설정
        if (_weaponList.Count > 0)
        {
            _gunIndex = 0;
            _curGun = _weaponList[_gunIndex];
            _curGun.gameObject.SetActive(true);
        }

        NotifyHealthChanged();
        audioSources = GetComponents<AudioSource>();
    }

    private void OnEnable()
    {
        _currentHp = maxHp;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            OnDamage(10);
        }


        if (Time.timeScale == 0)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            return;
        }
        Debug.DrawRay(transform.position, Vector3.down * _groundCheckDistance, Color.red);
        Death();

        if (!_isAlive)
            return;

        if (_isRolling)
            return;
        Shoot();//총알발사
        TryRolling();
        Reloading();
        Interact();
        ChangeWeapon();
    }
    private void FixedUpdate()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (!_isAlive)
            return;
        if (_isRolling)
            return;
        Jump();//점프
        Move();//이동
    }
    private void LateUpdate()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (!_isAlive)
            return;
        if (_isRolling)
            return;
        StepSoundPlay();
        ZoomIn();//줌
        PlayerRotate();//화면 좌우회전
        CameraRotate();//화면 상하이동
    }
    public void OnDamage(int _dmg)
    {
        if (!_isAlive)
            return;
        _currentHp -= _dmg;
        audioSources[1].Play();
        //OnHealthChange?.Invoke(_currentHp);
        NotifyHealthChanged();
        if (_currentHp <= 0)
        {
            _isAlive = false;
            OnDeath?.Invoke();
            audioSources[2].Play();
        }
    }

    void Death()//점점 넘어지기
    {
        if (!_isAlive)
        {
            if (_elapsedTime < _deathAnimeMotionTime)
            {
                _elapsedTime += Time.deltaTime;
                float t = _elapsedTime / _deathAnimeMotionTime;

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, 90), t);
                return;
            }
        }
        OnDeath?.Invoke();

    }
    // 무기교체
    void ChangeWeapon()
    {
        for (int i = 0; i < _weaponKeys.Length; i++)
        {
            if (Input.GetKeyDown((_weaponKeys[i])))
            {
                if (i < _weaponList.Count && _weaponList[i] != null && _gunIndex != i)
                {
                    // 장전 상태를 강제 취소
                    _curGun.CancelReload();

                    // 현재 무기 비활성화
                    _curGun.gameObject.SetActive(false);

                    // 새 무기로 교체
                    _curGun = _weaponList[i];
                    _gunIndex = i;

                    // 새 무기 활성화, DrawWeapon()메서드 호출
                    _curGun.gameObject.SetActive(true);
                    _curGun.DrawWeapon();
                    break;
                }
            }
        }

        //float scroll = Input.GetAxis("Mouse ScrollWheel");//휠 업, 휠 다운으로 교체

        //if (scroll != 0)
        //{
        //    if (scroll > 0f)
        //    {
        //        _gunIndex--;
        //        if (_gunIndex < 0)
        //        {
        //            _gunIndex = _weaponList.Count - 1;
        //        }
        //    }
        //    else if (scroll < 0f)
        //    {
        //        _gunIndex++;
        //        if (_gunIndex >= _weaponList.Count)
        //        {
        //            _gunIndex = 0;
        //        }
        //    }
        //    _curGun = _weaponList[_gunIndex];
        //}
    }


    private void Move()//이동
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 moveHor = transform.right * moveX;
        Vector3 moveVer = transform.forward * moveZ;
        Vector3 MoveDir = (moveHor + moveVer).normalized;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            _isWalking = false;
            _isDashing = true;
            _myRigid.velocity = new Vector3(MoveDir.x * _dashSpeed, _myRigid.velocity.y, MoveDir.z * _dashSpeed);
        }
        else if (Input.GetKey(KeyCode.W) ||
                Input.GetKey(KeyCode.A) ||
                Input.GetKey(KeyCode.S) ||
                 Input.GetKey(KeyCode.D))
        {
            _isWalking = true;
            _isDashing = false;
            _myRigid.velocity = new Vector3(MoveDir.x * _moveSpeed, _myRigid.velocity.y, MoveDir.z * _moveSpeed);
        }
        else
        {
            _isWalking = false;
            _isDashing = false;

            _myRigid.velocity = new Vector3(MoveDir.x * _moveSpeed * 0.9f, _myRigid.velocity.y, MoveDir.z * _moveSpeed * 0.9f);
        }

    }
    private void StepSoundPlay()
    {
        float stepSoundTime;
        if (_isWalking)
            stepSoundTime = _moveSoundInterval;
        else if (_isDashing)
            stepSoundTime = _dashSoundInterval;
        else
            return;

        if (Time.time - lastStepSoundTime >= stepSoundTime)
        {
            audioSources[0].Play();
            lastStepSoundTime = Time.time;
        }
    }
    private void Jump()//점프
    {
        if (Input.GetKey(KeyCode.Space) && IsGround())
        {
            _myRigid.AddForce(Vector3.up * _JumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGround()//지면체크
    {
        return Physics.Raycast(transform.position, Vector3.down, _groundCheckDistance, _groundLayer);

    }

    void TryRolling()//구르기 체크
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && !_isRolling)
        {
            Debug.Log("roll");
            StartCoroutine(Rolling());
        }
    }
    IEnumerator Rolling() //구르기 이동
    {   // 높이 줄이기
        transform.localScale = new Vector3(_originalScale.x, _originalScale.y * _rollSize, _originalScale.z);
        _myRigid.velocity = transform.forward * _rollSpeed;
        _isRolling = true;

        // rollTime 동안 구르기 유지
        yield return new WaitForSeconds(_rollTime);

        // 높이 복구
        transform.localScale = _originalScale;
        _isRolling = false;
    }

    void Interact()
    {
        Ray ray = _myCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * _interactDistance, Color.yellow, 1f);
        if (Physics.Raycast(ray, out hit, _interactDistance) && hit.collider.TryGetComponent<IInteractable>(out IInteractable inter))
        {
            if (inter.isInteract)
            {
                Debug.Log("상호작용 가능");
                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (inter.Use() && inter is BulletBox)
                    {
                        //_curGun.RefillAmmo();
                        Debug.Log("탄약 보충");
                    }
                }
            }
        }
    }
    private void ZoomIn()//줌 기능
    {
        if (Input.GetMouseButtonDown(1))
        {
            _myCamera.fieldOfView = _zoomFOV;
        }
        else if (Input.GetMouseButtonUp(1))
        {

            _myCamera.fieldOfView = _baseFOV;
        }
    }
    private void PlayerRotate()//화면 좌우회전
    {

        float rotationY = Input.GetAxisRaw("Mouse X");
        Vector3 playerRotaionY = new Vector3(0f, rotationY, 0f) * _mouseSensitivity;
        _myRigid.MoveRotation(_myRigid.rotation * Quaternion.Euler(playerRotaionY));

    }

    private void CameraRotate()//화면 상하이동
    {
        float rotationX = Input.GetAxisRaw("Mouse Y");
        float cameraRotaionX = rotationX * _mouseSensitivity;
        _curCameraRotationX -= cameraRotaionX;
        _curCameraRotationX = Math.Clamp(_curCameraRotationX, -_cameraRotationLimit, _cameraRotationLimit);
        _myCamera.transform.localEulerAngles = new Vector3(_curCameraRotationX, 0f, 0f);

    }

    public void Recoil(float recoil) // 무기 반동
    {
        _curCameraRotationX -= recoil;
    }

    private void Shoot()//무기사용
    {
        if (Input.GetMouseButton(0))
        {
            // ⭐️ Gun의 Shoot() 메서드를 호출하고 반동 값을 받음
            float recoilAmount = _curGun.Shoot();

            // ⭐️ 반환된 반동 값이 0보다 클 때 (즉, 발사가 실제로 성공했을 때)만 반동을 적용
            if (recoilAmount > 0)
            {
                // _tmpRecoil 필드 대신, Gun에서 실제 발생한 반동 값을 사용
                Recoil(recoilAmount);
            }
        }
    }

    private void Reloading()//재장전 
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            _curGun.StartReload();
        }
    }

    private void NotifyHealthChanged()
    {
        foreach (IPlayerHealthObserver observer in _healthObservers)
        {
            observer?.OnPlayerHealthChanged(_currentHp, maxHp);
        }
    }
}