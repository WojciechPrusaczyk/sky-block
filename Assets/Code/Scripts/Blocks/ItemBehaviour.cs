using System;
using UnityEngine;

public class ItemBehaviour : MonoBehaviour
{
    public Item itemData;

    private void Awake()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer && itemData)
            spriteRenderer.sprite = itemData.WorldIcon;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            var playerGameObject = collision.gameObject;
            var playerEq = playerGameObject.GetComponent<Equipment>();

            if (playerEq.AddItem(itemData))
                Destroy(gameObject);
        }
    }
}
