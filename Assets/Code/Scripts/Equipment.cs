using System;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    [Serializable]
    public class EquipmentSlot
    {
        public Item item;
        public int amount;
    }

    public int selectedSlot;
    public List<EquipmentSlot> slots = new List<EquipmentSlot>();

    public Item selectedItem;

    // Maksymalna liczba slotów
    public int hotbarMaxItems = 8;
    public int inventoryMaxItems = 32;

    private void Start()
    {
        selectedSlot = 0;

        if (slots.Count > 0)
            selectedItem = slots[selectedSlot].item;
        else
            selectedItem = null;

        MainUserInfaceController.Instance?.SelectItem(selectedSlot);
        MainUserInfaceController.Instance?.UpdateItemSlots();
    }

    public void SelectItemAtSlot(int slot)
    {
        selectedSlot = Mathf.Clamp(slot, 0, hotbarMaxItems - 1);

        if (slots != null && selectedSlot < slots.Count)
            selectedItem = slots[selectedSlot].item;
        else
            selectedItem = null;

        MainUserInfaceController.Instance?.SelectItem(selectedSlot);
        MainUserInfaceController.Instance?.UpdateItemSlots();
    }

    public Item GetItemAtSelectedSlot()
    {
        if (slots.Count == 0 || selectedSlot < 0 || selectedSlot >= slots.Count) return null;
        return slots[selectedSlot].item;
    }

    public int GetItemAmountAtSelectedSlot()
    {
        if (slots.Count == 0 || selectedSlot < 0 || selectedSlot >= slots.Count) return 0;
        return slots[selectedSlot].amount;
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

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot.item != item)
                continue;

            if (slot.amount < item.maxItems)
            {
                slot.amount = Mathf.Min(slot.amount + 1, item.maxItems);
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

        slots.Add(new EquipmentSlot
        {
            item = item,
            amount = 1
        });
        MainUserInfaceController.Instance?.UpdateItemSlots();
        return true;
    }

    public bool OnBlockPlace()
    {
        if (slots == null || slots.Count == 0)
            return false;

        if (selectedSlot < 0 || selectedSlot >= slots.Count)
            return false;

        var slot = slots[selectedSlot];
        Item item = slot.item;
        int count = slot.amount;

        if (item == null || count <= 0)
            return false;

        count -= 1;

        if (count > 0)
        {
            slot.amount = count;
        }
        else
        {
            slots.RemoveAt(selectedSlot);

            if (selectedSlot >= slots.Count)
                selectedItem = null;
            else
                selectedItem = slots[selectedSlot].item;
        }

        MainUserInfaceController.Instance?.UpdateItemSlots();

        return true;
    }
}
