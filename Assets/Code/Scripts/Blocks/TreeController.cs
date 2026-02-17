using System;
using UnityEngine;

public class TreeController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private GameObject player;
    private int playerSortingLayer;
    [SerializeField] private float fadeDuration = 0.12f;
    private Color fullColor = new Color(1, 1, 1, 1);
    private Color transparentColor = new Color(1, 1, 1, .8f);

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player");

        if( player)
        {
            var playerSpriteRenderer = player.GetComponentInChildren<SpriteRenderer>();
            playerSortingLayer = playerSpriteRenderer.sortingOrder;
        }
    }

    private void Update()
    {
        if (!player)
        {
            return;
        }

        Vector3 playerPos = player.transform.position;
        Vector3 treePos = transform.position;

        float dx = Mathf.Abs(playerPos.x - treePos.x);
        float dy = playerPos.y - treePos.y;

        Color targetColor;

        // Player must be at most 1 tile away on X and up to 2 tiles above the tree.
        if (dy > 0f)
        {
            spriteRenderer.sortingOrder = playerSortingLayer + 1;

            if (dx <= 1f  && dy <= 3f)
            {
                targetColor = transparentColor;
            }
            else
            {
                targetColor = fullColor;
            }
        }
        else
        {
            spriteRenderer.sortingOrder = playerSortingLayer - 1;
            targetColor = fullColor;
        }

        Color currentColor = spriteRenderer.color;
        float maxAlphaDelta;

        if (fadeDuration <= 0f)
        {
            maxAlphaDelta = 1f;
        }
        else
        {
            float alphaRange = Mathf.Abs(fullColor.a - transparentColor.a);
            maxAlphaDelta = alphaRange * (Time.deltaTime / fadeDuration);
        }

        currentColor.a = Mathf.MoveTowards(currentColor.a, targetColor.a, maxAlphaDelta);
        spriteRenderer.color = currentColor;
    }
}