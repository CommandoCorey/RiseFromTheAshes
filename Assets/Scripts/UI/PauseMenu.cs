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
    void Resume()
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
        SceneManager.LoadScene("MainMenu");
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
