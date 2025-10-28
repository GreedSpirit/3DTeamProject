using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IInteractable
{
    bool isInteract{ get; set;}
    bool Use();
}
public class BulletBox : MonoBehaviour, IInteractable
{
    public bool isInteract { get; set; } = true;
    public bool Use()
    {
        if (isInteract)
        {
            Transform child = transform.GetChild(1); 
            child.gameObject.SetActive(false);
            isInteract = false;
            return true;
        }
        else
            return false;
    }
}
