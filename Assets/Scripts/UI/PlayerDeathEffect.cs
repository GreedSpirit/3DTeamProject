using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerDeathEffect : MonoBehaviour
{
    [SerializeField] private GameObject _fadePanel;
    [SerializeField] private PlayerController _player;

    [SerializeField] private float _fadeSpeed;

    void Awake()
    {
        _player.OnDeath += Fade;
    }

    void OnDestroy()
    {
        _player.OnDeath -= Fade;
    }
    public void Fade()
    {
        StartCoroutine(FadeOutStart());
    }


    public IEnumerator FadeOutStart()
    {
        for (float f = 0f; f < 1; f += 0.0005f)
        {
            Color c = _fadePanel.GetComponent<Image>().color;
            c.a = f;
            _fadePanel.GetComponent<Image>().color = c;
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        LoadingUIManager.Instance.LoadScene("GameOverUI");
    }

}
