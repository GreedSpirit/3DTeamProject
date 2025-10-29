using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    bool _isInteract { get; set; }
    public virtual int GetValue()
    {
        return 0;
    }
    bool Use();
}
public class BulletBox : MonoBehaviour, IInteractable
{
    public bool _isInteract { get; set; } = true;
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
}
