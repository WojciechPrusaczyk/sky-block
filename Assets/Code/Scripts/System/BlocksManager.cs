using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BlocksManager : MonoBehaviour
{
    private readonly Dictionary<Vector3Int, GameObject> placedBlocks = new();

    private Tilemap tilemap;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        if (!tilemap)
        {
            Debug.LogError("BlocksManager requires a Tilemap on the same GameObject.");
            return;
        }

        foreach (Transform child in transform)
        {
            Vector3Int cell = tilemap.WorldToCell(child.position);
            if (!placedBlocks.ContainsKey(cell))
            {
                placedBlocks.Add(cell, child.gameObject);
            }
        }
    }

    /// <summary>
    /// Places a block prefab at the given cell if it's empty.
    /// </summary>
    public void PlaceBlock(Vector3Int cell, GameObject prefab, Item placedByItem = null)
    {
        if (placedBlocks.ContainsKey(cell))
        {
            Debug.Log($"Cell {cell} already occupied.");
            return;
        }

        if (prefab == null)
        {
            Debug.LogWarning("Prefab is null, cannot place block.");
            return;
        }

        Vector3 worldPos = tilemap.GetCellCenterWorld(cell);
        worldPos.z = transform.position.z;
        GameObject go = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        placedBlocks[cell] = go;

        BlockBehaviour blockBehaviour = go.GetComponent<BlockBehaviour>();
        if (blockBehaviour)
        {
            blockBehaviour.OnPlaced(placedByItem);
        }

        MainUserInfaceController.Instance?.equipment.OnBlockPlace();
    }

    /// <summary>
    /// Deletes a block at the given cell if one exists.
    /// </summary>
    public void DestroyBlock(Vector3Int cell)
    {
        if (placedBlocks.TryGetValue(cell, out GameObject block) && block != null)
        {
            BlockItemData blockItemData = block.GetComponent<BlockItemData>();
            if (!blockItemData || !blockItemData.itemData)
            {
                Debug.LogError($"Missing BlockItemData or itemData on block '{block.name}' at {cell}.");
                Destroy(block);
                placedBlocks.Remove(cell);
                return;
            }

            Item itemData = blockItemData.itemData;
            SpawnDropsFromList(itemData.blockDestroyedDrop, cell, block.name);

            Destroy(block);
            placedBlocks.Remove(cell);
        }
        else
        {
            Debug.Log($"No block to delete at {cell}");
        }
    }

    /// <summary>
    /// Returns block placed at provided cell without extra logs.
    /// </summary>
    public bool TryGetBlock(Vector3Int cell, out GameObject block)
    {
        if (placedBlocks.TryGetValue(cell, out GameObject foundBlock) && foundBlock != null)
        {
            block = foundBlock;
            return true;
        }

        block = null;
        return false;
    }

    /// <summary>
    /// Returns block placed at provided cell.
    /// </summary>
    public GameObject GetBlock(Vector3Int cell)
    {
        if (TryGetBlock(cell, out GameObject block))
        {
            return block;
        }

        Debug.Log($"No block at {cell}");
        return null;
    }

    public void SpawnDrop(GameObject itemPrefab, Vector3Int cell)
    {
        if (itemPrefab == null)
        {
            return;
        }

        Vector3 target = tilemap.GetCellCenterWorld(cell);
        float xTranslation = Random.Range(-.35f, .35f);
        float yTranslation = Random.Range(-.35f, .35f);
        Vector3 newPos = new Vector3(target.x + xTranslation, target.y + yTranslation, target.z);
        Instantiate(itemPrefab, newPos, Quaternion.identity, transform);
    }

    private void SpawnDropsFromList(List<Item.BlockDrop> drops, Vector3Int cell, string blockName)
    {
        if (drops == null || drops.Count == 0)
        {
            return;
        }

        foreach (Item.BlockDrop drop in drops)
        {
            if (drop == null)
            {
                continue;
            }

            if (drop.itemToDrop == null)
            {
                Debug.LogWarning($"Missing itemToDrop in destroyed drop list for block '{blockName}' at {cell}.");
                continue;
            }

            if (drop.chanceToDrop <= 0f)
            {
                continue;
            }

            if (drop.chanceToDrop < 100f && Random.Range(0f, 100f) >= drop.chanceToDrop)
            {
                continue;
            }

            float min = Mathf.Max(0f, drop.minItemDrop);
            float max = Mathf.Max(min, drop.maxItemDrop);
            int dropCount = Mathf.RoundToInt(Random.Range(min, max));

            for (int i = 0; i < dropCount; i++)
            {
                SpawnDrop(drop.itemToDrop, cell);
            }
        }
    }
}