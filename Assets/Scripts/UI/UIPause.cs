using UnityEngine;

public class UIPause : MonoBehaviour
{
    [SerializeField] private GameObject _escBlackPanel;
    [SerializeField] private GameObject _crossHair;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_escBlackPanel.activeSelf)
            {
                Resume();
            }
            else
            {
                _escBlackPanel.SetActive(true);
                _crossHair.SetActive(false);
                Time.timeScale = 0;
            }
        }
    }

    public void Resume()
    {
        Time.timeScale = 1;
        _escBlackPanel.SetActive(false);
        _crossHair.SetActive(true);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ReturnMainMenu()
    {
        Time.timeScale = 1;
        LoadingUIManager.Instance.LoadScene("Title");
    }
}
