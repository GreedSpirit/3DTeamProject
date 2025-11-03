using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealKit : InteractObject, IInteractable
{
    [SerializeField] private int _healValue = 10;
    public int GetValue()
    {
        return _healValue;
    }
}
