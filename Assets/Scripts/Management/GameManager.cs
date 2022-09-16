using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class GameManager : MonoBehaviour
{
    public GameObject minimap;
    public bool showMinimap = false;
    public Transform marker;

    [Header("Cursors")]
    public bool enableCursorChanges;
    public CursorSprite defaultCursor;
    public CursorSprite moveCursor;
    public CursorSprite attackCursor;

    private AudioSource audio;

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
    }

    // Update is called once per frame
    void Update()
    {
        
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

}

[System.Serializable]
public struct CursorSprite
{
    public Texture2D image;
    public Vector2 hotspot;
}

/*
public enum CursorMode
{
    Normal, Move, Attack
}*/