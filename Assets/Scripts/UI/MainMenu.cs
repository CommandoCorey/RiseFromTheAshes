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

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    public void LoadScene1()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene(1);
    }
    public void LoadScene2()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene(2);
    }
    public void LoadScene3()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene(3);
    }
}
