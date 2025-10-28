using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartBtn : MonoBehaviour
{
    public void TestGameStart()
    {
        SceneManager.LoadScene(1);
    }

    public void LoadVersusScene()
    {
        LoadingUIManager.Instance.LoadScene("InGameUI");
    }
}
