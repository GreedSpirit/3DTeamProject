using System.Collections;
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

public class InteractObject : MonoBehaviour, IInteractable
{
    public bool _isInteract { get; set; } = true;
    public bool Use()
    {
        if (_isInteract)
        {
            Transform child = transform.GetChild(1);
            child.gameObject.SetActive(false);
            _isInteract = false;
            StartCoroutine(ResetInteractable(5f));
            return true;
        }
        else
            return false;

    }
    private IEnumerator ResetInteractable(float delay)
    {
        yield return new WaitForSeconds(delay);

        Transform child = transform.GetChild(1);
        child.gameObject.SetActive(true);
        _isInteract = true;
        gameObject.SetActive(false);

    }
}