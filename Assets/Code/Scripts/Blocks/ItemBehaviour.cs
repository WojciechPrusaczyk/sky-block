using System;
using UnityEngine;

public class ItemBehaviour : MonoBehaviour
{
    public Item itemData;
    public bool animateFloating = true;
    public float maxBlockHeight = 0.15f;
    public float floatSpeed = 1f;

    private GameObject appearance;
    private Vector3 baseLocalPosition;

    private void Awake()
    {
        appearance = gameObject.transform.Find("Appearance").gameObject;

        if (appearance)
            baseLocalPosition = appearance.transform.localPosition;

        if(appearance && itemData)
            appearance.gameObject.GetComponent<SpriteRenderer>().sprite = itemData.WorldIcon;
    }

    private void Update()
    {
        if (animateFloating && appearance)
        {
            float t = (Mathf.Sin(Time.time * floatSpeed + Mathf.PI * 0.5f) + 1f) * 0.5f;

            float offsetY = t * maxBlockHeight;
            appearance.transform.localPosition = new Vector3(
                baseLocalPosition.x,
                baseLocalPosition.y + offsetY,
                baseLocalPosition.z
            );
        }
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
