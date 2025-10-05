using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public int selectedSlot;
    public Dictionary<Item, int> slots = new Dictionary<Item, int>();

    public Item selectedItem;

    // Maksymalna liczba slotów
    public int hotbarMaxItems = 8;
    public int inventoryMaxItems = 32;

    private void Start()
    {
        selectedSlot = 0;

        if (slots.Count > 0)
            selectedItem = slots.ElementAt(selectedSlot).Key;
        else
            selectedItem = null;

        MainUserInfaceController.Instance?.SelectItem(selectedSlot);
        MainUserInfaceController.Instance?.UpdateItemSlots();
    }

    public void SelectItemAtSlot(int slot)
    {
        selectedSlot = Mathf.Clamp(slot, 0, hotbarMaxItems - 1);

        if (slots != null && selectedSlot < slots.Count)
            selectedItem = slots.ElementAt(selectedSlot).Key;
        else
            selectedItem = null;

        MainUserInfaceController.Instance?.SelectItem(selectedSlot);
        MainUserInfaceController.Instance?.UpdateItemSlots();
    }

    public Item GetItemAtSelectedSlot()
    {
        if (slots.Count == 0 || slots.Count < selectedSlot) return null;
        return slots.ElementAt(selectedSlot).Key;
    }

    public int GetItemAmountAtSelectedSlot()
    {
        if (slots.Count == 0) return 0;
        return slots.ElementAt(selectedSlot).Value;
    }

    /// <summary>
    /// Adding item to equipment.
    /// Returns true/false depending on adding success.
    /// </summary>
    public bool AddItem(Item item)
    {
        if (item == null)
        {
            Debug.LogWarning("AddItem: item == null");
            return false;
        }

        if (slots.ContainsKey(item))
        {
            int currentAmount = slots[item];

            if (currentAmount < item.maxItems)
            {
                slots[item] = Mathf.Min(currentAmount + 1, item.maxItems);
                MainUserInfaceController.Instance?.UpdateItemSlots();
                return true;
            }
        }

        int maxSlots = hotbarMaxItems + inventoryMaxItems;
        if (slots.Count >= maxSlots)
        {
            Debug.Log("Inventory full, cannot add item: " + item.Name);
            return false;
        }

        slots.Add(item, 1);
        MainUserInfaceController.Instance?.UpdateItemSlots();
        return true;
    }

    public bool OnBlockPlace()
    {
        if (slots == null || slots.Count == 0)
            return false;

        if (selectedSlot < 0 || selectedSlot >= slots.Count)
            return false;

        var pair = slots.ElementAt(selectedSlot);
        Item item = pair.Key;
        int count = pair.Value;

        if (item == null || count <= 0)
            return false;

        count -= 1;

        if (count > 0)
        {
            slots[item] = count;
        }
        else
        {
            slots.Remove(item);
            selectedItem = null;
        }

        MainUserInfaceController.Instance?.UpdateItemSlots();

        return true;
    }
}