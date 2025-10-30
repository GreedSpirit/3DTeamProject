using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIBulletQuantity : MonoBehaviour, IBulletConsumObserver
{
    [SerializeField] private TextMeshProUGUI _bulletQuantity;
    [SerializeField] private List<Gun> _weaponList;

    private void Awake()
    {
        _weaponList = new List<Gun>(FindObjectsOfType<Gun>());

        foreach (var weapon in _weaponList)
        {
            weapon.AddBulletObserver(this);
        }
    }

    private void OnDestroy()
    {
        foreach (var weapon in _weaponList)
        {
            weapon.RemoveBulletObserver(this);
        }        
    }
    public void OnBulletChanged(int curBullet, int maxBullet)
    {
        _bulletQuantity.text = curBullet + " / " + maxBullet;
    }
}
