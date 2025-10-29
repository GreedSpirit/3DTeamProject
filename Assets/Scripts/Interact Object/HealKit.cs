using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealKit : MonoBehaviour, IInteractable
{
    public bool _isInteract { get; set; } = true;
    [SerializeField] private int _healValue= 10;
    public bool Use()
    {
        if (_isInteract)
        {
            Transform child = transform.GetChild(1);
            child.gameObject.SetActive(false);
            _isInteract = false;
            return true;
        }
        else
            return false;
    }
    public int GetValue()
    {
        return _healValue;
    }
}
