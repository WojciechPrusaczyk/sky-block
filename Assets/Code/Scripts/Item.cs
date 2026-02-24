using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "SkyBlock/Item", order = 1)]
public class Item : ScriptableObject
{
    /*
     * Zmienna listowa do określania dropu z bloków
     */
    [Serializable]
    public class BlockDrop
    {
        /// <summary>Item to drop.</summary>
        public GameObject itemToDrop;

        /// <summary>HP needed to be dealt to initiate drop.</summary>
        public float hpPerDrop;

        /// <summary>Minimal amount to drop per hp dealt.</summary>
        public float minItemDrop;

        /// <summary>Maximal amount to drop per hp dealt.</summary>
        public float maxItemDrop;

        /// <summary>Chance to drop per </summary>
        [Range(0.0f, 100f)]
        public float chanceToDrop;
    }


    [Tooltip("Item name.")]
    public string Name;

    [Tooltip("Item type.")]
    public Enums.ItemType Type;

    [Tooltip("Tool category of this item if item type is Tool.")]
    public Enums.ToolType toolType = Enums.ToolType.None;

    [Tooltip("Max items in stack.")]
    public int maxItems;

    [Tooltip("Base damage dealt when using this item to hit a block.")]
    public float hitDamage = 1f;

    [Tooltip("Health of this block when placed in the world.")]
    public float blockHealth = 4f;

    [Tooltip("Preferred tool type to hit this block efficiently.")]
    public Enums.ToolType preferredTool = Enums.ToolType.None;

    [Tooltip("Item inventory icon.")]
    public Sprite Icon;

    [Tooltip("Icon displayed when placed in the world enviorment.")]
    public Sprite WorldIcon;

    [Tooltip("GameObject placed in the blocks tileset.")]
    public GameObject BlockGameObject;

    [Tooltip("Items placed on the world when block is hit.")]
    public List<BlockDrop> blockDrop = new List<BlockDrop>();

    [Tooltip("Items placed on the world when block is destroyed.")]
    public List<BlockDrop> blockDestroyedDrop = new List<BlockDrop>();

    public virtual void Initialize() {}

    public interface ItemBehaviour
    {
        /// <summary>Method called when LMB is clicked.</summary>
        void Use();

        /// <summary>Method called when RMB is clicked.</summary>
        void AltUse();

        /// <summary>Method called when block is placed in the world.</summary>
        void OnPlace();

        /// <summary>Method called when block is destroyed.</summary>
        void OnDestroy();

        /// <summary>Method called when item (as block data) is hit.</summary>
        void OnHit();

        /// <summary>Method called when item is removed from inventory.</summary>
        void Remove();
    }
}