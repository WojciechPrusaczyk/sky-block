using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BlockBehaviour : MonoBehaviour
{
    [SerializeField] protected float fallbackHealth = 4f;
    [SerializeField] protected float wrongToolDamageMultiplier = 0.5f;

    protected float currentHealth;
    protected float maxHealth;
    protected Item blockItemData;
    protected BlocksManager parentBlocksManager;
    protected Tilemap parentTilemap;
    private readonly Dictionary<int, float> hitDropDamageBuffers = new();

    protected virtual void Awake()
    {
        BlockItemData blockData = GetComponent<BlockItemData>();
        if (blockData)
        {
            blockItemData = blockData.itemData;
        }

        maxHealth = blockItemData ? Mathf.Max(1f, blockItemData.blockHealth) : Mathf.Max(1f, fallbackHealth);
        currentHealth = maxHealth;

        parentBlocksManager = GetComponentInParent<BlocksManager>();
        if (parentBlocksManager)
        {
            parentTilemap = parentBlocksManager.GetComponent<Tilemap>();
        }
    }

    public virtual void OnPlaced(Item placedByItem) {}

    public virtual bool OnUse(Item usedByItem)
    {
        return false;
    }

    public virtual bool OnAltUse(Item usedByItem)
    {
        return false;
    }

    public virtual void Hit(Item hitByItem)
    {
        if (currentHealth <= 0f)
        {
            return;
        }

        float damage = ResolveHitDamage(hitByItem);
        damage = OnHit(hitByItem, damage);

        damage = Mathf.Max(0f, damage);
        if (damage <= 0f)
        {
            return;
        }

        float dealtDamage = Mathf.Min(damage, currentHealth);
        ProcessHitDrops(dealtDamage);

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        Debug.Log($"{GetHitLogName()} HP: {currentHealth:0.##}/{maxHealth:0.##}");

        if (currentHealth <= 0f)
        {
            DestroyBlock();
        }
    }

    protected virtual float OnHit(Item hitByItem, float damage)
    {
        return damage;
    }

    protected virtual float ResolveHitDamage(Item hitByItem)
    {
        Enums.ToolType requiredTool = blockItemData ? blockItemData.preferredTool : Enums.ToolType.None;
        Enums.ToolType usedTool = hitByItem ? hitByItem.toolType : Enums.ToolType.None;
        float baseDamage = hitByItem ? Mathf.Max(0f, hitByItem.hitDamage) : 1f;

        bool isCorrectTool = requiredTool == Enums.ToolType.None || usedTool == requiredTool;
        float damage = isCorrectTool ? baseDamage : baseDamage * wrongToolDamageMultiplier;

        if (damage <= 0f)
        {
            damage = isCorrectTool ? 1f : Mathf.Max(0.1f, wrongToolDamageMultiplier);
        }

        return damage;
    }

    protected virtual string GetHitLogName()
    {
        return gameObject.name;
    }

    private void ProcessHitDrops(float dealtDamage)
    {
        if (blockItemData == null || blockItemData.blockDrop == null || blockItemData.blockDrop.Count == 0)
        {
            return;
        }

        for (int i = 0; i < blockItemData.blockDrop.Count; i++)
        {
            Item.BlockDrop drop = blockItemData.blockDrop[i];
            if (drop == null || drop.itemToDrop == null || drop.hpPerDrop <= 0f)
            {
                continue;
            }

            hitDropDamageBuffers.TryGetValue(i, out float bufferedDamage);
            bufferedDamage += dealtDamage;

            int attempts = Mathf.FloorToInt(bufferedDamage / drop.hpPerDrop);
            bufferedDamage -= attempts * drop.hpPerDrop;

            hitDropDamageBuffers[i] = bufferedDamage;

            for (int attempt = 0; attempt < attempts; attempt++)
            {
                SpawnDropAttempt(drop);
            }
        }
    }

    private void SpawnDropAttempt(Item.BlockDrop drop)
    {
        if (drop == null || drop.itemToDrop == null)
        {
            return;
        }

        if (drop.chanceToDrop <= 0f)
        {
            return;
        }

        if (drop.chanceToDrop < 100f && Random.Range(0f, 100f) >= drop.chanceToDrop)
        {
            return;
        }

        int dropCount = ResolveDropCount(drop);
        for (int i = 0; i < dropCount; i++)
        {
            SpawnDropItem(drop.itemToDrop);
        }
    }

    private int ResolveDropCount(Item.BlockDrop drop)
    {
        float min = Mathf.Max(0f, drop.minItemDrop);
        float max = Mathf.Max(min, drop.maxItemDrop);
        return Mathf.RoundToInt(Random.Range(min, max));
    }

    private void SpawnDropItem(GameObject itemToDrop)
    {
        if (itemToDrop == null)
        {
            return;
        }

        if (parentBlocksManager && parentTilemap)
        {
            Vector3Int cell = parentTilemap.WorldToCell(transform.position);
            parentBlocksManager.SpawnDrop(itemToDrop, cell);
            return;
        }

        Instantiate(itemToDrop, transform.position, Quaternion.identity);
    }

    protected virtual void DestroyBlock()
    {
        OnDestroyed();

        if (parentBlocksManager && parentTilemap)
        {
            Vector3Int cell = parentTilemap.WorldToCell(transform.position);
            parentBlocksManager.DestroyBlock(cell);
            return;
        }

        Destroy(gameObject);
    }

    protected virtual void OnDestroyed() {}
}