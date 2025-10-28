using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // �̵� WASD, ���� Space, �޸��� Shift, ���� ��Ŭ��, �� ��Ŭ��, �� ��ü 1,2,3,4Ű or �� ��/�ٿ�

    // �̺�Ʈ
    event Action OnDeath;
    //event Action<int> OnHealthChange;
    

    // Weapon _curWeapon; //���� ����
    // List<Weapon> _weaponList; // ���� ���
    // int _weaponIndex; // ���� ���� �ε���
    private KeyCode[] _weaponKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };//���� ��ü�� Ű���� 1 2 3 4 �̸���Ƶα�


    [SerializeField] private LayerMask _groundLayer;//����ĳ��Ʈ üũ���
    [SerializeField] private float _groundCheckDistance = 100f;//����ĳ��Ʈ ����

    [SerializeField] private float _moveSpeed = 50f;//�̵� �ӵ�
    [SerializeField] private float _dashSpeed = 100f;//�뽬 �ӵ�
    [SerializeField] private float _JumpForce = 0.5f;//������

    [SerializeField] private float _tmpRecoil = 1.0f;//�ݵ� �׽�Ʈ��

    [SerializeField] private float _mouseSensitivity = 2.5f;//���콺 �ΰ���


    [SerializeField] private float _cameraRotationLimit = 90;// ī�޶� �����Ѱ�,
    [SerializeField] private float _baseFOV = 60; // �⺻ �þ� ��
    [SerializeField] private float _zoomFOV = 30; // �� �þ� ��


    [SerializeField] private float _rollSpeed = 25f;//������ �ӵ�
    [SerializeField] private float _rollTime = 0.5f;//������ �ð�
    [SerializeField] private float _rollSize = 0.7f;//������ ũ�� ����
    private Vector3 originalScale;
    private bool isRolling = false;

    [SerializeField] private Camera _myCamera;
    [SerializeField]
    public int maxHp// �ִ�ü��
    {
        get; set;
    } = 100;
    private int _currentHp;//���� ü��
    private float _curCameraRotationX = 0;//���� ī�޶󰢵�
    private Rigidbody _myRigid;

    public List<IPlayerHealthObserver> _healthObservers = new List<IPlayerHealthObserver>();
    public void AddHealthObserver(IPlayerHealthObserver observer) => _healthObservers.Add(observer);
    public void RemoveHealthObserver(IPlayerHealthObserver observer) => _healthObservers.Remove(observer);

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _myRigid = GetComponent<Rigidbody>();
        originalScale = transform.localScale;

        NotifyHealthChanged();
    }

    private void OnEnable()
    {
        _currentHp = maxHp;
    }

    void Update()
    {
        if(Time.timeScale == 0)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            return;
        }
        Debug.DrawRay(transform.position, Vector3.down * _groundCheckDistance, Color.red);

        if (isRolling)
            return;

        Shoot();//�Ѿ˹߻�
        TryRolling();
        Reloading();
    }
    private void FixedUpdate()
    {
        Cursor.lockState = CursorLockMode.Locked;
        if (isRolling)
            return;
        Jump();//����
        Move();//�̵�
        

    }
    private void LateUpdate()
    {
        if(Time.timeScale == 0)
        {
            return;
        }
        if (isRolling)
            return;
        ZoomIn();//��
        PlayerRotate();//ī�޶� �¿��̵�
        CameraRotate();//ī�޶� �����̵�

    }
    public void OnDamage(int _dmg)
    {
        _currentHp -= _dmg;
        //OnHealthChange?.Invoke(_currentHp);
        NotifyHealthChanged();
        if (_currentHp <= 0)
        {
            OnDeath?.Invoke();
        }

    }

    /*
     * void ChangeWeapon()
    {
        for (int i = 0; i < _weaponKeys.Length; i++) // ��ȣ������ ��ü
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

        float scroll = Input.GetAxis("Mouse ScrollWheel");//�� ��, �� �ٿ����� ��ü

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

    private void Move()//�̵�
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
    private void Jump()//����
    {
        if (Input.GetKey(KeyCode.Space) && IsGround())
        {
            _myRigid.AddForce(Vector3.up * _JumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGround()//����üũ
    {
        return Physics.Raycast(transform.position, Vector3.down, _groundCheckDistance, _groundLayer);

    }

    void TryRolling()//������ üũ
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isRolling)
        {
            Debug.Log("roll");
            StartCoroutine(Rolling());
        }
    }
    IEnumerator Rolling() //������ �̵�
    {   // ���� ���̱�
        transform.localScale = new Vector3(originalScale.x, originalScale.y * _rollSize, originalScale.z);
        _myRigid.velocity = transform.forward * _rollSpeed;
        isRolling = true;


        // rollTime ���� ������ ����
        yield return new WaitForSeconds(_rollTime);

        // ���� ����
        transform.localScale = originalScale;
        isRolling = false;
    }

    private void ZoomIn()//�� ���
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
    private void PlayerRotate()//ȭ�� �¿�ȸ��
    {

        float rotationY = Input.GetAxisRaw("Mouse X");
        Vector3 playerRotaionY = new Vector3(0f, rotationY, 0f) * _mouseSensitivity;
        _myRigid.MoveRotation(_myRigid.rotation * Quaternion.Euler(playerRotaionY));

    }

    private void CameraRotate()//ȭ�� ����ȸ��
    {
        float rotationX = Input.GetAxisRaw("Mouse Y");
        float cameraRotaionX = rotationX * _mouseSensitivity;
        _curCameraRotationX -= cameraRotaionX;
        _curCameraRotationX = Math.Clamp(_curCameraRotationX, -_cameraRotationLimit, _cameraRotationLimit);
        _myCamera.transform.localEulerAngles = new Vector3(_curCameraRotationX, 0f, 0f);

    }
    private void Shoot()//������
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Gun.Shoot(); //TryShoot?
           // _curCameraRotationX -= _tmpRecoil;
        }
    }

    private void Reloading()//������ 
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //_curWeapon.Reload();
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
