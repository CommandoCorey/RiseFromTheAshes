using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

public class GameOptions : MonoBehaviour
{
    // variable declaration
    //[Header("Tabs and panes")]
    //public Button startTab;
    //public GameObject audioOptions, videoOptions;

    //[Header("Audio Mixers")]
    [Header("Audio Settings")]
    [Space]
    public AudioMixer mixer;
    [Header("Volume Sliders")]
    public Slider masterSlider;
    public Slider soundFxSlider, musicSlider, ambienceSlider;
    [Header("Default Volumes")]
    [Range(0, 1)]
    public float defaultMasterVolume = 0.75f;
    [Range(0, 1)]
    public float defaultMusicVolume = 0.5f;
    [Range(0, 1)]
    public float defaultSoundFXVolume = 0.75f;
    [Range(0, 1)]
    public float defaultAmbienceVolume = 0.75f;

    [Space]

    [Header("Video Settings")]
    public TMP_Dropdown displayMode;
    public TMP_Dropdown screenResolution;

    [Header("Graphic Settings")]
    public TMP_Dropdown graphicsQuality;
    public TMP_Dropdown vSyncCount;
    public TMP_Dropdown antiAliasing;
    public TMP_Dropdown shadowQuality;
    public Toggle useTexture;

    //[Space]
    [Header("Camera Settings")]
    [Header("GamePlay Options")]        
    public Slider sensetivitySlider;

    // private variables
    private float dB;
    private float cameraSensetivity = 1;
    private Resolution[] resolutions;

    // Start is called before the first frame update
    void Start()
    {
        resolutions = Screen.resolutions;

        // Set audio volumes
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            dB = Mathf.Log10(PlayerPrefs.GetFloat("MasterVolume")) * 20;
            mixer.SetFloat("Master", dB);
            dB = Mathf.Log10(PlayerPrefs.GetFloat("MusicVolume")) * 20;
            mixer.SetFloat("Music", dB);
            dB = Mathf.Log10(PlayerPrefs.GetFloat("SoundEffectsVolume")) * 20;
            mixer.SetFloat("SoundEffects", dB);
            dB = Mathf.Log10(PlayerPrefs.GetFloat("AmbienceVolume")) * 20;
            mixer.SetFloat("Ambience", dB);
        }
        else
        {
            dB = Mathf.Log10(defaultMasterVolume) * 20;
            mixer.SetFloat("Master", dB);
            dB = Mathf.Log10(defaultMusicVolume) * 20;
            mixer.SetFloat("Music", dB);
            dB = Mathf.Log10(defaultSoundFXVolume) * 20;
            mixer.SetFloat("SoundEffects", dB);
            dB = Mathf.Log10(defaultAmbienceVolume) * 20;
            mixer.SetFloat("Ambience", dB);
        }

        // set video settings
        Screen.fullScreenMode = (FullScreenMode) PlayerPrefs.GetInt("ScreenMode", 0);

        // set screen resolution
        int width = PlayerPrefs.GetInt("ScreenWidth", Screen.currentResolution.width);
        int height = PlayerPrefs.GetInt("ScreenHeight", Screen.currentResolution.height);

        foreach (Resolution res in resolutions)
        {
            if (res.width == width && res.height == height)
            {
                Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
                break;
            }

        }

        // Set graphics settings
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Quality", 2), true);
        QualitySettings.vSyncCount = PlayerPrefs.GetInt("VSync", 1);

        int selectedOption = PlayerPrefs.GetInt("MSAA");

        switch (selectedOption)
        {
            case 0: QualitySettings.antiAliasing = 0;
                break;

            case 1: QualitySettings.antiAliasing = 2;
                break;

            case 2: QualitySettings.antiAliasing = 4;
                break;

            case 3: QualitySettings.antiAliasing = 8;
                break;
        }
        
        QualitySettings.shadows = (ShadowQuality) PlayerPrefs.GetInt("Shadows", 2);

