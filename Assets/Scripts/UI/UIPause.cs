using UnityEngine;
using UnityEngine.SceneManagement;

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
        escBlackPanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void ReturnMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
