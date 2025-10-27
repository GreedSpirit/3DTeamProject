using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionContorller : MonoBehaviour
{
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private Resolution[] resolutions;

    private void Start()
    {
        SetDropdown();
        SetUpToggle();
    }

    void GetAllResolutions()
    {
        resolutions = Screen.resolutions;
    }

    void SetDropdown()
    {
        resolutionDropdown.ClearOptions();
        GetAllResolutions();


        HashSet<string> options = new HashSet<string>();
        resolutionDropdown.onValueChanged.AddListener(delegate { SetResolution(); });

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " X " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(new List<string>(options));
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        SetResolution();
    }

    public void SetResolution()
    {
        int resolutionIndex = resolutionDropdown.value;
        Resolution resolution = resolutions[resolutionIndex];

        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        Debug.Log(Screen.width + " X " + Screen.height);
    }

    private void SetUpToggle()
    {
        fullScreenToggle.onValueChanged.AddListener(delegate { ToggleChanged(fullScreenToggle); });

        if (Screen.fullScreen)
        {
            fullScreenToggle.isOn = true;
        }
        else
        {
            fullScreenToggle.isOn = false;
        }
    }

    private void ToggleChanged(Toggle changedToggle)
    {

        if (fullScreenToggle.isOn)
        {

            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = true;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.fullScreen = false;
        }
    }    

}
