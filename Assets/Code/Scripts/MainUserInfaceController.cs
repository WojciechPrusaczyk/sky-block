using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MainUserInfaceController : MonoBehaviour
{
    public Sprite itemBackground;
    public Sprite selectedItemBackground;
    public static MainUserInfaceController Instance { get; private set; }

    [SerializeField] private VisualElement root;
    private List<VisualElement> slots = new List<VisualElement>(8);
    private List<VisualElement> slotsBackgrounds = new List<VisualElement>(8);
    private List<VisualElement> slotsImages = new List<VisualElement>(8);
    private Equipment equipment;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        equipment = GameObject.Find("Player").GetComponent<Equipment>();

        if (equipment == null)
        {
            Debug.LogError("Not found Player equipment component.");
        }
    }

    private void OnEnable()
    {

        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("ERROR! NIE MA UIDOCUMENT");
        }

        root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("ERROR! NIE MA ROOT VISUALELEMENT");
        }

        for (int i = 0; i <= 7; i++)
        {
            VisualElement item = root.Q<VisualElement>($"Item{i}");
            VisualElement itemBackground = root.Q<VisualElement>($"ItemBackground{i}");
            VisualElement itemImage = root.Q<VisualElement>($"ItemImage{i}");

            if ( null != item )
                slots.Add(item);

            if ( null != itemBackground)
                slotsBackgrounds.Add(itemBackground);

            if ( null != itemImage )
                slotsImages.Add(itemImage);
        }

        UpdateItemSlots();
    }

    public void SelectItem(int slot)
    {
        for (int i = 0; i <= 7; i++)
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
        if (slotsImages == null || equipment == null) return;

        int uiCount = slotsImages.Count;
        int itemsCount = equipment.slots != null ? equipment.slots.Count : 0;

        for (int i = 0; i < uiCount; i++)
        {
            var ve = slotsImages[i];
            if (ve == null) continue;

            Item item = (i < itemsCount) ? equipment.slots[i] : null;
            Sprite icon = item != null ? item.Icon : null;

            ve.style.backgroundImage = icon != null
                ? new StyleBackground(icon)
                : null;
        }
    }
}
