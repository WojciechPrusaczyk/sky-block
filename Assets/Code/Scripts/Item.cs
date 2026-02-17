using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "SkyBlock/Item", order = 1)]
public class Item : ScriptableObject
{
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

    [Tooltip("Item GameObject placed on the world when out of inventory.")]
    public GameObject ItemGameObject;

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
