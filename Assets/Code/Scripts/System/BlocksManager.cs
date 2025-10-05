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
    public void PlaceBlock(Vector3Int cell, GameObject prefab)
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
        MainUserInfaceController.Instance?.equipment.OnBlockPlace();
    }

    /// <summary>
    /// Deletes a block at the given cell if one exists.
    /// </summary>
    public void DestroyBlock(Vector3Int cell)
    {
        if (placedBlocks.TryGetValue(cell, out GameObject block) && block != null)
        {
            var itemData = block.GetComponent<BlockItemData>().itemData;
            Instantiate(itemData.ItemGameObject, tilemap.GetCellCenterWorld(cell), Quaternion.identity, transform);
            Destroy(block);
            placedBlocks.Remove(cell);
        }
        else
        {
            Debug.Log($"No block to delete at {cell}");
        }
    }
}