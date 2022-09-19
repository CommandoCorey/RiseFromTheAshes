using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
    public void LoadSceneTestEnvironment()
    {
        Debug.Log("Loading TestEnvironment...");
        SceneManager.LoadScene("TestEnvironment");
    }
    public void LoadSceneLevel01()
    {
        Debug.Log("Loading Level01...");
        SceneManager.LoadScene("Level01");
    }
    public void LoadSceneLevel02()
    {
        Debug.Log("Loading Level02...");
        SceneManager.LoadScene("Level02");
    }
    public void LoadSceneLevel03()
    {
        Debug.Log("Loading Level03...");
        SceneManager.LoadScene("Level03");
    }
}
