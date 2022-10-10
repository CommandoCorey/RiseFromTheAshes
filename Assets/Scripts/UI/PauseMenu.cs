using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }
    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
    public void LoadSceneMainMenu()
    {
        Debug.Log("Loading MainMenu...");
        SceneManager.LoadScene("Main Menu (Beta)");
    }
    public void LoadSceneTestEnvironment()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene("");
    }
    public void LoadSceneLevel01()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene("");
    }
    public void LoadSceneLevel02()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene("");
    }
    public void LoadSceneLevel03()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene("");
    }
    public void LoadSceneInsertHere1()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene("");
    }
    public void LoadSceneInsertHere2()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene("");
    }
    public void LoadSceneInsertHere3()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene("");
    }
    public void LoadSceneInsertHere4()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene("");
    }
}
