using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;
//using static UnityEditor.Experimental.GraphView.GraphView;

public enum GameState
{
    Running, Paused, Win, Lose
}

public class GameManager : MonoBehaviour
{
    [SerializeField] int startingMaxUnits = 20;

    [Range(0, 10)]
    public float timeScale = 1;
    public Transform marker;

    [Header("Headquater Buildings")]
    public Building playerHQ;
    public Building enemyHQ;

    [Header("Dialogs")]
    public GameObject pauseDialog;
    public GameObject GUI;
    public GameObject optionsDialog;
    public GameObject winDialog;
    public GameObject loseDialog;

    [Header("Heads Up Display")]
    public TextMeshProUGUI totalUnitsText;
    public TextMeshProUGUI maxUnitsText;
    public TextMeshProUGUI difficultyText;

    [Header("Cursors")]
    public bool enableCursorChanges;
    public CursorSprite defaultCursor;
    public CursorSprite selectableCursor;
    public CursorSprite moveCursor;
    public CursorSprite attackCursor;

    private CursorSprite currentCursor;

    [Header("Unit Display Options")]
    [SerializeField] bool showIcons = true;
    [SerializeField] bool showHealthbars = true;
    [SerializeField] bool showStatusText = true;
    [SerializeField] bool showDetectionRadius = true;
    [SerializeField] bool showAttackRange = true;

    [Header("Keyboard Shortcuts")]
    public KeyCode pauseKey;
    public KeyCode unitHealthbarKey;
    public KeyCode unitIconsKey;
    public KeyCode unitStatusTextKey;
    public KeyCode detectionRangeKey;
    public KeyCode attackRangeKey;    

    [Header("Particle systems")]
    public GameObject destroyPropEffect;

    //[Header("UI Sound Effects")]
    //public AudioClip

    private GameState state;
    private new AudioSource audio;

    private bool handleEndConidition = false;

    // private variables
    private int maxUnitsPlayer;
    private int maxUnitsAi;
    private int unitCountPlayer = 0;
    private int unitCountAi = 0;

    private GameOptions gameOptions;

    static GameManager gameManager;

    // properties
    static public GameManager Instance { get; private set; }
    public GameState State { get => state; }
    public int MaxUnitsPlayer { get => maxUnitsPlayer; }
    public int MaxUnitsAi { get => maxUnitsAi; }
    public int UnitCountPlayer { get => unitCountPlayer; }
    public int UnitCountAi { get => unitCountAi; }

    public bool ShowIcons { get => showIcons; set => showIcons = value; }
    public bool ShowHealthbars { get => showHealthbars; set => showHealthbars = value; }
    public bool ShowStatusText { get => showStatusText; set => showStatusText = value; }
    public bool ShowDetectionRange { get => showDetectionRadius; set => showDetectionRadius = value; }
    public bool ShowAttackRange { get => showAttackRange; set => showAttackRange = value; }

    private void Awake()
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
        marker.GetComponent<MeshRenderer>().enabled = false;
        audio = GetComponent<AudioSource>();
        gameOptions = GameOptions.Instance;

        // set cursor sizes
        //defaultCursor.Resize(32, 32);

        if (enableCursorChanges)        
            SetCursor(defaultCursor);        

        if (playerHQ != null && enemyHQ != null)
            handleEndConidition = true;

        state = GameState.Running;

        maxUnitsPlayer = startingMaxUnits;
        maxUnitsAi = startingMaxUnits;

        if(maxUnitsText)
            maxUnitsText.text = maxUnitsPlayer.ToString();

        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        if(state == GameState.Running)
            Time.timeScale = timeScale;

        if(state == GameState.Running)
        {
            if (handleEndConidition && (playerHQ == null || playerHQ.HP <= 0))
                ChangeGameState(GameState.Lose);
            else if (handleEndConidition && (enemyHQ == null || enemyHQ.HP <= 0))
                ChangeGameState(GameState.Win);

            if (Input.GetKeyDown(pauseKey))
                ChangeGameState(GameState.Paused);
        }
        else if (state == GameState.Paused && Input.GetKeyDown(pauseKey))
        {
            ChangeGameState(GameState.Running);
        }

        if (enableCursorChanges && currentCursor == selectableCursor &&
            PointerOverUI())
            SetCursor(defaultCursor);

