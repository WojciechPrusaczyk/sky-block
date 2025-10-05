using System;
using UnityEngine;

public class TreeController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private GameObject player;
    private int playerSortingLayer;

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
        if (player.transform.position.y > transform.position.y)
            spriteRenderer.sortingOrder = playerSortingLayer + 1;
        else
            spriteRenderer.sortingOrder = playerSortingLayer - 1;
    }
}
