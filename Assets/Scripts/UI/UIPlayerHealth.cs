using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIPlayerHealth : MonoBehaviour, IPlayerHealthObserver
{
    [SerializeField] private TextMeshProUGUI _playerHealth;
    public void OnPlayerHealthChanged(float curHp, float maxHp)
    {
        _playerHealth.text = "Health : " + curHp + " / " + maxHp;
    }
}
