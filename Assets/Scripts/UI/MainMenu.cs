using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject credits;
    [SerializeField] GameObject loadingScreen;
    [SerializeField] GameObject gameOptions;
    [SerializeField] [Range(1, 100)]
    float creditsRiseSpeed = 100.0f;
    [SerializeField] RectTransform creditsMover;
    [SerializeField] TextMeshProUGUI loadPercent;
    [SerializeField] ProgressBar loadProgressBar;

    Vector3 creditsMoverOriginalPos;    

    public void Awake()
	{
		creditsMoverOriginalPos = creditsMover.position;
	}

    public void Update()
    {
        creditsMover.position += Vector3.up * Time.deltaTime * creditsRiseSpeed;

        
    }

    private IEnumerator LoadLavel(int scene)
    {
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(scene);

        while (!sceneLoad.isDone)
        {
            float progress = Mathf.Clamp01(sceneLoad.progress / .9f);

            loadPercent.text = (sceneLoad.progress * 100) + " %";

            if (loadProgressBar)
            {
                loadProgressBar.maxValue = 100;
                loadProgressBar.progress = progress;
            }
            //Debug.Log(sceneLoad.progress);

            yield return null;
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void StartGame(int index)
    {
        mainMenu.SetActive(false);
        credits.SetActive(false);
        loadingScreen.SetActive(true);

        StartCoroutine(LoadLavel(index));
    }

	private void Start()
	{
        ShowMainMenu();
    }

	public void PlayCredits()
	{
        creditsMover.position = creditsMoverOriginalPos;
        mainMenu.SetActive(false);
        credits.SetActive(true);
        loadingScreen.SetActive(false);
    }

    public void ShowMainMenu()
    {
        creditsMover.position = creditsMoverOriginalPos;
        mainMenu.SetActive(true);
        credits.SetActive(false);
        loadingScreen.SetActive(false);
        gameOptions.SetActive(false);
    }

    public void OpenGameOptions()
    {
        mainMenu.SetActive(false);
        gameOptions.SetActive(true);
    }

    public static Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        Rect rect = new Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
        rect.x -= (transform.pivot.x * size.x);
        rect.y -= ((1.0f - transform.pivot.y) * size.y);
        return rect;
    }

    public void SetAiDifficulty(int difficulty)
    {
        AiPlayer.Difficulty = (AiDifficulty) difficulty;
    }
}
