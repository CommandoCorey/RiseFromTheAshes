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

        if (gameObject.layer == 7) // AI Unit
            selectionHighlight = GetComponent<UnitController>().selectionHighlight;
        else if(gameObject.layer == 9) // Ai Building
            selectionHighlight = GetComponent<Building>().selectionHighlight;

        selectionHighlight.SetActive(true);

        sprite = selectionHighlight.GetComponentInChildren<SpriteRenderer>();

        renderColor.r = 1;
        renderColor.a = alpha;

        if(sprite)
            sprite.color = renderColor;
    }

    // Update is called once per frame
    void Update()
    {
        if (sprite == null)
            return;

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
            curIteration = 0;
            selectionHighlight.SetActive(false);
            this.enabled = false;            
        }
    }

}