        // populate the screen resolutions dropdown  
        List<string> resolutionText = new List<string>();
        foreach (var resolution in resolutions)
        {
            //resolutionText.Add(resolution.width + " x " + resolution.height);
            //Debug.Log(resolution.ToString());
            screenResolution.options.Add(new OptionData(resolution.width + " x " + resolution.height));
        }
        //screenResolution.AddOptions(resolutionText);

    }

    public void InitGUI()
    {
        // set default selected tab
        //startTab.Select();
        //startTab.onClick.Invoke();

        // set volume sliders
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
            soundFxSlider.value = PlayerPrefs.GetFloat("SoundEffectsVolume", defaultSoundFXVolume);
            ambienceSlider.value = PlayerPrefs.GetFloat("AmbienceVolume", defaultAmbienceVolume);
        }
        else
        {
            masterSlider.value = defaultMasterVolume;
            musicSlider.value = defaultMusicVolume;
            soundFxSlider.value = defaultSoundFXVolume;
            ambienceSlider.value = defaultAmbienceVolume;
        }

        // set camera sensetivity slider
        //sensetivitySlider.value = PlayerPrefs.GetFloat("CameraSensetivity", 1);

        // set drop down boxes
        displayMode.SetValueWithoutNotify(PlayerPrefs.GetInt("ScreenMode", 0));
        //screenResolution.SetValueWithoutNotify(Array.IndexOf(resolutions, Screen.currentResolution));

        // set the seleted screen resolution        
        int width = PlayerPrefs.GetInt("ScreenWidth", Screen.currentResolution.width);
        int height = PlayerPrefs.GetInt("ScreenHeight", Screen.currentResolution.height);
        try
        {
            foreach (Resolution res in resolutions)
            {
                if (res.width == width && res.height == height)
                {
                    screenResolution.SetValueWithoutNotify(Array.IndexOf(resolutions, res));
                    break;
                }

            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        // update graphics tab
        graphicsQuality.SetValueWithoutNotify(PlayerPrefs.GetInt("Quality", 2));
        vSyncCount.SetValueWithoutNotify(PlayerPrefs.GetInt("VSync", 1));
        antiAliasing.SetValueWithoutNotify(PlayerPrefs.GetInt("MSAA", 2));
        shadowQuality.SetValueWithoutNotify(PlayerPrefs.GetInt("Shadows", 2));
    }

    /*
    #region tab controls    
    public void OpenAudioOptions()
    {
        audioOptions.SetActive(true);
        videoOptions.SetActive(false);
    }

    public void OpenVideoOptions()
    {
        audioOptions.SetActive(false);
        videoOptions.SetActive(true);
    }
    #endregion*/

    #region volume Controls
    public void SetMasterVolume()
    {
        float dB = Mathf.Log10(masterSlider.value) * 20;
        mixer.SetFloat("Master", dB);        
    }

    public void SetMusicVolume()
    {
        float dB = Mathf.Log10(musicSlider.value) * 20;
        mixer.SetFloat("Music", dB);        
    }

    public void SetSoundVolume()
    {
        float dB = Mathf.Log10(soundFxSlider.value) * 20;
        mixer.SetFloat("SoundEffects", dB);        
    }

    public void SetAmbienceVolume()
    {
        float dB = Mathf.Log10(ambienceSlider.value) * 20;
        mixer.SetFloat("Ambience", dB);
        PlayerPrefs.SetFloat("AmbienceVolume", ambienceSlider.value);
    }
    #endregion

    #region video options
    public void SetDisplayMode()
    {
        Screen.fullScreenMode = (FullScreenMode)displayMode.value;
    }

    public void SetScreenResolution() 
    {
        Screen.SetResolution(resolutions[screenResolution.value].width,
        resolutions[screenResolution.value].height, (FullScreenMode)displayMode.value);
    }

    #endregion

    #region graphics options
    public void SetGraphicsQuality()
    {
        QualitySettings.SetQualityLevel(graphicsQuality.value, true);
    }

    public void SetVSyncCount()
    {
        QualitySettings.vSyncCount = vSyncCount.value;
    }

    public void SetAntiAliasing()
    {
        switch (antiAliasing.value)
        {
            case 0:
                QualitySettings.antiAliasing = 0;
                break;

            case 1:
                QualitySettings.antiAliasing = 2;
                break;

            case 2:
                QualitySettings.antiAliasing = 4;
                break;

            case 3:
                QualitySettings.antiAliasing = 8;
                break;
        }
    }

    public void SetShadowQuality()
    {
        QualitySettings.shadows = (ShadowQuality) shadowQuality.value;
    }

    public void SetFowTexture()
    {
        //useTexture.isOn;
    }
    #endregion

    /*
    public void ToggleMotionBlur(bool inMainScene)
    {
        if(inMainScene && game != null)
            game.MotionBlur = motionBlur.isOn;
    }*/

    #region controls options
    public void SetCameraSensetivity(bool inMainScene)
    {
        cameraSensetivity = sensetivitySlider.value;

        if (inMainScene)
        {
            //PlayerMovement player = GameObject.Find("Player").GetComponent<PlayerMovement>();
            //player.CameraSensetivty = cameraSensetivity;

            // TODO: Replace with camera controller script
        }
    }
    #endregion

    public void SaveSettings()
    {
        // Save volume settings
        PlayerPrefs.SetFloat("MasterVolume", masterSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("SoundEffectsVolume", soundFxSlider.value);

        // save video settings
        PlayerPrefs.SetInt("ScreenMode", displayMode.value);
        PlayerPrefs.SetInt("ScreenWidth", resolutions[screenResolution.value].width);
        PlayerPrefs.SetInt("ScreenHeight", resolutions[screenResolution.value].height);

        // save graphics settings
        PlayerPrefs.SetInt("Quality", graphicsQuality.value);
        PlayerPrefs.SetInt("VSync", vSyncCount.value);
        PlayerPrefs.SetInt("MSAA", antiAliasing.value);
        PlayerPrefs.SetInt("Shadows", shadowQuality.value);

        // save controls settings
        //PlayerPrefs.SetFloat("CameraSensetivity", cameraSensetivity);        
    }

}