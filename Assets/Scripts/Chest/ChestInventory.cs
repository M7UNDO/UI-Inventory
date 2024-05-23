using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestInventory : MonoBehaviour
{
    [SerializeField] private GameObject chestSlotHolder;
    private SlotClass[] chestItems;
    private GameObject[] chestSlots;

    private void Start()
    {
        chestSlots = new GameObject[chestSlotHolder.transform.childCount];
        chestItems = new SlotClass[chestSlots.Length];

        for (int i = 0; i < chestItems.Length; i++)
            chestItems[i] = new SlotClass();
        
        for (int i = 0; i < chestSlotHolder.transform.childCount; i++)
            chestSlots[i] = chestSlotHolder.transform.GetChild(i).gameObject;

        RefreshChestUI();
    }

    public void RefreshChestUI()
    {
        for (int i = 0; i < chestSlots.Length; i++)
            RefreshSlot(chestItems[i], chestSlots[i].transform);
    }

    private void RefreshSlot(SlotClass slot, Transform slotUI)
    {
        if (slot.item is null)
        {
            slotUI.transform.GetChild(0).GetComponent<Image>().sprite = null;
            slotUI.transform.GetChild(0).GetComponent<Image>().enabled = false;
            slotUI.transform.GetChild(1).GetComponent<Text>().text = "";
            return;
        }

        slotUI.transform.GetChild(0).GetComponent<Image>().enabled = true;
        slotUI.transform.GetChild(0).GetComponent<Image>().sprite = slot.item.itemIcon;

        if (slot.item.isStackable)
            slotUI.transform.GetChild(1).GetComponent<Text>().text = slot.quantity + "";
        else
            slotUI.transform.GetChild(1).GetComponent<Text>().text = "";
    }

    public SlotClass GetClosestChestSlot()
    {
        for (int i = 0; i < chestSlots.Length; i++)
        {
            if (Vector2.Distance(chestSlots[i].transform.position, Input.mousePosition) <= 32)
                return chestItems[i];
        }
        return null;
    }

    public bool AddToChest(ItemClass item, int quantity)
    {
        SlotClass slot = ContainsInChest(item);

        if (slot != null)
        {
            var quantityCanAdd = slot.item.stackSize - slot.quantity;
            var quantityToAdd = Mathf.Clamp(quantity, 0, quantityCanAdd);
            var remainder = quantity - quantityCanAdd;

            slot.AddQuantity(quantityToAdd);
            if (remainder > 0) AddToChest(item, remainder);
        }
        else
        {
            for (int i = 0; i < chestItems.Length; i++)
            {
                if (chestItems[i].item == null)
                {
                    var quantityCanAdd = item.stackSize - chestItems[i].quantity;
                    var quantityToAdd = Mathf.Clamp(quantity, 0, quantityCanAdd);
                    var remainder = quantity - quantityToAdd;

                    chestItems[i].AddItem(item, quantityToAdd);
                    if (remainder > 0) AddToChest(item, remainder);

                    break;
                }
            }
        }

        RefreshChestUI();
        return true;
    }

    public SlotClass ContainsInChest(ItemClass item)
    {
        for (int i = 0; i < chestItems.Length; i++)
        {
            if (chestItems[i].item == item && chestItems[i].item.isStackable && chestItems[i].quantity < chestItems[i].item.stackSize)
                return chestItems[i];
        }

        return null;
    }
}
