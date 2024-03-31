using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResolutionControl : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;
    private float currentRefreshRate;
    private int currentResolutionIndex = 0;
    // Start is called before the first frame update
    void Start() 
    {
        resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();

        resolutionDropdown.ClearOptions();
        currentRefreshRate = (float)Screen.currentResolution.refreshRateRatio.value;

        for (int i = 0; i < resolutions.Length; i++) {
            if ((float)resolutions[i].refreshRateRatio.value == currentRefreshRate) {
                filteredResolutions.Add(resolutions[i]);
            }
        }

        List<string> options = new List<string>();
        for (int i = 0; i < filteredResolutions.Count; i++) {
            string resolutionOption = filteredResolutions[i].width + "x" + filteredResolutions[i].height + " " + (float)filteredResolutions[i].refreshRateRatio.value + " Hz";
            options.Add(resolutionOption);

            if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height) {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    // sets the resolution for the screen
    public void SetResolution(int resolutionIndex) {
        Resolution resolution = filteredResolutions[resolutionIndex];
        bool full;

        // checks if the screen is fullscreen or windowed
        if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen || Screen.fullScreenMode == FullScreenMode.FullScreenWindow) {
            full = true;
        } else {
            full = false;
        }

        // if full is true, we stay fullscreen. else we stay windowed
        Screen.SetResolution(resolution.width, resolution.height, full);
    }

}
