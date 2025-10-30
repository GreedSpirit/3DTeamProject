using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIBulletQuantity : MonoBehaviour, IBulletConsumObserver
{
    [SerializeField] private TextMeshProUGUI _bulletQuantity;
    [SerializeField] private Gun _gun;

    private void Awake()
    {
        _gun.AddBulletObserver(this);
    }

    private void OnDestroy()
    {
        _gun.RemoveBulletObserver(this);        
    }
    public void OnBulletChanged(int curBullet, int maxBullet)
    {
        _bulletQuantity.text = curBullet + " / " + maxBullet;
    }
}
