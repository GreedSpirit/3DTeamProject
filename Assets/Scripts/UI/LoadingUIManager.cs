using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingUIManager : MonoBehaviour
{
    private static LoadingUIManager _instance;

    [SerializeField] private CanvasGroup _loadingCanvas;
    [SerializeField] private Image _progressBar;

    private string _loadSceneName;

    public static LoadingUIManager Instance // 하나의 씬에서만 사용하는 것이 아닌 씬을 넘나들 때 사용할 수 있으며,
    {                                       // 여러 인스턴스를 관리할 필요가 없으므로 안정성을 향상시키고 범용적으로 사용하기 위해 싱글톤 패턴 사용
        get
        {
            if (_instance == null)
            {
                var obj = FindObjectOfType<LoadingUIManager>();

                if (obj != null)
                {
                    _instance = obj;
                }
                else
                {
                    _instance = Create();
                }
            }
            return _instance;
        }
    }

    private static LoadingUIManager Create()
    {
        return Instantiate(Resources.Load<LoadingUIManager>("LoadingUI"));
    }

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        gameObject.SetActive(true);
        SceneManager.sceneLoaded += OnSceneLoaded;
        _loadSceneName = sceneName;

        StartCoroutine(LoadSceneProgress());
    }

    private IEnumerator LoadSceneProgress()
    {
        _progressBar.fillAmount = 0.0f;

        yield return StartCoroutine(Fade(true));

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_loadSceneName);
        asyncOperation.allowSceneActivation = false;

        float timer = 0.0f;

        while (!asyncOperation.isDone)
        {
            yield return null;

            if (asyncOperation.progress < 0.8f)
            {
                _progressBar.fillAmount = asyncOperation.progress;
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                _progressBar.fillAmount = Mathf.Lerp(0.8f, 1.0f, timer);

                if(_progressBar.fillAmount >= 1.0f)
                {
                    asyncOperation.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }

    private IEnumerator Fade(bool isFadeIn)
    {
        float timer = 0.0f;

        while (timer <= 1.0f)
        {
            yield return null;
            timer += Time.unscaledDeltaTime * 3.0f;
            _loadingCanvas.alpha = isFadeIn ? Mathf.Lerp(0.0f, 1.0f, timer) : Mathf.Lerp(1.0f, 0.0f, timer);
        }

        if (!isFadeIn)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if(arg0.name == _loadSceneName)
        {
            StartCoroutine(Fade(false));

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
