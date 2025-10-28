using UnityEngine;

public class UIPause : MonoBehaviour
{
    [SerializeField] private GameObject escBlackPanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (escBlackPanel.activeSelf)
            {
                Resume();
            }
            else
            {
                escBlackPanel.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }

    public void Resume()
    {
        Time.timeScale = 1;
        escBlackPanel.SetActive(false);
    }

    public void ReturnMainMenu()
    {
        Time.timeScale = 1;
        LoadingUIManager.Instance.LoadScene("Title");
    }
}
