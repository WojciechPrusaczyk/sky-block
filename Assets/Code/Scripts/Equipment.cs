using System;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public int selectedSlot;
    public List<Item> slots;
    public Item selectedItem;

    private void Start()
    {
        selectedSlot = 0;
        selectedItem = slots != null && slots.Count > 0 ? slots[0] : null;

        MainUserInfaceController.Instance?.SelectItem(0);
        MainUserInfaceController.Instance?.UpdateItemSlots();
    }

    public void SelectItemAtSlot(int slot)
    {
        selectedSlot = Mathf.Clamp(slot, 0, slots.Count - 1);

        selectedItem = (slots != null && selectedSlot < slots.Count) ? slots[selectedSlot] : null;

        MainUserInfaceController.Instance?.SelectItem(selectedSlot);
        MainUserInfaceController.Instance?.UpdateItemSlots();
    }

    public Item GetItemAtSelectedSlot()
    {
        return slots[selectedSlot];
    }
}
