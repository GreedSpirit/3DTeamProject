using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 이벤트
    event Action OnDeath;
    event Action<int> OnHealthChange;

    //Weapon curGun;
    //List<Weapon> weaponList;

    [SerializeField] private LayerMask groundLayer;//레이캐스트 체크대상
    [SerializeField] private float groundCheckDistance = 100f;//레이캐스트 길이

    [SerializeField] private float _moveSpeed = 50f;//이동 속도
    [SerializeField] private float _dashSpeed = 100f;//대쉬 속도
    [SerializeField] private float _JumpForce = 0.5f;//점프력

    [SerializeField] private float tmpRecoil = 1.0f;//반동 테스트값

    [SerializeField] private float _mouseSensitivity = 2.5f;//마우스 민감도


    [SerializeField] private float _cameraRotationLimit;// 카메라 상하한계,
    [SerializeField] private float baseFOV = 60;
    [SerializeField] private float ZoomFOV = 30;

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
        Cursor.visible = false;
        _myRigid = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        _currentHp = maxHp;
    }

    void Update()
    {
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.red);

        Shoot();//총알발사


    }
    private void FixedUpdate()
    {
        Jump();//점프
        Move();//이동

    }
    private void LateUpdate()
    {
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

    private void Move()//이동
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 moveHor = transform.right * moveX;
        Vector3 moveVer = transform.forward * moveZ;
        Vector3 velocity;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            velocity = (moveHor + moveVer).normalized * _dashSpeed;
        }
        else
        {
            velocity = (moveHor + moveVer).normalized * _moveSpeed;
        }
        _myRigid.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
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
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

    }

    private void ZoomIn()//줌 기능
    {
        if (Input.GetMouseButtonDown(1))
        {
            _myCamera.fieldOfView = ZoomFOV;
        }
        else if (Input.GetMouseButtonUp(1))
        {

            _myCamera.fieldOfView = baseFOV;
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
            _curCameraRotationX -= tmpRecoil;

        }
    }
}
