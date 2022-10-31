using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingResourceLabel : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI plusSign;
    public TextMeshProUGUI amountLabel;

    [Range(0, 100)]
    [SerializeField] float raiseSpeed = 50;
    //[Range(0, 10)][SerializeField]
    float activeTime = 1;
    [Range(0, 1)]
    [SerializeField] float fadeInSpeed = 1;

    private RectTransform rect;
    private Vector2 startingPos;
    private bool moving = false;
    private Color transparentWhite;
    private Color transparentGreen;
    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        transparentWhite = new Color(255, 255, 255, 0);
        transparentGreen = new Color(0, 255, 0, 0);

        rect = GetComponent<RectTransform>();
        startingPos = rect.anchoredPosition;

        icon.color = transparentWhite;
        plusSign.color = transparentGreen;
        amountLabel.color = transparentGreen;

        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(gameObject.activeInHierarchy && moving)
        {            
            rect.anchoredPosition += new Vector2(0, raiseSpeed) * Time.fixedDeltaTime;
            icon.color += new Color(0, 0, 0, fadeInSpeed) * Time.fixedDeltaTime;
            plusSign.color += new Color(0, 0, 0, fadeInSpeed) * Time.fixedDeltaTime;
            amountLabel.color += new Color(0, 0, 0, fadeInSpeed) * Time.fixedDeltaTime;
        }

    }

    public void Begin(int amount, float timePerIncrement)
    {
        activeTime = timePerIncrement;
        amountLabel.text = amount.ToString();
        moving = true;
        Invoke("Reset", activeTime);
    }

    public void Reset()
    {
        rect.anchoredPosition = startingPos;

        icon.color = transparentWhite;
        plusSign.color = transparentGreen;
        amountLabel.color = transparentGreen;

        gameObject.SetActive(false);
    }
}
