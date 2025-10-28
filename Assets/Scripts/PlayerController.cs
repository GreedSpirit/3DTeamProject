using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 이동 WASD, 점프 Space, 달리기 Shift, 공격 좌클릭, 줌 우클릭, 총 교체 1,2,3,4키 or 휠 업/다운

    // 이벤트
    event Action OnDeath;
    event Action<int> OnHealthChange;

    // Weapon _curWeapon; //현재 무기
    // List<Weapon> _weaponList; // 무기 목록
    // int _weaponIndex; // 현재 무기 인덱스
    private KeyCode[] _weaponKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };//무기 교체시 키보드 1 2 3 4 미리담아두기


    [SerializeField] private LayerMask _groundLayer;//레이캐스트 체크대상
    [SerializeField] private float _groundCheckDistance = 100f;//레이캐스트 길이

    [SerializeField] private float _moveSpeed = 50f;//이동 속도
    [SerializeField] private float _dashSpeed = 100f;//대쉬 속도
    [SerializeField] private float _JumpForce = 0.5f;//점프력

    [SerializeField] private float _tmpRecoil = 1.0f;//반동 테스트값

    [SerializeField] private float _mouseSensitivity = 2.5f;//마우스 민감도


    [SerializeField] private float _cameraRotationLimit = 90;// 카메라 상하한계,
    [SerializeField] private float _baseFOV = 60; // 기본 시야 각
    [SerializeField] private float _zoomFOV = 30; // 줌 시야 각


    [SerializeField] private float _rollSpeed = 25f;//구르기 속도
    [SerializeField] private float _rollTime = 0.5f;//구르기 시간
    [SerializeField] private float _rollSize = 0.7f;//구를때 크기 비율
    private Vector3 originalScale;
    private bool isRolling = false;

    [SerializeField] private Camera _myCamera;
    [SerializeField]
    public int maxHp// 최대체력
    {
        get; set;
    } = 100;
    private int _currentHp;//현재 체력
    private float _curCameraRotationX = 0;//현재 카메라각도
    private Rigidbody _myRigid;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _myRigid = GetComponent<Rigidbody>();
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        _currentHp = maxHp;
    }

    void Update()
    {
        Debug.DrawRay(transform.position, Vector3.down * _groundCheckDistance, Color.red);

        if (isRolling)
            return;

        Shoot();//총알발사
        TryRolling();
        Reloading();
    }
    private void FixedUpdate()
    {
        if (isRolling)
            return;
        Jump();//점프
        Move();//이동
        

    }
    private void LateUpdate()
    {
        if (isRolling)
            return;
        ZoomIn();//줌
        PlayerRotate();//카메라 좌우이동
        CameraRotate();//카메라 상하이동

    }
    public void OnDamage(int _dmg)
    {
        _currentHp -= _dmg;
        OnHealthChange?.Invoke(_currentHp);
        if (_currentHp <= 0)
        {

            OnDeath?.Invoke();
        }

    }

    /*
     * void ChangeWeapon()
    {
        for (int i = 0; i < _weaponKeys.Length; i++) // 번호눌러서 교체
        {
            if (Input.GetKeyDown((_weaponKeys[i])))
            {
                if (i < _weaponList.Count && _weaponList[i] != null)
                {
                    _curWeapon = _weaponList[i];
                    _weaponIndex = i;
                    break;
                }
            }
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");//휠 업, 휠 다운으로 교체

        if (scroll != 0)
        {
            if(scroll > 0f)
            {
                _weaponIndex--;
                if(_weaponIndex < 0)
                {
                    _weaponIndex = _weaponList.Count-1;
                }
            }
            else if(scroll < 0f)
            {
                _weaponIndex++;
                if(_weaponIndex >= _weaponList.Count)
                {
                    _weaponIndex = 0;
                }
            }
            _curWeapon = _weaponList[_weaponIndex];
        }

    }
    */

    private void Move()//이동
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 moveHor = transform.right * moveX;
        Vector3 moveVer = transform.forward * moveZ;
        Vector3 MoveDir = (moveHor + moveVer).normalized;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            _myRigid.velocity = new Vector3(MoveDir.x * _dashSpeed, _myRigid.velocity.y, MoveDir.z * _dashSpeed);
        }
        else
        {

            _myRigid.velocity = new Vector3(MoveDir.x * _moveSpeed, _myRigid.velocity.y, MoveDir.z * _moveSpeed);
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
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isRolling)
        {
            Debug.Log("roll");
            StartCoroutine(Rolling());
        }
    }
    IEnumerator Rolling() //구르기 이동
    {   // 높이 줄이기
        transform.localScale = new Vector3(originalScale.x, originalScale.y * _rollSize, originalScale.z);
        _myRigid.velocity = transform.forward * _rollSpeed;
        isRolling = true;


        // rollTime 동안 구르기 유지
        yield return new WaitForSeconds(_rollTime);

        // 높이 복구
        transform.localScale = originalScale;
        isRolling = false;
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

    private void CameraRotate()//화면 상하회전
    {
        float rotationX = Input.GetAxisRaw("Mouse Y");
        float cameraRotaionX = rotationX * _mouseSensitivity;
        _curCameraRotationX -= cameraRotaionX;
        _curCameraRotationX = Math.Clamp(_curCameraRotationX, -_cameraRotationLimit, _cameraRotationLimit);
        _myCamera.transform.localEulerAngles = new Vector3(_curCameraRotationX, 0f, 0f);

    }
    private void Shoot()//무기사용
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Gun.Shoot(); //TryShoot?
           // _curCameraRotationX -= _tmpRecoil;
        }
    }

    private void Reloading()//재장전 
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //_curWeapon.Reload();
        }
    }
}
