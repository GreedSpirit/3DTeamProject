using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndUI : MonoBehaviour
{
    void Update()
    {
        ReturnTitle();
        ReStart();
    }

    void ReStart()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            LoadingUIManager.Instance.LoadScene("InGame");
        }
    }
    
    void ReturnTitle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadingUIManager.Instance.LoadScene("Title");
        }
    }
}
