using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "SkyBlock/Item", order = 1)]
public class Item : ScriptableObject
{
    [Tooltip("Item name.")]
    public string Name;

    [Tooltip("Item type.")]
    public Enums.ItemType Type;

    [Tooltip("Max items in stack.")]
    public int maxItems;

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

        /// <summary>Method called when item is removed from inventory.</summary>
        void Remove();
    }
}
