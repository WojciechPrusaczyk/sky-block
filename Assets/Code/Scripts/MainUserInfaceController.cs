using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MainUserInfaceController : MonoBehaviour
{
    public Sprite itemBackground;
    public Sprite selectedItemBackground;
    public static MainUserInfaceController Instance { get; private set; }

    [SerializeField] private VisualElement root;
    private List<VisualElement> slots;
    private List<VisualElement> slotsBackgrounds;
    private List<VisualElement> slotsImages;
    private List<Label> slotsQty;
    public Equipment equipment;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        var player = GameObject.Find("Player");
        if (!player)
        {
            Debug.LogError("Not found Player object.");
            return;
        }

        equipment = player.GetComponent<Equipment>();
        if (equipment == null)
            Debug.LogError("Not found Player equipment component.");
    }

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("ERROR! Brak UIDocument");
            return;
        }

        root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("ERROR! Brak root VisualElement");
            return;
        }

        slots = new List<VisualElement>(equipment != null ? equipment.hotbarMaxItems : 8);
        slotsBackgrounds = new List<VisualElement>(equipment != null ? equipment.hotbarMaxItems : 8);
        slotsImages = new List<VisualElement>(equipment != null ? equipment.hotbarMaxItems : 8);
        slotsQty = new List<Label>(equipment != null ? equipment.hotbarMaxItems : 8);

        int hotbarCount = equipment != null ? equipment.hotbarMaxItems : 8;

        for (int i = 0; i < hotbarCount; i++)
        {
            var item = root.Q<VisualElement>($"Item{i}");
            var itemBackground = root.Q<VisualElement>($"ItemBackground{i}");
            var itemImage = root.Q<VisualElement>($"ItemImage{i}");
            var itemQty = root.Q<Label>($"ItemLabel{i}");

            slots.Add(item);
            slotsBackgrounds.Add(itemBackground);
            slotsImages.Add(itemImage);
            slotsQty.Add(itemQty);
        }

        UpdateItemSlots();
    }

    public void SelectItem(int slot)
    {
        for (int i = 0; i < equipment.hotbarMaxItems; i++)
        {
            VisualElement item = slots[i];
            item.RemoveFromClassList("active");
        }

        VisualElement selectedItem = slots[slot];
        selectedItem.AddToClassList("active");

        UpdateItemSlots();
    }

    public void UpdateItemSlots()
    {
        if (slotsImages == null || equipment == null || slotsQty == null) return;

        int uiCount = equipment.hotbarMaxItems;
        int itemsCount = equipment.slots != null ? equipment.slots.Count : 0;

        var slotPairs = (equipment.slots != null)
            ? equipment.slots.ToList()
            : new List<KeyValuePair<Item, int>>();

        for (int i = 0; i < uiCount; i++)
        {
            var slotImage = slotsImages[i];
            var slotQty = slotsQty[i];
            if (slotImage == null) continue;

            Item item = (i < itemsCount) ? slotPairs[i].Key : null;
            int qty = (i < itemsCount) ? slotPairs[i].Value : 0;
            Sprite icon = item != null ? item.Icon : null;

            slotImage.style.backgroundImage = icon != null
                ? new StyleBackground(icon)
                : null;

            if (qty > 0)
                slotQty.text = qty.ToString();
            else
                slotQty.text = "";
        }
    }
}