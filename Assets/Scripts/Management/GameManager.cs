using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

public enum GameState
{
    Running, Paused, Win, Lose
}

public class GameManager : MonoBehaviour
{
    [SerializeField] int startingMaxUnits = 20;

    [Range(0, 10)]
    public float timeScale = 1;
    public GameObject minimap;
    public bool showMinimap = false;
    public Transform marker;

    [Header("Headquater Buildings")]
    public Building playerHQ;
    public Building enemyHQ;

    [Header("Dialogs")]
    public GameObject pauseDialog;
    public GameObject winDialog;
    public GameObject loseDialog;

    [Header("Heads Up Display")]
    public TextMeshProUGUI totalUnitsText;
    public TextMeshProUGUI maxUnitsText;

    [Header("Cursors")]
    public bool enableCursorChanges;
    public CursorSprite defaultCursor;
    public CursorSprite moveCursor;
    public CursorSprite attackCursor;

    [Header("Keyboard Shortcuts")]
    public KeyCode pauseKey;

    private GameState state;
    private new AudioSource audio;

    private bool handleEndConidition = false;

    // private variables
    private int maxUnitsPlayer;
    private int maxUnitsAi;
    private int unitCountPlayer = 0;
    private int unitCountAi = 0;

    static GameManager gameManager;

    // properties
    static public GameManager Instance { get; private set; }
    public GameState State { get => state; }
    public int MaxUnitsPlayer { get => maxUnitsPlayer; }
    public int MaxUnitsAi { get => maxUnitsAi; }
    public int UnitCountPlayer { get => unitCountPlayer; }
    public int UnitCountAi { get => unitCountAi; }

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
        if (showMinimap)
            minimap.SetActive(true);

        marker.GetComponent<MeshRenderer>().enabled = false;
        audio = GetComponent<AudioSource>();

        // set cursor sizes
        //defaultCursor.Resize(32, 32);

        if(enableCursorChanges)
            Cursor.SetCursor(defaultCursor.image, defaultCursor.hotspot, CursorMode.ForceSoftware);

        if (playerHQ != null && enemyHQ != null)
            handleEndConidition = true;

        state = GameState.Running;

        maxUnitsPlayer = startingMaxUnits;
        maxUnitsAi = startingMaxUnits;

        maxUnitsText.text = maxUnitsPlayer.ToString();
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
    }

    #region public functions
    public void ChangeGameState(GameState newState)
    {
        state = newState;

        Time.timeScale = 1;

        switch (state)
        {
            case GameState.Running:
                Time.timeScale = 1;
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

    /// <summary>
    /// Moves the marker gameobject to a specified location
    /// </summary>
    /// <param name="position">The position on the map that the user clicked on</param>
    public void SetMarkerLocation(Vector3 position)
    {
        marker.transform.position = position;
        marker.GetComponent<MeshRenderer>().enabled = true;
    }

    public bool IsLayerInMask(int layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << layer));
    }

    public void PlaySound(AudioClip clip, float volumeScale)
    {
        audio.PlayOneShot(clip, volumeScale);
    }

    public void InstantiateParticles(ParticleSystem prefab, Vector3 position)
    {
        var particles = Instantiate(prefab.gameObject, position, Quaternion.identity);
    }

    public void SetCursor(CursorSprite sprite)
    {
        if(enableCursorChanges)
            Cursor.SetCursor(sprite.image, sprite.hotspot, CursorMode.ForceSoftware);
    }
    public void ResetCursor()
    {
        if (enableCursorChanges)
            Cursor.SetCursor(defaultCursor.image, defaultCursor.hotspot, CursorMode.ForceSoftware);
    }
    #endregion
    public void TogglePause(bool paused)
    {
        pauseDialog.SetActive(paused);

        // disable all other scripts on the game manageer
        GetComponent<UnitManager>().enabled = !paused;
        GetComponent<SelectionManager>().enabled = !paused;
        GetComponent<ResourceManager>().enabled = !paused;
        GetComponent<BuildingManager>().enabled = !paused;
        //GetComponent<AiManager>().enabled = !paused;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(0);
    }
    
    /// <summary>
    /// Add's one unit to the unit count
    /// </summary>
    /// <param name="aiPlayer"> </param>
    public void IncreaseUnitCount(bool aiPlayer)
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
    /// <param name="aiPlayer"> </param>
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
    /// <param name="amount"></param>
    /// <param name="aiPlayer"></param>
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

}

[System.Serializable]
public struct CursorSprite
{
    public Texture2D image;
    public Vector2 hotspot;
}

[System.Serializable]
public class SoundEffect
{
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volumeScale = 1;
}