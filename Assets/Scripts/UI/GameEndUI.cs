using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndUI : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        ReturnTitle();
        ReStart();
    }

    void ReStart()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(1);
        }
    }
    
    void ReturnTitle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }
    }
}
