using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIBulletQuantity : MonoBehaviour, IBulletConsumObserver
{
    [SerializeField] private TextMeshProUGUI _bulletQuantity;
    public void OnBulletChanged(int curBullet, int maxBullet)
    {
        _bulletQuantity.text = curBullet + " / " + maxBullet;
    }
}
