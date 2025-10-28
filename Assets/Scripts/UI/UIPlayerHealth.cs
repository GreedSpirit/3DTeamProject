using TMPro;
using UnityEngine;

public class UIPlayerHealth : MonoBehaviour, IPlayerHealthObserver
{
    [SerializeField] private TextMeshProUGUI _playerHealth;
    [SerializeField] private PlayerController _player;

    private void Awake()
    {
        _player.AddHealthObserver(this);
    }

    private void OnDestroy()
    {
        _player.RemoveHealthObserver(this);        
    }
    public void OnPlayerHealthChanged(int curHp, int maxHp)
    {
        _playerHealth.text = "Health : " + curHp + " / " + maxHp;
    }
}
