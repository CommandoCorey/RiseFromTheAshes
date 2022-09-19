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
    public void LoadSceneL1()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene("Ashley Test");
    }
    public void LoadSceneL2()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene("");
    }
    public void LoadSceneL3()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene("");
    }
    public void LoadSceneL4()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene("");
    }
    public void LoadSceneInsertHere1()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene("TE_1_Controller");
    }
    public void LoadSceneInsertHere2()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene("TE_2_Buildings");
    }
    public void LoadSceneInsertHere3()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene("3_Test Environment");
    }
    public void LoadSceneInsertHere4()
    {
        Debug.Log("Loading...");
        SceneManager.LoadScene("Building_Test");
    }
}
