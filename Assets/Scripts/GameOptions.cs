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
    [SerializeField] Toggle fowTextureCheckbox;

    //[Space]
    [Header("Camera Settings")]
    [Header("GamePlay Options")]
    public Slider keyboardMoveSpeed;
    public Slider mousePanSpeed;
    public Slider zoomSpeed;
    public Toggle enableEdgeScrolling;
    public Slider edgeScrollSpeed;

    [Header("Unit Display Options")]
    public Toggle showIconstoggle;
    public Toggle showHealthbarsToggle;
    public Toggle showStatusTextToggle;
    public Toggle showDetectionRadiusToggle;
    public Toggle showAttackRangeToggle;

    // private variables
    private float dB;
    private Resolution[] resolutions;

    private GameManager gameManager;
    private new CameraController camera;

    // Singleton Instance
    public static GameOptions Instance { get; private set; }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
        if(Camera.main != null)
            camera = Camera.main.GetComponent<CameraController>();
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

        if (!PlayerPrefs.HasKey("FOWTexture"))
        {
            PlayerPrefs.SetInt("FOWTexture", 1);
        }

        fowTextureCheckbox.isOn = PlayerPrefs.GetInt("FOWTexture") != 0;
        fowTextureCheckbox.onValueChanged.AddListener(SetFogOfWarTexture);

        // read camera settings
        if (camera)
        {
            camera.ScaleKeyboardMoveSpeed(PlayerPrefs.GetFloat("cameraKeyboardMoveSpeed", 1));
            camera.ScaleMouseMoveSpeed(PlayerPrefs.GetFloat("cameraMousePanSpeed", 1));
            camera.ScaleCameraZoomSpeed(PlayerPrefs.GetFloat("cameraZoomSpeed", 1));
            int edgeScrollingOn = PlayerPrefs.GetInt("edgeScrolling", 1);
            camera.enableEdgeScrolling = (edgeScrollingOn == 1);
            PlayerPrefs.SetFloat("edgeScrollingSpeed", 1);
        }

        // read unit display options
        if(gameManager)
        {
            // update unit display options
            int value = PlayerPrefs.GetInt("ShowIcon", 1);
            gameManager.ShowIcons = (value == 1);
            value = PlayerPrefs.GetInt("ShowHealthBars", 1);
            gameManager.ShowHealthbars = (value == 1);
            value = PlayerPrefs.GetInt("ShowStatus", 0);
            gameManager.ShowStatusText = (value == 1);            
            value = PlayerPrefs.GetInt("ShowDetectionRadius", 1);
            gameManager.ShowDetectionRange = (value == 1);
            value = PlayerPrefs.GetInt("ShowAttackRange", 1);
            gameManager.ShowAttackRange = (value == 1);
        }

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
        int textureOn = PlayerPrefs.GetInt("FOWTexture", 1);                   
        useTexture.isOn = (textureOn == 1) ? true : false;

        // update game options
        // update camera options
        keyboardMoveSpeed.value = PlayerPrefs.GetFloat("cameraKeyboardMoveSpeed", 1);
        mousePanSpeed.value = PlayerPrefs.GetFloat("cameraMousePanSpeed", 1);
        zoomSpeed.value = PlayerPrefs.GetFloat("cameraZoomSpeed", 1);
        int edgeScrollingOn = PlayerPrefs.GetInt("edgeScrolling", 1);
        enableEdgeScrolling.isOn = (edgeScrollingOn == 1) ? true : false;
        edgeScrollSpeed.value = PlayerPrefs.GetFloat("edgeScrollingSpeed", 1);

        // update unit display options
        int value = PlayerPrefs.GetInt("ShowIcon", 1);
        showIconstoggle.isOn = (value == 1);
        value = PlayerPrefs.GetInt("ShowHealthBars", 1);
        showHealthbarsToggle.isOn = (value == 1);
        value = PlayerPrefs.GetInt("ShowStatus", 0);
        showStatusTextToggle.isOn = (value == 1);
        value = PlayerPrefs.GetInt("ShowDetectionRadius", 1);
        showDetectionRadiusToggle.isOn = (value == 1);
        value = PlayerPrefs.GetInt("ShowAttackRange", 1);
        showAttackRangeToggle.isOn = (value == 1);               
    }

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

    public static void SetFogOfWarTexture(bool val)
    {
        PlayerPrefs.SetInt("FOWTexture", val ? 1 : 0);
    }

    public void SetShadowQuality()
    {
        QualitySettings.shadows = (ShadowQuality) shadowQuality.value;
    }

    public void SetFowTexture()
    {
        SetFogOfWarTexture(useTexture.isOn);
    }
    #endregion

    #region camera options
    public void SetKeyboardMoveSpeed()
    {
        if(camera)        
            camera.ScaleKeyboardMoveSpeed(keyboardMoveSpeed.value);        
    }

    public void SetMiddleMousePan()
    {
        if (camera)
            camera.ScaleMouseMoveSpeed(mousePanSpeed.value);
    }

    public void SetZoomSpeed()
    {
        if (camera)
            camera.ScaleCameraZoomSpeed(zoomSpeed.value);
    }

    public void ToggleEdgeScrolling()
    {
        if (camera)
            camera.enableEdgeScrolling = enableEdgeScrolling.isOn;
    }

    public void SetEdgeScrollSpeed()
    {
        if(camera)
            camera.ScaleEdgeScrollSpeed(edgeScrollSpeed.value);
    }
    #endregion

    #region unit display options
    public void ToggleShowIcons()
    {
        if (gameManager)
            gameManager.ShowIcons = showIconstoggle.isOn;
        
    }

    public void ToggleShowHealthbars()
    {
        if (gameManager)        
            gameManager.ShowHealthbars = showHealthbarsToggle.isOn;
        
    }

    public void ToggleStatusText()
    {
        if (gameManager)
            gameManager.ShowStatusText = showStatusTextToggle.isOn;
        
    }

    public void ToggleDetectionRadius()
    {
        if (gameManager)
            gameManager.ShowDetectionRange = showDetectionRadiusToggle.isOn;
        
    }

    public void ToggleAttackRange()
    {
        if (gameManager)        
            gameManager.ShowAttackRange = showAttackRangeToggle.isOn;
        
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

        if (useTexture.isOn)
            PlayerPrefs.SetInt("FOWTexture", 1);
        else
            PlayerPrefs.SetInt("FOWTexture", 0);

        // save camera settings
        PlayerPrefs.SetFloat("cameraKeyboardMoveSpeed", keyboardMoveSpeed.value);
        PlayerPrefs.SetFloat("cameraMousePanSpeed", mousePanSpeed.value);
        PlayerPrefs.SetFloat("cameraZoomSpeed", zoomSpeed.value);
        int edgeScrollingOn =enableEdgeScrolling.isOn ? 1 : 0;
        PlayerPrefs.SetInt("edgeScrolling", edgeScrollingOn);
        PlayerPrefs.SetFloat("edgeScrollingSpeed", edgeScrollSpeed.value);

        // save unit display options
        int value = showIconstoggle.isOn ? 1 : 0;
        PlayerPrefs.SetInt("ShowIcon", value);
        value = showHealthbarsToggle.isOn ? 1 : 0;
        PlayerPrefs.SetInt("ShowHealthBars", value);
        value = showStatusTextToggle.isOn ? 1 : 0;
        PlayerPrefs.SetInt("ShowStatus", value);
        value = showDetectionRadiusToggle.isOn ? 1 : 0;
        PlayerPrefs.SetInt("ShowDetectionRadius", value);
        value = showAttackRangeToggle.isOn ? 1 : 0;
        PlayerPrefs.SetInt("ShowAttackRange", value);        
    }

}