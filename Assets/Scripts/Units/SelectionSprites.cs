using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

public class SelectionSprites : MonoBehaviour
{
    #region variables
    [Tooltip("The highlight that appears while the unit or building is selected")]
    [SerializeField] SpriteRenderer selectedSprite;

    [Header("Rotating Highlight")]
    [Tooltip("The highlight that appears when object it targeted by a unit")]
    [SerializeField] SpriteRenderer targetedSprite;
    [SerializeField] bool rotateSprite = true;
    [SerializeField][Range(10, 200)]
    float rotationSpeed = 100;

    private float highlightAngle = 0;

    [Header("Flashing Highlight (A.I. Only)")]
    [Tooltip("The highlight that when the player click on an enemy to send the attack command")]
    [SerializeField] SpriteRenderer attackedSprite;
    [SerializeField]
    [Range(1, 20)]
    float fadeSpeed = 8;
    [SerializeField]
    [Range(1, 10)]
    int iterations = 3;

    private int curIteration = 0;
    private bool fadeIn = true;
    private bool fadeOut = false;
    private Color renderColor;
    private float alpha = 0;    
    #endregion

    // properties
    public bool ShowTargetedSprite { get; set; } = false;
    public bool ShowAttackedSprite { get; set; } = false;

    public void SetSelectedSprite(bool active)
    {
        selectedSprite.gameObject.SetActive(active);
        renderColor.a = 1;
        selectedSprite.color = renderColor;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.layer == 7 || gameObject.layer == 9)
        {
            // set the opacity of the attacked sprite to zero
            renderColor = Color.red;
            renderColor.a = alpha;
            attackedSprite.color = renderColor;
        }
        else
        {
            renderColor = Color.blue;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ShowAttackedSprite)
        {
            FlashSprite();
        }
        else if (ShowTargetedSprite)
        {
            if (!targetedSprite.gameObject.activeInHierarchy)
                targetedSprite.gameObject.SetActive(true);

            if (rotateSprite)
                RotateSprite();
        }
        else if (targetedSprite != null && targetedSprite.gameObject.activeInHierarchy)
        {
            targetedSprite.gameObject.SetActive(false);
        }
        
    }

    private void RotateSprite()
    {       
        highlightAngle += rotationSpeed * Time.deltaTime;

        if (highlightAngle >= 360)
            highlightAngle = 0;

        targetedSprite.transform.rotation = Quaternion.Euler(90, highlightAngle, 0);
    }

    private void FlashSprite()
    {
        if (!attackedSprite.gameObject.activeInHierarchy)
            attackedSprite.gameObject.SetActive(true);

        if (fadeIn)
        {
            alpha += fadeSpeed * Time.deltaTime;
            renderColor.a = alpha;
            attackedSprite.color = renderColor;

            if (alpha >= 1)
            {
                fadeIn = false;
                fadeOut = true;
            }

        }

        if (fadeOut)
        {
            alpha -= fadeSpeed * Time.deltaTime;
            renderColor.a = alpha;
            attackedSprite.color = renderColor;

            if (alpha <= 0)
            {
                fadeOut = false;
                fadeIn = true;

                curIteration++;
            }
        }

        if (curIteration == iterations)
        {
            curIteration = 0;
            attackedSprite.gameObject.SetActive(false);

            //ShowTargetedSprite = true;
            ShowAttackedSprite = false;
        }
    }
}
