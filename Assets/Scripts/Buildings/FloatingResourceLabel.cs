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
    [Range(0, 10)]
    [SerializeField] float activeTime = 1;
    [Range(0, 1)]
    [SerializeField] float fadeInSpeed = 1;

    private RectTransform rect;
    private Vector2 startingPos;
    private bool moving = false;
    private Color transparentWhite;
    private Color transparentGreen;

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
    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.activeInHierarchy && moving)
        {            
            rect.anchoredPosition += new Vector2(0, raiseSpeed) * Time.deltaTime;
            icon.color += new Color(0, 0, 0, fadeInSpeed) * Time.deltaTime;
            plusSign.color += new Color(0, 0, 0, fadeInSpeed) * Time.deltaTime;
            amountLabel.color += new Color(0, 0, 0, fadeInSpeed) * Time.deltaTime;
        }

    }

    public void Begin(int amount)
    {
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
