using System.Collections.Generic;
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
    private UIDocument uiDocument;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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
        uiDocument = GetComponent<UIDocument>();
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

    private void OnDisable()
    {
        root = null;
        slots = null;
        slotsBackgrounds = null;
        slotsImages = null;
        slotsQty = null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SelectItem(int slot)
    {
        if (!IsUiReady())
        {
            return;
        }

        if (slot < 0 || slot >= equipment.hotbarMaxItems || slot >= slots.Count)
        {
            return;
        }

        for (int i = 0; i < equipment.hotbarMaxItems; i++)
        {
            if (i >= slots.Count)
            {
                break;
            }

            VisualElement item = slots[i];
            if (item == null || item.panel == null)
            {
                continue;
            }

            item.RemoveFromClassList("active");
        }

        VisualElement selectedItem = slots[slot];
        if (selectedItem == null || selectedItem.panel == null)
        {
            return;
        }

        selectedItem.AddToClassList("active");

        UpdateItemSlots();
    }

    public void UpdateItemSlots()
    {
        if (!IsUiReady()) return;

        int uiCount = equipment.hotbarMaxItems;
        int itemsCount = equipment.slots != null ? equipment.slots.Count : 0;

        for (int i = 0; i < uiCount; i++)
        {
            if (i >= slotsImages.Count || i >= slotsQty.Count)
            {
                break;
            }

            var slotImage = slotsImages[i];
            var slotQty = slotsQty[i];
            if (slotImage == null || slotQty == null || slotImage.panel == null || slotQty.panel == null) continue;

            Item item = (i < itemsCount) ? equipment.slots[i].item : null;
            int qty = (i < itemsCount) ? equipment.slots[i].amount : 0;
            Sprite icon = item != null ? item.Icon : null;

            slotImage.style.backgroundImage = icon != null
                ? new StyleBackground(icon)
                : null;

            if (qty > 1)
                slotQty.text = qty.ToString();
            else
                slotQty.text = "";
        }
    }

    private bool IsUiReady()
    {
        if (equipment == null || uiDocument == null)
        {
            return false;
        }

        if (root == null || root.panel == null)
        {
            return false;
        }

        if (slots == null || slotsImages == null || slotsQty == null)
        {
            return false;
        }

        return true;
    }
}
