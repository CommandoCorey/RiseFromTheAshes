using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashSelection : MonoBehaviour
{
    [SerializeField] float fadeSpeed = 8;
    [SerializeField] int iterations = 3;

    private GameObject selectionHighlight;
    private int curIteration = 0;

    bool fadeIn = true;
    bool fadeOut = false;

    Color renderColor;
    float alpha = 0;

    SpriteRenderer sprite;

    // Start is called before the first frame update
    void Start()
    {
        this.enabled = false;

        selectionHighlight = GetComponent<UnitController>().selectionHighlight;
        selectionHighlight.SetActive(true);

        sprite = selectionHighlight.GetComponentInChildren<SpriteRenderer>();

        renderColor.r = 1;
        renderColor.a = alpha;
        sprite.color = renderColor;
    }

    // Update is called once per frame
    void Update()
    {
        if(fadeIn)
        {
            alpha += fadeSpeed * Time.deltaTime;
            renderColor.a = alpha;
            sprite.color = renderColor;

            if (alpha >= 1)
            {
                fadeIn = false;
                fadeOut = true;
            }

        }
        
        if(fadeOut)
        {
            alpha -= fadeSpeed * Time.deltaTime;
            renderColor.a = alpha;
            sprite.color = renderColor;

            if (alpha <= 0)
            {                
                fadeOut = false;
                fadeIn = true;

                curIteration++;
            }
        }

        if(curIteration == iterations)
        {
            this.enabled = false;
            curIteration = 0;
        }
    }

    public void Begin()
    {
        renderColor = selectionHighlight.GetComponent<SpriteRenderer>().color;
        renderColor.a = alpha;
    }

}