        HandleKeyboardShortcuts();
    }

    private void HandleKeyboardShortcuts()
    {
        if (Input.GetKeyDown(unitHealthbarKey))
        { 
            showHealthbars = !showHealthbars;
            gameOptions.showHealthbarsToggle.isOn = showHealthbars;
        }

        if (Input.GetKeyDown(unitIconsKey))
        {
            showIcons = !showIcons;
            gameOptions.showIconstoggle.isOn = showIcons;
        }

        if (Input.GetKeyDown(unitStatusTextKey))
        {
            showStatusText = !showStatusText;
            gameOptions.showStatusTextToggle.isOn = showStatusText;
        }

        if (Input.GetKeyDown(detectionRangeKey))
        {
            showAttackRange = !showAttackRange;
            gameOptions.showDetectionRadiusToggle.isOn = showDetectionRadius;
        }

        if (Input.GetKeyDown(attackRangeKey))
        {
            showDetectionRadius = !showDetectionRadius;
            gameOptions.showAttackRangeToggle.isOn = showAttackRange;
        }
    }

    #region public functions
    public void ChangeGameState(GameState newState)
    {
        state = newState;        

        switch (state)
        {
            case GameState.Running:
                Time.timeScale = timeScale;
                TogglePause(false);
                break;

            case GameState.Paused:
                Time.timeScale = 0;
                TogglePause(true);
                break;

            case GameState.Win:
                Time.timeScale = 0;
                winDialog.SetActive(true);
                break;

            case GameState.Lose:
                Time.timeScale = 0;
                loseDialog.SetActive(true);
               break;
        }
    }

    public void ResumeGame()
    {
        ChangeGameState(GameState.Running);        
    }

    public void SetDifficultyText(string text)
    {
        difficultyText.text = text;
    }

    /// <summary>
    /// Determines whether or not the cursor if over a an object on the UI layer
    /// </summary>
    /// <returns>True touched or hovered on Unity UI element, false if it didn't</returns>
    public bool PointerOverUI()
    {
        var eventSystemRaysastResults = GetEventSystemRaycastResults();

        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == 5) // UI Layer
                return true;
        }
        return false;
    }

    /// <summary>
    /// Moves the marker gameobject to a specified location
    /// </summary>
    /// <param name="position">The position on the map that the user clicked on</param>
    public void SetMarkerLocation(Vector3 position)
    {
        marker.transform.position = position;
        marker.GetComponent<MeshRenderer>().enabled = true;
    }

    /// <summary>
    /// Checks it a layer number is within a layer mask
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="layerMask"></param>
    /// <returns></returns>
    public bool IsLayerInMask(int layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << layer));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="volumeScale"></param>
    public void PlaySound(AudioClip clip, float volumeScale)
    {
        audio.PlayOneShot(clip, volumeScale);
    }

    public void PlaySound(AudioClip clip)
    {
        audio.PlayOneShot(clip);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position"></param>
    public void InstantiateParticles(ParticleSystem prefab, Vector3 position)
    {
        var particles = Instantiate(prefab.gameObject, position, Quaternion.identity);
        Destroy(particles, 5.0f);
    }

    public void InstantiateParticles(VisualEffect prefab, Vector3 position)
    {
        var particles = Instantiate(prefab.gameObject, position, Quaternion.identity);
        Destroy(particles, 5.0f);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sprite"></param>
    public void SetCursor(CursorSprite sprite)
    {
        if (enableCursorChanges)
            Cursor.SetCursor(sprite.image, sprite.hotspot, CursorMode.Auto);

        currentCursor = sprite;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetCursor()
    {
        if (enableCursorChanges)
            Cursor.SetCursor(defaultCursor.image, defaultCursor.hotspot, CursorMode.Auto);
    }
    
    public void TogglePause(bool paused)
    {
        pauseDialog.SetActive(paused);
        GUI.SetActive(!paused);

        optionsDialog.SetActive(false);        

        // disable all other scripts on the game manageer
        GetComponent<UnitManager>().enabled = !paused;
        GetComponent<SelectionManager>().enabled = !paused;
        GetComponent<ResourceManager>().enabled = !paused;
        GetComponent<BuildingManager>().enabled = !paused;

        /* NOTE (George): I don't know if this is still required. It
         * appears to not be, but I have no idea. */
        //GetComponent<AiManager>().enabled = !paused;
    }

    /// <summary>
    /// Returns the game to the main menu scene
    /// </summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(0);
    }
    
    /// <summary>
    /// Increase a player's unit count
    /// </summary>
    /// <param name="amount">the amount of unit points to be taken up</param>
    /// <param name="aiPlayer">determines whether the</param>
    public void IncreaseUnitCount(int amount, bool aiPlayer)
    {
        if (aiPlayer)
        {
            unitCountAi++;
        }
        else
        {
            unitCountPlayer++;
            totalUnitsText.text = unitCountPlayer.ToString();
        }
        
    }

    /// <summary>
    /// Removes one unit to the unit count
    /// </summary>
    /// <param name="aiPlayer">Determines if the unit is an ai player unit</param>
    public void DecreaseUnitCount(bool aiPlayer)
    {
        if (aiPlayer)
        {
            unitCountAi--;
        }
        else
        {
            unitCountPlayer--;
            totalUnitsText.text = unitCountPlayer.ToString();
        }

    }

    /// <summary>
    /// Increases the maximum units by a specified amount
    /// </summary>
    /// <param name="amount">The increase in unit capacity</param>
    /// <param name="aiPlayer">determines whether or not the player is the human or the A.I.</param>
    public void IncreaseMaxUnits(int amount, bool aiPlayer)
    {
        if (aiPlayer)
        {
            maxUnitsAi += amount;
        }
        else
        {
            maxUnitsPlayer += amount;
            maxUnitsText.text = maxUnitsPlayer.ToString();
        }
    }
    public void DecreaseMaxUnits(int amount, bool aiPlayer)
    {
        if (aiPlayer)
        {
            maxUnitsAi -= amount;
        } else
        {
            maxUnitsPlayer -= amount;
            maxUnitsText.text = maxUnitsPlayer.ToString();
        }
    }
    #endregion

    #region private functions
    //Gets all event system raycast results of current mouse or touch position.
    private List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
    #endregion

}

[System.Serializable]
public struct CursorSprite
{
    public Texture2D image;
    public Vector2 hotspot;

    public override bool Equals(object otherObj)
    {
        CursorSprite other = (CursorSprite)otherObj;

        return image == other.image && hotspot == other.hotspot;
    }

    public override int GetHashCode()
    {
        return image.GetHashCode() ^ hotspot.GetHashCode();
    }

    public static bool operator ==(CursorSprite a, CursorSprite b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(CursorSprite a, CursorSprite b)
    {
        return !a.Equals(b);
    }
}

[System.Serializable]
public class SoundEffect
{
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volumeScale = 1;
}