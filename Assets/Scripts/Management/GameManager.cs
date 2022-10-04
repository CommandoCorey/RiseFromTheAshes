using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public enum GameState
{
    Running, Paused, Win, Lose
}

public class GameManager : MonoBehaviour
{
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

    // properties
    public GameState State { get => state;}

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

            else if (Input.GetKeyUp(pauseKey))
                ChangeGameState(GameState.Paused);
        }
        else if (state == GameState.Paused && Input.GetKeyUp(pauseKey))
        {
            ChangeGameState(GameState.Running);
        }
    }

    #region public functions
    public void ChangeGameState(GameState newState)
    {
        state = newState;

        if (state != GameState.Running)
            Time.timeScale = 0;
        // end if

        switch (state)
        {
            case GameState.Running:
                TogglePause(false);
                Time.timeScale = 1;
                break;

            case GameState.Paused:
                TogglePause(true);                
                break;

            case GameState.Win:
                winDialog.SetActive(true);
                break;

            case GameState.Lose: 
               // loseDialog.SetActive(true);
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

    #region private functions
    private void TogglePause(bool paused)
    {
        // disable all other scripts on the game manageer
        GetComponent<UnitManager>().enabled = !paused;
        GetComponent<SelectionManager>().enabled = !paused;
        GetComponent<ResourceManager>().enabled = !paused;
        GetComponent<BuildingManager>().enabled = !paused;
        GetComponent<AiManager>().enabled = !paused;

        pauseDialog.SetActive(paused);
    }
    #endregion
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