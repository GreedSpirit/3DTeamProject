using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UISoundSetting : MonoBehaviour
{
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

    private void Awake()
    {
        _masterSlider.onValueChanged.AddListener(SetMasterVolume);
        _bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        _sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("TitleMasterVolume"))
        {
            _masterSlider.value = PlayerPrefs.GetFloat("TitleMasterVolume");
        }
        else
        {
            _masterSlider.value = 0.7f;
            PlayerPrefs.SetFloat("TitleMasterVolume", 0.7f);
        }
        _audioMixer.SetFloat("Master", Mathf.Log10(PlayerPrefs.GetFloat("TitleMasterVolume")) * 20);

        if (PlayerPrefs.HasKey("TitleBGMVolume"))
        {
            _bgmSlider.value = PlayerPrefs.GetFloat("TitleBGMVolume");
        }
        else
        {
            _bgmSlider.value = 0.7f;
            PlayerPrefs.SetFloat("TitleBGMVolume", 0.7f);
        }
        _audioMixer.SetFloat("BGM", Mathf.Log10(PlayerPrefs.GetFloat("TitleBGMVolume")) * 20);

        if (PlayerPrefs.HasKey("TitleSFXVolume"))
        {
            _sfxSlider.value = PlayerPrefs.GetFloat("TitleSFXVolume");
        }
        else
        {
            _sfxSlider.value = 0.7f;
            PlayerPrefs.SetFloat("TitleSFXVolume", 0.7f);
        }
        _audioMixer.SetFloat("SFX", Mathf.Log10(PlayerPrefs.GetFloat("TitleSFXVolume")) * 20);
    }

    public void SetMasterVolume(float volume)
    {
        _audioMixer.SetFloat("Master", Mathf.Log10(volume) * 20); // 비선형적인 볼륨을 갖고 있기에 이렇게 계산해야 함
        PlayerPrefs.SetFloat("TitleMasterVolume", volume);
    }
    public void SetBGMVolume(float volume)
    {
        _audioMixer.SetFloat("BGM", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("TitleBGMVolume", volume);
    }
    public void SetSFXVolume(float volume)
    {
        _audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("TitleSFXVolume", volume);
    }

}
