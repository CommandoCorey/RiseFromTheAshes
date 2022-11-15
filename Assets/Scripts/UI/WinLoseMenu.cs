using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLoseMenu : MonoBehaviour
{
    public void LoadScene0()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene(0);
    }
}
